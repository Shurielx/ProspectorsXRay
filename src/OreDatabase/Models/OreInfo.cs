using ProspectorsXRay.OreDatabase.Services;
using Vintagestory.API.Common;

namespace ProspectorsXRay.OreDatabase.Models;

public sealed class OreInfo
{
    public int BlockId { get; init; }

    public AssetLocation Code { get; init; } = null!;

    public string Mineral { get; init; } = string.Empty;

    public string Grade { get; init; } = string.Empty;

    public string HostRock { get; init; } = string.Empty;

    public bool IsOre { get; init; }

    public string GetCleanName()
    {
        return OreColorHelper.GetReadableMineralName(Mineral);
    }
}
