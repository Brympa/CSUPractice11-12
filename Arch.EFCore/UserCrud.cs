using Microsoft.EntityFrameworkCore;

namespace Arch.EFCore;

public class UserCrud
{
    public static async Task<User> Create(string username, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var user = new User
        {
            Username = username
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user;
    }

    public static async Task<List<User>> Read(string search, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var result = await db.Users
            .Where(x => EF.Functions.Like(x.Username, $"%{search}%"))
            .ToListAsync(ct);
        return result;
    }

    public static async Task<User?> Read(int id, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        return await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public static async Task Update(User user, string username, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        user.Username = username;
        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
    }

    public static async Task Delete(User user, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);
    }
}
