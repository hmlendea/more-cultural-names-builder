using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NuciDAL.Repositories;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Mapping;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders.CrusaderKings2
{
    public sealed class CK3ModBuilder : ModBuilder, ICK2ModBuilder
    {
        public override string Game => "CK3";

        const string LandedTitlesFileName = "0_MoreCulturalNames.txt";
        const int SpacesPerIdentationLevel = 4;

        public CK3ModBuilder(
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            OutputSettings outputSettings)
            : base(languageRepository, locationRepository, outputSettings)
        {
        }

        public override void Build()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, outputSettings.CK2HipModId);
            string commonDirectoryPath = Path.Combine(mainDirectoryPath, "common");
            string landedTitlesDirectoryPath = Path.Combine(commonDirectoryPath, "landed_titles");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(landedTitlesDirectoryPath);

            Directory.CreateDirectory(commonDirectoryPath);

            CreateDataFiles(landedTitlesDirectoryPath);
            CreateDescriptorFiles();
        }

        void CreateDataFiles(string landedTitlesDirectoryPath)
        {
            List<Localisation> localisations = GetLocalisations();

            Dictionary<string, List<Localisation>> localisationsByLocation = localisations
                .GroupBy(x => x.LocationId)
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.ToList());

            IEnumerable<Location> locations = locationRepository.GetAll().ToServiceModels();
            
            string content = GetContentRecursively(locations);
            WriteLandedTitlesFile(content, landedTitlesDirectoryPath);
        }

        void CreateDescriptorFiles()
        {
            string fileContent = GenerateDescriptorFileContent();

            string descriptorFile1Path = Path.Combine(OutputDirectoryPath, $"{outputSettings.CK2HipModId}.mod");
            string descriptorFile2Path = Path.Combine(OutputDirectoryPath, outputSettings.CK2HipModId, "descriptor.mod");

            File.WriteAllText(descriptorFile1Path, fileContent);
            File.WriteAllText(descriptorFile2Path, fileContent);
        }

        void WriteLandedTitlesFile(string content, string landedTitlesDirectoryPath)
        {
            string filePath = Path.Combine(landedTitlesDirectoryPath, LandedTitlesFileName);

            File.WriteAllText(filePath, content);
        }

        IEnumerable<GameId> GetChildrenIds(IEnumerable<Location> locations, string gameId)
        {
            IEnumerable<GameId> childrenGameIds = locations
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Game && x.ParentId == gameId)
                .OrderBy(x => x.Order);
            
            return childrenGameIds;
        }

        string GetContentRecursively(IEnumerable<Location> locations)
        {
            return
                GetContentRecursively(locations, null) +
                GetContentRecursively(locations, string.Empty);
        }

        string GetContentRecursively(IEnumerable<Location> locations, string gameId)
        {
            string content = string.Empty;
            IEnumerable<GameId> childrenIds = GetChildrenIds(locations, gameId);

            foreach (GameId childGameId in childrenIds)
            {
                IList<Localisation> localisations = GetGameLocationLocalisations(childGameId.Id)
                    .OrderBy(x => x.LanguageId)
                    .ToList();

                string indentation1 = GetIndentation(childGameId);
                string indentation2 = indentation1 + string.Empty.PadRight(SpacesPerIdentationLevel);
                string thisContent = string.Empty;

                content += $"{indentation1}{childGameId.Id} = {{" + Environment.NewLine;

                foreach (Localisation localisation in localisations)
                {
                    content += $"{indentation2}{localisation.LanguageId} = \"{localisation.Name}\"" + Environment.NewLine;
                }

                string childContent = GetContentRecursively(locations, childGameId.Id);

                if (!string.IsNullOrWhiteSpace(childContent))
                {
                    if (localisations.Count > 0)
                    {
                        content += Environment.NewLine;
                    }
                    
                    content += childContent;
                }

                content += $"{indentation1}}}" + Environment.NewLine;
            }

            return content;
        }

        string GetIndentation(GameId gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId.ParentId))
            {
                return string.Empty;
            }
            
            return GetIndentation(gameId.Id);
        }

        string GenerateDescriptorFileContent()
        {
            return
                $"name = \"{outputSettings.CK2HipModName}\"" + Environment.NewLine +
                $"path = \"mod/{outputSettings.CK2HipModId}\"" + Environment.NewLine +
                $"dependencies = {{ \"HIP - Historical Immersion Project\" }}" + Environment.NewLine +
                $"tags = {{ map immersion HIP }}\"" + Environment.NewLine +
                $"picture = \"mcn.png\"";
        }

        string GetIndentation(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                return string.Empty;
            }

            switch (gameId[0])
            {
                default:
                case 'e':
                case 'E':
                    return string.Empty.PadRight(SpacesPerIdentationLevel * 0);
                case 'k':
                case 'K':
                    return string.Empty.PadRight(SpacesPerIdentationLevel * 1);
                case 'd':
                case 'D':
                    return string.Empty.PadRight(SpacesPerIdentationLevel * 2);
                case 'c':
                case 'C':
                    return string.Empty.PadRight(SpacesPerIdentationLevel * 3);
                case 'b':
                case 'B':
                    return string.Empty.PadRight(SpacesPerIdentationLevel * 4);
            }
        }
    }
}
