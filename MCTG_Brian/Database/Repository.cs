using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Data;
using System.ComponentModel;
using Npgsql.Internal.TypeHandlers.NumericHandlers;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace MCTG_Brian.Database
{

    public abstract class Repository<T>
    {
        private const string _CONNECTION_STRING = "Host = localhost; Username = postgres; Password = qwerqwer; Database = postgres";
        public static NpgsqlConnection? Connection { get; private set; }         // die Datenbankverbindung
        
        public Repository() 
        {
            InitDb();
        }

        public void InitDb()
        {                                                                       // wenn das Programm terminiert, machen
            Connection = new NpgsqlConnection(_CONNECTION_STRING);              // die Db-Verbindung her
            Connection.Open();
        }

        public void CloseDb()
        {                                                                       // wenn das Programm terminiert, machen
            Connection.Close();                                                 // wir die Db-Verbindung brav wieder zu
            Connection.Dispose();
        }

        public abstract void Add(T elm);                                        // Add-Methode 
        public abstract void Update(T elm);                                     // Update-Methode
        public abstract void Delete(Guid id);                                   // Delete-Methode
        public abstract T ByUniq(string Username);                           // Delete-Methode
        public abstract int Count(); //count noch entfernen und ein getAll machen? oder bei packages einfach diese granual machen
    }

    public class UserRepository : Repository<User>
    {
        private object lockThread = new object();

        public override int Count()
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM cards";


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
        public override void Add(User user)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO users (name, password) VALUES (@Name, @Password)";
                command.Parameters.AddWithValue("Name", user.Name);
                command.Parameters.AddWithValue("Password", user.Password);

                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override void Update(User user)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "UPDATE users SET name = @Name, password = @Password WHERE id = @Id";
                command.Parameters.AddWithValue("Id", user.Id);
                command.Parameters.AddWithValue("Name", user.Name);
                command.Parameters.AddWithValue("Password", user.Password);

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
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT id, name, password FROM users WHERE name = @Name";
                command.Parameters.AddWithValue("Name", Username);

                lock(lockThread)
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        { 
                            var json = new JObject
                            {
                                ["Id"] = reader.GetGuid(0),
                                ["Username"] = reader.GetString(1),
                                ["Password"] = reader.GetString(2)
                            };
                            return new User(json);
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
        public override int Count()
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
        public override void Add(Card card)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO cards (id, name, damage) VALUES (@Id, @Name, @Damage)";
                command.Parameters.AddWithValue("Id", card.Id);
                command.Parameters.AddWithValue("Name", card.Name);
                command.Parameters.AddWithValue("Damage", card.Damage);
                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
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
                        return new Card(json);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }

    public class PackRepository : Repository<List<Guid>>
    {
        public override int Count()
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
        public override void Add(List<Guid> packs)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO packages (package) VALUES (@Package)";
                command.Parameters.AddWithValue("Package", JsonConvert.SerializeObject(packs));
                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override void Update(List<Guid> packs)
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
                command.CommandText = "DELETE FROM packages WHERE id = @Id";
                command.Parameters.AddWithValue("Id", id);

                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public override List<Guid> ByUniq(string id)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT packages FROM packages WHERE id = @Id";
                command.Parameters.AddWithValue("Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var packages = JsonConvert.DeserializeObject<List<Guid>>(reader.GetString(0));
                        return packages;
                    }
                    else
                    {
                        return new List<Guid>();
                    }
                }
            }
        }
        public Tuple<Guid, List<Guid>> GetRandPackage()
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT id, package FROM packages LIMIT(1)";
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Guid PackageId = reader.GetGuid(0);
                        var packages = JsonConvert.DeserializeObject<List<Guid>>(reader.GetString(1));

                        Tuple<Guid, List<Guid>> result = new Tuple<Guid, List<Guid>>(PackageId, packages);
                        

                        return result;

                    }
                    else
                    {
                        return new Tuple<Guid, List<Guid>>(Guid.Empty, new List<Guid>());
                    }
                }
            }
        }
    }

    public class StackRepository : Repository<Tuple<User, Guid>>
    {
        public override int Count()
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM cards";


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

    public class DeckRepository : Repository<Dictionary<Guid, Guid>>
    {
        private object lockThread = new object();

        public override int Count()
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM cards";


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
 
 
            using (var connection = new NpgsqlConnection(_CONNECTION_STRING))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT card1.name as card_name, card1.damage as card_damage, card1.id as card_id FROM deck JOIN cards as card1 ON card1.id = deck.card1\r\nUNION \r\nSELECT card2.name as card_name, card2.damage as card_damage, card2.id as card_id FROM deck JOIN cards as card2 ON card2.id = deck.card2 UNION SELECT card3.name as card_name, card3.damage as card_damage, card3.id as card_id FROM deck JOIN cards as card3 ON card3.id = deck.card3 UNION SELECT card4.name as card_name, card4.damage as card_damage, card4.id as card_id FROM deck JOIN cards as card4 ON card4.id = deck.card4;";
                    using (var reader = command.ExecuteReader())
                    {
                        List<Card> cards = new List<Card>();
                        while (reader.Read())
                        {
                            var json = new JObject
                            {
                                ["Name"] = reader.GetString(0),
                                ["Damage"] = reader.GetDouble(1),
                                ["Id"] = reader.GetGuid(2)
                            };
                            cards.Add(new Card(json));
                        }

                        //foreach (var x in cards)
                        //{
                        //    Console.WriteLine(x.Id + " " + x.Name + " " + x.Damage);
                        //}
                        return cards;
                    }
                }
            }
 */