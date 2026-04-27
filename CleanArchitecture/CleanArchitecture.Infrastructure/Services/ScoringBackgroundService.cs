using CleanArchitecture.Core.Entities;
using CleanArchitecture.Infrastructure.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Services
{
    public class ScoringBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScoringBackgroundService> _logger;

        public ScoringBackgroundService(IServiceProvider serviceProvider, ILogger<ScoringBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scoring Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Scoring Background Service is recalculating scores.");

                try
                {
                    await RecalculateTotalScoresAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while recalculating total scores.");
                }

                // Wait for 24 hours
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("Scoring Background Service is stopping.");
        }

        private async Task RecalculateTotalScoresAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var users = await userManager.Users.ToListAsync();

            foreach (var user in users)
            {
                // Sum all points earned from event attendances
                var totalAttendancePoints = await context.EventAttendees
                    .Where(ea => ea.UserId == user.Id && ea.Status == "Attended")
                    .SumAsync(ea => ea.PointsEarned);

                // Update TotalScore
                user.TotalScore = totalAttendancePoints;
                await userManager.UpdateAsync(user);
            }

            _logger.LogInformation($"Successfully recalculated total scores for {users.Count} users.");
        }
    }
}
