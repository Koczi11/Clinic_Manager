# Clinic Manager

[![.NET CI/CD](https://github.com/Koczi11/Clinic_Manager/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/Koczi11/Clinic_Manager/actions/workflows/dotnet-ci.yml)

System zarządzania przychodnią lekarską (Clinic Manager) zbudowany przy użyciu platformy .NET 10 oraz Razor Pages.

## Potok CI/CD (GitHub Actions)

W repozytorium skonfigurowany jest automatyczny potok CI/CD realizowany przez GitHub Actions ([dotnet-ci.yml](file:///.github/workflows/dotnet-ci.yml)).

### Jak to działa?
Potok uruchamia się automatycznie przy każdym:
* **Push** na gałęzie `main` oraz `develop`.
* **Pull Request** kierowanym do gałęzi `main` oraz `develop`.

### Kroki potoku:
1. **Pobranie repozytorium** (`actions/checkout`).
2. **Konfiguracja środowiska .NET 10** (`actions/setup-dotnet`) przy użyciu wersji `10.0.x`.
3. **Przywrócenie zależności** (`dotnet restore`).
4. **Kompilacja aplikacji** (`dotnet build --no-restore`).
5. **Uruchomienie testów jednostkowych** (`dotnet test --no-build --verbosity normal`).

---

## Uruchamianie testów lokalnie

Aby uruchomić testy jednostkowe na swoim komputerze, przejdź do folderu z rozwiązaniem (`Clinic_Manager`) i wykonaj:

```bash
dotnet test
```
