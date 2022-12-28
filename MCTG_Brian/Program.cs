using MCTG_Brian;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using Npgsql;
using MCTG_Brian.User;

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
        string connString = "Host=localhost;Username=postgres;Password=qwerqwer;Database=postgres"
        var userRepository = new UserRepository(connString);
        var userService = new UserService(userRepository);

        var user = new User
        {
            Name = "John",
            Password = "password1234",
            Email = "john@example.com"
        };
        userService.AddUser(user);
    }
}

