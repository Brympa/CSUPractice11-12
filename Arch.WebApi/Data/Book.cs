namespace Arch.WebApi.Data;

public class Book
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Author { get; set; }
    public DateOnly? ReleasedDate { get; set; }
}
