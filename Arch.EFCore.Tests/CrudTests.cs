using Arch.EFCore;
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
    [InlineData("First note")]
    [InlineData("")]
    [InlineData("123")]
    public async Task Note_Create_ShouldSaveToDatabase(string text)
    {
        var createdAt = DateTimeOffset.Now;

        var note = await NoteCrud.Create(text, createdAt);

        Assert.True(note.Id > 0);
        Assert.Equal(text, note.Text);
        Assert.Equal(createdAt, note.CreatedAt);

        await using var db = new DataContext();
        var noteInDb = await db.Notes.FindAsync(note.Id);
        Assert.NotNull(noteInDb);
        Assert.Equal(text, noteInDb.Text);
        Assert.Equal(createdAt, noteInDb.CreatedAt);
    }

    [Theory]
    [InlineData("лекции", "Подготовка к лекции")]
    [InlineData("тренировку", "Сходить на тренировку")]
    [InlineData("кофе", "Купить кофе")]
    public async Task Note_Read_Search_ShouldReturnFilteredResults(string search, string expectedText)
    {
        var time = DateTimeOffset.Now;
        await NoteCrud.Create("Подготовка к лекции", time);
        await NoteCrud.Create("Сходить на тренировку", time);
        await NoteCrud.Create("Купить кофе", time);

        var result = await NoteCrud.Read(search);

        var singleNote = Assert.Single(result);
        Assert.Equal(expectedText, singleNote.Text);
    }

    [Theory]
    [InlineData("Проверить домашнее задание")]
    [InlineData("Test note")]
    public async Task Note_Read_ById_ShouldReturnCorrectNote(string text)
    {
        var note = await NoteCrud.Create(text, DateTimeOffset.Now);

        var foundNote = await NoteCrud.Read(note.Id);
        var notFoundNote = await NoteCrud.Read(999999);

        Assert.NotNull(foundNote);
        Assert.Equal(text, foundNote.Text);
        Assert.Null(notFoundNote);
    }

    [Theory]
    [InlineData("Начать писать диплом", "Закончить первую главу диплома")]
    [InlineData("Note 1", "Updated Note 1")]
    public async Task Note_Update_ShouldModifyExistingRecord(string initialText, string newText)
    {
        var note = await NoteCrud.Create(initialText, DateTimeOffset.Now);

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
        var note = await NoteCrud.Create(text, DateTimeOffset.Now);

        await NoteCrud.Delete(note);

        await using var db = new DataContext();
        var deletedNote = await db.Notes.FindAsync(note.Id);
        Assert.Null(deletedNote);
    }
}
