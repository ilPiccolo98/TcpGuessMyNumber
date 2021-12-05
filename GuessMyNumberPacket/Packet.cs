using Newtonsoft.Json;

namespace GuessMyNumberPacket
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
