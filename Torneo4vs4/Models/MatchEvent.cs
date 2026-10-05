using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Torneo4vs4.Enums;

namespace Torneo4vs4.Models;

[Table("match_events")]
public class MatchEvent : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("match_id")]
    public int MatchId { get; set; }

    [Column("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MatchEventType Type { get; set; }

    [Column("player_id")]
    public int PlayerId { get; set; }

    [Column("assist_player_id")]
    public int? AssistPlayerId { get; set; }

    [Column("event_order")]
    public int Order { get; set; }
}