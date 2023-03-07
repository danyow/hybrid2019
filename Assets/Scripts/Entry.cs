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
        // StartCoroutine(LoadAssembly());
        StartCoroutine(LoadAssetBundle());
    }

    private const string kAssembleName = "Assembly-CSharp.dll.bytes";

    private static IEnumerator LoadAssembly()
    {
        var path = GetPath(kAssembleName);
        var request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();
        if(!request.isDone)
        {
            yield break;
        }
        var assembleData = request.downloadHandler.data;
        var assembly = Assembly.Load(assembleData);

        // Debug.Log(assembly);

        // InvokeStaticMethod(assembly);

        // CreateDelegate(assembly);

        CreateInterface(assembly);
    }

    private static void InvokeStaticMethod(Assembly assembly)
    {
        var entryType = assembly.GetType("HotUpdateEntry");
        var method = entryType.GetMethod("Main");
        method.Invoke(null, new[] { nameof(InvokeStaticMethod), });
    }

    private static void CreateDelegate(Assembly assembly)
    {
        var entryType = assembly.GetType("HotUpdateEntry");
        var method = entryType.GetMethod("Main");
        var mainFunc = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), method);
        mainFunc(nameof(CreateDelegate));
    }

    private static void CreateInterface(Assembly assembly)
    {
        var entryType = assembly.GetType("HotUpdateEntry");
        var entry = (IEntry)Activator.CreateInstance(entryType);
        entry.Run(nameof(CreateInterface));
    }

    private const string kPrefabsName = "prefabs";

    private static IEnumerator LoadAssetBundle()
    {
        var path = GetPath(kPrefabsName);

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

    private const string kLocalUrl = "http://172.18.13.106:8000/";
    private static string GetPath(string fileName)
    {
        return Path.Combine(kLocalUrl, fileName);
        // return Path.Combine(Application.streamingAssetsPath, fileName);
    }
}