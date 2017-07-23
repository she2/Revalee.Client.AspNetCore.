using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Revalee.NetCore;
using Revalee.NetCore.Configuration;
using Revalee.NetCore.Mvc;
using Revalee.NetCore.Recurring;
using Revalee.NetCore.StringKeys;

namespace Revalee.SampleSite.NetCore.Controllers
{
    public class HomeController : Controller
    {
        IRevaleeRegistrar _revalee;
        IRecurringTaskModule _recurringTask;
        public HomeController(IRevaleeRegistrar revalee, IRecurringTaskModule recurringTask)
        {
            _revalee = revalee;
            _recurringTask = recurringTask;
        }
        public async Task<IActionResult> Index()
        {
            var url = new Uri(Url.Action("RevaleeCalled", "Home", null, Request.Scheme));

            var callback = await _revalee.RequestCallbackAsync(url, DateTime.Now.AddMinutes(1));
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "I am about page";
            return View();
        }

        [CallbackAction]
        [HttpPost]
        public IActionResult RevaleeCalled()
        {
            ViewData["Message"] = "";
            return Ok();
        }

        [HttpPost]
        public IActionResult RecurringCalled()
        {
            if (_recurringTask.IsProcessingRecurringCallback)
            {
                var url = new Uri(Url.Action("RecurringRevaleeCalled", "Home", null, Request.Scheme));
                var manifest = _recurringTask.GetManifest();
                manifest.AddWithinHourlyTaskAsync(1, url);

                ViewData["Message"] = "";
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost]
        public IActionResult RecurringRevaleeCalled()
        {
            if (_recurringTask.IsProcessingRecurringCallback)
            {
                ViewData["Message"] = "";
                return Ok();
            }
            return BadRequest();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
