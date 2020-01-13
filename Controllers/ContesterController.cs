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
        private static readonly ContesterService contesterService = new ContesterService();

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allContesters = await contesterService.GetAllContestersOrderedByRatio();
            return View(allContesters);

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
                var leaders = (await contesterService.GetAllContestersOrderedByRatio()).Take(5);
                await slackService.PostTop5Scoreboard(leaders);
            }
            else
            {
                // TODO: get Contester by slack handle
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
        public async Task<IActionResult> Create([Bind("Name")] Contester contester)
        {
  

            await contesterService.AddNew(contester);
            await slackService.LogToSlack($":crossed_swords: A new contester has signed up! :crossed_swords: Welcome {contester.Name}");

            return RedirectToAction("Index");
        }

        [HttpGet] // Submit a win // Fordi ActionLink er en GET.. (?)
        public async Task<IActionResult> AddWin(int contesterId)
        {
            var leaderPre = await contesterService.GetLeader();
            var contester = await contesterService.GetById(contesterId);

            await contesterService.RegisterWin(contester);
            await slackService.LogToSlack($"{contester.Name} submitted a win.");
            
            var leaderPost = await contesterService.GetLeader();

            if (leaderPre.ContesterId != leaderPost.ContesterId)
            {
                await slackService.LogToSlack($":crown: :crown: :crown: {leaderPost.Name} is now the new leader :crown: :crown: :crown:");
            }
            return RedirectToAction("Index");
        }

        [HttpGet] // Submit a loss
        public async Task<IActionResult> AddLoss(int contesterId)
        {
            var leaderPre = await contesterService.GetLeader();
            
            var contester = await contesterService.GetById(contesterId);
            await contesterService.RegisterLoss(contester);
            
            await slackService.LogToSlack($"{contester.Name} submitted a loss.");
            
            var leaderPost = await contesterService.GetLeader();
            
            if (leaderPre.ContesterId != leaderPost.ContesterId)
            {
                await slackService.LogToSlack($":crown: :crown: :crown: {leaderPost.Name} is now the new leader :crown: :crown: :crown:");
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
            await slackService.ProposeNewgame(null); // TODO: get contestor and pass Contestor object
            return RedirectToAction("Index");
        }




        ///////////////////// HELPER FUNCTIONS //////////////////////////////////////

        public string UppercaseName(string userName)
        {
            var nameCapitalized = userName[0].ToString().ToUpper() + userName.Substring(1);
            return nameCapitalized;
        }
    }
}