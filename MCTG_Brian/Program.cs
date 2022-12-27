using MCTG_Brian;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using Npgsql;
public class Program
{

    // Main entry point for the program
    public static void Main(string[] args)
    {
        RestAPI server = new RestAPI(10001);


       
        bool DebugMode = true;  // Just for Debbug reason -> make it to false, if Database is not installed in on your PC
        if (DebugMode) InitDb();
        
        server.Start();
        server.Stop();

        

    }

    public static void InitDb()
    {
        string connString = "Host=localhost;Username=postgres;Password=qwerqwer;Database=postgres";

        // Connect to the database
        using (var conn = new NpgsqlConnection(connString))
        {
            try
            {
                conn.Open();
            }
            catch (Exception) 
            { 
                Console.WriteLine("Database connection refused");
                System.Environment.Exit(-1);
                //throw;
            }

            // Create a SELECT * query
            string query = "SELECT * FROM TEST_TABLE";

            // Execute the query and retrieve the data
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    // Print the data to the console
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write(reader[i] + " ");
                        }
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}

