
# HybridCLR 原理


关键词：

- c#
- .net
- IL
- CLI
- Mono
- il2cpp
- AOT
- JIT
- il2cpp vm



附带链接：

[源码流程解析(烟雨)](https://www.lfzxb.top/hybridclr-source-analyze/)

[热更新原理](https://www.bilibili.com/video/BV1wT411V7Kk/?spm_id_from=333.337.search-card.all.click&vd_source=63c69e4588004688b161e96ed46205e2)








## 安装步骤

1. 导入`HybridCLR package`
   点击 `Window` / `Package Manager` /  左上角 + 号 /  `Add package form git URL...`
   键入 `https://gitee.com/focus-creative-games/hybridclr_unity.git` 回车
2. 切换版本为 `2019.4.40f1c1`
   因为目前只是支持大版本的相对最新版本
3. 初始化`HybridCLR`
   点击`HybridCLR` / `Installer` / 安装
4. 切换回`2019.4.23f1c1`版本

## 配置项目

1. 关闭 `Use Incremental GC` 选项。因为目前不支持增量式`GC`。
2. `Scripting Backend` 切换为 `il2cpp`
3. `Api Compatability Level` 切换为 `.Net 4 or .Net Framework`

## 配置`HybridCLR`

1. 点击菜单 `HybridCLR/Settings` 打开配置界面
2. 在 `Hot Update Assemblies` 内加入 `Assembly-CSharp`

## 生成 `Dll` 并放置到 `StreamingAssets`

例如 `Win64` :
1. 使用菜单`HybridCLR/CompileDll/win64`来编译你期望的平台的热更新`dll`，编译完成后的热更新`dll`将会生成到 `{project}/HybridCLRData/HotUpdateDlls/StandaloneWindows64` 目录下
2. 在 `Assets` 目录下创建 `StreamingAssets`
3. 将 `{project}/HybridCLRData/HotUpdateDlls/StandaloneWindows64` 目录下生成的 `Assembly-CSharp.dll` 修改为 `Assembly-CSharp.dll.bytes` 放到 `Assets/StreamingAssets`

> 第三步可以通过编辑器快捷操作

代码如下：
```csharp
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
```


## 运行热更新代码(代码在 `Assembly-CSharp`)

### 第一种：通过反射

#### 创建一个热更的入口类

```csharp
public interface IEntry
{
    void Run(string from);
}

public class HotUpdateEntry: IEntry
{
    public static void Main(string from)
    {
        UnityEngine.Debug.Log($"[HotUpdateEntry Main] Hello, HybridCLR from {from}");
    }

    public void Run(string from)
    {
        UnityEngine.Debug.Log($"[HotUpdateEntry Run] Hello, HybridCLR from {from}");
    }
}
```

> 创建完后要生成这个 `dll` 到 `StreamAssets`

#### 创建一个名为`Entry`的入口脚本

```csharp
using System;  
using System.Collections;  
using System.IO;  
using System.Reflection;  
using UnityEngine;  
using UnityEngine.Networking;  
  
public class Entry: MonoBehaviour  
{  
    private void Awake()  
    {  
        StartCoroutine(LoadAssembly());  
    }  
  
    private const string kAssembleName = "Assembly-CSharp.dll.bytes";  
  
    private static IEnumerator LoadAssembly()  
    {  
        var path = Path.Combine(Application.streamingAssetsPath, kAssembleName);  
        var request = UnityWebRequest.Get(path);  
        yield return request.SendWebRequest();  
        if(!request.isDone)  
        {  
            yield break;  
        }  
        var assembleData = request.downloadHandler.data;  
        var assembly = Assembly.Load(assembleData);  
  
        Debug.Log(assembly);  
    }  
}
```

挂在到场景里的游戏对象上


#### 运行 `HotUpdateEntry`的函数

1. 直接加载静态方法：
    
    ```csharp
    private static IEnumerator LoadAssembly()  
    {  
        ...
        Debug.Log(assembly);  
        InvokeStaticMethod(assembly);  
    }
    
    private static void InvokeStaticMethod(Assembly assembly)  
    {  
        var entryType = assembly.GetType("HotUpdateEntry");  
        var method = entryType.GetMethod("Main");  
        method.Invoke(null, new[] { nameof(InvokeStaticMethod), });  
    }
    ```

2. 创建 `Delegate` 加载方法

    ```csharp
    private static IEnumerator LoadAssembly()  
    {  
        ...
        Debug.Log(assembly);  
        CreateDelegate(assembly);  
    }
    
    private static void CreateDelegate(Assembly assembly)  
    {  
        var entryType = assembly.GetType("HotUpdateEntry");  
        var method = entryType.GetMethod("Main");  
        var mainFunc = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), method);  
        mainFunc(nameof(CreateDelegate));  
    }
    ```

3. 通过 `Interface` 生成对象调用

    ```csharp
    private static IEnumerator LoadAssembly()  
    {  
        ...
        Debug.Log(assembly);  
        CreateInterface(assembly);  
    }
    
    private static void CreateInterface(Assembly assembly)  
    {  
        var entryType = assembly.GetType("HotUpdateEntry");  
        var entry = (IEntry)Activator.CreateInstance(entryType);  
        entry.Run(nameof(CreateInterface));  
    }
    ```


### 第二种：通过 `Prefab`  (官方推荐)

#### 创建预制体

首先创建一个 `HotUpdatePreafb.prefab` 的预制体，放置到 `Assets/Prefabs/` 下

#### 挂载热更脚本

```csharp
using UnityEngine;

public class HotUpdateMono: MonoBehaviour
{
    private void Start()
    {
        Debug.Log("[HotUpdateMono Start] Hello, HybridCLR from Start");
    }
}
```

#### 生成 `AssetBundle` 并将其移动到 `StreamingAssets`

借助编辑器

```csharp
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
```

#### 修改 `Entry.cs` 入口脚本

```csharp
    private void Awake()
    {
        // StartCoroutine(LoadAssembly());
        StartCoroutine(LoadAssetBundle());
    }
    
    ...
    
    private const string kPrefabsName = "prefabs";

    private static IEnumerator LoadAssetBundle()
    {
        var path = Path.Combine(Application.streamingAssetsPath, kPrefabsName);

        var request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();
        if(!request.isDone)
        {
            yield break;
        }
        var assetBundleData = request.downloadHandler.data;

        var assetBundle = AssetBundle.LoadFromMemory(assetBundleData);
        Instantiate(assetBundle.LoadAsset<GameObject>("HotUpdatePrefab.prefab"));
    }
```