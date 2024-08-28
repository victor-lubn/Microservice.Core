using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Lueben.Microservice.Diagnostics
{
    [DebuggerStepThrough]
    public static class Ensure
    {
        [ContractAnnotation("argumentValue:null => halt")]
        public static T ArgumentNotNull<T>(T argumentValue, [InvokerParameterName] string argumentName)
            where T : class
        {
            return argumentValue ?? throw new ArgumentNullException(argumentName);
        }

        [ContractAnnotation("argumentValue:null => halt")]
        public static T ArgumentNotNull<T>(T argumentValue, [InvokerParameterName] string argumentName, string message)
            where T : class
        {
            return argumentValue ?? throw new ArgumentNullException(argumentName, message);
        }

        public static string ArgumentNotNullOrEmpty(string argumentValue, [InvokerParameterName] string argumentName)
        {
            return ArgumentNotNullOrEmpty(argumentValue, argumentName, "Value should be not null and not empty");
        }

        public static string ArgumentNotNullOrEmpty(string argumentValue, [InvokerParameterName] string argumentName, string message)
        {
            ArgumentCondition(!string.IsNullOrEmpty(argumentValue), argumentName, message);
            return argumentValue;
        }

        [ContractAnnotation("conditionValue:false => halt")]
        public static void ArgumentCondition(bool conditionValue, [InvokerParameterName] string argumentName)
        {
            ArgumentCondition(conditionValue, argumentName, "Value should be equal to true");
        }

        [ContractAnnotation("conditionValue:false => halt")]
        public static void ArgumentCondition(bool conditionValue, [InvokerParameterName] string argumentName, string message)
        {
            if (conditionValue == false)
            {
                throw new ArgumentException(message, argumentName);
            }
        }
    }
}