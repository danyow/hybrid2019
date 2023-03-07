using System.IO;
using HybridCLR.Editor;
using UnityEditor;
using UnityEngine;

public static class CopyAssetsEditor
{
    [MenuItem("HybridCLR/CopyHotUpdateAssembliesToStreamingAssets/ActiveBuildTarget")]
    public static void CopyHotUpdateAssembliesToStreamingAssets_Active()
    {
        CopyHotUpdateAssembliesToStreamingAssets(EditorUserBuildSettings.activeBuildTarget);
    }

    private static void CopyHotUpdateAssembliesToStreamingAssets(BuildTarget target)
    {
        var hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        var hotfixAssembliesDstDir = Application.streamingAssetsPath;
        Directory.CreateDirectory(hotfixAssembliesDstDir);
        foreach(var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
        {
            var dllPath = $"{hotfixDllSrcDir}/{dll}";
            var dllBytesPath = $"{hotfixAssembliesDstDir}/{dll}.bytes";
            File.Copy(dllPath, dllBytesPath, true);
            Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {dllPath} -> {dllBytesPath}");
        }
        AssetDatabase.Refresh();
    }
}