using System.IO;
using HybridCLR.Editor;
using UnityEditor;
using UnityEngine;

public static class CopyAssetsEditor
{
    [MenuItem("HybridCLR/CopyHotUpdateAssembliesToStreamingAssets")]
    public static void CopyHotUpdateAssembliesToStreamingAssets_Active()
    {
        CopyHotUpdateAssembliesToStreamingAssets(EditorUserBuildSettings.activeBuildTarget);
        CopyAOTAssembliesToStreamingAssets(EditorUserBuildSettings.activeBuildTarget);
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

    public static void CopyAOTAssembliesToStreamingAssets(BuildTarget target)
    {
        var aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
        var aotAssembliesDstDir = Application.streamingAssetsPath;

        foreach(var dll in SettingsUtil.AOTAssemblyNames)
        {
            var srcDllPath = $"{aotAssembliesSrcDir}/{dll}.dll";
            if(!File.Exists(srcDllPath))
            {
                Debug.LogError($"ab中添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                continue;
            }
            var dllBytesPath = $"{aotAssembliesDstDir}/{dll}.dll.bytes";
            File.Copy(srcDllPath, dllBytesPath, true);
            Debug.Log($"[CopyAOTAssembliesToStreamingAssets] copy AOT dll {srcDllPath} -> {dllBytesPath}");
        }
        AssetDatabase.Refresh();
    }
}