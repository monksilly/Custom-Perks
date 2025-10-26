using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CustomPerks.Config;

[Serializable]
public class PerkCollectionConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string collectionName;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string author;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string version;
    
    public List<PerkConfig> perks;
}

