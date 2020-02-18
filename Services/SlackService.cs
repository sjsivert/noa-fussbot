using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using MakingFuss.Models;
using MakingFuss.Utils.Helpers;
using System.Linq;

namespace MakingFuss.Services
{

    public class SlackService
    {
        private readonly string? _slackWebhookUrl;
        private readonly string? _slackOauthToken;
        private readonly string LETS_PLAY_BUTTON_VALUE = "click_me_go";
        private readonly string CANT_PLAY_BUTTON_VALUE = "click_me_no";


        public SlackService()
        {
            _slackWebhookUrl = Environment.GetEnvironmentVariable("SLACK_WEBHOOK_URL");
            _slackOauthToken = Environment.GetEnvironmentVariable("SLACK_OAUTH_BOT_TOKEN");

            if (String.IsNullOrEmpty(_slackWebhookUrl))
            {
                throw new Exception("SLACK_WEBHOOK_URL environment variable is not set!");
            }
            
            if (String.IsNullOrEmpty(_slackOauthToken))
            {
                throw new Exception("SLACK_OAUTH_BOT_TOKEN environment variable is not set!");
            }
        }



        public Task LogToSlack(string message)
        {

            var messageAsJson = JsonConvert.SerializeObject(
                new
                {
                    text = message
                }
            );

            return PostToSlack(messageAsJson);
        }

        public Task ProposeNewgame(Contester? proposingUser)
        {
            dynamic blocks = new
            {
                blocks = new List<dynamic>() {
                    new {
                        type = "section",
                        text = new {
                            type = "mrkdwn",
                            text = ":soccer::exclamation:Fussball time:exclamation::soccer:"
                        }
                    },
                    new {
                        type = "section",
                        fields = new List<dynamic>() {
                            new {
                                type = "mrkdwn",
                                text = $"{proposingUser?.Name ?? " "} wants to play. Are you in? { (Helpers.IsDevelopment() ? "" : "@here") }"
                            }
                        }
                    },
                    new {
                        type = "actions",
                        elements = new List<dynamic> {
                            new {
                                type = "button",
                                style = "primary",
                                value = LETS_PLAY_BUTTON_VALUE,
                                text = new {
                                    type = "plain_text",
                                    text = "Let's go!",
                                    emoji = true
                           }
                           },
                            new {
                                type = "button",
                                style = "danger",
                                value = CANT_PLAY_BUTTON_VALUE,
                                text = new {
                                    type = "plain_text",
                                    text = "Kan ikke",
                                    emoji = true
,                               }
                            }

                        }
            }
                }
            };

            var blockJson = JsonConvert.SerializeObject(blocks);

            return this.PostToSlack(blockJson);
        }

        public async Task LogNewUser(Contester contester)
        {
            await LogToSlack($":crossed_swords: A new contester has signed up! :crossed_swords: Welcome {contester.Name}");
        }

        public async Task<SlackUserProfile> GetUserProfile(string slackUserId)
        {
            var client = new HttpClient();

            var result = await client.PostAsync($"https://slack.com/api/users.info?token={_slackOauthToken}&user={slackUserId}", null);
            
            var slackResponse = JsonConvert.DeserializeObject<SlackUserProfileResponse>(await result.Content.ReadAsStringAsync());
            
            if (!slackResponse.Ok) {
                throw new Exception($"Failed to get user info for user id '{slackUserId}', reason: '{slackResponse.Error}'");
            }

            return slackResponse.User.Profile;
        }

        public Task PostTop10Scoreboard(IEnumerable<Contester> top10Contesters)
        {
            // ensure ordered
            top10Contesters = top10Contesters.OrderByDescending(x => x.Ratio);

            string payload = BuildScoreboardPayload(top10Contesters);

            return PostToSlack(payload);

        }

        private async Task PostToSlack(string payload)
        {
            var client = new HttpClient();


            var result = await client.PostAsync(_slackWebhookUrl, new StringContent(payload));

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Could not post data to slack, Status code: {result.StatusCode}, reason: {result.ReasonPhrase}");
            }


        }
        private string BuildScoreboardPayload(IEnumerable<Contester> contesters) {
            var medals  = new[] { ":medal:", ":second_place_medal:", ":third_place_medal:", ":champagne:", ":wine_glass:", ":beer:", ":glass_of_milk:", ":baby_bottle:", ":basketball:", ":clown_face:" };
            var sta = new Stack<string>(medals.Reverse());

            var contestorBlocks = contesters.Select(c =>
               new
               {
                   type = "section",
                   fields = new List<dynamic>() {
                        new {
                            type = "mrkdwn",
                            text = $"*{c.Name}* {(sta.Count != 0 ? sta.Pop() : "")}"

                        },
                         new {
                            type = "mrkdwn",
                            text = $"*Ratio*\n{@String.Format("{0:0.00}", c.Ratio)}"
                        },
                         new {
                            type = "mrkdwn",
                            text = $"*Wins*\n{c.Score.ToString()}"
                        },
                         new {
                            type = "mrkdwn",
                            text = $"*Games played*\n{c.GamesPlayed.ToString()}"
                        }
                   }
               }
            );

            var payload = new
            {
                blocks = new List<dynamic>() {
                    new {
                        type = "section",
                        text =
                        new {
                            type = "mrkdwn",
                            text = $":star: *Scoreboard* :star:"
                        }
                    }
                }
            };

            // Add the contestor blocks with a divider object in between
            payload.blocks.AddRange(contestorBlocks.SelectMany(c => new dynamic[] { new { type = "divider" }, c }));

            return JsonConvert.SerializeObject(payload);
        }

    }
}

