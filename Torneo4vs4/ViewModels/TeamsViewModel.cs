using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Torneo4vs4.Models;
using Torneo4vs4.Services;

namespace Torneo4vs4.ViewModels;

public class TeamsViewModel : INotifyPropertyChanged
{
    private readonly TeamService _teamService;
    private readonly PlayerService _playerService;

    private bool _isLoading;
    private string _errorMessage = string.Empty;

    public ObservableCollection<TeamGroupViewModel> Teams { get; }
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

    public TeamsViewModel(
        TeamService teamService,
        PlayerService playerService)
    {
        _teamService = teamService;
        _playerService = playerService;
    }

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

            List<Player> players =
                await _playerService.GetPlayersAsync();

            Teams.Clear();

            foreach (Team team in teams)
            {
                List<Player> teamPlayers =
                    players
                        .Where(player =>
                            player.TeamId == team.Id)
                        .OrderBy(player => player.LastName)
                        .ThenBy(player => player.FirstName)
                        .ToList();

                Teams.Add(
                    new TeamGroupViewModel(
                        team.Id,
                        team.Name,
                        teamPlayers));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Impossibile caricare le squadre.\n{ex.Message}";
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