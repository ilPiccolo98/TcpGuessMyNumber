using Newtonsoft.Json;

namespace GuessMyNumberServer
{
    internal class Packet
    {
        public Packet(string command, string message)
        {
            Command = command;
            Message = message;
        }

        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        public override string ToString()
        {
            return $"[Message = {Message}; Command = {Command}];";
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Packet FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Packet>(json);
        }
    }
}
