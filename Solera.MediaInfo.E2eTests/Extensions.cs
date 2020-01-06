using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Solera.MediaInfo.E2eTests
{
    public static class Extensions
    {
        public static async Task<JToken> ReadAsJSONAsync(this HttpContent content)
        {
            var contentString = await content.ReadAsStringAsync();
            return JToken.Parse(contentString);
        }
    }
}
