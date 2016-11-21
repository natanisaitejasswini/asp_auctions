using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BidApp.Factory;
using Microsoft.AspNetCore.Mvc;
using latest.Models;
using CryptoHelper;

namespace asplatest.Controllers
{
    public class AuctionController : Controller
    {
        private readonly UserRepository userFactory;
         private readonly BidRepository bidFactory;

        public AuctionController()
        {
            userFactory = new UserRepository();
            bidFactory = new BidRepository();
        }
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            if(TempData["errors"] != null)
            {
               ViewBag.errors = TempData["errors"];
            }
            return View("Login");
        }
// Post Methods:: Login, Registration
        [HttpPost]
        [Route("registration")]
        public IActionResult Create(User newuser)
        {   
            List<string> temp_errors = new List<string>();
            if(ModelState.IsValid)
            {
                 if(userFactory.FindEmail(newuser.email) == null){ // Checking email is registered previously
                    userFactory.Add(newuser);
                    ViewBag.User_Extracting = userFactory.FindByID();
                    int current_other_id = ViewBag.User_Extracting.id;
                    HttpContext.Session.SetInt32("current_id", (int) current_other_id);
                    return RedirectToAction("Dashboard");
                }
                 else{
                    temp_errors.Add("Email is already in use");
                    TempData["errors"] = temp_errors;
                    return RedirectToAction("Index");
                }
            }
            foreach(var error in ModelState.Values)
            {
                if(error.Errors.Count > 0)
                {
                    temp_errors.Add(error.Errors[0].ErrorMessage);
                }  
            }
            TempData["errors"] = temp_errors;
            return RedirectToAction("Index");
        }
        [HttpPost]
        [Route("login")]
        public IActionResult Login(string email, string password)
        {
            List<string> temp_errors = new List<string>();
            if(email == null || password == null)
            {
                temp_errors.Add("Enter Email and Password Fields to Login");
                TempData["errors"] = temp_errors;
                return RedirectToAction("Index");
            }
//Login User Id Extracting query
          User check_user = userFactory.FindEmail(email);
            if(check_user == null)
            {
                temp_errors.Add("Email is not registered");
                TempData["errors"] = temp_errors;
                return RedirectToAction("Index");
            }
            bool correct = Crypto.VerifyHashedPassword((string) check_user.password, password);
            if(correct)
            {
                HttpContext.Session.SetInt32("current_id", check_user.id);
                return RedirectToAction("Dashboard");
            }
            else{
                temp_errors.Add("Password is not matching");
                TempData["errors"] = temp_errors;
                return RedirectToAction("Index");
            }
        }
 //Dashboard start
        [HttpGet]
        [Route("dashboard")]
        public IActionResult Dashboard()
        {
            //on refresh once after logout
            if(HttpContext.Session.GetInt32("current_id") == null)
            {
                return RedirectToAction("Index");
            }
             if(TempData["errors"] != null)
            {
               ViewBag.errors = TempData["errors"];
            }
            //Dashboard begins
            ViewBag.User_one = userFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
            ViewBag.All_Auctions = bidFactory.All_Auctions();
            // if(ViewBag.All_Auctions.bidend_date.ToString("%d") != "0"){
            //                 ViewBag.All_Auctions.bidend_date = ViewBag.All_Auctions.bidend_date.ToString("%d") + " Days";  
            //             }else if (ViewBag.All_Auctions.bidend_date.ToString("%h") != "0"){
            //                 ViewBag.All_Auctions.bidend_date = ViewBag.All_Auctions.bidend_date.ToString("%h") + " Hours";
            //             }else if(ViewBag.All_Auctions.bidend_date.ToString("%m") != "0"){
            //                 ViewBag.All_Auctions.bidend_date = ViewBag.All_Auctions.bidend_date.ToString("%m") + " Minutes";
            //             }else if(ViewBag.All_Auctions.bidend_date.ToString("%s") != "0"){
            //                 ViewBag.All_Auctions.bidend_date = ViewBag.All_Auctions.bid_enddate.ToString("%s") + " Seconds";
            // }
            return View("Dashboard");
        }
//New Auction adding
        [HttpGet]
        [Route("new")]
        public IActionResult New()
        {
            //on refresh once after logout
            if(HttpContext.Session.GetInt32("current_id") == null)
            {
                return RedirectToAction("Index");
            }
            if(TempData["errors"] != null)
            {
               ViewBag.errors = TempData["errors"];
            }
            //Dashboard begins
            ViewBag.User_one = userFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
            return View("New");
        }
//post for Auction
        [HttpPost]
        [Route("addauction")]
         public IActionResult AddAuction(Auction newauction)
        {
            List<string> temp_errors = new List<string>();
            if(ModelState.IsValid)
            {
                if(newauction.bidend_date > DateTime.Now && newauction.start_bid > 0)
                {
                 bidFactory.AddAuction(newauction);
                 ViewBag.User_Extracting = bidFactory.Auction_Last_ID();
                 bidFactory.Add_Bider(ViewBag.User_Extracting.id, (int)HttpContext.Session.GetInt32("current_id"), newauction.start_bid);
                 Console.WriteLine("Auction is Successfully added");
                 return RedirectToAction("Dashboard");
                }
                else{
                    temp_errors.Add("Select Bid End date and  Price of Bid correctly");
                    TempData["errors"] = temp_errors;
                    return RedirectToAction("New");
                }
            }
            foreach(var error in ModelState.Values)
            {
                if(error.Errors.Count > 0)
                {
                    temp_errors.Add(error.Errors[0].ErrorMessage);
                }  
            }
            TempData["errors"] = temp_errors;
            return RedirectToAction("New");
        }
//Delete Auction 
        [HttpGet]
        [Route("deleteauction/{id}")]
        public IActionResult Group_Delete(string id = "")
        {
            bidFactory.DeleteGroup(id);
            return RedirectToAction("Dashboard");
        }
//Show Auction Page
        [HttpGet]
        [Route("auction/{id}")]
        public IActionResult Show(string id = "")
        {
            if(HttpContext.Session.GetInt32("current_id") == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.Auction_Info = bidFactory.Auction_Info(id);
            ViewBag.User_one = userFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
                if(ViewBag.Auction_Info.bidend_date <= DateTime.Now)
                {
                    bidFactory.Money_Transfer(id, ViewBag.Auction_Info.user_id, ViewBag.Auction_Info.top_bid_id, ViewBag.Auction_Info.top_bid, ViewBag.User_one.wallet);
                }
            return View("Show");
        }
//Add Bid
        [HttpPost]
        [Route("addbid")]
        public IActionResult AddBid(Bids newbid)
        {
            int show_id = newbid.auction_id;
            ViewBag.User_one = userFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
            List<string> temp_errors = new List<string>();
            //Checking bids table if entry present remove that entry new one
            ViewBag.Switch = bidFactory.Switch(newbid.auction_id, newbid.user_id);
            if(ModelState.IsValid)
            {
                if(newbid.bid_amount <  ViewBag.User_one.wallet && newbid.bid_amount > newbid.top_bid)  
                {
                    if(ViewBag.Switch == 1)
                    {
                        bidFactory.Leave_Bid(newbid.auction_id, newbid.user_id);
                        bidFactory.Join_Bid(newbid);
                        return RedirectToAction("Dashboard");
                    }
                    else{
                        bidFactory.Join_Bid(newbid);
                        return RedirectToAction("Dashboard");
                    }     
                } 
                else
                {
                    temp_errors.Add("Change your bid amount");
                    TempData["errors"] = temp_errors;
                    return RedirectToAction("Show", new { id = show_id});
                }
                 
            }
            foreach(var error in ModelState.Values)
            {
                if(error.Errors.Count > 0)
                {
                    temp_errors.Add(error.Errors[0].ErrorMessage);
                }  
            }
            TempData["errors"] = temp_errors;
            return RedirectToAction("Dashboard");
        } 
        [HttpGet]
        [Route("getmoney/{id}")]
        public IActionResult Money(string id = "")
        {
            ViewBag.Auction_Info = bidFactory.Auction_Info(id);
            ViewBag.User_one = userFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
            bidFactory.Money_Transfer(id, ViewBag.Auction_Info.user_id, ViewBag.Auction_Info.top_bid_id, ViewBag.Auction_Info.top_bid, ViewBag.User_one.wallet);
            bidFactory.DeleteGroup(id);
            return RedirectToAction("Dashboard");
        }      
// Logout
        [HttpGet]
        [Route("logout")]
         public IActionResult Logout()
         {
             HttpContext.Session.Clear();
             return RedirectToAction("Index");
         }
    }
}