namespace InSync
{
    /// <summary>
    /// 
    /// </summary>
    public enum TimingMethod
    {
        /// <summary>
        /// Specifies to use <seealso cref="System.Diagnostics.Stopwatch"/> to count time. It usually has the best resolution without overflow. However, it does not work in some multi-processor systems. The resolution may be the same as <seealso cref="EnvironmentTick"/> if a system does not support high precision timer. 
        /// </summary>
        Stopwatch = 0,

        /// <summary>
        /// Specifies to use <seealso cref="System.Environment.TickCount"/> to count time. The order of magnitude of the resolution may be 10ms. It overflows every 49.8 days.
        /// </summary>
        EnvironmentTick = 1,

        /// <summary>
        /// Specifies to use <seealso cref="System.DateTime.UtcNow"/> to count time. The order of magnitude of the resolution may be from 1ms to 10ms. A clock change, such as a network time synchronization, messes up the counting.
        /// </summary>
        DateTime = 2,
    }
}