using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using OpenAPI.GameEngine.Games.Configuration;
using OpenAPI.GameEngine.Models.Teams;
using OpenAPI.Player;

namespace OpenAPI.GameEngine.Games.Teams
{
    public class TeamManager
    {
        private TeamsConfiguration Config { get; }
        private ConcurrentDictionary<string, Team> Teams { get; set; }

        public TeamManager(TeamsConfiguration configuration)
        {
            Config = configuration;
            Teams = new ConcurrentDictionary<string, Team>();

            for (int i = 0; i < configuration.Max; i++)
               CreateTeam();
        }

        private int _teamCounter = 0;
        private Team CreateTeam()
        {
            var id = Interlocked.Increment(ref _teamCounter);
            Team team = new Team($"Team {id}", Config.MaxPlayers);

            if (Teams.TryAdd(team.Name, team))
            {
                return team;
            }

            throw new DuplicateTeamException();
        }

        private bool TryAssign(OpenPlayer player, TeamFillMode fillMode)
        {
            switch (fillMode)
            {
                case TeamFillMode.Fill:
                {
                    var team = Teams.Values.FirstOrDefault(x => x.TryJoin(player));
                    if (team == null)
                        return false;

                    return true;
                }
                case TeamFillMode.Spread:
                {
                    var teamWithLeast =
                        Teams.Values.FirstOrDefault(x => x.PlayerCount == Teams.Values.Min(xx => xx.PlayerCount));
                    if (teamWithLeast == null)
                        return false;

                    if (!teamWithLeast.TryJoin(player))
                        return false;
                    
                    return true;
                }
                case TeamFillMode.FillMinSpread:
                {
                    var team = Teams.Values.FirstOrDefault(x => x.PlayerCount < Config.MinPlayers);
                    if (team == null)
                        return false;

                    if (!team.TryJoin(player))
                        return false;
                    
                    return true;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool TryAssignTeam(OpenPlayer player)
        {
            switch (Config.FillMode)
            {
                case TeamFillMode.Fill:
                {
                    return TryAssign(player, TeamFillMode.Fill);
                }
                case TeamFillMode.Spread:
                {
                    return TryAssign(player, TeamFillMode.Spread);
                }
                case TeamFillMode.FillMinSpread:
                {
                    if (!TryAssign(player, TeamFillMode.FillMinSpread))
                    {
                        return TryAssign(player, TeamFillMode.Spread);
                    }

                    return true;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool CanStart()
        {
            return Teams.All(x => x.Value.PlayerCount >= Config.MinPlayers) && Teams.Count >= Config.Min;
        }
    }
}