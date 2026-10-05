using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Torneo4vs4.Models;

[Table("players")]
public class Player : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("team_id")]
    public int TeamId { get; set; }
}
