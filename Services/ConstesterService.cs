using System;
using System.Threading.Tasks;
using System.Linq;
using MakingFuss.Data;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MakingFuss.Services
{
    public class ContesterService
    {


        public async Task<List<Contester>> GetAllContestersOrderedByRatio()
        {
            using (var db = new EFCoreWebFussballContext())
            {

                var contesters = await db.Contesters
                            .AsNoTracking()
                            .ToListAsync();

                // Since Ratio is a computed property, we cannot sort in the query itself
                return contesters
                        .OrderByDescending(x => x.Ratio)
                        .ThenByDescending(x => x.GamesPlayed)
                        .ToList();
            }

        }

        public async Task<Contester> GetLeader()
        {
            return (await GetAllContestersOrderedByRatio()).First();
        }

        public async Task<Contester> GetById(int id) {
            using (var db = new EFCoreWebFussballContext())
            {
                 return await db.Contesters.FindAsync(id);
            }
        }

        public async Task AddNew(Contester contester)
        {
            using (var db = new EFCoreWebFussballContext())
            {
                await db.Contesters.AddAsync(contester);
                await db.SaveChangesAsync();
            }
        }

        public async Task RegisterWin(Contester contester)
        {
            using (var db = new EFCoreWebFussballContext())
            {
                contester.Score += 1;
                contester.GamesPlayed += 1;
                contester.LastUpdated = DateTime.Now.ToString("H:mm dd/MM");
                db.Update(contester);
                await db.SaveChangesAsync();
            }
        }
        public async Task RegisterLoss(Contester contester)
        {
            using (var db = new EFCoreWebFussballContext())
            {
                contester.GamesPlayed += 1;
                contester.LastUpdated = DateTime.Now.ToString("H:mm dd/MM");
                db.Update(contester);
                await db.SaveChangesAsync();
            }
        }

        public async Task<Contester> getUserBySlackId(string slackId)
        {
            using (var db = new EFCoreWebFussballContext())
            {
                return await db.Contesters.SingleAsync(x => x.SlackUserId == slackId);
            }
        }

        public async Task<bool> IsEnrolledBySlackId(string slackId) {
            using (var db = new EFCoreWebFussballContext())
            {
                return await db.Contesters.AnyAsync(x => x.SlackUserId == slackId);
            }
        }
    }
}