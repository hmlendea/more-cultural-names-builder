namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class Settings
    {
        public InputSettings Input { get; }

        public ModSettings Mod { get; }

        public OutputSettings Output { get; }

        public Settings(string[] args)
        {
            Input = new InputSettings(args);
            Mod = new ModSettings(args);
            Output = new OutputSettings(args);
        }
    }
}
