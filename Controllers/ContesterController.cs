using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MakingFuss.Data;
using System.Web;

using MakingFuss.Services;

namespace MakingFuss.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ContesterController : Controller
    {
        private static ContesterService ContesterService;
        private static SlackService SlackService;

        public ContesterController(ContesterService contesterService, SlackService slackService)
        {
            ContesterService = contesterService;
            SlackService = slackService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allContesters = await ContesterService.GetAllContestersOrderedByRatio();
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
            await SlackService.LogToSlack(message);
            return Ok();
        }

        [HttpPost] // Handles Slack-interaction. "/fuss" and "/fuss score"
        public async Task<IActionResult> SlashProposeGame(SlashPayload payload)
        {
            if (payload.text == "score") // TODO: Switch statement instead
            {
                var leaders = (await ContesterService.GetAllContestersOrderedByRatio()).Take(10);
                await SlackService.PostTop10Scoreboard(leaders);
            }
            else if (payload.text == "enroll")
            {

                var contester = new Contester();
                var slackUserId = payload.user_id;

                if (await ContesterService.IsEnrolledBySlackId(slackUserId))
                {
                    await SlackService.LogToSlack($"Cannot enroll new contester. <@{slackUserId}> is already enrolled!");
                }
                else
                {
                    SlackUserProfile userProfile;
                    try
                    {
                        userProfile = await SlackService.GetUserProfile(slackUserId);
                    }
                    catch (Exception e)
                    {
                        await SlackService.LogToSlack(e.Message);
                        throw e;
                    }

                    contester.Name = userProfile.RealNameNormalized;
                    contester.SlackUserId = slackUserId;

                    await ContesterService.AddNew(contester);
                    await SlackService.LogNewUser(contester);
                }


            }
            else if (payload.text == "reveal_id")
            {

                await SlackService.LogToSlack($"<@{payload.user_id}> user id is *{payload.user_id}*");

            }
            else if (payload.text == "win")
            {
                await RegisterWin(payload.user_id);
            }
            else if (payload.text == "loss")
            {
                await RegisterLoss(payload.user_id);
            }
            else
            {
                // TODO: get Contester by slack handle
                Contester user;
                try
                {
                    user = await ContesterService.getUserBySlackId(payload.user_id);
                }
                catch (InvalidOperationException e)
                {
                    await SlackService.LogToSlack($"Can't find the user profile for id {payload.user_id}. Have you done `/fuss enroll`?");
                    throw e;
                }
                await SlackService.ProposeNewgame(user);
            }
            return Ok();
        }

        [HttpGet] // Get the create-page view (new user/contester)
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost] // Create a new contester
        public async Task<IActionResult> Create([Bind("Name", "SlackUserId")] Contester contester)
        {


            await ContesterService.AddNew(contester);
            await SlackService.LogNewUser(contester);

            return RedirectToAction("Index");
        }

        [HttpGet] // Submit a win // Fordi ActionLink er en GET.. (?)
        public async Task<IActionResult> AddWin(string SlackUserId)
        {
            await RegisterWin(SlackUserId);

            return RedirectToAction("Index");
        }

        [HttpGet] // Submit a loss
        public async Task<IActionResult> AddLoss(string SlackUserId)
        {
            await RegisterLoss(SlackUserId);
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> ProposeGame()
        {
            await SlackService.ProposeNewgame(null);
            return RedirectToAction("Index");
        }




        ///////////////////// HELPER FUNCTIONS //////////////////////////////////////

        private async Task RegisterWin(string contesterId)
        {
            var leaderPre = await ContesterService.GetLeader();
            var contester = await ContesterService.getUserBySlackId(contesterId);

            await ContesterService.RegisterWin(contester);
            await SlackService.LogToSlack($"{contester.Name} submitted a win.");

            var leaderPost = await ContesterService.GetLeader();

            if (leaderPre.ContesterId != leaderPost.ContesterId)
            {
                await SlackService.LogToSlack($":crown: :crown: :crown: <@{leaderPost.SlackUserId}> is now the new leader :crown: :crown: :crown:");
            }
        }
        private async Task RegisterLoss(string contesterId)
        {
            var leaderPre = await ContesterService.GetLeader();

            var contester = await ContesterService.getUserBySlackId(contesterId);
            await ContesterService.RegisterLoss(contester);

            await SlackService.LogToSlack($"{contester.Name} submitted a loss.");

            var leaderPost = await ContesterService.GetLeader();

            if (leaderPre.ContesterId != leaderPost.ContesterId)
            {
                await SlackService.LogToSlack($":crown: :crown: :crown: <@{leaderPost.SlackUserId}> is now the new leader :crown: :crown: :crown:");
            }
        }



        [NonAction]
        public string UppercaseName(string userName)
        {
            var nameCapitalized = userName[0].ToString().ToUpper() + userName.Substring(1);
            return nameCapitalized;
        }
    }
}