using System.Text.Json;
using System.Text.Json.Serialization;
using Torneo4vs4.Models;
using Torneo4vs4.Enums;
using Torneo4vs4.DTOs;

namespace Torneo4vs4.Services;

public class MatchSessionService
{
    private const string FileName = "current_match.json";

    private const string TemporaryFileName = "current_match.tmp";

    // Contiene le impostazioni utilizzate durante la conversione tra oggetti C# e JSON.
    private readonly JsonSerializerOptions _jsonOptions;

    // Costruttore.
    public MatchSessionService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            // Formatta il JSON su più righe rendendolo leggibile anche manualmente.
            WriteIndented = true,

            // Permette di leggere le proprietà del JSON senza distinguere maiuscole e minuscole.
            PropertyNameCaseInsensitive = true
        };

        // Aggiunge un convertitore che salva gli enum usando il loro nome invece del valore numerico.
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    // Restituisce il percorso completo del file della partita in corso.
    private string GetFilePath()
    {
        return Path.Combine(FileSystem.AppDataDirectory, FileName);
    }

    // Restituisce il percorso completo del file temporaneo.
    private string GetTemporaryFilePath()
    {
        return Path.Combine(FileSystem.AppDataDirectory, TemporaryFileName);
    }

    // Crea una nuova sessione locale per una partita che sta per iniziare.
    public async Task<CurrentMatchSession> CreateSessionAsync(Match match, Team homeTeam, Team awayTeam, IEnumerable<Player> players)
    {
        // Crea il nuovo oggetto che rappresenterà la partita in corso.
        CurrentMatchSession session = new CurrentMatchSession
        {
            MatchId = match.Id,
            HomeTeamId = match.HomeTeamId,
            AwayTeamId = match.AwayTeamId,
            HomeTeamName = homeTeam.Name,
            AwayTeamName = awayTeam.Name,
            HomeGoals = 0,
            AwayGoals = 0,
            Players = players.ToList()
        };

        foreach (Player player in session.Players)
        {
            // Aggiunge una nuova presenza alla lista.
            session.Presences.Add(
                new MatchPresence
                {
                    Id = 0,
                    MatchId = match.Id,
                    PlayerId = player.Id,
                    IsPresent = false
                });
        }

        // Salva immediatamente la nuova sessione nel file current_match.json.
        await SaveSessionAsync(session);
        return session;
    }

    // Aggiorna i nomi degli arbitri della partita in corso.
    public async Task SetRefereesAsync(CurrentMatchSession session, string referee1, string referee2)
    {
        session.Referee1 = referee1;
        session.Referee2 = referee2;
        await SaveSessionAsync(session);
    }

    // Modifica lo stato di presenza di un giocatore nella partita corrente.
    public async Task SetPlayerPresenceAsync(CurrentMatchSession session, int playerId, bool isPresent)
    {
        // Cerca nella sessione il record di presenza appartenente al giocatore indicato.
        MatchPresence? presence =
            session.Presences.FirstOrDefault(
                presence => presence.PlayerId == playerId);

        if (presence is null)
        {
            throw new InvalidOperationException(
                "Il giocatore non appartiene alla partita corrente.");
        }

        presence.IsPresent = isPresent;
        await SaveSessionAsync(session);
    }

    // Aggiunge un nuovo evento alla partita in corso.
    public async Task<MatchEvent> AddEventAsync(CurrentMatchSession session, MatchEventType eventType, int playerId, int? assistPlayerId = null)
    {
        // Cerca il giocatore protagonista dell'evento
        Player? player =
            session.Players.FirstOrDefault(
                player => player.Id == playerId);

        if (player is null)
        {
            throw new InvalidOperationException(
                "Il giocatore non appartiene alla partita corrente.");
        }

        // Cerca il record relativo alla presenza del giocatore.
        MatchPresence? playerPresence =
            session.Presences.FirstOrDefault(
                presence => presence.PlayerId == playerId);

        // Controlla che il giocatore sia stato indicato come presente.
        if (playerPresence is null || !playerPresence.IsPresent)
        {
            throw new InvalidOperationException(
                "Non è possibile assegnare un evento a un giocatore assente.");
        }

        // Se l'evento non è un gol, non deve essere presente alcun assist.
        if (eventType != MatchEventType.Goal && assistPlayerId.HasValue)
        {
            throw new InvalidOperationException(
                "Un assist può essere associato solamente a un gol.");
        }

        // Se è stato indicato un assist, eseguiamo ulteriori controlli.
        if (assistPlayerId.HasValue)
        {
            Player? assistPlayer =
                session.Players.FirstOrDefault(
                    player => player.Id == assistPlayerId.Value);

            if (assistPlayer is null)
            {
                throw new InvalidOperationException(
                    "Il giocatore dell'assist non appartiene alla partita corrente.");
            }

            MatchPresence? assistPresence =
                session.Presences.FirstOrDefault(
                    presence => presence.PlayerId == assistPlayerId.Value);

            if (assistPresence is null || !assistPresence.IsPresent)
            {
                throw new InvalidOperationException(
                    "Non è possibile assegnare un assist a un giocatore assente.");
            }

            // Controlla che marcatore e assist-man appartengano alla stessa squadra.
            if (assistPlayer.TeamId != player.TeamId)
            {
                throw new InvalidOperationException(
                    "Il giocatore dell'assist deve appartenere alla stessa squadra del marcatore.");
            }

            // Controlla che il giocatore non venga indicato contemporaneamente come marcatore e autore dell'assist.
            if (assistPlayer.Id == player.Id)
            {
                throw new InvalidOperationException(
                    "Il marcatore non può essere anche l'autore dell'assist.");
            }
        }

        // Calcola l'ordine del nuovo evento.
        int nextOrder =
            session.Events.Count == 0
                ? 1
                : session.Events.Max(matchEvent => matchEvent.Order) + 1;

        // Crea il nuovo evento.
        MatchEvent newEvent = new MatchEvent
        {
            Id = 0,
            MatchId = session.MatchId,
            Type = eventType,
            PlayerId = playerId,
            AssistPlayerId = assistPlayerId,
            Order = nextOrder
        };

        session.Events.Add(newEvent);

        if (eventType == MatchEventType.Goal)
        {
            if (player.TeamId == session.HomeTeamId)
            {
                session.HomeGoals++;
            }
            else if (player.TeamId == session.AwayTeamId)
            {
                session.AwayGoals++;
            }
            else
            {
                throw new InvalidOperationException(
                    "Il giocatore non appartiene a nessuna delle squadre della partita.");
            }
        }

        await SaveSessionAsync(session);
        return newEvent;
    }

    // Rimuove un evento precedentemente inserito nella partita.
    public async Task RemoveEventAsync(CurrentMatchSession session, int eventOrder)
    {
        // Cerca l'evento utilizzando il suo ordine cronologico.
        MatchEvent? eventToRemove =
            session.Events.FirstOrDefault(
                matchEvent => matchEvent.Order == eventOrder);

        if (eventToRemove is null)
        {
            throw new InvalidOperationException(
                "L'evento indicato non esiste.");
        }

        if (eventToRemove.Type == MatchEventType.Goal)
        {
            Player? scorer =
                session.Players.FirstOrDefault(
                    player => player.Id == eventToRemove.PlayerId);

            if (scorer is null)
            {
                throw new InvalidOperationException(
                    "Il marcatore dell'evento non appartiene alla partita corrente.");
            }

            if (scorer.TeamId == session.HomeTeamId)
            {
                if (session.HomeGoals <= 0)
                {
                    throw new InvalidOperationException(
                        "Il risultato della squadra di casa non è coerente con gli eventi.");
                }

                session.HomeGoals--;
            }
            else if (scorer.TeamId == session.AwayTeamId)
            {
                if (session.AwayGoals <= 0)
                {
                    throw new InvalidOperationException(
                        "Il risultato della squadra ospite non è coerente con gli eventi.");
                }

                session.AwayGoals--;
            }
            else
            {
                throw new InvalidOperationException(
                    "Il marcatore non appartiene a nessuna delle squadre della partita.");
            }
        }

        session.Events.Remove(eventToRemove);

        // Riordina gli eventi rimasti in base al vecchio ordine.
        List<MatchEvent> orderedEvents =
            session.Events
                .OrderBy(matchEvent => matchEvent.Order)
                .ToList();

        // Assegna nuovamente gli ordini.
        for (int index = 0; index < orderedEvents.Count; index++)
        {
            orderedEvents[index].Order = index + 1;
        }

        session.Events = orderedEvents;
        await SaveSessionAsync(session);
    }

    // Controlla se sul dispositivo è presente una partita salvata.
    public bool SessionExists()
    {
        string filePath = GetFilePath();
        return File.Exists(filePath);
    }

    // Salva lo stato corrente della partita nel file JSON.
    public async Task SaveSessionAsync(CurrentMatchSession session)
    {
        // Converte l'oggetto CurrentMatchSession in una stringa JSON.
        string json = JsonSerializer.Serialize(session, _jsonOptions);

        string filePath = GetFilePath();
        string temporaryFilePath = GetTemporaryFilePath();

        await File.WriteAllTextAsync(temporaryFilePath, json);

        // Sposta il file temporaneo sopra il file principale.
        File.Move(temporaryFilePath, filePath, true);
    }

    // Carica dal dispositivo la partita salvata.
    public async Task<CurrentMatchSession?> LoadSessionAsync()
    {
        string filePath = GetFilePath();

        if (!File.Exists(filePath))
        {
            return null;
        }

        string json = await File.ReadAllTextAsync(filePath);

        CurrentMatchSession? session =
            JsonSerializer.Deserialize<CurrentMatchSession>(json, _jsonOptions);

        return session;
    }

    // Elimina definitivamente il salvataggio locale della partita.
    public void DeleteSession()
    {
        string filePath = GetFilePath();

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        string temporaryFilePath = GetTemporaryFilePath();

        if (File.Exists(temporaryFilePath))
        {
            File.Delete(temporaryFilePath);
        }
    }

    // Controlla che tutti i dati della partita siano giusti prima di chiudere la gara.
    public void ValidateSession(CurrentMatchSession session)
    {
        if (string.IsNullOrWhiteSpace(session.Referee1))
        {
            throw new InvalidOperationException(
                "È necessario inserire il primo arbitro.");
        }

        if (string.IsNullOrWhiteSpace(session.Referee2))
        {
            throw new InvalidOperationException(
                "È necessario inserire il secondo arbitro.");
        }

        int calculatedHomeGoals =
            session.Events.Count(matchEvent =>
            {
                if (matchEvent.Type != MatchEventType.Goal)
                {
                    return false;
                }

                Player? player =
                    session.Players.FirstOrDefault(
                        player => player.Id == matchEvent.PlayerId);

                return player is not null &&
                       player.TeamId == session.HomeTeamId;
            });

        int calculatedAwayGoals =
            session.Events.Count(matchEvent =>
            {
                if (matchEvent.Type != MatchEventType.Goal)
                {
                    return false;
                }

                Player? player =
                    session.Players.FirstOrDefault(
                        player => player.Id == matchEvent.PlayerId);

                return player is not null &&
                       player.TeamId == session.AwayTeamId;
            });

        if (calculatedHomeGoals != session.HomeGoals)
        {
            throw new InvalidOperationException(
                "Il numero di gol della squadra di casa non coincide con gli eventi registrati.");
        }

        if (calculatedAwayGoals != session.AwayGoals)
        {
            throw new InvalidOperationException(
                "Il numero di gol della squadra ospite non coincide con gli eventi registrati.");
        }

        foreach (MatchEvent matchEvent in session.Events)
        {
            MatchPresence? presence =
                session.Presences.FirstOrDefault(
                    presence => presence.PlayerId == matchEvent.PlayerId);

            if (presence is null || !presence.IsPresent)
            {
                throw new InvalidOperationException(
                    "È presente un evento associato a un giocatore assente.");
            }

            if (matchEvent.AssistPlayerId.HasValue)
            {
                MatchPresence? assistPresence =
                    session.Presences.FirstOrDefault(
                        presence =>
                            presence.PlayerId ==
                            matchEvent.AssistPlayerId.Value);

                if (assistPresence is null || !assistPresence.IsPresent)
                {
                    throw new InvalidOperationException(
                        "È presente un assist associato a un giocatore assente.");
                }
            }
        }
    }

    public CompletedMatchData CreateCompletedMatchData(CurrentMatchSession session)
    {
        ValidateSession(session);

        CompletedMatchData completedMatch =
            new CompletedMatchData
            {
                MatchId = session.MatchId,
                HomeTeamId = session.HomeTeamId,
                AwayTeamId = session.AwayTeamId,
                HomeGoals = session.HomeGoals,
                AwayGoals = session.AwayGoals,

                Referee1 = session.Referee1!,
                Referee2 = session.Referee2!,

                Presences = session.Presences.ToList(),
                Events = session.Events.ToList()
            };

        return completedMatch;
    }
}