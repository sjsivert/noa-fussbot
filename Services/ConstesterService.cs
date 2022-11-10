using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MakingFuss.Data;
using Microsoft.EntityFrameworkCore;

namespace MakingFuss.Services
{
    public class ContesterService
    {
        // TODO: Make service available EFCoreWebFussballContext to eliminate duplicate code
        private readonly EFCoreWebFussballContext _Context;

        public ContesterService(EFCoreWebFussballContext context)
        {
            _Context = context;
        }


        public async Task<List<Contester>> GetAllContestersOrderedByRatio()
        {
            var contesters = await _Context.Contesters
                .AsNoTracking()
                .ToListAsync();

            // Since Ratio is a computed property, we cannot sort in the query itself
            return contesters
                .OrderByDescending(x => x.Ratio)
                .ThenByDescending(x => x.GamesPlayed)
                .ToList();
        }

        public async Task<Contester> GetLeader()
        {
            return (await GetAllContestersOrderedByRatio()).First();
        }

        public async Task<Contester> GetById(int id)
        {
            return await _Context.Contesters.FindAsync(id);
        }

        public async Task AddNew(Contester contester)
        {
            await _Context.Contesters.AddAsync(contester);
            await _Context.SaveChangesAsync();
        }

        public async Task RegisterWin(Contester contester)
        {
            contester.Score += 1;
            contester.GamesPlayed += 1;
            contester.LastUpdated = DateTime.Now.ToString("H:mm dd/MM");
            _Context.Update(contester);
            await _Context.SaveChangesAsync();
        }

        public async Task RegisterLoss(Contester contester)
        {
            contester.GamesPlayed += 1;
            contester.LastUpdated = DateTime.Now.ToString("H:mm dd/MM");
            _Context.Update(contester);
            await _Context.SaveChangesAsync();
        }

        public async Task<Contester> getUserBySlackId(string slackId)
        {
            return await _Context.Contesters.SingleAsync(x => x.SlackUserId == slackId);
        }

        public async Task<bool> IsEnrolledBySlackId(string slackId)
        {
            return await _Context.Contesters.AnyAsync(x => x.SlackUserId == slackId);
        }
    }
}