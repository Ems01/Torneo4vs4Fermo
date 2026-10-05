using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Torneo4vs4.Enums;

namespace Torneo4vs4.Models;

[Table("matches")]
public class Match : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("home_team_id")]
    public int HomeTeamId { get; set; }

    [Column("away_team_id")]
    public int AwayTeamId { get; set; }

    [Column("home_goals")]
    public int? HomeGoals { get; set; }

    [Column("away_goals")]
    public int? AwayGoals { get; set; }

    [Column("match_date")]
    public DateTime DateTime { get; set; }

    [Column("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MatchStatus Status { get; set; }

    [Column("referee1")]
    public string? Referee1 { get; set; }

    [Column("referee2")]
    public string? Referee2 { get; set; }
}