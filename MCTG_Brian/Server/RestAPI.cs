using MCTG_Brian.Authentication;
using MCTG_Brian.Battle;
using MCTG_Brian.Database;
using MCTG_Brian.Database.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Sockets;
using System.Text;

namespace MCTG_Brian.Server
{
    public static class RestAPI
    {
        public static UserRepository  userRepo = new UserRepository();
        public static CardRepository  cardRepo = new CardRepository();
        public static PackRepository  packRepo = new PackRepository();
        public static TradeRepository tradeRepo = new TradeRepository();

        //TODO: ÜNBEALL BEI DEN METHODEN COMMENDS MACHEN MIT -> /// <-
        //TODO: ÜBERLEGEN ÜBERALL EINEN TEMPUSER ZUERSTELLEN! asntellen von dem ganzen auth.getUser(request.Token)
        //TODO: STRINGBUILDER FÜR DIE AUSGABE; DIE DANN NACH UNTEN KOMMT! das es nur eine einmaliges sendmessage gibt. und mit tupel für den status und dem string
        //TODO: VLLT DAS ALLES IN EIN RouteAtorization packen
        //TODO: Passwort noch hashen!
        
        public static BattleLobby Lobby = new BattleLobby();

        private static List<Tuple<string, string>> notAuthList = new List<Tuple<string, string>>()
        {
            new Tuple<string, string>("POST", "/users"),
            new Tuple<string, string>("POST", "/sessions"),
        };
        private static List<Tuple<string, string>> AuthList = new List<Tuple<string, string>>()
        {
            new Tuple<string, string>("GET", "/cards"),
            new Tuple<string, string>("GET", "/deck"),
            new Tuple<string, string>("GET", "/stats"),
            new Tuple<string, string>("GET", "/score"),
            new Tuple<string, string>("GET", "/tradings"), 

            new Tuple<string, string>("PUT", "/deck"),
            new Tuple<string, string>("PUT", "/deck"),

            new Tuple<string, string>("POST", "/packages"),
            new Tuple<string, string>("POST", "/battles"),
            new Tuple<string, string>("POST", "/trades"),
            new Tuple<string, string>("POST", "/transactions/packages"),
        };
        private static List<Tuple<string, string>> AdminList = new List<Tuple<string, string>>()
        {
            new Tuple<string, string>("POST", "/packages"),
        };

        public static void HandleRequest(RequestContainer request, NetworkStream stream)
        {
            AuthList.Add(new Tuple<string, string>("GET", $"/users/{request.Path.Replace("/users/", "")}"));
            AuthList.Add(new Tuple<string, string>("PUT", $"/users/{request.Path.Replace("/users/", "")}"));
            AuthList.Add(new Tuple<string, string>("POST", $"/tradings/{request.Path.Replace("/tradings/", "")}"));
            AuthList.Add(new Tuple<string, string>("DELETE", $"/tradings/{request.Path.Replace("/tradings/", "")}"));
            AuthList.Add(new Tuple<string, string>("POST", $"/tradings/{request.Path.Replace("/tradings/", "")}"));

            if (notAuthList.Contains(new Tuple<string, string>(request.Method, request.Path))) { }
            else if (AdminList.Contains(new Tuple<string, string>(request.Method, request.Path)) && (!Auth.isAdmin(request.getToken())))
            {
                SendMessage(stream, "Du musst Admin sein!");
                return;
            }
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
                            return;
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
                    
                    if (request.Path.Contains("/users/"))
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

                    if (request.Path.EndsWith("/cards"))
                    {
                        foreach (Card card in Auth.getUser(request.getToken()).Stack)
                        {
                            Console.WriteLine($"{card.Id} {card.Name} {card.Damage}");
                        }

                        SendMessage(stream, $"KartenStack wird vom Player {Auth.getUser(request.getToken()).Username} ausgegeben!");
                    }

                    if (request.Path.EndsWith("/stats"))
                    {
                        User tempUser = Auth.getUser(request.getToken());

                        SendMessage(stream, $"Id: {tempUser.Id} Name: {tempUser.Username} Pw: {tempUser.Password} Coins: {tempUser.Coins} Bio: {tempUser.Bio} Image: {tempUser.Image} Elo: {tempUser.Stats.Elo} Wins: {tempUser.Stats.Wins} Draws: {tempUser.Stats.Draws}");
                    }
                    
                    if (request.Path.EndsWith("/score"))
                    {
                        foreach(var tempUser in Auth.getAll())
                        {
                            Console.WriteLine($"Id: {tempUser.Id} Name: {tempUser.Username} Pw: {tempUser.Password} Coins: {tempUser.Coins} Bio: {tempUser.Bio} Image: {tempUser.Image} Elo: {tempUser.Stats.Elo} Wins: {tempUser.Stats.Wins} Draws: {tempUser.Stats.Draws}");
                        }

                        SendMessage(stream, "Scoreboard wurde geprinted");
                    }

                    if (request.Path.EndsWith("/tradings"))
                    {
                        List<Trade> tempTrades = tradeRepo.GetAll();

                        foreach(Trade hansi in tempTrades)
                        {
                            Console.WriteLine($"{hansi.Id} {hansi.CardId} {hansi.UserId} {hansi.Details["Type"]} {hansi.Details["MinimumDamage"]}  ");
                        }

                        SendMessage(stream, "Liste ausgegeben");

                    }

                    break;

                case "POST":

                    if (request.Path.EndsWith("/battles"))
                    {
                        User tempUser = Auth.getUser(request.getToken());

                        if(!Lobby.joinLobby(tempUser))
                        {
                            SendMessage(stream, "Die Decks sind nicht gefüllt");
                            return;
                        }
                        
                        Lobby.log.printProtocol();
                        
                    

                        //Console.WriteLine($"Winner: {Lobby.log.winner.Username} Loser: {Lobby.log.loser.Username}");



                        if (tempUser == Lobby.log.winner)
                        {
                            tempUser.Stats.updateWinStats();
                            SendMessage(stream, "Du hast gewonnen");
                        }
                        else if (tempUser == Lobby.log.loser)
                        {
                            tempUser.Stats.updateLoseStats();
                            foreach(Card card in tempUser.Deck)
                            {
                                cardRepo.ChangeOwner(card, Lobby.log.winner);
                            }
                            SendMessage(stream, "Du hast verloren");
                        }
                        else
                        {
                            tempUser.Stats.updateDrawStats();
                            SendMessage(stream, "Unentschieden");
                        }


                        Console.WriteLine($"Id: {tempUser.Id} Name: {tempUser.Username} Pw: {tempUser.Password} Coins: {tempUser.Coins} Bio: {tempUser.Bio} Image: {tempUser.Image} Elo: {tempUser.Stats.Elo} Wins: {tempUser.Stats.Wins} Draws: {tempUser.Stats.Draws}");

                        // Statsupdate DB
                        //userRepo.UpdateStats(tempUser);
                        //cardRepo.ChangeDepot(tempUser.Deck.Select(c => c.Id).ToList(), tempUser);



                        //tempUser.Stack = cardRepo.GetCards(tempUser.Id);
                        //tempUser.Deck = cardRepo.GetCards(tempUser.Id, true);
                        //foreach (Card card in tempUser.Deck)
                        //{
                        //    tempUser.Deck.Remove(card);
                        //    tempUser.Stack.Add(card);
                        //}
                        //Auth.updateUser(request.getToken(), tempUser);


                        User tempUser1 = Auth.getUser(request.getToken());
                        Console.WriteLine($"Id: {tempUser1.Id} Name: {tempUser1.Username} Pw: {tempUser1.Password} Coins: {tempUser1.Coins} Bio: {tempUser1.Bio} Image: {tempUser1.Image} Elo: {tempUser1.Stats.Elo} Wins: {tempUser1.Stats.Wins} Draws: {tempUser1.Stats.Draws}");

                        // Change Card on DB

                        // Change Card 
           
                        // TODO: DeckDelete from Table
                        // TODO: UpdateDeck at both players 

                        //SendMessage(stream, "Battle really done");
                    }

                    if (request.Path.EndsWith("/users"))
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

                    if (request.Path.EndsWith("/sessions"))
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

                    if (request.Path.EndsWith("/transactions/packages"))
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

                    if (request.Path.Contains("/tradings/"))
                    {
                        Trade checkTrade = tradeRepo.ByUniq(request.Path.Replace("/tradings/", ""));
                       
                        if(checkTrade == null)
                        {
                            SendMessage(stream, "Kein Trade verfügbar!");
                            return;
                        }

                        User offerdUser = Auth.getUserViaId(checkTrade.UserId);
                        User tradeUser = Auth.getUser(request.getToken());

                        Card offerCard = offerdUser.Stack.First(x => x.Id == checkTrade.CardId);
                        Card tradeCard = tradeUser.Stack.First(x => x.Id == Guid.Parse(request.Body[0].ToString()));

                        if(offerCard == null || tradeCard == null)
                        {
                            SendMessage(stream, "Mind. eine Karte befindet sich nicht in einem Stack");
                            return;
                        }

                        if(offerCard.Damage < double.Parse(checkTrade.Details["MinimumDamage"].ToString()))
                        {
                            SendMessage(stream, "Die Karte hat nicht den gewünschten Damage");
                            return;
                        }

                        if   (!string.Equals(offerCard.Type.ToString(), checkTrade.Details["Type"].ToString(), StringComparison.OrdinalIgnoreCase)) 
                        {
                            SendMessage(stream, "Die Karte hat nicht den passende Type");
                            return;
                        }

                        tradeUser.Stack.Add(offerCard);
                        tradeUser.Stack.Remove(tradeCard);
                        cardRepo.ChangeOwner(tradeCard, offerdUser);

                        offerdUser.Stack.Add(tradeCard);
                        offerdUser.Stack.Remove(offerCard);
                        cardRepo.ChangeOwner(offerCard, tradeUser);

                        tradeRepo.Delete(checkTrade.Id);

                        SendMessage(stream, "Trade findet statt");
                    }

                    if (request.Path.EndsWith("/tradings"))
                    {
                        JObject details = new JObject();

                        foreach (var item in (JObject)request.Body[0])
                        {
                            if (item.Key != "Id" && item.Key != "CardToTrade")
                            {
                                details.Add(item.Key, item.Value);
                            }
                        }

                        Trade hansi = new Trade()
                        {
                            Id = Guid.Parse(request.Body[0]["Id"].ToString()),
                            CardId = Guid.Parse(request.Body[0]["CardToTrade"].ToString()),
                            UserId = Auth.getUser(request.getToken()).Id,
                            Details = details
                        };

                        tradeRepo.Add(hansi);


                        Console.WriteLine($"{hansi.Id} {hansi.CardId} {hansi.UserId} {hansi.Details["Type"]} {hansi.Details["MinimumDamage"]}  ");


                        SendMessage(stream, "OK");
                    }

                    break;

                case "PUT":

                    if (request.Path.EndsWith("/deck"))
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

                    if (request.Path.Contains("/users/"))
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

                case "DELETE":

                    if (request.Path.Contains("/tradings/"))
                    {
                        tradeRepo.Delete(Guid.Parse(request.Path.Replace("/tradings/", "")));
                        SendMessage(stream, "Trade wurde gelöscht");
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



//Console.WriteLine($"{checkTrade.Id} {checkTrade.CardId} {checkTrade.UserId} {checkTrade.Details["Type"]} {checkTrade.Details["MinimumDamage"]}  ");
//Console.WriteLine($"Card {card.Id} {card.Name} {card.Damage}  {card.Type}");