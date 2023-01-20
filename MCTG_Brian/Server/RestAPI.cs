using MCTG_Brian.Auth;
using MCTG_Brian.Battle;
using MCTG_Brian.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Transactions;

namespace MCTG_Brian.Server
{
    public static class RestAPI
    {        
        public static UserRepository  userRepo = new UserRepository();
        public static CardRepository  cardRepo = new CardRepository();
        public static PackRepository  packRepo = new PackRepository();
        //public static StackRepository stackRepo = new StackRepository();
        //public static DeckRepository  deckRepo = new DeckRepository();
        public static StatsRepository statsRepo = new StatsRepository();

        public static BattleLobby     Lobby = new BattleLobby();


        public static User loggedPlayer;
        //TODO: STRINGBUILDER FÜR DIE AUSGABE; DIE DANN NACH UNTEN KOMMT! das es nur eine einmaliges sendmessage gibt. und mit tupel für den status und dem string

        public static void HandleRequest(RequestContainer request, NetworkStream stream)
        {
            switch (request.Method)
            {
                case "GET":


                    if (request.Path.StartsWith("/deck"))
                    {
                        foreach (var x in loggedPlayer.Deck)
                        {
                            Console.WriteLine($"{x.Id} {x.Name} {x.Damage}");
                        }

 
                        SendMessage(stream, $"KartenStack wird vom Player {loggedPlayer.Username} ausgegeben!");
                    }
                   
                    if (request.Path.StartsWith("/cards"))
                    {
                            
                        foreach(var x in loggedPlayer.Stack)
                        {
                            Console.WriteLine($"{x.Id} {x.Name} {x.Damage}");
                        }
 
                        SendMessage(stream, $"KartenStack wird vom Player {loggedPlayer.Username} ausgegeben!");
                    }

                    break;


                case "POST":

                    if (request.Path.StartsWith("/users"))
                    {
                        User tempUser = userRepo.ByUniq(Authentication.getName(request));

                        if(tempUser == null)
                        {
                            loggedPlayer = JsonConvert.DeserializeObject<User>(request.Body[0].ToString());
                            userRepo.Add(loggedPlayer);
                            
                            SendMessage(stream, $"Player mit Credential {Authentication.getName(request)} gespeichert");
                            break;
                        }

                        SendMessage(stream, $"Player mit Namen {Authentication.getName(request)} existiert schon");
                    }

                    if (request.Path.StartsWith("/sessions"))
                    {
                        User tempUser = JsonConvert.DeserializeObject<User>(request.Body[0].ToString());

                        loggedPlayer = userRepo.ByUniq(Authentication.getName(request));
                        loggedPlayer.Stack = cardRepo.GetCards(loggedPlayer.Id);
                        loggedPlayer.Deck = cardRepo.GetCards(loggedPlayer.Id, true);

                        if (loggedPlayer.Username != tempUser.Username || loggedPlayer.Password != tempUser.Password)
                        {
                            SendMessage(stream, "Username oder Passwort ist nicht korrekt");
                            return;
                        }

                        if(!Authentication.loginUser(loggedPlayer))
                        {
                            SendMessage(stream, $"User {loggedPlayer.Username} ist schon eingeloggt");
                            return;
                        }

                        foreach(var x in loggedPlayer.Stack)
                        {
                            Console.WriteLine(x.Name);
                        }

                        SendMessage(stream, $"User {loggedPlayer.Username} wurde eingeloggt");
                    }
                    
                    if (request.Path.StartsWith("/packages"))
                    {
                        List<Card> newPack = request.Body.ToObject<List<Card>>();

                        if(newPack.Count() != 5)
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

                        if(package == null)
                        {
                            SendMessage(stream, "Es sind keine Packages im Store vorhanden!");
                            return;
                        }

                        if(loggedPlayer.Coins < 5)
                        {
                            SendMessage(stream, $"Player {loggedPlayer.Username} hat weniger als 5 Coins!");
                            return;
                        }

                        packRepo.Delete(package.Item1);

                        foreach (Card card in package.Item2)
                        {
                            cardRepo.Add(loggedPlayer, card);
                            loggedPlayer.Stack.Add(card); // muss ich kucken/testen
                        }

                        loggedPlayer.Coins -= 5;
                        userRepo.Update(loggedPlayer);

                        SendMessage(stream, $"Packages mit ID ({package.Item1}) wurde User gegeben {loggedPlayer.Username}!");
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