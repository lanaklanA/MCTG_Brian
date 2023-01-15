using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using Npgsql;
using MCTG_Brian.Server;
using MCTG_Brian.Database;

public class Program
{ 
    // Main entry point for the program
    public static void Main(string[] args)
    {     
        Server server = new Server("127.0.0.1", 10001);
        


        server.Start();

        // will be never reached
        server.Stop();       
    }
}
