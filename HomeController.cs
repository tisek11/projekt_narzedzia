using Strona_2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Strona_2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var kontakty = new List<Kontakt>

            {
           new Kontakt{ Imie="Krystian " ,Nazwisko="Bąkowski " ,Miejscowosc="Bydgoszcz ",Ulica="Akademicka" },
            
        
            };

            ViewBag.Kontakty = kontakty;


            return View();
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}