using System;
using Lueben.Microservice.Localization.Constants;

namespace Lueben.Microservice.Localization.Exceptions
{
    public class LocaleCodeHeaderNotFoundException : Exception
    {
        public LocaleCodeHeaderNotFoundException()
            : base(ErrorMessages.LocaleCodeHeaderNotFound)
        {
        }
    }
}
