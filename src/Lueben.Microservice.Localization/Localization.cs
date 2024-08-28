using System.Globalization;
using System.Threading;

namespace Lueben.Microservice.Localization
{
    public class Localization
    {
        private static AsyncLocal<CultureInfo> asyncLocal = new AsyncLocal<CultureInfo>();

        public static CultureInfo Current
        {
            get => asyncLocal.Value;
            set => asyncLocal.Value = value;
        }
    }
}
