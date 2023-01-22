using MCTG_Brian.Authentication;
using MCTG_Brian.Battle;
using MCTG_Brian.Database;
using MCTG_Brian.Database.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql.Replication.PgOutput.Messages;
using System;
using System.Net.Sockets;
using System.Text;

namespace MCTG_Brian.Server
{
    public static class RestAPI
    {
        private static readonly UserRepository  userRepo = new UserRepository();
        private static readonly CardRepository cardRepo = new CardRepository();
        private static readonly PackRepository packRepo = new PackRepository();
        private static readonly TradeRepository tradeRepo = new TradeRepository();

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
                SendMessage(stream, 403, "Provided user is not an admin");
                return;
            }
            else if (AuthList.Contains(new Tuple<string, string>(request.Method, request.Path)) && Auth.isUserLoggedIn(request.getToken())) { }
            else if (AuthList.Contains(new Tuple<string, string>(request.Method, request.Path)) && (request.getToken() == null))
            {
                SendMessage(stream, 401, "Access token is missing or invalid");
                return;
            }
            else if (AuthList.Contains(new Tuple<string, string>(request.Method, request.Path)))
            {
                SendMessage(stream, 401, "Authorization is needed for this action");
                return;
            }
            else
            {
                SendMessage(stream, 404, "Path not found");
                return;
            }

            switch (request.Method)
            {
                case "GET":

                    if (request.Path.StartsWith("/deck"))
                    {
                        List<Card> tempDeck = Auth.getUser(request.getToken()).Deck;
                        
                        if (tempDeck.Count() <= 0)
                        {
                            SendMessage(stream, 202, $"The requst was fine, but the deck of user ({Auth.getUser(request.getToken()).Username}) is empty");
                            return;
                        }

                        if(request.HasQueryParameter("format", "plain"))
                        { 
                            SendMessage(stream, 200, $"The deck of user ({Auth.getUser(request.getToken()).Username}) has cards, the response contains these");
                            return;
                        }

                        SendMessage(stream, 200, $"The deck of user ({Auth.getUser(request.getToken()).Username}) has cards, the response contains these", tempDeck);
                    }/**/

                    if (request.Path.Contains("/users/"))
                    {
                        string pathUsername = request.Path.Replace("/users/", "");

                        if (Auth.getUser(request.getToken()).Username != pathUsername)
                        {
                            //SendMessage(stream, $"Keine Berechtiung Benutzer {pathUsername} anzuzeigen!");
                            return;
                        }
                      
                        User tempUser = Auth.getUser(request.getToken()); // INKONSITENT
                        //SendMessage(stream, $"Benutzerausgabe: {tempUser.Id} {tempUser.Username} {tempUser.Coins} {tempUser.Bio} {tempUser.Image}");
     
                    }

                    if (request.Path.EndsWith("/cards"))
                    {
                        List<Card> tempStack = Auth.getUser(request.getToken()).Stack;

                        if(tempStack.Count() <= 0)
                        {
                            SendMessage(stream, 202, $"The request was fine, but the user ({Auth.getUser(request.getToken()).Username}) doesn't have any cards");
                            return;
                        }

                        SendMessage(stream, 200, $"The user has cards, the response contains these", tempStack);
                    }/**/

                    if (request.Path.EndsWith("/stats"))
                    {
                        User tempUser = Auth.getUser(request.getToken());

                        SendMessage(stream, 200, "The stats could be retrieved succesfully", tempUser.Stats);
                    }/**/
                    
                    if (request.Path.EndsWith("/score"))
                    {
                        JArray scoreboard = new JArray();

                        foreach (var tempUser in Auth.getAll())
                        {
                            scoreboard.Add($"Id: {tempUser.Id} Name: {tempUser.Username} Elo: {tempUser.Stats.Elo}");
                        }

                        SendMessage(stream, 200, "The scoreboard could be retrieved successfully", scoreboard);
                    }/**/

                    if (request.Path.EndsWith("/tradings"))
                    {
                        List<Trade> tempTrades = tradeRepo.GetAll();

                        foreach(Trade hansi in tempTrades)
                        {
                            Console.WriteLine($"{hansi.Id} {hansi.CardId} {hansi.UserId} {hansi.Details["Type"]} {hansi.Details["MinimumDamage"]}  ");
                        }

                        //SendMessage(stream, "Liste ausgegeben");

                    }

                    break;

                case "POST":

                    if (request.Path.EndsWith("/battles"))
                    {
                        User tempUser = Auth.getUser(request.getToken());

                        if(!Lobby.joinLobby(tempUser))
                        {
                            SendMessage(stream, 200, "Die Decks sind nicht gefüllt");
                            return;
                        }
                        
                        Lobby.log.printProtocol();
                        
                    

                        //Console.WriteLine($"Winner: {Lobby.log.winner.Username} Loser: {Lobby.log.loser.Username}");



                        if (tempUser == Lobby.log.winner)
                        {
                            tempUser.Stats.updateWinStats();
                            SendMessage(stream, 200, "Du hast gewonnen");
                        }
                        else if (tempUser == Lobby.log.loser)
                        {
                            tempUser.Stats.updateLoseStats();
                            foreach(Card card in tempUser.Deck)
                            {
                                cardRepo.ChangeOwner(card, Lobby.log.winner);
                            }
                            SendMessage(stream, 200, "Du hast verloren");
                        }
                        else
                        {
                            tempUser.Stats.updateDrawStats();
                            SendMessage(stream, 200, "Unentschieden");
                        }


                        Console.WriteLine($"Id: {tempUser.Id} Name: {tempUser.Username} Pw: {tempUser.Password} Coins: {tempUser.Coins} Bio: {tempUser.Bio} Image: {tempUser.Image} Elo: {tempUser.Stats.Elo} Wins: {tempUser.Stats.Wins} Draws: {tempUser.Stats.Draws}");

                        // Statsupdate DB
                        userRepo.UpdateStats(tempUser);
                        cardRepo.ChangeDepot(tempUser.Deck.Select(c => c.Id).ToList(), tempUser);



                        tempUser.Stack = cardRepo.GetCards(tempUser.Id);
                        tempUser.Deck = cardRepo.GetCards(tempUser.Id, true);
                        //foreach (Card card in tempUser.Deck)
                        //{
                        //    tempUser.Deck.Remove(card);
                        //    tempUser.Stack.Add(card);
                        //}
                        Auth.updateUser(request.getToken(), tempUser);


                        User tempUser1 = Auth.getUser(request.getToken());
                        Console.WriteLine($"Id: {tempUser1.Id} Name: {tempUser1.Username} Pw: {tempUser1.Password} Coins: {tempUser1.Coins} Bio: {tempUser1.Bio} Image: {tempUser1.Image} Elo: {tempUser1.Stats.Elo} Wins: {tempUser1.Stats.Wins} Draws: {tempUser1.Stats.Draws}");

          
                    }

                    if (request.Path.EndsWith("/users"))
                    {
                        User tempUser = userRepo.ByUniq(Auth.getName(request));

                        if (tempUser != null)
                        {
                            SendMessage(stream, 409, $"User with same username ({tempUser.Username}) already registered", tempUser);
                            return;
                        }

                        tempUser = JsonConvert.DeserializeObject<User>(request.Body[0].ToString());
                        userRepo.Add(tempUser);

                        SendMessage(stream, 201, $"User successfully created");

                    }/**/

                    if (request.Path.EndsWith("/sessions"))
                    {
                        User toCheck = JsonConvert.DeserializeObject<User>(request.Body[0].ToString());
                        User tempUser = userRepo.ByUniq(Auth.getName(request));

                        if (tempUser == null || (tempUser.Username != toCheck.Username || tempUser.Password != toCheck.Password))
                        {
                            SendMessage(stream, 401, "Invalid username/password provided");
                            return;
                        }

                        if (!Auth.loginUser(tempUser))
                        {
                            SendMessage(stream, 409, $"User ({tempUser.Username}) already logged in, no further actions", tempUser);
                            return;
                        }

                        tempUser.Stack = cardRepo.GetCards(tempUser.Id);
                        tempUser.Deck = cardRepo.GetCards(tempUser.Id, true);

                        SendMessage(stream, 200, $"User login successful", tempUser);
                    }/**/

                    if (request.Path.StartsWith("/packages"))
                    {
                        List<Card> newPack = request.Body.ToObject<List<Card>>();

                        if (newPack.Count() != 5)
                        {
                            SendMessage(stream, 400, $"The provided packages did not include the required amount of cards ({newPack.Count()}/5)");
                            return;
                        }

                        if(!packRepo.Add(newPack)) 
                        {
                            SendMessage(stream, 409, "At least one card in the packages already exists");
                            return;
                        }
                        
                        SendMessage(stream, 200, "Packages and cards succesfully created");
                    }/**/

                    if (request.Path.EndsWith("/transactions/packages"))
                    { 
                        Tuple<Guid, List<Card>> package = packRepo.GetRandPackage();

                        if (package == null)
                        {
                            SendMessage(stream, 404, "No card package available for buying");
                            return;
                        }

                        if (Auth.getUser(request.getToken()).Coins < 5)
                        {
                            SendMessage(stream, 403, $"User {Auth.getUser(request.getToken()).Username} has not enough money ({Auth.getUser(request.getToken()).Coins}) for buying a card package");
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

                        SendMessage(stream, 200, $"A package ({package.Item1}) has been succesfully bought for User {Auth.getUser(request.getToken()).Username}");
                    }/**/

                    if (request.Path.Contains("/tradings/"))
                    {
                        Trade checkTrade = tradeRepo.ByUniq(request.Path.Replace("/tradings/", ""));
                       
                        if(checkTrade == null)
                        {
                            SendMessage(stream, 200, "Kein Trade verfügbar!");
                            return;
                        }

                        User offerdUser = Auth.getUserViaId(checkTrade.UserId);
                        User tradeUser = Auth.getUser(request.getToken());

                        Card offerCard = offerdUser.Stack.First(x => x.Id == checkTrade.CardId);
                        Card tradeCard = tradeUser.Stack.First(x => x.Id == Guid.Parse(request.Body[0].ToString()));

                        if(offerCard == null || tradeCard == null)
                        {
                            SendMessage(stream, 200, "Mind. eine Karte befindet sich nicht in einem Stack");
                            return;
                        }

                        if(offerCard.Damage < double.Parse(checkTrade.Details["MinimumDamage"].ToString()))
                        {
                            //SendMessage(stream, "Die Karte hat nicht den gewünschten Damage");
                            return;
                        }

                        if   (!string.Equals(offerCard.Type.ToString(), checkTrade.Details["Type"].ToString(), StringComparison.OrdinalIgnoreCase)) 
                        {
                            SendMessage(stream, 200, "Die Karte hat nicht den passende Type");
                            return;
                        }

                        tradeUser.Stack.Add(offerCard);
                        tradeUser.Stack.Remove(tradeCard);
                        cardRepo.ChangeOwner(tradeCard, offerdUser);

                        offerdUser.Stack.Add(tradeCard);
                        offerdUser.Stack.Remove(offerCard);
                        cardRepo.ChangeOwner(offerCard, tradeUser);

                        tradeRepo.Delete(checkTrade.Id);

                        SendMessage(stream, 200, "Trade findet statt");
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


                        SendMessage(stream, 200, "OK");
                    }

                    break;

                case "PUT":

                    if (request.Path.EndsWith("/deck"))
                    {
                        List<Guid> deckIds = request.Body.ToObject<List<Guid>>();

                        if (deckIds.Count() != 4)
                        {
                            SendMessage(stream, 400, $"The provided deck did not include the required amount of cards ({deckIds.Count()}/4)");
                            return;
                        }

                        //VLLT DANN NOCH EINE STATIC USER.removeDeckToStack

                        //foreach jede karte durch, zuerst alle 4 karten durch, wenn ich eins nicht finde dann huchit
                        //wenn alles gefunden wurde, dann diekarte vom stack in den deck
                        if (cardRepo.ChangeDepot(deckIds, Auth.getUser(request.getToken())) == false)
                        {
                            SendMessage(stream, 403, "At least one of the provided cards does not belong to the user or is not available");
                            return;
                        }

                        Auth.getUser(request.getToken()).Stack = cardRepo.GetCards(Auth.getUser(request.getToken()).Id);
                        Auth.getUser(request.getToken()).Deck = cardRepo.GetCards(Auth.getUser(request.getToken()).Id, true);

                        SendMessage(stream, 200, "The deck has been successfully configured");
                    }/**/

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

                        //SendMessage(stream, $"Neuer User mit {updateUser.Id} {updateUser.Password} {updateUser.Username} {updateUser.Coins} {updateUser.Image} {updateUser.Bio}");
                    }

                    break;


                /*
                 monster fusion funktion. zwei carden werden gegeben, die schwächere wird entfernt und die stärke deren wird stärker von 
                (Damage Card 1 * 1.1 + Damage Card 2 * 1.1) / 2
                (Damage Card 1 + Damage Card 2) / 2 + (Damage Card 1 + Damage Card 2) / 10
                 */


                case "DELETE":

                    if (request.Path.Contains("/tradings/"))
                    {
                        tradeRepo.Delete(Guid.Parse(request.Path.Replace("/tradings/", "")));
                        SendMessage(stream, 200, "Trade wurde gelöscht");
                    }

                    break;
            }
        }

        public static void SendMessage(NetworkStream stream, int statusCode, string? info = null, Object obj = null)
        {
            string fullResponse = ResponseContainer.createBody(info, obj);
            Console.WriteLine($"Ausgabe: {info}");

            ResponseContainer response = new ResponseContainer(statusCode, obj != null ? "application/json" : "text/plain", fullResponse);
            string responseString = response.HttpResponseToString();

            response.sendClient(stream, responseString);
            
        }
    }
}
