using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
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

        /// <summary>
        /// Creates the body of the response
        /// </summary>
        /// <param name="info"></param>
        /// <param name="obj"></param>
        /// <param name="proto"></param>
        /// <returns></returns>
        public static string createBody(string? info, Object obj, bool proto) {

            if(proto)
            {
                StringBuilder sb = new StringBuilder();
                List<string> list = obj as List<string>;

                foreach (string str in list)
                {
                    sb.AppendLine(str);
                }
                return $"{info}\n\n{sb}\n";
            }

            var jsonSerializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            string? output = JsonConvert.SerializeObject(obj, jsonSerializerSettings);

            if (output == "null") output = null;
            

            return $"{info}\n\n{output}\n";

        }
        
        /// <summary>
        /// Creates an synatx und semantik corret HttpRepsonse
        /// </summary>
        /// <returns></returns>
        public string HttpResponseToString()
        {
            return $"HTTP/1.1 {Status} OK\r\nContent-Type: {ContentType}\r\nContent-Length: {Body.Length}\r\n\r\n{Body}";
        }
        
        /// <summary>
        /// Sends an HttpReponse in Type string to the client
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="data"></param>
        public void sendClient(NetworkStream stream, string data)
        {
            byte[] responseBuffer = Encoding.ASCII.GetBytes(data);
            stream.Write(responseBuffer, 0, responseBuffer.Length);
            stream.Close();
        }
    }
}
