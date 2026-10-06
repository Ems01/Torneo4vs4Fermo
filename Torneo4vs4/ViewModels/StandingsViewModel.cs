using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Torneo4vs4.Enums;
using Torneo4vs4.Models;
using Torneo4vs4.Services;
using MatchTypeEnum = Torneo4vs4.Enums.MatchType;

namespace Torneo4vs4.ViewModels;

public class StandingsViewModel : INotifyPropertyChanged
{
    private readonly TeamService _teamService;
    private readonly MatchService _matchService;
    private readonly MatchEventService _matchEventService;
    private readonly PlayerService _playerService;

    private bool _isLoading;
    private string _errorMessage = string.Empty;

    // Classifica mostrata dalla pagina.
    public ObservableCollection<StandingRowViewModel> Standings { get; }
        = new();

    public bool IsLoading
    {
        get => _isLoading;

        private set
        {
            if (_isLoading == value)
                return;

            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;

        private set
        {
            if (_errorMessage == value)
                return;

            _errorMessage = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);

    public StandingsViewModel(
    TeamService teamService,
    MatchService matchService,
    MatchEventService matchEventService,
    PlayerService playerService)
    {
        _teamService = teamService;
        _matchService = matchService;
        _matchEventService = matchEventService;
        _playerService = playerService;
    }

    // Ricalcola completamente la classifica.
    public async Task LoadAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            List<Team> teams =
                await _teamService.GetTeamsAsync();

            List<Match> matches =
                await _matchService.GetMatchesAsync();

            List<MatchEvent> events =
                await _matchEventService.GetMatchEventsAsync();

            List<Player> players =
                await _playerService.GetPlayersAsync();

            Dictionary<int, Player> playersById =
                players.ToDictionary(
                    player => player.Id,
                    player => player);

            Dictionary<int, StandingRowViewModel> table =
                teams.ToDictionary(
                    team => team.Id,
                    team => new StandingRowViewModel
                    {
                        TeamId = team.Id,
                        TeamName = team.Name
                    });

            // Solo girone + terminate.
            List<Match> groupMatches =
                matches
                    .Where(match =>
                        match.Status == MatchStatus.Finished &&
                        match.MatchType == MatchTypeEnum.Group)
                    .ToList();

            HashSet<int> groupMatchIds =
                groupMatches
                    .Select(match => match.Id)
                    .ToHashSet();

            // =================================================
            // RISULTATI
            // =================================================

            foreach (Match match in groupMatches)
            {
                if (!match.HomeGoals.HasValue ||
                    !match.AwayGoals.HasValue)
                {
                    continue;
                }

                StandingRowViewModel home =
                    table[match.HomeTeamId];

                StandingRowViewModel away =
                    table[match.AwayTeamId];

                int homeGoals =
                    match.HomeGoals.Value;

                int awayGoals =
                    match.AwayGoals.Value;

                home.Played++;
                away.Played++;

                home.GoalsFor += homeGoals;
                home.GoalsAgainst += awayGoals;

                away.GoalsFor += awayGoals;
                away.GoalsAgainst += homeGoals;

                if (homeGoals > awayGoals)
                {
                    home.Wins++;
                    away.Losses++;
                }
                else if (awayGoals > homeGoals)
                {
                    away.Wins++;
                    home.Losses++;
                }
                else
                {
                    home.Draws++;
                    away.Draws++;
                }
            }

            // =================================================
            // CARTELLINI
            // =================================================

            IEnumerable<MatchEvent> groupEvents =
                events.Where(matchEvent =>
                    groupMatchIds.Contains(matchEvent.MatchId));

            foreach (MatchEvent matchEvent in groupEvents)
            {
                if (!playersById.TryGetValue(
                        matchEvent.PlayerId,
                        out Player? player))
                {
                    continue;
                }

                if (!table.TryGetValue(
                        player.TeamId,
                        out StandingRowViewModel? teamRow))
                {
                    continue;
                }

                if (matchEvent.Type ==
                    MatchEventType.YellowCard)
                {
                    teamRow.YellowCards++;
                }

                else if (matchEvent.Type ==
                         MatchEventType.RedCard)
                {
                    teamRow.RedCards++;
                }
            }

            // =================================================
            // PRIMO ORDINAMENTO:
            // punti -> rossi -> gialli
            // =================================================

            List<IGrouping<
                (int Points, int Reds, int Yellows),
                StandingRowViewModel>> tieGroups =
                    table.Values
                        .GroupBy(row => (
                            Points: row.Points,
                            Reds: row.RedCards,
                            Yellows: row.YellowCards))
                        .OrderByDescending(group =>
                            group.Key.Points)
                        .ThenBy(group =>
                            group.Key.Reds)
                        .ThenBy(group =>
                            group.Key.Yellows)
                        .ToList();

            List<StandingRowViewModel> ordered =
                new();

            // =================================================
            // SCONTRI DIRETTI
            // =================================================

            foreach (var group in tieGroups)
            {
                List<StandingRowViewModel> tiedTeams =
                    group.ToList();

                // Se c'è una sola squadra nel gruppo
                // non servono scontri diretti.
                if (tiedTeams.Count == 1)
                {
                    ordered.Add(tiedTeams[0]);
                    continue;
                }

                HashSet<int> tiedTeamIds =
                    tiedTeams
                        .Select(row => row.TeamId)
                        .ToHashSet();

                // Azzera i punti scontri diretti.
                foreach (StandingRowViewModel row in tiedTeams)
                {
                    row.HeadToHeadPoints = 0;
                }

                // Considera solamente le partite giocate
                // tra squadre appartenenti allo stesso gruppo.
                IEnumerable<Match> headToHeadMatches =
                    groupMatches.Where(match =>
                        tiedTeamIds.Contains(match.HomeTeamId)
                        &&
                        tiedTeamIds.Contains(match.AwayTeamId));

                foreach (Match match in headToHeadMatches)
                {
                    if (!match.HomeGoals.HasValue ||
                        !match.AwayGoals.HasValue)
                    {
                        continue;
                    }

                    StandingRowViewModel home =
                        table[match.HomeTeamId];

                    StandingRowViewModel away =
                        table[match.AwayTeamId];

                    if (match.HomeGoals > match.AwayGoals)
                    {
                        home.HeadToHeadPoints += 3;
                    }
                    else if (match.AwayGoals > match.HomeGoals)
                    {
                        away.HeadToHeadPoints += 3;
                    }
                    else
                    {
                        home.HeadToHeadPoints++;
                        away.HeadToHeadPoints++;
                    }
                }

                // Ordine interno al gruppo.
                List<StandingRowViewModel> orderedTie =
                    tiedTeams
                        .OrderByDescending(row =>
                            row.HeadToHeadPoints)

                        .ThenByDescending(row =>
                            row.GoalDifference)

                        .ThenByDescending(row =>
                            row.GoalsFor)

                        // Ultimo fallback TEMPORANEO:
                        // mantiene la classifica stabile.
                        //
                        // Se anche qui sono pari,
                        // il regolamento richiede sorteggio.
                        .ThenBy(row =>
                            row.TeamName)

                        .ToList();

                ordered.AddRange(orderedTie);
            }

            // =================================================
            // POSIZIONI
            // =================================================

            for (int index = 0;
                 index < ordered.Count;
                 index++)
            {
                ordered[index].Position =
                    index + 1;
            }

            Standings.Clear();

            foreach (StandingRowViewModel row in ordered)
            {
                Standings.Add(row);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Impossibile calcolare la classifica.\n{ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}