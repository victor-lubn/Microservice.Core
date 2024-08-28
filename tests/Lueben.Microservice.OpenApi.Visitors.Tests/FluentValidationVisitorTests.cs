using System.Reflection;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Lueben.Microservice.OpenApi.Visitors.Tests.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using Microsoft.OpenApi.Models;
using Moq;
using Newtonsoft.Json.Serialization;

namespace Lueben.Microservice.OpenApi.Visitors.Tests
{
    public class FluentValidationVisitorTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IValidatorDescriptor> _validatorDescriptor;
        private readonly FluentValidationVisitor _visitor;

        public FluentValidationVisitorTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();

            _validatorDescriptor = new Mock<IValidatorDescriptor>();

            var validator = new Mock<IValidator>();
            validator.Setup(x => x.CreateDescriptor()).Returns(_validatorDescriptor.Object);

            _serviceProviderMock.Setup(x => x.GetService(It.IsAny<Type>())).Returns(validator.Object);

            _visitor = new FluentValidationVisitor(new VisitorCollection(new List<IVisitor>()), _serviceProviderMock.Object);
        }

        [Fact]
        public void GivenFluentValidationVisitor_WhenIsVisitableCalled_ThenTrueIsReturned()
        {
            var result = _visitor.IsVisitable(typeof(TestClass));

            Assert.True(result);
        }

        [Fact]
        public void GivenFluentValidationVisitor_WhenVisitCalledAndNoPropertiesInAcceptor_ThenAcceptorSchemaIsNotUpdated()
        {
            var key = "key";
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);
            acceptor.Properties = new Dictionary<string, PropertyInfo>();

            _visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());
        }

        [Fact]
        public void GivenFluentValidationVisitor_WhenVisitCalledAndNoSchemasInAcceptor_ThenAcceptorSchemaIsNotUpdated()
        {
            var key = "key";
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();

            _visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());
        }

        [Fact]
        public void GivenFluentValidationVisitor_WhenVisitCalledAndNoExpectedAcceptor_ThenAcceptorSchemaIsNotUpdated()
        {
            var key = "key";
            var acceptor = new TestAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);

            _visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());
        }

        [Fact]
        public void GivenFluentValidationVisitor_WhenVisitCalledAndNoAppropriateValidator_ThenAcceptorSchemaIsNotUpdated()
        {
            var key = "key";
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.Created));
            acceptor.Properties.Add(key, propertyInfo);

            _visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.Empty(schema.Required);
            Assert.Null(schema.Maximum);
            Assert.Null(schema.Minimum);
            Assert.Null(schema.ExclusiveMinimum);
            Assert.Null(schema.MaxLength);
            Assert.Null(schema.MinLength);
            Assert.Null(schema.Format);
            Assert.Null(schema.Example);
            Assert.Null(schema.Pattern);
        }

        [Fact]
        public void GivenFluentValidationVisitor_WhenVisitCalledAndNotEmptyValidator_ThenKeyOfTypeIsAddedToRequired()
        {
            var key = nameof(TestClass.StringProperty);
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StringProperty));
            acceptor.Properties.Add(key, propertyInfo);

            
            var propertyValidator = new Mock<INotEmptyValidator>();
            var options = new Mock<IRuleComponent>();
            (IPropertyValidator Validator, IRuleComponent Options) group = (propertyValidator.Object, options.Object);
            _validatorDescriptor.Setup(x => x.GetValidatorsForMember(nameof(TestClass.StringProperty)))
                .Returns(new List<(IPropertyValidator Validator, IRuleComponent Options)> {group});

            _visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.NotEmpty(schema.Required);
            Assert.Contains(key, schema.Required);
            Assert.Null(schema.Maximum);
            Assert.Null(schema.Minimum);
            Assert.Null(schema.ExclusiveMinimum);
            Assert.Null(schema.MaxLength);
            Assert.Null(schema.MinLength);
            Assert.Null(schema.Format);
            Assert.Null(schema.Example);
            Assert.Null(schema.Pattern);
        }

        [Theory]
        [InlineData(0,0)]
        [InlineData(0,5)]
        [InlineData(5,0)]
        [InlineData(5,10)]
        public void GivenFluentValidationVisitor_WhenVisitCalledAndLengthValidator_ThenMinMaxLengthAreSet(int min, int max)
        {
            var key = nameof(TestClass.StringProperty);
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StringProperty));
            acceptor.Properties.Add(key, propertyInfo);


            var propertyValidator = new Mock<ILengthValidator>();
            propertyValidator.Setup(x => x.Min).Returns(min);
            propertyValidator.Setup(x => x.Max).Returns(max);
            var options = new Mock<IRuleComponent>();
            (IPropertyValidator Validator, IRuleComponent Options) group = (propertyValidator.Object, options.Object);
            _validatorDescriptor.Setup(x => x.GetValidatorsForMember(nameof(TestClass.StringProperty)))
                .Returns(new List<(IPropertyValidator Validator, IRuleComponent Options)> { group });

            _visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            var expectedMin = min == 0 ? (int?)null : min;
            var expectedMax = max == 0 ? (int?)null : max;
            Assert.Null(schema.Maximum);
            Assert.Null(schema.Minimum);
            Assert.Null(schema.ExclusiveMinimum);
            Assert.Equal(expectedMax, schema.MaxLength);
            Assert.Equal(expectedMin, schema.MinLength);
            Assert.Empty(schema.Required);
            Assert.Null(schema.Format);
            Assert.Null(schema.Example);
            Assert.Null(schema.Pattern);
        }

        [Theory]
        [InlineData(2, 3, 9.99)]
        [InlineData(0, 2, 99)]
        [InlineData(0, 0, 0)]
        public void GivenFluentValidationVisitor_WhenVisitCalledAndScalePrecisionValidator_ThenMaximumIsSet(int scale, int precision, decimal expected)
        {
            var key = nameof(TestClass.StringProperty);
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StringProperty));
            acceptor.Properties.Add(key, propertyInfo);


            var propertyValidator = new ScalePrecisionValidator<decimal>(scale, precision);
            var options = new Mock<IRuleComponent>();
            (IPropertyValidator Validator, IRuleComponent Options) group = (propertyValidator, options.Object);
            _validatorDescriptor.Setup(x => x.GetValidatorsForMember(nameof(TestClass.StringProperty)))
                .Returns(new List<(IPropertyValidator Validator, IRuleComponent Options)> { group });

            _visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.Equal(expected, schema.Maximum);
            Assert.Null(schema.Minimum);
            Assert.Null(schema.ExclusiveMinimum);
            Assert.Null(schema.MaxLength);
            Assert.Null(schema.MinLength);
            Assert.Empty(schema.Required);
            Assert.Null(schema.Format);
            Assert.Null(schema.Example);
            Assert.Null(schema.Pattern);
        }

        [Fact]
        public void GivenFluentValidationVisitor_WhenVisitCalledAndEmailValidator_ThenFormatAndExampleAreSet()
        {
            var key = nameof(TestClass.StringProperty);
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StringProperty));
            acceptor.Properties.Add(key, propertyInfo);


            var propertyValidator = new Mock<AspNetCoreCompatibleEmailValidator<string>>();
            var options = new Mock<IRuleComponent>();
            (IPropertyValidator Validator, IRuleComponent Options) group = (propertyValidator.Object, options.Object);
            _validatorDescriptor.Setup(x => x.GetValidatorsForMember(nameof(TestClass.StringProperty)))
                .Returns(new List<(IPropertyValidator Validator, IRuleComponent Options)> { group });

            _visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.NotNull(schema.Example);
            Assert.Equal("email", schema.Format);
            Assert.Null(schema.Maximum);
            Assert.Null(schema.Minimum);
            Assert.Null(schema.ExclusiveMinimum);
            Assert.Empty(schema.Required);
            Assert.Null(schema.MaxLength);
            Assert.Null(schema.MinLength);
            Assert.Null(schema.Pattern);
        }

        [Fact]
        public void GivenFluentValidationVisitor_WhenVisitCalledAndGreaterThanOrEqualComparisonValidator_ThenMinimumIsSet()
        {
            var key = nameof(TestClass.StringProperty);
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StringProperty));
            acceptor.Properties.Add(key, propertyInfo);


            var propertyValidator = new Mock<IComparisonValidator>();
            propertyValidator.Setup(x => x.Comparison).Returns(Comparison.GreaterThanOrEqual);
            var options = new Mock<IRuleComponent>();
            (IPropertyValidator Validator, IRuleComponent Options) group = (propertyValidator.Object, options.Object);
            _validatorDescriptor.Setup(x => x.GetValidatorsForMember(nameof(TestClass.StringProperty)))
                .Returns(new List<(IPropertyValidator Validator, IRuleComponent Options)> { group });

            _visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());


            Assert.NotNull(schema.Minimum);
            Assert.True(schema.ExclusiveMinimum);
            Assert.Null(schema.Example);
            Assert.Null(schema.Format);
            Assert.Null(schema.Maximum);
            Assert.Empty(schema.Required);
            Assert.Null(schema.MaxLength);
            Assert.Null(schema.MinLength);
            Assert.Null(schema.Pattern);
        }

        [Fact]
        public void GivenFluentValidationVisitor_WhenVisitCalledAndNotGreaterThanOrEqualComparisonValidator_ThenMinimumIsNotSet()
        {
            var key = nameof(TestClass.StringProperty);
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StringProperty));
            acceptor.Properties.Add(key, propertyInfo);


            var propertyValidator = new Mock<IComparisonValidator>();
            propertyValidator.Setup(x => x.Comparison).Returns(Comparison.LessThan);
            var options = new Mock<IRuleComponent>();
            (IPropertyValidator Validator, IRuleComponent Options) group = (propertyValidator.Object, options.Object);
            _validatorDescriptor.Setup(x => x.GetValidatorsForMember(nameof(TestClass.StringProperty)))
                .Returns(new List<(IPropertyValidator Validator, IRuleComponent Options)> { group });

            _visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());


            Assert.Null(schema.Minimum);
            Assert.Null(schema.ExclusiveMinimum);
            Assert.Null(schema.Example);
            Assert.Null(schema.Format);
            Assert.Null(schema.Maximum);
            Assert.Empty(schema.Required);
            Assert.Null(schema.MaxLength);
            Assert.Null(schema.MinLength);
            Assert.Null(schema.Pattern);
        }

        [Fact]
        public void GivenFluentValidationVisitor_WhenVisitCalledAndRegularExpressionValidator_ThenPatternIsSet()
        {
            var key = nameof(TestClass.StringProperty);
            var acceptor = new OpenApiSchemaAcceptor();

            var schema = new OpenApiSchema();
            acceptor.Schemas.Add(key, schema);
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StringProperty));
            acceptor.Properties.Add(key, propertyInfo);

            var expression = "test expression";
            var propertyValidator = new Mock<IRegularExpressionValidator>();
            propertyValidator.Setup(x => x.Expression).Returns(expression);
            var options = new Mock<IRuleComponent>();
            (IPropertyValidator Validator, IRuleComponent Options) group = (propertyValidator.Object, options.Object);
            _validatorDescriptor.Setup(x => x.GetValidatorsForMember(nameof(TestClass.StringProperty)))
                .Returns(new List<(IPropertyValidator Validator, IRuleComponent Options)> { group });

            _visitor.Visit(acceptor, new KeyValuePair<string, Type>(key, typeof(TestClass)), new CamelCaseNamingStrategy());

            Assert.Equal(expression, schema.Pattern);
            Assert.Null(schema.Minimum);
            Assert.Null(schema.ExclusiveMinimum);
            Assert.Null(schema.Example);
            Assert.Null(schema.Format);
            Assert.Null(schema.Maximum);
            Assert.Empty(schema.Required);
            Assert.Null(schema.MaxLength);
            Assert.Null(schema.MinLength);
        }
    }
}