using System;
using System.Collections.Generic;

namespace MoreCulturalNamesBuilder.UnitTests.Configuration
{
    internal static class SettingsTestArguments
    {
        internal static string[] Except(string argumentName)
            => Without(WithLandedTitles(), argumentName);

        internal static string[] With(string argumentName, string argumentValue)
            => Replace(WithLandedTitles(), argumentName, argumentValue);

        internal static string[] WithoutOptionalArguments()
            => Without(WithLandedTitles(), "--landed-titles");

        internal static string[] WithGameAndDependency(string game, string dependency)
            => Replace(Replace([.. AllArguments()], "--game", game), "--dependency", dependency);

        private static List<string> AllArguments() =>
        [
            "--lang", "languages.xml",
            "--loc", "locations.xml",
            "--output", "mods",
            "--id", "more-cultural-names",
            "--name", "More Cultural Names",
            "--version", "1.0.0",
            "--game", "CK2",
            "--game-version", "1.12.*"
        ];

        private static string[] WithLandedTitles()
        {
            List<string> arguments = [.. AllArguments()];
            arguments.Add("--landed-titles");
            arguments.Add("landed_titles.txt");

            return [.. arguments];
        }

        private static string[] Without(string[] arguments, string argumentName)
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

        private static string[] Replace(string[] arguments, string argumentName, string argumentValue)
        {
            List<string> updatedArguments = [.. arguments];

            for (int argumentIndex = 0; argumentIndex + 1 < updatedArguments.Count; argumentIndex += 2)
            {
                if (string.Equals(updatedArguments[argumentIndex], argumentName, StringComparison.Ordinal))
                {
                    updatedArguments[argumentIndex + 1] = argumentValue;

                    return [.. updatedArguments];
                }
            }

            updatedArguments.Add(argumentName);
            updatedArguments.Add(argumentValue);

            return [.. updatedArguments];
        }
    }
}
