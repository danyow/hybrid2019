using System.IO;
using UnityEditor;
using UnityEngine;

public static class CopyToServerEditor
{
    [MenuItem("HybridCLR/CopyToServer")]
    public static void CopyToServer()
    {
        HybridCLR.Editor.Commands.CompileDllCommand.CompileDllActiveBuildTarget();
        BuildAssetBundleEditor.BuildAssetBundleByTarget_Active();
        CopyAssetsEditor.CopyHotUpdateAssembliesToStreamingAssets_Active();
        const string target = "E:\\server\\StreamingAssets";
        if(Directory.Exists(target))
        {
            Directory.Delete(target, true);
        }
        Directory.Move(Application.streamingAssetsPath, target);
    }
}