﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System.IO;

namespace MCTG_Brian.Server
{
    public class Server
    {
        private readonly int port;
        private readonly IPAddress ipAddress;
        private TcpListener? listener;
        
        public Server(string ipAddress, int port)
        {
            this.port = port;
            this.ipAddress = IPAddress.Parse(ipAddress);
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
            RestAPI.Controller( new RequestContainer(HttpRequest), stream );
        }

        public static string ReceiveMessage(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesReceived = stream.Read(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, bytesReceived);
        }

        public void Stop()
        {
            listener?.Stop();
        }

    }
}



