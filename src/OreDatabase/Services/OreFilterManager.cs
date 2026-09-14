using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProspectorsXRay.OreDatabase.Services;

public record OreDefinition(string Key, string DisplayName, string[] Aliases);

public record IndividualOre(string Key, string DisplayName, string GroupKey, string[] Aliases);

public static class OreFilterManager
{
    public static readonly List<OreDefinition> Definitions = new()
    {
        new("copper", "Copper", new[] { "copper", "nativecopper", "malachite", "azurite" }),
        new("tin", "Tin", new[] { "tin", "cassiterite" }),
        new("iron", "Iron", new[] { "iron", "hematite", "limonite", "magnetite", "bogiron", "meteoriciron", "meteoriteiron" }),
        new("gold", "Gold", new[] { "gold", "nativegold", "quartznativegold", "quartz_nativegold" }),
        new("silver", "Silver", new[] { "silver", "nativesilver", "quartznativesilver", "quartz_nativesilver", "galenanativesilver", "galena_nativesilver" }),
        new("lead", "Lead", new[] { "lead", "galena", "cerussite" }),
        new("zinc", "Zinc", new[] { "zinc", "sphalerite", "smithsonite" }),
        new("bismuth", "Bismuth", new[] { "bismuth", "bismuthinite" }),
        new("nickel", "Nickel", new[] { "nickel", "pentlandite" }),
        new("titanium", "Titanium", new[] { "titanium", "ilmenite" }),
        new("tungsten", "Tungsten", new[] { "tungsten", "wolframite" }),
        new("uranium", "Uranium", new[] { "uranium" }),
        new("platinum", "Platinum", new[] { "platinum" }),
        new("coal", "Coal", new[] { "coal", "bituminouscoal", "lignite", "anthracite" }),
        new("quartz", "Quartz", new[] { "quartz" }),
        new("olivine", "Olivine", new[] { "olivine", "peridot", "olivineperidot" }),
        new("sulfur", "Sulfur", new[] { "sulfur", "sulphur" }),
        new("saltpeter", "Saltpeter", new[] { "saltpeter", "saltpetre" }),
        new("borax", "Borax", new[] { "borax" }),
        new("graphite", "Graphite", new[] { "graphite" }),
        new("cinnabar", "Cinnabar", new[] { "cinnabar" }),
        new("fluorite", "Fluorite", new[] { "fluorite" }),
        new("lapis", "Lapis Lazuli", new[] { "lapis", "lapislazuli" }),
        new("diamond", "Diamond", new[] { "diamond" }),
        new("emerald", "Emerald", new[] { "emerald" }),
        new("corundum", "Corundum", new[] { "corundum" }),
        new("chromite", "Chromite", new[] { "chromite", "chromium", "chrom" }),
        new("manganese", "Manganese", new[] { "manganese", "rhodochrosite" }),
        new("alum", "Alum", new[] { "alum" }),
        new("phosphorite", "Phosphorite", new[] { "phosphorite" }),
        new("sylvite", "Sylvite", new[] { "sylvite" }),
        new("kernite", "Kernite", new[] { "kernite" })
    };

    public static readonly List<IndividualOre> IndividualOres = new()
    {
        // Copper
        new("nativecopper", "Native Copper", "copper", new[] { "nativecopper" }),
        new("malachite", "Malachite", "copper", new[] { "malachite" }),
        new("azurite", "Azurite", "copper", new[] { "azurite" }),

        // Tin
        new("cassiterite", "Cassiterite (Tin)", "tin", new[] { "cassiterite" }),

        // Iron
        new("hematite", "Hematite", "iron", new[] { "hematite" }),
        new("limonite", "Limonite", "iron", new[] { "limonite" }),
        new("magnetite", "Magnetite", "iron", new[] { "magnetite" }),
        new("bogiron", "Bog Iron", "iron", new[] { "bogiron", "bog" }),
        new("meteoriciron", "Meteoric Iron", "iron", new[] { "meteoriciron", "meteoriteiron", "meteorite", "meteoric" }),

        // Precious
        new("nativegold", "Native Gold", "gold", new[] { "nativegold" }),
        new("quartznativegold", "Gold in Quartz", "gold", new[] { "quartznativegold", "quartzgold", "goldquartz" }),
        new("nativesilver", "Native Silver", "silver", new[] { "nativesilver" }),
        new("quartznativesilver", "Silver in Quartz", "silver", new[] { "quartznativesilver", "quartzsilver", "silverquartz" }),
        new("galenanativesilver", "Silver in Galena", "silver", new[] { "galenanativesilver", "galenasilver", "silvergalena" }),
        new("platinum", "Platinum", "platinum", new[] { "platinum" }),

        // Base & Alloy Metals
        new("galena", "Galena (Lead)", "lead", new[] { "galena" }),
        new("cerussite", "Cerussite (Lead)", "lead", new[] { "cerussite" }),
        new("sphalerite", "Sphalerite (Zinc)", "zinc", new[] { "sphalerite" }),
        new("smithsonite", "Smithsonite (Zinc)", "zinc", new[] { "smithsonite" }),
        new("bismuthinite", "Bismuthinite", "bismuth", new[] { "bismuthinite" }),
        new("pentlandite", "Pentlandite (Nickel)", "nickel", new[] { "pentlandite" }),
        new("ilmenite", "Ilmenite (Titanium)", "titanium", new[] { "ilmenite" }),
        new("wolframite", "Wolframite (Tungsten)", "tungsten", new[] { "wolframite" }),
        new("uranium", "Uranium", "uranium", new[] { "uranium" }),
        new("chromite", "Chromite", "chromite", new[] { "chromite", "chromium", "chrom" }),
        new("rhodochrosite", "Rhodochrosite (Manganese)", "manganese", new[] { "rhodochrosite" }),

        // Coal & Carbon
        new("bituminouscoal", "Bituminous Coal", "coal", new[] { "bituminouscoal", "bituminous" }),
        new("lignite", "Lignite (Brown Coal)", "coal", new[] { "lignite", "browncoal" }),
        new("anthracite", "Anthracite", "coal", new[] { "anthracite" }),
        new("graphite", "Graphite", "graphite", new[] { "graphite" }),

        // Minerals & Gems
        new("quartz", "Quartz", "quartz", new[] { "quartz" }),
        new("olivine", "Olivine", "olivine", new[] { "olivine" }),
        new("peridot", "Peridot", "olivine", new[] { "peridot" }),
        new("sulfur", "Sulfur", "sulfur", new[] { "sulfur", "sulphur" }),
        new("saltpeter", "Saltpeter", "saltpeter", new[] { "saltpeter", "saltpetre" }),
        new("borax", "Borax", "borax", new[] { "borax" }),
        new("cinnabar", "Cinnabar", "cinnabar", new[] { "cinnabar" }),
        new("fluorite", "Fluorite", "fluorite", new[] { "fluorite" }),
        new("lapislazuli", "Lapis Lazuli", "lapis", new[] { "lapislazuli", "lapis" }),
        new("diamond", "Diamond", "diamond", new[] { "diamond" }),
        new("emerald", "Emerald", "emerald", new[] { "emerald" }),
        new("corundum", "Corundum", "corundum", new[] { "corundum" }),
        new("alum", "Alum", "alum", new[] { "alum" }),
        new("phosphorite", "Phosphorite", "phosphorite", new[] { "phosphorite" }),
        new("sylvite", "Sylvite", "sylvite", new[] { "sylvite" }),
        new("kernite", "Kernite", "kernite", new[] { "kernite" })
    };

    // Meta-group for ores that smelt directly into tool/crafting metal ingots
    public static readonly string[] IngotableGroupKeys = new[]
    {
        "copper", "tin", "iron", "gold", "silver", "lead", "zinc", "bismuth", "nickel"
    };

    public static readonly string[] IngotableAliases = new[]
    {
        "ingotable", "ingotables", "ingot", "ingots", "metal", "metals", "tool", "tools"
    };

    public static readonly string[] Suggestions;
    public static readonly string[] ListSuggestions = new[] { "group", "ore", "all", "ingotable" };

    static OreFilterManager()
    {
        var list = new List<string> { "all", "group", "ore", "ingotable" };
        foreach (var def in Definitions)
        {
            if (!list.Contains(def.Key, StringComparer.OrdinalIgnoreCase))
            {
                list.Add(def.Key);
            }
        }
        foreach (var ore in IndividualOres)
        {
            if (!list.Contains(ore.Key, StringComparer.OrdinalIgnoreCase))
            {
                list.Add(ore.Key);
            }
        }
        Suggestions = list.ToArray();
    }

    public static string Normalize(string input)
    {
        return input.Trim().ToLowerInvariant().Replace(" ", "").Replace("-", "").Replace("_", "");
    }

    public static bool IsIngotableQuery(string term)
    {
        string norm = Normalize(term);
        return IngotableAliases.Any(a => a.Equals(norm, StringComparison.OrdinalIgnoreCase));
    }

    public static List<IndividualOre> GetIngotableOres()
    {
        return IndividualOres.Where(o => IngotableGroupKeys.Contains(o.GroupKey, StringComparer.OrdinalIgnoreCase)).ToList();
    }

    public static bool IsOreDisabled(string mineral, HashSet<string> disabledOres)
    {
        if (disabledOres == null || disabledOres.Count == 0) return false;
        if (disabledOres.Contains("all")) return true;

        string m = Normalize(mineral);

        // 1. Direct match with disabled individual ore key
        if (disabledOres.Contains(m)) return true;

        // 2. Find individual ore definition
        var indOre = FindIndividualOre(m);
        if (indOre != null)
        {
            if (disabledOres.Contains(indOre.Key)) return true;
            if (disabledOres.Contains(indOre.GroupKey)) return true;
        }

        // 3. Find group definition
        var def = FindDefinition(m);
        if (def != null)
        {
            if (disabledOres.Contains(def.Key)) return true;
        }

        // 4. Fallback for custom or compound ore names
        foreach (var disabled in disabledOres)
        {
            if (m.Equals(disabled, StringComparison.OrdinalIgnoreCase) ||
                m.Contains(disabled, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static OreDefinition? FindDefinition(string term)
    {
        string norm = Normalize(term);
        var exact = Definitions.FirstOrDefault(d =>
            d.Key.Equals(norm, StringComparison.OrdinalIgnoreCase) ||
            d.Aliases.Any(a => Normalize(a).Equals(norm, StringComparison.OrdinalIgnoreCase)));
        if (exact != null) return exact;

        return Definitions.FirstOrDefault(d =>
            d.Key.StartsWith(norm, StringComparison.OrdinalIgnoreCase) ||
            Normalize(d.DisplayName).StartsWith(norm, StringComparison.OrdinalIgnoreCase) ||
            d.Aliases.Any(a => Normalize(a).StartsWith(norm, StringComparison.OrdinalIgnoreCase)));
    }

    public static IndividualOre? FindIndividualOre(string term)
    {
        string norm = Normalize(term);
        var exact = IndividualOres.FirstOrDefault(o =>
            o.Key.Equals(norm, StringComparison.OrdinalIgnoreCase) ||
            Normalize(o.DisplayName).Equals(norm, StringComparison.OrdinalIgnoreCase) ||
            o.Aliases.Any(a => Normalize(a).Equals(norm, StringComparison.OrdinalIgnoreCase)));
        if (exact != null) return exact;

        return IndividualOres.FirstOrDefault(o =>
            o.Key.StartsWith(norm, StringComparison.OrdinalIgnoreCase) ||
            Normalize(o.DisplayName).StartsWith(norm, StringComparison.OrdinalIgnoreCase) ||
            o.Aliases.Any(a => Normalize(a).StartsWith(norm, StringComparison.OrdinalIgnoreCase)));
    }

    // Levenshtein distance for fuzzy matching typos (e.g. chromtie -> chromite, magnatite -> magnetite)
    public static int ComputeLevenshtein(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) return b?.Length ?? 0;
        if (string.IsNullOrEmpty(b)) return a.Length;

        int[,] d = new int[a.Length + 1, b.Length + 1];
        for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) d[0, j] = j;

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[a.Length, b.Length];
    }

    public static (string ResolvedType, object? Match, string Note) ResolveOreQuery(string input)
    {
        string norm = Normalize(input);

        if (norm == "all") return ("all", null, "");
        if (IsIngotableQuery(norm)) return ("ingotable", null, "");

        // 1. Exact or prefix match on individual ore
        var ind = FindIndividualOre(norm);
        if (ind != null && !Definitions.Any(d => d.Key.Equals(norm, StringComparison.OrdinalIgnoreCase)))
        {
            return ("individual", ind, "");
        }

        // 2. Exact or prefix match on group definition
        var def = FindDefinition(norm);
        if (def != null)
        {
            return ("group", def, "");
        }

        // 3. Fuzzy search among individual ores
        IndividualOre? closestInd = null;
        int bestIndDist = int.MaxValue;
        foreach (var o in IndividualOres)
        {
            int d1 = ComputeLevenshtein(norm, o.Key);
            int d2 = ComputeLevenshtein(norm, Normalize(o.DisplayName));
            int best = Math.Min(d1, d2);
            foreach (var alias in o.Aliases)
            {
                best = Math.Min(best, ComputeLevenshtein(norm, Normalize(alias)));
            }

            if (best < bestIndDist)
            {
                bestIndDist = best;
                closestInd = o;
            }
        }

        // 4. Fuzzy search among group definitions
        OreDefinition? closestDef = null;
        int bestDefDist = int.MaxValue;
        foreach (var d in Definitions)
        {
            int d1 = ComputeLevenshtein(norm, d.Key);
            int d2 = ComputeLevenshtein(norm, Normalize(d.DisplayName));
            int best = Math.Min(d1, d2);
            foreach (var alias in d.Aliases)
            {
                best = Math.Min(best, ComputeLevenshtein(norm, Normalize(alias)));
            }

            if (best < bestDefDist)
            {
                bestDefDist = best;
                closestDef = d;
            }
        }

        if (bestIndDist <= 2 && bestIndDist <= bestDefDist && closestInd != null)
        {
            return ("individual", closestInd, $" (auto-corrected from '{input}')");
        }

        if (bestDefDist <= 2 && closestDef != null)
        {
            return ("group", closestDef, $" (auto-corrected from '{input}')");
        }

        return ("unknown", null, "");
    }

    public static (bool Success, string Message) DisableOre(string input, HashSet<string> disabledOres)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return (false, "Usage: /ore hide [name] or /ore hide all (e.g. /ore hide copper, /ore hide magnetite). Use '/ore list' to see all ores.");
        }

        var (type, match, note) = ResolveOreQuery(input);

        if (type == "all")
        {
            disabledOres.Add("all");
            foreach (var d in Definitions)
            {
                disabledOres.Add(d.Key);
            }
            foreach (var o in IndividualOres)
            {
                disabledOres.Add(o.Key);
            }
            return (true, "All ores are now <font color=\"#ff5555\">[DISABLED]</font> (hidden from X-Ray).");
        }

        if (type == "ingotable")
        {
            foreach (var gk in IngotableGroupKeys)
            {
                disabledOres.Add(gk);
            }
            foreach (var ore in GetIngotableOres())
            {
                disabledOres.Add(ore.Key);
            }
            return (true, "All ingot-producing metals (Copper, Tin, Iron, Gold, Silver, Lead, Zinc, Bismuth, Nickel) are now <font color=\"#ff5555\">[DISABLED]</font>.");
        }

        if (type == "individual" && match is IndividualOre indOre)
        {
            disabledOres.Add(indOre.Key);
            return (true, $"'{indOre.DisplayName}'{note} is now <font color=\"#ff5555\">[DISABLED]</font> (hidden from X-Ray).");
        }

        if (type == "group" && match is OreDefinition groupDef)
        {
            disabledOres.Add(groupDef.Key);
            foreach (var o in IndividualOres.Where(x => x.GroupKey == groupDef.Key))
            {
                disabledOres.Add(o.Key);
            }
            return (true, $"Group '{groupDef.DisplayName}'{note} is now <font color=\"#ff5555\">[DISABLED]</font> (hidden from X-Ray).");
        }

        return (false, $"Unknown ore '{input}'. Type '/ore list' to view all available ores and groups.");
    }

    public static (bool Success, string Message) EnableOre(string input, HashSet<string> disabledOres)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return (false, "Usage: /ore show [name] or /ore show all (e.g. /ore show copper, /ore show magnetite). Use '/ore list' to see all ores.");
        }

        var (type, match, note) = ResolveOreQuery(input);

        if (type == "all")
        {
            disabledOres.Clear();
            return (true, "All ores are now <font color=\"#55ff55\">[ENABLED]</font> (visible on X-Ray).");
        }

        if (type == "ingotable")
        {
            disabledOres.Remove("all");
            foreach (var gk in IngotableGroupKeys)
            {
                disabledOres.Remove(gk);
            }
            foreach (var ore in GetIngotableOres())
            {
                disabledOres.Remove(ore.Key);
            }
            return (true, "All ingot-producing metals (Copper, Tin, Iron, Gold, Silver, Lead, Zinc, Bismuth, Nickel) are now <font color=\"#55ff55\">[ENABLED]</font>.");
        }

        if (type == "individual" && match is IndividualOre indOre)
        {
            // If "all" was disabled, expand all known ores as disabled first, then enable only indOre
            if (disabledOres.Contains("all"))
            {
                disabledOres.Remove("all");
                foreach (var o in IndividualOres)
                {
                    disabledOres.Add(o.Key);
                }
            }

            // Remove indOre
            disabledOres.Remove(indOre.Key);

            // If the parent group was disabled, remove group key and ensure sibling ores remain explicitly disabled
            if (disabledOres.Contains(indOre.GroupKey))
            {
                disabledOres.Remove(indOre.GroupKey);
                foreach (var sibling in IndividualOres.Where(x => x.GroupKey == indOre.GroupKey && x.Key != indOre.Key))
                {
                    disabledOres.Add(sibling.Key);
                }
            }

            return (true, $"'{indOre.DisplayName}'{note} is now <font color=\"#55ff55\">[ENABLED]</font> (visible on X-Ray).");
        }

        if (type == "group" && match is OreDefinition def)
        {
            if (disabledOres.Contains("all"))
            {
                disabledOres.Remove("all");
                foreach (var o in IndividualOres)
                {
                    disabledOres.Add(o.Key);
                }
            }

            disabledOres.Remove(def.Key);
            foreach (var o in IndividualOres.Where(x => x.GroupKey == def.Key))
            {
                disabledOres.Remove(o.Key);
            }

            return (true, $"Group '{def.DisplayName}'{note} is now <font color=\"#55ff55\">[ENABLED]</font> (visible on X-Ray).");
        }

        return (false, $"Unknown ore '{input}'. Type '/ore list' to view all available ores and groups.");
    }

    public static (bool Success, string Message) ShowOnly(string input, HashSet<string> disabledOres)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return (false, "Usage: /ore showonly [name] (e.g. /ore showonly copper, /ore showonly ingotable). Use '/ore list' to see all ores.");
        }

        var (type, match, note) = ResolveOreQuery(input);

        if (type == "ingotable")
        {
            disabledOres.Clear();
            var ingotKeys = new HashSet<string>(GetIngotableOres().Select(o => o.Key), StringComparer.OrdinalIgnoreCase);
            foreach (var ore in IndividualOres)
            {
                if (!ingotKeys.Contains(ore.Key))
                {
                    disabledOres.Add(ore.Key);
                }
            }
            foreach (var def in Definitions)
            {
                if (!IngotableGroupKeys.Contains(def.Key, StringComparer.OrdinalIgnoreCase))
                {
                    disabledOres.Add(def.Key);
                }
            }
            return (true, "Now showing ONLY <font color=\"#55ff55\">Ingot-Producing Metals</font> (all other ores hidden).");
        }

        if (type == "individual" && match is IndividualOre indOre)
        {
            disabledOres.Clear();
            foreach (var ore in IndividualOres)
            {
                if (ore.Key != indOre.Key)
                {
                    disabledOres.Add(ore.Key);
                }
            }
            foreach (var def in Definitions)
            {
                if (def.Key != indOre.GroupKey)
                {
                    disabledOres.Add(def.Key);
                }
            }
            return (true, $"Now showing ONLY <font color=\"#55ff55\">'{indOre.DisplayName}'</font>{note} (all other ores hidden).");
        }

        if (type == "group" && match is OreDefinition groupDef)
        {
            disabledOres.Clear();
            foreach (var d in Definitions)
            {
                if (!d.Key.Equals(groupDef.Key, StringComparison.OrdinalIgnoreCase))
                {
                    disabledOres.Add(d.Key);
                }
            }
            foreach (var ore in IndividualOres)
            {
                if (!ore.GroupKey.Equals(groupDef.Key, StringComparison.OrdinalIgnoreCase))
                {
                    disabledOres.Add(ore.Key);
                }
            }
            return (true, $"Now showing ONLY group <font color=\"#55ff55\">'{groupDef.DisplayName}'</font>{note} (all other ores hidden).");
        }

        return (false, $"Unknown ore '{input}'. Type '/ore list' to view all available ores.");
    }

    public static (bool Success, string Message) HideOnly(string input, HashSet<string> disabledOres)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return (false, "Usage: /ore hideonly [name] (e.g. /ore hideonly copper, /ore hideonly ingotable). Use '/ore list' to see all ores.");
        }

        var (type, match, note) = ResolveOreQuery(input);

        if (type == "ingotable")
        {
            disabledOres.Clear();
            foreach (var gk in IngotableGroupKeys)
            {
                disabledOres.Add(gk);
            }
            foreach (var ore in GetIngotableOres())
            {
                disabledOres.Add(ore.Key);
            }
            return (true, "Now hiding ONLY <font color=\"#ff5555\">Ingot-Producing Metals</font> (all other ores visible).");
        }

        if (type == "individual" && match is IndividualOre indOre)
        {
            disabledOres.Clear();
            disabledOres.Add(indOre.Key);
            return (true, $"Now hiding ONLY <font color=\"#ff5555\">'{indOre.DisplayName}'</font>{note} (all other ores visible).");
        }

        if (type == "group" && match is OreDefinition groupDef)
        {
            disabledOres.Clear();
            disabledOres.Add(groupDef.Key);
            foreach (var o in IndividualOres.Where(x => x.GroupKey == groupDef.Key))
            {
                disabledOres.Add(o.Key);
            }
            return (true, $"Now hiding ONLY group <font color=\"#ff5555\">'{groupDef.DisplayName}'</font>{note} (all other ores visible).");
        }

        return (false, $"Unknown ore '{input}'. Type '/ore list' to view all available ores.");
    }

    public static string GetListText(HashSet<string> disabledOres)
    {
        return GetFullListText(disabledOres);
    }

    public static string GetGroupsListText(HashSet<string> disabledOres)
    {
        StringBuilder sb = new();
        sb.AppendLine("<font color=\"#ffdf55\"><strong>[Prospector's Better Instinct] Ore Groups:</strong></font>");

        for (int i = 0; i < Definitions.Count; i += 2)
        {
            var def1 = Definitions[i];
            string status1 = GetGroupStatusTag(def1.Key, disabledOres);
            string col1 = $" • {def1.DisplayName}: {status1}";

            if (i + 1 < Definitions.Count)
            {
                var def2 = Definitions[i + 1];
                string status2 = GetGroupStatusTag(def2.Key, disabledOres);
                string col2 = $" • {def2.DisplayName}: {status2}";
                sb.AppendLine($"{col1,-38} |  {col2}");
            }
            else
            {
                sb.AppendLine(col1);
            }
        }

        sb.AppendLine("<font color=\"#aaaaaa\">Commands: /ore show [name] | /ore hide [name] | /ore showonly [name] | /ore show ingotable</font>");
        return sb.ToString().TrimEnd();
    }

    public static string GetIndividualOresListText(HashSet<string> disabledOres)
    {
        StringBuilder sb = new();
        sb.AppendLine("<font color=\"#ffdf55\"><strong>[Prospector's Better Instinct] Individual Ore Variants:</strong></font>");

        for (int i = 0; i < IndividualOres.Count; i += 2)
        {
            var ore1 = IndividualOres[i];
            bool dis1 = IsOreDisabled(ore1.Key, disabledOres);
            string status1 = dis1 ? "<font color=\"#ff5555\">[DISABLED]</font>" : "<font color=\"#55ff55\">[ENABLED]</font>";
            string col1 = $" • {ore1.DisplayName}: {status1}";

            if (i + 1 < IndividualOres.Count)
            {
                var ore2 = IndividualOres[i + 1];
                bool dis2 = IsOreDisabled(ore2.Key, disabledOres);
                string status2 = dis2 ? "<font color=\"#ff5555\">[DISABLED]</font>" : "<font color=\"#55ff55\">[ENABLED]</font>";
                string col2 = $" • {ore2.DisplayName}: {status2}";
                sb.AppendLine($"{col1,-40} |  {col2}");
            }
            else
            {
                sb.AppendLine(col1);
            }
        }

        sb.AppendLine("<font color=\"#aaaaaa\">Commands: /ore show [ore] | /ore hide [ore] | /ore showonly [ore]</font>");
        return sb.ToString().TrimEnd();
    }

    public static string GetFullListText(HashSet<string> disabledOres)
    {
        StringBuilder sb = new();
        sb.AppendLine("<font color=\"#ffdf55\"><strong>[Prospector's Better Instinct] Ore Groups & Variants:</strong></font>");

        for (int i = 0; i < Definitions.Count; i++)
        {
            var def = Definitions[i];
            var variants = IndividualOres.Where(o => o.GroupKey == def.Key).ToList();
            string groupStatus = GetGroupStatusTag(def.Key, disabledOres);

            if (variants.Count <= 1)
            {
                sb.AppendLine($" • <font color=\"#ffffff\">{def.DisplayName}</font>: {groupStatus}");
            }
            else
            {
                int enabledCount = variants.Count(v => !IsOreDisabled(v.Key, disabledOres));
                string detail = string.Join(", ", variants.Select(v =>
                {
                    bool isOff = IsOreDisabled(v.Key, disabledOres);
                    string color = isOff ? "#ff7777" : "#77ff77";
                    return $"<font color=\"{color}\">{v.DisplayName}</font>";
                }));

                sb.AppendLine($" • <font color=\"#ffffff\">{def.DisplayName}</font> ({enabledCount}/{variants.Count}): {groupStatus} [{detail}]");
            }
        }

        sb.AppendLine("<font color=\"#55ff55\">Quick filters: /ore show ingotable | /ore list group | /ore list ore</font>");
        return sb.ToString().TrimEnd();
    }

    private static string GetGroupStatusTag(string groupKey, HashSet<string> disabledOres)
    {
        var variants = IndividualOres.Where(o => o.GroupKey == groupKey).ToList();
        if (variants.Count == 0)
        {
            bool dis = IsOreDisabled(groupKey, disabledOres);
            return dis ? "<font color=\"#ff5555\">[DISABLED]</font>" : "<font color=\"#55ff55\">[ENABLED]</font>";
        }

        int enabledCount = variants.Count(v => !IsOreDisabled(v.Key, disabledOres));
        if (enabledCount == variants.Count)
        {
            return "<font color=\"#55ff55\">[ENABLED]</font>";
        }
        if (enabledCount == 0)
        {
            return "<font color=\"#ff5555\">[DISABLED]</font>";
        }
        return $"<font color=\"#ffdf55\">[PARTIAL {enabledCount}/{variants.Count}]</font>";
    }
}
