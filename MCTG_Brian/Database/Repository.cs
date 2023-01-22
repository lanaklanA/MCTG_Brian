using MCTG_Brian.Database.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace MCTG_Brian.Database
{
    public abstract class Repository<T>
    {
        public static NpgsqlConnection? Connection { get; private set; }       
        private const string _CONNECTION_STRING = "Host = localhost; Username = postgres; Password = qwerqwer; Database = postgres";
        
        public Repository() 
        {
            Connection = new NpgsqlConnection(_CONNECTION_STRING);              // die Db-Verbindung her
            Connection.Open();
        }

        public abstract bool Add(T elm);                                         
        public abstract void Update(T elm);                                     
        public abstract T ByUniq(string Username);                           
    }

    /// <summary>
    /// The UserRepository class provides CRUD operations and statistics update for User entities, using a thread-safe implementation.
    /// </summary>
    public class UserRepository : Repository<User>
    {
        private object syncPrimitive = new object(); 
        public void UpdateStats(User user)
        {
            lock(syncPrimitive)
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
        public override bool Add(User user)
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
            return true;
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
        { 
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT users.id, users.name, users.password, users.coins, users.bio, users.image, stats.id, stats.elo, stats.wins, stats.loses, stats.draws FROM users JOIN stats ON stats.userid = users.id WHERE users.name = @Name";
                command.Parameters.AddWithValue("Name", Username);

                lock (syncPrimitive)
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

    /// <summary>
    /// The CardRepository class provides CRUD operations and owner management for Card entities, using a thread-safe implementation.
    /// </summary>
    public class CardRepository : Repository<Card> 
    {
        private object syncPrimitive = new object(); 

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
        public void ChangeOwner(Card card, User user)
        {
            using (var command = Connection.CreateCommand())
            {

                command.CommandText = "UPDATE cards SET owner = @Owner WHERE id = @Id";
                command.Parameters.AddWithValue("Id", card.Id);
                command.Parameters.AddWithValue("Owner", user.Id);
                command.ExecuteNonQuery();
                command.Parameters.Clear();
      
            }
        } //whsl auch weg
        public void pushCards(User user)
        {
            lock(syncPrimitive)
            {

                using (var command = Connection.CreateCommand())
            {
                foreach(Card deckCard in user.Deck)
                {
                    command.CommandText = "UPDATE cards SET owner = @Userid, damage = @Damage, depot = 'deck' WHERE id = @Id";
                    command.Parameters.AddWithValue("Userid", user.Id);
                    command.Parameters.AddWithValue("Damage", deckCard.Damage);
                    command.Parameters.AddWithValue("Id", deckCard.Id);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }

                foreach (Card stackCard in user.Stack)
                {
                    command.CommandText = "UPDATE cards SET owner = @Userid, damage = @Damage, depot = 'stack' WHERE id = @Id";
                    command.Parameters.AddWithValue("Userid", user.Id);
                    command.Parameters.AddWithValue("Damage", stackCard.Damage);
                    command.Parameters.AddWithValue("Id", stackCard.Id);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
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
                            cards.Add(new Card(reader.GetString(1))
                            {
                                Id = reader.GetGuid(0),
                                Damage = reader.GetDouble(2),
                            });
                        }
                    }
                
            }

            return cards ?? null;
        }
        public override bool Add(Card card) { return false; }
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
                        return new Card(reader.GetString(1))
                        {
                            Id = reader.GetGuid(0),
                            Damage = reader.GetDouble(2)
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }

    /// <summary>
    /// A repository for managing and querying card packs stored in a database. Includes methods for adding, updating, and deleting packs, as well as retrieving a random pack and searching for a specific pack by its unique ID.
    /// </summary>
    public class PackRepository : Repository<List<Card>>
    {
        public override bool Add(List<Card> pack)
        {
            using (var command = Connection.CreateCommand())
            {
                long rowAffected = 0;
                foreach (Card cardId in pack)
                {
                    for(int i = 0; i < 5; i++)
                    { 
                        command.CommandText = $"SELECT COUNT(ID) FROM store where cast(card{i+1}->>'Id' as text) = cast(@cardId as text)";
                        command.Parameters.AddWithValue("cardId", cardId.Id);
                        rowAffected += (long)command.ExecuteScalar();
                        command.Parameters.Clear();
                    }
                    command.CommandText = $"SELECT COUNT(ID) FROM cards where cast(id as text) = cast(@cardId as text)";
                    command.Parameters.AddWithValue("cardId", cardId.Id);
                    rowAffected += (long)command.ExecuteScalar();
                    command.Parameters.Clear();
                }

                if(rowAffected > 0)
                {
                    return false;
                }

                command.CommandText = "INSERT INTO store (card1, card2, card3, card4, card5) VALUES (@Card1, @Card2, @Card3, @Card4, @Card5)";

                for (int i = 0; i < 5; i++)
                {
                    var param = new NpgsqlParameter($"@Card{i + 1}", NpgsqlDbType.Json);
                    param.Value = JsonConvert.SerializeObject(pack[i]);
                    command.Parameters.Add(param);
                }

                command.Prepare();
                command.ExecuteNonQuery();

                return true;
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

    /// <summary>
    /// The TradeRepository class is a repository that handles the CRUD operations for the trades. It has methods for adding a new trade, deleting a trade, updating a trade, getting a trade by its unique ID, and getting all trades.
    /// </summary>
    public class TradeRepository : Repository<Trade> 
    {
        public override bool Add(Trade elm)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO trades (id, userid, cardid, tradeinfo) VALUES (@Id, @Userid, @Cardid, @Details)";
                command.Parameters.AddWithValue("Id", elm.Id);
                command.Parameters.AddWithValue("Userid", elm.UserId);
                command.Parameters.AddWithValue("Cardid", elm.CardId);
                var param = new NpgsqlParameter("@Details", NpgsqlDbType.Json);
                param.Value = JsonConvert.SerializeObject(elm.Details);
                command.Parameters.Add(param);
                command.Prepare();
                command.ExecuteNonQuery();
            }
            return true;
        }
        public void Delete(Guid tradeId)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM trades WHERE id = @Id";
                command.Parameters.AddWithValue("Id", tradeId);
                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override void Update(Trade elm)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "UPDATE trades SET userid = @Userid, cardid = @Cardid, tradeinfo = @Details WHERE id = @Id";
                command.Parameters.AddWithValue("Id", elm.Id);
                command.Parameters.AddWithValue("Userid", elm.UserId);
                command.Parameters.AddWithValue("Cardid", elm.CardId);
                var param = new NpgsqlParameter("@Details", NpgsqlDbType.Json);
                param.Value = JsonConvert.SerializeObject(elm.Details);
                command.Parameters.Add(param);
                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override Trade ByUniq(string elm)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT id, userid, cardid, tradeinfo FROM trades WHERE id = @Id";
                command.Parameters.AddWithValue("Id", Guid.Parse(elm));
                command.Prepare();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Trade
                        {
                            Id = reader.GetGuid(0),
                            UserId = reader.GetGuid(1),
                            CardId = reader.GetGuid(2),
                            Details = JObject.Parse(reader.GetString(3))
                        };
                    }
                }
                return null;
            }
        }
        public List<Trade>? GetAll()
        {
            List<Trade> trades = new List<Trade>();
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT id, userid, cardid, tradeinfo FROM trades";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var trade = new Trade
                        {
                            Id = reader.GetGuid(0),
                            UserId = reader.GetGuid(1),
                            CardId = reader.GetGuid(2),
                            Details = JObject.Parse(reader.GetString(3))
                        };
                        trades.Add(trade);
                    }
                }
            }
            return trades.Count > 0 ? trades : null;
        }
    }
}
