using Newtonsoft.Json;

namespace MeteredPDUWebNew.SNMP;

[JsonObject]
public class OidParameters
{
    [JsonProperty("amperage_oid")] public string AmperageOid;
    [JsonProperty("voltage_oid")] public string VoltageOid;
    [JsonProperty("power_oid")] public string PowerOid;
    [JsonProperty("reactive_power_oid")] public string ReactivePowerOid;
}