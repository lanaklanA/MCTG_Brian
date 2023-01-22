using MCTG_Brian.Authentication;
using MCTG_Brian.Battle;
using MCTG_Brian.Database;
using MCTG_Brian.Database.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            new Tuple<string, string>("GET", "/deck?format=plain"),
            new Tuple<string, string>("GET", "/stats"),
            new Tuple<string, string>("GET", "/score"),
            new Tuple<string, string>("GET", "/tradings"), 

            new Tuple<string, string>("PUT", "/deck"),

            new Tuple<string, string>("POST", "/packages"),
            new Tuple<string, string>("POST", "/battles"),
            new Tuple<string, string>("POST", "/tradings"),
            new Tuple<string, string>("POST", "/fusion"),
            new Tuple<string, string>("POST", "/transactions/packages"),
        };
        private static List<Tuple<string, string>> AdminList = new List<Tuple<string, string>>()
        {
            new Tuple<string, string>("POST", "/packages"),
        };

        /// <summary>
        /// Handles the incoming client request, depending on path and body. After processing the response is send to the client
        /// </summary>
        /// <param name="request"></param>
        /// <param name="stream"></param>
        public static void HandleRequest(RequestContainer request, NetworkStream stream)
        {
            AuthList.Add(new Tuple<string, string>("GET", $"/users/{request.Path.Replace("/users/", "")}"));
            AuthList.Add(new Tuple<string, string>("PUT", $"/users/{request.Path.Replace("/users/", "")}"));
            AuthList.Add(new Tuple<string, string>("POST", $"/tradings/{request.Path.Replace("/tradings/", "")}"));
            AuthList.Add(new Tuple<string, string>("DELETE", $"/tradings/{request.Path.Replace("/tradings/", "")}"));
            AuthList.Add(new Tuple<string, string>("POST", $"/tradings/{request.Path.Replace("/tradings/", "")}"));

            /// <summary>
            /// Check for existing path, Authentication and Authorization
            /// </summary>
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

            /// <summary>
            /// Does the process depending on the path
            /// </summary>
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

                        if (request.HasQueryParameter("format", "plain"))
                        { 
                            StringBuilder sb = new StringBuilder();

                            foreach(Card card in tempDeck)
                            {
                                sb.Append($"ID: {card.Id} Name: {card.Name} Damage {card.Damage} Type: {card.Type} Element: {card.Element}  Monster: {card.Monster}\n");
                            }
                            
                            SendMessage(stream, 200, $"{sb}\n");
                            return;
                        }

                        SendMessage(stream, 200, $"The deck of user ({Auth.getUser(request.getToken()).Username}) has cards, the response contains these", tempDeck);
                    }/**/

                    if (request.Path.Contains("/users/"))
                    {
                        string pathUsername = request.Path.Replace("/users/", "");
                        User tempUser = Auth.getUser(request.getToken());

                        if (Auth.getUserViaName(pathUsername) == null) 
                        {
                            SendMessage(stream, 404, $"User ({pathUsername}) not found");
                            return;
                        }

                        if (pathUsername != tempUser.Username)
                        {
                            SendMessage(stream, 401, $"Keine Berechtiung Benutzer {pathUsername} als {tempUser.Username} anzuzeigen!");
                            return;
                        }


                        tempUser = Auth.getUserViaName(pathUsername);
                     
                        SendMessage(stream, 200, $"Data successfully retrieved", tempUser);
                    }/**/

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
                        var usersList = Auth.getAll().OrderByDescending(x => x.Stats.Elo);

                        foreach (User tempUser in usersList)
                        {
                            scoreboard.Add($"Id: {tempUser.Id} Name: {tempUser.Username} Elo: {tempUser.Stats.Elo}");
                        }

                        SendMessage(stream, 200, "The scoreboard could be retrieved successfully", scoreboard);
                    }/**/

                    if (request.Path.EndsWith("/tradings"))
                    {
                        List<Trade> tempTrades = tradeRepo.GetAll();

                        if(tempTrades == null)
                        {
                            SendMessage(stream, 205, "The request was fine, but there are no trading deals available");
                            return;
                        }

                        SendMessage(stream, 200, "There are trading deals available, the repsones contains these", tempTrades);
                    }/**/

                    break;

                case "POST":

                    if (request.Path.EndsWith("/battles"))
                    {
                        User tempUser = Auth.getUser(request.getToken());

                        if(!Lobby.joinLobby(tempUser))
                        {
                            SendMessage(stream, 400, "Provided deck do not contain the right amount of cards");
                            return;
                        }
  
                        if (tempUser == Lobby.log.winner)
                        {
                            tempUser.Stats.updateWinStats();
                            tempUser.Deck = Lobby.log.winner.Deck;
                            SendMessage(stream, 200, "The battle has been carried out successfully: You are the winner", Lobby.log.protocol, true);
                        }
                        else if (tempUser == Lobby.log.loser)
                        {
                            tempUser.Stats.updateLoseStats();
                            tempUser.Deck = Lobby.log.loser.Deck;
                            SendMessage(stream, 200, "The battle has been carried out successfully: You are the loser", Lobby.log.protocol, true);
                        }
                        else
                        {
                            tempUser.Stats.updateDrawStats();
                            SendMessage(stream, 200, "The battle has been carried out successfully: It is a draw", Lobby.log.protocol, true);
                        }

                        Card.clearDeck(tempUser);

                        userRepo.UpdateStats(tempUser);
                        cardRepo.pushCards(tempUser);
                    }/**/

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
                            SendMessage(stream, 403, $"User {Auth.getUser(request.getToken()).Username} has not enough money ({Auth.getUser(request.getToken()).Coins}/5) for buying a card package");
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
                            SendMessage(stream, 404, "The provided deal ID was not found.");
                            return;
                        }

                        User offerdUser = Auth.getUserViaId(checkTrade.UserId);
                        User tradeUser = Auth.getUser(request.getToken());
                        
                        if(offerdUser == tradeUser)
                        {
                            SendMessage(stream, 409, "You can not trade with yourself");
                            return;
                        }

                        Card offerCard = offerdUser.Stack.FirstOrDefault(x => x.Id == checkTrade.CardId);
                        Card tradeCard = tradeUser.Stack.FirstOrDefault(x => x.Id == Guid.Parse(request.Body[0].ToString()));

                        if (offerCard == null || tradeCard == null)
                        {
                            SendMessage(stream, 403, "The offered card is not owned by the user, or the requirements are not met (Type, MinimumDamage), or the offered card is locked in the deck.");
                            return;
                        }

                        if (offerCard.Damage < double.Parse(checkTrade.Details["MinimumDamage"].ToString()))
                        {
                            SendMessage(stream, 403, "The offered card is not owned by the user, or the requirements are not met (Type, MinimumDamage), or the offered card is locked in the deck.");
                            return;
                        }

                        if (!string.Equals(offerCard.Type.ToString(), checkTrade.Details["Type"].ToString(), StringComparison.OrdinalIgnoreCase)) 
                        {
                            SendMessage(stream, 403, "The offered card is not owned by the user, or the requirements are not met (Type, MinimumDamage), or the offered card is locked in the deck.");
                            return;
                        }

                        offerdUser.Stack.Add(tradeCard);
                        offerdUser.Stack.Remove(offerCard);
                        cardRepo.pushCards(offerdUser);

                        tradeUser.Stack.Add(offerCard);
                        tradeUser.Stack.Remove(tradeCard);
                        cardRepo.pushCards(tradeUser);
                          
                        tradeRepo.Delete(checkTrade.Id);

                        SendMessage(stream, 200, "Trading deal successfully executed.");
                    }/**/

                    if (request.Path.EndsWith("/tradings"))
                    {
                        JObject details = new JObject();
                        Trade tempTrade = tradeRepo.ByUniq(request.Body[0]["Id"].ToString());

                        if(tempTrade != null)
                        {
                            SendMessage(stream, 409, "A deal with this ID already exists");
                            return;
                        }

                        foreach (var item in (JObject)request.Body[0])
                        {
                            if (item.Key != "Id" && item.Key != "CardToTrade")
                            {
                                details.Add(item.Key, item.Value);
                            }
                        }

                        tempTrade = new Trade()
                        {
                            Id = Guid.Parse(request.Body[0]["Id"].ToString()),
                            CardId = Guid.Parse(request.Body[0]["CardToTrade"].ToString()),
                            UserId = Auth.getUser(request.getToken()).Id,
                            Details = details
                        };

                        tradeRepo.Add(tempTrade);

                        SendMessage(stream, 200, "Trading offer successfully executed", tempTrade);
                    }/**/

                    if (request.Path.EndsWith("/fusion"))
                    {
                        Tuple<Guid, Guid> cardsToFuse = new Tuple<Guid, Guid>(Guid.Parse(request.Body[0].ToString()), Guid.Parse(request.Body[1].ToString()));

                        User tempUser = Auth.getUser(request.getToken());
                        Card card1 = tempUser.Stack.FirstOrDefault(x => x.Id == cardsToFuse.Item1);
                        Card card2 = tempUser.Stack.FirstOrDefault(x => x.Id == cardsToFuse.Item2);

                        if (card1 == null || card2 == null)
                        {
                            SendMessage(stream, 400, "Provided card is not in stack");
                            return;
                        }

                        if(tempUser.Coins < 10)
                        {
                            SendMessage(stream, 403, $"User {tempUser.Username} has not enough money ({tempUser.Coins}/10) for buying a card package");
                            return;
                        }

                        if (Auth.getUser(request.getToken()).Coins < 5)
                        {
                            SendMessage(stream, 403, $"User {Auth.getUser(request.getToken()).Username} has not enough money ({Auth.getUser(request.getToken()).Coins}) for buying a card package");
                            return;
                        }
                        
                        Card strongerCard = (card1.Damage > card2.Damage) ? card1 : card2;
                        Card weakerCard = (card1.Damage < card2.Damage) ? card1 : card2;
                        strongerCard.Damage = (strongerCard.Damage * 1.5 + weakerCard.Damage * 1.5) / 2;
                        weakerCard.Damage = (weakerCard.Damage * 0.5);

                        Auth.getUser(request.getToken()).Coins -= 10;

                        userRepo.Update(tempUser);
                        cardRepo.pushCards(tempUser);

                        SendMessage(stream, 200, $"Fusion between two cards is accepted");
                    }/**/

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

                        User tempUser = Auth.getUser(request.getToken());
                        
                        if(tempUser.Stack.Select(x => x.Id).Intersect(deckIds).Count() != deckIds.Count())
                        {
                            SendMessage(stream, 403, "At least one of the provided cards does not belong to the user or is not available");
                            return;
                        }

                        Card.clearDeck(tempUser);

                        List<Card> cardsToTransfer = tempUser.Stack.Where(x => deckIds.Contains(x.Id)).ToList();

                        foreach (Card card in cardsToTransfer)
                        {
                            tempUser.Stack.Remove(card);
                            tempUser.Deck.Add(card);
                        }

                        cardRepo.pushCards(Auth.getUser(request.getToken()));

                        SendMessage(stream, 200, "The deck has been successfully configured", tempUser.Deck);
                    }/**/

                    if (request.Path.Contains("/users/"))
                    {
                        User tempUser = Auth.getUser(request.getToken());
                        string pathUsername = request.Path.Replace("/users/", "");
                        if(pathUsername != tempUser.Username)
                        {
                            SendMessage(stream, 403, "You do not hace permission for this");
                            return;
                        }

                        if (tempUser == null)
                        {
                            SendMessage(stream, 404, "User not found");
                            return;
                        }

                        JObject jsonUser = (JObject)request.Body[0];

                        tempUser.Username = jsonUser["Name"].ToString();
                        tempUser.Bio = jsonUser["Bio"].ToString();
                        tempUser.Image = jsonUser["Image"].ToString();

                        userRepo.Update(tempUser);

                        SendMessage(stream, 200, "User sucessfully updated", tempUser);
                    }/**/

                    break;

                case "DELETE":
                  
                    if (request.Path.Contains("/tradings/"))
                    {
                        Trade tempTrade = tradeRepo.ByUniq(request.Path.Replace("/tradings/", ""));
                        
                        if(tempTrade == null)
                        {
                            SendMessage(stream, 404, "The provided deal ID was not found");
                            return;
                        }

                        User tempUser = Auth.getUser(request.getToken());
                        Card checkStack = tempUser.Stack.FirstOrDefault(x => x.Id == tempTrade.CardId);

                        if(checkStack == null)
                        {
                            SendMessage(stream, 403, "The deal contains a card that is not owned by the user");
                            return;
                        }

                        tradeRepo.Delete(tempTrade.Id);
                        SendMessage(stream, 200, "Trading deal successfully deleted");
                    }/**/

                    break;
            }
        }

        /// <summary>
        /// Build an body of info and the object sends it to the client
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="statusCode"></param>
        /// <param name="info"></param>
        /// <param name="obj"></param>
        /// <param name="proto"></param>
        public static void SendMessage(NetworkStream stream, int statusCode, string? info = null, Object obj = null, bool proto = false)
        {
            string fullResponse = ResponseContainer.createBody(info, obj, proto);
            Console.WriteLine($"Output: {info}");

            ResponseContainer response = new ResponseContainer(statusCode, obj != null ? "application/json" : "text/plain", fullResponse);
            string responseString = response.HttpResponseToString();

            response.sendClient(stream, responseString);
            
        }
    }
}
