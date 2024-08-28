namespace Lueben.Microservice.EntityFunction.Models
{
    public interface IEntityOperation<T>
    {
        T Id { get; set; }
    }
}