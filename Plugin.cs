using BepInEx;
using CustomPerks.Controllers;
using CustomPerks.Patches;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using WKLib.Core;

namespace CustomPerks;

[BepInDependency("WKLib")]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance;
    public ModContext Context;
    
    public GameObject stuffHolder;

    
    private bool _isObjectInitialized;
    private bool _isPerkControllerInitialized;
    
    private void Awake()
    {
        Instance = this;
        
        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
        
        Context = ModRegistry.Register(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION);
        
        LogManager.Init(Logger);
        LogManager.Info($"Plugin {MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }



    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupMainObject();

        switch (scene.name)
        {
            case "Intro":
                SetupPerkController();
                break;
        }
    }

    private void SetupMainObject()
    {
        if (_isObjectInitialized) return;
        _isObjectInitialized = true;
        
        stuffHolder = new GameObject("CustomPerksManager");
        DontDestroyOnLoad(stuffHolder);
    }

    private void SetupPerkController()
    {
        if (_isPerkControllerInitialized) return;
        _isPerkControllerInitialized = true;
        
        stuffHolder.AddComponent<PerkController>();
    }
}
