using System;

namespace ProspectorsXRay.OreDatabase.Services;

public static class OreCodeParser
{
    public static bool TryParse(string codePath, out string grade, out string mineral, out string hostRock)
    {
        grade = "normal";
        mineral = string.Empty;
        hostRock = string.Empty;

        if (string.IsNullOrWhiteSpace(codePath))
        {
            return false;
        }

        string lower = codePath.ToLowerInvariant();

        // Specific standalone ores
        if (lower == "bogiron")
        {
            mineral = "bogiron";
            return true;
        }

        if (lower == "meteorite-iron" || lower == "meteoric-iron" || lower == "meteoriciron")
        {
            mineral = "meteoriciron";
            return true;
        }

        if (lower.StartsWith("saltpeter-"))
        {
            mineral = "saltpeter";
            return true;
        }

        // Standard pattern: ore-{grade}-{mineral}-{rock} or ore-{mineral}-{rock}
        string[] array = codePath.Split('-');
        if (array.Length == 4 && array[0].Equals("ore", StringComparison.OrdinalIgnoreCase))
        {
            grade = array[1];
            mineral = array[2];
            hostRock = array[3];
            return true;
        }

        if (array.Length == 3 && array[0].Equals("ore", StringComparison.OrdinalIgnoreCase))
        {
            grade = "none";
            mineral = array[1];
            hostRock = array[2];
            return true;
        }

        // Catch other custom ore patterns like "ore-mineral"
        if (array.Length == 2 && array[0].Equals("ore", StringComparison.OrdinalIgnoreCase))
        {
            grade = "none";
            mineral = array[1];
            return true;
        }

        return false;
    }
}
