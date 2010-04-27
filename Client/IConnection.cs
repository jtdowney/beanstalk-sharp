using System;

namespace Beanstalk.Client
{
    public interface IConnection
    {
        /// <summary>
        /// Closes this instance.
        /// </summary>
        void Close();

        /// <summary>
        /// Puts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The newly created job.</returns>
        IJob Put(byte[] data);

        /// <summary>
        /// Puts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="timeToRun">The time to run.</param>
        /// <returns>The newly created job.</returns>
        IJob Put(byte[] data, int priority, TimeSpan delay, TimeSpan timeToRun);
    }
}