using Npgsql;
using System.Data.Common;

namespace MCTG_Brian.User
{
    public interface IUserRepository
    {
        void Add(User user);
        void Update(User user);
        void Delete(int id);
        User GetById(int id);
        //User GetByName(string name);
        //List<User> GetAll();
    }

    public class UserRepository : IUserRepository  
    {
        private readonly NpgsqlConnection connection;
        public UserRepository(string conncetionString)
        {
            connection = new NpgsqlConnection(conncetionString);
        }

        public void Add(User user)
        {
            using (var command = connection.CreateCommand()) 
            { 
    
                command.CommandText = "INSERT INTO users (name, password, email) VALUES (@Name, @Password, @Email)";
                command.Parameters.AddWithValue("Name", user.Name);
                command.Parameters.AddWithValue("Password", user.Password);
                command.Parameters.AddWithValue("Email", user.Email);
            
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void Update(User user)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE users SET name = @Name, password = @Password, email = @Email WHERE id = @Id";
                command.Parameters.AddWithValue("Id", user.Id);
                command.Parameters.AddWithValue("Name", user.Name);
                command.Parameters.AddWithValue("Password", user.Password);
                command.Parameters.AddWithValue("Email", user.Email);
                
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void Delete(int id) 
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM users WHERE id = @Id";
                command.Parameters.AddWithValue("Id", id);
                
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public User GetById(int id)
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
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Password = reader.GetString(2),
                            Email = reader.GetString(3)
                        };
                        return user;
                    }
                    return null;
                }
            }
        }

    }
}
