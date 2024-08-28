using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Lueben.Microservice.Diagnostics;
using Lueben.Microservice.Json.Encrypt.Exceptions;
using Newtonsoft.Json;

namespace Lueben.Microservice.Json.Encrypt
{
    public class EncryptingJsonConverter : JsonConverter
    {
        private readonly string secretKey;

        public EncryptingJsonConverter(string secretKey)
        {
            Ensure.ArgumentNotNullOrEmpty(secretKey, nameof(secretKey));

            this.secretKey = secretKey;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            try
            {
                var stringValue = JsonConvert.SerializeObject(value, new JsonSerializerSettings());
                if (string.IsNullOrEmpty(stringValue))
                {
                    writer.WriteNull();
                    return;
                }

                using var aes = Aes.Create();
                aes.GenerateIV();
                aes.Key = Encoding.UTF8.GetBytes(this.secretKey);

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using var outputStream = new MemoryStream();
                using (var cs = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
                {
                    using var sw = new StreamWriter(cs);
                    sw.Write(stringValue);
                }

                var encryptedValue = outputStream.ToArray();
                var resultEncryptedValue = encryptedValue.Concat(aes.IV).ToArray();
                writer.WriteValue(Convert.ToBase64String(resultEncryptedValue));
            }
            catch (CryptographicException ex)
            {
                throw new EncryptionTokenInvalidException(ex.Message);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value as string;
            if (string.IsNullOrEmpty(value))
            {
                return reader.Value;
            }

            try
            {
                var buffer = Convert.FromBase64String(value);

                var bytesIv = new byte[16];
                if (buffer.Length <= bytesIv.Length)
                {
                    throw new EncryptionTokenInvalidException($"Wrong encrypted value: {value}");
                }

                var encryptedBuffer = new byte[buffer.Length - bytesIv.Length];

                Array.Copy(buffer, buffer.Length - bytesIv.Length, bytesIv, 0, bytesIv.Length);

                Array.Copy(buffer, 0, encryptedBuffer, 0, buffer.Length - bytesIv.Length);

                using var aes = Aes.Create();
                aes.IV = bytesIv;
                aes.Key = Encoding.UTF8.GetBytes(this.secretKey);

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using MemoryStream ms = new MemoryStream(encryptedBuffer);
                using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using StreamReader sr = new StreamReader(cs);
                var decryptedValue = sr.ReadToEnd();

                return JsonConvert.DeserializeObject(decryptedValue, objectType, new JsonSerializerSettings());
            }
            catch (CryptographicException ex)
            {
                throw new EncryptionTokenInvalidException(ex.Message);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
