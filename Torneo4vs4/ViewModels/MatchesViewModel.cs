using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Torneo4vs4.Models;
using Torneo4vs4.Services;

namespace Torneo4vs4.ViewModels;

public class MatchesViewModel : INotifyPropertyChanged
{
    private readonly MatchService _matchService;
    private readonly TeamService _teamService;

    private bool _isLoading;
    private string _errorMessage = string.Empty;

    // Lista mostrata direttamente dalla CollectionView.
    public ObservableCollection<MatchListItemViewModel> Matches { get; }
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

    public MatchesViewModel(
        MatchService matchService,
        TeamService teamService)
    {
        _matchService = matchService;
        _teamService = teamService;
    }

    // Carica squadre e partite da Supabase.
    public async Task LoadAsync()
    {
        // Evita due caricamenti contemporanei.
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            // Recupera i dati.
            List<Match> matches =
                await _matchService.GetMatchesAsync();

            List<Team> teams =
                await _teamService.GetTeamsAsync();

            // Creiamo un dizionario:
            //
            // TeamId → Team
            //
            // per recuperare velocemente i nomi.
            Dictionary<int, Team> teamsById =
                teams.ToDictionary(
                    team => team.Id,
                    team => team);

            Matches.Clear();

            foreach (Match match in matches)
            {
                // Cerca la squadra di casa.
                teamsById.TryGetValue(
                    match.HomeTeamId,
                    out Team? homeTeam);

                // Cerca la squadra ospite.
                teamsById.TryGetValue(
                    match.AwayTeamId,
                    out Team? awayTeam);

                // Crea l'oggetto da mostrare a schermo.
                MatchListItemViewModel item =
                    new MatchListItemViewModel
                    {
                        Id = match.Id,

                        HomeTeamName =
                            homeTeam?.Name ?? "Squadra",

                        AwayTeamName =
                            awayTeam?.Name ?? "Squadra",

                        DateTime = match.DateTime,

                        Status = match.Status,

                        HomeGoals = match.HomeGoals,

                        AwayGoals = match.AwayGoals
                    };

                Matches.Add(item);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Impossibile caricare gli incontri.\n{ex.Message}";
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