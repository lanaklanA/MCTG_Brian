using MCTG_Brian.Database;
using MCTG_Brian.User;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MCTG_Brian.Server
{
    public static class RestAPI
    {
        private static string connString = "Host=localhost;Username=postgres;Password=qwerqwer;Database=postgres";
        private static readonly UserRepository userRepo = new UserRepository(new Database.Database(connString));

        public static void Controller(RequestContainer request, NetworkStream stream)
        {

            switch (request.Method) 
            {
                case "GET":

                    var user1 = userRepo.GetUserById(Guid.Parse("4e38b8ab-53cb-40d5-aa3b-6524f5bb715d"));
                    Console.WriteLine("[GET] User bekommen: " + user1.Name);
                    SendMessage(stream, "[GET] User bekommen");
                    

                break;


                    
                case "POST":
                    
                    var user = new User.User
                    { 
                        Name = "MalWasNeues",
                        Password = "MalWasNeues",
                    };

                    userRepo.AddUser(user);
                    Console.WriteLine("[POST] User hinzugefügt");
                    SendMessage(stream, "[POST] User hinzugefügt");
                    
                    break;


             
                case "DELETE":
                    
                    userRepo.DeleteUser(Guid.Parse("b6fc004e-e029-47c6-aed6-bd770473c193"));
                    Console.WriteLine("[DELETE] User wurde geloescht");
                    SendMessage(stream, "[DELETE] User wurde geloescht");

                    
                    break;


                case "PUT":

                    var updatedUser = new User.User
                    {
                        Id = Guid.Parse("44cac76a-cf5d-4c2c-b68b-8635d505d233"),
                        Name = "NeuerName",
                        Password = "NeuesPassword",
                    };


                    userRepo.UpdateUser(updatedUser);
                    Console.WriteLine("[PUT] User wurde geupdated");
                    SendMessage(stream, "[PUT] User wurde geupdated");
                    

                    break;
            }


        }

        public static void SendMessage(NetworkStream stream, string body)
        {
            int statusCode = 200;
            ResponseContainer response = new ResponseContainer(statusCode, "application/json", body);

            string responseString = response.HttpResponseToString();
            byte[] responseBuffer = Encoding.ASCII.GetBytes(responseString);
            stream.Write(responseBuffer, 0, responseBuffer.Length);

            stream.Close();
        }
    }
}
