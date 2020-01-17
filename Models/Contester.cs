using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;


namespace MakingFuss.Models
{
    public class Contester
    {
        [Key]
        public int ContesterId { get; set; }

        [StringLength(255)]
        [Display(Name = "Name")]
        public string Name { get; set; }
        public int Score { get; set; } = 0;
        public int GamesPlayed { get; set; } = 0;

        public string SlackUserId { get; set; } = "";
        public string LastUpdated { get; set; } = DateTime.Now.ToString("H:mm dd/MM");
        public bool IsActive { get; set; }

        [NotMapped]
        public double Ratio
        {
            get
            {
                if (this.Score == 0) // Avoid "can't divide by 0" error
                {
                    return 0;
                }

                if (this.GamesPlayed < 10) // "Normalization" of score for players who rarely play/are new - starts with a 0.5 in w/l ratio
                {
                    double starterGames = 10.0;
                    double starterWin = 5.0;
                    double normalizedRatio = (Convert.ToDouble(this.Score) + starterWin) / (Convert.ToDouble(this.GamesPlayed) + starterGames);
                    return normalizedRatio;
                }
                else
                {
                    return Convert.ToDouble(this.Score) / Convert.ToDouble(this.GamesPlayed);
                }
            }
        }
    }
}