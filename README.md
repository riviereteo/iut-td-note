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

1. **Refacto AAA** : tous les tests (unitaires et intégration) avec blocs **Arrange / Act / Assert** clairs.
2. **Test data builders** : introduire **FilmBuilder** (et éventuellement DirectorBuilder, ActorBuilder), refactoriser **au moins 3 tests** pour les utiliser.
3. **Snapshot testing** : refactoriser **au moins un test** (ex. FilmDetailSnapshotTests) avec **Verify** ; gérer Id / DateTime avec les options Verify.
4. **Tests d’intégration MongoDB** : MongoFixture + WebApplicationFactory déjà fournis ; écrire / compléter **au moins 2 tests** (POST 201, GET après POST).
5. **Aspire** : lancer une **seed** (50k ou 500k), puis un **test de perf** (load ou spike, 50k ou 500k) ; consulter les métriques dans **Grafana**.

---

## Consignes par partie (critères de notation)

### 1. Refacto AAA

- **Objectif** : Chaque test doit avoir des commentaires ou un découpage clair **Arrange** (données, mocks), **Act** (un seul appel métier ou HTTP),
  **Assert** (uniquement des vérifications).
- **Critères** : lisibilité, séparation nette des trois blocs, pas d’assertion dans l’Act.

### 2. Test Data Builders

- **Objectif** : Avec le modèle **Film** imbriqué (Réalisateur, Acteurs, Genres, etc.), construire des instances en test via des **builders** (valeurs par
  défaut, surcharge uniquement de ce qui compte).
- **À faire** : introduire **FilmBuilder** (API fluide : `WithTitle`, `WithYear`, `WithDirector`, `WithActors`, `WithGenres`, etc.), et éventuellement
  **DirectorBuilder**, **ActorBuilder**. Refactoriser **au moins 3 tests** pour utiliser ces builders.
- **Critères** : API fluide, valeurs par défaut cohérentes, au moins un test avec customisation partielle (ex. seul le titre ou seul le réalisateur change).

### 3. Snapshot testing — Verify

- **Objectif** : Remplacer les longues séries d’`Assert.Equal` sur un DTO complexe par **un snapshot** Verify.
- **À faire** : refactoriser **au moins un test** (ex. `FilmDetailSnapshotTests`) en utilisant Verify (ex. `await Verify(filmDetailDto)`). Générer le fichier
  `.verified.*` au premier run. Pour les champs instables (Id, Guid, DateTime), utiliser les paramètres Verify (scoped settings, ignore de membres).
  **Verify.Xunit** est déjà installé dans le projet de tests ; s’appuyer sur la [documentation Verify](https://github.com/VerifyTests/Verify).
- **Critères** : au moins un test converti en snapshot sur une sortie « Film complexe » ; snapshot stable ; fichier snapshot versionné ou expliqué dans le
  README.

### 4. Tests d’intégration MongoDB

- **Objectif** : Les tests d’intégration font tourner l’API avec une base **MongoDB** (Testcontainers.MongoDb).
- **À faire** : **implémenter un test d'intégration** qui vérifie le **filtre par année de sortie** du `GET /films` : appeler `GET /films?releaseYear=XXXX`
  après avoir inséré des films d'années différentes, et s'assurer que seuls les films de l'année demandée sont retournés (et que le total correspond).
- **Critères** : conteneur MongoDB partagé (IClassFixture), API lancée avec MongoDB en tests, au moins 2 tests passants (HTTP → Service → MongoDB), plus un
  test d'intégration pour le filtre par année de sortie sur `GET /films`.

### 5. Tests de performance

- **Objectif** : Exécuter le workflow seed → test de charge → visualisation Grafana (50k ou 500k films ; load ou spike).
- **Critères** : AppHost avec MongoDB, InfluxDB et Grafana ; seed-50k / seed-500k utilisées ; tests k6 (load.js, spike.js) lancés ; métriques consultées dans
  Grafana.

#### Workflow (à suivre une seule fois par type de test)

| Étape | Action                                                                                                 |
|-------|--------------------------------------------------------------------------------------------------------|
| 1     | Lancer l’AppHost : `dotnet run --project AppHost` (à la racine).                                       |
| 2     | Dans le tableau de bord Aspire : lancer **seed-50k** ou **seed-500k**, attendre la fin de l’insertion. |
| 3     | Lancer un test k6 (voir ci‑dessous).                                                                   |
| 4     | Consulter les métriques : **http://localhost:3000/d/k6-load-testing/k6-load-testing** (Grafana).       |

**Lancer un test k6** (nécessite [Task](https://taskfile.dev/docs/installation)) :

```bash
task load-50k     # load test, 50 000 films
task load-500k    # load test, 500 000 films
task spike-50k    # spike test, 50 000 films
task spike-500k   # spike test, 500 000 films
```

Sans Task : construire l’image puis exécuter avec `docker compose -f docker-compose.k6.yml` en passant `BASE_URL`, `TOTAL_ITEMS` (50000 ou 500000) et le
script (`load.js` ou `spike.js`) — voir le Taskfile pour la commande exacte.

#### Rapports à rendre (`reports/`)

| Fichier         | Commande          | Description               |
|-----------------|-------------------|---------------------------|
| `load-50k.md`   | `task load-50k`   | Load test, 50 000 films   |
| `load-500k.md`  | `task load-500k`  | Load test, 500 000 films  |
| `spike-50k.md`  | `task spike-50k`  | Spike test, 50 000 films  |
| `spike-500k.md` | `task spike-500k` | Spike test, 500 000 films |

Pour chaque rapport : exécuter le test → capture d’écran du dashboard Grafana dans `reports/captures/` (nom du fichier indiqué dans le template) → compléter la
section **Observations** dans le `.md` (débit, latence, erreurs).