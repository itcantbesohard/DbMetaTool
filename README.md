# DbMetaTool - Firebird Database Utility

**DbMetaTool** to prosta aplikacja konsolowa (CLI) napisana w C# (.NET 8.0), stworzona do automatyzacji kluczowych operacji na bazach danych **Firebird 5.0** w trybie Embedded (tworzenie, aktualizacja, eksport metadanych).

## 🚀 Wymagania i Uruchomienie

### 1. Wymagania natywne (Firebird Embedded)

Aplikacja wymaga natywnych plików bibliotek Firebird 5.0.

**WAŻNE:** Aby aplikacja działała poprawnie, należy pobrać **Firebird 5.0 Zip Kit** i skopiować niezbędne pliki (m.in. `fbclient.dll`, folder `plugins/`, folder `intl/`) bezpośrednio do katalogu wyjściowego kompilacji, np.:
```
/bin/Debug/net8.0/
└── fbclient.dll 
└── firebird.conf 
└── /plugins 
└── /intl
```

### 2. Użycie

Aplikacja przyjmuje polecenia jako pierwszy argument: `build-db`, `export-scripts` lub `update-db`.

#### a) Budowanie nowej bazy danych (`build-db`)

Tworzy nową bazę danych `.fdb` i natychmiast wykonuje skrypty inicjalizujące ze wskazanego katalogu.
W folderze projektu:

```bash
dotnet run -- build-db --db-dir "C:\db\fb5" --scripts-dir "C:\scripts"
```
Przykład wywołania i efekt:
<img width="1093" height="164" alt="image" src="https://github.com/user-attachments/assets/64b1ac84-9ad8-4bf5-9b83-63d587d67c87" />

#### b) Aktualizacja istniejącej bazy danych (update-db)
Łączy się z bazą (w ramach transakcji) i wykonuje skrypty SQL. Pliki .sql są sortowane alfabetycznie, dlatego zaleca się nadawanie im numerów, np. 01_domains.sql, 02_tables.sql.
```bash
dotnet run -- update-db --connection-string "Database=C:\db\fb5\baza.fdb;User=SYSDBA;Password=masterkey" --scripts-dir "C:\scripts"
```
#### c) Eksport metadanych (export-scripts)
Pobiera metadane (domeny, tabele, procedury) z bazy i zapisuje ich listę do plików w katalogu wyjściowym.
```bash
dotnet run -- export-scripts --connection-string "Database=C:\db\fb5\baza.fdb;User=SYSDBA;Password=masterkey" --output-dir "C:\out"
```
