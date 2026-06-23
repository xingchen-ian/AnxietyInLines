using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class WebGLBuild
{
    public static void PerformBuild()
    {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = true;

        DirectoryInfo projectRootInfo = Directory.GetParent(Application.dataPath);
        if (projectRootInfo == null)
            throw new InvalidOperationException("Unable to resolve Unity project root.");

        DirectoryInfo repositoryRootInfo = Directory.GetParent(projectRootInfo.FullName);
        if (repositoryRootInfo == null)
            throw new InvalidOperationException("Unable to resolve repository root.");

        string projectRoot = projectRootInfo.FullName;
        string repositoryRoot = repositoryRootInfo.FullName;
        string outputPath = Path.Combine(repositoryRoot, "website", "game");
        Directory.CreateDirectory(outputPath);

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new InvalidOperationException("No enabled scenes found in Build Settings.");

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException($"WebGL build failed: {report.summary.result}");
    }
}
