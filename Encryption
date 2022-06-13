using System;
using System.Text;

namespace Encryption
{
    class Program
    {
        static void Main(string[] args)
        {
            string str = "This is a Base64 test.";

            byte[] byteStr = Encoding.UTF8.GetBytes(str);
            string enCodeStr = Convert.ToBase64String(byteStr);

            Console.WriteLine(enCodeStr);

            byte[] deCodeStr = Convert.FromBase64String(enCodeStr);

            Console.WriteLine(Encoding.Default.GetString(deCodeStr));
        }
    }
}
