using MCTG_Brian.Auth;
using MCTG_Brian.Database;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MCTG_Brian.Server
{
    public static class RestAPI
    {        

        public static void HandleRequest(RequestContainer request, NetworkStream stream)
        {
            UserRepository.InitDb();
            CardRepository.InitDb();
            PackRepository.InitDb();
            DeckRepository.InitDb();
            StackRepository.InitDb();


            switch (request.Method)
            {
                case "GET":

                    if(request.Path == "/cards")
                    {
                        User loggedInUser = UserRepository.USERS.ByUniq( request.getNameFromToken() );
                        List<Card> UsersStack = new StackRepository().GetAll(loggedInUser.Id);
               
                        foreach(var Card in UsersStack)
                        {
                            Console.WriteLine(Card.Id + " " + Card.Name + " " + Card.Damage);
                        }

                        SendMessage(stream, "[GET] User bekommen");
                    }

                    if(request.Path == "/deck")
                    {
                        
                        List<Card> Decks = new DeckRepository().GetAll( Guid.Parse("b2b63683-3454-4f50-8362-992ced439808") );

                        foreach (var Card in Decks)
                        {
                            Console.WriteLine(Card.Id + " " + Card.Name + " " + Card.Damage);
                        }
                    }
                   
                    break;

                case "POST":
                   
                    if(request.Path == "/users") 
                    {
                        if (UserRepository.USERS.ByUniq( (string)request.Body[0]["Username"] ) == null)
                        {
                            UserRepository.USERS.Add( new User((JObject)request.Body[0]) );
                            SendMessage(stream, "User hinzugefügt");
                        }
                        else
                        {
                            SendMessage(stream, "User gibt es schon");
                        }
                        
                    }
                    
                    if(request.Path == "/sessions")
                    {
                        string catchedUsername = (string)request.Body[0]["Username"];
                        User loggedInUser = UserRepository.USERS.ByUniq(catchedUsername);
                        User dataBaseUser = new User( (JObject)request.Body[0] );

                        if (loggedInUser.Name == dataBaseUser.Name && loggedInUser.Password == dataBaseUser.Password)
                        {
                            Authentication.loginUser(loggedInUser);
                            SendMessage(stream, "User gibt es und wurde eingeloggt");
                        }
                        else
                        {
                            SendMessage(stream, "Die Credential sind falsch");
                        }
                    }

                    if (request.Path == "/packages" /* && user is admin */)
                    {
                        List<Guid> GuidCollection = new List<Guid>();

                        foreach (JObject card in request.Body)
                        {
                            CardRepository.CARDS.Add( new Card(card) );
                            GuidCollection.Add(Guid.Parse( (string)card["Id"]) );
                        }
                        PackRepository.PACKS.Add( GuidCollection );

                        SendMessage(stream, "Karten wurde gesendet");
                    }


                    if (request.Path == "/transactions/packages")
                    {                       
                        Tuple<Guid, List<Guid>> newPackage = new PackRepository().GetRandPackage();

                        string Username = request.getNameFromToken();
                        User loggedInUser = UserRepository.USERS.ByUniq(Username);


                        PackRepository.PACKS.Delete(newPackage.Item1);
                        foreach (var x in newPackage.Item2)
                        {
                            StackRepository.STACK.Add(new Tuple<User, Guid>(loggedInUser, x));
                        }

                        SendMessage(stream, "Karten wurde gesendet");
                    }

                    break;



                case "DELETE":
                    // coming soon ...
                    break;


                case "PUT":
                    if(request.Path == "/deck")
                    {
                        Dictionary<Guid, Guid> deck = new Dictionary<Guid, Guid>();
                        string Username = request.getNameFromToken();
                        User user = UserRepository.USERS.ByUniq(Username);


                        foreach (var x in request.Body)
                        {
                            Guid cardId = Guid.Parse((string)x);
                            deck.Add(cardId, user.Id);
                        }

                        DeckRepository.DECKS.Add(deck);
                        
                        SendMessage(stream, "Es klappt");
                    }
                    break;
            
            }

            UserRepository.CloseDb();
            CardRepository.CloseDb();
            PackRepository.CloseDb();
            DeckRepository.CloseDb();
            StackRepository.CloseDb();
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
