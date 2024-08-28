namespace Lueben.Microservice.Options.OptionManagers
{
    public interface IRefreshedOptionsManager
    {
        TOptions GetOptions<TOptions>()
            where TOptions : class, new();
    }
}
