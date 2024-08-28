namespace Lueben.Microservice.Api.ValidationFunction.Exceptions
{
    public class JsonBodyNotValidException : ModelNotValidException
    {
        public JsonBodyNotValidException(string field, string errorMessage) : base(field, errorMessage)
        {
        }
    }
}
