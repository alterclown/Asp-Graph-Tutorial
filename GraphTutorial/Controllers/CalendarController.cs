using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using GraphTutorial.Helpers;

namespace GraphTutorial.Controllers
{
    public class CalendarController : Controller
    {
        // GET: Calendar
        public async Task<ActionResult> Index()
        {
            var events = await GraphHelper.GetEventsAsync();
            return View(events);
        }
    }
}