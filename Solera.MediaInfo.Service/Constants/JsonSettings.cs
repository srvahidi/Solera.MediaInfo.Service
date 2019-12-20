using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Solera.MediaInfo.Service.Constants
{
    public static class JsonSettings
    {
        private static JsonSerializerSettings camelCaseSettings;

        public static JsonSerializerSettings CamelCase
        {
            get
            {
                if (camelCaseSettings == null)
                {
                    camelCaseSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new CamelCaseNamingStrategy()
                        }
                    };
                }
                return camelCaseSettings;
            }
        }
    }
}
