using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using thewall.Models;
using WallApp.Factory;
using CryptoHelper;

namespace thewall.Controllers
{
    public class WallController : Controller
    {
        private readonly WallRepository wallFactory;
        private readonly MessageRepository messageFactory;
        private readonly CommentRepository commentFactory;
        public WallController()
        {
            wallFactory = new WallRepository();
            messageFactory = new MessageRepository();
            commentFactory = new CommentRepository();
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

        [HttpPost]
        [Route("registration")]
        public IActionResult Create(User newuser)
        {   
            if(ModelState.IsValid)
            {
                 wallFactory.Add(newuser);
                 ViewBag.User_Extracting = wallFactory.FindByID();
                 int current_id = ViewBag.User_Extracting.id;
                 HttpContext.Session.SetInt32("current_id", (int) current_id);
                 HttpContext.Session.SetString("display", "Successfully Registered");
                 return RedirectToAction("Dashboard");
            }
            List<string> temp_errors = new List<string>();
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

        [HttpGet]
        [Route("dashboard")]
        public IActionResult Dashboard()
        {
            if(HttpContext.Session.GetInt32("current_id") == null)
            {
                return View("Index");
            }
            ViewBag.display = HttpContext.Session.GetString("display");
            ViewBag.User_all = wallFactory.CurrentUser((int)HttpContext.Session.GetInt32("current_id"));
            if(TempData["errors"] != null)
            {
               ViewBag.errors = TempData["errors"];
            }
            ViewBag.messages = messageFactory.FindAllMessages();
            ViewBag.comments = commentFactory.FindAllComments();
            return View("Success");
        }

        [HttpPost]
        [RouteAttribute("login")]
        public IActionResult Login(string email, string password)
        {
            List<string> temp_errors = new List<string>();
            if(email == null || password == null)
            {
                temp_errors.Add("Enter Email and Password Fields to Login");
                TempData["errors"] = temp_errors;
                return RedirectToAction("Index");
            }
            //query
            User check_user = wallFactory.FindEmail(email);
            if(check_user == null)
            {
                temp_errors.Add("Email is not registered");
                TempData["errors"] = temp_errors;
                return RedirectToAction("Index");
            }
            bool correct = Crypto.VerifyHashedPassword((string) check_user.password, password);
            if(correct)
            {
                HttpContext.Session.SetString("display", "Successfully Logged in!");
                HttpContext.Session.SetInt32("current_id", check_user.id);
                return RedirectToAction("Dashboard");
            }
            else{
                temp_errors.Add("Password is not matching");
                TempData["errors"] = temp_errors;
                return RedirectToAction("Index");
            }
        }
        
        [HttpPost]
        [Route("message")]
        public IActionResult Message(Message newmessage)
        {
            List<string> temp_errors = new List<string>();
            if(ModelState.IsValid)
            {
                 messageFactory.AddMessage(newmessage);
                 Console.WriteLine("Message is Successfully added");
                 return RedirectToAction("Dashboard");
            }
            else
            {
                temp_errors.Add("Message Strength is weak");
                TempData["errors"] = temp_errors;
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        [Route("comment")]
        public IActionResult Comment(Comment newcomment)
        {
            List<string> temp_errors = new List<string>();
            if(ModelState.IsValid)
            {
                Console.WriteLine("Comment is Successfully added");
                commentFactory.AddComment(newcomment);
                 return RedirectToAction("Dashboard");
            }
            else
            {
                temp_errors.Add("Comment Strength is weak");
                TempData["errors"] = temp_errors;
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        [Route("del_message")]
        public IActionResult Delete_Message(int del_message)
        {
            Console.WriteLine("id to be deleted is" + del_message);
            messageFactory.DelMessage(del_message);
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [RouteAttribute("logout")]
         public IActionResult Logout()
         {
             HttpContext.Session.Clear();
             Console.WriteLine("session is" + HttpContext.Session.GetInt32("current_id"));
             return RedirectToAction("Index");

         }
    }
}