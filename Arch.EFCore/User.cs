namespace Arch.EFCore;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    

    public List<Note> Notes { get; set; } = new();
}
