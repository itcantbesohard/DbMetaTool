using FirebirdSql.Data.FirebirdClient;
using System;

namespace DbMetaTool
{
    public static class Program
    {
        // Przykładowe wywołania:
        // DbMetaTool build-db --db-dir "C:\db\fb5" --scripts-dir "C:\scripts"
        // DbMetaTool export-scripts --connection-string "..." --output-dir "C:\out"
        // DbMetaTool update-db --connection-string "..." --scripts-dir "C:\scripts"
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Użycie:");
                Console.WriteLine("  build-db --db-dir <ścieżka> --scripts-dir <ścieżka>");
                Console.WriteLine("  export-scripts --connection-string <connStr> --output-dir <ścieżka>");
                Console.WriteLine("  update-db --connection-string <connStr> --scripts-dir <ścieżka>");
                return 1;
            }

            try
            {
                var command = args[0].ToLowerInvariant();

                switch (command)
                {
                    case "build-db":
                        {
                            string dbDir = GetArgValue(args, "--db-dir");
                            string scriptsDir = GetArgValue(args, "--scripts-dir");

                            BuildDatabase(dbDir, scriptsDir);
                            Console.WriteLine("Baza danych została zbudowana pomyślnie.");
                            return 0;
                        }

                    case "export-scripts":
                        {
                            string connStr = GetArgValue(args, "--connection-string");
                            string outputDir = GetArgValue(args, "--output-dir");

                            ExportScripts(connStr, outputDir);
                            Console.WriteLine("Skrypty zostały wyeksportowane pomyślnie.");
                            return 0;
                        }

                    case "update-db":
                        {
                            string connStr = GetArgValue(args, "--connection-string");
                            string scriptsDir = GetArgValue(args, "--scripts-dir");

                            UpdateDatabase(connStr, scriptsDir);
                            Console.WriteLine("Baza danych została zaktualizowana pomyślnie.");
                            return 0;
                        }

                    default:
                        Console.WriteLine($"Nieznane polecenie: {command}");
                        return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd: " + ex.Message);
                return -1;
            }
        }

        private static string GetArgValue(string[] args, string name)
        {
            int idx = Array.IndexOf(args, name);
            if (idx == -1 || idx + 1 >= args.Length)
                throw new ArgumentException($"Brak wymaganego parametru {name}");
            return args[idx + 1];
        }

        /// <summary>
        /// Buduje nową bazę danych Firebird 5.0 na podstawie skryptów.
        /// </summary>
        public static void BuildDatabase(string databaseDirectory, string scriptsDirectory)
        {
            // TODO:
            // 1) Utwórz pustą bazę danych FB 5.0 w katalogu databaseDirectory.
            // 2) Wczytaj i wykonaj kolejno skrypty z katalogu scriptsDirectory
            //    (tylko domeny, tabele, procedury).
            // 3) Obsłuż błędy i wyświetl raport.

            string dbPath = Path.Combine(databaseDirectory, "newdb.fdb");

            if (!Directory.Exists(databaseDirectory))
            {
                Console.WriteLine($"Tworzenie katalogu: {databaseDirectory}");
                Directory.CreateDirectory(databaseDirectory);
            }

            if (File.Exists(dbPath))
            {
                throw new IOException($"Plik bazy danych już istnieje: {dbPath}. Usuń go ręcznie lub zmień katalog, aby uniknąć utraty danych.");
            }

            string connStr = $"Database={dbPath};User=SYSDBA;Password=masterkey;ServerType=Embedded;ClientLibrary=fbclient.dll;WireCrypt=Disabled";

            Console.WriteLine("Tworzenie pliku bazy danych...");
            FbConnection.CreateDatabase(connStr);

            Console.WriteLine("Aplikowanie skryptów startowych...");
            UpdateDatabase(connStr, scriptsDirectory);
        }

        /// <summary>
        /// Generuje skrypty metadanych z istniejącej bazy danych Firebird 5.0.
        /// </summary>
        public static void ExportScripts(string connectionString, string outputDirectory)
        {
            // TODO:
            // 1) Połącz się z bazą danych przy użyciu connectionString.
            // 2) Pobierz metadane domen, tabel (z kolumnami) i procedur.
            // 3) Wygeneruj pliki .sql / .json / .txt w outputDirectory.

            Directory.CreateDirectory(outputDirectory);

            using var conn = new FbConnection(connectionString);
            conn.Open();

            // Domeny
            using var cmdDomains = new FbCommand("SELECT RDB$FIELD_NAME FROM RDB$FIELDS WHERE RDB$SYSTEM_FLAG=0", conn);
            using var readerDomains = cmdDomains.ExecuteReader();
            using var domainFile = new StreamWriter(Path.Combine(outputDirectory, "domains.sql"));
            while (readerDomains.Read())
            {
                string domainName = readerDomains.GetString(0).Trim();
                domainFile.WriteLine($"-- DOMAIN: {domainName}");
            }

            // Tabele
            using var cmdTables = new FbCommand("SELECT RDB$RELATION_NAME FROM RDB$RELATIONS WHERE RDB$SYSTEM_FLAG=0", conn);
            using var readerTables = cmdTables.ExecuteReader();
            using var tableFile = new StreamWriter(Path.Combine(outputDirectory, "tables.sql"));
            while (readerTables.Read())
            {
                string tableName = readerTables.GetString(0).Trim();
                tableFile.WriteLine($"-- TABLE: {tableName}");
            }

            // Procedury
            using var cmdProcedures = new FbCommand("SELECT RDB$PROCEDURE_NAME FROM RDB$PROCEDURES", conn);
            using var readerProcedures = cmdProcedures.ExecuteReader();
            using var procFile = new StreamWriter(Path.Combine(outputDirectory, "procedures.sql"));
            while (readerProcedures.Read())
            {
                string procName = readerProcedures.GetString(0).Trim();
                procFile.WriteLine($"-- PROCEDURE: {procName}");
            }

            Console.WriteLine($"Skrypty zapisane w: {outputDirectory}");

        }

        /// <summary>
        /// Aktualizuje istniejącą bazę danych Firebird 5.0 na podstawie skryptów.
        /// </summary>
        public static void UpdateDatabase(string connectionString, string scriptsDirectory)
        {
            // TODO:
            // 1) Połącz się z bazą danych przy użyciu connectionString.
            // 2) Wykonaj skrypty z katalogu scriptsDirectory (tylko obsługiwane elementy).
            // 3) Zadbaj o poprawną kolejność i bezpieczeństwo zmian.

            // 1) Połączenie
            using var conn = new FbConnection(connectionString);
            conn.Open();

            // Rozpoczynamy transakcję
            using var transaction = conn.BeginTransaction();

            try
            {
                Console.WriteLine("Rozpoczynanie aktualizacji bazy danych...");
                // 2) Zadbaj o poprawną kolejność
                // Pobieramy tylko pliki .sql i sortujemy je alfabetycznie.
                // WYMAGANIE: Nazywaj pliki np.: "01_tables.sql", "02_procs.sql".
                var files = Directory.GetFiles(scriptsDirectory, "*.sql")
                                     .OrderBy(f => f)
                                     .ToList();
                if (!files.Any())
                {
                    Console.WriteLine("Brak plików .sql w wskazanym katalogu.");
                    return;
                }

                foreach (var file in files)
                {
                    Console.WriteLine($"Wykonywanie pliku: {Path.GetFileName(file)}");

                    string scriptContent = File.ReadAllText(file);

                    var commands = scriptContent.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var sql in commands)
                    {
                        string cleanSql = sql.Trim();
                        if (string.IsNullOrWhiteSpace(cleanSql)) continue;

                        // Ignoruj komentarze
                        if (cleanSql.StartsWith("--")) continue;

                        using var cmd = new FbCommand(cleanSql, conn, transaction);
                        cmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
                Console.WriteLine("Baza danych zaktualizowana.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("Błąd podczas aktualizacji bazy danych: " + ex.Message);
                throw;

            }

        }
    }
}