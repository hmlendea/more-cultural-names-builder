using NuciCLI.Arguments;

namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class Settings
    {
        public InputSettings Input { get; set; }

        public ModSettings Mod { get; set; }

        public OutputSettings Output { get; set; }

        public Settings(string[] args)
        {
            ArgumentParser parser = new();

            // Input arguments.
            parser.AddArgument("lang", required: true);
            parser.AddArgument("loc", required: true);
            parser.AddArgument("landed-titles");

            // Output arguments.
            parser.AddArgument("output");
            parser.AddArgument("out");
            parser.AddArgument("verbose", defaultValue: "false");
            parser.AddArgument("landed-titles-name");

            // Mod arguments.
            parser.AddArgument("id", required: true);
            parser.AddArgument("name", required: true);
            parser.AddArgument("version");
            parser.AddArgument("ver");
            parser.AddArgument("dependency");
            parser.AddArgument("dep");
            parser.AddArgument("game", required: true);
            parser.AddArgument("game-version", required: true);

            ArgumentsCollection parsedArgs = parser.ParseArgs(args);

            Input = new(parsedArgs);
            Mod = new(parsedArgs);
            Output = new(parsedArgs);
        }
    }
}
