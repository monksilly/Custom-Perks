using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CustomPerks;
using CustomPerks.Config;
using CustomPerks.Factories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using WKLib.Assets;
using WKLib.Perks.Builders;

namespace CustomPerks.Controllers;

public class PerkController : MonoBehaviour
{
    public static PerkController Instance;
    public string currentScene;
    
    private AssetService _assetService;
    private PerkBuilder _perkBuilder;
    private static readonly Dictionary<string, string> _builtInFrameFileMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "basic", "PerkCard_Frame_Basic.png" },
        { "advanced", "PerkCard_Frame_Advanced.png" },
        { "experimental", "PerkCard_Frame_Experimental.png" },
        { "item", "PerkCard_Frame_Item.png" },
        { "timed", "PerkCard_Frame_Timed.png" }
    };

    private const string ConfigFileName = "config.json";
    private const string PerksRoot = "Perks";

    private readonly List<Perk> _customPerks = new();
    private string _customRoot;

    private void Awake()
    {
        if (Instance is null || Instance != this)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        
        LogManager.Info("[PerkController] Awake()");
            
        _assetService = new AssetService(Plugin.Instance.Context);
        _perkBuilder = new PerkBuilder();

        var customRoot = Path.Combine(BepInEx.Paths.PluginPath, PerksRoot);
        if (!Directory.Exists(customRoot))
            Directory.CreateDirectory(customRoot);
        _customRoot = customRoot;
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;
        if (scene.name != "Main-Menu") return;
        SetupPerks();
        gameObject.SetActive(true);
    }

    private async void SetupPerks()
    {
        LogManager.Info("[PerkController] Setting up custom perks!");
        
        var folders = Directory.GetDirectories(_customRoot);
        foreach (var folder in folders)
        {
            await ProcessFolder(folder);
        }
        
        RegisterCustomPerks();
    }

    private async Task ProcessFolder(string folderPath)
    {
        var configPath = Path.Combine(folderPath, ConfigFileName);
        
        // If this folder has a config.json, it's a perk
        if (File.Exists(configPath))
        {
            await ProcessPerkFolder(folderPath);
            return;
        }
        
        // Otherwise, check subfolders for perks
        var subFolders = Directory.GetDirectories(folderPath);
        foreach (var subFolder in subFolders)
        {
            var subConfigPath = Path.Combine(subFolder, ConfigFileName);
            if (File.Exists(subConfigPath))
            {
                await ProcessPerkFolder(subFolder);
            }
        }
    }

    private async Task ProcessPerkFolder(string folderPath)
    {
        var configPath = Path.Combine(folderPath, ConfigFileName);
        if (!File.Exists(configPath))
        {
            return;
        }

        string jsonText;
        try
        {
            jsonText = await File.ReadAllTextAsync(configPath);
        }
        catch (Exception e)
        {
            LogManager.Error($"[PerkLoader] Failed reading {configPath}: {e.Message}");
            return;
        }

        JObject root;
        try
        {
            root = JObject.Parse(jsonText);
        }
        catch (Exception e)
        {
            LogManager.Error($"[PerkLoader] Invalid JSON in {configPath}: {e.Message}");
            return;
        }
        
        if (root["perks"] != null)
        {
            await HandlePerkCollection(folderPath, root);
        }
        else if (root["id"] != null && root["title"] != null)
        {
            await HandleSinglePerk(folderPath, root);
        }
        else
        {
            LogManager.Error($"[PerkLoader] Unrecognized config format in {configPath}");
        }
    }

    private async Task HandleSinglePerk(string folderPath, JObject root)
    {
        try
        {
            PerkConfig cfg = root.ToObject<PerkConfig>();
            if (cfg == null)
            {
                LogManager.Error($"[PerkLoader] Failed to deserialize perk config in {folderPath}");
                return;
            }

            var assetsFolder = Path.Combine(folderPath, "Assets");
            var perk = await BuildPerkFromConfig(cfg, assetsFolder);
            
            if (perk != null)
            {
                _customPerks.Add(perk);
                LogManager.Info($"[PerkLoader] Loaded custom perk: {cfg.title} (ID: {cfg.id})");
            }
        }
        catch (Exception e)
        {
            LogManager.Error($"[PerkLoader] Error processing single perk in {folderPath}: {e.Message}");
        }
    }

    private async Task HandlePerkCollection(string folderPath, JObject root)
    {
        try
        {
            PerkCollectionConfig cfg = root.ToObject<PerkCollectionConfig>();
            if (cfg == null || cfg.perks == null)
            {
                LogManager.Error($"[PerkLoader] Failed to deserialize perk collection in {folderPath}");
                return;
            }

            LogManager.Info($"[PerkLoader] Loading perk collection: {cfg.collectionName ?? "Unnamed"} by {cfg.author ?? "Unknown"}");

            var assetsFolder = Path.Combine(folderPath, "Assets");
            foreach (var perkConfig in cfg.perks)
            {
                var perk = await BuildPerkFromConfig(perkConfig, assetsFolder);
                if (perk != null)
                {
                    _customPerks.Add(perk);
                    LogManager.Info($"[PerkLoader] Loaded custom perk: {perkConfig.title} (ID: {perkConfig.id})");
                }
            }
        }
        catch (Exception e)
        {
            LogManager.Error($"[PerkLoader] Error processing perk collection in {folderPath}: {e.Message}");
        }
    }

    private async Task<Perk> BuildPerkFromConfig(PerkConfig cfg, string assetsFolder)
    {
        try
        {
            var builder = new PerkBuilder()
                .WithTitle(cfg.title)
                .WithID(cfg.id)
                .WithDescription(cfg.description)
                .WithCost(cfg.cost);

            // Build flavor text alongside  author name(optional)
            if (!string.IsNullOrEmpty(cfg.flavorText))
            {
                var flavorText = cfg.flavorText;
                if (!string.IsNullOrEmpty(cfg.author))
                {
                    flavorText += $"\n\nCreated by {cfg.author}";
                }
                builder.WithFlavorText(flavorText);
            }
            else if (!string.IsNullOrEmpty(cfg.author))
            {
                builder.WithFlavorText($"Created by {cfg.author}");
            }

            if (!string.IsNullOrEmpty(cfg.perkType))
                builder.WithPerkType(ParsePerkType(cfg.perkType));

            builder.IsCompetitive(cfg.competitive);

            if (!string.IsNullOrEmpty(cfg.spawnPool))
                builder.WithSpawnPool(ParseSpawnPool(cfg.spawnPool));

            builder.SpawnInEndless(cfg.spawnInEndless);
            builder.CanStack(cfg.canStack);
            builder.WithStackMax(cfg.stackMax);

            if (cfg.multiplierCurveKeys != null && cfg.multiplierCurveKeys.Count > 0)
            {
                var curve = BuildAnimationCurve(cfg.multiplierCurveKeys);
                builder.WithMultiplierCurve(curve);
            }

            if (cfg.buff != null)
            {
                var buffContainer = BuildBuffContainer(cfg.buff);
                builder.WithBuff(buffContainer);
            }

            if (cfg.baseBuff != null)
            {
                var baseBuffContainer = BuildBuffContainer(cfg.baseBuff);
                builder.WithBaseBuff(baseBuffContainer);
            }

            builder.WithBuffMultiplier(cfg.buffMultiplier);

            if (cfg.flags != null && cfg.flags.Count > 0)
                builder.WithFlags(cfg.flags);

            if (cfg.modules != null && cfg.modules.Count > 0)
            {
                var modules = BuildPerkModules(cfg.modules);
                builder.WithModules(modules);
            }

            if (!string.IsNullOrEmpty(cfg.icon) && Directory.Exists(assetsFolder))
            {
                var iconPath = Path.Combine(assetsFolder, cfg.icon);
                var iconSprite = _assetService.LoadPngAsSprite(iconPath);
                if (iconSprite != null)
                    builder.WithIcon(iconSprite);
            }

            if (!string.IsNullOrEmpty(cfg.perkCard) && Directory.Exists(assetsFolder))
            {
                var cardPath = Path.Combine(assetsFolder, cfg.perkCard);
                var cardSprite = _assetService.LoadPngAsSprite(cardPath);
                if (cardSprite != null)
                    builder.WithPerkCard(cardSprite);
            }

            if (!string.IsNullOrEmpty(cfg.perkFrame))
            {
                if (_builtInFrameFileMap.TryGetValue(cfg.perkFrame, out var builtInFileName))
                {
                    var framePath = Path.Combine(_customRoot, "Frames", builtInFileName);
                    var frameSprite = _assetService.LoadPngAsSprite(framePath);
                    if (frameSprite != null)
                        builder.WithPerkFrame(frameSprite);
                }
                else
                {
                    if (Directory.Exists(assetsFolder))
                    {
                        var framePath = Path.Combine(assetsFolder, cfg.perkFrame);
                        var frameSprite = _assetService.LoadPngAsSprite(framePath);
                        if (frameSprite != null)
                            builder.WithPerkFrame(frameSprite);
                        else
                            LogManager.Warning($"[PerkLoader] Could not load perkFrame '{cfg.perkFrame}' as path or key.");
                    }
                }
            }

            if (!string.IsNullOrEmpty(cfg.unlockProgressionID))
                builder.WithUnlockProgressionID(cfg.unlockProgressionID);

            builder.WithUnlockXP(cfg.unlockXP);

            return builder.Build();
        }
        catch (Exception e)
        {
            LogManager.Error($"[PerkLoader] Failed to build perk {cfg.title}: {e.Message}");
            return null;
        }
    }


    private Perk.PerkType ParsePerkType(string type)
    {
        return type.ToLower() switch
        {
            "standard" => Perk.PerkType.standard,
            "orange" => Perk.PerkType.orange,
            "red" => Perk.PerkType.red,
            "unstable" => Perk.PerkType.unstable,
            "peripheral" => Perk.PerkType.peripheral,
            "delta" => Perk.PerkType.delta,
            "rho" => Perk.PerkType.rho,
            _ => Perk.PerkType.standard
        };
    }

    private Perk.PerkPool ParseSpawnPool(string pool)
    {
        return pool.ToLower() switch
        {
            "standard" => Perk.PerkPool.standard,
            "unstable" => Perk.PerkPool.unstable,
            "never" => Perk.PerkPool.never,
            _ => Perk.PerkPool.standard
        };
    }

    private AnimationCurve BuildAnimationCurve(List<float> keys)
    {
        if (keys.Count < 2) return AnimationCurve.Linear(0, 1, 1, 1);
        
        var keyframes = new Keyframe[keys.Count];
        for (int i = 0; i < keys.Count; i++)
        {
            keyframes[i] = new Keyframe(i / (float)(keys.Count - 1), keys[i]);
        }
        
        return new AnimationCurve(keyframes);
    }

    private BuffContainer BuildBuffContainer(BuffConfig cfg)
    {
        var container = new BuffContainer
        {
            id = cfg.id,
            desc = cfg.desc ?? "",
            loseRate = cfg.loseRate,
            loseRateEffectedByPerks = cfg.loseRateEffectedByPerks,
            loseOverTime = cfg.loseOverTime,
            buffs = new List<BuffContainer.Buff>()
        };

        if (cfg.buffs != null)
        {
            foreach (var buffStat in cfg.buffs)
            {
                container.buffs.Add(new BuffContainer.Buff
                {
                    id = buffStat.id,
                    maxAmount = buffStat.maxAmount
                });
            }
        }

        return container;
    }

    private List<PerkModule> BuildPerkModules(List<PerkModuleConfig> moduleConfigs)
    {
        var modules = new List<PerkModule>();
        
        foreach (var moduleConfig in moduleConfigs)
        {
            var module = PerkModuleFactory.CreateModule(moduleConfig);
            if (module != null)
            {
                modules.Add(module);
                LogManager.Info($"[PerkLoader] Created module: {moduleConfig.type} ({moduleConfig.name})");
            }
            else
            {
                LogManager.Error($"[PerkLoader] Failed to create module: {moduleConfig.type} ({moduleConfig.name})");
            }
        }
        
        return modules;
    }

    private void RegisterCustomPerks()
    {
        if (_customPerks.Count == 0)
        {
            LogManager.Info("[PerkController] No custom perks to register");
            return;
        }

        try
        {
            CL_AssetManager.InitializeAssetManager();
            var baseDb = CL_AssetManager.GetBaseAssetDatabase();
            if (baseDb == null)
            {
                LogManager.Error("[PerkController] Base WKAssetDatabase is null! Cannot register perks.");
                return;
            }

            foreach (var perk in _customPerks)
            {
                var exists = baseDb.perkAssets != null && baseDb.perkAssets.Any(p => p != null && string.Equals(p.id, perk.id, StringComparison.OrdinalIgnoreCase));
                if (exists)
                {
                    LogManager.Debug($"[PerkController] Perk already present, skipping: {perk.id}");
                    continue;
                }

                baseDb.perkAssets.Add(perk);
                LogManager.Info($"[PerkController] Registered perk: {perk.title} (ID: {perk.id}) to asset database");
            }

            LogManager.Info($"[PerkController] Successfully registered {_customPerks.Count} custom perks!");
        }
        catch (Exception e)
        {
            LogManager.Error($"[PerkController] Failed to register custom perks: {e.Message}");
        }
    }
}

