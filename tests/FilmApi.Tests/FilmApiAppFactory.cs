using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

namespace FilmApi.Tests;

/// <summary>
/// Factory pour lancer l'API dans les tests en pointant MongoDB vers le conteneur Testcontainers.
/// On remplace IMongoClient via ConfigureTestServices pour pointer vers le conteneur Testcontainers.
/// </summary>
public class FilmApiAppFactory : WebApplicationFactory<Program>
{
    private readonly MongoFixture _mongo;

    public FilmApiAppFactory(MongoFixture mongo)
    {
        _mongo = mongo;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IMongoClient>();
            services.AddSingleton<IMongoClient>(new MongoClient(_mongo.GetConnectionString()));
        });
    }
}
