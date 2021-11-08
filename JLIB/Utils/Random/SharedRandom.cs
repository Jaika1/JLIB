using System;
using System.Collections.Generic;
using System.Text;

namespace JLIB.Utils.Random
{
    /// <summary>
    /// Contains a thread-safe way of obtaining an instance of <c>System.Random</c>, great for when you want a global random where new instantiation is unnessecary or messy.
    /// </summary>
    /// <remarks>If targeting .NET 6.0 or greater, consider using <c>System.Random.Shared</c> instead.</remarks>
#if NET6_0_OR_GREATER
    [Obsolete("This class is no longer required as of .NET 6.0 due to the introduction of the System.Random.Shared property.")]
#endif
    public static class SharedRandom
    {
        private static ThreadLocal<System.Random> _random = new ThreadLocal<System.Random>();

        /// <summary>
        /// Provides a thread-safe instance of <c>System.Random</c>.
        /// </summary>
        public static System.Random Instance => _random.Value;
    }
}
