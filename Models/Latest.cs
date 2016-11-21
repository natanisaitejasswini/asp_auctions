using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace latest.Models
{
    public abstract class BaseEntity {}

    public class User : BaseEntity
        {
            public int id;
            [Required]
            [MinLength(2)]
            [RegularExpression(@"^[a-zA-Z]+$")]
            public string first_name { get; set; }
            [Required]
            [MinLength(1)]
            [RegularExpression(@"^[a-zA-Z]+$")]
            public string last_name { get; set; }
            [Required]
            [EmailAddress]
            public string email{ get; set; }
            [Required]
            [MinLength(3)]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}")]
            public string password { get; set; }
            [Required]
            [Compare("password")]
            public string confirm_password {get; set;}
            public double wallet {get; set;}
            public DateTime created_at;
            public DateTime updated_at;
        }
        public class Auction :  BaseEntity
        {
            public int id;
            [Required]
            [MinLength(4)]
            public string product_name { get; set; }
            [Required]
            [MinLength(11)]
            public string description {get; set;}
            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            [Required(ErrorMessage = "BidEnd is mandatory")]
            public DateTime bidend_date {get; set;}
            public double start_bid {get; set;}
            public DateTime created_at;
            public DateTime updated_at;
            public double top_bid {get; set;}
            public string first_name {get; set;}
            public string last_name {get; set;}
            public int user_id {get; set;}
            public int top_bid_id {get; set;}
            public string top_bid_creator{get; set;}
        }
        public class Bids : BaseEntity
        {
            public int id;
            public int user_id {get; set;}
            public int auction_id {get; set;}
            [Required]
            public double bid_amount {get; set;}
            public double top_bid {get; set;}
            public string top_bid_creator{get; set;}
    
            public int count {get; set;}
        }
}