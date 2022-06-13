using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SPUtil
{
    public class SPUtil
    {
        public SPUtil()
        {

        }

        /// <summary>
        /// 입력파일 읽기
        /// </summary>
        /// <param name="fileName"></param>
        public static void ReadFile(string fileName)
        {
            String line;

            using (StreamReader sr = new StreamReader(fileName))
            {
                while (sr.Peek() > -1)
                {
                    line = sr.ReadLine();

                    DoLineJob(line);
                }

            }

        }

        /// <summary>
        /// 파일 출력
        /// </summary>
        /// <param name="fileName"></param>
        public static void WriteFile(string fileName, string text, bool append)
        {
            using (StreamWriter outputFile = new StreamWriter(fileName, append))
            {
                outputFile.Write(text); ;
            }
        }

        /// <summary>
        /// 외부 프로그램 실행 기술
        /// </summary>
        /// <param name="messageCode"></param>
        /// <returns></returns>
        public static string executeExternalProcess(string fileName, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = fileName;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            start.Arguments = args;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 서버 소켓 시작
        /// </summary>
        public static void startServerSocket(int portNumber)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, portNumber);
            TcpListener server = new TcpListener(ep);

            server.Start();

            Console.WriteLine("The server socket is started at {0}", ep.ToString());

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                Console.WriteLine("A new client is connected from {0}", ep.ToString());

                ParameterizedThreadStart sTh = new ParameterizedThreadStart(ThreadFunc);

                Thread th = new Thread(sTh);

                th.Start(client);
            }
        }



        /// <summary>
        /// 접속된 클라이언트별 전용 쓰레드 작업
        /// </summary>
        /// <param name="client"></param>
        static void ThreadFunc(object client)
        {
            TcpClient c = (TcpClient)client;
            NetworkStream st = c.GetStream();

            byte[] buffer;

            // 1회 전송 데이터의 최대 길이
            int bufferSize = 1024;

            // 1회 데이터를 읽은 길이
            int nRead = 0;
            string line;
            while (c.Connected)
            {
                buffer = new byte[bufferSize];

                try
                {
                    nRead = st.Read(buffer, 0, bufferSize);
                    if(nRead > 0)
                    {
                        line = Encoding.Default.GetString(buffer, 0, nRead);
                        Console.WriteLine("Received Data : {0} from {1}", line, c.Client.RemoteEndPoint);
                        DoLineJob(line);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    Thread.Sleep(1);
                }
            }
        }
 
        static void DoLineJob(string line)
        {
            string[] items = line.Split((",").ToCharArray());
        }
    }
}