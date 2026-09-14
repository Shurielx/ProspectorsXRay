using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using OpenTK.Graphics.OpenGL;
using ProspectorsXRay.OreDatabase.Services;
using ProspectorsXRay.Survey;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ProspectorsXRay.Patches;

public static class ItemProspectingPickPatch
{
    private static SkillItem? cachedMode32;
    private static SkillItem? cachedMode64;
    private static readonly HashSet<Type> PatchedTypes = new();

    public static SkillItem GetOrCreateMode32(ICoreClientAPI? capi)
    {
        if (cachedMode32 == null)
        {
            cachedMode32 = new SkillItem
            {
                Code = new AssetLocation("game", "instinct-32"),
                Name = "Instinct: 32x32x32 (3 Samples)"
            };
        }

        if (capi != null && cachedMode32.Texture == null)
        {
            try
            {
                cachedMode32.WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("textures/icons/rocks.svg"), 48, 48, 5, -1));
                cachedMode32.TexturePremultipliedAlpha = false;
            }
            catch (Exception ex)
            {
                capi.Logger.Warning($"[Prospector's X-Ray] Could not load 32x32 icon: {ex.Message}");
            }
        }

        return cachedMode32;
    }

    public static SkillItem GetOrCreateMode64(ICoreClientAPI? capi)
    {
        if (cachedMode64 == null)
        {
            cachedMode64 = new SkillItem
            {
                Code = new AssetLocation("game", "instinct-64"),
                Name = "Instinct: 64x64x64 (5 Samples)"
            };
        }

        if (capi != null && cachedMode64.Texture == null)
        {
            try
            {
                cachedMode64.WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("textures/icons/heatmap.svg"), 48, 48, 5, -1));
                cachedMode64.TexturePremultipliedAlpha = false;
            }
            catch (Exception ex)
            {
                capi.Logger.Warning($"[Prospector's X-Ray] Could not load 64x64 icon: {ex.Message}");
            }
        }

        return cachedMode64;
    }

    public static void PatchAllProspectingPicks(Harmony harmony, ICoreAPI api)
    {
        var postfixGetModes = new HarmonyMethod(typeof(ItemProspectingPickPatch), nameof(GetToolModes_Postfix));
        var postfixGetMode = new HarmonyMethod(typeof(ItemProspectingPickPatch), nameof(GetToolMode_Postfix));
        var postfixSetMode = new HarmonyMethod(typeof(ItemProspectingPickPatch), nameof(SetToolMode_Postfix));
        var prefixBrokenWith = new HarmonyMethod(typeof(ItemProspectingPickPatch), nameof(OnBlockBrokenWith_Prefix));

        HashSet<Type> targetTypes = new();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly asm in assemblies)
        {
            Type[] types;
            try { types = asm.GetTypes(); } catch { continue; }

            foreach (Type t in types)
            {
                if (typeof(ItemProspectingPick).IsAssignableFrom(t) || t.Name.Contains("ProspectingPick"))
                {
                    targetTypes.Add(t);
                }
            }
        }

        foreach (Type t in targetTypes)
        {
            if (PatchedTypes.Contains(t)) continue;

            try
            {
                MethodInfo? mGetModes = t.GetMethod("GetToolModes", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mGetModes != null)
                {
                    harmony.Patch(mGetModes, postfix: postfixGetModes);
                }

                MethodInfo? mGetMode = t.GetMethod("GetToolMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mGetMode != null)
                {
                    harmony.Patch(mGetMode, postfix: postfixGetMode);
                }

                MethodInfo? mSetMode = t.GetMethod("SetToolMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mSetMode != null)
                {
                    harmony.Patch(mSetMode, postfix: postfixSetMode);
                }

                MethodInfo? mBroken = t.GetMethod("OnBlockBrokenWith", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mBroken != null)
                {
                    harmony.Patch(mBroken, prefix: prefixBrokenWith);
                }

                PatchedTypes.Add(t);
                api.Logger.Notification($"[Prospector's X-Ray] Successfully hooked prospecting pick class: {t.FullName}");
            }
            catch (Exception ex)
            {
                api.Logger.Warning($"[Prospector's X-Ray] Could not hook {t.FullName}: {ex.Message}");
            }
        }
    }

    public static void GetToolModes_Postfix(Item __instance, ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel, ref SkillItem[]? __result)
    {
        if (__result == null) return;

        ICoreClientAPI? capi = ProspectorsXRayModSystem.ClientApi;
        if (capi == null) return;

        // Ensure textures are loaded for all instinct modes in the array
        for (int i = 0; i < __result.Length; i++)
        {
            SkillItem skill = __result[i];
            if (skill?.Code?.Path == null) continue;

            if (skill.Code.Path.EndsWith("instinct-32") && skill.Texture == null)
            {
                skill.WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("textures/icons/rocks.svg"), 48, 48, 5, -1));
                skill.TexturePremultipliedAlpha = false;
            }
            else if (skill.Code.Path.EndsWith("instinct-64") && skill.Texture == null)
            {
                skill.WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("textures/icons/heatmap.svg"), 48, 48, 5, -1));
                skill.TexturePremultipliedAlpha = false;
            }
        }

        // If not present in array, append them
        if (!__result.Any(m => m?.Code?.Path?.EndsWith("instinct-32") == true))
        {
            SkillItem mode32 = GetOrCreateMode32(capi);
            SkillItem mode64 = GetOrCreateMode64(capi);

            SkillItem[] expanded = new SkillItem[__result.Length + 2];
            Array.Copy(__result, expanded, __result.Length);
            expanded[__result.Length] = mode32;
            expanded[__result.Length + 1] = mode64;

            __result = expanded;
        }
    }

    public static void SetToolMode_Postfix(Item __instance, ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel, int toolMode)
    {
        if (slot?.Itemstack?.Attributes == null) return;

        // Determine tool mode name
        FieldInfo? f = typeof(ItemProspectingPick).GetField("toolModes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        SkillItem[]? modes = f?.GetValue(__instance) as SkillItem[];
        if (modes == null && byPlayer is IClientPlayer clientPlayer)
        {
            modes = __instance.GetToolModes(slot, clientPlayer, blockSel);
        }

        int vanillaCount = (byPlayer.Entity?.World?.Config?.GetAsInt("propickNodeSearchRadius", 0) ?? 0) > 0 ? 2 : 1;

        if (modes != null && toolMode >= 0 && toolMode < modes.Length)
        {
            string? path = modes[toolMode]?.Code?.Path;
            if (path != null && path.EndsWith("instinct-32"))
            {
                slot.Itemstack.Attributes.SetString("instinctModeCode", "instinct-32");
            }
            else if (path != null && path.EndsWith("instinct-64"))
            {
                slot.Itemstack.Attributes.SetString("instinctModeCode", "instinct-64");
            }
            else
            {
                slot.Itemstack.Attributes.RemoveAttribute("instinctModeCode");
            }
        }
        else
        {
            // Fallback: check index relative to vanilla modes
            if (toolMode == vanillaCount)
            {
                slot.Itemstack.Attributes.SetString("instinctModeCode", "instinct-32");
            }
            else if (toolMode == vanillaCount + 1)
            {
                slot.Itemstack.Attributes.SetString("instinctModeCode", "instinct-64");
            }
            else
            {
                slot.Itemstack.Attributes.RemoveAttribute("instinctModeCode");
            }
        }

        slot.MarkDirty();
    }

    public static void GetToolMode_Postfix(Item __instance, ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel, ref int __result)
    {
        if (slot?.Itemstack?.Attributes != null)
        {
            int stored = slot.Itemstack.Attributes.GetInt("toolMode", -1);
            if (stored >= 0)
            {
                // Prevent vanilla Math.Min clamping from cutting off our custom toolMode
                __result = stored;
            }
        }
    }

    public static bool OnBlockBrokenWith_Prefix(Item __instance, IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel, float dropQuantityMultiplier, ref bool __result)
    {
        if (byEntity is not EntityPlayer entityPlayer) return true;
        IPlayer player = entityPlayer.Player;
        if (player == null || blockSel?.Position == null || itemslot?.Itemstack == null) return true;

        string? instinctCode = itemslot.Itemstack.Attributes?.GetString("instinctModeCode", null);
        int toolMode = itemslot.Itemstack.Attributes?.GetInt("toolMode", -1) ?? -1;

        if (instinctCode == null)
        {
            FieldInfo? f = typeof(ItemProspectingPick).GetField("toolModes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            SkillItem[]? modes = f?.GetValue(__instance) as SkillItem[];
            if (modes != null && toolMode >= 0 && toolMode < modes.Length)
            {
                string? p = modes[toolMode]?.Code?.Path;
                if (p != null && (p.EndsWith("instinct-32") || p.EndsWith("instinct-64")))
                {
                    instinctCode = p.EndsWith("instinct-32") ? "instinct-32" : "instinct-64";
                }
            }
        }

        // Bulletproof fallback: check toolMode index against vanilla count
        if (instinctCode == null && toolMode >= 0)
        {
            int vanillaCount = (world.Config.GetAsInt("propickNodeSearchRadius", 0) > 0) ? 2 : 1;
            if (toolMode == vanillaCount)
            {
                instinctCode = "instinct-32";
            }
            else if (toolMode == vanillaCount + 1)
            {
                instinctCode = "instinct-64";
            }
        }

        if (instinctCode != "instinct-32" && instinctCode != "instinct-64")
        {
            // Pass through to vanilla or other mods
            return true;
        }

        // Check if block is propickable
        Block block = world.BlockAccessor.GetBlock(blockSel.Position);
        if (block == null) return true;

        bool isPropickable = block.BlockMaterial == EnumBlockMaterial.Stone || block.BlockMaterial == EnumBlockMaterial.Ore;
        if (!isPropickable)
        {
            return true;
        }

        // Break the sample block (0 drop multiplier for stone samples, exactly like vanilla propick)
        block.OnBlockBroken(world, blockSel.Position, player, 0f);

        int targetSamples = (instinctCode == "instinct-32") ? 3 : 5;
        int radius = (instinctCode == "instinct-32") ? 32 : 64;
        string modeName = (instinctCode == "instinct-32") ? "32x32x32" : "64x64x64";

        if (player is IServerPlayer serverPlayer)
        {
            int dmg = HandleSampleOrSurvey(__instance, world, serverPlayer, itemslot, blockSel, radius, targetSamples, modeName);
            DamageItem(__instance, world, byEntity, itemslot, dmg);
        }

        __result = true;
        return false;
    }

    public static int HandleSampleOrSurvey(Item? pick, IWorldAccessor world, IServerPlayer serverPlayer, ItemSlot itemslot, BlockSelection blockSel, int radius, int targetSamples, string modeName)
    {
        // In creative mode, 1 sample completes survey immediately
        if (serverPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative)
        {
            ProspectorsXRayModSystem.ServerSurveyEngine?.ExecuteSurvey(serverPlayer, blockSel.Position.Copy(), radius, modeName);
            return 2;
        }

        if (itemslot?.Itemstack == null) return 1;
        ITreeAttribute attrs = itemslot.Itemstack.Attributes;
        IntArrayAttribute? probeAttr = attrs["instinctSamples"] as IntArrayAttribute;

        if (probeAttr == null || probeAttr.value == null || probeAttr.value.Length == 0)
        {
            probeAttr = new IntArrayAttribute();
            attrs["instinctSamples"] = probeAttr;
            probeAttr.AddInt(new int[] { blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z });
            itemslot.MarkDirty();

            int needed = targetSamples - 1;
            serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup,
                $"[Prospector's X-Ray] Need {needed} more sample{(needed > 1 ? "s" : "")} for {modeName} survey.",
                EnumChatType.Notification);

            return 2;
        }

        int[] sampleValues = probeAttr.value!;
        int firstX = sampleValues[0];
        int firstY = sampleValues[1];
        int firstZ = sampleValues[2];
        double dist = Math.Sqrt(blockSel.Position.DistanceTo(firstX, firstY, firstZ));

        if (dist > 24)
        {
            // Reset sampling if moved too far from initial sample point
            probeAttr.value = new int[] { blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z };
            itemslot.MarkDirty();
            int needed = targetSamples - 1;
            serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup,
                $"[Prospector's X-Ray] Sample too far away from initial point. Starting new sample here (need {needed} more).",
                EnumChatType.Notification);

            return 2;
        }

        probeAttr.AddInt(new int[] { blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z });
        itemslot.MarkDirty();
        int samplesTaken = probeAttr.value.Length / 3;

        if (samplesTaken < targetSamples)
        {
            int needed = targetSamples - samplesTaken;
            serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup,
                $"[Prospector's X-Ray] Need {needed} more sample{(needed > 1 ? "s" : "")} for {modeName} survey.",
                EnumChatType.Notification);
        }
        else
        {
            // All samples gathered! Average positions for accurate center
            int sumX = 0, sumY = 0, sumZ = 0;
            int count = probeAttr.value.Length / 3;
            for (int i = 0; i < probeAttr.value.Length; i += 3)
            {
                sumX += probeAttr.value[i];
                sumY += probeAttr.value[i + 1];
                sumZ += probeAttr.value[i + 2];
            }
            BlockPos centerPos = new BlockPos(sumX / count, sumY / count, sumZ / count, 0);

            attrs.RemoveAttribute("instinctSamples");
            itemslot.MarkDirty();

            ProspectorsXRayModSystem.ServerSurveyEngine?.ExecuteSurvey(serverPlayer, centerPos, radius, modeName);
        }

        return 2;
    }

    private static void DamageItem(Item pick, IWorldAccessor world, Entity byEntity, ItemSlot itemslot, int amount)
    {
        pick.DamageItem(world, byEntity, itemslot, amount, true);
    }
}

[HarmonyPatch]
public static class SystemHighlightBlocksPatch
{
    [HarmonyTargetMethod]
    public static MethodBase? TargetMethod()
    {
        Type? type = AccessTools.TypeByName("Vintagestory.Client.NoObf.SystemHighlightBlocks");
        if (type == null) return null;
        return AccessTools.Method(type, "OnRenderFrame3DTransparent");
    }

    [HarmonyPrefix]
    public static void Prefix()
    {
        try
        {
            ProspectorsXRayModSystem.ClientApi?.Render.GLDisableDepthTest();

            if (ActiveSurveyClient.Instance?.DisplayMode == DisplayModeType.Wireframe)
            {
                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
                GL.LineWidth(2.5f);
            }
        }
        catch { }
    }

    [HarmonyPostfix]
    public static void Postfix()
    {
        try
        {
            if (ActiveSurveyClient.Instance?.DisplayMode == DisplayModeType.Wireframe)
            {
                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            }

            ProspectorsXRayModSystem.ClientApi?.Render.GLEnableDepthTest();
        }
        catch { }
    }
}

[HarmonyPatch]
public static class BlockHighlightPatch
{
    [HarmonyTargetMethod]
    public static MethodBase? TargetMethod()
    {
        Type? type = AccessTools.TypeByName("Vintagestory.Client.NoObf.BlockHighlight");
        if (type == null) return null;
        return AccessTools.Method(type, "TesselateArbitraryModel");
    }

    [HarmonyPrefix]
    public static bool Prefix(object __instance, object game, BlockPos[] positions, int[] colors)
    {
        if (!ActiveSurveyClient.IsTesselatingOurHighlight) return true;

        int sizePercent = ActiveSurveyClient.Instance?.DisplaySizePercent ?? 25;
        if (sizePercent >= 98) return true; // Full size: let vanilla run

        float size = Math.Clamp(sizePercent / 100f, 0.10f, 1.0f);

        try
        {
            if (positions == null || positions.Length == 0) return true;

            int minX = int.MaxValue, minY = int.MaxValue, minZ = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue, maxZ = int.MinValue;

            for (int i = 0; i < positions.Length; i++)
            {
                BlockPos p = positions[i];
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Z < minZ) minZ = p.Z;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
                if (p.Z > maxZ) maxZ = p.Z;
            }

            BlockPos origin = new BlockPos(minX, minY, minZ, 0);
            Vec3i modelSize = new Vec3i(maxX - minX + 1, maxY - minY + 1, maxZ - minZ + 1);

            FieldInfo? fOrigin = __instance.GetType().GetField("origin", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo? fSize = __instance.GetType().GetField("Size", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo? fAttach = __instance.GetType().GetField("attachmentPoints", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo? fModelRef = __instance.GetType().GetField("modelRef", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            fOrigin?.SetValue(__instance, origin);
            fSize?.SetValue(__instance, modelSize);
            fAttach?.SetValue(__instance, new BlockPos[] { origin });

            MeshData intoMesh = new MeshData(positions.Length * 24, positions.Length * 36, false, false, true, false);
            Vec3f sizeVec = new Vec3f(size, size, size);

            for (int i = 0; i < positions.Length; i++)
            {
                BlockPos p = positions[i];
                int col = (colors != null && i < colors.Length) ? colors[i] : OreColorHelper.DefaultColor;

                Vec3f center = new Vec3f(
                    p.X - origin.X + 0.5f,
                    p.Y - origin.Y + 0.5f,
                    p.Z - origin.Z + 0.5f
                );

                foreach (BlockFacing face in BlockFacing.ALLFACES)
                {
                    ModelCubeUtilExt.AddFaceSkipTex(intoMesh, face, center, sizeVec, col, 1f);
                }
            }

            dynamic clientGame = game;
            MeshRef oldRef = (MeshRef)fModelRef?.GetValue(__instance)!;
            oldRef?.Dispose();

            MeshRef newRef = clientGame.Platform.UploadMesh(intoMesh);
            fModelRef?.SetValue(__instance, newRef);

            return false; // Handled with custom size!
        }
        catch (Exception ex)
        {
            ProspectorsXRayModSystem.ClientApi?.Logger.Warning($"[Prospector's X-Ray] Custom tesselating highlight fallback: {ex.Message}");
            return true;
        }
    }
}
