using FilmApi.Models;
using FilmApi.Repositories;
using FilmApi.Services;
using FilmApi.Tests.Builders;
using NSubstitute;
using Xunit;

namespace FilmApi.Tests;

/// <summary>
/// Tests unitaires du FilmService avec un mock du repository.
/// </summary>
public class FilmServiceUnitTests
{
    [Fact]
    public async Task CreateAsync_Calls_Repository_AddAsync_And_Returns_Film()
    {
        // Arrange
        var substituteRepo = Substitute.For<IFilmRepository>();

        var expectedFilm = FilmBuilder.AFilm()
            .WithId("film1")
            .WithTitle("Dune")
            .WithSummary("Un jeune duc...")
            .WithYear(2021)
            .WithDurationMinutes(155)
            .WithReleaseDate(new DateTime(2021, 9, 15))
            .WithDirector(DirectorBuilder.ADirector()
                .WithId("d1")
                .WithLastName("Villeneuve")
                .WithFirstName("Denis")
                .WithNationality("CA")
                .WithBirthDate(new DateTime(1967, 10, 3)))
            .WithGenres(GenreBuilder.AGenre()
                .WithId("g1")
                .WithName("Science-Fiction"))
            .WithNoActors()
            .WithProductionCountry(CountryBuilder.ACountry()
                .WithCode("US")
                .WithName("États-Unis"))
            .Build();

        substituteRepo
            .AddAsync(Arg.Any<Film>())
            .Returns(expectedFilm);

        var request = CreateFilmRequestBuilder.ACreateFilmRequest()
            .WithTitle("Dune")
            .WithSummary("Un jeune duc...")
            .WithYear(2021)
            .WithDurationMinutes(155)
            .WithReleaseDate(new DateTime(2021, 9, 15))
            .WithDirector(DirectorBuilder.ADirector()
                .WithId("d1")
                .WithLastName("Villeneuve")
                .WithFirstName("Denis")
                .WithNationality("CA")
                .WithBirthDate(new DateTime(1967, 10, 3)))
            .WithGenres(GenreBuilder.AGenre()
                .WithId("g1")
                .WithName("Science-Fiction"))
            .WithNoActors()
            .WithProductionCountry(CountryBuilder.ACountry()
                .WithCode("US")
                .WithName("États-Unis"))
            .Build();

        var service = new FilmService(substituteRepo);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.Equal("film1", result.Id);
        Assert.Equal("Dune", result.Title);
        Assert.Equal(2021, result.Year);
        await substituteRepo
            .Received(1)
            .AddAsync(Arg.Is<Film>(f => f.Title == "Dune"));
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Film_When_Exists()
    {
        // Arrange
        var substituteRepo = Substitute.For<IFilmRepository>();

        var film = FilmBuilder.AFilm()
            .WithId("f2")
            .WithTitle("Inception")
            .WithYear(2010)
            .WithDirector(DirectorBuilder.ADirector()
                .WithId("d2")
                .WithLastName("Nolan")
                .WithFirstName("Christopher")
                .WithNationality("GB"))
            .WithNoActors()
            .Build();

        substituteRepo.GetByIdAsync("f2").Returns(film);

        var service = new FilmService(substituteRepo);

        // Act
        var result = await service.GetByIdAsync("f2");

        // Assert
        
        Assert.NotNull(result);
        Assert.Equal("Inception", result.Title);
        Assert.Equal("Nolan", result.Director.LastName);
    }

    [Fact]
    public async Task DeleteAsync_Returns_True_When_Repository_Deletes()
    {
        // Arrange
        var substituteRepo = Substitute.For<IFilmRepository>();
        substituteRepo.DeleteByIdAsync("f1").Returns(true);

        var service = new FilmService(substituteRepo);

        // Act
        var result = await service.DeleteAsync("f1");

        // Assert
        Assert.True(result);
        await substituteRepo.Received(1).DeleteByIdAsync("f1");
    }

    [Fact]
    public async Task DeleteAsync_Returns_False_When_Not_Found()
    {
        // Arrange
        var substituteRepo = Substitute.For<IFilmRepository>();
        substituteRepo.DeleteByIdAsync("missing").Returns(false);

        var service = new FilmService(substituteRepo);

        // Act
        var result = await service.DeleteAsync("missing");

        // Assert
        Assert.False(result);
    }
}