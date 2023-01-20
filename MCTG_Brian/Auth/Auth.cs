using MCTG_Brian.Database.Models;
using MCTG_Brian.Server;

namespace MCTG_Brian.Authentication
{
    public static class Auth
    {
        private static Dictionary<string, User> loggedUser = new Dictionary<string, User>();
        public static User getUser(string token)
        {
            token = token ?? "";
            return loggedUser.FirstOrDefault(x => x.Key == token).Value ?? new User();
        }

        public static List<User> getAll()
        {
            return loggedUser.Values.ToList();
        }
        public static void updateUser(string key, User value)
        {
            loggedUser[key] = value;
        }
        public static bool loginUser(User user)
        {
            string token = $"{user.Username}-mtcgToken";

            if (loggedUser.ContainsKey(token))
                return false;

            loggedUser.Add(token, user);
            return true;
        }

        public static bool isUserLoggedIn(string token)
        {
            return loggedUser.ContainsKey(token ?? "");
        }

        public static string getName(RequestContainer request)
        {
            return (string)request.Body[0]["Username"];
        }

        public static bool isAdmin(string token)
        {
            return token.Contains("admin");
        }
    }
}
