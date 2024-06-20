using System;
using System.IO;

using MoreCulturalNamesBuilder.Configuration;

namespace MoreCulturalNamesBuilder.Service.ModBuilders.ImperatorRome
{
    public sealed class ImperatorRomeDescriptorBuilder(Settings settings) : IImperatorRomeDescriptorBuilder
    {
        public void CreateDescriptorFiles(string outputDirectoryPath)
        {
            string mainDirectoryPath = Path.Combine(outputDirectoryPath, settings.Mod.Id);

            Directory.CreateDirectory(mainDirectoryPath);

            string mainDescriptorContent = GenerateMainDescriptorContent();
            string innerDescriptorContent = GenerateInnerDescriptorContent();

            string mainDescriptorFilePath = Path.Combine(outputDirectoryPath, $"{settings.Mod.Id}.mod");
            string innerDescriptorFilePath = Path.Combine(mainDirectoryPath, $"descriptor.mod");

            File.WriteAllText(mainDescriptorFilePath, mainDescriptorContent);
            File.WriteAllText(innerDescriptorFilePath, innerDescriptorContent);
        }

        string GenerateMainDescriptorContent()
            => GenerateInnerDescriptorContent() +
                Environment.NewLine +
                $"path=\"mod/{settings.Mod.Id}\"";

        string GenerateInnerDescriptorContent()
            =>  $"# Version {settings.Mod.Version} ({DateTime.Now})" + Environment.NewLine +
                $"name=\"{settings.Mod.Name}\"" + Environment.NewLine +
                $"version=\"{settings.Mod.Version}\"" + Environment.NewLine +
                $"supported_version=\"{settings.Mod.GameVersion}\"" + Environment.NewLine +
                $"tags={{" + Environment.NewLine +
                $"    \"Historical\"" + Environment.NewLine +
                $"}}";
    }
}
