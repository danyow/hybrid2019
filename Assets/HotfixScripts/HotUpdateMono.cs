using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HybridCLR;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HotUpdateMono: MonoBehaviour
{
    [SerializeField]
    private Text text;

    private IEnumerator Start()
    {
        text.text = "[HotUpdateMono Start] Hello, HybridCLR from Start\n\nHello, HybridCLR from Start";

        // text.text = text.text.ToUpper();
        text.text = text.text.ToLower();

        Debug.Log("text.text");

        var testInt = new MyClass<int> { a = 10, };
        text.text += testInt.a + "\n";

        var testFloat = new MyClass<float> { a = 7.777f, };
        text.text += testFloat.a + "\n";

        yield return null;

        yield return LoadMetadataForAOTAssemblies();
        
        var list = new List<MyIntVec3> { new MyIntVec3 { x = 1, }, };

        text.text += list.First().x + "\n";

        // var arr = Array.Empty<MyIntVec3>();
    }

    private static List<string> AOTMetaAssemblyNames{ get; } =
        new List<string> { "mscorlib.dll.bytes", "System.dll.bytes", "System.Core.dll.bytes", };

    private static IEnumerator LoadMetadataForAOTAssemblies()
    {
        // 不限补充元数据dll文件的读取方式，你可以从ab、StreamingAssets、或者裸文件下载等办法获得
        const HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach(var aotDllName in AOTMetaAssemblyNames)
        {
            yield return LoadAssembly(aotDllName, mode); // 获得某个aot dll文件所有字节
        }
    }

    private const string kLocalUrl = "http://172.18.13.106:8000/StreamingAssets/";

    private static string GetPath(string fileName)
    {
        return Path.Combine(kLocalUrl, fileName);
    }

    private static IEnumerator LoadAssembly(string dllName, HomologousImageMode mode)
    {
        Debug.Log(nameof(LoadAssembly));
        var path = GetPath(dllName);
        var request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();
        if(!request.isDone)
        {
            yield break;
        }
        var assembleData = request.downloadHandler.data;

        // var assembly = Assembly.Load(assembleData);
        // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
        var err = RuntimeApi.LoadMetadataForAOTAssembly(assembleData, mode);
        Debug.Log($"LoadMetadataForAOTAssembly:{dllName}. mode:{mode} ret:{err}");

        Debug.Log(assembleData);
    }

    public class MyClass
    {
        public int a;
    }

    public class MyClass<T>
    {
        public T a;
    }

    struct MyIntVec3
    {
        public int x;
        public int y;
        public int z;
    }
}