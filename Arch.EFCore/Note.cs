namespace Arch.EFCore;

public class Note
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }


    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
