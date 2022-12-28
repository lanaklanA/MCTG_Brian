using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using Npgsql;
using MCTG_Brian.User;
using MCTG_Brian.Server;

public class Program
{

    // Main entry point for the program
    public static void Main(string[] args)
    {
        RestAPI server = new RestAPI("127.0.0.1", 10001);

        InitDb();

        server.Start();
        server.Stop();
    }

    public static void InitDb()
    {
        string connString = "Host=localhost;Username=postgres;Password=qwerqwer;Database=postgres";
        var userRepository = new UserRepository(connString);
        var userService = new UserService(userRepository);


        // should fetch all user
        var allUsers = userService.GetAllUsers();
        foreach (var tempUser in allUsers)
        {
            Console.WriteLine($"Id: {tempUser.Id}, Name: {tempUser.Name}\t, Password: {tempUser.Password}");
        }
        Console.WriteLine("\n\n");
        

        //  should update the user
        var user1 = new User
        {
            Id = Guid.Parse("9b9501c0-74c8-4bf7-937b-53f0a4950149"),
            Name = "NeuerName",
            Password = "NeuesPassword",
        };
        userService.UpdateUser(user1);
        Console.WriteLine($"Id: {user1.Id}, Name: {user1.Name}\t, Password: {user1.Password} ... wird geupdated");
        Console.WriteLine("\n\n");


        // should delete catched user
        Guid uid = allUsers.ElementAt(1).Id;
        userService.DeleteUser(uid);
        Console.WriteLine($"Folgende User mit der Guid: {uid} wird gelöscht");
        Console.WriteLine("\n\n");


        // should add an new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "DasIstDerNeueBenutzer",
            Password = "MitSeinemNeuenPassword",
        };
        Console.WriteLine($"Id: {user.Id}, Name: {user.Name}\t, Password: {user.Password} ... wird hinzugefügt");
        userService.AddUser(user);
        Console.WriteLine("\n\n");


        // should fetch user by id
        Guid uid1 = allUsers.ElementAt(0).Id;
        var user2 = userService.GetUserById(uid1);
        Console.WriteLine($"Fetched Username with id {user2.Id} has the name {user2.Name} and password {user2.Password}\n");
        Console.WriteLine("\n\n");

        // should fetch all user
        var allUsers1 = userService.GetAllUsers();
        foreach (var tempUser in allUsers1)
        {
            Console.WriteLine($"Id: {tempUser.Id}, Name: {tempUser.Name}\t, Password: {tempUser.Password}");
        }
        Console.WriteLine("\n\n");


    }
}

