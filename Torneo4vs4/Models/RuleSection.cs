using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Torneo4vs4.Models;

[Table("rules")]
public class RuleSection : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("display_order")]
    public int DisplayOrder { get; set; }
}