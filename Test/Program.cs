using System.Net;
using Beanstalk.Client;

namespace Beanstalk.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var beanstalk = new BeanstalkConnection(IPAddress.Loopback, 11300))
            {
                beanstalk.Put("hello");                
            }
        }
    }
}