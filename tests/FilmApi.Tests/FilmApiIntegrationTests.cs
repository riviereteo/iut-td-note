using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FilmApi.Models;
using FilmApi.Tests.Builders;
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
        var request = CreateFilmRequestBuilder.ACreateFilmRequest()
            .WithTitle("Mon Film")
            .WithSummary("Résumé.")
            .WithYear(2024)
            .WithDurationMinutes(90)
            .WithNoReleaseDate()
            .WithDirector(DirectorBuilder.ADirector()
                .WithId("d1")
                .WithLastName("Dupont")
                .WithFirstName("Jean")
                .WithNationality("FR"))
            .WithGenres(GenreBuilder.AGenre()
                .WithId("g1")
                .WithName("Drame"))
            .WithNoActors()
            .WithProductionCountry(CountryBuilder.ACountry()
                .WithCode("FR")
                .WithName("France"))
            .Build();

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
        var request = CreateFilmRequestBuilder.ACreateFilmRequest()
            .WithTitle("Film pour GET")
            .WithSummary("Résumé GET")
            .WithYear(2023)
            .WithDurationMinutes(100)
            .WithNoReleaseDate()
            .WithDirector(DirectorBuilder.ADirector()
                .WithId("d2")
                .WithLastName("Martin")
                .WithFirstName("Marie")
                .WithNationality("FR"))
            .WithGenres(GenreBuilder.AGenre()
                .WithId("g2")
                .WithName("Comédie"))
            .WithNoActors()
            .WithNoProductionCountry()
            .Build();

        var postResponse = await _client.PostAsJsonAsync("/films", request, JsonOptions);
        postResponse.EnsureSuccessStatusCode();
        var created = await postResponse.Content.ReadFromJsonAsync<Film>(JsonOptions);

        // Act
        var response = await _client.GetAsync($"/films/{created!.Id}");
        var film = await response.Content.ReadFromJsonAsync<Film>(JsonOptions);

        // Assert
        Assert.NotNull(created);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(film);
        Assert.Equal(created.Id, film.Id);
        Assert.Equal("Film pour GET", film.Title);
        Assert.Equal("Martin", film.Director.LastName);
    }
}