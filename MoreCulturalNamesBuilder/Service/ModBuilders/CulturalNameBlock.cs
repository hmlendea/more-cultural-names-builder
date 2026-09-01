namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    internal sealed class CulturalNameBlock(int startLineIndex, int endLineIndex)
    {
        internal int StartLineIndex { get; } = startLineIndex;

        internal int EndLineIndex { get; } = endLineIndex;
    }
}