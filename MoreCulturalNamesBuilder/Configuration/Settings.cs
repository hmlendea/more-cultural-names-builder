namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class Settings(string[] args)
    {
        public InputSettings Input { get; } = new(args);

        public ModSettings Mod { get; } = new(args);

        public OutputSettings Output { get; } = new(args);
    }
}
