using System.Net;
using System.Text;
using Beanstalk.Client;

namespace Beanstalk.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var connection = new Connection(IPAddress.Parse("192.168.1.136"), 11300);
            connection.Put(Encoding.ASCII.GetBytes("Hello \0from C#"));
        }
    }
}