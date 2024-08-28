using System;
using Xunit;

namespace Lueben.Microservice.Diagnostics.Tests
{
    public class EnsureTests
    {
        [Fact]
        public void ArgumentNotNull_NotNullArgument_NoException()
        {
            var userName = "test";

            Ensure.ArgumentNotNull(userName, nameof(userName));
        }

        [Fact]
        public void ArgumentNotNull_NullArgument_ArgumentNullException()
        {
            string userName = null;

            Assert.Throws<ArgumentNullException>(() => Ensure.ArgumentNotNull(userName, nameof(userName)));
        }

        [Fact]
        public void ArgumentNotNullWithMessage_NotNullObjectProperty_NoException()
        {
            var user = new { Name = "test" };

            Ensure.ArgumentNotNull(user.Name, nameof(user.Name), "User info is invalid");
        }

        [Fact]
        public void ArgumentNotNullWithMessage_NullObjectProperty_ArgumentNullException()
        {
            var user = new { Name = (string)null };

            Assert.Throws<ArgumentNullException>(() => Ensure.ArgumentNotNull(user.Name, nameof(user.Name), "User info is invalid"));
        }

        [Fact]
        public void ArgumentNotNullOrEmpty_NotNullOrEmptyArgument_NoException()
        {
            var argument = "test";

            Ensure.ArgumentNotNullOrEmpty(argument, nameof(argument));
        }

        [Fact]
        public void ArgumentNotNullOrEmpty_NullOrEmptyArgument_ArgumentException()
        {
            var argument = string.Empty;

            Assert.Throws<ArgumentException>(() => Ensure.ArgumentNotNullOrEmpty(argument, nameof(argument)));
        }

        [Fact]
        public void ArgumentCondition_StringEqualsSpecified_NoException()
        {
            var argument = "test";

            Ensure.ArgumentCondition(string.Equals(argument, "test"), nameof(argument));
        }

        [Fact]
        public void ArgumentCondition_StringNotEqualsSpecified_ArgumentException()
        {
            var argument = "not-test";

            Assert.Throws<ArgumentException>(() => Ensure.ArgumentCondition(string.Equals(argument, "test"), nameof(argument)));
        }
    }
}
