using System;
using System.Collections.Generic;
using CustomPerks.Config;
using CustomPerks.PerkModules;
using CustomPerks.Patches;
using UnityEngine;

namespace CustomPerks.Factories;

public static class PerkModuleFactory
{
    public static PerkModule CreateModule(PerkModuleConfig config)
    {
        if (string.IsNullOrEmpty(config.type))
        {
            Debug.LogError($"[PerkModuleFactory] Module type is null or empty for module: {config.name}");
            return null;
        }

        try
        {
            return config.type.ToLower() switch
            {
                "removaltimer" => CreateRemovalTimerModule(config),
                "consumebuff" => CreateConsumeBuffModule(config),
                "roachbanker" => CreateRoachBankerModule(config),
                "bufffrominventory" => CreateBuffFromInventoryModule(config),
                "holddrop" => CreateHoldDropModule(config),
                "autoconsumer" => CreateAutoConsumerModule(config),
                "fallbuff" => CreateFallBuffModule(config),
                "hungermeter" => CreateHungerMeterModule(config),
                "jazzhand" => CreateJazzHandModule(config),
                "masscontroller" => CreateMassControllerModule(config),
                "objectspawner" => CreateObjectSpawnerModule(config),
                "itemremover" => CreateItemRemoverModule(config),
                "itemtransformer" => CreateItemTransformerModule(config),
                "ondamage" => CreateOnDamageModule(config),
                _ => CreateGenericModule(config)
            };
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PerkModuleFactory] Failed to create module {config.type}: {ex.Message}");
            return null;
        }
    }

    private static PerkModule_RemovalTimer CreateRemovalTimerModule(PerkModuleConfig config)
    {
        var module = new PerkModule_RemovalTimer();
        module.name = config.name ?? "RemovalTimer";
        
        if (config.parameters != null)
        {
            if (config.parameters.TryGetValue("removeTime", out var removeTimeObj) && 
                float.TryParse(removeTimeObj.ToString(), out var removeTime))
            {
                module.removeTime = removeTime;
            }
            else
            {
                module.removeTime = 30f;
            }
        }
        else
        {
            module.removeTime = 30f;
        }

        return module;
    }

    private static PerkModule_ConsumeBuff CreateConsumeBuffModule(PerkModuleConfig config)
    {
        var module = new PerkModule_ConsumeBuff();
        module.name = config.name ?? "ConsumeBuff";
        
        if (config.parameters != null)
        {
            if (config.parameters.TryGetValue("consumeBuffIDs", out var buffIdsObj) && 
                buffIdsObj is List<object> buffIdsList)
            {
                module.consumeBuffIDs = new List<string>();
                foreach (var id in buffIdsList)
                {
                    module.consumeBuffIDs.Add(id.ToString());
                }
            }
            else
            {
                module.consumeBuffIDs = new List<string> { "food" };
            }

            if (config.parameters.TryGetValue("addExtraJumpOnConsume", out var jumpObj) && 
                int.TryParse(jumpObj.ToString(), out var jumps))
            {
                module.addExtraJumpOnConsume = jumps;
            }

            if (config.parameters.TryGetValue("canStack", out var stackObj) && 
                bool.TryParse(stackObj.ToString(), out var canStack))
            {
                module.canStack = canStack;
            }

            if (config.parameters.TryGetValue("audioVolume", out var volumeObj) && 
                float.TryParse(volumeObj.ToString(), out var volume))
            {
                module.audioVolume = volume;
            }
        }

        return module;
    }

    private static PerkModule_RoachBanker CreateRoachBankerModule(PerkModuleConfig config)
    {
        var module = new PerkModule_RoachBanker();
        module.name = config.name ?? "RoachBanker";
        
        if (config.parameters != null)
        {
            if (config.parameters.TryGetValue("itemTag", out var tagObj))
            {
                module.itemTag = tagObj.ToString();
            }

            if (config.parameters.TryGetValue("bagOnly", out var bagObj) && 
                bool.TryParse(bagObj.ToString(), out var bagOnly))
            {
                module.bagOnly = bagOnly;
            }
        }

        return module;
    }

    private static PerkModule_BuffFromInventory CreateBuffFromInventoryModule(PerkModuleConfig config)
    {
        var module = new PerkModule_BuffFromInventory();
        module.name = config.name ?? "BuffFromInventory";
        
        if (config.parameters != null)
        {
            if (config.parameters.TryGetValue("itemTag", out var tagObj))
            {
                module.itemTag = tagObj.ToString();
            }

            if (config.parameters.TryGetValue("itemName", out var nameObj))
            {
                PerkModule_BuffFromInventory_Update_Patch.SetItemName(module, nameObj.ToString());
            }

            if (config.parameters.TryGetValue("buffMultiplier", out var multObj) && 
                float.TryParse(multObj.ToString(), out var multiplier))
            {
                module.buffMultiplier = multiplier;
            }

            if (config.parameters.TryGetValue("useCurve", out var curveObj) && 
                bool.TryParse(curveObj.ToString(), out var useCurve))
            {
                module.useCurve = useCurve;
            }

            if (config.parameters.TryGetValue("curveMax", out var maxObj) && 
                int.TryParse(maxObj.ToString(), out var curveMax))
            {
                module.curveMax = curveMax;
            }

            if (config.parameters.TryGetValue("includeHands", out var handsObj) && 
                bool.TryParse(handsObj.ToString(), out var includeHands))
            {
                module.includeHands = includeHands;
            }

            if (config.parameters.TryGetValue("buff", out var buffObj) && buffObj is Dictionary<string, object> buffDict)
            {
                module.buff = CreateBuffContainerFromDict(buffDict);
            }
        }

        return module;
    }

    private static PerkModule_HoldDrop CreateHoldDropModule(PerkModuleConfig config)
    {
        var module = new PerkModule_HoldDrop();
        module.name = config.name ?? "HoldDrop";
        
        if (config.parameters != null)
        {
            if (config.parameters.TryGetValue("itemTagToRemove", out var tagObj))
            {
                module.itemTagToRemove = tagObj.ToString();
            }
        }

        return module;
    }

    private static PerkModule CreateGenericModule(PerkModuleConfig config)
    {
        var module = new PerkModule();
        module.name = config.name ?? config.type;
        return module;
    }

    private static BuffContainer CreateBuffContainerFromDict(Dictionary<string, object> buffDict)
    {
        var buff = new BuffContainer();
        
        if (buffDict.TryGetValue("id", out var idObj))
            buff.id = idObj.ToString();
        
        if (buffDict.TryGetValue("desc", out var descObj))
            buff.desc = descObj.ToString();
        
        if (buffDict.TryGetValue("loseRate", out var rateObj) && float.TryParse(rateObj.ToString(), out var rate))
            buff.loseRate = rate;
        
        if (buffDict.TryGetValue("loseRateEffectedByPerks", out var effectedObj) && bool.TryParse(effectedObj.ToString(), out var effected))
            buff.loseRateEffectedByPerks = effected;
        
        if (buffDict.TryGetValue("loseOverTime", out var overTimeObj) && bool.TryParse(overTimeObj.ToString(), out var overTime))
            buff.loseOverTime = overTime;

        if (buffDict.TryGetValue("buffs", out var buffsObj) && buffsObj is List<object> buffsList)
        {
            buff.buffs = new List<BuffContainer.Buff>();
            foreach (var buffObj in buffsList)
            {
                if (buffObj is Dictionary<string, object> buffItemDict)
                {
                    var buffItem = new BuffContainer.Buff();
                    if (buffItemDict.TryGetValue("id", out var buffIdObj))
                        buffItem.id = buffIdObj.ToString();
                    if (buffItemDict.TryGetValue("maxAmount", out var amountObj) && float.TryParse(amountObj.ToString(), out var amount))
                        buffItem.maxAmount = amount;
                    buff.buffs.Add(buffItem);
                }
            }
        }

        return buff;
    }


    private static PerkModule_AutoConsumer CreateAutoConsumerModule(PerkModuleConfig config)
    {
        var module = new PerkModule_AutoConsumer();
        module.name = config.name ?? "AutoConsumer";
        
        if (config.parameters != null)
        {
            if (config.parameters.TryGetValue("consumptionReason", out var reasonObj) && 
                Enum.TryParse<PerkModule_AutoConsumer.ConsumptionReason>(reasonObj.ToString(), out var reason))
            {
                module.consumptionReason = reason;
            }
            
            if (config.parameters.TryGetValue("itemTag", out var tagObj))
            {
                module.itemTag = tagObj.ToString();
            }
            
            if (config.parameters.TryGetValue("restoreGrip", out var gripObj) && 
                float.TryParse(gripObj.ToString(), out var restoreGrip))
            {
                module.restoreGrip = restoreGrip;
            }
            
            if (config.parameters.TryGetValue("buffDecayRate", out var decayObj) && 
                float.TryParse(decayObj.ToString(), out var decayRate))
            {
                module.buffDecayRate = decayRate;
            }
            
            if (config.parameters.TryGetValue("consumeShake", out var shakeObj) && 
                float.TryParse(shakeObj.ToString(), out var shake))
            {
                module.consumeShake = shake;
            }
            
            if (config.parameters.TryGetValue("invokeOnEat", out var eatObj) && 
                bool.TryParse(eatObj.ToString(), out var invokeOnEat))
            {
                module.invokeOnEat = invokeOnEat;
            }
            
            if (config.parameters.TryGetValue("buff", out var buffObj) && buffObj is Dictionary<string, object> buffDict)
            {
                module.buff = CreateBuffContainerFromDict(buffDict);
            }
        }
        
        return module;
    }

    private static PerkModule_FallBuff CreateFallBuffModule(PerkModuleConfig config)
    {
        var module = new PerkModule_FallBuff();
        module.name = config.name ?? "FallBuff";
        
        if (config.parameters != null)
        {
            if (config.parameters.TryGetValue("buffMultiplier", out var multObj) && 
                float.TryParse(multObj.ToString(), out var multiplier))
            {
                module.buffMultiplier = multiplier;
            }
            
            if (config.parameters.TryGetValue("fallMin", out var minObj) && 
                float.TryParse(minObj.ToString(), out var fallMin))
            {
                module.fallMin = fallMin;
            }
            
            if (config.parameters.TryGetValue("fallMax", out var maxObj) && 
                float.TryParse(maxObj.ToString(), out var fallMax))
            {
                module.fallMax = fallMax;
            }
            
            if (config.parameters.TryGetValue("buffDecayRate", out var decayObj) && 
                float.TryParse(decayObj.ToString(), out var decayRate))
            {
                module.buffDecayRate = decayRate;
            }
            
            if (config.parameters.TryGetValue("buff", out var buffObj) && buffObj is Dictionary<string, object> buffDict)
            {
                module.buff = CreateBuffContainerFromDict(buffDict);
            }
        }
        
        return module;
    }

    private static PerkModule_MassController CreateMassControllerModule(PerkModuleConfig config)
    {
        var module = new PerkModule_MassController();
        module.name = config.name ?? "MassController";
        
        if (config.parameters != null)
        {
            if (config.parameters.TryGetValue("massMult", out var massObj) && 
                float.TryParse(massObj.ToString(), out var massMult))
            {
                module.massMult = massMult;
            }
        }
        
        return module;
    }

    private static PerkModule_ItemRemover CreateItemRemoverModule(PerkModuleConfig config)
    {
        var module = new PerkModule_ItemRemover();
        module.name = config.name ?? "ItemRemover";
        
        if (config.parameters != null)
        {
            if (config.parameters.TryGetValue("itemTagToRemove", out var tagObj))
            {
                module.itemTagToRemove = tagObj.ToString();
            }
        }
        
        return module;
    }

    private static PerkModule_HungerMeter CreateHungerMeterModule(PerkModuleConfig config)
    {
        var module = new PerkModule_HungerMeter();
        module.name = config.name ?? "HungerMeter";
        
        if (config.parameters != null)
        {
            if (config.parameters.TryGetValue("consumeBuffIDs", out var buffIdsObj) && buffIdsObj is List<object> buffIdsList)
            {
                module.consumeBuffIDs = buffIdsList.ConvertAll(x => x.ToString());
            }
            
            if (config.parameters.TryGetValue("hungerMax", out var maxObj) && 
                float.TryParse(maxObj.ToString(), out var hungerMax))
            {
                module.hungerMax = hungerMax;
            }
            
            if (config.parameters.TryGetValue("hungerDecayRate", out var decayObj) && 
                float.TryParse(decayObj.ToString(), out var decayRate))
            {
                module.hungerDecayRate = decayRate;
            }
            
            if (config.parameters.TryGetValue("eatRecovery", out var recoveryObj) && 
                float.TryParse(recoveryObj.ToString(), out var recovery))
            {
                module.eatRecovery = recovery;
            }
            
            if (config.parameters.TryGetValue("buff", out var buffObj) && buffObj is Dictionary<string, object> buffDict)
            {
                module.buff = CreateBuffContainerFromDict(buffDict);
            }
            
            if (config.parameters.TryGetValue("debuff", out var debuffObj) && debuffObj is Dictionary<string, object> debuffDict)
            {
                module.debuff = CreateBuffContainerFromDict(debuffDict);
            }
        }

        return module;
    }

    private static PerkModule_JazzHand CreateJazzHandModule(PerkModuleConfig config)
    {
        var module = new PerkModule_JazzHand();
        module.name = config.name ?? "JazzHand";
        
        if (config.parameters != null)
        {
            if (config.parameters.TryGetValue("restoreGrip", out var gripObj) && 
                float.TryParse(gripObj.ToString(), out var restoreGrip))
            {
                module.restoreGrip = restoreGrip;
            }
            
            if (config.parameters.TryGetValue("useBuff", out var useBuffObj) && 
                bool.TryParse(useBuffObj.ToString(), out var useBuff))
            {
                module.useBuff = useBuff;
            }
            
            if (config.parameters.TryGetValue("buffMaxMultiplier", out var maxMultObj) && 
                float.TryParse(maxMultObj.ToString(), out var maxMult))
            {
                module.buffMaxMultiplier = maxMult;
            }
            
            if (config.parameters.TryGetValue("buffMultiplierIncreaseAmount", out var increaseObj) && 
                float.TryParse(increaseObj.ToString(), out var increase))
            {
                module.buffMultiplierIncreaseAmount = increase;
            }
            
            if (config.parameters.TryGetValue("buffDecayRate", out var decayObj) && 
                float.TryParse(decayObj.ToString(), out var decayRate))
            {
                module.buffDecayRate = decayRate;
            }
            
            if (config.parameters.TryGetValue("jazzVolume", out var volumeObj) && 
                float.TryParse(volumeObj.ToString(), out var volume))
            {
                module.jazzVolume = volume;
            }
            
            if (config.parameters.TryGetValue("buff", out var buffObj) && buffObj is Dictionary<string, object> buffDict)
            {
                module.buff = CreateBuffContainerFromDict(buffDict);
            }
        }
        
        return module;
    }

    private static PerkModule_ObjectSpawner CreateObjectSpawnerModule(PerkModuleConfig config)
    {
        var module = new PerkModule_ObjectSpawner();
        module.name = config.name ?? "ObjectSpawner";
        
        if (config.parameters != null)
        {
            if (config.parameters.TryGetValue("spawnFrequency", out var freqObj) && 
                float.TryParse(freqObj.ToString(), out var frequency))
            {
                module.spawnFrequency = frequency;
            }
            
            if (config.parameters.TryGetValue("numberToSpawn", out var numObj) && 
                int.TryParse(numObj.ToString(), out var number))
            {
                module.numberToSpawn = number;
            }
            
            if (config.parameters.TryGetValue("minimumDistance", out var distObj) && 
                float.TryParse(distObj.ToString(), out var distance))
            {
                module.minimumDistance = distance;
            }
            
            if (config.parameters.TryGetValue("checkStepDistance", out var stepObj) && 
                float.TryParse(stepObj.ToString(), out var stepDistance))
            {
                module.checkStepDistance = stepDistance;
            }
            
            if (config.parameters.TryGetValue("hitOffset", out var offsetObj) && 
                float.TryParse(offsetObj.ToString(), out var offset))
            {
                module.hitOffset = offset;
            }
        }

        return module;
    }

    private static PerkModule_ItemTransformer CreateItemTransformerModule(PerkModuleConfig config)
    {
        var module = new PerkModule_ItemTransformer();
        module.name = config.name ?? "ItemTransformer";
        
        if (config.parameters != null)
        {
            if (config.parameters.TryGetValue("transformToItemID", out var itemIdObj))
            {
                module.transformToItemID = itemIdObj.ToString();
            }
            
            if (config.parameters.TryGetValue("excludedItemNames", out var excludedObj) && excludedObj is List<object> excludedList)
            {
                module.excludedItemNames = excludedList.ConvertAll(x => x.ToString());
            }
            
            if (config.parameters.TryGetValue("preservePosition", out var preserveObj) && 
                bool.TryParse(preserveObj.ToString(), out var preserve))
            {
                module.preservePosition = preserve;
            }
        }

        return module;
    }

    private static PerkModule_OnDamage CreateOnDamageModule(PerkModuleConfig config)
    {
        var module = new PerkModule_OnDamage();
        module.name = config.name ?? "OnDamage";
        
        if (config.parameters != null)
        {
            if (config.parameters.TryGetValue("applyBuffOnHit", out var buffObj))
            {
                module.applyBuffOnHit = buffObj.ToString();
            }
            
            if (config.parameters.TryGetValue("buffDuration", out var durationObj) &&
                float.TryParse(durationObj.ToString(), out var duration))
            {
                module.buffDuration = duration;
            }
            
            if (config.parameters.TryGetValue("buffAmount", out var amountObj) &&
                float.TryParse(amountObj.ToString(), out var amount))
            {
                module.buffAmount = amount;
            }
            
            if (config.parameters.TryGetValue("healAmount", out var healObj) &&
                float.TryParse(healObj.ToString(), out var heal))
            {
                module.healAmount = heal;
            }
            
            if (config.parameters.TryGetValue("dropAllItems", out var dropObj) &&
                bool.TryParse(dropObj.ToString(), out var drop))
            {
                module.dropAllItems = drop;
            }
            
            if (config.parameters.TryGetValue("damageTypeFilter", out var filterObj))
            {
                module.damageTypeFilter = filterObj.ToString();
            }
        }
        
        return module;
    }
}
