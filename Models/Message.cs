using System.ComponentModel.DataAnnotations;

public class Message
{
    public int Id { get; set; }

    [StringLength(128)]
    public string Text { get; set; }

    public DateTime CreatedAt { get; set; }
    public int ClientNumber { get; set; }
}
