using MCTG_Brian.Authentication;
using MCTG_Brian.Battle;
using MCTG_Brian.Database;
using MCTG_Brian.Database.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql.Replication.PgOutput.Messages;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace MCTG_Brian.Server
{
    public static class RestAPI
    {
        public static UserRepository  userRepo = new UserRepository();
        public static CardRepository  cardRepo = new CardRepository();
        public static PackRepository  packRepo = new PackRepository();

        //ÜBERLEGEN ÜBERALL EINEN TEMPUSER ZUERSTELLEN! asntellen von dem ganzen auth.getUser(request.Token)
        //TODO: STRINGBUILDER FÜR DIE AUSGABE; DIE DANN NACH UNTEN KOMMT! das es nur eine einmaliges sendmessage gibt. und mit tupel für den status und dem string
        public static BattleLobby Lobby = new BattleLobby();

        private static List<Tuple<string, string>> notAuthList = new List<Tuple<string, string>>()
        {
            new Tuple<string, string>("POST", "/users"),
            new Tuple<string, string>("POST", "/sessions")
        };
        private static List<Tuple<string, string>> AuthList = new List<Tuple<string, string>>()
        {
            new Tuple<string, string>("GET", "/cards"),
            new Tuple<string, string>("GET", "/deck"),
            new Tuple<string, string>("GET", "/stats"),
            new Tuple<string, string>("GET", "/score"),

            new Tuple<string, string>("PUT", "/deck"),
            new Tuple<string, string>("PUT", "/deck"),
            
            new Tuple<string, string>("POST", "/packages"),
            new Tuple<string, string>("POST", "/battle"),
            new Tuple<string, string>("POST", "/transactions/packages"),
        };

        public static void HandleRequest(RequestContainer request, NetworkStream stream)
        {

            AuthList.Add(new Tuple<string, string>("GET", $"/users/{request.Path.Replace("/users/", "")}"));
            AuthList.Add(new Tuple<string, string>("PUT", $"/users/{request.Path.Replace("/users/", "")}"));

            if (notAuthList.Contains(new Tuple<string, string>(request.Method, request.Path))) { }
            else if (AuthList.Contains(new Tuple<string, string>(request.Method, request.Path)) && Auth.isUserLoggedIn(request.getToken())) { }
            else if (AuthList.Contains(new Tuple<string, string>(request.Method, request.Path)) && (request.getToken() == null))
            {
                SendMessage(stream, "Token is missing");
                return;
            }
            else if (AuthList.Contains(new Tuple<string, string>(request.Method, request.Path)))
            {
                SendMessage(stream, "User is not logged in");
                return;
            }
            else
            {
                SendMessage(stream, "Path not found");
                return;
            }

            switch (request.Method)
            {
                case "GET":

                    if (request.Path.EndsWith("/deck"))
                    {
                        if(Auth.getUser(request.getToken()).Deck.Count() <= 0)
                        {
                            SendMessage(stream, $"Das Deck von User {Auth.getUser(request.getToken()).Username} ist leer");
                        }

                        foreach (Card card in Auth.getUser(request.getToken()).Deck)
                        {
                            Console.WriteLine($"{card.Id} {card.Name} {card.Damage}");
                        }

                        SendMessage(stream, $"KartenDeck wird vom Player {Auth.getUser(request.getToken()).Username} ausgegeben!");
                    }
 
                    if (request.Path.StartsWith("/deck") && request.HasQueryParameter("format", "plain"))
                    {
                        Console.WriteLine("YEAH ICH BIN DRINNEN IM FORMAR");
                    }
                    
                    if (request.Path.StartsWith("/users/"))
                    {
                        string pathUsername = request.Path.Replace("/users/", "");

                        if (Auth.getUser(request.getToken()).Username != pathUsername)
                        {
                            SendMessage(stream, $"Keine Berechtiung Benutzer {pathUsername} anzuzeigen!");
                            return;
                        }
                      
                        User tempUser = Auth.getUser(request.getToken()); // INKONSITENT
                        SendMessage(stream, $"Benutzerausgabe: {tempUser.Id} {tempUser.Username} {tempUser.Coins} {tempUser.Bio} {tempUser.Image}");
     
                    }

                    if (request.Path.StartsWith("/cards"))
                    {
                        foreach (Card card in Auth.getUser(request.getToken()).Stack)
                        {
                            Console.WriteLine($"{card.Id} {card.Name} {card.Damage}");
                        }

                        SendMessage(stream, $"KartenStack wird vom Player {Auth.getUser(request.getToken()).Username} ausgegeben!");
                    }

                    if (request.Path.StartsWith("/stats"))
                    {
                        User tempUser = Auth.getUser(request.getToken());

                        SendMessage(stream, $"Id: {tempUser.Id} Name: {tempUser.Username} Pw: {tempUser.Password} Coins: {tempUser.Coins} Bio: {tempUser.Bio} Image: {tempUser.Image} Elo: {tempUser.Stats.Elo} Wins: {tempUser.Stats.Wins} Draws: {tempUser.Stats.Draws}");
                    }
                    
                    if (request.Path.StartsWith("/score"))
                    {
                        foreach(var tempUser in Auth.getAll())
                        {
                            Console.WriteLine($"Id: {tempUser.Id} Name: {tempUser.Username} Pw: {tempUser.Password} Coins: {tempUser.Coins} Bio: {tempUser.Bio} Image: {tempUser.Image} Elo: {tempUser.Stats.Elo} Wins: {tempUser.Stats.Wins} Draws: {tempUser.Stats.Draws}");
                        }

                        SendMessage(stream, "Scoreboard wurde geprinted");
                    }

                    break;

                case "POST":


                    if (request.Path.StartsWith("/battles"))
                    {
                        //User player = Authentication.GetUserViaToken(request.getToken());
                        //Stats playerStats = statsRepo.ByUniq(player.Id.ToString());

                        //Lobby.startLobby(player);
                        //Lobby.log.printProtocol();

                        //Console.WriteLine($"Winner: {Lobby.log.winner} and Loser: {Lobby.log.loser}");

                        //if (player == Lobby.log.winner)
                        //{
                        //    playerStats.updateWinStats();
                        //    SendMessage(stream, "Du hast gewonnen");
                        //}
                        //else if (player == Lobby.log.loser)
                        //{
                        //    playerStats.updateLoseStats();

                        //    SendMessage(stream, "Du hast verloren");
                        //}
                        //else
                        //{
                        //    playerStats.updateDrawStats();
                        //    SendMessage(stream, "Unentschieden");
                        //}

                        //statsRepo.Update(playerStats);


                        // TODO: DeckDelete from Table
                        // TODO: UpdateDeck at both players 

                        //SendMessage(stream, "Battle really done");
                    }

                    if (request.Path.StartsWith("/users"))
                    {
                        User tempUser = userRepo.ByUniq(Auth.getName(request));

                        if (tempUser != null)
                        {
                            SendMessage(stream, $"Player mit Namen {tempUser.Username} existiert schon");
                            return;
                        }

                        tempUser = JsonConvert.DeserializeObject<User>(request.Body[0].ToString());
                        userRepo.Add(tempUser);

                        SendMessage(stream, $"Player mit Credential {tempUser.Username} gespeichert");

                    }

                    if (request.Path.StartsWith("/sessions"))
                    {
                        User toCheck = JsonConvert.DeserializeObject<User>(request.Body[0].ToString());

                        User tempUser = userRepo.ByUniq(Auth.getName(request));


                        if (tempUser == null || (tempUser.Username != toCheck.Username || tempUser.Password != toCheck.Password))
                        {
                            SendMessage(stream, "Username oder Passwort ist nicht korrekt");
                            return;

                        }

                        if (!Auth.loginUser(tempUser))
                        {
                            SendMessage(stream, $"User {tempUser.Username} ist schon eingeloggt");
                            return;
                        }

                        tempUser.Stack = cardRepo.GetCards(tempUser.Id);
                        tempUser.Deck = cardRepo.GetCards(tempUser.Id, true);

                        SendMessage(stream, $"User {tempUser.Username} wurde eingeloggt");
                    }

                    if (request.Path.StartsWith("/packages"))
                    {
                        List<Card> newPack = request.Body.ToObject<List<Card>>();

                        if (newPack.Count() != 5)
                        {
                            SendMessage(stream, "Das Packages ist nicht Konform!");
                            return;
                        }

                        packRepo.Add(newPack);
                        SendMessage(stream, "Package mit 5 Karten sind nun im Store verfügbar!");
                    }

                    if (request.Path.StartsWith("/transactions/packages"))
                    {
                        Tuple<Guid, List<Card>> package = packRepo.GetRandPackage();

                        if (package == null)
                        {
                            SendMessage(stream, "Es sind keine Packages im Store vorhanden!");
                            return;
                        }

                        if (Auth.getUser(request.getToken()).Coins < 5)
                        {
                            SendMessage(stream, $"Player {Auth.getUser(request.getToken()).Username} hat weniger als 5 Coins!");
                            return;
                        }

                        packRepo.Delete(package.Item1);

                        foreach (Card card in package.Item2)
                        {
                            cardRepo.Add(Auth.getUser(request.getToken()), card);
                            Auth.getUser(request.getToken()).Stack.Add(card); // muss ich kucken/testen
                        }

                        Auth.getUser(request.getToken()).Coins -= 5;
                        userRepo.Update(Auth.getUser(request.getToken()));

                        SendMessage(stream, $"Packages mit ID ({package.Item1}) wurde User gegeben {Auth.getUser(request.getToken()).Username}!");
                    }
                    
                    break;

                case "PUT":

                    if (request.Path.StartsWith("/deck"))
                    {
                        List<Guid> deckIds = request.Body.ToObject<List<Guid>>();

                        if (deckIds.Count() != 4)
                        {
                            SendMessage(stream, $"Das gegeben Deck beinhaltet {deckIds.Count()} sollte aber 4 haben!");
                            return;
                        }

                        if (cardRepo.ChangeDepot(deckIds, Auth.getUser(request.getToken())) == false)
                        {
                            SendMessage(stream, "Mind. eine Karte ist nicht in deinem Stack!");
                            return;
                        }

                        SendMessage(stream, "Deck geupdatet");


                        Auth.getUser(request.getToken()).Stack = cardRepo.GetCards(Auth.getUser(request.getToken()).Id);
                        Auth.getUser(request.getToken()).Deck = cardRepo.GetCards(Auth.getUser(request.getToken()).Id, true);
                    }

                    if (request.Path.StartsWith("/users/"))
                    {
                        /*
                         TODO:
                                curl -X PUT http://localhost:10001/users/kienboec --header "Content-Type: application/json" --header "Authorization: Basic altenhof-mtcgToken" -d "{\"Name\": \"Hoax\",  \"Bio\": \"me playin...\", \"Image\": \":-)\"}"
                                curl -X PUT http://localhost:10001/users/altenhof --header "Content-Type: application/json" --header "Authorization: Basic kienboec-mtcgToken" -d "{\"Name\": \"Hoax\", \"Bio\": \"me codin...\",  \"Image\": \":-D\"}"
                                curl -X GET http://localhost:10001/users/someGuy  --header "Authorization: Basic kienboec-mtcgToken"
                         */

                        User updateUser = Auth.getUser(request.getToken());
                        JObject jsonUser = (JObject)request.Body[0];

                        updateUser.Username =   jsonUser["Name"].ToString();
                        updateUser.Bio =        jsonUser["Bio"].ToString();
                        updateUser.Image =      jsonUser["Image"].ToString();

                        Auth.updateUser(request.getToken(), updateUser);
                        userRepo.Update(updateUser);

                        SendMessage(stream, $"Neuer User mit {updateUser.Id} {updateUser.Password} {updateUser.Username} {updateUser.Coins} {updateUser.Image} {updateUser.Bio}");
                    }

                    break;
            }
        }

        public static void SendMessage(NetworkStream stream, string body)
        {
            Console.WriteLine($"Clientausgabe: {body}");


            int statusCode = 200;
            ResponseContainer response = new ResponseContainer(statusCode, "application/json", body);

            string responseString = response.HttpResponseToString();
            byte[] responseBuffer = Encoding.ASCII.GetBytes(responseString);
            stream.Write(responseBuffer, 0, responseBuffer.Length);

            stream.Close();
        }
    }
}




/*
 
  case "GET":

                    if (request.Path == "/cards" && Authentication.isUserLoggedIn(request))
                    {
                        //User loggedInUser = Authentication.GetUserViaToken(request.getToken());
                        //List<Card> UsersStack = new StackRepository().GetAll(loggedInUser.Id);

                        //foreach (var Card in UsersStack)
                        //{
                        //    Console.WriteLine($"{Card.Id} {Card.Name} {Card.Damage} {Card.Element} {Card.Monster}");
                        //}

                        //SendMessage(stream, "User wird ausgegeben");
                    }
                    else if(request.Path == "/cards")
                    {
                        //SendMessage(stream, "Not logged in");
                    }

                    if(request.Path.StartsWith("/users") && Authentication.isUserLoggedIn(request))
                    { 
                        //string catchUsername = request.Path.Replace("/users/", "");
                        //User user = Authentication.GetUserViaName(catchUsername);

                        //Console.WriteLine(user.Id + " " + user.Name + " " + user.Password);
                        //SendMessage(stream, "User gelistet");
                    }
                    else if(request.Path.StartsWith("/users"))
                    {

                    }

                    if (request.Path.StartsWith("/deck") && Authentication.isUserLoggedIn(request))
                    {
                        //User cachedUser = Authentication.GetUserViaToken(request.getToken());

                        //List<Card> Decks = deckRepo.GetAll( cachedUser.Id );

                        //if(Decks.Count() > 0)
                        //{
                        //    foreach (var Card in Decks)
                        //    {
                        //        Console.WriteLine("karte");
                        //        Console.WriteLine(Card.Id + " " + Card.Name + " " + Card.Damage);
                        //    }
                        //    SendMessage(stream, "Decks wurde ausgegeben");
                        //}
                        //else
                        //{
                        //    SendMessage(stream, "Decks sind unkonfiguriert");
                        //}
                    }
                    else if (request.Path == "/deck")
                    {
                        SendMessage(stream, "Not logged in");
                    }

                    if (request.Path.StartsWith("/stats") && Authentication.isUserLoggedIn(request))
                    {
                        //User cachedUser = Authentication.GetUserViaToken(request.getToken());
                        //Stats userStats = statsRepo.ByUniq(cachedUser.Id.ToString());

                        //Console.WriteLine($"{userStats.Id} {userStats.UserId} with Name {userStats.Username} has Elo: {userStats.Elo} Wins: {userStats.Wins} Loses: {userStats.Loses} Draws: {userStats.Draws}");

                        //SendMessage(stream, "Stats werden ausgegeben");    
                    }
                    else if (request.Path == "/stats")
                    {
                        SendMessage(stream, "Not logged in");
                    }

                    if (request.Path.StartsWith("/score") && Authentication.isUserLoggedIn(request))
                    {
                        //List<Stats> allStats = statsRepo.GetAll();

                        //foreach(var userStats in allStats)
                        //{
                        //    Console.WriteLine($"{userStats.Id} {userStats.UserId} with Name {userStats.Username} has Elo: {userStats.Elo} Wins: {userStats.Wins} Loses: {userStats.Loses} Draws: {userStats.Draws}");
                        //}
                        //SendMessage(stream, "Stats von jedem werden ausgegeben");

                    }
                    else if (request.Path == "/score")
                    {
                        SendMessage(stream, "Not logged in");
                    }

                    break;
                    
                case "POST":

                    if (request.Path.StartsWith("/sessions"))
                    {
                        //User cachedUser = userRepo.ByUniq(Authentication.getName(request));

                        //if (cachedUser.Name == Authentication.getName(request) && cachedUser.Password == Authentication.getPassword(request))
                        //{
                        //    Authentication.loginUser(cachedUser);
                        //    SendMessage(stream, "User eingeloggt");
                        //}
                        //else
                        //{
                        //    SendMessage(stream, "User nicht eingeloggt");
                        //}
                    }

                    if (request.Path.StartsWith("/users"))
                    {
                        //if (userRepo.ByUniq( Authentication.getName(request) ) == null)
                        //{
                        //    userRepo.Add(new User((JObject)request.Body[0]));
                        //    SendMessage(stream, "User hinzugefügt");
                        //}
                        //else
                        //{
                        //    SendMessage(stream, "User gibt es schon");
                        //}

                    }

                    if (request.Path.StartsWith("/battles") && Authentication.isUserLoggedIn(request))
                    {
                        //User player = Authentication.GetUserViaToken(request.getToken());
                        //Stats playerStats = statsRepo.ByUniq(player.Id.ToString());

                        //Lobby.startLobby(player);
                        //Lobby.log.printProtocol();

                        //Console.WriteLine($"Winner: {Lobby.log.winner} and Loser: {Lobby.log.loser}");

                        //if (player == Lobby.log.winner)
                        //{
                        //    playerStats.updateWinStats();
                        //    SendMessage(stream, "Du hast gewonnen");
                        //}
                        //else if (player == Lobby.log.loser)
                        //{
                        //    playerStats.updateLoseStats();
                      
                        //    SendMessage(stream, "Du hast verloren");
                        //}
                        //else
                        //{
                        //    playerStats.updateDrawStats();
                        //    SendMessage(stream, "Unentschieden");
                        //}

                        //statsRepo.Update(playerStats);
                   

                        // TODO: DeckDelete from Table
                        // TODO: UpdateDeck at both players 

                        //SendMessage(stream, "Battle really done");
                    }
                    else if(request.Path.StartsWith("/battles"))
                    {
                        //SendMessage(stream, "User nicht eingeloggt");
                    }

                    if (request.Path.StartsWith("/packages") && Authentication.isUserLoggedIn(request) && Authentication.isAdmin(request) )
                    {
                        //List<Guid> GuidCollection = new List<Guid>();

                        //foreach (JObject card in request.Body)
                        //{
                        //   cardRepo.Add(new Card(card));
                        //    GuidCollection.Add(Guid.Parse((string)card["Id"]));
                        //}
                        //packRepo.Add(GuidCollection);

                        //SendMessage(stream, "Karten wurde gesendet");
                    }
                    else if(request.Path.StartsWith("/packages") && !Authentication.isUserLoggedIn(request))
                    {
                        //SendMessage(stream, "Karten wurden nicht hinzugefügt, du bist nicht eingeloggt");
                    }
                    else if (request.Path.StartsWith("/packages") && !Authentication.isAdmin(request))
                    {
                        //SendMessage(stream, "Karten wurden nicht hinzugefügt, du bist kein Admin");
                    }

                    if (request.Path.StartsWith("/transactions/packages") && Authentication.isUserLoggedIn(request))
                    {
                        //Tuple<Guid, List<Guid>> newPackage = packRepo.GetRandPackage();
                        //User loggedInUser = Authentication.GetUserViaToken(request.getToken());

                        //if(packRepo.Count() > 0)
                        //{ 
                        //    packRepo.Delete(newPackage.Item1);
                        
                        //    foreach (var Card in newPackage.Item2)
                        //    {
                        //        stackRepo.Add(new Tuple<User, Guid>(loggedInUser, Card));
                        //    }
                        //    SendMessage(stream, "Karten wurde gesendet");
                        //}
                        //else
                        //{
                        //    SendMessage(stream, "Keine Packete vorhanden!");
                        //}
                    }
                    else if(request.Path.StartsWith("/transactions/packages") )
                    {
                            //SendMessage(stream, "User nicht eingeloggt");
                    }

                    break;

                case "PUT":
                    if(request.Path.StartsWith("/deck") && Authentication.isUserLoggedIn(request))
                    {
                        //Dictionary<Guid, Guid> deckCollection = new Dictionary<Guid, Guid>();

                        //User user = Authentication.GetUserViaToken(request.getToken());

                        //foreach (string CardId in request.Body)
                        //{
                        //    deckCollection.Add(Guid.Parse(CardId), user.Id);
                        //}

                        //if (deckCollection.Count() > 3)
                        //{
                        //    deckRepo.Add(deckCollection);
                        //    SendMessage(stream, "Es klappt");
                        //}
                        //else
                        //{
                        //    SendMessage(stream, "Zu wenig Karten");
                        //}
                    }
                    else if(request.Path.StartsWith("/deck"))
                    {
                      //  SendMessage(stream, "User nicht eingeloggt");
                    }

                    break;

                case "DELETE":
                    break;
 
 */