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

    [Fact]
    public async Task GET_films_releaseYear_Returns_Only_Films_Of_That_Year()
    {
        // Arrange
        var film2021a = CreateFilmRequestBuilder.ACreateFilmRequest()
            .WithTitle("Dune")
            .WithYear(2021)
            .WithReleaseDate(new DateTime(2021, 9, 15))
            .Build();

        var film2021b = CreateFilmRequestBuilder.ACreateFilmRequest()
            .WithTitle("The Power of the Dog")
            .WithYear(2021)
            .WithReleaseDate(new DateTime(2021, 11, 17))
            .Build();

        var film2022 = CreateFilmRequestBuilder.ACreateFilmRequest()
            .WithTitle("Everything Everywhere All at Once")
            .WithYear(2022)
            .WithReleaseDate(new DateTime(2022, 3, 25))
            .Build();

        await _client.PostAsJsonAsync("/films", film2021a, JsonOptions);
        await _client.PostAsJsonAsync("/films", film2021b, JsonOptions);
        await _client.PostAsJsonAsync("/films", film2022, JsonOptions);

        // Act
        var response = await _client.GetAsync("/films?releaseYear=2021");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var films = await response.Content.ReadFromJsonAsync<List<Film>>(JsonOptions);
        Assert.NotNull(films);
        Assert.Equal(2, films.Count);
        Assert.All(films, f => Assert.Equal(2021, f.ReleaseDate!.Value.Year));
        Assert.Contains(films, f => f.Title == "Dune");
        Assert.Contains(films, f => f.Title == "The Power of the Dog");
    }

    [Fact]
    public async Task DELETE_films_id_Removes_Film_From_Collection()
    {
        // Arrange
        var request = CreateFilmRequestBuilder.ACreateFilmRequest()
            .WithTitle("Film à supprimer")
            .WithYear(2020)
            .Build();

        var postResponse = await _client.PostAsJsonAsync("/films", request, JsonOptions);
        postResponse.EnsureSuccessStatusCode();
        var created = await postResponse.Content.ReadFromJsonAsync<Film>(JsonOptions);

        // Act
        var deleteResponse = await _client.DeleteAsync($"/films/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        var getResponse = await _client.GetAsync("/films");
        var films = await getResponse.Content.ReadFromJsonAsync<List<Film>>(JsonOptions);
        Assert.NotNull(films);
        Assert.DoesNotContain(films, f => f.Id == created.Id);
    }
}