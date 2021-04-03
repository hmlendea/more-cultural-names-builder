using System;

namespace MoreCulturalNamesModBuilder.Configuration
{
    public sealed class OutputSettings
    {
        public string ModOutputDirectory { get; set; }
        public string ModVersion { get; set; }

        public bool AreVerboseCommentsEnabled { get; set; }

        public string CK2ModId { get; set; }
        public string CK2ModName { get; set; }

        public string CK2HipModId { get; set; }
        public string CK2HipModName { get; set; }

        public string CK3GameVersion { get; set; }
        public string CK3ModId { get; set; }
        public string CK3ModName { get; set; }

        public string HOI4GameVersion { get; set; }
        public string HOI4ModId { get; set; }
        public string HOI4ModName { get; set; }

        public string ImperatorRomeGameVersion { get; set; }
        public string ImperatorRomeModId { get; set; }
        public string ImperatorRomeModName { get; set; }

        public string GetModId(string game)
        {
            switch (game.ToUpperInvariant())
            {
                case "CK2":
                    return CK2ModId;
                case "CK2HIP":
                    return CK2HipModId;
                case "CK3":
                    return CK3ModId;
                case "HOI4":
                    return HOI4ModId;
                case "ImperatorRome":
                    return ImperatorRomeModId;
                default:
                    throw new ArgumentException("Invalid game identifier");
            }
        }
    }
}
