using Microsoft.Xrm.Sdk;
using MODEL;
using Moq;
using Xunit;
using DataversePluginFramework.Services.Risk;

namespace DataversePluginFramework.Tests.Services
{
    public class RiskServiceTests
    {
        private readonly Mock<IOrganizationService> _orgService;
        private readonly Mock<ITracingService> _tracing;
        private readonly RiskService _sut;

        public RiskServiceTests()
        {
            _orgService = new Mock<IOrganizationService>();
            _tracing = new Mock<ITracingService>();
            _sut = new RiskService(_orgService.Object, _tracing.Object);
        }

        [Fact]
        public void Evaluate_NullAccount_ReturnsWithoutError()
        {
            _sut.Evaluate(null);

            _tracing.Verify(t => t.Trace(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Evaluate_AccountWithoutRevenue_NoTrace()
        {
            var account = new Account { Name = "NoRevCo" };

            _sut.Evaluate(account);

            _tracing.Verify(t => t.Trace(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Evaluate_HighRevenueAccount_TracesRiskAssessment()
        {
            var account = new Account
            {
                Name = "WealthyCorp",
                Revenue = new Money(10_000_000m)
            };

            _sut.Evaluate(account);

            _tracing.Verify(t => t.Trace(It.Is<string>(s => s.Contains("risk assessment"))), Times.Once);
        }

        [Fact]
        public void Evaluate_LowRevenueAccount_NoRiskTrace()
        {
            var account = new Account
            {
                Name = "SmallBiz",
                Revenue = new Money(100_000m)
            };

            _sut.Evaluate(account);

            _tracing.Verify(t => t.Trace(It.IsAny<string>()), Times.Never);
        }
    }
}
