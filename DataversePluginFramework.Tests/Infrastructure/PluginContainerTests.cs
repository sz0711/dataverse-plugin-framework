using System;
using Moq;
using Xunit;
using PluginInfrastructure.Infrastructure;

namespace DataversePluginFramework.Tests.Infrastructure
{
    public class PluginContainerTests
    {
        [Fact]
        public void RegisterInstance_And_Resolve_ReturnsSameInstance()
        {
            var provider = new Mock<IServiceProvider>();
            var container = new PluginContainer(provider.Object);
            var instance = "test-value";

            container.RegisterInstance<string>(instance);

            var resolved = container.Resolve<string>();
            Assert.Same(instance, resolved);
        }

        [Fact]
        public void Register_And_Resolve_CreatesInstanceWithInjectedDependencies()
        {
            var provider = new Mock<IServiceProvider>();
            var container = new PluginContainer(provider.Object);

            container.RegisterInstance<IDependency>(new ConcreteDependency());
            container.Register<ITestService, TestService>();

            var resolved = container.Resolve<ITestService>();
            Assert.NotNull(resolved);
            Assert.IsType<TestService>(resolved);
        }

        [Fact]
        public void Resolve_UnregisteredType_FallsBackToServiceProvider()
        {
            var expected = new ConcreteDependency();
            var provider = new Mock<IServiceProvider>();
            provider.Setup(p => p.GetService(typeof(IDependency))).Returns(expected);

            var container = new PluginContainer(provider.Object);

            var resolved = container.Resolve<IDependency>();
            Assert.Same(expected, resolved);
        }

        [Fact]
        public void Resolve_UnregisteredTypeNotInProvider_ReturnsNull()
        {
            var provider = new Mock<IServiceProvider>();
            provider.Setup(p => p.GetService(It.IsAny<Type>())).Returns(null);

            var container = new PluginContainer(provider.Object);

            var resolved = container.Resolve<IDependency>();
            Assert.Null(resolved);
        }

        #region Test Doubles

        public interface IDependency { }
        public class ConcreteDependency : IDependency { }

        public interface ITestService { }
        public class TestService : ITestService
        {
            public TestService(IDependency dependency) { }
        }

        #endregion
    }
}
