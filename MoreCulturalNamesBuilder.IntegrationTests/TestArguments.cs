using System;
using System.Collections.Generic;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    internal static class TestArguments
    {
        internal static string[] Build(string outputDirectoryPath, ModBuildRequest request)
        {
            List<string> arguments = [];

            Add(arguments, "--lang", request.LanguageStorePath);
            Add(arguments, "--loc", request.LocationStorePath);
            Add(arguments, "--output", outputDirectoryPath);
            Add(arguments, "--id", request.ModId);
            Add(arguments, "--name", request.ModName);
            Add(arguments, "--version", request.ModVersion);
            Add(arguments, "--game", request.Game);
            Add(arguments, "--game-version", request.GameVersion);
            Add(arguments, "--verbose", GetVerboseValue(request.AreVerboseCommentsEnabled));

            AddOptional(arguments, "--dependency", request.Dependency);
            AddOptional(arguments, "--landed-titles", request.LandedTitlesFilePath);
            AddOptional(arguments, "--landed-titles-name", request.LandedTitlesFileName);

            return [.. arguments];
        }

        internal static string[] Without(string[] arguments, string argumentName)
        {
            List<string> remainingArguments = [];
            int argumentIndex = 0;

            while (argumentIndex < arguments.Length)
            {
                if (string.Equals(arguments[argumentIndex], argumentName, StringComparison.Ordinal))
                {
                    argumentIndex += 2;

                    continue;
                }

                remainingArguments.Add(arguments[argumentIndex]);
                argumentIndex += 1;
            }

            return [.. remainingArguments];
        }

        internal static string[] With(string[] arguments, string argumentName, string argumentValue)
        {
            List<string> extendedArguments = [.. arguments];

            Add(extendedArguments, argumentName, argumentValue);

            return [.. extendedArguments];
        }

        private static void Add(ICollection<string> arguments, string argumentName, string argumentValue)
        {
            arguments.Add(argumentName);
            arguments.Add(argumentValue);
        }

        private static void AddOptional(ICollection<string> arguments, string argumentName, string? argumentValue)
        {
            if (argumentValue is null)
            {
                return;
            }

            Add(arguments, argumentName, argumentValue);
        }

        private static string GetVerboseValue(bool areVerboseCommentsEnabled)
        {
            if (areVerboseCommentsEnabled)
            {
                return "true";
            }

            return "false";
        }
    }
}
