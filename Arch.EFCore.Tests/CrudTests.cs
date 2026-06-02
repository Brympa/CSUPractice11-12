using Arch.EFCore;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Arch.EFCore.Tests;

public class CrudTests : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await using var db = new DataContext();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [InlineData("Иван Иванов", 20)]
    [InlineData("John Doe", 30)]
    [InlineData("A", 1)]
    public async Task Student_Create_ShouldSaveToDatabase(string name, int age)
    {
        var student = await Crud.Create(name, age);

        Assert.True(student.Id > 0);
        Assert.Equal(name, student.Name);
        Assert.Equal(age, student.Age);

        await using var db = new DataContext();
        var studentInDb = await db.Students.FindAsync(student.Id);
        Assert.NotNull(studentInDb);
        Assert.Equal(name, studentInDb.Name);
        Assert.Equal(age, studentInDb.Age);
    }

    [Theory]
    [InlineData("Иван", "Мария Иванова")]
    [InlineData("Петров", "Дмитрий Петров")]
    [InlineData("Алексей", "Алексей Смирнов")]
    public async Task Student_Read_Search_ShouldReturnFilteredResults(string search, string expectedName)
    {
        await Crud.Create("Алексей Смирнов", 22);
        await Crud.Create("Мария Иванова", 19);
        await Crud.Create("Дмитрий Петров", 21);

        var result = await Crud.Read(search);

        var singleStudent = Assert.Single(result);
        Assert.Equal(expectedName, singleStudent.Name);
    }

    [Theory]
    [InlineData("Елена Сидорова", 20)]
    [InlineData("Test Student", 25)]
    public async Task Student_Read_ById_ShouldReturnCorrectStudent(string name, int age)
    {
        var student = await Crud.Create(name, age);

        var foundStudent = await Crud.Read(student.Id);
        var notFoundStudent = await Crud.Read(999999);

        Assert.NotNull(foundStudent);
        Assert.Equal(name, foundStudent.Name);
        Assert.Null(notFoundStudent);
    }

    [Theory]
    [InlineData("Ольга Кузнецова", 18, "Ольга Васильева", 19)]
    [InlineData("Ivan", 20, "Ivan Updated", 21)]
    public async Task Student_Update_ShouldModifyExistingRecord(string initialName, int initialAge, string newName, int newAge)
    {
        var student = await Crud.Create(initialName, initialAge);

        await Crud.Update(student, newName, newAge);

        await using var db = new DataContext();
        var updatedStudent = await db.Students.FindAsync(student.Id);
        Assert.NotNull(updatedStudent);
        Assert.Equal(newName, updatedStudent.Name);
        Assert.Equal(newAge, updatedStudent.Age);
    }

    [Theory]
    [InlineData("Сергей Попов", 25)]
    [InlineData("A B", 30)]
    public async Task Student_Delete_ShouldRemoveRecord(string name, int age)
    {
        var student = await Crud.Create(name, age);

        await Crud.Delete(student);

        await using var db = new DataContext();
        var deletedStudent = await db.Students.FindAsync(student.Id);
        Assert.Null(deletedStudent);
    }




    [Theory]
    [InlineData("brympa")]
    [InlineData("mops123")]
    [InlineData("Администратор")]
    public async Task User_Create_ShouldSaveToDatabase(string username)
    {
        var user = await UserCrud.Create(username);

        Assert.True(user.Id > 0);
        Assert.Equal(username, user.Username);

        await using var db = new DataContext();
        var userInDb = await db.Users.FindAsync(user.Id);
        Assert.NotNull(userInDb);
        Assert.Equal(username, userInDb.Username);
    }

    [Theory]
    [InlineData("brympa", "brympa")]
    [InlineData("mops", "mops123")]
    [InlineData("Админ", "Администратор")]
    public async Task User_Read_Search_ShouldReturnFilteredResults(string search, string expectedUsername)
    {
        await UserCrud.Create("brympa");
        await UserCrud.Create("mops123");
        await UserCrud.Create("Администратор");

        var result = await UserCrud.Read(search);

        var singleUser = Assert.Single(result);
        Assert.Equal(expectedUsername, singleUser.Username);
    }

    [Theory]
    [InlineData("Елена")]
    [InlineData("TestUser")]
    public async Task User_Read_ById_ShouldReturnCorrectUser(string username)
    {
        var user = await UserCrud.Create(username);

        var foundUser = await UserCrud.Read(user.Id);
        var notFoundUser = await UserCrud.Read(999999);

        Assert.NotNull(foundUser);
        Assert.Equal(username, foundUser.Username);
        Assert.Null(notFoundUser);
    }

    [Theory]
    [InlineData("Ольга", "Ольга Васильева")]
    [InlineData("user", "user_updated")]
    public async Task User_Update_ShouldModifyExistingRecord(string initialUsername, string newUsername)
    {
        var user = await UserCrud.Create(initialUsername);

        await UserCrud.Update(user, newUsername);

        await using var db = new DataContext();
        var updatedUser = await db.Users.FindAsync(user.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal(newUsername, updatedUser.Username);
    }

    [Theory]
    [InlineData("Сергей")]
    [InlineData("A")]
    public async Task User_Delete_ShouldRemoveRecord(string username)
    {
        var user = await UserCrud.Create(username);

        await UserCrud.Delete(user);

        await using var db = new DataContext();
        var deletedUser = await db.Users.FindAsync(user.Id);
        Assert.Null(deletedUser);
    }




    [Theory]
    [InlineData("First note", "test_user1")]
    [InlineData("", "test_user2")]
    [InlineData("123", "test_user3")]
    public async Task Note_Create_ShouldSaveToDatabase(string text, string username)
    {
        var user = await UserCrud.Create(username);
        var createdAt = DateTimeOffset.Now;

        var note = await NoteCrud.Create(text, createdAt, user.Id);

        Assert.True(note.Id > 0);
        Assert.Equal(text, note.Text);
        Assert.Equal(createdAt, note.CreatedAt);
        Assert.Equal(user.Id, note.UserId);

        await using var db = new DataContext();
        var noteInDb = await db.Notes.FindAsync(note.Id);
        Assert.NotNull(noteInDb);
        Assert.Equal(text, noteInDb.Text);
        Assert.Equal(createdAt, noteInDb.CreatedAt);
        Assert.Equal(user.Id, noteInDb.UserId);
    }

    [Theory]
    [InlineData("лекции", "Подготовка к лекции")]
    [InlineData("тренировку", "Сходить на тренировку")]
    [InlineData("кофе", "Купить кофе")]
    public async Task Note_Read_Search_ShouldReturnFilteredResults(string search, string expectedText)
    {
        var user = await UserCrud.Create("test_user");
        var time = DateTimeOffset.Now;
        await NoteCrud.Create("Подготовка к лекции", time, user.Id);
        await NoteCrud.Create("Сходить на тренировку", time, user.Id);
        await NoteCrud.Create("Купить кофе", time, user.Id);

        var result = await NoteCrud.Read(search);

        var singleNote = Assert.Single(result);
        Assert.Equal(expectedText, singleNote.Text);
    }

    [Theory]
    [InlineData("Проверить домашнее задание")]
    [InlineData("Test note")]
    public async Task Note_Read_ById_ShouldReturnCorrectNote(string text)
    {
        var user = await UserCrud.Create("test_user");
        var note = await NoteCrud.Create(text, DateTimeOffset.Now, user.Id);

        var foundNote = await NoteCrud.Read(note.Id);
        var notFoundNote = await NoteCrud.Read(999999);

        Assert.NotNull(foundNote);
        Assert.Equal(text, foundNote.Text);
        Assert.Null(notFoundNote);
    }

    [Theory]
    [InlineData("userA", "userB")]
    public async Task Note_GetByUserId_ShouldReturnOnlyUserNotes(string usernameA, string usernameB)
    {
        var userA = await UserCrud.Create(usernameA);
        var userB = await UserCrud.Create(usernameB);

        var time = DateTimeOffset.Now;
        await NoteCrud.Create("Заметка A1", time, userA.Id);
        await NoteCrud.Create("Заметка A2", time, userA.Id);
        await NoteCrud.Create("Заметка B1", time, userB.Id);

        var notesOfA = await NoteCrud.GetByUserId(userA.Id);
        var notesOfB = await NoteCrud.GetByUserId(userB.Id);

        Assert.Equal(2, notesOfA.Count);
        Assert.All(notesOfA, n => Assert.Equal(userA.Id, n.UserId));

        Assert.Single(notesOfB);
        Assert.Equal(userB.Id, notesOfB[0].UserId);
    }

    [Theory]
    [InlineData("Начать писать диплом", "Закончить первую главу диплома")]
    [InlineData("Note 1", "Updated Note 1")]
    public async Task Note_Update_ShouldModifyExistingRecord(string initialText, string newText)
    {
        var user = await UserCrud.Create("test_user");
        var note = await NoteCrud.Create(initialText, DateTimeOffset.Now, user.Id);

        await NoteCrud.Update(note, newText);

        await using var db = new DataContext();
        var updatedNote = await db.Notes.FindAsync(note.Id);
        Assert.NotNull(updatedNote);
        Assert.Equal(newText, updatedNote.Text);
    }

    [Theory]
    [InlineData("Временная заметка")]
    [InlineData("Temp note")]
    public async Task Note_Delete_ShouldRemoveRecord(string text)
    {
        var user = await UserCrud.Create("test_user");
        var note = await NoteCrud.Create(text, DateTimeOffset.Now, user.Id);

        await NoteCrud.Delete(note);

        await using var db = new DataContext();
        var deletedNote = await db.Notes.FindAsync(note.Id);
        Assert.Null(deletedNote);
    }

    [Theory]
    [InlineData("user_to_delete")]
    public async Task Note_CascadeDelete_ShouldDeleteNotesOnUserDelete(string username)
    {
        var user = await UserCrud.Create(username);
        var time = DateTimeOffset.Now;
        var note1 = await NoteCrud.Create("Заметка 1", time, user.Id);
        var note2 = await NoteCrud.Create("Заметка 2", time, user.Id);

        await UserCrud.Delete(user);

        await using var db = new DataContext();
        var deletedUser = await db.Users.FindAsync(user.Id);
        var dbNote1 = await db.Notes.FindAsync(note1.Id);
        var dbNote2 = await db.Notes.FindAsync(note2.Id);

        Assert.Null(deletedUser);
        Assert.Null(dbNote1);
        Assert.Null(dbNote2);
    }

    [Theory]
    [InlineData(99999)]
    [InlineData(-1)]
    public async Task Note_Create_WithoutValidUser_ShouldThrowForeignKeyException(int invalidUserId)
    {
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await NoteCrud.Create("Заметка сирота", DateTimeOffset.Now, invalidUserId);
        });
    }
}
