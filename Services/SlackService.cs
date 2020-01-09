using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using vscodecore.Models;
using vscodecore.Utils.Helpers;
using System.Linq;

namespace vscodecore.Services
{

    public class SlackService
    {
        private readonly string? _slackWebhookUrl;
        private readonly string LETS_PLAY_BUTTON_VALUE = "click_me_go";
        private readonly string CANT_PLAY_BUTTON_VALUE = "click_me_no";


        public SlackService()
        {
            _slackWebhookUrl = Environment.GetEnvironmentVariable("SLACK_WEBHOOK_URL");

            if (String.IsNullOrEmpty(_slackWebhookUrl))
            {
                throw new Exception("SLACK_WEBHOOK_URL environment variable is not set!");
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
                                text = $"{proposingUser?.FirstName ?? " "}Er du med? { (Helpers.IsDevelopment() ? "" : "@here") }"
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

        public Task PostTop5Scoreboard(IEnumerable<Contester> top5Contesters)
        {
            // ensure ordered
            top5Contesters = top5Contesters.OrderByDescending(x => x.Ratio);

            string payload = BuildScoreboardPayload(top5Contesters);

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
            var medals  = new[] { ":medal:", ":second_place_medal:", ":third_place_medal:", ":champagne:", ":baby_bottle:" };
            var sta = new Stack<string>(medals.Reverse());

            var contestorBlocks = contesters.Select(c =>
               new
               {
                   type = "section",
                   fields = new List<dynamic>() {
                        new {
                            type = "mrkdwn",
                            text = $"*{c.ToString()}* {(sta.Count != 0 ? sta.Pop() : "")}"

                        },
                         new {
                            type = "mrkdwn",
                            text = $"*Ratio*\n{c.Ratio.ToString()}"
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

