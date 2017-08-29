using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vidly.Models;

namespace Vidly.Controllers
{
    public class HomeController : Controller
    {  // dodanie metody listującej 
        private NuntiusDBEntities m_oNuntiusDBEntities = null;

        public HomeController()
        {
            m_oNuntiusDBEntities = new NuntiusDBEntities();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult List()
        {
            // nie wiem co dalej napisać , nie działa to 
            var oPersons = from oPerson in m_oNuntiusDBEntities.Warnings
                           select oPerson;
            return View(oPersons.ToList());
        }


    }
}