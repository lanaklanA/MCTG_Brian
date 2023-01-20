using MCTG_Brian.Database.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace MCTG_Brian.Database
{
    //TODO CHANGE LOCK TO syncPrimitive
    public abstract class Repository<T>
    {
        public static NpgsqlConnection? Connection { get; private set; }         // die Datenbankverbindung
        private const string _CONNECTION_STRING = "Host = localhost; Username = postgres; Password = qwerqwer; Database = postgres";
        
        public Repository() 
        {
            Connection = new NpgsqlConnection(_CONNECTION_STRING);              // die Db-Verbindung her
            Connection.Open();
        }

        public abstract void Add(T elm);                                        // Add-Methode 
        public abstract void Update(T elm);                                     // Update-Methode
        public abstract T ByUniq(string Username);                           // Delete-Methode
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
        public void UpdateStats(User user)
        {
            lock(lockThread)
            {

                using (var command = Connection.CreateCommand())
                {
                    command.CommandText = "UPDATE stats SET elo = @Elo, wins = @Wins, loses = @Loses, draws = @Draws WHERE userid = @Id";
                    command.Parameters.AddWithValue("Id", user.Id);
                    command.Parameters.AddWithValue("Elo", user.Stats.Elo);
                    command.Parameters.AddWithValue("Wins", user.Stats.Wins);
                    command.Parameters.AddWithValue("Loses", user.Stats.Loses);
                    command.Parameters.AddWithValue("Draws", user.Stats.Draws);

                    command.Prepare();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
    public class CardRepository : Repository<Card> // vllt zu tuple<user, card> umändenr
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

        public bool ChangeDepot(List<Guid> cardsId, User user)
        {
            using (var command = Connection.CreateCommand())
            {
                long rowAffected = 0;
                foreach(Guid cardId in cardsId)
                {
                    command.CommandText = "SELECT COUNT(ID) FROM cards WHERE id = @Id AND owner = @Owner AND depot = 'stack'";
                    command.Parameters.AddWithValue("Id", cardId);
                    command.Parameters.AddWithValue("Owner", user.Id);
                    rowAffected += (long)command.ExecuteScalar();
                    command.Parameters.Clear();
                    //Console.WriteLine($"rowaffected: {rowAffected}");
                }

                //if (rowAffected != 4) return false;
                //    Console.WriteLine($"wird geupadeted");

                foreach (Guid cardId in cardsId)
                { 
                    command.CommandText = "UPDATE cards SET depot = CASE depot WHEN 'stack' THEN 'deck' ELSE 'stack' END WHERE id = @Id AND owner = @Owner";
                    command.Parameters.AddWithValue("Id", cardId);
                    command.Parameters.AddWithValue("Owner", user.Id);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }

                return true;
            }
        }

        public void ChangeOwner(List<Card> cards, User user)
        {
            using (var command = Connection.CreateCommand())
            {
                foreach (Card card in cards)
                {
                    command.CommandText = "UPDATE cards SET owner = @Owner WHERE id = @Id";
                    command.Parameters.AddWithValue("Id", card.Id);
                    command.Parameters.AddWithValue("Owner", user.Id);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
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
        public void Delete(Guid id)
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
}
