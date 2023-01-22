using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System.IO;
using MCTG_Brian.Database;

namespace MCTG_Brian.Server
{
    public class Server
    {
        private readonly int port;
        private readonly IPAddress ipAddress;
        private TcpListener? listener;

        /// <summary>
        /// Creates a new server instance
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        public Server(string ipAddress, int port)
        {
            this.port = port;
            this.ipAddress = IPAddress.Parse(ipAddress);
        }

        /// <summary>
        /// Starts the server
        /// </summary>
        public void Start()
        {
            listener = new TcpListener(ipAddress, port);
            listener.Start();

            Console.WriteLine($"Server wartet am port {port} ...");

            while (true)
            {
                // Accept incoming connection
                TcpClient client = listener.AcceptTcpClient();

                // Receive message
                NetworkStream stream = client.GetStream();
                string HttpRequest = ReceiveMessage(stream);

                // Start a new thread to handle the request
                Thread thread = new Thread(() => RestAPI.HandleRequest(new RequestContainer(HttpRequest), stream));
                thread.Start();
            }
        }

        /// <summary>
        /// Receives a message from the client
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string ReceiveMessage(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesReceived = stream.Read(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, bytesReceived);
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public void Stop()
        {
            listener?.Stop();
        }


    }
}



