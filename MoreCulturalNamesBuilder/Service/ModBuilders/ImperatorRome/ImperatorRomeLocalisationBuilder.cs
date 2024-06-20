using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.ModBuilders.ImperatorRome
{
    public sealed class ImperatorRomeLocalisationBuilder(
        INameNormaliser nameNormaliser,
        Settings settings) : IImperatorRomeLocalisationBuilder
    {
        public void CreateLocalisationFiles(
            string outputDirectoryPath,
            IDictionary<string, IDictionary<string, Localisation>> localisations,
            IEnumerable<GameId> locationGameIds)
        {
            string localisationDirectoryPath = Path.Combine(outputDirectoryPath, settings.Mod.Id, "localization");
            string content = GenerateLocalisationFileContent(localisations, locationGameIds);

            Parallel.ForEach(
                new List<string>{ "english", "french", "german", "spanish" },
                fileLanguage => CreateLocalisationFile(localisationDirectoryPath, fileLanguage, content));
        }

        void CreateLocalisationFile(string localisationDirectoryPath, string language, string content)
        {
            string fileContent = $"l_{language}:{Environment.NewLine}{content}";
            string fileName = $"{settings.Mod.Id}_provincenames_l_{language}.yml";
            string filePath = Path.Combine(localisationDirectoryPath, fileName);

            File.WriteAllText(filePath, fileContent, Encoding.UTF8);
        }

        string GenerateLocalisationFileContent(
            IDictionary<string, IDictionary<string, Localisation>> localisations,
            IEnumerable<GameId> locationGameIds)
        {
            ConcurrentBag<string> lines = [];

            Parallel.ForEach(localisations.Keys, provinceId =>
            {
                GameId gameId = locationGameIds.First(x => x.Id.Equals(provinceId));

                IDictionary<string, Localisation> provinceLocalisations = localisations[provinceId];
                Localisation defaultLocalisation = provinceLocalisations.Values
                    .FirstOrDefault(x => x.LanguageId.Equals(gameId.DefaultNameLanguageId));

                if (defaultLocalisation is not null)
                {
                    string provinceDefaultLocalisationDefinition = GenerateLocationLocalisationLine(
                        defaultLocalisation,
                        $"PROV{provinceId}");

                    lines.Add(provinceDefaultLocalisationDefinition);
                }

                foreach (string culture in provinceLocalisations.Keys.OrderBy(x => x))
                {
                    Localisation localisation = provinceLocalisations[culture];

                    string provinceCulturalLocalisationDefinition = GenerateLocationLocalisationLine(
                        localisation,
                        $"PROV{provinceId}_{localisation.LanguageGameId}");

                    lines.Add(provinceCulturalLocalisationDefinition);
                }
            });

            return string.Join(
                Environment.NewLine,
                lines.OrderBy(line => line));
        }

        string GenerateLocationLocalisationLine(Localisation localisation, string localisationKey)
        {
            string provinceLocalisationDefinition =
                $" {localisationKey}:0 " +
                $"\"{nameNormaliser.ToImperatorRomeCharset(localisation.Name)}\"";

            if (settings.Output.AreVerboseCommentsEnabled)
            {
                provinceLocalisationDefinition += $" # Language={localisation.LanguageId}";
            }

            if (!string.IsNullOrWhiteSpace(localisation.Comment))
            {
                provinceLocalisationDefinition += $" # {localisation.Comment}";
            }

            return provinceLocalisationDefinition;
        }
    }
}
