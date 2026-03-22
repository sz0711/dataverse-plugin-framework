using System;
using Microsoft.Xrm.Sdk;
using MODEL;
using Moq;
using Xunit;
using DataversePluginFramework.Services.AccountServices;
using DataversePluginFramework.Services.Risk;
using PluginInfrastructure.Constants;

namespace DataversePluginFramework.Tests.Services
{
    public class AccountServiceTests
    {
        private readonly Mock<IOrganizationService> _orgService;
        private readonly Mock<ITracingService> _tracing;
        private readonly Mock<IRiskService> _riskService;
        private readonly AccountService _sut;

        public AccountServiceTests()
        {
            _orgService = new Mock<IOrganizationService>();
            _tracing = new Mock<ITracingService>();
            _riskService = new Mock<IRiskService>();
            _sut = new AccountService(_orgService.Object, _tracing.Object, _riskService.Object);
        }

        [Fact]
        public void Process_NullAccount_TracesAndReturns()
        {
            _sut.Process(null);

            _tracing.Verify(t => t.Trace(It.Is<string>(s => s.Contains("null"))), Times.Once);
            _riskService.Verify(r => r.Evaluate(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public void Process_AccountWithoutRevenue_TracesAndReturns()
        {
            var account = new Account { Name = "TestCo" };

            _sut.Process(account);

            _tracing.Verify(t => t.Trace(It.Is<string>(s => s.Contains("no revenue"))), Times.Once);
            _riskService.Verify(r => r.Evaluate(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public void Process_RevenueBelowThreshold_DoesNotMarkStrategic()
        {
            var account = new Account
            {
                Name = "SmallCo",
                Revenue = new Money(1_000_000m)
            };

            _sut.Process(account);

            Assert.NotEqual(PluginConstants.StrategicCustomerDescription, account.Description);
            _riskService.Verify(r => r.Evaluate(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public void Process_RevenueAboveThreshold_MarksStrategicAndEvaluatesRisk()
        {
            var account = new Account
            {
                Name = "BigCorp",
                Revenue = new Money(10_000_000m)
            };

            _sut.Process(account);

            Assert.Equal(PluginConstants.StrategicCustomerDescription, account.Description);
            _riskService.Verify(r => r.Evaluate(account), Times.Once);
        }

        [Fact]
        public void Process_RevenueExactlyAtThreshold_DoesNotMarkStrategic()
        {
            var account = new Account
            {
                Name = "EdgeCo",
                Revenue = new Money(PluginConstants.StrategicRevenueThreshold)
            };

            _sut.Process(account);

            Assert.NotEqual(PluginConstants.StrategicCustomerDescription, account.Description);
            _riskService.Verify(r => r.Evaluate(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public void Process_RiskServiceThrows_ContinuesWithoutRethrowing()
        {
            var account = new Account
            {
                Name = "RiskyCo",
                Revenue = new Money(10_000_000m)
            };
            _riskService.Setup(r => r.Evaluate(It.IsAny<Account>()))
                .Throws(new InvalidOperationException("Risk engine failure"));

            // Should NOT throw – risk evaluation is non-critical
            _sut.Process(account);

            Assert.Equal(PluginConstants.StrategicCustomerDescription, account.Description);
            _tracing.Verify(t => t.Trace(It.Is<string>(s => s.Contains("Risk evaluation failed"))), Times.Once);
        }
    }
}
