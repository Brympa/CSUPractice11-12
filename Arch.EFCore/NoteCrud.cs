using Microsoft.EntityFrameworkCore;

namespace Arch.EFCore;

public class NoteCrud
{
    public static async Task<Note> Create(string text, DateTimeOffset createdAt, int userId, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var note = new Note
        {
            Text = text,
            CreatedAt = createdAt,
            UserId = userId
        };
        db.Notes.Add(note);
        await db.SaveChangesAsync(ct);
        return note;
    }

    public static async Task<List<Note>> Read(string search, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var result = await db.Notes
            .Where(x => EF.Functions.Like(x.Text, $"%{search}%"))
            .ToListAsync(ct);
        return result;
    }

    public static async Task<Note?> Read(int id, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        return await db.Notes.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public static async Task<List<Note>> GetByUserId(int userId, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        return await db.Notes
            .Where(x => x.UserId == userId)
            .ToListAsync(ct);
    }

    public static async Task Update(Note note, string text, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        note.Text = text;
        db.Notes.Update(note);
        await db.SaveChangesAsync(ct);
    }

    public static async Task Delete(Note note, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        db.Notes.Remove(note);
        await db.SaveChangesAsync(ct);
    }
}
