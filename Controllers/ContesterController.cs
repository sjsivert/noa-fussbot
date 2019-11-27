using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vscodecore.Models;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Web;
using Newtonsoft.Json.Linq;

namespace vscodecore.Controllers
{
    public class ContesterController : Controller
    {
        private static readonly HttpClient client = new HttpClient();

        [HttpGet] // Gets the frontpage
        public async Task<IActionResult> Index()
        {
            using (var context = new EFCoreWebFussballContext())
            {
                var allContesters = await context.Contesters.Where(z => z.IsActive == true).AsNoTracking().ToListAsync();
                return View(allContesters.OrderByDescending(x => calculateWinLossRatio(x.Score, x.GamesPlayed)).ThenByDescending(y => y.GamesPlayed));
            }
        }

        [HttpPost]
        public async Task<IActionResult> PayloadFromSlack([FromForm] string payload)
        {
            var decodedPayload = HttpUtility.UrlDecode(payload);
            Payload payloadObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Payload>(decodedPayload);
            string message = "";
            if (payloadObject.Actions.FirstOrDefault().Value == "click_me_go")
            {
                message = $"➕ {payloadObject.User.Username} er med!";
            }
            else
            {
                message = $"➖ {payloadObject.User.Username} kan ikke nå!";
            }
            await LogToSlack(message);
            return Ok();
        }

        [HttpGet] // Get the create-page view
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost] // Create a new contester
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
            }
            return RedirectToAction("Index");
        }

        [HttpGet] // Submit a win // Fordi ActionLink er en GET.. 
        public async Task<IActionResult> AddWin(int contesterId)
        {
            var leaderPre = await checkLeaderPre();
            var contester = new Contester();
            using (var context = new EFCoreWebFussballContext())
            {
                contester = await context.Contesters.FindAsync(contesterId);
                contester.Score += 1;
                contester.GamesPlayed += 1;
                contester.LastUpdated = DateTime.Now.ToString("H:mm dd/MM");
                await context.SaveChangesAsync();
                string message = $"{contester.ToString()} submitted a win.";
                LogToSlack(message);
            }
            var leaderPost = await checkLeaderPost();
            if (leaderPre != leaderPost.ContesterId)
            {
                string winnerMessage = $":crown: :crown: :crown: {leaderPost.ToString()} is now the new leader!!! :crown: :crown: :crown:";
                LogToSlack(winnerMessage);
            }
            return RedirectToAction("Index");
        }

        [HttpGet] // Submit a loss
        public async Task<IActionResult> AddLoss(int contesterId)
        {
            var leaderPre = await checkLeaderPre();
            var contester = new Contester();
            using (var context = new EFCoreWebFussballContext())
            {
                contester = await context.Contesters.FindAsync(contesterId);
                contester.GamesPlayed += 1;
                contester.LastUpdated = DateTime.Now.ToString("H:mm dd/MM");
                await context.SaveChangesAsync();
                string message = $"{contester.ToString()} submitted a loss.";
                LogToSlack(message);
            }
            var leaderPost = await checkLeaderPost();
            if (leaderPre != leaderPost.ContesterId)
            {
                string winnerMessage = $":crown: :crown: :crown: {leaderPost.ToString()} is now the new leader!!! :crown: :crown: :crown:";
                LogToSlack(winnerMessage);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ProposeGame()
        {
            var jsonstring = "{ 	\"blocks\": [ { \"type\": \"section\", \"text\": { \"type\": \"mrkdwn\", \"text\": \":soccer::exclamation:Fussball time:exclamation::soccer:\" } }, { \"type\": \"section\", \"fields\": [ { \"type\": \"mrkdwn\", \"text\": \"Er du med?!\" } ] }, { \"type\": \"actions\", \"elements\": [ { \"type\": \"button\", \"text\": { 	\"type\": \"plain_text\", 	\"emoji\": true, 	\"text\": \"Let's go!\" }, \"style\": \"primary\", \"value\": \"click_me_go\" }, { \"type\": \"button\", \"text\": { 	\"type\": \"plain_text\", 	\"emoji\": true, 	\"text\": \"Kan ikke...\" }, \"style\": \"danger\", \"value\": \"click_me_no\" } ] } 	] }";
            // client.PostAsync(Environment.GetEnvironmentVariable("slackwebhookurl"), new StringContent(JsonConvert.SerializeObject(dynamicObject)));
            var callback = client.PostAsync(Environment.GetEnvironmentVariable("slackwebhookurl"), new StringContent(jsonstring));
            return RedirectToAction("Index");
        }


        ///////////////////// HELPER FUNCTIONS //////////////////////////////////////
        // Posts a new winner on Slack (mw-no-makingfuss)
        public void PostToSlackWin(string message)
        {
            var dynamicObject = new
            {
                text = message
            };
            client.PostAsync(Environment.GetEnvironmentVariable("slackwebhookurl"), new StringContent(JsonConvert.SerializeObject(dynamicObject)));
        }

        // Posts a submit to Slack
        public async Task<string> LogToSlack(string message)
        {
            var dynamicObject = new
            {
                text = message
            };
            await client.PostAsync(Environment.GetEnvironmentVariable("slackwebhookurl"), new StringContent(JsonConvert.SerializeObject(dynamicObject)));
            return "ok";
        }

        // Calculate Win-loss ratio
        public double calculateWinLossRatio(int score, int gamesPlayed)
        {
            if (gamesPlayed == 0)
            {
                return 0;
            }
            else
            {
                return Convert.ToDouble(score) / Convert.ToDouble(gamesPlayed);
            }
        }

        // Check who the the leader is pre-submitting
        public async Task<int> checkLeaderPre()
        {
            List<Contester> leaderPre = new List<Contester>();
            using (var context = new EFCoreWebFussballContext())
            {
                var allContesters = await context.Contesters.Where(z => z.IsActive == true).AsNoTracking().ToListAsync();
                leaderPre = allContesters.OrderByDescending(x => calculateWinLossRatio(x.Score, x.GamesPlayed)).ThenByDescending(y => y.GamesPlayed).Take(1).ToList<Contester>();
            }
            return leaderPre[0].ContesterId;
        }

        // Check who the the leader is post-submitting
        public async Task<Contester> checkLeaderPost()
        {
            List<Contester> leaderPost = new List<Contester>();
            using (var context = new EFCoreWebFussballContext())
            {
                var allContesters2 = await context.Contesters.Where(z => z.IsActive == true).AsNoTracking().ToListAsync();
                leaderPost = allContesters2.OrderByDescending(x => calculateWinLossRatio(x.Score, x.GamesPlayed)).ThenByDescending(y => y.GamesPlayed).Take(1).ToList();
            }
            return leaderPost[0];
        }
    }
}