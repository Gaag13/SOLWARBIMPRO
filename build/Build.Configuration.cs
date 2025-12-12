sealed partial class Build
{
    const string Version = "1.2.1";
    readonly AbsolutePath ArtifactsDirectory = RootDirectory / "output";

    protected override void OnBuildInitialized()
    {
        Configurations =
        [
            "Release*",
            "Installer*"
        ];

        Bundles =
        [
            Solution.WARBIMPRO
        ];

        InstallersMap = new()
        {
            {Solution.Installer, Solution.WARBIMPRO}
        };
    }
}