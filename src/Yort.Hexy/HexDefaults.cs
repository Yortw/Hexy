using System;
using System.Threading;

namespace Yort.Hexy
{
    /// <summary>
    /// Provides application-wide default settings for <see cref="Hex"/> formatting.
    /// </summary>
    /// <remarks>
    /// <para>This class is intended to be configured once at application startup (e.g., in
    /// <c>Program.cs</c> or <c>Startup.cs</c>) and not modified during request processing.
    /// Call <see cref="Lock"/> after configuration is complete to enforce this.</para>
    /// <para>All property reads are thread-safe. Property writes are thread-safe but are
    /// intended only for startup configuration.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // In Program.cs:
    /// HexDefaults.Format = HexFormat.UppercaseColons;
    /// HexDefaults.Lock();
    ///
    /// // Later in application code:
    /// var hex = Hex.Parse("DEADBEEF");
    /// Console.WriteLine(hex); // "DE:AD:BE:EF"
    /// </code>
    /// </example>
    public static class HexDefaults
    {
        private static volatile HexFormat s_format = HexFormat.Lowercase;
        private static int s_locked; // 0 = unlocked, 1 = locked; accessed via Interlocked/Volatile

        /// <summary>
        /// Gets or sets the default <see cref="HexFormat"/> used by <see cref="Hex.ToString()"/>
        /// when no explicit format is specified.
        /// </summary>
        /// <value>
        /// The default format. Initially <see cref="HexFormat.Lowercase"/>.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// The setter is called after <see cref="Lock"/> has been called.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>Reads are always thread-safe via volatile access. Writes are thread-safe
        /// but should only occur during application startup before <see cref="Lock"/> is called.</para>
        /// </remarks>
        /// <seealso cref="Lock"/>
        /// <seealso cref="IsLocked"/>
        public static HexFormat Format
        {
            get => s_format;
            set
            {
#if NET6_0_OR_GREATER
                ArgumentNullException.ThrowIfNull(value);
#else
                if (value == null) throw new ArgumentNullException(nameof(value));
#endif
                ThrowIfLocked();
                s_format = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether defaults have been locked and can no longer be changed.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if <see cref="Lock"/> has been called; otherwise, <see langword="false"/>.
        /// </value>
        /// <seealso cref="Lock"/>
        public static bool IsLocked => Volatile.Read(ref s_locked) == 1;

        /// <summary>
        /// Prevents any further changes to default settings. This operation is idempotent;
        /// calling it multiple times has no additional effect.
        /// </summary>
        /// <remarks>
        /// <para>After this method is called, any attempt to set <see cref="Format"/>
        /// will throw <see cref="InvalidOperationException"/>.</para>
        /// <para>This method is thread-safe and uses an atomic compare-and-swap internally.</para>
        /// </remarks>
        /// <seealso cref="IsLocked"/>
        public static void Lock()
        {
            Interlocked.Exchange(ref s_locked, 1);
        }

        /// <summary>
        /// Resets all defaults to their initial values and unlocks the configuration.
        /// Intended for unit testing only.
        /// </summary>
        internal static void Reset()
        {
            // s_format is volatile, so this assignment has release semantics —
            // guaranteed visible before the Interlocked unlock that follows.
            s_format = HexFormat.Lowercase;
            Interlocked.Exchange(ref s_locked, 0);
        }

        private static void ThrowIfLocked()
        {
            if (IsLocked)
                throw new InvalidOperationException(
                    "HexDefaults have been locked and cannot be modified. " +
                    "Configure defaults at application startup before calling HexDefaults.Lock().");
        }
    }
}
