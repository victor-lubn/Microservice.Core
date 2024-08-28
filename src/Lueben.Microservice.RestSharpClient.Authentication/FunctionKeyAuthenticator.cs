using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;

namespace Lueben.Microservice.RestSharpClient.Authentication
{
    public class FunctionKeyAuthenticator : AuthenticatorBase
    {
        public const string FunctionsKeyHeaderName = "x-functions-key";

        public FunctionKeyAuthenticator(string functionKey) : base(functionKey)
        {
        }

        protected override ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
        {
            return new ValueTask<Parameter>(new HeaderParameter(FunctionsKeyHeaderName, accessToken));
        }
    }
}