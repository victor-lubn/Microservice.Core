using System.IO;
using System.Threading.Tasks;

namespace Lueben.Microservice.Api.Idempotency.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ReadFully(this Stream input)
        {
            using var ms = new MemoryStream();
            input.CopyTo(ms);
            input.Position = 0;

            return ms.ToArray();
        }

        public static async Task<byte[]> ReadFullyAsync(this Stream input)
        {
            var body = new byte[input.Length];
            _ = await input.ReadAsync(body, 0, (int)input.Length);
            input.Position = 0;

            return body;
        }
    }
}
