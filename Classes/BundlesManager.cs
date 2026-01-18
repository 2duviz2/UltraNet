using BepInEx;
using UnityEngine;
using System.IO;
using System.Reflection;
using UltraNet;

public class BundlesManager : MonoBehaviour
{
    public static AssetBundle netBundle;

    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        var assembly = Assembly.GetExecutingAssembly();

        string resourceName = "UltraNet.Assets.ultranet.bundle";

        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                Plugin.LogError($"Embedded resource '{resourceName}' not found!");
                return;
            }

            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            netBundle = AssetBundle.LoadFromMemory(data);
            if (netBundle != null)
                Plugin.LogInfo("Loaded embedded AssetBundle!");
            else
                Plugin.LogError("Failed to load AssetBundle from memory!");
        }
    }

    public void OnDestroy()
    {
        netBundle?.Unload(false);
    }

}