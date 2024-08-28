using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Lueben.Microservice.Localization.Configurations;
using Lueben.Microservice.Localization.Constants;
using Lueben.Microservice.Localization.Exceptions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Lueben.Microservice.Localization.Middleware
{
    public class LocalizationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IEnumerable<string> _allowedLocaleCodes;

        public LocalizationMiddleware()
        {
            _allowedLocaleCodes = LocalizationOptions.Value.AllowedLocales
                .Split(CommonConstants.CommaSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim());
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var httpContext = context.GetHttpContext();
            if (httpContext != null)
            {
                if (httpContext.Request.Headers.TryGetValue(CommonConstants.LocaleCodeHeaderKey,
                        out var localeCodeValue) && !string.IsNullOrEmpty(localeCodeValue.FirstOrDefault()))
                {
                    var cultureInfoName = localeCodeValue.ToString();
                    if (!string.IsNullOrEmpty(cultureInfoName) && !_allowedLocaleCodes.Contains(cultureInfoName, StringComparer.InvariantCultureIgnoreCase))
                    {
                        throw new LocaleCodeNotSupportedException(cultureInfoName);
                    }

                    Localization.Current = new CultureInfo(cultureInfoName);
                }
                else if (!string.IsNullOrEmpty(LocalizationOptions.Value.DefaultLocale))
                {
                    Localization.Current = new CultureInfo(LocalizationOptions.Value.DefaultLocale);
                }
                else
                {
                    throw new LocaleCodeHeaderNotFoundException();
                }
            }

            await next(context);
        }
    }
}
