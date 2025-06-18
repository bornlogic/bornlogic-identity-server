



using System.Diagnostics;

#pragma warning disable 1591

namespace Bornlogic.IdentityServer.Storage.Extensions
{
    internal static class IEnumerableExtensions
    {
        [DebuggerStepThrough]
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            if (list == null)
            {
                return true;
            }

            if (!list.Any())
            {
                return true;
            }

            return false;
        }
    }
}