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

namespace SP_TEST
{
    class Program
    {
        static List<Bus> busList = null;
        static List<Bus> preBusList = new List<Bus>();
        static List<BusStation> stationList = new List<BusStation>();

        static string currentTime;
        static void Main(string[] args)
        {
            string fileNameLocation = @"INFILE\LOCATION.TXT";
            string fileNameStation = @"INFILE\STATION.TXT";
            //ReadFile(fileNameLocation, fileNameStation);
            //string writeFileName = @"OUTFILE\PREPOST.TXT";
            //DoWriteFileJob(writeFileName);
            string writeFileName = @"OUTFILE\ARRIVAL.TXT";
            //DoWriteFileJob2(writeFileName);
            string writeFileName3 = @"OUTFILE\SIGNAGE.TXT";
            //DoWriteFileJob3(writeFileName3);
            ServerProgram(9876);
        }
        public static void ServerProgram(int PortNumber)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, PortNumber);
            TcpListener listener = new TcpListener(ep);
            listener.Start();
            Console.WriteLine("Server Socket Start : " + ep.ToString());

            while(true)
            {
                TcpClient client = listener.AcceptTcpClient();

                Console.WriteLine("A new cliend connected : " + ep.ToString());

                ParameterizedThreadStart ParaTh = new ParameterizedThreadStart(ThreadFunc);

                Thread th = new Thread(ParaTh);

                th.Start(client);

            }



        }
        public static void ThreadFunc(object client)
        {
            TcpClient c = (TcpClient)client;
            NetworkStream ns = c.GetStream();

            byte[] buffer;

            int bufferSize = 1024;

            int nRead = 0;
            string line = "";

            while(c.Connected)
            {
                buffer = new byte[bufferSize];

                try
                {
                    using (StreamReader sw = new StreamReader(ns))
                    {

                        line = sw.ReadLine();
                        Console.WriteLine(line);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    Thread.Sleep(1);
                }

                nRead = ns.Read(buffer, 0, bufferSize);

                if (nRead > 0)
                {
                    line = Encoding.Default.GetString(buffer, 0, nRead);


                }

            }



        }

        public static void ReadFile(string locationFilename, string stationFilename)
        {
            String line;
            string line2;
            using (StreamReader sr = new StreamReader(locationFilename))
            {
                while (sr.Peek() > -1)
                {
                    line = sr.ReadLine();

                    DoLineJob(line);
                }

            }
            using (StreamReader sr = new StreamReader(stationFilename))
            {
                while (sr.Peek() > -1)
                {
                    line2 = sr.ReadLine();

                    DoLineJob2(line2);
                }

            }
        }
        static void DoLineJob(string line)
        {
            if (line == "PRINT")
                return;

            if (line.Contains("#"))
            {
                string[] items = line.Split(("#").ToArray());
                currentTime = items[0];
                busList = new List<Bus>();
                for (int i = 1; i < items.Length; i++)
                {
                    Bus bus = new Bus();
                    string[] items2 = items[i].Split(',').ToArray();
                    if (preBusList.Count > 0)
                    {
                        bus.speed = int.Parse(items2[1]) - int.Parse(preBusList[i - 1].distance);
                        bus.phSpeed = bus.speed * 3600 / 1000;
                    }
                    else
                    {
                        bus.speed = 0;
                    }
                    bus.busName = items2[0];
                    bus.distance = items2[1];
                    busList.Add(bus);

                }
                preBusList = busList;
            }
            else
            {
                currentTime = line;
                busList = new List<Bus>();
                for (int i = 0; i < 3; i++)
                {
                    Bus bus = new Bus();
                    bus.busName = "BUS0" + (i + 1);
                    bus.distance = (int.Parse(preBusList[i].distance) + preBusList[i].speed).ToString("00000");
                    bus.speed = preBusList[i].speed;
                    bus.phSpeed = preBusList[i].phSpeed;
                    busList.Add(bus);
                }
                preBusList = busList;

            }
        }
        static void DoLineJob2(string line)
        {

            string[] items = line.Split(("#").ToArray());
            BusStation busStation = new BusStation();
            busStation.stationName = items[0];
            busStation.distance = items[1];
            busStation.limitSpeed = int.Parse(items[2]);
            stationList.Add(busStation);


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
        public static void DoWriteFileJob(string fileName)
        {
            for (int i = 0; i < busList.Count; i++)
            {
                string bus = busList[i].busName;
                int tempBeforeDistance = 10000;
                int tempBehindDistance = 10000;
                string beforeBus = "";
                string beforeBusDistance = "";
                string behindBus = "";
                string behindBusDistance = "";
                int beforecount = 0;
                int behindcount = 0;
                for (int j = 0; j < busList.Count; j++)
                {
                    if (bus == busList[j].busName)
                        continue;

                    if (int.Parse(busList[i].distance) - int.Parse(busList[j].distance) < 0)
                    {
                        beforecount++;
                        if (Math.Abs(int.Parse(busList[i].distance) - int.Parse(busList[j].distance)) < tempBeforeDistance)
                        {
                            beforeBus = busList[j].busName;
                            beforeBusDistance = (int.Parse(busList[j].distance) - int.Parse(busList[i].distance)).ToString("00000");
                            tempBeforeDistance = int.Parse(busList[j].distance) - int.Parse(busList[i].distance);
                        }

                        if (beforecount == 2)
                        {
                            behindBus = "NOBUS";
                            behindBusDistance = "00000";
                        }

                    }
                    else
                    {
                        behindcount++;
                        if (int.Parse(busList[i].distance) - int.Parse(busList[j].distance) < tempBehindDistance)
                        {
                            behindBus = busList[j].busName;
                            behindBusDistance = (int.Parse(busList[i].distance) - int.Parse(busList[j].distance)).ToString("00000");
                            tempBehindDistance = int.Parse(busList[j].distance) - int.Parse(busList[i].distance);
                        }
                        if (behindcount == 2)
                        {
                            beforeBus = "NOBUS";
                            beforeBusDistance = "00000";
                        }
                    }
                }
                string text = currentTime + "#" + bus + "#" + beforeBus + "," + beforeBusDistance + "#" + behindBus + "," + behindBusDistance + "\n";
                WriteFile(fileName, text, true);
            }
        }
        public static void DoWriteFileJob2(string fileName)
        {
            string arrivalBus = "";
            string stationName = "";
            string arrivalDistance = "";



            for (int i = 0; i < stationList.Count; i++)
            {
                bool flag = false;
                int tmpLength = 10000;
                for (int j = 0; j < busList.Count; j++)
                {
                    if (int.Parse(stationList[i].distance) - int.Parse(busList[j].distance) < 0 && flag == false)
                    {
                        arrivalBus = "NOBUS";
                        stationName = stationList[i].stationName;
                        arrivalDistance = "00000";
                    }
                    if (int.Parse(stationList[i].distance) - int.Parse(busList[j].distance) > 0)
                    {

                        if (int.Parse(stationList[i].distance) - int.Parse(busList[j].distance) < tmpLength)
                        {
                            arrivalBus = busList[j].busName;
                            stationName = stationList[i].stationName;
                            arrivalDistance = (int.Parse(stationList[i].distance) - int.Parse(busList[j].distance)).ToString("00000");
                            tmpLength = int.Parse(stationList[i].distance) - int.Parse(busList[j].distance);
                        }
                        flag = true;
                    }
                }
                string text = currentTime + "#" + stationName + "#" + arrivalBus + "," + arrivalDistance + "\n";
                WriteFile(fileName, text, true);
            }
        }
        public static void DoWriteFileJob3(string fileName)
        {
            string arrivalBus = "";
            string stationName = "";
            string arrivalTime = "";
            string arrivalTimeHH = "11";
            string arrivalTimeMM = "";
            string arrivalTimeSS = "";
            int tmpArrivalTime = 999999;
            string textProgram = "";

            for (int i = 0; i < stationList.Count; i++)
            {
                bool flag = false;

                for (int j = 0; j < busList.Count; j++)
                {
                    if (int.Parse(stationList[i].distance) < int.Parse(busList[j].distance) && flag == false)
                    {
                        stationName = stationList[i].stationName;
                        arrivalBus = "NOBUS";
                        arrivalTime = "00:00:00";
                    }
                    else if (int.Parse(stationList[i].distance) > int.Parse(busList[j].distance))
                    {
                        flag = true;
                        int length = (int.Parse(stationList[i].distance) - int.Parse(busList[j].distance)) / busList[j].speed;
                        if ((length / 60) + (length % 60) < tmpArrivalTime)
                        {
                            arrivalTimeMM = (length / 60).ToString("00");
                            arrivalTimeSS = (length % 60).ToString("00");
                            tmpArrivalTime = int.Parse(arrivalTimeSS) + (int.Parse(arrivalTimeMM) * 60);
                            stationName = stationList[i].stationName;
                            arrivalBus = busList[j].busName;
                            arrivalTime = arrivalTimeHH + ":" + arrivalTimeMM + ":" + arrivalTimeSS;
                        }


                    }
                }
                string text = currentTime + "#" + stationName + "#" + arrivalBus + "," + arrivalTime + "\n";
                textProgram += currentTime + "#" + stationName + "#" + arrivalBus + "," + arrivalTime + "\n";

                //WriteFile(fileName, text, true);
            }
            executeExternalProcess("SIGNAGE.EXE", textProgram);
        }
        public static void executeExternalProcess(string fileName, string text)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = fileName;
            start.UseShellExecute = false;
            start.RedirectStandardInput = true;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            //start.Arguments = args;
            using (Process process = Process.Start(start))
            {
                using (StreamWriter sw = process.StandardInput)
                {
                    sw.Write(text);
                }
            }
        }
        class Bus
        {
            public string busName { get; set; }
            public string distance { get; set; }
            public int speed { get; set; }
            public int phSpeed { get; set; }
        }
        class BusStation
        {
            public string stationName { get; set; }
            public string distance { get; set; }
            public int limitSpeed { get; set; }
        }
    }
}

