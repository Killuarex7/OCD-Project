using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.Rendering;

// Adds an "OCD Project" menu to the Unity menu bar with three steps:
//   1. Prepare for Quest  - switches to Android and applies headset-ready settings
//   2. Build APK          - builds Builds/OCD-Project.apk
//   3. Build And Run      - builds and installs straight onto a connected headset
public static class QuestBuildSetup
{
    private const string ScenePath = "Assets/Scenes/OCD Project Setup.unity";
    private const string ApkPath = "Builds/OCD-Project.apk";
    private const string PackageName = "com.ocdproject.vr";
    private const string TemplatePackagePrefix = "com.UnityTechnologies.com.unity.template";
    private const string MobileUrpAssetPath = "Assets/URPDefaultResources/Mobile.asset";

    [MenuItem("OCD Project/1. Prepare for Quest (Android)", priority = 1)]
    public static void PrepareForQuest()
    {
        // Switch platform to Android (this can take a few minutes the first time).
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                EditorUtility.DisplayDialog("OCD Project",
                    "Could not switch to Android.\n\nInstall Android Build Support (with OpenJDK and Android SDK & NDK) " +
                    "for this Unity version from Unity Hub > Installs > Add modules.", "OK");
                return;
            }
        }

        NamedBuildTarget android = NamedBuildTarget.Android;

        // Quest needs 64-bit IL2CPP builds.
        PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // Quest 2 / 3 / Pro run Android 12L (API 32) or newer.
        PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)32;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

        // Replace the URP template's package name with one for this app.
        string currentId = PlayerSettings.GetApplicationIdentifier(android);
        if (string.IsNullOrEmpty(currentId) || currentId.StartsWith(TemplatePackagePrefix))
            PlayerSettings.SetApplicationIdentifier(android, PackageName);

        // Vulkan first (best on Quest), OpenGL ES 3 as fallback. Linear colour.
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,
            new[] { GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLES3 });
        PlayerSettings.colorSpace = ColorSpace.Linear;

        // Quest texture format, and build a plain .apk (not an .aab).
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
        EditorUserBuildSettings.buildAppBundle = false;

        // Make sure the game scene is the one that gets built.
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        TuneMobileRendering();

        string xrStatus = CheckAndroidXR();

        AssetDatabase.SaveAssets();

        string message =
            "Android / Quest settings applied:\n\n" +
            "- Platform: Android\n" +
            "- IL2CPP, ARM64\n" +
            "- Min Android API 32\n" +
            "- Package: " + PlayerSettings.GetApplicationIdentifier(android) + "\n" +
            "- Vulkan + OpenGL ES 3, Linear colour, ASTC textures\n" +
            "- Mobile URP: HDR off, 4x MSAA, hard shadows\n" +
            "- Scene: " + ScenePath + "\n\n" +
            xrStatus;

        Debug.Log("[OCD Project] " + message);
        EditorUtility.DisplayDialog("OCD Project", message, "OK");
    }

    [MenuItem("OCD Project/2. Build APK", priority = 2)]
    public static void BuildApk()
    {
        Build(false);
    }

    [MenuItem("OCD Project/3. Build And Run on Headset (USB)", priority = 3)]
    public static void BuildAndRun()
    {
        Build(true);
    }

    private static void Build(bool runOnHeadset)
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUtility.DisplayDialog("OCD Project",
                "Run 'OCD Project > 1. Prepare for Quest (Android)' first.", "OK");
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(ApkPath));

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = ApkPath,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = runOnHeadset ? BuildOptions.AutoRunPlayer : BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            string fullPath = Path.GetFullPath(ApkPath);
            Debug.Log($"[OCD Project] Build succeeded: {fullPath} ({summary.totalSize / (1024 * 1024)} MB)");

            if (!runOnHeadset)
                EditorUtility.RevealInFinder(fullPath);
        }
        else
        {
            Debug.LogError($"[OCD Project] Build {summary.result} with {summary.totalErrors} error(s). See the Console above.");
        }
    }

    // Quest-friendly settings on the URP asset Android uses ("Mobile" quality level).
    private static void TuneMobileRendering()
    {
        Object urpAsset = AssetDatabase.LoadMainAssetAtPath(MobileUrpAssetPath);
        if (urpAsset == null)
        {
            Debug.LogWarning("[OCD Project] Mobile URP asset not found at " + MobileUrpAssetPath + "; rendering settings left as they are.");
            return;
        }

        SerializedObject so = new SerializedObject(urpAsset);
        SetInt(so, "m_SupportsHDR", 0);           // HDR is expensive on Quest
        SetInt(so, "m_MSAA", 4);                  // 4x MSAA keeps edges smooth in VR
        SetInt(so, "m_SoftShadowsSupported", 0);  // soft shadows cost a lot on mobile
        SetFloat(so, "m_ShadowDistance", 20f);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(urpAsset);
    }

    private static void SetInt(SerializedObject so, string name, int value)
    {
        SerializedProperty p = so.FindProperty(name);
        if (p == null) return;
        if (p.propertyType == SerializedPropertyType.Boolean) p.boolValue = value != 0;
        else p.intValue = value;
    }

    private static void SetFloat(SerializedObject so, string name, float value)
    {
        SerializedProperty p = so.FindProperty(name);
        if (p != null) p.floatValue = value;
    }

    private static string CheckAndroidXR()
    {
        var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);

        bool ok = settings != null &&
                  settings.InitManagerOnStart &&
                  settings.Manager != null &&
                  settings.Manager.activeLoaders.Count > 0;

        if (ok)
            return "XR: OpenXR is enabled for Android.";

        Debug.LogWarning("[OCD Project] OpenXR is not enabled for Android.");
        return "WARNING: OpenXR is NOT enabled for Android.\n" +
               "Open Edit > Project Settings > XR Plug-in Management, Android tab, tick OpenXR " +
               "and enable the 'Meta Quest Support' feature.";
    }
}
