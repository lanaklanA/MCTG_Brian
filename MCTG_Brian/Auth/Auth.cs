using MCTG_Brian.Database.Models;
using MCTG_Brian.Server;
using System.IO;

namespace MCTG_Brian.Authentication
{
    public static class Auth
    {
        private static Dictionary<string, User> loggedUser = new Dictionary<string, User>();

        private static List<Tuple<string, string>> notAuthList = new List<Tuple<string, string>>()
        {
            new Tuple<string, string>("POST", "/users"),
            new Tuple<string, string>("POST", "/sessions"),
        };
        private static List<Tuple<string, string>> AuthList = new List<Tuple<string, string>>()
        {
            new Tuple<string, string>("GET", "/cards"),
            new Tuple<string, string>("GET", "/deck"),
            new Tuple<string, string>("GET", "/users/"),
            new Tuple<string, string>("GET", "/deck?format=plain"),
            new Tuple<string, string>("GET", "/stats"),
            new Tuple<string, string>("GET", "/score"),
            new Tuple<string, string>("GET", "/tradings"),

            new Tuple<string, string>("PUT", "/deck"),
            new Tuple<string, string>("PUT", "/users/"),
            new Tuple<string, string>("POST", "/tradings/"),


            new Tuple<string, string>("POST", "/packages"),
            new Tuple<string, string>("POST", "/battles"),
            new Tuple<string, string>("POST", "/tradings"),
            new Tuple<string, string>("POST", "/tradings/"),
            new Tuple<string, string>("POST", "/fusion"),
            new Tuple<string, string>("POST", "/transactions/packages"),

            new Tuple<string, string>("DELETE", "/tradings/"),

        };
        private static List<Tuple<string, string>> AdminList = new List<Tuple<string, string>>()
        {
            new Tuple<string, string>("POST", "/packages"),
        };

        public static User getUser(string token)
        {
            token = token ?? "";
            return loggedUser.FirstOrDefault(x => x.Key == token).Value ?? new User();
        }
        public static List<User> getAll()
        {
            return loggedUser.Values.ToList();
        }
        public static User getUserViaId(Guid id)
        {
            return loggedUser.FirstOrDefault(x => x.Value.Id == id).Value;
        }
        public static User getUserViaName(string Username)
        {
            return loggedUser.FirstOrDefault(x => x.Value.Username == Username).Value;
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
            var name = request.Body[0]["Username"] ?? "";
            return name.ToString();
        }
        public static bool isAdmin(string token)
        {
            return token.Contains("admin");
        }
        
        public static bool existsPathAuth(RequestContainer request)
        {
            if(request.Path.Contains("/users/") || request.Path.Contains("/tradings/"))
            {
                string modifiedRequest = request.Path.Substring(0, request.Path.LastIndexOf("/")+1);
                return AuthList.Contains(new Tuple<string, string>(request.Method, modifiedRequest));

            }
            return AuthList.Contains(new Tuple<string, string>(request.Method, request.Path));
        }

        public static bool existsPathNotAuth(RequestContainer request)
        {
            return notAuthList.Contains(new Tuple<string, string>(request.Method, request.Path));
        }

        public static bool existsPathAdmin(RequestContainer request)
        {
            return Auth.AdminList.Contains(new Tuple<string, string>(request.Method, request.Path));
        }

        public static void AddAuthPath(string httpMethod, string path)
        {
            Auth.AuthList.Add(new Tuple<string, string>(httpMethod, path));
        }



    }
}
