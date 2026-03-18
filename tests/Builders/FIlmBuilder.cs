namespace FilmApi.Tests.Builders;

public class FilmBuilder
{
    private string _id = "1030";
    private string _title = "Le Silence des agneaux";
    private string _summary = "Un psychopathe connu sous le nom de Buffalo Bill sème la terreur dans le Middle West en kidnappant et en assassinant de jeunes femmes. ";
    private int _year = 1991;
    private int _durationMinutes = 118;
    private DateTime? _releaseDate = new DateTime(1991, 4, 10);
    private Director _director = DirectorBuilder.ADirector().Build();
    private List<Genre> _genres = [GenreBuilder.AGenre().Build()];
    private List<Actor> _actors = [ActorBuilder.AnActor().Build()];
    private Country? _productionCountry = CountryBuilder.ACountry().Build();

    public static FilmBuilder AFilm() => new();

    public FilmBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public FilmBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public FilmBuilder WithSummary(string summary)
    {
        _summary = summary;
        return this;
    }

    public FilmBuilder WithYear(int year)
    {
        _year = year;
        return this;
    }

    public FilmBuilder WithDurationMinutes(int durationMinutes)
    {
        _durationMinutes = durationMinutes;
        return this;
    }

    public FilmBuilder WithReleaseDate(DateTime? releaseDate)
    {
        _releaseDate = releaseDate;
        return this;
    }

    public FilmBuilder WithNoReleaseDate()
    {
        _releaseDate = null;
        return this;
    }

    public FilmBuilder WithDirector(Director director)
    {
        _director = director;
        return this;
    }

    public FilmBuilder WithDirector(DirectorBuilder directorBuilder)
    {
        _director = directorBuilder.Build();
        return this;
    }

    public FilmBuilder WithGenres(params Genre[] genres)
    {
        _genres = [..genres];
        return this;
    }

    public FilmBuilder WithGenres(params GenreBuilder[] genreBuilders)
    {
        _genres = genreBuilders.Select(b => b.Build()).ToList();
        return this;
    }

    public FilmBuilder WithActors(params Actor[] actors)
    {
        _actors = [..actors];
        return this;
    }

    public FilmBuilder WithActors(params ActorBuilder[] actorBuilders)
    {
        _actors = actorBuilders.Select(b => b.Build()).ToList();
        return this;
    }

    public FilmBuilder WithNoActors()
    {
        _actors = [];
        return this;
    }

    public FilmBuilder WithProductionCountry(Country? country)
    {
        _productionCountry = country;
        return this;
    }

    public FilmBuilder WithProductionCountry(CountryBuilder countryBuilder)
    {
        _productionCountry = countryBuilder.Build();
        return this;
    }

    public FilmBuilder WithNoProductionCountry()
    {
        _productionCountry = null;
        return this;
    }

    public Film Build() => new()
    {
        Id = _id,
        Title = _title,
        Summary = _summary,
        Year = _year,
        DurationMinutes = _durationMinutes,
        ReleaseDate = _releaseDate,
        Director = _director,
        Genres = _genres,
        Actors = _actors,
        ProductionCountry = _productionCountry
    };
}