namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class Settings(string[] args)
    {
        public InputSettings Input { get; } = new InputSettings(args);

        public ModSettings Mod { get; } = new ModSettings(args);

        public OutputSettings Output { get; } = new OutputSettings(args);
    }
}
