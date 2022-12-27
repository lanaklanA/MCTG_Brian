﻿using MCTG_Brian;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

public class Program
{

    // Main entry point for the program
    public static void Main(string[] args)
    {
        RestAPI server = new RestAPI(10001);


        InitDb();
        
        server.Start();
        server.Stop();

    }

    public static void InitDb()
    {
        // TODO create db
    }
}

