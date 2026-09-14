using System;
using System.Collections.Generic;
using System.Linq;
using ProspectorsXRay.Network;
using ProspectorsXRay.OreDatabase.Models;
using ProspectorsXRay.OreDatabase.Services;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace ProspectorsXRay.Survey;

public class SurveyEngine
{
    private readonly ICoreServerAPI sapi;
    private readonly IServerNetworkChannel serverChannel;
    private readonly Func<IReadOnlyDictionary<int, OreInfo>> oreDatabaseProvider;

    public SurveyEngine(ICoreServerAPI sapi, IServerNetworkChannel serverChannel, Func<IReadOnlyDictionary<int, OreInfo>> oreDatabaseProvider)
    {
        this.sapi = sapi ?? throw new ArgumentNullException(nameof(sapi));
        this.serverChannel = serverChannel ?? throw new ArgumentNullException(nameof(serverChannel));
        this.oreDatabaseProvider = oreDatabaseProvider ?? throw new ArgumentNullException(nameof(oreDatabaseProvider));
    }

    public void ExecuteSurvey(IServerPlayer player, BlockPos center, int radius, string modeName)
    {
        var oreDatabase = oreDatabaseProvider();
        IBlockAccessor blockAccessor = sapi.World.BlockAccessor;

        int minX = center.X - radius;
        int maxX = center.X + radius;
        int minY = Math.Max(1, center.Y - radius);
        int maxY = Math.Min(blockAccessor.MapSizeY - 1, center.Y + radius);
        int minZ = center.Z - radius;
        int maxZ = center.Z + radius;

        Dictionary<string, int> totalCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        List<(BlockPos Pos, string MineralName, string RawMineral, int Color)> allFoundOres = new();

        BlockPos checkPos = center.Copy();
        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    checkPos.Set(x, y, z);
                    int blockId = blockAccessor.GetBlockId(checkPos);
                    if (oreDatabase.TryGetValue(blockId, out OreInfo? ore) && ore != null)
                    {
                        string name = ore.GetCleanName();
                        totalCounts[name] = totalCounts.GetValueOrDefault(name, 0) + 1;
                        int color = OreColorHelper.GetColorForMineral(ore.Mineral);
                        allFoundOres.Add((checkPos.Copy(), name, ore.Mineral, color));
                    }
                }
            }
        }

        Dictionary<string, int> shownCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        SurveyResultPacket packet = new SurveyResultPacket
        {
            CenterX = center.X,
            CenterY = center.Y,
            CenterZ = center.Z,
            Radius = radius,
            ModeName = modeName,
            TotalFoundCount = allFoundOres.Count,
            TotalCounts = totalCounts,
            ShownCounts = shownCounts
        };

        if (allFoundOres.Count > 0)
        {
            // Reveal exactly 50% of the found ore locations initially, rest queued as hidden bonuses
            int revealCount = Math.Max(1, (int)Math.Round(allFoundOres.Count * 0.50));
            Random rng = new Random();

            // In-place Fisher-Yates shuffle (zero allocation, O(N))
            for (int i = allFoundOres.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (allFoundOres[i], allFoundOres[j]) = (allFoundOres[j], allFoundOres[i]);
            }

            for (int i = 0; i < revealCount; i++)
            {
                var item = allFoundOres[i];
                packet.RevealedX.Add(item.Pos.X);
                packet.RevealedY.Add(item.Pos.Y);
                packet.RevealedZ.Add(item.Pos.Z);
                packet.RevealedColors.Add(item.Color);
                packet.RevealedMinerals.Add(item.RawMineral);

                shownCounts[item.MineralName] = shownCounts.GetValueOrDefault(item.MineralName, 0) + 1;
            }

            for (int i = revealCount; i < allFoundOres.Count; i++)
            {
                var item = allFoundOres[i];
                packet.HiddenX.Add(item.Pos.X);
                packet.HiddenY.Add(item.Pos.Y);
                packet.HiddenZ.Add(item.Pos.Z);
                packet.HiddenColors.Add(item.Color);
                packet.HiddenMinerals.Add(item.RawMineral);
            }
        }

        serverChannel.SendPacket(packet, player);
    }
}
