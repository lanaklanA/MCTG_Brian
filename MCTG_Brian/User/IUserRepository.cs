using Npgsql;
using System.Data.Common;

namespace MCTG_Brian.User
{
    public interface IUserRepository
    {
        void Add(User user);
        void Update(User user);
        void Delete(Guid id);
        User GetById(Guid id);
        IEnumerable<User> GetAll();
    }

    public class UserRepository : IUserRepository
    {
        private readonly string connectionString;
        public UserRepository(string conncetion)
        {
            connectionString = conncetion;
        }

        public void Add(User user)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO users (name, password) VALUES (@Name, @Password)";
                    command.Parameters.AddWithValue("Name", user.Name);
                    command.Parameters.AddWithValue("Password", user.Password);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(User user)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE users SET name = @Name, password = @Password WHERE id = @Id";
                    command.Parameters.AddWithValue("Id", user.Id);
                    command.Parameters.AddWithValue("Name", user.Name);
                    command.Parameters.AddWithValue("Password", user.Password);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM users WHERE id = @Id";
                    command.Parameters.AddWithValue("Id", id);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public User GetById(Guid id)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM users WHERE id = @Id";
                    command.Parameters.AddWithValue("Id", id);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new User
                            {
                                Id = reader.GetGuid(0),
                                Name = reader.GetString(1),
                                Password = reader.GetString(2),
                            };
                            return user;
                        }
                        return null;
                    }
                }
            }
        }

        public IEnumerable<User> GetAll()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM users";
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new User
                            {
                                Id = reader.GetGuid(0),
                                Name = reader.GetString(1),
                                Password = reader.GetString(2),
                            };
                            yield return user;
                        }
                    }
                }
            }
        }
    }
}
