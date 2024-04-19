// You can call the builder function using commands like:
//
// /Path/To/Unity -quit -batchmode -executeMethod Builder.Build Android
// /Path/To/Unity -quit -batchmode -executeMethod Builder.Build AndroidTest
// /Path/To/Unity -quit -batchmode -executeMethod Builder.Build iOS
//
// See https://support.unity.com/hc/en-us/articles/115000368846-How-do-I-support-different-configurations-to-build-specific-players-by-command-line-or-auto-build-system

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Builder
{
    public const string BUILD_PATH = "Builds/";

    public static void SetBaseSettings()
    {
        // Set core settings for all builds
        PlayerSettings.actionOnDotNetUnhandledException = ActionOnDotNetUnhandledException.Crash;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowFullscreenSwitch = true;
        PlayerSettings.captureSingleScreen = false;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.enableCrashReportAPI = true;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.macRetinaSupport = true;
        // PlayerSettings.resetResolutionOnWindowResize = true;
        // PlayerSettings.runInBackground = true;
        PlayerSettings.statusBarHidden = true;
        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);

        var version = Environment.GetEnvironmentVariable("OKEY_BUILD_VERSION");
        if (!string.IsNullOrEmpty(version))
        {
            PlayerSettings.bundleVersion = version;
        }
        else
        {
            PlayerSettings.bundleVersion = "0.0.0";
        }
    }

    public static void SetTestSettings()
    {
        // Set extra settings for a test build
        PlayerSettings.enableFrameTimingStats = true;
        PlayerSettings.enableInternalProfiler = true;
        PlayerSettings.forceSingleInstance = false;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.usePlayerLog = true;
    }

    public static void SetReleaseSettings()
    {
        // Set extra settings for a test build
        PlayerSettings.enableFrameTimingStats = false;
        PlayerSettings.enableInternalProfiler = false;
        PlayerSettings.forceSingleInstance = true;
        PlayerSettings.resizableWindow = false;
        PlayerSettings.usePlayerLog = false;
    }

    public static List<string> GetEditorScenes()
    {
        // Get all the scenes in the build settings of the editor
        var EditorScenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                EditorScenes.Add(scene.path);
            }
        }
        return EditorScenes;
    }

    [MenuItem("MyTools/Test Build/Android Test Build")]
    public static void AndroidTestBuild()
    {
        SetBaseSettings();
        SetTestSettings();

        // Save path for the build relative to the Unity project root
        var RelativeSaveLocation = "Builds/Android/OkeyTest";

        // Build the player
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetEditorScenes().ToArray(),
            locationPathName = RelativeSaveLocation,
            target = BuildTarget.Android,
            options =
                BuildOptions.Development
                | BuildOptions.AllowDebugging
                | BuildOptions.EnableDeepProfilingSupport
                | BuildOptions.DetailedBuildReport,
            extraScriptingDefines = new[] { "DEBUG" }
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);

        EditorUtility.DisplayDialog(
            "Build Complete",
            "Android Test Build Complete\n\nSaved to: "
                + Path.Join(
                    Directory.GetParent(Application.dataPath).ToString(),
                    RelativeSaveLocation
                ),
            "OK"
        );
    }

    [MenuItem("MyTools/Test Build/iOS Test Build")]
    public static void iOSTestBuild()
    {
        SetBaseSettings();
        SetTestSettings();

        // Save path for the build relative to the Unity project root
        var RelativeSaveLocation = "Builds/iOS/OkeyTest";

        // Build the player
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetEditorScenes().ToArray(),
            locationPathName = RelativeSaveLocation,
            target = BuildTarget.iOS,
            options =
                BuildOptions.Development
                | BuildOptions.AllowDebugging
                | BuildOptions.EnableDeepProfilingSupport
                | BuildOptions.DetailedBuildReport,
            extraScriptingDefines = new[] { "DEBUG" }
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);

        EditorUtility.DisplayDialog(
            "Build Complete",
            "iOS Test Build Complete\n\nSaved to: "
                + Path.Join(
                    Directory.GetParent(Application.dataPath).ToString(),
                    RelativeSaveLocation
                ),
            "OK"
        );
    }

    [MenuItem("MyTools/Test Build/Linux x86_64 Test Build")]
    public static void LinuxTestBuild()
    {
        SetBaseSettings();
        SetTestSettings();

        // Save path for the build relative to the Unity project root
        var RelativeSaveLocation = "Builds/Linux/OkeyTest";

        // Build the player
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetEditorScenes().ToArray(),
            locationPathName = RelativeSaveLocation,
            target = BuildTarget.StandaloneLinux64,
            options =
                BuildOptions.Development
                | BuildOptions.AllowDebugging
                | BuildOptions.EnableDeepProfilingSupport
                | BuildOptions.DetailedBuildReport,
            extraScriptingDefines = new[] { "DEBUG" }
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);

        EditorUtility.DisplayDialog(
            "Build Complete",
            "Linux x86_64 Test Build Complete\n\nSaved to: "
                + Path.Join(
                    Directory.GetParent(Application.dataPath).ToString(),
                    RelativeSaveLocation
                ),
            "OK"
        );
    }

    [MenuItem("MyTools/Test Build/Mac Universal Test Build")]
    public static void MacTestBuild()
    {
        SetBaseSettings();
        SetTestSettings();

        // Save path for the build relative to the Unity project root
        var RelativeSaveLocation = "Builds/Mac/OkeyTest.app";

        // Build the player
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetEditorScenes().ToArray(),
            locationPathName = RelativeSaveLocation,
            target = BuildTarget.StandaloneOSX,
            options =
                BuildOptions.Development
                | BuildOptions.AllowDebugging
                | BuildOptions.EnableDeepProfilingSupport
                | BuildOptions.DetailedBuildReport,
            extraScriptingDefines = new[] { "DEBUG" }
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);

        EditorUtility.DisplayDialog(
            "Build Complete",
            "Mac Universal Test Build Complete\n\nSaved to: "
                + Path.Join(
                    Directory.GetParent(Application.dataPath).ToString(),
                    RelativeSaveLocation
                ),
            "OK"
        );
    }

    [MenuItem("MyTools/Release Build/Mac Universal Release Build")]
    public static void MacReleaseBuild()
    {
        SetBaseSettings();
        SetReleaseSettings();

        // Save path for the build relative to the Unity project root
        var RelativeSaveLocation = "Builds/Mac/Okey.app";

        // Build the player
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetEditorScenes().ToArray(),
            locationPathName = RelativeSaveLocation,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);

        EditorUtility.DisplayDialog(
            "Build Complete",
            "Mac Universal Release Build Complete\n\nSaved to: "
                + Path.Join(
                    Directory.GetParent(Application.dataPath).ToString(),
                    RelativeSaveLocation
                ),
            "OK"
        );
    }

    [MenuItem("MyTools/Test Build/Windows x86_64 Test Build")]
    public static void WindowsTestBuild()
    {
        SetBaseSettings();
        SetTestSettings();

        // Save path for the build relative to the Unity project root
        var RelativeSaveLocation = "Builds/Windows/OkeyTest.exe";

        // Build the player
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetEditorScenes().ToArray(),
            locationPathName = RelativeSaveLocation,
            target = BuildTarget.StandaloneWindows64,
            options =
                BuildOptions.Development
                | BuildOptions.AllowDebugging
                | BuildOptions.EnableDeepProfilingSupport
                | BuildOptions.DetailedBuildReport,
            extraScriptingDefines = new[] { "DEBUG" }
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);

        EditorUtility.DisplayDialog(
            "Build Complete",
            "Windows x86_64 Test Build Complete\n\nSaved to: "
                + Path.Join(
                    Directory.GetParent(Application.dataPath).ToString(),
                    RelativeSaveLocation
                ),
            "OK"
        );
    }
}
