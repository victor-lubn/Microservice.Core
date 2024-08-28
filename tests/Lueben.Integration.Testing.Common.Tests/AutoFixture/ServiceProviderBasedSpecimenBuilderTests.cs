using System;
using AutoFixture.Kernel;
using Lueben.Integration.Testing.Common.AutoFixture;
using Moq;
using Xunit;

namespace Lueben.Integration.Testing.Common.Tests.AutoFixture
{
    public class ServiceProviderBasedSpecimenBuilderTests
    {
        private Mock<IServiceProvider> _spMock;
        private Mock<ISpecimenContext> _specimenContextMock;
        private ServiceProviderBasedSpecimenBuilder _builder;

        public ServiceProviderBasedSpecimenBuilderTests()
        {
            _spMock = new Mock<IServiceProvider>();
            _specimenContextMock = new Mock<ISpecimenContext>();

            _builder = new ServiceProviderBasedSpecimenBuilder(_spMock.Object);
        }

        [Fact]
        public void GivenSpecimenBuilder_WhenRequestIsNotType_ThenShouldReturnNoSpecimen()
        {
            var result = _builder.Create(new object(), _specimenContextMock.Object);
            Assert.NotNull(result);
            Assert.IsType<NoSpecimen>(result);

            _spMock.Verify(x => x.GetService(It.IsAny<Type>()), Times.Never);
            _specimenContextMock.Verify(x => x.Resolve(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public void GivenSpecimenBuilder_WhenServiceProvierReturnsOmitSpecimen_ThenShouldReturnNoSpecimen()
        {
            _spMock.Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(new OmitSpecimen());

            var result = _builder.Create(typeof(TestService), _specimenContextMock.Object);
            Assert.NotNull(result);
            Assert.IsType<NoSpecimen>(result);

            _spMock.Verify(x => x.GetService(It.IsAny<Type>()), Times.Once);
            _specimenContextMock.Verify(x => x.Resolve(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public void GivenSpecimenBuilder_WhenServiceProvierReturnsService_ThenThisServiceShouldBeReturned()
        {
            _spMock.Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(new TestService());

            var result = _builder.Create(typeof(TestService), _specimenContextMock.Object);
            Assert.NotNull(result);
            Assert.IsType<TestService>(result);

            _spMock.Verify(x => x.GetService(It.IsAny<Type>()), Times.Once);
            _specimenContextMock.Verify(x => x.Resolve(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public void GivenSpecimenBuilder_WhenServiceProvierReturnsNull_ThenSpecimenContextShouldResolve()
        {
            _spMock.Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(null);

            _specimenContextMock.Setup(x => x.Resolve(It.IsAny<object>()))
                .Returns(new TestService());

            var result = _builder.Create(typeof(TestService), _specimenContextMock.Object);
            Assert.NotNull(result);
            Assert.IsType<TestService>(result);

            _spMock.Verify(x => x.GetService(It.IsAny<Type>()), Times.Once);
            _specimenContextMock.Verify(x => x.Resolve(It.IsAny<object>()), Times.Once);
        }
    }
}
