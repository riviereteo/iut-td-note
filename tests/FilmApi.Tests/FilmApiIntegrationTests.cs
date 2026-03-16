using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FilmApi.Models;
using Xunit;

namespace FilmApi.Tests;

/// <summary>
/// Tests d'intégration : HTTP → API → Service → Repository → MongoDB.
/// </summary>
public sealed class FilmApiIntegrationTests : IClassFixture<MongoFixture>, IAsyncLifetime, IDisposable
{
    private readonly MongoFixture _mongo;
    private readonly FilmApiAppFactory _factory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public FilmApiIntegrationTests(MongoFixture mongo)
    {
        _mongo = mongo;
        _factory = new FilmApiAppFactory(mongo);
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _mongo.InitializeAsync();
        await _mongo.ClearFilmsAsync();
    }

    public void Dispose() => _factory.Dispose();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task POST_films_Returns_201_And_Film()
    {
        // Arrange
        var director = new Director { Id = "d1", LastName = "Dupont", FirstName = "Jean", Nationality = "FR" };
        var request = new CreateFilmRequest(
            Title: "Mon Film",
            Summary: "Résumé.",
            Year: 2024,
            DurationMinutes: 90,
            ReleaseDate: null,
            Director: director,
            Genres: new List<Genre> { new() { Id = "g1", Name = "Drame" } },
            Actors: new List<Actor>(),
            ProductionCountry: new Country { Code = "FR", Name = "France" }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/films", request, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var film = await response.Content.ReadFromJsonAsync<Film>(JsonOptions);
        Assert.NotNull(film);
        Assert.False(string.IsNullOrEmpty(film.Id));
        Assert.Equal("Mon Film", film.Title);
        Assert.Equal(2024, film.Year);
    }

    [Fact]
    public async Task GET_films_id_Returns_200_After_Post()
    {
        // Arrange
        var director = new Director { Id = "d2", LastName = "Martin", FirstName = "Marie", Nationality = "FR" };
        var request = new CreateFilmRequest(
            "Film pour GET",
            "Résumé GET",
            2023,
            100,
            null,
            director,
            new List<Genre> { new() { Id = "g2", Name = "Comédie" } },
            new List<Actor>(),
            null
        );
        var postResponse = await _client.PostAsJsonAsync("/films", request, JsonOptions);
        postResponse.EnsureSuccessStatusCode();
        var created = await postResponse.Content.ReadFromJsonAsync<Film>(JsonOptions);
        Assert.NotNull(created);

        // Act
        var response = await _client.GetAsync($"/films/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var film = await response.Content.ReadFromJsonAsync<Film>(JsonOptions);
        Assert.NotNull(film);
        Assert.Equal(created.Id, film.Id);
        Assert.Equal("Film pour GET", film.Title);
        Assert.Equal("Martin", film.Director.LastName);
    }
}
