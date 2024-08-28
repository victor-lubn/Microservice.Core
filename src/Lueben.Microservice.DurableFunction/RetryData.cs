namespace Lueben.Microservice.DurableFunction
{
    public class RetryData<T>
    {
        public int Retry { get; set; }

        public T Input { get; set; }

        public RetryData(T input)
        {
            Input = input;
        }
    }
}