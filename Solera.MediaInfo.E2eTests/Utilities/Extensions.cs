using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Net.Http;
using System.Threading.Tasks;

namespace Solera.MediaInfo.E2eTests.Utilities
{
    public static class Extensions
    {
        public static async Task<JToken> ReadAsJsonAsync(this HttpContent content)
        {
            var contentString = await content.ReadAsStringAsync();
            return JToken.Parse(contentString);
        }

        public static StringContent ToJsonStringContent(this object obj)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };
            return new StringContent(JsonConvert.SerializeObject(obj, settings), System.Text.Encoding.Default, "application/json");
        }
    }
}
