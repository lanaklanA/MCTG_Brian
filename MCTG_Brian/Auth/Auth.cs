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
        private static Dictionary<string, User> loggedUser = new Dictionary<string, User>();

        public static User GetUserViaToken(string token)
        {
            return loggedUser.FirstOrDefault(x => x.Key == token).Value;
        }

        public static User GetUserViaName(string name)
        {
            return loggedUser.FirstOrDefault(x => x.Value.Name == name).Value;
        }

        public static bool isUserLoggedIn(RequestContainer request)
        {
            return loggedUser.ContainsKey(request.getToken());
        }

        public static string getName(RequestContainer request)
        {
            return (string)request.Body[0]["Username"];
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

        public static void loginUser(User user)
        {
            string token = $"{user.Name}-mtcgToken";
            loggedUser.Add(token, user);
        }
    }
}
