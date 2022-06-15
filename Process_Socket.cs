using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;


namespace SSP
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestClient();


            //전체 파일 읽기/쓰기
            ////File.ReadAllText("INPUT.txt");
            ////File.WriteAllText("OUTPUT.txt", data);

            ////ReadFile("INFILE\\STATION.txt");
            ////Console.WriteLine(Math.Pow(3, 5));

            ////Console.WriteLine(Math.Sqrt(4));
            ///
            /// 
            /// 
            /// 
            /// 
            /// 
            ////Process proc = executeExternalProcess_standardInput("SIGNAGE", "");
            ////StreamWriter sr = proc.StandardInput;

            ////sr.WriteLine("a");

            ////sr.WriteLine("b");

            ////sr.WriteLine("c");
            ////startServerSocket(9876);


            ////int c = (int)ExecuteExternalLib("LIB\\TestLib.dll", "Test.TestLib", "Call", new object[] { 1, 2 });

            ////Console.WriteLine(c);

            ////// 병렬 실행

            ////// 입/출력 데이터 외부에서 static으로 정의

            ////Console.WriteLine("Start");

            // 쓰레드풀 설정
            ThreadPool.SetMinThreads(100, 100);
            // 병렬 실행
            Parallel.For(0, 10, new Action<int>(TestClient));

            ////Console.WriteLine("End");

            Console.ReadLine();

        }
        // File Read

        public static void ReadFile(string inFileName)
        {
            string line;
            using (StreamReader sr = new StreamReader(inFileName))
            {
                while (sr.Peek() > -1)
                {
                    line = sr.ReadLine();

                    ////if (line.Equals("PRINT"))
                    ////{

                    //// // 출력 파일에 출력
                    //// string printData = MakeFilePrintData();
                    //// string outFileName = "output.txt";
                    //// WriteFile(string.Format("OUTFILE\\{0}", outFileName), printData, false);
                    //// break;
                    ////}

                    string outFileName = "output.txt";
                    WriteFile(string.Format("OUTFILE\\{0}", outFileName), line, true);
                    //DoFileLineJob(line);
                }
            }
        }


        static string MakeFilePrintData()
        {
            string ret = string.Empty;
            return ret;
        }

        /// <summary>
        /// 파일 출력
        /// </summary>
        /// <param name="fileName"></param>
        public static void WriteFile(string fileName, string text, bool append)
        {
            // Append text to an existing file named "WriteLines.txt".
            using (StreamWriter outputFile = new StreamWriter(fileName, append))
            {
                outputFile.WriteLine(text);
            }

        }
        /// <summary>
        ///  외부프로그램 호출,  args
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string executeExternalProcess_standardOutput(string fileName, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = fileName;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            start.Arguments = args;

            string str = string.Empty;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    str = reader.ReadToEnd();
                }
            }
            return str;
        }

        public static Process executeExternalProcess_standardInput(string fileName, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = fileName;
            start.UseShellExecute = false;
            start.RedirectStandardInput = true;
            start.WindowStyle = ProcessWindowStyle.Normal;
            start.CreateNoWindow = false;
            start.Arguments = args;

            return Process.Start(start);
        }

        public static void startServerSocket(int portNumber)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, portNumber);
            TcpListener server = new TcpListener(ep);
            server.Start();
            Console.WriteLine("The server socket is started at {0}", ep.ToString());


            // Console은 그냥 종료 된다.
            new Action(delegate ()
            {
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("A new client is connected from {0}", ep.ToString());
                    new Action<TcpClient>(ClientProcess).BeginInvoke(client, null, null);
                }

            }).BeginInvoke(null, null);
        }


        static void ClientProcess(TcpClient client)
        {
            NetworkStream ns = client.GetStream();

            byte[] buffer;
            // 1회 전송 데이터의 최대 길이
            int bufferSize = 1024;
            // 1회 데이터를 읽은 길이
            int nRead = 0;
            string line;
            string clientName = "";

            while (client.Connected)
            {
                buffer = new byte[bufferSize];

                try
                {
                    nRead = ns.Read(buffer, 0, bufferSize);
                    if (nRead > 0)
                    {
                        line = Encoding.Default.GetString(buffer, 0, nRead);

                        Console.WriteLine(string.Format("Received Data : [{2}] {0} : {1}", clientName, line, client.Client.RemoteEndPoint));
                        /* 출력용 클라이언트인가 프린트를 전송 */

                        if (clientName.Equals("PRINT_CLIENT") && line.Equals("PRINT"))
                        {
                            ////string outputData = MakeSocketPrintData();
                            ////SendData(ns, outputData);
                            break;
                        }
                        else if (line.StartsWith("BUS") || line.StartsWith("MOBILE")) //개행 문자 처리해야지
                        {
                            clientName = line;
                            ////inputData[clientName] = new List<string>();
                        }
                        else
                        {
                            ////DoSocketLineJob(clientName, line);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    Thread.Sleep(1);
                }
            }
        }


        static void SendData(NetworkStream ns, string data)
        {
            byte[] bufs = Encoding.ASCII.GetBytes(data);
            ns.Write(bufs, 0, bufs.Length);
        }

        // 병렬 처리 테스트 프로그램
        static public void Test(int index)
        {
            Console.WriteLine(string.Format("{0} Start", index));
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine(string.Format("{0} End", index));
        }

        //Runtime 외부 라이브러리 펑션 호출
        static public object ExecuteExternalLib(string LibPath, string className, string function, params object[] args)
        {
            Assembly assembly = Assembly.LoadFile(Environment.CurrentDirectory + "\\" + LibPath);
            Type type = assembly.GetType(className);

            var obj = Activator.CreateInstance(type);
            // Alternately you could get the MethodInfo for the TestRunner.Run method

            return type.InvokeMember(function,
                              BindingFlags.Default | BindingFlags.InvokeMethod,
                              null,
                              obj,
                              args);

        }

        static public void TestClient(int index)
        {
            TcpClient client = null;

            bool bSuccess = false;
            while (!bSuccess)
            {
                try
                {
                    client = new TcpClient();

                    client.Connect("127.0.0.1", 9999);
                    Console.WriteLine("Connected!");

                    SendData(client.GetStream(), "REQUEST");
                    Console.WriteLine("Write Success!");

                    string data = "";
                    ReadData(client.GetStream(), 1024, out data);
                    Console.WriteLine("Read : " + data);

                    bSuccess = true;
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    client.GetStream().Close();
                    client.Close();

                    if (!bSuccess)
                    {
                        Console.WriteLine("Fail!");
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        static public void ReadData(NetworkStream ns, int iLength, out string data)
        {
            data = "";
            byte [] bdata = new byte[iLength];

            int iread = -1;

            ns.ReadTimeout = 5000;
            while (ns.CanRead)
            {
                iread = ns.Read(bdata, 0, iLength);
                if (iread > 0)
                {
                    data = Encoding.ASCII.GetString(bdata, 0, iread);
                    break;
                }

                Thread.Sleep(1000);
            }

        }

    }
}

