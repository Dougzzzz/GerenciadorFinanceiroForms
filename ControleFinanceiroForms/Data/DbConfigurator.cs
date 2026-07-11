using System;
using System.IO;
using System.Linq;

namespace ControleFinanceiroForms.Data;

public static class DbConfigurator
{
    public static string GetDatabasePath(string[] args)
    {
        var e2eDbPathIndex = Array.IndexOf(args, "--e2e-db-path");
        if (e2eDbPathIndex >= 0 && e2eDbPathIndex < args.Length - 1)
        {
            var dbPath = args[e2eDbPathIndex + 1];
            var dir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dbPath;
        }

        var dbFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ControleFinanceiro");
        Directory.CreateDirectory(dbFolder);
        return Path.Combine(dbFolder, "controle-financeiro.db");
    }
}
