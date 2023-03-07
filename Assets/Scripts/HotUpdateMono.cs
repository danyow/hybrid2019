using UnityEngine;
using UnityEngine.UI;

public class HotUpdateMono: MonoBehaviour
{
    [SerializeField]
    private Text text;

    private void Start()
    {
        text.text = "[HotUpdateMono Start] Hello, HybridCLR from Start";

        Debug.Log("text.text");
    }
}