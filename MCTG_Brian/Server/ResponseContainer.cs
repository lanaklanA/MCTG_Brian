using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace MCTG_Brian.Server
{
    public class ResponseContainer
    {
        public int Status { get; set; }
        public string ContentType { get; set; }
        public string Body { get; set; }

        
        public ResponseContainer(int statusCode, string contentType, string body)
        {
            Status = statusCode;
            ContentType = contentType;
            Body = body;
        }

        public static string createBody(string? info, Object obj) {
            var jsonSerializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            string hans = JsonConvert.SerializeObject(obj, jsonSerializerSettings);



            return $"{info}\n\n{hans}\n";

        }
        public string HttpResponseToString()
        {
            return $"HTTP/1.1 {Status} OK\r\nContent-Type: {ContentType}\r\nContent-Length: {Body.Length}\r\n\r\n{Body}";
        }

        public void sendClient(NetworkStream stream, string data)
        {
            byte[] responseBuffer = Encoding.ASCII.GetBytes(data);
            stream.Write(responseBuffer, 0, responseBuffer.Length);
            stream.Close();
        }
    }
}
