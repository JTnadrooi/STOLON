namespace STOLON.CLI.Build
{
    public class FontsBuilder : IBuilder
    {
        public FontsBuilder() { }

        public void PreBuild()
        {
            Directory.CreateDirectory("Fonts\\");
        }

        public void Build(string src, string dest)
        {
            BuildHelper.Copy(src, dest);
        }

        public BuildItemInfo[] GetBuildItems() => BuildItemInfo.GetRelativeBuildItems(CLI.SourcePath! + @"STOLON\resources\Fonts\", "*");
    }
}
