using System;
using System.Collections.Generic;
using Vintagestory.API.MathTools;

namespace ProspectorsXRay.OreDatabase.Services;

public static class OreColorHelper
{
    private static readonly Dictionary<string, (string DisplayName, int Color)> MineralData = new(StringComparer.OrdinalIgnoreCase)
    {
        // Copper
        ["nativecopper"] = ("Native Copper", ColorUtil.ToRgba(190, 224, 106, 59)),
        ["copper"] = ("Copper", ColorUtil.ToRgba(190, 224, 106, 59)),
        ["malachite"] = ("Malachite", ColorUtil.ToRgba(190, 30, 184, 118)),
        ["azurite"] = ("Azurite", ColorUtil.ToRgba(190, 16, 112, 224)),
        
        // Iron & Meteorite
        ["hematite"] = ("Hematite (Iron)", ColorUtil.ToRgba(190, 156, 40, 40)),
        ["limonite"] = ("Limonite (Iron)", ColorUtil.ToRgba(190, 194, 144, 43)),
        ["magnetite"] = ("Magnetite (Iron)", ColorUtil.ToRgba(190, 74, 74, 82)),
        ["bogiron"] = ("Bog Iron", ColorUtil.ToRgba(190, 122, 75, 41)),
        ["meteoriciron"] = ("Meteoric Iron", ColorUtil.ToRgba(190, 155, 89, 182)),
        ["meteoriteiron"] = ("Meteoric Iron", ColorUtil.ToRgba(190, 155, 89, 182)),
        
        // Tin & Precious Metals
        ["cassiterite"] = ("Cassiterite (Tin)", ColorUtil.ToRgba(190, 158, 181, 199)),
        ["tin"] = ("Tin", ColorUtil.ToRgba(190, 158, 181, 199)),
        ["nativegold"] = ("Native Gold", ColorUtil.ToRgba(190, 255, 215, 0)),
        ["gold"] = ("Gold", ColorUtil.ToRgba(190, 255, 215, 0)),
        ["quartznativegold"] = ("Gold in Quartz", ColorUtil.ToRgba(190, 255, 215, 0)),
        ["quartz_nativegold"] = ("Gold in Quartz", ColorUtil.ToRgba(190, 255, 215, 0)),
        ["nativesilver"] = ("Native Silver", ColorUtil.ToRgba(190, 232, 240, 248)),
        ["silver"] = ("Silver", ColorUtil.ToRgba(190, 232, 240, 248)),
        ["quartznativesilver"] = ("Silver in Quartz", ColorUtil.ToRgba(190, 232, 240, 248)),
        ["quartz_nativesilver"] = ("Silver in Quartz", ColorUtil.ToRgba(190, 232, 240, 248)),
        ["galenanativesilver"] = ("Silver in Galena", ColorUtil.ToRgba(190, 232, 240, 248)),
        ["galena_nativesilver"] = ("Silver in Galena", ColorUtil.ToRgba(190, 232, 240, 248)),
        ["platinum"] = ("Platinum", ColorUtil.ToRgba(190, 208, 229, 245)),
        
        // Zinc, Lead, Bismuth, Nickel
        ["sphalerite"] = ("Sphalerite (Zinc)", ColorUtil.ToRgba(190, 168, 134, 50)),
        ["smithsonite"] = ("Smithsonite (Zinc)", ColorUtil.ToRgba(190, 62, 176, 192)),
        ["galena"] = ("Galena (Lead)", ColorUtil.ToRgba(190, 104, 110, 128)),
        ["cerussite"] = ("Cerussite (Lead)", ColorUtil.ToRgba(190, 214, 206, 170)),
        ["bismuthinite"] = ("Bismuthinite", ColorUtil.ToRgba(190, 217, 136, 176)),
        ["bismuth"] = ("Bismuth", ColorUtil.ToRgba(190, 217, 136, 176)),
        ["pentlandite"] = ("Pentlandite (Nickel)", ColorUtil.ToRgba(190, 143, 133, 82)),
        ["nickel"] = ("Nickel", ColorUtil.ToRgba(190, 143, 133, 82)),
        
        // Technical & Rare Minerals
        ["chromite"] = ("Chromite", ColorUtil.ToRgba(190, 59, 56, 56)),
        ["chromium"] = ("Chromite", ColorUtil.ToRgba(190, 59, 56, 56)),
        ["ilmenite"] = ("Ilmenite (Titanium)", ColorUtil.ToRgba(190, 107, 91, 115)),
        ["titanium"] = ("Titanium", ColorUtil.ToRgba(190, 107, 91, 115)),
        ["wolframite"] = ("Wolframite (Tungsten)", ColorUtil.ToRgba(190, 69, 50, 41)),
        ["tungsten"] = ("Tungsten", ColorUtil.ToRgba(190, 69, 50, 41)),
        ["rhodochrosite"] = ("Rhodochrosite (Manganese)", ColorUtil.ToRgba(190, 232, 77, 138)),
        ["manganese"] = ("Manganese", ColorUtil.ToRgba(190, 232, 77, 138)),
        ["uranium"] = ("Uranium", ColorUtil.ToRgba(190, 57, 255, 20)),
        
        // Minerals & Non-metals
        ["cinnabar"] = ("Cinnabar", ColorUtil.ToRgba(190, 230, 34, 34)),
        ["sulfur"] = ("Sulfur", ColorUtil.ToRgba(190, 255, 255, 36)),
        ["sulphur"] = ("Sulfur", ColorUtil.ToRgba(190, 255, 255, 36)),
        ["saltpeter"] = ("Saltpeter", ColorUtil.ToRgba(190, 240, 235, 225)),
        ["saltpetre"] = ("Saltpeter", ColorUtil.ToRgba(190, 240, 235, 225)),
        ["borax"] = ("Borax", ColorUtil.ToRgba(190, 237, 232, 208)),
        ["fluorite"] = ("Fluorite", ColorUtil.ToRgba(190, 160, 64, 208)),
        ["sylvite"] = ("Sylvite", ColorUtil.ToRgba(190, 232, 125, 101)),
        ["phosphorite"] = ("Phosphorite", ColorUtil.ToRgba(190, 138, 154, 101)),
        ["alum"] = ("Alum", ColorUtil.ToRgba(190, 226, 238, 242)),
        ["kernite"] = ("Kernite", ColorUtil.ToRgba(190, 250, 242, 230)),
        ["graphite"] = ("Graphite", ColorUtil.ToRgba(190, 92, 92, 92)),
        
        // Carbon & Gems
        ["coal"] = ("Bituminous Coal", ColorUtil.ToRgba(190, 36, 36, 36)),
        ["bituminouscoal"] = ("Bituminous Coal", ColorUtil.ToRgba(190, 36, 36, 36)),
        ["lignite"] = ("Lignite (Brown Coal)", ColorUtil.ToRgba(190, 84, 61, 43)),
        ["anthracite"] = ("Anthracite", ColorUtil.ToRgba(190, 26, 26, 26)),
        ["quartz"] = ("Quartz", ColorUtil.ToRgba(190, 255, 255, 255)),
        ["olivine"] = ("Olivine (Peridot)", ColorUtil.ToRgba(190, 153, 184, 60)),
        ["peridot"] = ("Peridot", ColorUtil.ToRgba(190, 153, 184, 60)),
        ["olivineperidot"] = ("Peridot", ColorUtil.ToRgba(190, 153, 184, 60)),
        ["lapislazuli"] = ("Lapis Lazuli", ColorUtil.ToRgba(190, 31, 71, 136)),
        ["lapis"] = ("Lapis Lazuli", ColorUtil.ToRgba(190, 31, 71, 136)),
        ["diamond"] = ("Diamond", ColorUtil.ToRgba(190, 125, 249, 255)),
        ["emerald"] = ("Emerald", ColorUtil.ToRgba(190, 0, 201, 87)),
        ["corundum"] = ("Corundum", ColorUtil.ToRgba(190, 224, 17, 95))
    };

    public static int DefaultColor => ColorUtil.ToRgba(190, 255, 190, 60);

    public static string GetReadableMineralName(string mineral)
    {
        if (string.IsNullOrWhiteSpace(mineral)) return "Unknown Ore";

        string key = mineral.Replace(" ", "").Replace("-", "").Replace("_", "").ToLowerInvariant();
        if (MineralData.TryGetValue(key, out var data))
        {
            return data.DisplayName;
        }

        return char.ToUpperInvariant(mineral[0]) + mineral[1..];
    }

    public static int GetColorForMineral(string mineral)
    {
        if (string.IsNullOrWhiteSpace(mineral)) return DefaultColor;

        string key = mineral.Replace(" ", "").Replace("-", "").Replace("_", "").ToLowerInvariant();
        if (MineralData.TryGetValue(key, out var data))
        {
            return data.Color;
        }

        return DefaultColor;
    }
}
