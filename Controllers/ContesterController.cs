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
using System.Net;

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
            var capitalizedName = UppercaseName(payloadObject.User.Name);
            string message = "";
            if (payloadObject.Actions.FirstOrDefault().Value == "click_me_go")
            {
                message = $"➕ {capitalizedName} er med!";
            }
            else
            {
                message = $"➖ {capitalizedName} kan ikke nå!";
            }
            await LogToSlack(message);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SlashProposeGame(SlashPayload payload)
        {
            if (payload.text == "score")
            {
                await postLeaderboard();
            }
            else
            {
                var user = UppercaseName(payload.user_name);
                await ProposeGame(user);
            }
            return Ok();
        }
        // [HttpPost]
        // public async Task<IActionResult> SlashRegisterWin([FromForm] string payload)
        // {
        //     var decodedPayload = HttpUtility.UrlDecode(payload);
        //     Payload payloadObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Payload>(decodedPayload);
        //     // TODO: register win to DB

        //     // find the user how?

        //     // var user = payloadObject.User.Name;
        //     // await ProposeGame(user);
        //     return Ok();
        // }
        //   [HttpPost]
        // public async Task<IActionResult> SlashRegisterLoss([FromForm] string payload)
        // {
        //     // var decodedPayload = HttpUtility.UrlDecode(payload);
        //     // Payload payloadObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Payload>(decodedPayload);
        //     // var user = payloadObject.User.Name;
        //     // await ProposeGame(user);
        //     return Ok();
        // }



        //   [HttpPost]
        // public async Task<IActionResult>  ([FromForm] string payload)
        // {
        //     // var decodedPayload = HttpUtility.UrlDecode(payload);
        //     // Payload payloadObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Payload>(decodedPayload);
        //     // var user = payloadObject.User.Name;
        //     // await ProposeGame(user);
        //     return Ok();
        // }


        public string UppercaseName(string userName)
        {
            var nameCapitalized = userName[0].ToString().ToUpper() + userName.Substring(1);
            return nameCapitalized;
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
                contester.Ratio = calculateWinLossRatio(contester.Score, contester.GamesPlayed);
                contester.IsActive = true;
                contester.LastUpdated = DateTime.Now.ToString("H:mm dd/MM");
                context.Add(contester);
                await context.SaveChangesAsync();
                string welcomeMessage = $":crossed_swords: A new contester has signed up! :crossed_swords: Welcome {contester.ToString()}";
                LogToSlack(welcomeMessage);
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
                contester.Ratio = calculateWinLossRatio(contester.Score, contester.GamesPlayed);
                contester.LastUpdated = DateTime.Now.ToString("H:mm dd/MM");
                await context.SaveChangesAsync();
                string message = $"{contester.ToString()} submitted a win.";
                LogToSlack(message);
            }
            var leaderPost = await checkLeaderPost();
            if (leaderPre != leaderPost.ContesterId)
            {
                string winnerMessage = $":crown: :crown: :crown: {leaderPost.ToString()} is now the new leader :crown: :crown: :crown:";
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
                contester.Ratio = calculateWinLossRatio(contester.Score, contester.GamesPlayed);
                contester.LastUpdated = DateTime.Now.ToString("H:mm dd/MM");
                await context.SaveChangesAsync();
                string message = $"{contester.ToString()} submitted a loss.";
                LogToSlack(message);
            }
            var leaderPost = await checkLeaderPost();
            if (leaderPre != leaderPost.ContesterId)
            {
                string winnerMessage = $":crown: :crown: :crown: {leaderPost.ToString()} is now the new leader :crown: :crown: :crown:";
                LogToSlack(winnerMessage);
            }
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> ProposeGame(string? user = "")
        {
            var proposer = "";
            if (user != "")
            {
                proposer = $"{user} er klar for spill!";
            }
            var jsonstring = $"{{\"blocks\": [ {{ \"type\": \"section\", \"text\": {{ \"type\": \"mrkdwn\", \"text\": \":soccer::exclamation:Fussball time:exclamation::soccer:\" }} }}, {{ \"type\": \"section\", \"fields\": [ {{ \"type\": \"mrkdwn\", \"text\": \"{proposer} Er du med? @here \" }} ] }}, {{ \"type\": \"actions\", \"elements\": [ {{ \"type\": \"button\", \"text\": {{ 	\"type\": \"plain_text\", 	\"emoji\": true, 	\"text\": \"Let's go!\" }}, \"style\": \"primary\", \"value\": \"click_me_go\" }}, {{ \"type\": \"button\", \"text\": {{ 	\"type\": \"plain_text\", 	\"emoji\": true, 	\"text\": \"Kan ikke...\" }}, \"style\": \"danger\", \"value\": \"click_me_no\" }} ] }} 	] }}";
            // client.PostAsync(Environment.GetEnvironmentVariable("slackwebhookurl"), new StringContent(JsonConvert.SerializeObject(dynamicObject)));
            var callback = client.PostAsync(Environment.GetEnvironmentVariable("slackwebhookurl"), new StringContent(jsonstring));
            return RedirectToAction("Index");
        }




        ///////////////////// HELPER FUNCTIONS //////////////////////////////////////
        // Posts a submit to Slack
        public async Task<string> LogToSlack(string message)
        {
            using var client = new HttpClient();
            var result = await client.PostAsync($"https://logthistoslack.azurewebsites.net/api/HttpTriggByLog?code=Zm07T1iKXKtCF0/o1MGKBOa/epaltK4Ko2CfzGd7ZZQGQE2iupWleg==&message={message}", null);
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
                if (gamesPlayed < 10)
                {
                    double starterGames = 10.0;
                    double starterWin = 5.0;
                    double normalizedRatio = (Convert.ToDouble(score) + starterWin) / (Convert.ToDouble(gamesPlayed) + starterGames);
                    return normalizedRatio;
                }
                else
                {
                    return Convert.ToDouble(score) / Convert.ToDouble(gamesPlayed);
                }
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

        public async Task<string> postLeaderboard()
        {
            var scopeTop5 = Enumerable.Empty<Contester>();
            using (var context = new EFCoreWebFussballContext())
            {
                var allContesters = await context.Contesters.Where(z => z.IsActive == true).AsNoTracking().ToListAsync();
                var res = allContesters.OrderByDescending(x => calculateWinLossRatio(x.Score, x.GamesPlayed)).ThenByDescending(y => y.GamesPlayed).Take(5);
                scopeTop5 = res;
            }
            var ratio1 = (Convert.ToDecimal(scopeTop5.ElementAt(0).Score) / Convert.ToDecimal(scopeTop5.ElementAt(0).GamesPlayed));
            var ratio2 = (Convert.ToDecimal(scopeTop5.ElementAt(1).Score) / Convert.ToDecimal(scopeTop5.ElementAt(1).GamesPlayed));
            var ratio3 = (Convert.ToDecimal(scopeTop5.ElementAt(2).Score) / Convert.ToDecimal(scopeTop5.ElementAt(2).GamesPlayed));
            var ratio4 = (Convert.ToDecimal(scopeTop5.ElementAt(3).Score) / Convert.ToDecimal(scopeTop5.ElementAt(3).GamesPlayed));
            var ratio5 = (Convert.ToDecimal(scopeTop5.ElementAt(4).Score) / Convert.ToDecimal(scopeTop5.ElementAt(4).GamesPlayed));
            var jsonstring = $"{{\"blocks\":[{{\"type\":\"section\",\"text\":{{\"type\":\"mrkdwn\",\"text\":\"*Scoreboard*\"}}}},{{\"type\":\"divider\"}},{{\"type\":\"section\",\"fields\":[{{\"type\":\"mrkdwn\",\"text\":\"*{scopeTop5.ElementAt(0).ToString()}*\"}},{{\"type\":\"mrkdwn\",\"text\":\"*Ratio:*\n{scopeTop5.ElementAt(0).Ratio.ToString()}\"}},{{\"type\": \"mrkdwn\",\"text\": \"*Wins: *\n{scopeTop5.ElementAt(0).Score}\"}},{{\"type\":\"mrkdwn\",\"text\":\"*Games played:*\n{scopeTop5.ElementAt(0).GamesPlayed}\"}}]}},{{\"type\":\"divider\"}},{{\"type\":\"section\",\"fields\":[{{\"type\":\"mrkdwn\",\"text\":\"*{scopeTop5.ElementAt(1).ToString()}*\"}},{{\"type\":\"mrkdwn\",\"text\":\"*Ratio:*\n{scopeTop5.ElementAt(1).Ratio.ToString()}\"}},{{\"type\":\"mrkdwn\",\"text\":\"*Wins:*\n{scopeTop5.ElementAt(1).Score}\"}},{{\"type\":\"mrkdwn\",\"text\":\"*Games played:*\n{scopeTop5.ElementAt(1).GamesPlayed}\"}}]}},{{\"type\":\"divider\"}},{{\"type\":\"section\",\"fields\":[{{\"type\":\"mrkdwn\",\"text\":\"*{scopeTop5.ElementAt(2).ToString()}*\"}},{{\"type\":\"mrkdwn\",\"text\":\"*Ratio:*\n{scopeTop5.ElementAt(2).Ratio.ToString()}\"}},{{\"type\":\"mrkdwn\",\"text\":\"*Wins:*\n{scopeTop5.ElementAt(2).Score}\"}},{{\"type\":\"mrkdwn\",\"text\":\"*Games played:*\n{scopeTop5.ElementAt(2).GamesPlayed}\"}}]}},{{\"type\":\"divider\"}},{{\"type\":\"section\",\"fields\":[{{\"type\":\"mrkdwn\",\"text\":\"*{scopeTop5.ElementAt(3).ToString()}*\"}},{{\"type\":\"mrkdwn\",\"text\":\"*Ratio:*\n{scopeTop5.ElementAt(3).Ratio.ToString()}\"}},{{\"type\":\"mrkdwn\",\"text\":\"*Wins:*\n{scopeTop5.ElementAt(3).Score}\"}},{{\"type\":\"mrkdwn\",\"text\":\"*Games played:*\n{scopeTop5.ElementAt(3).GamesPlayed}\"}}]}},{{\"type\":\"divider\"}},{{\"type\":\"section\",\"fields\":[{{\"type\":\"mrkdwn\",\"text\":\"*{scopeTop5.ElementAt(4).ToString()}*\"}},{{\"type\":\"mrkdwn\",\"text\":\"*Ratio:*\n{scopeTop5.ElementAt(4).Ratio.ToString()}\"}},{{\"type\":\"mrkdwn\",\"text\":\"*Wins:*\n{scopeTop5.ElementAt(4).Score}\"}},{{\"type\":\"mrkdwn\",\"text\":\"*Games played:*\n{scopeTop5.ElementAt(4).GamesPlayed}\"}}]}},{{\"type\":\"divider\"}}]}}";
            using var client = new HttpClient();
            var result = await client.PostAsync($"https://logthistoslack.azurewebsites.net/api/ShowScoreboard?code=cI4VLMJqwjryhUBXb2otf6wovsRV1W/UxaGviryixhRrvLqc8/44TA==&scoreboard={jsonstring}", null);
            return "ok";
        }


        [HttpGet] // Used to update W/L ratios 
        public async Task<string> updateRatio()
        {
            using (var context = new EFCoreWebFussballContext())
            {
                var allContesters = await context.Contesters.Where(z => z.IsActive == true).AsNoTracking().ToListAsync();
                foreach (var contester in allContesters)
                {
                    var contesterr = new Contester();
                    contesterr = await context.Contesters.FindAsync(contester.ContesterId);
                    contesterr.Ratio = calculateWinLossRatio(contester.Score, contester.GamesPlayed);
                    await context.SaveChangesAsync();
                }
            }
            return "Win/loss rations updated";
        }
        // Posts a new winner on Slack (mw-no-makingfuss)
        // public void PostToSlackWin(string message)
        // {
        //     var dynamicObject = new
        //     {
        //         text = message
        //     };
        //     client.PostAsync(Environment.GetEnvironmentVariable("slackwebhookurl"), new StringContent(JsonConvert.SerializeObject(dynamicObject)));
        // }
    }
}