using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Beanstalk.Client
{
    public class Connection : IConnection
    {
        private readonly IPEndPoint endpoint;
        private TcpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public Connection(IPAddress address, int port)
            : this(new IPEndPoint(address, port))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="endpoint">The end point.</param>
        public Connection(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        private void Connect()
        {
            if (this.client != null && this.client.Connected)
            {
                return;
            }

            this.client = new TcpClient();
            this.client.Connect(this.endpoint);
        }

        #region Implementation of IConnection

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            this.client.Client.Disconnect(true);
        }

        /// <summary>
        /// Puts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The newly created job.</returns>
        public IJob Put(byte[] data)
        {
            return this.Put(data, 65536, TimeSpan.Zero, TimeSpan.FromMinutes(2));
        }

        /// <summary>
        /// Puts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="timeToRun">The time to run.</param>
        /// <returns>The newly created job.</returns>
        public IJob Put(byte[] data, int priority, TimeSpan delay, TimeSpan timeToRun)
        {
            this.Connect();

            var stream = this.client.GetStream();
            var writer = new StreamWriter(stream, Encoding.ASCII);

            string msg = Encoding.ASCII.GetString(data);

            writer.WriteLine("put {0} {1} {2} {3}", priority, delay.TotalSeconds, timeToRun.TotalSeconds,
                             data.Length);
            writer.WriteLine(msg);
            writer.Flush();

            var reader = new StreamReader(stream);
            string response = reader.ReadLine();

            writer.WriteLine("quit");
            writer.Flush();

            this.Close();

            return null;
        }

        #endregion
    }
}