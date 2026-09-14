using System;
using System.Collections.Generic;
using ProspectorsXRay.OreDatabase.Models;
using Vintagestory.API.Common;

namespace ProspectorsXRay.OreDatabase.Services;

public sealed class OreDatabaseBuilder
{
    private readonly ICoreAPI api;

    public OreDatabaseBuilder(ICoreAPI api)
    {
        this.api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public Dictionary<int, OreInfo> Build()
    {
        Dictionary<int, OreInfo> dictionary = new Dictionary<int, OreInfo>();
        IList<Block> blocks = api.World.Blocks;

        if (blocks == null || blocks.Count == 0)
        {
            api.Logger.Warning("[Prospector's Better Instinct] Ore database could not be built: block registry was empty.");
            return dictionary;
        }

        foreach (Block block in blocks)
        {
            if (block?.Code == null) continue;

            string path = block.Code.Path;
            bool isOreCandidate = path.StartsWith("ore-", StringComparison.OrdinalIgnoreCase) 
                               || path.Equals("bogiron", StringComparison.OrdinalIgnoreCase)
                               || path.Equals("meteorite-iron", StringComparison.OrdinalIgnoreCase)
                               || path.Equals("meteoric-iron", StringComparison.OrdinalIgnoreCase)
                               || path.StartsWith("saltpeter-", StringComparison.OrdinalIgnoreCase)
                               || block.BlockMaterial == EnumBlockMaterial.Ore;

            if (!isOreCandidate) continue;

            if (OreCodeParser.TryParse(path, out string grade, out string mineral, out string hostRock))
            {
                dictionary[block.BlockId] = new OreInfo
                {
                    BlockId = block.BlockId,
                    Code = block.Code,
                    Grade = grade,
                    Mineral = mineral,
                    HostRock = hostRock,
                    IsOre = true
                };
            }
            else if (block.BlockMaterial == EnumBlockMaterial.Ore)
            {
                // Fallback for custom modded ore blocks
                dictionary[block.BlockId] = new OreInfo
                {
                    BlockId = block.BlockId,
                    Code = block.Code,
                    Grade = "normal",
                    Mineral = path,
                    HostRock = "rock",
                    IsOre = true
                };
            }
        }

        api.Logger.Notification($"[Prospector's Better Instinct] Ore Database ready with {dictionary.Count} registered ore blocks.");
        return dictionary;
    }
}
