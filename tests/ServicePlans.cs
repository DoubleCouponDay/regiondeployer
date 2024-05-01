using Xunit;
using Xunit.Abstractions;
using Microsoft.Azure.Management.AppService.Fluent;
using System;

namespace Tests
{
    public class WhenALinuxServicePlanIsCreated : IDisposable
    {
        private readonly ITestOutputHelper _inputLogger;
        private readonly WhenAStorageAccountIsCreated _storageTestsField;
        private readonly IAppServicePlan _storageContextsField;

        public WhenALinuxServicePlanIsCreated(ITestOutputHelper inputLogger)
        {
            _inputLogger = inputLogger;
            _storageTestsField = new WhenAStorageAccountIsCreated(inputLogger);
            _storageContextsField = _storageTestsField.ItSucceeds();

            ConfigAdapter.Initialise();
        }

        public WhenAStorageAccountIsCreated StorageTests => _storageTestsField;
        public IAppServicePlan StorageContext => _storageContextsField;

        public void Dispose()
        {
            (_storageTestsField as IDisposable)?.Dispose();
        }

        [Fact]
        public IAppServicePlan ItSucceeds()
        {
            try
            {
                var testContext = CreateLinuxServicePlan(
                    _storageTestsField.GroupContext,
                    _inputLogger.WriteLine
                );
                return testContext;
            }
            catch (Exception error)
            {
                PrintCloudError(error, _inputLogger.WriteLine);
                RaiseCloudError(error);
                return null;
            }
        }
    }

    public class WhenABuggedLinuxAppFixIsApplied : IDisposable
    {
        private readonly WhenALinuxServicePlanIsCreated _servicePlanTests;
        private readonly IAppServicePlan _serverPlanContext;

        public WhenABuggedLinuxAppFixIsApplied(ITestOutputHelper inputLogger)
        {
            _servicePlanTests = new WhenALinuxServicePlanIsCreated(inputLogger);
            _serverPlanContext = _servicePlanTests.ItSucceeds();
        }

        public void Dispose()
        {
            (_servicePlanTests as IDisposable)?.Dispose();
        }

        [Fact]
        public void ItSucceeds()
        {
            try
            {
                var resourceGroupName = _servicePlanTests.StorageTests.GroupContext;
                var storageGroup = _servicePlanTests.StorageContext;
                FixLinuxAppBug(resourceGroupName, _inputLogger.WriteLine, storageGroup);
            }
            catch (Exception error)
            {
                PrintCloudError(error, _inputLogger.WriteLine);
                RaiseCloudError(error);
            }
        }
    }
}