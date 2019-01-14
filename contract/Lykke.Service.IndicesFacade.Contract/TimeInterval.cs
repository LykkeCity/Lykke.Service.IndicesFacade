using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.IndicesFacade.Contract
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TimeInterval
    {
        [EnumMember(Value = "Unspecified")] Unspecified,
        [EnumMember(Value = "Hour24Chart")] Hour24Chart,
        [EnumMember(Value = "Day5Chart")] Day5Chart,
        [EnumMember(Value = "Day30Chart")] Day30Chart
    }
}
