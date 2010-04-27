using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Beanstalk.Client
{
    public class BeanstalkConnection : IBeanstalkConnection
    {
        private const int DefaultPriority = 65536;
        private static readonly TimeSpan DefaultDelay = TimeSpan.Zero;
        private static readonly TimeSpan DefaultTimeToRun = TimeSpan.FromMinutes(2);

        private readonly IPEndPoint endpoint;
        private TcpClient client;
        private Stream stream;
        private BufferedStream writer;
        private StreamReader reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanstalkConnection"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public BeanstalkConnection(IPAddress address, int port)
            : this(new IPEndPoint(address, port))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanstalkConnection"/> class.
        /// </summary>
        /// <param name="endpoint">The end point.</param>
        public BeanstalkConnection(IPEndPoint endpoint)
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
            this.stream = this.client.GetStream();
            this.writer = new BufferedStream(stream);
            this.reader = new StreamReader(stream);

        }

        #region Implementation of IBeanstalkConnection

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            this.client.Client.Disconnect(true);
            this.client.Close();
        }

        /// <summary>
        /// Uses the specified tube.
        /// </summary>
        /// <param name="tube">The tube.</param>
        public void Use(string tube)
        {
            this.Connect();

            string command = string.Format("use {0}\r\n", tube);
            this.writer.Write(command);
            this.writer.Flush();

            string response = this.reader.ReadLine();
            if (response != string.Format("USING {0}", tube))
            {
                throw new InvalidOperationException(response);
            }
        }

        /// <summary>
        /// Puts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The newly created job.</returns>
        public IJob Put(byte[] data)
        {
            return this.Put(data, DefaultPriority, DefaultDelay, DefaultTimeToRun);
        }

        /// <summary>
        /// Puts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The newly created job.</returns>
        public IJob Put(string data)
        {
            return this.Put(data, DefaultPriority, DefaultDelay, DefaultTimeToRun);
        }

        /// <summary>
        /// Puts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="timeToRun">The time to run.</param>
        /// <returns>The newly created job.</returns>
        public IJob Put(string data, int priority, TimeSpan delay, TimeSpan timeToRun)
        {
            return this.Put(Encoding.UTF8.GetBytes(data), priority, delay, timeToRun);
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

            string command = string.Format("put {0} {1} {2} {3}\r\n", priority, delay.TotalSeconds, timeToRun.TotalSeconds,
                                           data.Length);
            this.writer.Write(command);
            this.writer.Write(data, 0, data.Length);
            this.writer.Write("\r\n");
            this.writer.Flush();

            string response = this.reader.ReadLine();

            var regex = new Regex(@"(?:INSERTED|BURIED)\s(\d+)", RegexOptions.Compiled);
            Match match = regex.Match(response);
            if (match.Success)
            {
                //TODO: Return job object
                return null;
            }

            switch (response)
            {
                case "EXPECTED_CRLF":
                    throw new InvalidDataException(
                        "EXPECTED_CRLF: The job body must be followed by a CR-LF pair, that is,\"\r\n\". These two bytes are not counted in the job size given by the client in the put command line.");
                case "JOB_TOO_BIG":
                    throw new InvalidOperationException(
                        "JOB_TOO_BIG: The client has requested to put a job with a body larger than max-job-size bytes.");
                default:
                    throw new InvalidOperationException(response);
            }
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            this.Close();
        }

        #endregion
    }
}