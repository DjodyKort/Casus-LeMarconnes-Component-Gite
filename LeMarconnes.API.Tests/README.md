# Le Marconnès Gîte API - Test Suite

## 📊 Test Overzicht

Dit project bevat **39 integration tests** die alle API endpoints dekken.

## 🏗️ Test Architectuur

### WebApplicationFactory

- `TestWebApplicationFactory.cs` - Custom factory met mock repository
- Gebruikt `Moq` voor repository mocking
- Geen echte database nodig

### Test Data

- `TestDataFactory.cs` - Factory voor consistent test data
- Voorkomt duplicatie van test setup code

## 🧪 Test Bestanden

### 1. ReserveringenControllerTests (10 tests)

✅ POST /boeken - Succesvol boeken
✅ POST /boeken - Niet beschikbaar fout
✅ GET /{id} - Details ophalen
✅ GET /{id} - 404 als niet bestaat
✅ GET /gast/{gastId} - Lijst per gast
✅ GET /{id}/details - Kostenopbouw
✅ PUT /{id} - Update reservering
✅ PATCH /{id}/annuleer - Annuleer reservering
✅ GET / - Alle reserveringen (admin)
✅ PATCH /{id}/status - Status wijzigen (admin)
✅ DELETE /{id} - Hard delete (admin)

### 2. VerhuurEenhedenControllerTests (6 tests)

✅ GET / - Alle eenheden
✅ GET /{id} - Details
✅ POST / - Nieuwe eenheid (admin)
✅ PUT /{id} - Update eenheid (admin)
✅ DELETE /{id} - Verwijderen zonder reserveringen
✅ DELETE /{id} - Fout bij actieve reserveringen

### 3. TarievenControllerTests (6 tests)

✅ GET / - Alle tarieven
✅ GET /{typeId}/{platformId} - Geldig tarief
✅ GET /berekenen - Prijsberekening preview
✅ POST / - Nieuw tarief (admin)
✅ PUT /{id} - Update tarief (admin)
✅ DELETE /{id} - Verwijder tarief (admin)

### 4. OtherControllersTests (17 tests)

#### GastenController (5 tests)

✅ GET / - Alle gasten (admin)
✅ GET /{id} - Gast details
✅ PUT /{id} - Update NAW
✅ POST / - Nieuwe gast (admin)
✅ DELETE /{id} - Anonimiseren (GDPR)

#### LookupsController (9 tests)

✅ GET /platformen - Alle platformen
✅ GET /accommodatietypes - Alle types
✅ POST /accommodatietypes - Nieuw type (admin)
✅ PUT /accommodatietypes/{id} - Update type (admin)
✅ DELETE /accommodatietypes/{id} - Verwijder type (admin)
✅ GET /tariefcategorieen - Alle categorieën
✅ GET /gebruikers - Alle gebruikers

#### LogboekenController (2 tests)

✅ GET / - Recent logs (admin)
✅ GET /entiteit/{type}/{id} - Logs per entiteit

#### BeschikbaarheidController (1 test)

✅ GET / - Beschikbaarheid check

## 🚀 Tests Uitvoeren

### Alle tests

```bash
cd LeMarconnes.API.Tests
dotnet test
```

### Specifieke test class

```bash
dotnet test --filter "FullyQualifiedName~ReserveringenControllerTests"
```

### Met verbose output

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Coverage report

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📋 Test Resultaten Verwacht

- **Totaal tests**: 39
- **Expected Pass**: 39
- **Expected Fail**: 0
- **Test Duration**: < 10 seconden

## 🔍 Wat Wordt Getest?

### ✅ Happy Paths

- Succesvolle GET requests
- Succesvolle POST/PUT/DELETE operaties
- Correcte HTTP status codes (200, 201, 204)

### ✅ Error Paths

- 404 Not Found scenarios
- 400 Bad Request bij validatie fouten
- Business logic fouten (bijv. geen beschikbaarheid)

### ✅ Edge Cases

- Parent-child beschikbaarheid logica
- GDPR anonimisatie
- Actieve reserveringen bij delete

## 🛠️ Mock Setup

Alle tests gebruiken `Moq` voor repository mocking:

- Geen echte database queries
- Snel (< 10 seconden voor alle tests)
- Geïsoleerd (geen side effects)
- Voorspelbaar (consistent test data)

## 📦 Dependencies

- xUnit - Test framework
- Microsoft.AspNetCore.Mvc.Testing - Integration testing
- Moq - Mocking framework
- FluentAssertions - Readable assertions

## 🎯 Next Steps

1. **Run tests**: `dotnet test`
2. **Check coverage**: Alle controllers zijn gedekt
3. **Add more tests**: Unit tests voor services indien nodig
4. **CI/CD**: Integreer tests in build pipeline
