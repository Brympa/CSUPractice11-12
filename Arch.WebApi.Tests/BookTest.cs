using System.Net;
using System.Net.Http.Json;
using Arch.WebApi;

namespace Arch.WebApi.Tests;

public class BookTest(ApiFixture fixture)
{
    private readonly HttpClient _client = fixture.Api.CreateClient();

    [Fact]
    public async Task CreateBook_Success()
    {
        var ct = TestContext.Current.CancellationToken;
        var requestBody = new BookBody
        {
            Name = "451 градус по фаренгейту",
            Author = "Рэй Брэдбери",
            ReleasedDate = new DateOnly(1953, 10, 19)
        };

        var response = await _client.PostAsJsonAsync("/books", requestBody, ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var model = await response.Content.ReadFromJsonAsync<BookModel>(cancellationToken: ct);
        Assert.NotNull(model);
        Assert.True(model.Id > 0);
        Assert.Equal(requestBody.Name, model.Name);
        Assert.Equal(requestBody.Author, model.Author);
        Assert.Equal(requestBody.ReleasedDate, model.ReleasedDate);
    }

    [Fact]
    public async Task GetBookById_Success()
    {
        var ct = TestContext.Current.CancellationToken;
        var requestBody = new BookBody
        {
            Name = "Вино из одуванчиков",
            Author = "Рэй Брэдбери",
            ReleasedDate = new DateOnly(1957, 1, 1)
        };

        var postResponse = await _client.PostAsJsonAsync("/books", requestBody, ct);
        var created = await postResponse.Content.ReadFromJsonAsync<BookModel>(cancellationToken: ct);
        Assert.NotNull(created);

        var response = await _client.GetAsync($"/books/{created.Id}", ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var retrieved = await response.Content.ReadFromJsonAsync<BookModel>(cancellationToken: ct);
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal(created.Name, retrieved.Name);
        Assert.Equal(created.Author, retrieved.Author);
        Assert.Equal(created.ReleasedDate, retrieved.ReleasedDate);
    }

    [Fact]
    public async Task GetBookById_NotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await _client.GetAsync("/books/99999", ct);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_Success()
    {
        var ct = TestContext.Current.CancellationToken;
        var requestBody = new BookBody
        {
            Name = "Улитка на склоне",
            Author = "Братья Стругацкие",
            ReleasedDate = new DateOnly(1966, 1, 1)
        };

        var postResponse = await _client.PostAsJsonAsync("/books", requestBody, ct);
        var created = await postResponse.Content.ReadFromJsonAsync<BookModel>(cancellationToken: ct);
        Assert.NotNull(created);

        var updateBody = new BookBody
        {
            Name = "Пикник на обочине",
            Author = "Братья Стругацкие",
            ReleasedDate = new DateOnly(1972, 1, 1)
        };

        var response = await _client.PutAsJsonAsync($"/books/{created.Id}", updateBody, ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<BookModel>(cancellationToken: ct);
        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal(updateBody.Name, updated.Name);
        Assert.Equal(updateBody.Author, updated.Author);
        Assert.Equal(updateBody.ReleasedDate, updated.ReleasedDate);
    }

    [Fact]
    public async Task UpdateBook_NotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var updateBody = new BookBody
        {
            Name = "Does Not Exist",
            Author = "No Author",
            ReleasedDate = null
        };
        var response = await _client.PutAsJsonAsync("/books/99999", updateBody, ct);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_Success()
    {
        var ct = TestContext.Current.CancellationToken;
        var requestBody = new BookBody
        {
            Name = "Пикник на обочине",
            Author = "Братья Стругацкие",
            ReleasedDate = new DateOnly(1972, 1, 1)
        };

        var postResponse = await _client.PostAsJsonAsync("/books", requestBody, ct);
        var created = await postResponse.Content.ReadFromJsonAsync<BookModel>(cancellationToken: ct);
        Assert.NotNull(created);

        var response = await _client.DeleteAsync($"/books/{created.Id}", ct);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/books/{created.Id}", ct);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_NotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await _client.DeleteAsync("/books/99999", ct);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBooks_WithSearch()
    {
        var ct = TestContext.Current.CancellationToken;
        
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var book1 = new BookBody { Name = $"Вино из одуванчиков {suffix}", Author = "Рэй Брэдбери", ReleasedDate = new DateOnly(1957, 1, 1) };
        var book2 = new BookBody { Name = "Пикник на обочине", Author = $"Братья Стругацкие {suffix}", ReleasedDate = new DateOnly(1972, 1, 1) };

        var post1 = await _client.PostAsJsonAsync("/books", book1, ct);
        Assert.Equal(HttpStatusCode.OK, post1.StatusCode);
        var post2 = await _client.PostAsJsonAsync("/books", book2, ct);
        Assert.Equal(HttpStatusCode.OK, post2.StatusCode);

        var allResponse = await _client.GetAsync("/books", ct);
        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);
        var allBooks = await allResponse.Content.ReadFromJsonAsync<List<BookModel>>(cancellationToken: ct);
        Assert.NotNull(allBooks);
        Assert.Contains(allBooks, b => b.Name == book1.Name);
        Assert.Contains(allBooks, b => b.Author == book2.Author);

        var searchNameResponse = await _client.GetAsync($"/books?search={suffix}", ct);
        Assert.Equal(HttpStatusCode.OK, searchNameResponse.StatusCode);
        var searchNameBooks = await searchNameResponse.Content.ReadFromJsonAsync<List<BookModel>>(cancellationToken: ct);
        Assert.NotNull(searchNameBooks);
        Assert.Contains(searchNameBooks, b => b.Name == book1.Name);
        Assert.Contains(searchNameBooks, b => b.Author == book2.Author);

        var searchAuthorResponse = await _client.GetAsync($"/books?search=Стругацкие", ct);
        Assert.Equal(HttpStatusCode.OK, searchAuthorResponse.StatusCode);
        var searchAuthorBooks = await searchAuthorResponse.Content.ReadFromJsonAsync<List<BookModel>>(cancellationToken: ct);
        Assert.NotNull(searchAuthorBooks);
        Assert.Contains(searchAuthorBooks, b => b.Author == book2.Author);
        Assert.DoesNotContain(searchAuthorBooks, b => b.Name == book1.Name);
    }
}
