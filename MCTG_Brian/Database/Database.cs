using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCTG_Brian.User;

namespace MCTG_Brian.Database
{
    public class Database
    {
        public readonly string connectionString;

        public Database(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void AddUser(NpgsqlConnection connection, User.User user)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO users (name, password) VALUES (@Name, @Password)";
                command.Parameters.AddWithValue("Name", user.Name);
                command.Parameters.AddWithValue("Password", user.Password);

                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public void UpdateUser(NpgsqlConnection connection, User.User user)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE users SET name = @Name, password = @Password WHERE id = @Id";
                command.Parameters.AddWithValue("Id", user.Id);
                command.Parameters.AddWithValue("Name", user.Name);
                command.Parameters.AddWithValue("Password", user.Password);

                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public void DeleteUser(NpgsqlConnection connection, Guid id)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM users WHERE id = @Id";
                command.Parameters.AddWithValue("Id", id);

                command.Prepare();
                command.ExecuteNonQuery();
            }
        }
        public User.User GetUserById(NpgsqlConnection connection, Guid id)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT id, name, password FROM users WHERE id = @Id";
                command.Parameters.AddWithValue("Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User.User
                        {
                            Id = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            Password = reader.GetString(2)
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

    public class UserRepository
    {
        private readonly Database database;

        public UserRepository(Database database)
        {
            this.database = database;
        }

        public void AddUser(User.User user)
        {
            using (var connection = new NpgsqlConnection(database.connectionString))
            {
                connection.Open();
                database.AddUser(connection, user);

            }
        }
        public void UpdateUser(User.User user)
        {
            using (var connection = new NpgsqlConnection(database.connectionString))
            {
                connection.Open();
                database.UpdateUser(connection, user);

            }
        }
        public void DeleteUser(Guid id)
        {
            using (var connection = new NpgsqlConnection(database.connectionString))
            {
                connection.Open();
                database.DeleteUser(connection, id);

            }
        }
        public User.User GetUserById(Guid id)
        {
            using (var connection = new NpgsqlConnection(database.connectionString))
            {
                connection.Open();
                return database.GetUserById(connection, id);
            }
        }

    }
}


