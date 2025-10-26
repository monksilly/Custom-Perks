using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CustomPerks.Config;

[Serializable]
public class PerkConfig
{
    public string title;
    public string id;
    public string description;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string flavorText;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string author;
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string perkType;
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool competitive = true;
    public int cost;
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string spawnPool;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool spawnInEndless = true;
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool canStack = false;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int stackMax = 1;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<float> multiplierCurveKeys;
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public BuffConfig buff;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public BuffConfig baseBuff;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public float buffMultiplier = 1f;
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<string> flags;
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<PerkModuleConfig> modules;
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string icon;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string perkCard;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string perkFrame;
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string unlockProgressionID;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int unlockXP = 0;
}

[Serializable]
public class BuffConfig
{
    public string id;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string desc;
    public List<BuffStatConfig> buffs;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public float loseRate = 0.1f;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool loseRateEffectedByPerks = true;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool loseOverTime = true;
}

[Serializable]
public class BuffStatConfig
{
    public string id;
    public float maxAmount;
}

[Serializable]
public class PerkModuleConfig
{
    public string type;
    public string name;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, object> parameters;
}

