using System.Collections.Generic;
using System;
using System.Linq;
using Dapper;
using System.Data;
using MySql.Data.MySqlClient;
using latest.Models;
using CryptoHelper;

namespace BidApp.Factory
{
    public class UserRepository : IFactory<User>
    {
        private string connectionString;
        public UserRepository()
        {
            connectionString = "server=localhost;userid=root;password=root;port=8889;database=latestbid;SslMode=None";
        }

        internal IDbConnection Connection
        {
            get {
                return new MySqlConnection(connectionString);
            }
        }
        public void Add(User item)
        {
            using (IDbConnection dbConnection = Connection) {
                
                string password_Hash = Crypto.HashPassword(item.password);
                string query = $"INSERT INTO users (first_name,last_name, email, password, created_at, updated_at, wallet) VALUES ('{item.first_name}', '{item.last_name}', '{item.email}', '{password_Hash}', NOW(), NOW(), '{item.wallet}')";
                dbConnection.Open();
                dbConnection.Execute(query);
            }
        }
        public User FindByID()
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<User>("SELECT * FROM users ORDER BY id DESC LIMIT 1").FirstOrDefault();
            }
        }
        public User FindEmail(string email)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<User>("SELECT * FROM users WHERE email = @Email LIMIT 1", new { Email = email }).FirstOrDefault();
            }
        }

        public IEnumerable<User> AllUsers()
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<User>("SELECT * FROM users");
            }
        }

        public User CurrentUser(int num)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<User>($"SELECT users.first_name, users.last_name, users.id, users.wallet, users.email FROM users WHERE id = {num}").FirstOrDefault();
            }
        }
    }
}