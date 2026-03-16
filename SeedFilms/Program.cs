using FilmApi.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

var basePath = AppContext.BaseDirectory;
var config = new ConfigurationBuilder()
    .SetBasePath(basePath)
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

static string? NonEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

var conn = NonEmpty(Environment.GetEnvironmentVariable("ConnectionStrings__mongodb"))
    ?? NonEmpty(config.GetConnectionString("mongodb"))
    ?? throw new InvalidOperationException("Chaîne de connexion MongoDB manquante. Définir ConnectionStrings__mongodb (Aspire) ou ConnectionStrings:mongodb.");

var dbName = NonEmpty(Environment.GetEnvironmentVariable("FILMAPI_DATABASENAME"))
    ?? config["MongoDb:DatabaseName"]
    ?? throw new InvalidOperationException("DatabaseName manquant. Définir FILMAPI_DATABASENAME (Aspire) ou MongoDb:DatabaseName.");

var validCounts = new[] { 50_000, 500_000 };
if (args.Length == 0 || !int.TryParse(args[0], out var total) || !validCounts.Contains(total))
{
    Console.WriteLine("Usage: dotnet run --project SeedFilms -- <count>");
    Console.WriteLine("  count: 50000 | 500000");
    return 1;
}

var pack = new ConventionPack { new CamelCaseElementNameConvention() };
ConventionRegistry.Register("camelCase", pack, _ => true);

var client = new MongoClient(conn);
var database = client.GetDatabase(dbName);
var collection = database.GetCollection<Film>("films");

Console.WriteLine("Vidage de la collection films...");
await collection.DeleteManyAsync(FilterDefinition<Film>.Empty);

const int batchSize = 5000;
var inserted = 0;
var sw = System.Diagnostics.Stopwatch.StartNew();

// Données seed allégées : un réalisateur, un genre par film (plan §1)
var defaultDirector = new Director
{
    Id = ObjectId.GenerateNewId().ToString(),
    LastName = "Default",
    FirstName = "Director",
    Nationality = "FR",
    BirthDate = new DateTime(1970, 1, 1)
};
var defaultGenre = new Genre { Id = ObjectId.GenerateNewId().ToString(), Name = "Drama" };
var defaultCountry = new Country { Code = "FR", Name = "France" };

for (var offset = 0; offset < total; offset += batchSize)
{
    var count = Math.Min(batchSize, total - offset);
    var films = new List<Film>(count);
    for (var i = 0; i < count; i++)
    {
        var year = 2000 + (offset + i) % 25;
        films.Add(new Film
        {
            Title = $"Film-{offset + i + 1}",
            Summary = $"Resume for film {offset + i + 1}.",
            Year = year,
            DurationMinutes = 90 + (offset + i) % 60,
            ReleaseDate = new DateTime(year, 1, 1).AddDays((offset + i) % 8000),
            Director = defaultDirector,
            Genres = new List<Genre> { defaultGenre },
            Actors = new List<Actor>(),
            ProductionCountry = defaultCountry
        });
    }
    await collection.InsertManyAsync(films);
    inserted += count;
    Console.WriteLine($"  Inséré {inserted} / {total}");
}

sw.Stop();
Console.WriteLine($"Terminé : {inserted} films en {sw.ElapsedMilliseconds} ms.");
return 0;
