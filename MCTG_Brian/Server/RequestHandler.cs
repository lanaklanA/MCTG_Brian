using Newtonsoft.Json.Linq;

namespace MCTG_Brian.Server
{
    class RequestHandler
    {
        public string? Method { get; set; }
        public string? Path { get; set; }
        public string? Protocol { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public JArray? Body { get; set; }

        public void ParseRequest(string data) //  public static dynamic ParseRequest(string data)
        {
            // Zerteile die Anfrage in Zeilen
            string[] lines = data.Split(new[] { "\r\n" }, StringSplitOptions.None);

            // Parse die erste Zeile als Methode, Pfad und Protokoll
            string[] firstLine = lines[0].Split(' ');
            string method = firstLine[0];
            string path = firstLine[1];
            string protocol = firstLine[2];

            //Erstellen eines neues Request-Objekt
            Method = method;
            Path = path;
            Protocol = protocol;
            Headers = new Dictionary<string, string>();

            // Parse die Header-Zeilen
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];

                // Wenn die Zeile leer ist, haben wir alle Header-Zeilen geparst und der Rest ist der Body
                if (string.IsNullOrEmpty(line))
                {
                    string jsonString = string.Join("\r\n", lines, i + 1, lines.Length - i - 1);

                    // Parse the JSON string comming from Body into a          
                    ParseBody(jsonString);
                    break;
                }

                // Sonst teile die Zeile in Schlüssel und Wert auf
                string[] header = lines[i].Split(": ");
                string key = header[0].Trim();
                string value = header[1].Trim();

                // Füge den Header dem Request-Objekt hinzu
                Headers.Add(key, value);
            }
        }

        public string HandleRequest()
        {
            switch (Path)
            {
                case "/users":
                    return "\nBody contains: i'm in users\n";
                case "/packages":
                    return "\nBody contains: i'm in packages\n";
                case "/cards":
                    return "\nBody contains: i'm in cards\n";
                case "/game":
                    return "\nBody contains: i'm in game\n";
                case "/trading":
                    return "\nBody contains: i'm in trading\n";
                default:
                    return "\nBody contains: Hi there, this is the content of my Body!\n";
            }

        }

        private void ParseBody(string HttpBody)
        {
            // Parsing empty string to valid JsonObject#
            string jsonString = HttpBody == "" ? "{}" : HttpBody;

            // Parse the JSON string into a JToken
            JToken json = JToken.Parse(jsonString);
            JArray jsonArray = new JArray();

            // Check if the JSON is an object or an array
            if (json.Type == JTokenType.Object)
            {
                jsonArray.Add(JObject.Parse(jsonString));
            }
            else if (json.Type == JTokenType.Array)
            {
                // JSON string is an array, so parse it directly into the JArray
                jsonArray = JArray.Parse(jsonString);
            }
            Body = jsonArray;
        }
    }
}




