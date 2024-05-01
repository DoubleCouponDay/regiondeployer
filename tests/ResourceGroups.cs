namespace Tests;

using Xunit;
using regiondeployer.cloudactions.resourcegroup;
using Microsoft.Azure.Management.Fluent;
using System;
using regiondeployer.clouderror;
using Xunit.Abstractions;
using regiondeployer;

public class WhenItCreatesAResourceGroup : IDisposable
{
    private string groupname = null;
    private readonly ITestOutputHelper logger;

    public WhenItCreatesAResourceGroup(ITestOutputHelper inputLogger)
    {
        configadapter.initialise();
        this.logger = inputLogger;
    }

    public void Dispose()
    {
        deleteresourcegroup(this.groupname, logger.WriteLine);
    }

    [Fact]
    public string ItSucceeds()
    {
        this.groupname = createrandomresourcegroup(defaultregion, logger.WriteLine);
        return this.groupname;
    }

    [Fact]
    public void ItsNameIsSuchALength()
    {
        var newregionname = this.ItSucceeds();
        Assert.True(newregionname.Length == maxgroupnamelength);
    }

    [Fact]
    public void ItsNameContainsDefaultGroupName()
    {
        var newregionname = this.ItSucceeds();
        Assert.True(newregionname.Contains(basegroupname));
    }
}