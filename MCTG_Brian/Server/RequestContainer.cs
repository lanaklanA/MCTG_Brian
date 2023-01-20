using Newtonsoft.Json.Linq;
using System.Xml.Linq;

namespace MCTG_Brian.Server
{
    public class RequestContainer
    {
        public string? Method { get; set; }
        public string? Path { get; set; }
        public string? Protocol { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public JArray? Body { get; set; }

        public RequestContainer(string data)
        {
            // Zerteile die Anfrage in Zeilen
            string[] lines = data.Split(new[] { "\r\n" }, StringSplitOptions.None);

            // Parse die erste Zeile als Methode, Pfad und Protokoll
            string[] firstLine = lines[0].Split(' ');
            string method = firstLine[0];
            string path = firstLine[1];
            string protocol = firstLine[2];

            //Erstellen eines neues Request-Objekt
            this.Method = method;
            this.Path = path;
            this.Protocol = protocol;
            this.Headers = new Dictionary<string, string>();

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

        public bool HasQueryParameter(string key, string value)
        {
            int queryStartIndex = Path.IndexOf("?");
            if (queryStartIndex == -1) return false;

            string queryString = Path.Substring(queryStartIndex + 1);
            string[] queryParams = queryString.Split("&");
            foreach (string queryParam in queryParams)
            {
                string[] keyValue = queryParam.Split("=");
                if (keyValue[0] == key && keyValue[1] == value)
                {
                    return true;
                }
            }
            return false;
        }

        public string? getToken()
        {
            return Headers.ContainsKey("Authorization") ? Headers["Authorization"].Replace("Basic ", "") : null;
        }
    }
}




