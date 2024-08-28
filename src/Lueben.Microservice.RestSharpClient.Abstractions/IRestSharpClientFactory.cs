namespace Lueben.Microservice.RestSharpClient.Abstractions
{
    public interface IRestSharpClientFactory
    {
        IRestSharpClient Create(object client);

        IRestSharpClient Create(string sectionName);
    }
}