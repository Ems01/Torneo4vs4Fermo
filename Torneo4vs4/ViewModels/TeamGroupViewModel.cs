using System.Collections.ObjectModel;
using Torneo4vs4.Models;

namespace Torneo4vs4.ViewModels;

// Rappresenta una squadra insieme ai suoi giocatori.
public class TeamGroupViewModel : ObservableCollection<Player>
{
    public int TeamId { get; }

    public string TeamName { get; }

    public int PlayerCount => Count;

    public TeamGroupViewModel(
        int teamId,
        string teamName,
        IEnumerable<Player> players)
        : base(players)
    {
        TeamId = teamId;
        TeamName = teamName;
    }
}