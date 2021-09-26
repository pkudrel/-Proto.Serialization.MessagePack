using System;
using System.IO;
using AbcVersionTool;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    [Parameter("Build counter from outside environment")]
    readonly int BuildCounter;


    [Parameter("NuGet server URL.")]
    readonly string NugetSource = "https://api.nuget.org/v3/index.json";


    string NugetApiKey = Environment.GetEnvironmentVariable("NugetToken");

    readonly DateTime BuildDate = DateTime.UtcNow;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository] readonly GitRepository GitRepository;

    [Solution] readonly Solution Solution;



    AbcVersion AbcVersion => AbcVersionFactory.Create(BuildCounter, BuildDate);

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";


    Target Information => _ => _
        .Before(Clean)
        .Executes(() =>
        {
            var b = AbcVersion;
            Logger.Info($"Host: '{Host}'");
            Logger.Info($"Version: '{b.SemVersion}'");
            Logger.Info($"Date: '{b.DateTime:s}Z'");
            Logger.Info($"Build target: {Configuration}");
            Logger.Info($"FullVersion: '{b.InformationalVersion}'");
        });

    Target Clean => _ => _
        .DependsOn(Information)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x =>
            {
                if (Path.GetFileName(x.Parent) == "build") return;
                Logger.Info($"Delete: {x}");
                DeleteDirectory(x);
            });
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                    .SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .SetVersion(AbcVersion.MajorMinor)
                    .SetAssemblyVersion(AbcVersion.AssemblyVersion)
                    .SetFileVersion(AbcVersion.FileVersion)
                    .SetInformationalVersion(AbcVersion.InformationalVersion)
                    .EnableNoRestore())
                ;
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPack(_ => _
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
                .EnableNoBuild()
                .AddProperty("Version", AbcVersion.NugetVersion)
            );
        });

    Target Push => _ => _
        .DependsOn(Pack)
        .OnlyWhenStatic(()=> string.IsNullOrEmpty(NugetApiKey) == false)
        .Executes(() =>
        {


            DotNetNuGetPush(s => s
                .SetSource(NugetSource)
                .SetApiKey(NugetApiKey)
                .CombineWith(ArtifactsDirectory.GlobFiles("*.nupkg"), (s, v) => s
                    .SetTargetPath(v)
                )
            );
        });

    public static int Main() => Execute<Build>(x => x.Push);
}