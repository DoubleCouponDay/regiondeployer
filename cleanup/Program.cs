﻿using System;
using RegionDeployer;
using RegionDeployer.AzureAuthenticator;
using RegionDeployer.CloudActions.ResourceGroup;
using RegionDeployer.DatabaseSeeder;
using RegionDeployer.DatabaseCleaner;
using RegionDeployer.Logger;
using MoonMachine.Credentials;
using MoonMachine.Infrastructure.Models;

public class Program
{
    public static void Main(string[] args)
    {
        Initialize();
        ConfigAdapter.Initialize();
        var currentConfig = ProjectCredentials.Get.MatchingCurrentEnvironment();
        var currentEnvironment = Enum.GetName(typeof(CurrentEnvironment), currentConfig.CurrentEnvironment);
        LogToConsoleAndFile("Cleaning deployment and initial state of environment: " + currentEnvironment);
        DeleteFarmerGroupsMatchingCurrentEnvironmentMode(LogToConsoleAndFile);
        DatabaseCleaner.RemoveInitialState(LogToConsoleAndFile);
    }
}