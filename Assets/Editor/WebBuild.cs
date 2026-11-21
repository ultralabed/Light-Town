using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 做出網頁版（WebGL）。GitHub Actions 會自動呼叫 WebBuild.Build。
/// 你也可以在 Unity 上面的選單按：Seven Lantern City → Build Web Version。
/// 輸出資料夾：build/WebGL/web
/// </summary>
public static class WebBuild
{
    const string ScenePath = "Assets/Scenes/Main.unity";
    const string OutputPath = "build/WebGL/web";

    [MenuItem("Seven Lantern City/Build Web Version")]
    public static void Build()
    {
        // 遊戲會自己啟動，所以只需要一個有攝影機的空場景。沒有就自動做一個。
        if (!File.Exists(ScenePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
        }

        PlayerSettings.productName = "Seven Lantern City";
        PlayerSettings.companyName = "Light Town";
        PlayerSettings.runInBackground = true;
        PlayerSettings.defaultWebScreenWidth = 960;
        PlayerSettings.defaultWebScreenHeight = 540;
        // GitHub Pages 不會幫忙解壓縮，所以一定要關掉壓縮，不然網頁打不開
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;

        var options = new BuildPlayerOptions();
        options.scenes = new[] { ScenePath };
        options.locationPathName = OutputPath;
        options.target = BuildTarget.WebGL;
        options.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("Web build failed: " + report.summary.result);
            if (Application.isBatchMode) EditorApplication.Exit(1);
            return;
        }
        Debug.Log("Web build done: " + OutputPath);
    }
}
