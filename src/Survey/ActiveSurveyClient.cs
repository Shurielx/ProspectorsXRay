using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProspectorsXRay.Network;
using ProspectorsXRay.OreDatabase.Services;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace ProspectorsXRay.Survey;

public enum DisplayModeType
{
    Block,
    Wireframe
}

public class ClientConfig
{
    public int DisplayOpacity { get; set; } = 60;
    public DisplayModeType DisplayMode { get; set; } = DisplayModeType.Block;
    public int DisplaySizePercent { get; set; } = 25;
    public List<string> DisabledOres { get; set; } = new();
}

public class ActiveSurveyData
{
    public BlockPos Center { get; init; } = new(0, 0, 0, 0);
    public int Radius { get; init; }
    public string ModeName { get; init; } = string.Empty;
    public DateTime ExpireTime { get; init; }
    public int TotalDetected { get; init; }
    public HashSet<BlockPos> RemainingOres { get; init; } = new();
    public Dictionary<BlockPos, int> OreColors { get; init; } = new();
    public Dictionary<BlockPos, string> OreMinerals { get; init; } = new();
    public List<(BlockPos Pos, int Color, string Mineral)> HiddenOres { get; init; } = new();
}

public class ActiveSurveyClient
{
    public static ActiveSurveyClient? Instance { get; private set; }
    public static bool IsTesselatingOurHighlight { get; set; } = false;

    private const string ConfigFileName = "ProspectorsXRayClient.json";
    private const int HighlightSlotId = 77123;
    private readonly ICoreClientAPI capi;
    private readonly long tickListenerId;

    public ActiveSurveyData? ActiveSurvey { get; private set; }
    public int DisplayOpacity { get; private set; } = 60;
    public DisplayModeType DisplayMode { get; private set; } = DisplayModeType.Block;
    public int DisplaySizePercent { get; private set; } = 25;
    public HashSet<string> DisabledOres { get; } = new(StringComparer.OrdinalIgnoreCase);

    private bool isHighlightActive = false;
    private bool manualHideOverride = false;
    private bool lastHoldsPickaxe = false;
    private bool lastInBounds = false;
    private bool needsHighlightRefresh = true;

    public ActiveSurveyClient(ICoreClientAPI capi)
    {
        this.capi = capi ?? throw new ArgumentNullException(nameof(capi));
        Instance = this;

        LoadConfig();

        // Register event hooks
        capi.Event.BlockChanged += OnBlockChanged;
        capi.Event.AfterActiveSlotChanged += OnActiveSlotChanged;
        capi.Event.OnSendChatMessage += OnSendChatMessage;
        tickListenerId = capi.Event.RegisterGameTickListener(OnClientTick, 250);

        // Register client chat commands
        RegisterCommands();
    }

    public void Dispose()
    {
        try
        {
            capi.Event.BlockChanged -= OnBlockChanged;
            capi.Event.AfterActiveSlotChanged -= OnActiveSlotChanged;
            capi.Event.OnSendChatMessage -= OnSendChatMessage;
            capi.Event.UnregisterGameTickListener(tickListenerId);
        }
        catch { }

        ClearHighlights();
        ActiveSurvey = null;
        if (Instance == this) Instance = null;
    }

    public void LoadConfig()
    {
        try
        {
            ClientConfig? cfg = capi.LoadModConfig<ClientConfig>(ConfigFileName);
            if (cfg != null)
            {
                DisplayOpacity = Math.Clamp(cfg.DisplayOpacity, 0, 100);
                DisplayMode = cfg.DisplayMode;
                DisplaySizePercent = Math.Clamp(cfg.DisplaySizePercent, 10, 100);
                DisabledOres.Clear();
                if (cfg.DisabledOres != null)
                {
                    foreach (var d in cfg.DisabledOres)
                    {
                        if (!string.IsNullOrWhiteSpace(d))
                        {
                            DisabledOres.Add(d.Trim());
                        }
                    }
                }
            }
            else
            {
                SaveConfig();
            }
        }
        catch
        {
            DisplayOpacity = 60;
            DisplayMode = DisplayModeType.Block;
            DisplaySizePercent = 25;
            DisabledOres.Clear();
        }
    }

    public void SaveConfig()
    {
        try
        {
            capi.StoreModConfig(new ClientConfig
            {
                DisplayOpacity = this.DisplayOpacity,
                DisplayMode = this.DisplayMode,
                DisplaySizePercent = this.DisplaySizePercent,
                DisabledOres = this.DisabledOres.ToList()
            }, ConfigFileName);
        }
        catch { }
    }

    public void SetDisplayOpacity(int opacityPercent)
    {
        DisplayOpacity = Math.Clamp(opacityPercent, 0, 100);
        SaveConfig();
        needsHighlightRefresh = true;
        UpdateHighlightState();
    }

    public void SetDisplayMode(DisplayModeType mode)
    {
        DisplayMode = mode;
        SaveConfig();
        needsHighlightRefresh = true;
        UpdateHighlightState();
    }

    public void SetDisplaySize(int sizePercent)
    {
        DisplaySizePercent = Math.Clamp(sizePercent, 10, 100);
        SaveConfig();
        needsHighlightRefresh = true;
        UpdateHighlightState();
    }

    private int ApplyOpacity(int baseColor)
    {
        int alpha = (int)Math.Clamp(Math.Round(DisplayOpacity * 2.55), 0, 255);
        int r = ColorUtil.ColorR(baseColor);
        int g = ColorUtil.ColorG(baseColor);
        int b = ColorUtil.ColorB(baseColor);
        return ColorUtil.ToRgba(alpha, r, g, b);
    }

    public void OnSurveyResultReceived(SurveyResultPacket packet)
    {
        ClearHighlights();
        manualHideOverride = false;
        needsHighlightRefresh = true;

        ActiveSurvey = new ActiveSurveyData
        {
            Center = new BlockPos(packet.CenterX, packet.CenterY, packet.CenterZ, 0),
            Radius = packet.Radius,
            ModeName = packet.ModeName,
            ExpireTime = DateTime.UtcNow.AddHours(1),
            TotalDetected = packet.TotalFoundCount
        };

        for (int i = 0; i < packet.RevealedX.Count; i++)
        {
            BlockPos pos = new BlockPos(packet.RevealedX[i], packet.RevealedY[i], packet.RevealedZ[i], 0);
            ActiveSurvey.RemainingOres.Add(pos);
            ActiveSurvey.OreColors[pos] = packet.RevealedColors[i];
            string mineral = (i < packet.RevealedMinerals.Count) ? packet.RevealedMinerals[i] : "";
            ActiveSurvey.OreMinerals[pos] = mineral;
        }

        if (packet.HiddenX != null)
        {
            for (int i = 0; i < packet.HiddenX.Count; i++)
            {
                BlockPos hPos = new BlockPos(packet.HiddenX[i], packet.HiddenY[i], packet.HiddenZ[i], 0);
                int hColor = (i < packet.HiddenColors.Count) ? packet.HiddenColors[i] : OreColorHelper.DefaultColor;
                string hMineral = (i < packet.HiddenMinerals.Count) ? packet.HiddenMinerals[i] : "";
                ActiveSurvey.HiddenOres.Add((hPos, hColor, hMineral));
            }
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"<font color=\"#55ff55\"><strong>[Prospector's X-Ray] Survey Complete ({packet.ModeName}):</strong></font>");

        if (packet.TotalFoundCount == 0)
        {
            sb.AppendLine("No ore deposits detected in this area.");
        }
        else
        {
            sb.AppendLine($"Total ores detected: <font color=\"#ffffff\">{packet.TotalFoundCount}</font> blocks.");

            foreach (var kvp in packet.TotalCounts.OrderByDescending(k => k.Value))
            {
                int shown = packet.ShownCounts.GetValueOrDefault(kvp.Key, 0);
                bool isOff = OreFilterManager.IsOreDisabled(kvp.Key, DisabledOres);
                if (isOff)
                {
                    sb.AppendLine($" • <font color=\"#ffffff\">{kvp.Key}</font>: {kvp.Value} blocks (<font color=\"#ff5555\">hidden</font>)");
                }
                else
                {
                    sb.AppendLine($" • <font color=\"#ffffff\">{kvp.Key}</font>: {kvp.Value} blocks (<font color=\"#ffdf55\">{shown} shown</font>)");
                }
            }

            int visibleCount = ActiveSurvey.RemainingOres.Count(p => !OreFilterManager.IsOreDisabled(ActiveSurvey.OreMinerals.GetValueOrDefault(p, ""), DisabledOres));
            sb.AppendLine($"<font color=\"#ffdf55\">Highlighted initial 50% ({visibleCount} blocks). Mining ore reveals +5% more!</font>");
            sb.AppendLine("<font color=\"#aaaaaa\"><i>Visible when holding a pickaxe in this area. Active for 1 hour.</i></font>");
        }

        capi.ShowChatMessage(sb.ToString().TrimEnd());
        UpdateHighlightState();
    }

    private void OnClientTick(float dt)
    {
        if (ActiveSurvey == null)
        {
            if (isHighlightActive)
            {
                ClearHighlights();
            }
            return;
        }

        // Check expiration
        if (DateTime.UtcNow >= ActiveSurvey.ExpireTime)
        {
            capi.ShowChatMessage("<font color=\"#aaaaaa\">[Prospector's X-Ray] Active survey has expired.</font>");
            ClearHighlights();
            ActiveSurvey = null;
            return;
        }

        IClientPlayer player = capi.World.Player;
        if (player?.Entity == null) return;

        bool holdsPickaxe = IsHoldingPickaxe(player);
        BlockPos playerPos = player.Entity.Pos.AsBlockPos;
        int buffer = ActiveSurvey.Radius + 4;
        bool inBounds = Math.Abs(playerPos.X - ActiveSurvey.Center.X) <= buffer
                     && Math.Abs(playerPos.Z - ActiveSurvey.Center.Z) <= buffer
                     && Math.Abs(playerPos.Y - ActiveSurvey.Center.Y) <= buffer;

        // Zero-cost tick: only update GPU highlights if state or player boundary changed!
        if (holdsPickaxe != lastHoldsPickaxe || inBounds != lastInBounds || needsHighlightRefresh)
        {
            lastHoldsPickaxe = holdsPickaxe;
            lastInBounds = inBounds;
            UpdateHighlightState();
        }
    }

    private void OnActiveSlotChanged(ActiveSlotChangeEventArgs args)
    {
        needsHighlightRefresh = true;
        UpdateHighlightState();
    }

    public void UpdateHighlightState()
    {
        needsHighlightRefresh = false;

        if (ActiveSurvey == null || manualHideOverride || ActiveSurvey.RemainingOres.Count == 0)
        {
            if (isHighlightActive)
            {
                ClearHighlights();
            }
            return;
        }

        IClientPlayer player = capi.World.Player;
        if (player?.Entity == null) return;

        bool holdsPickaxe = IsHoldingPickaxe(player);
        lastHoldsPickaxe = holdsPickaxe;

        BlockPos playerPos = player.Entity.Pos.AsBlockPos;
        int buffer = ActiveSurvey.Radius + 4;
        bool inBounds = Math.Abs(playerPos.X - ActiveSurvey.Center.X) <= buffer
                     && Math.Abs(playerPos.Z - ActiveSurvey.Center.Z) <= buffer
                     && Math.Abs(playerPos.Y - ActiveSurvey.Center.Y) <= buffer;
        lastInBounds = inBounds;

        bool shouldShow = holdsPickaxe && inBounds;

        if (shouldShow)
        {
            List<BlockPos> visiblePositions = new();
            List<int> visibleColors = new();

            foreach (var pos in ActiveSurvey.RemainingOres)
            {
                string mineral = ActiveSurvey.OreMinerals.GetValueOrDefault(pos, "");
                if (OreFilterManager.IsOreDisabled(mineral, DisabledOres))
                {
                    continue;
                }

                visiblePositions.Add(pos);
                int baseColor = ActiveSurvey.OreColors.GetValueOrDefault(pos, OreColorHelper.DefaultColor);
                visibleColors.Add(ApplyOpacity(baseColor));
            }

            if (visiblePositions.Count > 0)
            {
                IsTesselatingOurHighlight = true;
                try
                {
                    capi.World.HighlightBlocks(
                        player,
                        HighlightSlotId,
                        visiblePositions,
                        visibleColors,
                        EnumHighlightBlocksMode.Absolute,
                        EnumHighlightShape.Arbitrary
                    );
                }
                finally
                {
                    IsTesselatingOurHighlight = false;
                }
                isHighlightActive = true;
            }
            else if (isHighlightActive)
            {
                ClearHighlights();
            }
        }
        else
        {
            if (isHighlightActive)
            {
                ClearHighlights();
            }
        }
    }

    private void OnBlockChanged(BlockPos pos, Block oldBlock)
    {
        if (ActiveSurvey == null) return;

        bool wasOreMined = false;

        if (ActiveSurvey.RemainingOres.Remove(pos))
        {
            ActiveSurvey.OreColors.Remove(pos);
            ActiveSurvey.OreMinerals.Remove(pos);
            wasOreMined = true;
        }
        else
        {
            int hiddenIndex = ActiveSurvey.HiddenOres.FindIndex(h => h.Pos.Equals(pos));
            if (hiddenIndex >= 0)
            {
                ActiveSurvey.HiddenOres.RemoveAt(hiddenIndex);
                wasOreMined = true;
            }
        }

        if (wasOreMined)
        {
            needsHighlightRefresh = true;

            if (ActiveSurvey.HiddenOres.Count > 0)
            {
                int revealBatch = Math.Max(1, (int)Math.Ceiling(ActiveSurvey.TotalDetected * 0.05));
                int toReveal = Math.Min(revealBatch, ActiveSurvey.HiddenOres.Count);

                for (int i = 0; i < toReveal; i++)
                {
                    var item = ActiveSurvey.HiddenOres[0];
                    ActiveSurvey.HiddenOres.RemoveAt(0);

                    ActiveSurvey.RemainingOres.Add(item.Pos);
                    ActiveSurvey.OreColors[item.Pos] = item.Color;
                    ActiveSurvey.OreMinerals[item.Pos] = item.Mineral;
                }

                capi.ShowChatMessage($"<font color=\"#ffdf55\"><strong>[Prospector's X-Ray] Ore mined! Revealed +{toReveal} more ore locations ({ActiveSurvey.HiddenOres.Count} still hidden).</strong></font>");
            }

            if (ActiveSurvey.RemainingOres.Count == 0 && ActiveSurvey.HiddenOres.Count == 0)
            {
                ClearHighlights();
                capi.ShowChatMessage("<font color=\"#55ff55\"><strong>[Prospector's X-Ray] All ores in this survey deposit have been mined!</strong></font>");
                ActiveSurvey = null;
            }
            else if (isHighlightActive)
            {
                UpdateHighlightState();
            }
        }
    }

    public void ClearHighlights()
    {
        IClientPlayer player = capi.World.Player;
        if (player != null)
        {
            capi.World.HighlightBlocks(
                player,
                HighlightSlotId,
                new List<BlockPos>(),
                EnumHighlightBlocksMode.Absolute,
                EnumHighlightShape.Arbitrary
            );
        }
        isHighlightActive = false;
    }

    public void ClearSurveyByUser()
    {
        ClearHighlights();
        ActiveSurvey = null;
        manualHideOverride = false;
        capi.ShowChatMessage("<font color=\"#55ff55\">[Prospector's X-Ray] Active survey cleared.</font>");
    }

    private bool IsHoldingPickaxe(IClientPlayer player)
    {
        if (player.InventoryManager == null) return false;

        ItemSlot activeSlot = player.InventoryManager.ActiveHotbarSlot;
        if (IsPickaxe(activeSlot)) return true;

        ItemSlot offhandSlot = player.Entity.LeftHandItemSlot;
        if (IsPickaxe(offhandSlot)) return true;

        return false;
    }

    private static bool IsPickaxe(ItemSlot? slot)
    {
        if (slot?.Itemstack?.Collectible == null) return false;
        string path = slot.Itemstack.Collectible.Code?.Path?.ToLowerInvariant() ?? "";
        return path.Contains("prospectingpick") || path.Contains("pickaxe");
    }

    private void RegisterCommands()
    {
        var oreCmd = capi.ChatCommands.Create("ore")
            .WithDescription("Prospector's X-Ray commands")
            .HandleWith(_ => TextCommandResult.Success(GetHelpText()));

        oreCmd.BeginSubCommand("help")
            .WithDescription("Show command help")
            .HandleWith(_ => TextCommandResult.Success(GetHelpText()))
            .EndSubCommand();

        oreCmd.BeginSubCommand("clear")
            .WithDescription("Clear the active ore survey")
            .HandleWith(_ =>
            {
                ClearSurveyByUser();
                return TextCommandResult.Success();
            })
            .EndSubCommand();

        oreCmd.BeginSubCommand("reload")
            .WithAlias("reset")
            .WithDescription("Reset all settings to default (Block mode, 25% size, 60% opacity, all ores enabled)")
            .HandleWith(_ =>
            {
                ResetDefaults();
                return TextCommandResult.Success();
            })
            .EndSubCommand();

        oreCmd.BeginSubCommand("display")
            .WithDescription("Set display mode: 'wireframe' (outline edges) or 'block' (solid/mini box)")
            .WithArgs(new WordArgParser("mode", true, new[] { "wireframe", "block" }))
            .HandleWith(args =>
            {
                string modeStr = (args[0] as string)?.ToLowerInvariant() ?? "";
                if (modeStr == "wireframe" || modeStr == "lines" || modeStr == "wire")
                {
                    SetDisplayMode(DisplayModeType.Wireframe);
                    return TextCommandResult.Success("Prospector's X-Ray display mode set to <font color=\"#55ff55\">Wireframe</font> (sleek block outlines).");
                }
                if (modeStr == "block" || modeStr == "solid" || modeStr == "cube")
                {
                    SetDisplayMode(DisplayModeType.Block);
                    return TextCommandResult.Success($"Prospector's X-Ray display mode set to <font color=\"#55ff55\">Block</font> (mini-cube size: {DisplaySizePercent}%).");
                }
                return TextCommandResult.Success("Usage: /ore display wireframe | /ore display block");
            })
            .EndSubCommand();

        oreCmd.BeginSubCommand("displaysize")
            .WithAlias("size")
            .WithDescription("Set block highlight size percentage in block mode (10 to 100, e.g. 25 for 25% mini-box)")
            .WithArgs(capi.ChatCommands.Parsers.IntRange("percent", 10, 100))
            .HandleWith(args =>
            {
                int val = (int)args[0];
                SetDisplaySize(val);
                return TextCommandResult.Success($"Prospector's X-Ray block size set to <font color=\"#55ff55\">{val}%</font> of full block.");
            })
            .EndSubCommand();

        oreCmd.BeginSubCommand("displayopacity")
            .WithAlias("capacity")
            .WithAlias("opacity")
            .WithDescription("Set X-Ray highlight opacity (0 to 100)")
            .WithArgs(capi.ChatCommands.Parsers.IntRange("percent", 0, 100))
            .HandleWith(args =>
            {
                int val = (int)args[0];
                SetDisplayOpacity(val);
                return TextCommandResult.Success($"Prospector's X-Ray opacity set to <font color=\"#55ff55\">{val}%</font>.");
            })
            .EndSubCommand();

        oreCmd.BeginSubCommand("showonly")
            .WithAlias("shownow")
            .WithDescription("Show only the specified ore, group, or metals (e.g. /ore showonly copper, /ore showonly ingotable)")
            .WithArgs(new WordArgParser("ore", false, OreFilterManager.Suggestions))
            .HandleWith(args =>
            {
                string? input = args[0] as string;
                var result = OreFilterManager.ShowOnly(input ?? "", DisabledOres);
                if (result.Success)
                {
                    SaveConfig();
                    needsHighlightRefresh = true;
                    UpdateHighlightState();
                }
                return TextCommandResult.Success(result.Message);
            })
            .EndSubCommand();

        oreCmd.BeginSubCommand("hideonly")
            .WithDescription("Hide only the specified ore, group, or metals, showing all others")
            .WithArgs(new WordArgParser("ore", false, OreFilterManager.Suggestions))
            .HandleWith(args =>
            {
                string? input = args[0] as string;
                var result = OreFilterManager.HideOnly(input ?? "", DisabledOres);
                if (result.Success)
                {
                    SaveConfig();
                    needsHighlightRefresh = true;
                    UpdateHighlightState();
                }
                return TextCommandResult.Success(result.Message);
            })
            .EndSubCommand();

        oreCmd.BeginSubCommand("show")
            .WithDescription("Show an ore, group, or metal set (e.g. /ore show copper, /ore show magnetite, /ore show ingotable)")
            .WithArgs(new WordArgParser("ore", false, OreFilterManager.Suggestions))
            .HandleWith(args =>
            {
                string? input = args[0] as string;
                var result = OreFilterManager.EnableOre(input ?? "", DisabledOres);
                if (result.Success)
                {
                    SaveConfig();
                    needsHighlightRefresh = true;
                    UpdateHighlightState();
                }
                return TextCommandResult.Success(result.Message);
            })
            .EndSubCommand();

        oreCmd.BeginSubCommand("hide")
            .WithDescription("Hide an ore, group, or metal set (e.g. /ore hide coal, /ore hide chromium, /ore hide ingotable)")
            .WithArgs(new WordArgParser("ore", false, OreFilterManager.Suggestions))
            .HandleWith(args =>
            {
                string? input = args[0] as string;
                var result = OreFilterManager.DisableOre(input ?? "", DisabledOres);
                if (result.Success)
                {
                    SaveConfig();
                    needsHighlightRefresh = true;
                    UpdateHighlightState();
                }
                return TextCommandResult.Success(result.Message);
            })
            .EndSubCommand();

        oreCmd.BeginSubCommand("info")
            .WithDescription("Show status of the active survey")
            .HandleWith(_ =>
            {
                if (ActiveSurvey == null)
                {
                    return TextCommandResult.Success("No active survey. Use a Prospecting Pick to perform a survey.");
                }

                int minsLeft = (int)Math.Max(0, (ActiveSurvey.ExpireTime - DateTime.UtcNow).TotalMinutes);
                return TextCommandResult.Success($"Active survey ({ActiveSurvey.ModeName}): {ActiveSurvey.RemainingOres.Count} highlighted ores remaining. Expires in {minsLeft} minutes. Mode: {DisplayMode} ({DisplaySizePercent}% size, {DisplayOpacity}% opacity).");
            })
            .EndSubCommand();

        var listCmd = oreCmd.BeginSubCommand("list")
            .WithDescription("List all ores and their visibility (options: 'group', 'ore', 'all')")
            .WithArgs(new WordArgParser("mode", false, OreFilterManager.ListSuggestions))
            .HandleWith(args =>
            {
                string? mode = args[0] as string;
                if (mode != null && (mode.Equals("group", StringComparison.OrdinalIgnoreCase) || mode.Equals("groups", StringComparison.OrdinalIgnoreCase)))
                {
                    return TextCommandResult.Success(OreFilterManager.GetGroupsListText(DisabledOres));
                }
                if (mode != null && (mode.Equals("ore", StringComparison.OrdinalIgnoreCase) || mode.Equals("ores", StringComparison.OrdinalIgnoreCase)))
                {
                    return TextCommandResult.Success(OreFilterManager.GetIndividualOresListText(DisabledOres));
                }
                return TextCommandResult.Success(OreFilterManager.GetFullListText(DisabledOres));
            });

        listCmd.BeginSubCommand("group")
            .WithAlias("groups")
            .WithDescription("List ore groups/families")
            .HandleWith(_ => TextCommandResult.Success(OreFilterManager.GetGroupsListText(DisabledOres)))
            .EndSubCommand();

        listCmd.BeginSubCommand("ore")
            .WithAlias("ores")
            .WithDescription("List individual ore variants")
            .HandleWith(_ => TextCommandResult.Success(OreFilterManager.GetIndividualOresListText(DisabledOres)))
            .EndSubCommand();

        listCmd.EndSubCommand();
    }

    private void OnSendChatMessage(int groupId, ref string message, ref EnumHandling handled)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        string trimmed = message.Trim();
        if (trimmed.Equals("/ore", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("/ore ", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals(".ore", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith(".ore ", StringComparison.OrdinalIgnoreCase))
        {
            handled = EnumHandling.PreventDefault;
            string commandArgs = trimmed.Length > 4 ? trimmed.Substring(4).Trim() : "";
            ProcessOreCommand(commandArgs);
        }
    }

    public void ProcessOreCommand(string commandArgs)
    {
        if (string.IsNullOrWhiteSpace(commandArgs))
        {
            capi.ShowChatMessage(GetHelpText());
            return;
        }

        string[] parts = commandArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string subCmd = parts[0].ToLowerInvariant();
        string target = parts.Length > 1 ? string.Join(" ", parts.Skip(1)).Trim() : "";

        switch (subCmd)
        {
            case "help":
            case "?":
                capi.ShowChatMessage(GetHelpText());
                break;

            case "display":
                if (target.Equals("wireframe", StringComparison.OrdinalIgnoreCase) || target.Equals("wire", StringComparison.OrdinalIgnoreCase) || target.Equals("lines", StringComparison.OrdinalIgnoreCase))
                {
                    SetDisplayMode(DisplayModeType.Wireframe);
                    capi.ShowChatMessage("<font color=\"#55ff55\"><strong>[Prospector's X-Ray] Display mode set to Wireframe (outlines only).</strong></font>");
                }
                else if (target.Equals("block", StringComparison.OrdinalIgnoreCase) || target.Equals("solid", StringComparison.OrdinalIgnoreCase) || target.Equals("cube", StringComparison.OrdinalIgnoreCase))
                {
                    SetDisplayMode(DisplayModeType.Block);
                    capi.ShowChatMessage($"<font color=\"#55ff55\"><strong>[Prospector's X-Ray] Display mode set to Block (size: {DisplaySizePercent}%).</strong></font>");
                }
                else
                {
                    capi.ShowChatMessage("<font color=\"#ff5555\">Usage: /ore display wireframe | /ore display block</font>");
                }
                break;

            case "displaysize":
            case "size":
                if (int.TryParse(target.Replace("%", ""), out int sizeVal) && sizeVal >= 10 && sizeVal <= 100)
                {
                    SetDisplaySize(sizeVal);
                    capi.ShowChatMessage($"<font color=\"#55ff55\"><strong>[Prospector's X-Ray] Highlight block size set to {sizeVal}% of full block.</strong></font>");
                }
                else
                {
                    capi.ShowChatMessage("<font color=\"#ff5555\">Usage: /ore displaysize [10-100] (e.g. /ore displaysize 25 for non-intrusive 25% mini-box)</font>");
                }
                break;

            case "displayopacity":
            case "opacity":
            case "capacity":
            case "cap":
                if (int.TryParse(target.Replace("%", ""), out int opVal) && opVal >= 0 && opVal <= 100)
                {
                    SetDisplayOpacity(opVal);
                    capi.ShowChatMessage($"<font color=\"#55ff55\"><strong>[Prospector's X-Ray] X-Ray opacity set to {DisplayOpacity}%.</strong></font>");
                }
                else
                {
                    capi.ShowChatMessage("<font color=\"#ff5555\">Usage: /ore displayopacity [0-100] (e.g. /ore displayopacity 60)</font>");
                }
                break;

            case "list":
                if (target.Equals("group", StringComparison.OrdinalIgnoreCase) || target.Equals("groups", StringComparison.OrdinalIgnoreCase))
                {
                    capi.ShowChatMessage(OreFilterManager.GetGroupsListText(DisabledOres));
                }
                else if (target.Equals("ore", StringComparison.OrdinalIgnoreCase) || target.Equals("ores", StringComparison.OrdinalIgnoreCase))
                {
                    capi.ShowChatMessage(OreFilterManager.GetIndividualOresListText(DisabledOres));
                }
                else
                {
                    capi.ShowChatMessage(OreFilterManager.GetFullListText(DisabledOres));
                }
                break;

            case "reload":
            case "reset":
                ResetDefaults();
                break;

            case "showonly":
            case "shownow":
                if (string.IsNullOrWhiteSpace(target))
                {
                    capi.ShowChatMessage("<font color=\"#ff5555\">Usage: /ore showonly [name] (e.g. /ore showonly copper, /ore showonly ingotable). Use '/ore list' to see all ores.</font>");
                }
                else
                {
                    var res = OreFilterManager.ShowOnly(target, DisabledOres);
                    if (res.Success)
                    {
                        SaveConfig();
                        needsHighlightRefresh = true;
                        UpdateHighlightState();
                    }
                    capi.ShowChatMessage(res.Message);
                }
                break;

            case "hideonly":
                if (string.IsNullOrWhiteSpace(target))
                {
                    capi.ShowChatMessage("<font color=\"#ff5555\">Usage: /ore hideonly [name] (e.g. /ore hideonly copper, /ore hideonly ingotable). Use '/ore list' to see all ores.</font>");
                }
                else
                {
                    var res = OreFilterManager.HideOnly(target, DisabledOres);
                    if (res.Success)
                    {
                        SaveConfig();
                        needsHighlightRefresh = true;
                        UpdateHighlightState();
                    }
                    capi.ShowChatMessage(res.Message);
                }
                break;

            case "show":
                if (string.IsNullOrWhiteSpace(target))
                {
                    capi.ShowChatMessage("<font color=\"#ff5555\">Usage: /ore show [name] (e.g. /ore show copper, /ore show magnetite, /ore show ingotable, /ore show all). Use '/ore list' to see all ores.</font>");
                }
                else
                {
                    var res = OreFilterManager.EnableOre(target, DisabledOres);
                    if (res.Success)
                    {
                        SaveConfig();
                        needsHighlightRefresh = true;
                        UpdateHighlightState();
                    }
                    capi.ShowChatMessage(res.Message);
                }
                break;

            case "hide":
                if (string.IsNullOrWhiteSpace(target))
                {
                    capi.ShowChatMessage("<font color=\"#ff5555\">Usage: /ore hide [name] (e.g. /ore hide coal, /ore hide chromium, /ore hide ingotable, /ore hide all). Use '/ore list' to see all ores.</font>");
                }
                else
                {
                    var res = OreFilterManager.DisableOre(target, DisabledOres);
                    if (res.Success)
                    {
                        SaveConfig();
                        needsHighlightRefresh = true;
                        UpdateHighlightState();
                    }
                    capi.ShowChatMessage(res.Message);
                }
                break;

            case "clear":
                ClearSurveyByUser();
                break;

            case "info":
                if (ActiveSurvey == null)
                {
                    capi.ShowChatMessage("<font color=\"#aaaaaa\">[Prospector's X-Ray] No active survey. Use a Prospecting Pick to perform a survey.</font>");
                }
                else
                {
                    int minsLeft = (int)Math.Max(0, (ActiveSurvey.ExpireTime - DateTime.UtcNow).TotalMinutes);
                    int hiddenCount = ActiveSurvey.HiddenOres.Count;
                    int visibleCount = ActiveSurvey.RemainingOres.Count;
                    capi.ShowChatMessage($"<font color=\"#ffdf55\"><strong>[Prospector's X-Ray] Active survey ({ActiveSurvey.ModeName}):</strong></font>\n" +
                                         $" • Visible ores remaining: <font color=\"#ffffff\">{visibleCount}</font>\n" +
                                         $" • Hidden bonus ores waiting: <font color=\"#ffffff\">{hiddenCount}</font>\n" +
                                         $" • Mode: <font color=\"#ffffff\">{DisplayMode}</font> ({DisplaySizePercent}% size, {DisplayOpacity}% opacity)\n" +
                                         $" • Expires in: <font color=\"#ffffff\">{minsLeft}</font> minutes");
                }
                break;

            default:
                capi.ShowChatMessage("<font color=\"#ff5555\">Unknown command. Type /ore help for available commands.</font>");
                break;
        }
    }

    public void ResetDefaults()
    {
        DisplayOpacity = 60;
        DisplayMode = DisplayModeType.Block;
        DisplaySizePercent = 25;
        DisabledOres.Clear();
        SaveConfig();
        needsHighlightRefresh = true;
        UpdateHighlightState();
        capi.ShowChatMessage("<font color=\"#55ff55\"><strong>[Prospector's X-Ray] Settings reset to default (Block mode, 25% size, 60% opacity, all ores visible).</strong></font>");
    }

    private string GetHelpText()
    {
        return "<font color=\"#ffdf55\"><strong>[Prospector's X-Ray Commands]</strong></font>\n" +
               " • <font color=\"#ffffff\">/ore display wireframe|block</font> - Switch between wireframe outlines & mini-blocks\n" +
               " • <font color=\"#ffffff\">/ore displaysize [10-100]</font> - Set block size (e.g. 25 = 25% mini-box in center of block)\n" +
               " • <font color=\"#ffffff\">/ore displayopacity [0-100]</font> - Set highlight opacity (default: 60%)\n" +
               " • <font color=\"#ffffff\">/ore list</font> - List all ore groups & individual variants\n" +
               " • <font color=\"#ffffff\">/ore list group</font> - List only ore groups/families\n" +
               " • <font color=\"#ffffff\">/ore list ore</font> - List only individual ore variants\n" +
               " • <font color=\"#ffffff\">/ore show [name|ingotable|all]</font> - Show group, ore (e.g. /ore show magnetite), or ingot metals\n" +
               " • <font color=\"#ffffff\">/ore hide [name|ingotable|all]</font> - Hide group, ore (e.g. /ore hide chromium), or ingot metals\n" +
               " • <font color=\"#ffffff\">/ore showonly [name|ingotable]</font> - Show ONLY this group/ore/metals (hides all others)\n" +
               " • <font color=\"#ffffff\">/ore hideonly [name|ingotable]</font> - Hide ONLY this group/ore/metals (shows all others)\n" +
               " • <font color=\"#ffffff\">/ore clear</font> - Clear active survey and hide highlights\n" +
               " • <font color=\"#ffffff\">/ore info</font> - Show active survey and display status\n" +
               " • <font color=\"#ffffff\">/ore reload</font> - Reset all settings to defaults";
    }
}
