using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoonMachine.Infrastructure.Entities; // Assuming namespace based on provided context

public class ContractFiler
{
    private string folderPath;
    private Action<string> LogMessage;

    public ContractFiler(string folderPath, Action<string> logMessage)
    {
        this.folderPath = folderPath;
        this.LogMessage = logMessage;
    }

    private ExchangeContract.Root FilterContractThroughTests(string file)
    {
        LogMessage($"Filtering contract through tests: {file}");

        if (!file.EndsWith(".json"))
        {
            LogMessage("File is not a json file.");
            return null;
        }

        try
        {
            var contract = ExchangeContract.Load(file); // Assuming ExchangeContract.Load() is a method to load contract from file
            if (!string.IsNullOrEmpty(contract.Info.ContractName))
            {
                LogMessage("Contract passed tests.");
                return contract;
            }
            else
            {
                LogMessage("Contract is empty.");
                return null;
            }
        }
        catch
        {
            LogMessage("Contract could not be loaded.");
            return null; // Assuming some json files in the contract directory are not yet valid json
        }
    }

    public IEnumerable<ExchangeContract.Root> GetValidContractFiles()
    {
        LogMessage("Getting valid contract files...");
        var files = Directory.EnumerateFiles(folderPath);

        foreach (var file in files)
        {
            var testOutcome = FilterContractThroughTests(file);
            if (testOutcome != null)
            {
                yield return testOutcome;
            }
        }
    }
}