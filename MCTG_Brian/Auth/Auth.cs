using MCTG_Brian.Database;
using MCTG_Brian.Server;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTG_Brian.Auth
{
    public static class Authentication
    {
        //static IDictionary<User, string> loggedUser = new Dictionary<User, string>();
        private static List<User> loggedUser = new List<User>();
       

        //public static Authentication(RequestContainer request)
        //{
        //    this.request = request;
        //}

        public static string getName(RequestContainer request)
        {
            return (string)request.Body[0]["Username"];
        }

        public static bool hasToken(RequestContainer request)
        {
            if (request.Headers.TryGetValue("Authorization", out var token))
            {
                return true;
            }

            return false;
        }

        public static string getPassword(RequestContainer request)
        {
            return (string)request.Body[0]["Password"];
        }

        public static bool isAdmin(RequestContainer request)
        {
            string token = request.Headers["Authorization"];
            string name = token.Split(" ")[1].Split("-")[0];

            return name == "admin";
        }

        public static string getNameFromToken(RequestContainer request)
        {
            string token = request.Headers["Authorization"];
            return token.Split(" ")[1].Split("-")[0];
        }
        
        public static void loginUser(User user)
        {
            loggedUser.Add(user);
        }

        //public  void logoutUser(User user)
        //{
        //    loggedUser.Remove(user);
        //}

        //public  bool isUserLoggedIn(User user)
        //{
        //    return loggedUser.Contains(user);
        //}

        //public static void loginUser(User user, string token)
        //{
        //    loggedUser.Add(user, token);
        //}
        //public static bool IsLoggedIn(User user)
        //{
        //    return loggedUser.ContainsKey(user);
        //}
    }
}
