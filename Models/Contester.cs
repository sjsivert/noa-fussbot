using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace vscodecore.Models
{
    public class Contester
    {
        [Key]
        public int ContesterId { get; set; }
        [StringLength(255)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [StringLength(255)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
        public int Score { get; set; }
        public int GamesPlayed { get; set; }
        public double Ratio { get; set; }
        public string LastUpdated { get; set; }
        // public int TournamentWon { get; set; }
        public bool IsActive { get; set; }
        // public Tournament Tournament { get; set; }

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }
    }
}