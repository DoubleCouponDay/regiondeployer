using MoonMachine.Infrastructure.Identity;
using System;

public class DatabaseCleaner
{
    public ApplicationDbContext OverrideConnectionString(string connectionString = null)
    {
        if (!string.IsNullOrEmpty(connectionString))
        {
            return new ApplicationDbContext(connectionString);
        }
        else
        {
            return new ApplicationDbContext();
        }
    }
}