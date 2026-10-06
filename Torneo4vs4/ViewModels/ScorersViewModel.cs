using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Torneo4vs4.Enums;
using Torneo4vs4.Models;
using Torneo4vs4.Services;

namespace Torneo4vs4.ViewModels;

public class ScorersViewModel : INotifyPropertyChanged
{
    private readonly PlayerService _playerService;
    private readonly TeamService _teamService;
    private readonly MatchService _matchService;
    private readonly MatchPresenceService _matchPresenceService;
    private readonly MatchEventService _matchEventService;

    private bool _isLoading;
    private string _errorMessage = string.Empty;

    public ObservableCollection<ScorerRowViewModel> Players
    {
        get;
    } = new();

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

    public ScorersViewModel(
        PlayerService playerService,
        TeamService teamService,
        MatchService matchService,
        MatchPresenceService matchPresenceService,
        MatchEventService matchEventService)
    {
        _playerService = playerService;
        _teamService = teamService;
        _matchService = matchService;
        _matchPresenceService = matchPresenceService;
        _matchEventService = matchEventService;
    }

    public async Task LoadAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            List<Player> players =
                await _playerService.GetPlayersAsync();

            List<Team> teams =
                await _teamService.GetTeamsAsync();

            List<Match> matches =
                await _matchService.GetMatchesAsync();

            List<MatchPresence> presences =
                await _matchPresenceService
                    .GetMatchPresencesAsync();

            List<MatchEvent> events =
                await _matchEventService
                    .GetMatchEventsAsync();

            // Consideriamo esclusivamente
            // le partite terminate.
            HashSet<int> finishedMatchIds =
                matches
                    .Where(match =>
                        match.Status ==
                        MatchStatus.Finished)
                    .Select(match => match.Id)
                    .ToHashSet();

            Dictionary<int, Team> teamsById =
                teams.ToDictionary(
                    team => team.Id,
                    team => team);

            // ============================================
            // PRESENZE
            // ============================================

            Dictionary<int, int> appearancesByPlayer =
                presences
                    .Where(presence =>
                        presence.IsPresent &&
                        finishedMatchIds.Contains(
                            presence.MatchId))
                    .GroupBy(presence =>
                        presence.PlayerId)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Count());

            // ============================================
            // GOL
            // ============================================

            Dictionary<int, int> goalsByPlayer =
                events
                    .Where(matchEvent =>
                        finishedMatchIds.Contains(
                            matchEvent.MatchId) &&
                        matchEvent.Type ==
                            MatchEventType.Goal)
                    .GroupBy(matchEvent =>
                        matchEvent.PlayerId)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Count());

            // ============================================
            // ASSIST
            // ============================================

            Dictionary<int, int> assistsByPlayer =
                events
                    .Where(matchEvent =>
                        finishedMatchIds.Contains(
                            matchEvent.MatchId) &&
                        matchEvent.Type ==
                            MatchEventType.Goal &&
                        matchEvent.AssistPlayerId.HasValue)
                    .GroupBy(matchEvent =>
                        matchEvent.AssistPlayerId!.Value)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Count());

            // ============================================
            // CREAZIONE RIGHE
            // ============================================

            List<ScorerRowViewModel> rows =
                new();

            foreach (Player player in players)
            {
                string teamName = string.Empty;

                if (teamsById.TryGetValue(
                        player.TeamId,
                        out Team? team))
                {
                    teamName = team.Name;
                }

                appearancesByPlayer.TryGetValue(
                    player.Id,
                    out int appearances);

                goalsByPlayer.TryGetValue(
                    player.Id,
                    out int goals);

                assistsByPlayer.TryGetValue(
                    player.Id,
                    out int assists);

                rows.Add(
                    new ScorerRowViewModel
                    {
                        PlayerId = player.Id,
                        FirstName = player.FirstName,
                        LastName = player.LastName,
                        TeamName = teamName,
                        Appearances = appearances,
                        Goals = goals,
                        Assists = assists
                    });
            }

            // ============================================
            // ORDINAMENTO
            // ============================================

            List<ScorerRowViewModel> ordered =
                rows
                    .OrderByDescending(row =>
                        row.Goals)
                    .ThenByDescending(row =>
                        row.Assists)
                    .ThenByDescending(row =>
                        row.Appearances)
                    .ThenBy(row =>
                        row.LastName)
                    .ThenBy(row =>
                        row.FirstName)
                    .ToList();

            Players.Clear();

            foreach (ScorerRowViewModel row in ordered)
            {
                Players.Add(row);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Impossibile caricare i marcatori.\n{ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler?
        PropertyChanged;

    protected virtual void OnPropertyChanged(
        [CallerMemberName]
        string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}