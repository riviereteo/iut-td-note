using FilmApi.Models;
using FilmApi.Repositories;
<<<<<<< HEAD
=======
using FilmApi.Services;
>>>>>>> etape3
using FilmApi.Tests.Builders;
using NSubstitute;
using VerifyXunit;
using Xunit;

namespace FilmApi.Tests;

/// <summary>
/// État initial du squelette : ce test vérifie un DTO complexe (Film avec Réalisateur, Acteurs, Genres)
/// via une longue série d'Assert.Equal.
/// </summary>
[UsesVerify]
public class FilmDetailSnapshotTests
{
    [Fact]
    public async Task GetById_Returns_Complex_Film_Structure()
    {
        // Arrange
        var substituteRepo = Substitute.For<IFilmRepository>();

<<<<<<< HEAD
        var film = FilmBuilder.AFilm()
=======
        var film = FilmBuilder
            .AFilm()
>>>>>>> etape3
            .WithId("film-abc-123")
            .WithTitle("Dune")
            .WithSummary("Sur la planète Arrakis...")
            .WithYear(2021)
            .WithDurationMinutes(155)
            .WithReleaseDate(new DateTime(2021, 9, 15))
<<<<<<< HEAD
            .WithDirector(DirectorBuilder.ADirector()
                .WithId("dir-1")
                .WithLastName("Villeneuve")
                .WithFirstName("Denis")
                .WithNationality("CA")
                .WithBirthDate(new DateTime(1967, 10, 3)))
            .WithActors(
                ActorBuilder.AnActor()
=======
            .WithDirector(
                DirectorBuilder
                    .ADirector()
                    .WithId("dir-1")
                    .WithLastName("Villeneuve")
                    .WithFirstName("Denis")
                    .WithNationality("CA")
                    .WithBirthDate(new DateTime(1967, 10, 3))
            )
            .WithActors(
                ActorBuilder
                    .AnActor()
>>>>>>> etape3
                    .WithId("a1")
                    .WithLastName("Chalamet")
                    .WithFirstName("Timothée")
                    .WithRole("Paul Atréides"),
<<<<<<< HEAD
                ActorBuilder.AnActor()
                    .WithId("a2")
                    .WithLastName("Zendaya")
                    .WithFirstName("")
                    .WithRole("Chani"))
            .WithGenres(
                GenreBuilder.AGenre()
                    .WithId("g1")
                    .WithName("Science-Fiction"),
                GenreBuilder.AGenre()
                    .WithId("g2")
                    .WithName("Aventure"))
            .WithProductionCountry(CountryBuilder.ACountry()
                .WithCode("US")
                .WithName("États-Unis"))
=======
                ActorBuilder
                    .AnActor()
                    .WithId("a2")
                    .WithLastName("Zendaya")
                    .WithFirstName("")
                    .WithRole("Chani")
            )
            .WithGenres(
                GenreBuilder.AGenre().WithId("g1").WithName("Science-Fiction"),
                GenreBuilder.AGenre().WithId("g2").WithName("Aventure")
            )
            .WithProductionCountry(CountryBuilder.ACountry().WithCode("US").WithName("États-Unis"))
>>>>>>> etape3
            .Build();

        substituteRepo.GetByIdAsync("film-abc-123").Returns(film);

        var service = new FilmService(substituteRepo);

        // Act
        var result = await service.GetByIdAsync("film-abc-123");

        // Assert
        var settings = new VerifySettings();
        settings.ScrubMembers("Id");
        settings.ScrubMembersWithType<DateTime>();

        await Verify(result, settings);
    }
}