using System.IO;
using System.Text;

namespace Beanstalk.Client
{
    public static class StreamExtensions
    {
        public static void Write(this Stream stream, string str)
        {
            stream.Write(str, Encoding.ASCII);
        }

        public static void Write(this Stream stream, string str, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(str);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}