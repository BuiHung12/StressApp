using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RangerCity.Lobby.Editor
{
    public static class BuildScript
    {
        [MenuItem("Build/Build Android APK")]
        public static void BuildAndroid()
        {
            string[] scenes = { "Assets/Scenes/Lobby.unity" };
            
            // Ensure output directory exists
            string outputDir = "Builds/Android";
            if (!System.IO.Directory.Exists(outputDir))
            {
                System.IO.Directory.CreateDirectory(outputDir);
            }
            
            string outputPath = System.IO.Path.Combine(outputDir, "RangerCity.apk");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = outputPath;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;

            Debug.Log("[BuildScript] Starting Android build...");
            
            // Switch target group first to avoid compatibility warnings
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            // Force OpenGL ES 3 only to avoid Vulkan flickering/compatibility issues on some Android devices
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] { 
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 
            });

            // Force default screen orientation to landscape
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

            // Support both 32-bit and 64-bit ARM architectures for wide device compatibility
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[BuildScript] Build succeeded: " + (summary.totalSize / 1024f / 1024f).ToString("F2") + " MB at " + outputPath);
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("[BuildScript] Build failed");
            }
        }

        [MenuItem("Build/Build Linux Server")]
        public static void BuildLinuxServer()
        {
            string[] scenes = { "Assets/Scenes/Lobby.unity" };
            string outputDir = "Builds/Linux";
            if (!System.IO.Directory.Exists(outputDir))
            {
                System.IO.Directory.CreateDirectory(outputDir);
            }
            string outputPath = System.IO.Path.Combine(outputDir, "RangerCityServer");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = outputPath;
            buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
            buildPlayerOptions.options = BuildOptions.None;

            Debug.Log("[BuildScript] Starting StandaloneLinux64 player build...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
            EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
            
            // Support OpenGLCore for full rendering/shader stability when running with DISPLAY=:0
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneLinux64, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneLinux64, new UnityEngine.Rendering.GraphicsDeviceType[] {
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore,
                UnityEngine.Rendering.GraphicsDeviceType.Null
            });

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[BuildScript] Linux build succeeded: " + (summary.totalSize / 1024f / 1024f).ToString("F2") + " MB at " + outputPath);
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("[BuildScript] Linux build failed");
            }
        }
    }
}

