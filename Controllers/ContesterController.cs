using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vscodecore.Models;

namespace vscodecore.Controllers
{
    public class ContesterController : Controller
    {
        public async Task<IActionResult> Index()
        {
            ViewData["Message"] = "Hello, This is my Contester view(data)";
            using (var context = new EFCoreWebFussballContext())
            {
                var allContesters = await context.Contesters.Where(z => z.IsActive == true).AsNoTracking().ToListAsync();

                return View(allContesters.OrderByDescending(x => x.Score).ThenByDescending(y => y.GamesPlayed));
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create([Bind("FirstName, LastName")] Contester contester)
        {
            using (var context = new EFCoreWebFussballContext())
            {
                contester.GamesPlayed = 0;
                contester.Score = 0;
                contester.IsActive = true;
                contester.LastUpdated = DateTime.Now.ToString("H:mm dd/MM");
                context.Add(contester);
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
        }

        [HttpGet] // Fordi ActionLink er en GET..
        public async Task<IActionResult> AddWin(int contesterId)
        {
            var contester = new Contester();
            using (var context = new EFCoreWebFussballContext())
            {
                contester = await context.Contesters.FindAsync(contesterId);
                contester.Score += 1;
                contester.GamesPlayed += 1;
                contester.LastUpdated = DateTime.Now.ToString("H:mm dd/MM");
                await context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> AddLoss(int contesterId)
        {
            var contester = new Contester();
            using (var context = new EFCoreWebFussballContext())
            {
                contester = await context.Contesters.FindAsync(contesterId);
                contester.GamesPlayed += 1;
                contester.LastUpdated = DateTime.Now.ToString("H:mm dd/MM");
                await context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}