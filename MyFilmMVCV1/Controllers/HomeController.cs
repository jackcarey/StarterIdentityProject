﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyFilmMVCV1.Models;

namespace MyFilmMVCV1.Controllers
{
    public class HomeController : Controller
    {

        const string SessionName = "_Name";
        const string SessionAge = "_Age";
        const string SessionCart = "_Cart";
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AllMovies()
        {
            if (String.IsNullOrEmpty(HttpContext.Session.GetString(SessionName)))
            {
                HttpContext.Session.SetString(SessionName, "Jarvik");
                HttpContext.Session.SetInt32(SessionAge, 24);
            }
            List<Movie> model = _context.Movies.ToList();
            return View(model);
        }

        public IActionResult Search(String SearchString)
        {

            ViewBag.Name = HttpContext.Session.GetString(SessionName);
            ViewBag.Age = HttpContext.Session.GetInt32(SessionAge);

            var movies = from m in _context.Movies

                        select m;

            if (!string.IsNullOrEmpty(SearchString))

            {

                movies = movies.Where(s => s.FilmTitle.Contains(SearchString));

            }



            List<Movie> model = movies.ToList();
            ViewData["SearchString"] = SearchString;
            return View(model);
        }

        // note uses id because of routing in startup.cs
        [HttpGet]
        public IActionResult MovieDetails(int id)
        {
            //List<Movie> model = _context.Movies.Find(FilmID);
            Movie model = _context.Movies.Find(id);
            return View(model);
        }

        [HttpPost]
        public IActionResult MovieDetails(IFormCollection form)
        {
            int FilmID = int.Parse(form["FilmID"]);
            string FilmTitle = form["FilmTitle"].ToString();
            decimal FilmPrice = Decimal.Parse(form["FilmPrice"]);
            int OrderQuantity = int.Parse(form["OrderQuantity"]);
            CartItem newOrder = new CartItem();
            newOrder.FilmID = FilmID;
            newOrder.FilmTitle = FilmTitle;
            newOrder.FilmPrice = FilmPrice;
            newOrder.OrderQuantity = OrderQuantity;
            newOrder.OrderDate = DateTime.Now;
            var CartList = new List<CartItem>();
            if (HttpContext.Session.GetString(SessionCart) != null)
            {
                string serialJSON = HttpContext.Session.GetString(SessionCart);
                CartList = JsonSerializer.Deserialize<List<CartItem>>(serialJSON);
                //
                var item = CartList.FirstOrDefault(o => o.FilmID == FilmID);
                if (item != null)
                {
                    item.OrderQuantity += OrderQuantity;
                }
                else
                {
                    CartList.Add(newOrder);
                }

            }
            else
            {
                CartList.Add(newOrder);
            }
            HttpContext.Session.SetString(SessionCart, JsonSerializer.Serialize(CartList));
            return RedirectToAction("MovieDetails");
        }



        [HttpGet]
        public IActionResult ManageCart()
        {
            List<CartItem> cart = new List<CartItem>();
            if (HttpContext.Session.GetString(SessionCart) != null)
            {
                string serialJSON = HttpContext.Session.GetString(SessionCart);
                cart = JsonSerializer.Deserialize<List<CartItem>>(serialJSON);
            }
            if (TempData["msg"] != null)
            {
                ViewBag.msg = TempData["msg"].ToString();
            }
            return View(cart);
        }

        [HttpPost]
        public IActionResult RemoveCartItem(IFormCollection form)
        {
            int FilmID = int.Parse(form["FilmID"]);
            var CartList = new List<CartItem>();
            if (HttpContext.Session.GetString(SessionCart) != null)
            {
                string serialJSON = HttpContext.Session.GetString(SessionCart);
                CartList = JsonSerializer.Deserialize<List<CartItem>>(serialJSON);
                //
                var item = CartList.FirstOrDefault(o => o.FilmID == FilmID);
                if (item != null)
                {
                    CartList.Remove(item);
                }

            }

            HttpContext.Session.SetString(SessionCart, JsonSerializer.Serialize(CartList));
            TempData["msg"] = "Item Removed";
            return RedirectToAction("ManageCart");
            //return RedirectToAction("ManageCart", new { id = FilmID });
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
