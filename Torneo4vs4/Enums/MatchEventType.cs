namespace Torneo4vs4.Enums;

// Definisce i possibili tipi di evento che possono verificarsi durante una partita.
public enum MatchEventType
{
    // Rappresenta un gol segnato da un giocatore.
    Goal,

    // Rappresenta un'ammonizione con cartellino giallo.
    YellowCard,

    // Rappresenta un'espulsione con cartellino rosso.
    RedCard
}