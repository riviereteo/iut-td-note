# TD noté — Maîtrise des Tests Logiciels Avancés

## Prérequis

- **.NET 10** SDK
- **Docker** (pour Testcontainers en tests, et pour MongoDB / InfluxDB / Grafana via l’AppHost)
- **Task** (pour lancer les tests de charge/spike via le Taskfile) — [Installation Task](https://taskfile.dev/docs/installation)
- IDE (Visual Studio, Rider ou VS Code)

---

## Structure du projet

```
├── FilmApi.slnx
├── README.md
├── AppHost/                    # Aspire : MongoDB, FilmApi, seeds (50k, 500k), InfluxDB, Grafana
├── src/
│   └── FilmApi/                # API (GET/POST /films), modèle Film imbriqué, MongoDB
│       ├── Models/             # Film, Director, Actor, Genre, Country, CreateFilmRequest, PagedResult
│       ├── Repositories/       # IFilmRepository, FilmRepository
│       └── Services/           # IFilmService, FilmService
├── SeedFilms/                  # Application console : seed 50k ou 500k (args)
├── k6/                         # Scripts k6
│   ├── load.js                 # Load test (50k ou 500k selon TOTAL_ITEMS)
│   └── spike.js                # Spike test (50k ou 500k)
├── grafana/
│   └── provisioning/           # Datasource InfluxDB + dashboard k6
├── reports/                    # Rapports des tests de performance (à compléter)
│   ├── captures/               # Captures d’écran Grafana (ex. load-50k.png)
│   ├── load-50k.md             # Template load test 50k
│   ├── load-500k.md            # Template load test 500k
│   ├── spike-50k.md            # Template spike test 50k
│   └── spike-500k.md           # Template spike test 500k
└── tests/
    └── FilmApi.Tests/          # Unitaires (mock) + intégration (MongoFixture, WebApplicationFactory)
        ├── MongoFixture.cs
        ├── FilmApiAppFactory.cs
        ├── FilmServiceUnitTests.cs      # À refactoriser : AAA + builders
        ├── FilmDetailSnapshotTests.cs   # À refactoriser : snapshot Verify
        └── FilmApiIntegrationTests.cs   # Intégration HTTP → MongoDB
```

---

## Objectifs du TD (3h)

1. **Refacto AAA** : tous les tests (unitaires et intégration) doivent être implémentés avec le pattern du AAA.
2. **Test data builders** : introduire des test data builder pour les objets du domaine (**FilmBuilder**, **DirectorBuilder**, **ActorBuilder**...) et refactoriser les tests existants
3. **Snapshot testing** : refactoriser **FilmDetailSnapshotTests** en passant par un snapshot test via **Verify**. Les Id / DateTime devront être "scrubé" (voir
   documentation ou ancien TD).
4. **Tests d’intégration** : Le projet utlise Mongo comme base de données ; écriture de **2 tests** d'intégrations :
    1. tester le **filtre par année de sortie** de `GET /films` : appeler `GET /films?releaseYear=XXXX` après avoir inséré des films d'années différentes, et
       s'assurer que seuls les films de l'année demandée sont retournés (et que le total correspond).
    2. TODO
5. **Tests de performance** : lancer la stack via **Aspire**, puis une batterie de **test de perf** (exécution des tests via Task, voir ci-dessous), load et
   spike, chacun avec 50k et 500k en bdd ; consulter les métriques dans **Grafana** ; compléter par votre observation les fichiers `*.md` du dossier `/reports`.

Pour les tests de performances, vous pouvez suivre ces étapes :

| Étape | Action                                                                                                 |
|-------|--------------------------------------------------------------------------------------------------------|
| 1     | Lancer l’AppHost : `dotnet run --project AppHost` (à la racine).                                       |
| 2     | Dans le tableau de bord Aspire : lancer **seed-50k** ou **seed-500k**, attendre la fin de l’insertion. |
| 3     | Lancer un test k6 via Task (voir ci‑dessous).                                                          |
| 4     | Consulter les métriques : **http://localhost:3000/d/k6-load-testing/k6-load-testing** (Grafana).       |
| 5     | Prendre une capture d'écran de l'exécution du test et la stocker dans `reports/captures/`              |
| 6     | compléter la section **Observations** dans le `.md` (débit, latence, erreurs)                          |

```bash
task load-50k     # load test, 50 000 films
task load-500k    # load test, 500 000 films
task spike-50k    # spike test, 50 000 films
task spike-500k   # spike test, 500 000 films
```

Sans Task : regarder dans le fichier Taskfile.yml pour exécuter les commandes manuellement.