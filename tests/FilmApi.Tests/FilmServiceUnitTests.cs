using FilmApi.Models;
using FilmApi.Repositories;
using FilmApi.Services;
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
        var director = new Director { Id = "d1", LastName = "Villeneuve", FirstName = "Denis", Nationality = "CA", BirthDate = new DateTime(1967, 10, 3) };
        var genre = new Genre { Id = "g1", Name = "Science-Fiction" };
        var expectedFilm = new Film
        {
            Id = "film1",
            Title = "Dune",
            Summary = "Un jeune duc...",
            Year = 2021,
            DurationMinutes = 155,
            ReleaseDate = new DateTime(2021, 9, 15),
            Director = director,
            Genres = new List<Genre> { genre },
            Actors = new List<Actor>(),
            ProductionCountry = new Country { Code = "US", Name = "États-Unis" }
        };
        substituteRepo
            .AddAsync(Arg.Any<Film>())
            .Returns(expectedFilm);

        var service = new FilmService(substituteRepo);

        var request = new CreateFilmRequest(
            Title: "Dune",
            Summary: "Un jeune duc...",
            Year: 2021,
            DurationMinutes: 155,
            ReleaseDate: new DateTime(2021, 9, 15),
            Director: director,
            Genres: new List<Genre> { genre },
            Actors: new List<Actor>(),
            ProductionCountry: new Country { Code = "US", Name = "États-Unis" }
        );

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
        var director = new Director { Id = "d2", LastName = "Nolan", FirstName = "Christopher", Nationality = "GB" };
        var film = new Film { Id = "f2", Title = "Inception", Year = 2010, Director = director, Genres = new List<Genre>(), Actors = new List<Actor>() };
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
