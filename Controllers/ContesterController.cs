using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MakingFuss.Models;
using System.Web;

using MakingFuss.Services;

namespace MakingFuss.Controllers
{
    public class ContesterController : Controller
    {
        private static readonly SlackService slackService = new SlackService();

        [HttpGet]
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
            if (payloadObject.Actions.FirstOrDefault().Value == "click_me_go") // Message according to which button the user pressed in Slack
            {
                message = $"➕ {capitalizedName} er med!";
            }
            else
            {
                message = $"➖ {capitalizedName} kan ikke nå!";
            }
            await slackService.LogToSlack(message);
            return Ok();
        }

        [HttpPost] // Handles Slack-interaction. "/fuss" and "/fuss score"
        public async Task<IActionResult> SlashProposeGame(SlashPayload payload)
        {
            if (payload.text == "score") // TODO: Switch statement instead
            {
                await PostLeaderboard();
            }
            else
            {
                var user = UppercaseName(payload.user_name);
                await ProposeGame(user);
            }
            return Ok();
        }

        [HttpGet] // Get the create-page view (new user/contester)
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
                
                await slackService.LogToSlack(welcomeMessage);
            }
            return RedirectToAction("Index");
        }

        [HttpGet] // Submit a win // Fordi ActionLink er en GET.. (?)
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
                await slackService.LogToSlack(message);
            }
            var leaderPost = await checkLeaderPost();
            if (leaderPre != leaderPost.ContesterId)
            {
                string winnerMessage = $":crown: :crown: :crown: {leaderPost.ToString()} is now the new leader :crown: :crown: :crown:";
                await slackService.LogToSlack(winnerMessage);
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
                await slackService.LogToSlack(message);
            }
            var leaderPost = await checkLeaderPost();
            if (leaderPre != leaderPost.ContesterId)
            {
                string winnerMessage = $":crown: :crown: :crown: {leaderPost.ToString()} is now the new leader :crown: :crown: :crown:";
                await slackService.LogToSlack(winnerMessage);
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
            await slackService.ProposeNewgame(null);
            return RedirectToAction("Index");
        }




        ///////////////////// HELPER FUNCTIONS //////////////////////////////////////

        public string UppercaseName(string userName)
        {
            var nameCapitalized = userName[0].ToString().ToUpper() + userName.Substring(1);
            return nameCapitalized;
        }
        // Posts a submit to Slack (via a Function App)

        // Calculate Win-loss ratio
        public double calculateWinLossRatio(int score, int gamesPlayed)
        {
            if (gamesPlayed == 0) // Avoid "can't divide by 0" error
            {
                return 0;
            }

            if (gamesPlayed < 10) // "Normalization" of score for players who rarely play/are new - starts with a 0.5 in w/l ratio
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

        public async Task<Task> PostLeaderboard()
        {
            var scopeTop5 = Enumerable.Empty<Contester>();
            using (var context = new EFCoreWebFussballContext())
            {
                var allContesters = await context.Contesters.Where(z => z.IsActive == true).AsNoTracking().ToListAsync();
                var res = allContesters.OrderByDescending(x => calculateWinLossRatio(x.Score, x.GamesPlayed)).ThenByDescending(y => y.GamesPlayed).Take(5);
                scopeTop5 = res;
            }

            return slackService.PostTop5Scoreboard(scopeTop5);
        }


        [HttpGet] // Used to update W/L ratios manually through Postman
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
    }
}