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
    public class BidRepository : IFactory<User>
    {
        private string connectionString;
        public BidRepository()
        {
            connectionString = "server=localhost;userid=root;password=root;port=8889;database=latestbid;SslMode=None";
        }

         internal IDbConnection Connection
        {
            get {
                return new MySqlConnection(connectionString);
            }
        }

         public void AddAuction(Auction auction_item)
        {
             using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute("INSERT IGNORE INTO auctions (product_name, description, bidend_date, start_bid, created_at, updated_at, user_id) VALUES (@product_name, @description, @bidend_date, @start_bid,  NOW(), NOW(), @user_id)", auction_item);
            }
        }
        public Auction Auction_Last_ID()
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<Auction>("SELECT * FROM auctions ORDER BY id DESC LIMIT 1").FirstOrDefault();
            }
        }
        public void Add_Bider(int num1, int num2, double price)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute($"INSERT INTO bids (auction_id, user_id, bid_amount) VALUES ('{num1}', '{num2}', '{price}')");
            }
        }
        public  IEnumerable<Auction> All_Auctions()
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<Auction>("SELECT users.first_name, auctions.top_bid, auctions.user_id, users.last_name, auctions.id, description, product_name, start_bid, bidend_date from auctions,users WHERE auctions.user_id = users.id ORDER BY bidend_date ASC;");
            }
        }
        public void DeleteGroup(string num)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute($"DELETE FROM auctions WHERE id = '{num}'");
            }
        }
         public Auction Auction_Info(string id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                var test = dbConnection.Query<Auction>($"SELECT auctions.product_name, auctions.bidend_date, auctions.top_bid_id, auctions.top_bid_creator, auctions.id, auctions.top_bid, auctions.description, auctions.bidend_date, auctions.start_bid, users.first_name, auctions.user_id, users.last_name FROM auctions LEFT JOIN users ON users.id = auctions.user_id where auctions.id ='{id}';").FirstOrDefault();
                return test;
            }
        }
         public int  Switch(int num1, int num2)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                var temp = dbConnection.Query<Bids>($"SELECT COUNT(distinct bids.user_id) as count FROM bids WHERE auction_id = '{num1}' and user_id = '{num2}'").SingleOrDefault();
                return temp.count;
            }
        }
        public void Join_Bid(Bids newbid)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute($"INSERT INTO bids (auction_id, user_id, bid_amount) VALUES ('{newbid.auction_id}', '{newbid.user_id}', '{newbid.bid_amount}')");
                dbConnection.Execute($"UPDATE auctions SET top_bid = '{newbid.bid_amount}' , top_bid_creator = '{newbid.top_bid_creator}', top_bid_id = '{newbid.user_id}' WHERE id ='{newbid.auction_id}';");
            }
        }
        public void Leave_Bid(int auction_id, int user_id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute($"DELETE FROM bids WHERE bids.user_id = '{user_id}' AND bids.auction_id = '{auction_id}'");
            }
        }
         public void Money_Transfer(string id, int user_id, int top_bid_id, double top_bid, double wallet)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute($"UPDATE users SET users.wallet = wallet + '{top_bid}' where id = '{user_id}';");
                dbConnection.Execute($"UPDATE users SET users.wallet = wallet - '{top_bid}' where id = '{top_bid_id}';");
            }
        }
    }
}
