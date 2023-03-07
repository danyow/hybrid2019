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