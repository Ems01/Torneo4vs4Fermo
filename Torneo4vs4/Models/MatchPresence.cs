using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Torneo4vs4.Models;

[Table("match_presences")]
public class MatchPresence : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("match_id")]
    public int MatchId { get; set; }

    [Column("player_id")]
    public int PlayerId { get; set; }

    [Column("is_present")]
    public bool IsPresent { get; set; }
}