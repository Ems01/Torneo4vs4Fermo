using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Torneo4vs4.Models;

[Table("teams")]
public class Team : BaseModel
{
    // false perché l'ID viene generato automaticamente da PostgreSQL.
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}