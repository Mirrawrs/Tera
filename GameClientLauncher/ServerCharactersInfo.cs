using Newtonsoft.Json;

namespace Tera.Launcher
{
    public class ServerCharactersInfo
    {
        [JsonProperty("id")]
        public string ServerId { get; set; }

        [JsonProperty("char_count")]
        public string CharactersCount { get; set; }
    }
}