using MoonMachine.Infrastructure;
using MoonMachine.Infrastructure.Identity;
using MoonMachine.Infrastructure.Entities;
using System.IO;
using System.Data.Entity;
using nodeadapter.adapter;
using Microsoft.Azure.Management.AppService.Fluent;
using MoonMachine.Infrastructure.models;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;

public class DatabaseSeeder
{
    private ApplicationDbContext database;

    public DatabaseSeeder(ApplicationDbContext db)
    {
        database = db;
    }

    private AuthenticationFormat CreateNewAuthenticationFormat(AvailableExchange exchange, ExchangeContract.Root contract, Action<string> logMessage)
    {
        logMessage($"Creating new authentication format for exchange: {exchange.ExchangeName}");

        using (var writer = new StringWriter())
        {
            var newFormat = new AuthenticationFormat
            {
                Exchange = exchange
            };

            contract.Info.Authentication.Content.JsonValue.WriteTo(writer, JsonSaveOptions.None);
            newFormat.Content = writer.ToString();
            writer.Flush();

            contract.Info.Authentication.Headers.JsonValue.WriteTo(writer, JsonSaveOptions.None);
            newFormat.Headers = writer.ToString();
            writer.Flush();

            contract.Info.Authentication.Parameters.JsonValue.WriteTo(writer, JsonSaveOptions.None);
            newFormat.Parameters = writer.ToString();

            database.AuthenticationFormats.Add(newFormat);
            database.SaveChanges();

            logMessage("Authentication format created.");
            return newFormat;
        }
    }

    private AvailableExchange SeedAvailableExchange(ExchangeContract.Root contract, Action<string> logMessage)
    {
        logMessage($"Seeding available exchange: {contract.Info.ContractName}");

        var exchangeDuplicate = database.AvailableExchanges
            .FirstOrDefault(exchange => exchange.ExchangeName == contract.Info.ContractName);

        if (exchangeDuplicate == null)
        {
            var newExchange = new AvailableExchange
            {
                DaysAvailable = contract.Info.Frequencies.Days,
                HoursAvailable = contract.Info.Frequencies.Hours,
                MonthAvailable = contract.Info.Frequencies.Months,
                WeekAvailable = contract.Info.Frequencies.Weeks,
                ExchangeName = contract.Info.ContractName,
                RateLimitMilliseconds = contract.Info.RateLimit,
                IsCurrent = true
            };

            database.AvailableExchanges.Add(newExchange);
            logMessage("New available exchange added.");
            database.SaveChanges();
            return newExchange;
        }
        else
        {
            exchangeDuplicate.DaysAvailable = contract.Info.Frequencies.Days;
            exchangeDuplicate.HoursAvailable = contract.Info.Frequencies.Hours;
            exchangeDuplicate.MonthAvailable = contract.Info.Frequencies.Months;
            exchangeDuplicate.WeekAvailable = contract.Info.Frequencies.Weeks;
            exchangeDuplicate.RateLimitMilliseconds = contract.Info.RateLimit;
            exchangeDuplicate.IsCurrent = true;

            logMessage("Existing available exchange updated.");
            database.SaveChanges();
            return exchangeDuplicate;
        }
    }

    private void SeedAvailableMarkets(AvailableExchange marketsExchange, Action<string> logMessage)
    {
        var markets = GetMarkets(marketsExchange.ExchangeName, PathToExchangeAdaptersIndexFile);

        foreach (var currentInputMarket in markets)
        {
            logMessage($"Seeding available markets: {currentInputMarket.HoardedCurrency}, {currentInputMarket.PriceCurrency}");

            var marketDuplicate = database.AvailableMarkets
                .FirstOrDefault(market => market.ExchangeId == marketsExchange.Id &&
                                         market.Exchange.ExchangeName == marketsExchange.ExchangeName &&
                                         market.HoardedCurrency == currentInputMarket.HoardedCurrency &&
                                         market.PriceCurrency == currentInputMarket.PriceCurrency);

            if (marketDuplicate == null)
            {
                currentInputMarket.ExchangeId = marketsExchange.Id;
                currentInputMarket.IsCurrent = true;
                database.AvailableMarkets.Add(currentInputMarket);
                database.SaveChanges();
                logMessage("Added new market.");
            }
            else
            {
                marketDuplicate.IsCurrent = true;
                marketDuplicate.MinimumPrice = currentInputMarket.MinimumPrice;
                marketDuplicate.MinimumVolume = currentInputMarket.MinimumVolume;
                database.SaveChanges();
                logMessage("Filled existing market.");
            }
        }

        logMessage("Seeded available markets.");
    }

    // Additional methods for seeding other data like scripting languages and regional farmers would follow a similar pattern.
}
