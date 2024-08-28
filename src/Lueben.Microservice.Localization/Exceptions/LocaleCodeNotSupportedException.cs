using System;
using Lueben.Microservice.Localization.Constants;

namespace Lueben.Microservice.Localization.Exceptions
{
    public class LocaleCodeNotSupportedException : Exception
    {
        public LocaleCodeNotSupportedException(string msg)
            : base(string.Format(ErrorMessages.LocaleCodeNotSupported, msg))
        {
        }
    }
}
