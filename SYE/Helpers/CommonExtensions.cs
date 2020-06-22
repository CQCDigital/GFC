using System;
using Microsoft.AspNetCore.Hosting;

namespace SYE.Helpers
{
    public static class CommonExtensions
    {
        public static bool IsLocal(this IHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }
            return hostingEnvironment.IsEnvironment("Local");
        }
    }
}
