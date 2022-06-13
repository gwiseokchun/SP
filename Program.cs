using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace SP_TEST
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:8080/");
            listener.Start();

            //while(true)
            //{
            //    var context = listener.GetContext();
            //    Console.WriteLine("Request : " + context.Request.Url);
            //    byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //    context.Response.OutputStream.Write(data, 0, data.Length);
            //    context.Response.StatusCode = 200;
            //    context.Response.Close();
            //}

            while (true)
            {
                var context = listener.GetContext();
                Console.WriteLine(string.Format("Request({0}) : {1}", context.Request.HttpMethod, context.Request.Url));
                DateTime dt = DateTime.Now;
                String strRes = "";
                if (context.Request.HttpMethod == "GET")
                {
                    strRes = dt.ToString();
                }
                else if (context.Request.HttpMethod == "POST")
                {
                    strRes = "(POST) " + dt.ToString();
                    System.IO.Stream body = context.Request.InputStream;
                    System.Text.Encoding encoding = context.Request.ContentEncoding;
                    System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
                    string s = reader.ReadToEnd();
                    System.IO.Directory.CreateDirectory(".\\Output");
                    string fileName = string.Format(".\\Output\\{0:D2}{1:D2}{2:D2}.json", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    StreamWriter sw = new StreamWriter(fileName);
                    sw.WriteLine(s);
                    sw.Close();
                    body.Close();
                    reader.Close();
                }
                else
                {

                }

                byte[] data = Encoding.UTF8.GetBytes(strRes);
                context.Response.OutputStream.Write(data, 0, data.Length);
                context.Response.StatusCode = 200;
                context.Response.Close();
            }
        }
    }
}
