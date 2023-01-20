using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace MCTG_Brian.Database
{

    public abstract class Repository<T>
    {
        private const string _CONNECTION_STRING = "Host = localhost; Username = postgres; Password = qwerqwer; Database = postgres";
        public static NpgsqlConnection? Connection { get; private set; }         // die Datenbankverbindung
        
        public Repository() 
        {
            Connection = new NpgsqlConnection(_CONNECTION_STRING);              // die Db-Verbindung her
            Connection.Open();
        }

        public abstract void Add(T elm);                                        // Add-Methode 
        public abstract void Update(T elm);                                     // Update-Methode
        public abstract void Delete(Guid id);                                   // Delete-Methode
        public abstract T ByUniq(string Username);                           // Delete-Methode
    }

    public class StatsRepository : Repository<Stats>
    {
        private object lockThread = new object(); // TODO: brauch ich das? testen und weggeben

        public override void Add(Stats stat)
        {
            lock (lockThread)
            {

                using (var command = Connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO stats (userid, elo, wins, loses, draws) VALUES (@UserId, @Elo, @Wins, @Loses, @Draws)";
                   // command.Parameters.AddWithValue("UserId", stat.UserId);
                    command.Parameters.AddWithValue("Elo", stat.Elo);
                    command.Parameters.AddWithValue("Wins", stat.Wins);
                    command.Parameters.AddWithValue("Loses", stat.Loses);
                    command.Parameters.AddWithValue("Draws", stat.Draws);
                    command.Prepare();
                    command.ExecuteNonQuery();
                }
            }
        }
        public override void Update(Stats stat)
        {
            lock (lockThread)
            {
                using (var command = Connection.CreateCommand())
                {
                    //command.CommandText = "UPDATE stats SET elo = @Elo, wins = @Wins, loses = @Loses, draws = @Draws WHERE userid = @UserId";
                    command.CommandText = "INSERT INTO stats (userid, elo, wins, loses, draws) VALUES (@UserId, @Elo, @Wins, @Loses, @Draws) ON CONFLICT (userid) DO UPDATE SET elo = excluded.elo, wins = excluded.wins, loses = excluded.loses, draws = excluded.draws;";
                    //command.Parameters.AddWithValue("UserId", stat.UserId);
                    command.Parameters.AddWithValue("Elo", stat.Elo);
                    command.Parameters.AddWithValue("Wins", stat.Wins);
                    command.Parameters.AddWithValue("Loses", stat.Loses);
                    command.Parameters.AddWithValue("Draws", stat.Draws);
                    command.Prepare();
                    command.ExecuteNonQuery();
                }
            }
        }
        public override void Delete(Guid id) {}
        public override Stats ByUniq(string userGuid)
        {
            using (var command = Connection.CreateCommand())
            {
                
                command.CommandText = "SELECT stats.id, userid, users.name, elo, wins, loses, draws FROM stats Join users on stats.userid = users.id WHERE userId = @Id";
                command.Parameters.AddWithValue("Id", Guid.Parse(userGuid));

                lock(lockThread)
                {

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var stat = new Stats
                        {
                            Id = reader.GetGuid(0),
                            //UserId = reader.GetGuid(1),
                            //Username = reader.GetString(2),
                            Elo = reader.GetInt32(3),
                            Wins = reader.GetInt32(4),
                            Loses = reader.GetInt32(5),
                            Draws = reader.GetInt32(6)
                        };
                        return stat;
                    }
                    else
                    {
                        return null;
                    }
                }
                }
            }
        }
        public List<Stats> GetAll()
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT stats.id, userid, users.name, elo, wins, loses, draws FROM stats Join users on stats.userid = users.id";

                using (var reader = command.ExecuteReader())
                {
                    List<Stats> stats = new List<Stats>();
                    while (reader.Read())
                    {
                        var stat = new Stats
                        {
                            Id = reader.GetGuid(0),
                            //UserId = reader.GetGuid(1),
                            //Username = reader.GetString(2),
                            Elo = reader.GetInt32(3),
                            Wins = reader.GetInt32(4),
                            Loses = reader.GetInt32(5),
                            Draws = reader.GetInt32(6)
                        };
                        stats.Add(stat);
                    }
                    return stats;
                }
            }
        }
    }
    public class UserRepository : Repository<User>
    {
        private object lockThread = new object(); // TODO: brauch ich das? testen und weggeben
        public override void Add(User user)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO users (name, password) VALUES (@Name, @Password) RETURNING id;";
                command.Parameters.AddWithValue("@Name", user.Username);
                command.Parameters.AddWithValue("@Password", user.Password);
                var user_id = (Guid)command.ExecuteScalar();

                command.CommandText = "INSERT INTO stats (userid) VALUES (@user_id);";
                command.Parameters.AddWithValue("@user_id", user_id);
                command.Prepare();
                command.ExecuteNonQuery();
            }
        }

        public override void Update(User user)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "UPDATE users SET name = @Name, password = @Password, coins = @Coins, bio = @Bio, image = @Image WHERE id = @Id";
                command.Parameters.AddWithValue("Id", user.Id);
                command.Parameters.AddWithValue("Name", user.Username);
                command.Parameters.AddWithValue("Password", user.Password);
                command.Parameters.AddWithValue("Coins", user.Coins);
                command.Parameters.AddWithValue("Bio", user.Bio == null ? DBNull.Value : user.Bio);
                command.Parameters.AddWithValue("Image", user.Image == null ? DBNull.Value : user.Image);

                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override void Delete(Guid id)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM users WHERE id = @Id";
                command.Parameters.AddWithValue("Id", id);

                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override User? ByUniq(string Username)
        { //TODO: VLLT 3 seperate abfragen, und jede einzelene befüllt unterobjekt des users (erste abfrage den user, zweite abfrage die stats und dritte abfrage den stack)
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT users.id, users.name, users.password, users.coins, users.bio, users.image, stats.id, stats.elo, stats.wins, stats.loses, stats.draws FROM users JOIN stats ON stats.userid = users.id WHERE users.name = @Name";
                command.Parameters.AddWithValue("Name", Username);

                lock (lockThread)
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var user = new User
                            {
                                Id = reader.GetGuid(0),
                                Username = reader.GetString(1),
                                Password = reader.GetString(2),
                                Coins = reader.GetInt32(3),
                                Bio = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Image = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Stats = new Stats
                                {
                                    Id = reader.GetGuid(6),
                                    Elo = reader.GetInt32(7),
                                    Wins = reader.GetInt32(8),
                                    Loses = reader.GetInt32(9),
                                    Draws = reader.GetInt32(10)
                                },
                                Stack = new List<Card>(),
                                Deck = new List<Card>()
                            };


                            return user;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }
    }
    public class CardRepository : Repository<Card>
    { 
        public void Add(User user, Card card)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO cards (id, owner, name, damage) VALUES (@Id, @Owner, @Name, @Damage)";
                command.Parameters.AddWithValue("Id", card.Id);
                command.Parameters.AddWithValue("Owner", user.Id);
                command.Parameters.AddWithValue("Name", card.Name);
                command.Parameters.AddWithValue("Damage", card.Damage);
                command.Prepare();
                command.ExecuteNonQuery();
            }
        }

        public override void Add(Card card) { }
        public override void Update(Card card)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "UPDATE cards SET name = @Name, damage = @Damage WHERE id = @Id";
                command.Parameters.AddWithValue("Id", card.Id);
                command.Parameters.AddWithValue("Name", card.Name);
                command.Parameters.AddWithValue("Damage", card.Damage);

                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override void Delete(Guid id)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM cards WHERE id = @Id";
                command.Parameters.AddWithValue("Id", id);

                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override Card? ByUniq(string Id)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT id, name, damage FROM cards WHERE id = @Id";
                command.Parameters.AddWithValue("Id", Guid.Parse(Id));

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var json = new JObject
                        {
                            ["Id"] = reader.GetGuid(0),
                            ["Name"] = reader.GetString(1),
                            ["Damage"] = reader.GetDouble(2)
                        };
                        //return new Card(json);
                        return new Card();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public List<Card> GetCards(Guid userId, bool obj = false)
        {
            var cards = new List<Card>();
           

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT id, name, damage, owner FROM cards WHERE owner = @OwnerId AND depot = @Depot";
                command.Parameters.AddWithValue("OwnerId", userId);
                command.Parameters.AddWithValue("Depot", obj ? "deck" : "stack");

                using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cards.Add(new Card
                            {
                                Id = reader.GetGuid(0),
                                Name = reader.GetString(1),
                                Damage = reader.GetDouble(2),
                            });
                        }
                    }
                
            }

            return cards ?? null;
        }
    }
    public class PackRepository : Repository<List<Card>>
    {
        public int Count()
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM packages";


                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetInt32(0);
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }
        public override void Add(List<Card> pack)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO store (card1, card2, card3, card4, card5) VALUES (@Card1, @Card2, @Card3, @Card4, @Card5)";

                for (int i = 0; i < 5; i++)
                {
                    var param = new NpgsqlParameter($"@Card{i + 1}", NpgsqlDbType.Json);
                    param.Value = JsonConvert.SerializeObject(pack[i]);
                    command.Parameters.Add(param);
                }

                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override void Update(List<Card> packs)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "UPDATE packages SET packages = @Packages WHERE id = @Id";
               // command.Parameters.AddWithValue("Id", packs[0]);
                command.Parameters.AddWithValue("Packages", JsonConvert.SerializeObject(packs));

                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override void Delete(Guid id)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM store WHERE id = @Id";
                command.Parameters.AddWithValue("Id", id);

                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override List<Card> ByUniq(string id)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT packages FROM packages WHERE id = @Id";
                command.Parameters.AddWithValue("Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var packages = JsonConvert.DeserializeObject<List<Card>>(reader.GetString(0));
                        return packages;
                    }
                    else
                    {
                        return new List<Card>();
                    }
                }
            }
        }
        public Tuple<Guid, List<Card>> GetRandPackage()
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM store LIMIT(1)";
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Guid PackageId = reader.GetGuid(0);
                        List<Card> cards = new List<Card>();

                        for (int i = 1; i <= 5; i++)
                        {
                            cards.Add(JsonConvert.DeserializeObject<Card>(reader.GetString(i)));
                        }
                        return new Tuple<Guid, List<Card>>(PackageId, cards);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
    /*
    public class StackRepository : Repository<Tuple<User, Guid>>
    {
        private object lockThread = new object(); // TODO: brauch ich das? testen und weggeben

        public override void Add(Tuple<User, Guid> stack)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO stacks (id, owner, cardid) VALUES (@Id, @User, @Card)";
                command.Parameters.AddWithValue("Id", stack.Item2);
                command.Parameters.AddWithValue("User", stack.Item1.Id);
                command.Parameters.AddWithValue("Card", stack.Item2);
                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override void Update(Tuple<User, Guid> stack)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "UPDATE stacks SET user = @User, card = @Card WHERE id = @Id";
               // command.Parameters.AddWithValue("Id", stack.Key);
               // command.Parameters.AddWithValue("User", stack.Key);
               // command.Parameters.AddWithValue("Card", stack.Value);

                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override void Delete(Guid id)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM stacks WHERE id = @Id";
                command.Parameters.AddWithValue("Id", id);

                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public List<Card> GetAll(Guid id)
        {
            using (var command = Connection.CreateCommand())
            {
                ;
                command.CommandText = "SELECT cards.id, name, damage FROM stacks JOIN cards ON stacks.cardid = cards.id WHERE owner = @User";
                command.Parameters.AddWithValue("User", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {

                        List<Card> cards = new List<Card>();
                        while (reader.Read())
                        {
                            var json = new JObject
                            {
                                ["Id"] = reader.GetGuid(0),
                                ["Name"] = reader.GetString(1),
                                ["Damage"] = reader.GetDouble(2)
                            };

                            cards.Add(new Card(json));
                        }
                        return cards;

                    }
                    else
                    {
                        return new List<Card>();
                    }
                }
            }

        }
        public override Tuple<User, Guid> ByUniq(string id)
        {
            return null;
        //    using (var command = Connection.CreateCommand())
        //    {
        //        command.CommandText = "SELECT user, card FROM stacks WHERE id = @Id";
        //        command.Parameters.AddWithValue("Id", id);

        //        using (var reader = command.ExecuteReader())
        //        {
        //            if (reader.Read())
        //            {
        //                var user = reader.GetString(0);
        //                var card = reader.GetString(1);
        //                var stack = new Dictionary<User, Card>();
        //                stack.Add(user, card);
        //                return stack;
        //            }
        //            else
        //            {
        //                return new Dictionary<User, Card>();
        //            }
        //        }
        //    }
        }
      
    }
    */
    public class DeckRepository : Repository<Dictionary<Guid, Guid>>
    {
        private object lockThread = new object();
        
        public override void Add(Dictionary<Guid, Guid> Deck)
            {

            foreach(var x in Deck)
            {
                Console.WriteLine(x.Key + " " + x.Value);
            }
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO deck (userid, card1, card2, card3, card4) VALUES (@User, @Card1, @Card2, @Card3, @Card4)";
                command.Parameters.AddWithValue("User", Deck.First().Value);

                int i = 1;
                foreach(var x in Deck)
                {
                    string tupleName = $"Card{i}";
                    command.Parameters.AddWithValue(tupleName, x.Key);
                    i++;
                }
                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override void Update(Dictionary<Guid, Guid> Deck)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "UPDATE deck SET card1 = @Card1, card2 = @Card2, card3 = @Card3, card4 = @Card4 WHERE userid = @User";
                //command.Parameters.AddWithValue("User", userId);
                //command.Parameters.AddWithValue("Card1", card.Id);
                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override void Delete(Guid id)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM deck WHERE id = @Id";
                command.Parameters.AddWithValue("Id", id);

                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public List<Card> GetAll(Guid userId)
        {

            lock (lockThread)
            {
                using (var command = Connection.CreateCommand())
                {

                    command.CommandText = "SELECT cards.id, cards.name, cards.damage FROM deck JOIN cards ON deck.card1 = cards.id OR deck.card2 = cards.id OR deck.card3 = cards.id OR deck.card4 = cards.id WHERE deck.userid = @Id";
                    command.Parameters.AddWithValue("Id", userId);

                        using (var reader = command.ExecuteReader())
                        {
                            List<Card> cards = new List<Card>();
                            while (reader.Read())
                            {
                                var json = new JObject
                                {
                                    ["Id"] = reader.GetGuid(0),
                                    ["Name"] = reader.GetString(1),
                                    ["Damage"] = reader.GetDouble(2)
                                };
                                cards.Add(new Card(json));
                            }

                            return cards;
                    }
                }
            }
        }

   
        public override Dictionary<Guid, Guid> ByUniq(string id)
        {
            return null;
            //    using (var command = Connection.CreateCommand())
            //    {
            //        command.CommandText = "SELECT user, card FROM stacks WHERE id = @Id";
            //        command.Parameters.AddWithValue("Id", id);

            //        using (var reader = command.ExecuteReader())
            //        {
            //            if (reader.Read())
            //            {
            //                var user = reader.GetString(0);
            //                var card = reader.GetString(1);
            //                var stack = new Dictionary<User, Card>();
            //                stack.Add(user, card);
            //                return stack;
            //            }
            //            else
            //            {
            //                return new Dictionary<User, Card>();
            //            }
            //        }
            //    }
        }

    }
}



/*
 
  using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var user = new User
                            {
                                Id = reader.GetGuid(0),
                                Username = reader.GetString(1),
                                Password = reader.GetString(2),
                                Coins = reader.GetInt32(3),
                                Bio = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Image = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Stats = new Stats
                                {
                                    Id = reader.GetGuid(6),
                                    Elo = reader.GetInt32(7),
                                    Wins = reader.GetInt32(8),
                                    Loses = reader.GetInt32(9),
                                    Draws = reader.GetInt32(10)
                                },
                                Stack = new List<Card>()
                            };

                            user.Stack.Add(new Card
                            {
                                Id = reader.GetGuid(11),
                                Name = reader.GetString(12),
                                Damage = reader.GetDouble(13),
                            });
                            while (reader.Read())
                            {
                                user.Stack.Add(new Card
                                {
                                    Id = reader.GetGuid(11),
                                    Name = reader.GetString(12),
                                    Damage = reader.GetDouble(13),
                                });
                            }
                            return user;
                        }
                        else
                        {
                            return null;
                      
 */