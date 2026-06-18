# Clinic Manager

[![.NET CI/CD](https://github.com/Koczi11/Clinic_Manager/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/Koczi11/Clinic_Manager/actions/workflows/dotnet-ci.yml)

**Clinic Manager** to nowoczesny system wspomagania zarządzania placówką medyczną. Zaimplementowany przy użyciu platformy **.NET 10** oraz technologii **Razor Pages**, system umożliwia pełne zarządzanie kartotekami pacjentów, dokumentacją medyczną (w tym skanami dokumentów), wizytami lekarskimi, notatkami klinicznymi, lekami oraz raportowaniem finansowym.

---

## 🛠️ Stos Technologiczny

*   **Framework**: .NET 10 (ASP.NET Core)
*   **Baza Danych**: Microsoft SQL Server (EF Core + Migracje Code First)
*   **Uwierzytelnianie**: ASP.NET Core Identity
*   **Mapowanie obiektów**: Riok.Mapperly (wysoka wydajność)
*   **Logowanie zdarzeń**: NLog (zapisywanie błędów do pliku)
*   **Generowanie raportów**: QuestPDF
*   **Testy jednostkowe**: xUnit
*   **Testy obciążeniowe**: NBomber
*   **Frontend**: Razor Pages + Bootstrap

---

## 🔐 Uwierzytelnianie, Autoryzacja i Role

Dostęp do poszczególnych modułów systemu jest autoryzowany na podstawie przypisanych ról użytkowników. System posiada wbudowany mechanizm automatycznego seedowania bazy danych przy starcie.

### Dostępne Role i Uprawnienia:

1.  **Admin (Administrator)**:
    *   Pełny dostęp do wszystkich sekcji systemu.
    *   Zarządzanie katalogiem leków.
    *   Dostęp do generowania raportów kosztów.
2.  **Rejestratorka (Personel Rejestracyjny)**:
    *   Tworzenie i podgląd profili pacjentów.
    *   Wgrywanie skanów dokumentów medycznych do kartoteki.
    *   Planowanie wizyt i zmiana ich statusów.
    *   Dostęp do katalogu leków (tylko odczyt/edycja słownika).
    *   Dostęp do raportów finansowych przychodni.
    *   *Brak dostępu do edycji danych medycznych (notatki lekarskie, przepisywanie leków w wizycie).*
3.  **Lekarz (Personel Medyczny)**:
    *   Podgląd pacjentów i przypisanych do siebie wizyt.
    *   Prowadzenie notatek klinicznych (rozpoznanie, zalecenia).
    *   Dodawanie wykonanych procedur oraz przepisanych leków z katalogu bezpośrednio do karty wizyty.
    *   *Brak dostępu do całościowego katalogu leków oraz raportów kosztów.*

### Dane Testowe do Logowania (Seeded Accounts):

*   **Administrator**:
    *   Login: `admin@clinic.com`
    *   Hasło: `SecureAdmin123!`
*   **Lekarz**:
    *   Login: `lekarz@clinic.com`
    *   Hasło: `SecureDoctor123!`
*   **Rejestratorka**:
    *   Login: `rejestratorka@clinic.com`
    *   Hasło: `SecureStaff123!`

---

## 🚀 Uruchomienie Projektu Lokalnie

### Wymagania wstępne:
*   .NET 10 SDK
*   Docker (dla uruchomienia bazy danych SQL Server)

### Instrukcja krok po kroku:

1.  **Uruchomienie bazy danych w Dockerze**:
    Przejdź do folderu głównego i uruchom bazę danych SQL Server za pomocą przygotowanego kontenera Docker (lub lokalnej instancji):
    ```bash
    docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=ClinicDev!2026" -p 1433:1433 --name clinic-sql -d mcr.microsoft.com/mssql/server:2022-latest
    ```

2.  **Wykonanie migracji bazy danych**:
    EF Core automatycznie zainicjalizuje schemat i zaaplikuje migracje na bazie SQL Server przy pierwszym uruchomieniu aplikacji, ale możesz to wywołać również ręcznie w katalogu `Clinic_Manager`:
    ```bash
    dotnet ef database update
    ```

3.  **Uruchomienie serwera**:
    W folderze `Clinic_Manager/Clinic_Manager` uruchom projekt:
    ```bash
    dotnet run
    ```
    Aplikacja uruchomi się domyślnie pod adresem: `http://localhost:5080` lub `https://localhost:7080`.

---

## 🩺 Funkcjonalności i Moduły

*   **CRUD Pacjentów & Kartoteka**: Dodawanie, edycja i wyszukiwanie pacjentów po nazwisku lub PESEL. RODO: Pacjenci nie są usuwani fizycznie, lecz ukrywani za pomocą filtru *Soft Delete*.
*   **Skanowanie / Upload Skierowań**: Możliwość załączenia dokumentu PDF/obrazu (do 5MB) do karty pacjenta. Pliki zapisywane są w `wwwroot/uploads`.
*   **Obsługa Wizyt**: Rejestrowanie wizyt, statusy (`Scheduled`, `InProgress`, `Completed`, `Canceled`), przypisywanie lekarzy. Wyliczany koszt wizyty (procedury + leki).
*   **Notatki Kliniczne i Recepty**: Lekarz w szczegółach wizyty uzupełnia wywiad lekarski, wybiera leki z katalogu z określeniem dawkowania i wykonuje procedury medyczne.
*   **RODO Audyt**: Każde wyświetlenie szczegółów medycznych pacjenta przez personel zapisuje w logach serwera audyt (użytkownik, ID wizyty, ID pacjenta, znacznik czasu).
*   **Raporty Kosztów (QuestPDF)**: Generowanie eleganckich zestawień kosztów świadczeń medycznych dla pacjenta, lekarza lub wybranego miesiąca z możliwością eksportu do PDF.
*   **Logowanie błędów (NLog)**: Logi aplikacji i pełne zrzuty stosu (Stack Trace) nieobsłużonych wyjątków zapisywane są do pliku `logs/errors.log`.
*   **Usługa w Tle (BackgroundService)**: Serwis `UpcomingVisitsReportBackgroundService` generuje raz na dobę raport nadchodzących wizyt i wysyła go e-mailem jako załącznik PDF administratorowi.

---

## 🧪 Uruchamianie Testów

### Testy Jednostkowe (xUnit)
Aby uruchomić testy jednostkowe na swoim komputerze, przejdź do folderu `Clinic_Manager` i wykonaj:
```bash
dotnet test
```

### Testy Wydajnościowe (NBomber)
Aby przeprowadzić test obciążeniowy dedykowanego endpointu `GET /api/visits/active`, upewnij się, że aplikacja główna działa, a następnie w folderze `Clinic_Manager/ClinicManager.PerformanceTests` wykonaj:
```bash
dotnet run
```
Wyniki i statystyki testu obciążeniowego (50 wątków, 100 iteracji) zostaną zapisane w katalogu `nbomber-reports`.

---

## ⚙️ Potok CI/CD (GitHub Actions)

W repozytorium skonfigurowany jest automatyczny potok CI/CD realizowany przez GitHub Actions ([dotnet-ci.yml](file:///.github/workflows/dotnet-ci.yml)).

### Jak to działa?
Potok uruchamia się automatycznie przy każdym:
*   **Push** na gałęzie `main` oraz `develop`.
*   **Pull Request** kierowanym do gałęzi `main` oraz `develop`.

### Kroki potoku:
1.  **Pobranie repozytorium** (`actions/checkout`).
2.  **Konfiguracja środowiska .NET 10** (`actions/setup-dotnet`) przy użyciu wersji `10.0.x`.
3.  **Przywrócenie zależności** (`dotnet restore`).
4.  **Kompilacja aplikacji** (`dotnet build --no-restore`).
5.  **Uruchomienie testów jednostkowych** (`dotnet test --no-build --verbosity normal`).
