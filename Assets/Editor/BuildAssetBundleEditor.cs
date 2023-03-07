using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BuildAssetBundleEditor
{
    [MenuItem("HybridCLR/BuildAssetBundle")]
    public static void BuildAssetBundleByTarget_Active()
    {
        BuildAssetBundleByTarget(EditorUserBuildSettings.activeBuildTarget);
    }

    private static void BuildAssetBundleByTarget(BuildTarget target)
    {
        var tmp = Path.Combine(Application.dataPath, "BuildCache", "AssetBundle", target.ToString(), "Tmp")
                      .Replace(Path.DirectorySeparatorChar, '/');
        var output = Path.Combine(Application.dataPath, "BuildCache", "AssetBundle", target.ToString(), "Output")
                         .Replace(Path.DirectorySeparatorChar, '/');
        BuildAssetBundles(tmp, output, target);
        CopyAssetBundlesToStreamingAssets(output);
    }

    private static void BuildAssetBundles(string tempDir, string outputDir, BuildTarget target)
    {
        Directory.CreateDirectory(tempDir);
        Directory.CreateDirectory(outputDir);

        var abs = new List<AssetBundleBuild>();

        {
            var prefabAssets = new List<string>();
            var testPrefab = $"{Application.dataPath}/Prefabs/HotUpdatePrefab.prefab";
            prefabAssets.Add(testPrefab);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            abs.Add(new AssetBundleBuild { assetBundleName = "prefabs", assetNames = prefabAssets.Select(ToRelativeAssetPath).ToArray(), });
        }

        BuildPipeline.BuildAssetBundles(outputDir, abs.ToArray(), BuildAssetBundleOptions.None, target);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    private static void CopyAssetBundlesToStreamingAssets(string outputDir)
    {
        var streamingAssetPathDst = Application.streamingAssetsPath;
        Directory.CreateDirectory(streamingAssetPathDst);
        var abs = new[] { "prefabs", };
        foreach(var ab in abs)
        {
            var srcAb = ToRelativeAssetPath($"{outputDir}/{ab}");
            var dstAb = ToRelativeAssetPath($"{streamingAssetPathDst}/{ab}");
            Debug.Log($"[CopyAssetBundlesToStreamingAssets] copy assetbundle {srcAb} -> {dstAb}");
            AssetDatabase.CopyAsset(srcAb, dstAb);
            AssetDatabase.Refresh();
        }
    }

    private static string ToRelativeAssetPath(string s)
    {
        return s.Substring(s.IndexOf("Assets/"));
    }
}