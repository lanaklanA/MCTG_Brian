using MCTG_Brian.Auth;
using MCTG_Brian.Battle;
using MCTG_Brian.Database;
using Newtonsoft.Json.Linq;
using System.Net.Sockets;
using System.Text;

namespace MCTG_Brian.Server
{
    public static class RestAPI
    {        
        public static BattleLobby     Lobby = new BattleLobby();
        public static UserRepository  userRepo = new UserRepository();
        public static CardRepository  cardRepo = new CardRepository();
        public static PackRepository  packRepo = new PackRepository();
        public static StackRepository stackRepo = new StackRepository();
        public static DeckRepository  deckRepo = new DeckRepository();

        public static void HandleRequest(RequestContainer request, NetworkStream stream)
        {
            switch (request.Method)
            {
                case "GET":

                    if (request.Path == "/cards" && Authentication.hasToken(request))
                    {
                        User loggedInUser = userRepo.ByUniq( Authentication.getNameFromToken(request) );
                        List<Card> UsersStack = new StackRepository().GetAll(loggedInUser.Id);

                        foreach (var Card in UsersStack)
                        {
                            Console.WriteLine(Card.Id + " " + Card.Name + " " + Card.Damage);
                        }

                        SendMessage(stream, "User wird ausgegeben");
                    }
                    else if(request.Path == "/cards" && !Authentication.hasToken(request))
                    {
                        SendMessage(stream, "Token nicht vorhanden");
                    }

                    if(request.Path.StartsWith("/users"))
                    {
                        string catchUsername = request.Path.Split("/")[2];

                        User user = userRepo.ByUniq(catchUsername);
                        Console.WriteLine(user.Id + " " + user.Name + " " + user.Password);
                        SendMessage(stream, "User gelistet");
                    }

                    if (request.Path == "/deck")
                    {
                        User cachedUser = userRepo.ByUniq( Authentication.getNameFromToken(request) );

                        List<Card> Decks = deckRepo.GetAll( cachedUser.Id );

                        if(Decks.Count() > 0)
                        {
                            foreach (var Card in Decks)
                            {
                                Console.WriteLine("karte");
                                Console.WriteLine(Card.Id + " " + Card.Name + " " + Card.Damage);
                            }
                            SendMessage(stream, "Decks wurde ausgegeben");

                        }
                        else
                        {
                            SendMessage(stream, "Decks sind unkonfiguriert");

                        }
                    }

                    break;

                case "POST":

                    if (request.Path == "/battles")
                    {
                        string Username = Authentication.getNameFromToken(request);
                        User player = userRepo.ByUniq(Username);

                        Lobby.startLobby(player);
                        Lobby.log.printProtocol();                     

                        SendMessage(stream, "Battle really done");
                    }

                    if (request.Path == "/users")
                    {
                        if (userRepo.ByUniq( Authentication.getName(request) ) == null)
                        {
                            userRepo.Add(new User((JObject)request.Body[0]));
                            SendMessage(stream, "User hinzugefügt");
                        }
                        else
                        {
                            SendMessage(stream, "User gibt es schon");
                        }

                    }

                    if (request.Path == "/sessions")
                    {
                        User cachedUser = userRepo.ByUniq( Authentication.getName(request) );

                        if (cachedUser.Name == Authentication.getName(request) && cachedUser.Password == Authentication.getPassword(request))
                        {
                            Authentication.loginUser(cachedUser);
                            SendMessage(stream, "User eingeloggt");
                        }
                        else
                        {
                            SendMessage(stream, "User nicht eingeloggt");
                        }
                    }

                    if (request.Path.StartsWith("/packages") && Authentication.isAdmin(request))
                    {
                        List<Guid> GuidCollection = new List<Guid>();

                        foreach (JObject card in request.Body)
                        {
                           cardRepo.Add(new Card(card));
                            GuidCollection.Add(Guid.Parse((string)card["Id"]));
                        }
                        packRepo.Add(GuidCollection);

                        SendMessage(stream, "Karten wurde gesendet");
                    }
                    else if(request.Path.StartsWith("/packages") && !Authentication.isAdmin(request))
                    {
                        SendMessage(stream, "Karten wurden nicht hinzugefügt, du bist kein Admin");
                    }

                    if (request.Path == "/transactions/packages")
                    {                       
                        Tuple<Guid, List<Guid>> newPackage = packRepo.GetRandPackage();
                        string nameFromToken = Authentication.getNameFromToken(request);
                        User loggedInUser = userRepo.ByUniq(nameFromToken);
                        int countContent = packRepo.Count();

                        if(countContent > 0)
                        { 
                            packRepo.Delete(newPackage.Item1);
                        
                            foreach (var Card in newPackage.Item2)
                            {
                                stackRepo.Add(new Tuple<User, Guid>(loggedInUser, Card));
                            }
                            SendMessage(stream, "Karten wurde gesendet");
                        }
                        else
                        {
                            SendMessage(stream, "Keine Packete vorhanden!");
                        }
                    }

                    break;

                case "PUT":
                    if(request.Path == "/deck")
                    {
                        Dictionary<Guid, Guid> deckCollection = new Dictionary<Guid, Guid>();
                        string cachedUser = Authentication.getNameFromToken(request);
                        Console.WriteLine(cachedUser);
                        User user = userRepo.ByUniq(cachedUser);


                        foreach (string CardId in request.Body)
                        {
                            deckCollection.Add(Guid.Parse(CardId), user.Id);
                        }

                        if (deckCollection.Count() > 3)
                        {
                            deckRepo.Add(deckCollection);
                            SendMessage(stream, "Es klappt");
                        }
                        else
                        {
                            SendMessage(stream, "Zu wenig Karten");
                        }
                    }

                    break;

                case "DELETE":
                    break;
            }

            //UserRepository.CloseDb();
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
