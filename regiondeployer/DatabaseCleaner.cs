using System;
using System.Linq;
using MoonMachine.Infrastructure;
using MoonMachine.Infrastructure.Entities;

public class DatabaseCleaner
{
    private ApplicationDbContext database;
    private Action<string> LogMessage;

    public DatabaseCleaner(ApplicationDbContext db, Action<string> logMessage)
    {
        database = db;
        LogMessage = logMessage;
    }

    private void RemoveRegionalFarmerRecords()
    {
        LogMessage("Removing previous regional farmer records from database...");

        var farmers = database.RegionalFarmers.ToList();
        database.RegionalFarmers.RemoveRange(farmers);
        database.SaveChanges();

        LogMessage("Previous farmer records removed.");
    }

    private void RemoveApiContractRecords()
    {
        LogMessage("Removing previous API contract records...");

        var authFormats = database.AuthenticationFormats.ToList();
        database.AuthenticationFormats.RemoveRange(authFormats);

        foreach (var exchange in database.AvailableExchanges)
        {
            exchange.IsCurrent = false;
        }

        foreach (var market in database.AvailableMarkets)
        {
            market.IsCurrent = false; // Can't remove since a bunch of historic records depend on markets and exchanges existing in relationships
        }

        database.SaveChanges();

        LogMessage("Previous API contract records removed.");
    }

    private void RemoveScriptingLanguages()
    {
        LogMessage("Removing previous scripting language records...");

        foreach (var language in database.Languages)
        {
            language.IsCurrent = false;
        }

        database.SaveChanges();

        LogMessage("Previous scripting language records removed.");
    }

    public void RemoveInitialState(string connectionString = null)
    {
        if (!string.IsNullOrEmpty(connectionString))
        {
            database.Database.Connection.ConnectionString = connectionString;
        }

        RemoveScriptingLanguages();
        RemoveApiContractRecords();
        RemoveRegionalFarmerRecords();
    }
}