using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System.IO;

namespace MCTG_Brian.Server
{
    public class RestAPI
    {
        private readonly int port;
        private readonly IPAddress ipAddress;
        private TcpListener listener;
        RequestHandler rq;

        public RestAPI(string IPADDRESS, int PORT)
        {
            port = PORT;
            ipAddress = IPAddress.Parse(IPADDRESS);
            rq = new RequestHandler();
        }

        public void Start()
        {
            listener = new TcpListener(ipAddress, port);
            listener.Start();

            Console.WriteLine($"Server wartet am port {port} ...");

            while (true)
            {
                // Accept incoming connection
                TcpClient client = listener.AcceptTcpClient();

                // Start a new thread to handle the request
                Thread thread = new Thread(() => ThreadRequest(client));
                thread.Start();
            }

        }
        public void ThreadRequest(TcpClient client)
        {
            // Receive message
            NetworkStream stream = client.GetStream();
            string HttpRequest = ReceiveMessage(stream);

            // Handle message
            rq.ParseRequest(HttpRequest);
            printRequest();     // just there for debugging reason!

            string HttpResponse = rq.HandleRequest();

            // Send response
            SendMessage(stream, HttpResponse);

            client.Close();
        }

        public void SendMessage(NetworkStream stream, string body)
        {
            int statusCode = 200;
            ResponseHandler rh = new ResponseHandler(statusCode, "application/json", body);

            string response = rh.HttpResponseToString();
            byte[] responseBuffer = Encoding.ASCII.GetBytes(response);
            stream.Write(responseBuffer, 0, responseBuffer.Length);
        }

        public string ReceiveMessage(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesReceived = stream.Read(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, bytesReceived);
        }

        public void Stop()
        {
            listener.Stop();
        }

        public void printRequest()  // just there for debug reason
        {
            Console.WriteLine("Request wird gehandelt: \n");
            Console.WriteLine("Protocol is: \t" + rq.Protocol);
            Console.WriteLine("Method is: \t" + rq.Method);
            Console.WriteLine("Path is: \t" + rq.Path);
            Console.WriteLine("Headers are:");
            foreach (var line in rq.Headers!)
            {
                Console.WriteLine("\t" + line.Key + ": \t" + line.Value);
            }
            Console.WriteLine("Body contains:");

            foreach (JObject obj in rq.Body!)
            {
                // Iterate over the keys and values in the object
                foreach (var property in obj)
                {
                    Console.WriteLine("\t" + property.Key + ":\t" + property.Value);
                }
                Console.WriteLine("\n\r");
            }
        }

    }
}



