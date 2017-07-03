using System.Collections.Generic;

namespace Vanderstack.Api.Core.Infrastructure.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Wraps this object instance into an IEnumerable consisting of the item.
        /// </summary>
        /// <typeparam name="T"> Type of the wrapped object.</typeparam>
        /// <param name="item"> The object to wrap.</param>
        /// <returns>An IEnumerable consisting of the wrapped item.</returns>
        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            if (item != null)
                yield return item;
        }
    }
}
