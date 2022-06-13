 class Program
    {
        static string inputValue = Console.ReadLine();
        static bool end = true;
        static List<string> reSendData = new List<string>();
        static Dictionary <string,string> sendDatas = new Dictionary<string, string>();

        static object obj = new object();

        static void Main(string[] args)
        {
            //상대경로
            string inPath1 = @"..\SUB4\INPUT\MONITORING_KBWAS02.txt";
            string inPath2 = @"..\SUB4\INPUT\MONITORING_KORDB07.txt";
            string inPath3 = @"..\SUB4\INPUT\MONITORING_LGWEB01.txt";
            string inPath4 = @"..\SUB4\INPUT\MONITORING_WAS0005.txt";
            string inPath5 = @"..\SUB4\INPUT\MONITORING_WEB0003.txt";

            //Thread Para 전달
            Thread t1 = new Thread(() => Run(inPath1, "KBWAS02"));
            t1.Start();
            Thread t2 = new Thread(() => Run(inPath2, "KORDB07"));
            t2.Start();
            Thread t3 = new Thread(() => Run(inPath3, "LGWEB01"));
            t3.Start();
            Thread t4 = new Thread(() => Run(inPath4, "WAS000"));
            t4.Start();
            Thread t5 = new Thread(() => Run(inPath5, "WEB0003"));
            t5.Start();


        }

        public static void Run(object inPath, string name)
        {

            int cnt = 0;
            int val = 0;
            int firstVal = 0;
            List<string> sendLines = new List<string>();
 

            bool bSuccess = false;
            while (!bSuccess)
            {
                //  string filePath = @"..\SUB3/SUPPORT/ALERT.EXE";

                Thread.Sleep(500);

                string line;

                string[] temp;
                string[] temp2;


                //FileInfo생성
                FileInfo fi = new FileInfo(inPath.ToString());
                //FileInfo.Exists로 파일 존재유무 확인 "
                if (fi.Exists)
                {
                    FileStream ReadData = new FileStream(inPath.ToString(), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

                    List<int> avr = new List<int>();
                    List<int> vals = new List<int>();
                    List<string> lines = new List<string>();
                    StreamReader fileRead = new StreamReader(ReadData);

                    try
                    {
                        while (fileRead.Peek() > -1)
                        {
                            line = fileRead.ReadLine();
                            cnt++;
                            temp = line.Split('#');

                            foreach (var item2 in temp)
                            {
                                temp2 = item2.Split(':');
                                if (temp2.Contains("SYS"))
                                {
                                    //Y,N 표시
                                    if (Convert.ToInt32(temp2[1]) > Convert.ToInt32(inputValue))
                                    {
                                        line = line + "#Y";
                                    }
                                    else
                                    {
                                        line = line + "#N";
                                    }

                                    //평균 표시
                                    avr.Add(Convert.ToInt32(temp2[1]));
                                    if (avr.Count < 3)
                                    {
                                        val = Convert.ToInt32(avr.Average());
                                    }
                                    else
                                    {
                                        val = (avr[avr.Count - 1] + avr[avr.Count - 2] + avr[avr.Count - 3]) / 3;
                                    }
                                    line = line + "#" + String.Format("{0:000}", val);

                                    //임계값보다 크면
                                    if (val > Convert.ToInt32(inputValue))
                                    {
                                        if (vals.Count > 0)
                                        {
                                            if (vals[vals.Count - 1] <= val || (vals.Max() - 3) <= val)
                                            {
                                                //if (val > firstVal) firstVal = val;
                                                vals.Add(val);
                                                lines.Add(line);

                                                // 5번 연속 6번째 장애
                                                if (lines.Count > 5)
                                                {
                                                    //장애원인 계산(line)
                                                    string proc = errorList(lines);

                                                    line = name + "#" + line + "#" + proc + "\n";
                                                    //Console.WriteLine(line);                                                
                                                    lines.RemoveAt(0);

                                                    if (!sendLines.Contains(line))
                                                    {
                                                        //서버 송신
                                                        sendLines.Add(line);
                                                        TestClient(line);

                                                        //다른 프로그램 호출
                                                        //executeExternalProcess_standardOutput(filePath, line);
                                                        //Console.WriteLine(line);
                                                    }
                                                }
                                            }
                                            // 연속 해제
                                            else
                                            {
                                                vals.Clear();
                                                lines.Clear();
                                            }
                                        }
                                        // 임계값 오버 처음
                                        else
                                        {
                                            if (vals.Count == 0) firstVal = val;
                                            vals.Add(val);
                                            lines.Add(line);
                                        }
                                    }
                                    else
                                    {
                                        vals.Clear();
                                        lines.Clear();
                                    }
                                }

                            }
                        }
                        //bSuccess = true;
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                    ////               Thread.Sleep(300);
                }
            }            
        }

        static string errorList(List<string> lines)
        {
            List<int> SYS = new List<int>();
            List<int> PROC01 = new List<int>();
            List<int> PROC02 = new List<int>();
            List<int> PROC03 = new List<int>();
            List<int> PROC04 = new List<int>();
            List<int> PROC05 = new List<int>();

            foreach (string line in lines)
            {
                string[] temp = line.Split('#');

                foreach (var item in temp)
                {
                    string[] temp2 = item.Split(':');
                    foreach (var item2 in temp2)
                    {
                        switch (item2.ToString())
                        {
                            case "SYS":
                                SYS.Add(Convert.ToInt32(temp2[1]));
                                break;
                            case "PROC01":
                                PROC01.Add(Convert.ToInt32(temp2[1]));
                                break;
                            case "PROC02":
                                PROC02.Add(Convert.ToInt32(temp2[1]));
                                break;
                            case "PROC03":
                                PROC03.Add(Convert.ToInt32(temp2[1]));
                                break;
                            case "PROC04":
                                PROC04.Add(Convert.ToInt32(temp2[1]));
                                break;
                            case "PROC05":
                                PROC05.Add(Convert.ToInt32(temp2[1]));
                                break;
                        }

                    }

                }

            }

            //List<List<int>> intList = new List<List<int>>();

            Dictionary<string, List<int>> intList = new Dictionary<string, List<int>>();

            intList.Add("SYS", SYS);
            intList.Add("PROC01", PROC01);
            intList.Add("PROC02", PROC02);
            intList.Add("PROC03", PROC03);
            intList.Add("PROC04", PROC04);
            intList.Add("PROC05", PROC05);

            int sys = 0;
            int min = 0;
            string maxString = null;

            foreach (List<int> itemList in intList.Values)
            {
                int temp = 0;
                for (int i = 0; i < itemList.Count; i++)
                {
                    if (i > 0)
                    {
                        temp = temp + (itemList[i] - itemList[i - 1]);
                    }
                }
                if (intList.FirstOrDefault(x => x.Value == itemList).Key == "SYS")
                {
                    sys = temp;
                }
                else
                {

                    if (intList.FirstOrDefault(x => x.Value == itemList).Key == "PROC01")
                    {
                        min = Math.Abs(sys - temp);
                        maxString = intList.FirstOrDefault(x => x.Value == itemList).Key;
                    }
                    else
                    {
                        if (Math.Abs(sys - temp) < min)
                        {
                            min = Math.Abs(sys - temp);
                            // Dictionary valu로 key 찾기
                            maxString = intList.FirstOrDefault(x => x.Value == itemList).Key;
                        }
                    }
                }
            }
            return maxString;
        }

        static void SendData(NetworkStream ns, string data)
        {
            byte[] bufs = Encoding.ASCII.GetBytes(data);
            ns.Write(bufs, 0, bufs.Length);
        }


        static public void TestClient(string sendData)
        {
            TcpClient client = null;

            lock (obj)
            {
                string name = sendData.Substring(0, 7);
                if (!sendDatas.ContainsKey(name))
                {
                    sendDatas.Add(name, sendData.Replace("\n", string.Empty));
                }
                else
                {
                    sendDatas[name] = sendData.Replace("\n", string.Empty);
                }


                bool bSuccess = false;
                while (!bSuccess)
                {
                    try
                    {
                        client = new TcpClient();

                        client.Connect("127.0.0.1", 5000);
                        Console.WriteLine("Connected!");

                        SendData(client.GetStream(), sendData);
                        Console.WriteLine("Write Success!");

                        //Thread.Sleep(2000);

                        string data = "";
                        ReadData(client.GetStream(), 1024, out data);
                        Console.WriteLine("Read : " + data);

                        //재전송
                        //문자열 끝 제거 TrimEnd
                        //문자열 시작 제거 TrimStart
                        if (data.TrimEnd('\n') == "F")
                        {
                            if (!reSendData.Contains(sendData))
                            {
                                //SendData(client.GetStream(), sendData);
                                //Console.WriteLine("resend");
                                reSendData.Add(sendData);
                                TestClient(sendData);
                                //Thread.Sleep(2000);

                                //ReadData(client.GetStream(), 1024, out data);
                                //Console.WriteLine("Read : " + data);

                                if (data.TrimEnd('\n') == "F")
                                {
                                    //파일 기록
                                    //client.GetStream().Close();
                                    //client.Close();
                                }
                            }
                        }
                        else if (data.TrimEnd('\n') == "U")
                        {
                            writeReport(sendDatas);
                        }
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
        }
         public static void writeReport(Dictionary <string, string> sendDatas)
        {
            int count = 1;

            string fullPath = @"..\SUB4\OUTPUT\REPORT.txt";


            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string path = Path.GetDirectoryName(fullPath);
            string newFullPath = fullPath;

            while (File.Exists(newFullPath))
            {
                string tempFileName = fileNameOnly + "_" + string.Format("{0:000}", count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }

            StreamWriter fileOut = new StreamWriter(newFullPath);

            //오름차순 정렬 . 내림차순은 OrderByDescending
            sendDatas = sendDatas.OrderBy(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value); 

            foreach (string item in sendDatas.Values)
            {
                fileOut.WriteLine(item);
            }
            fileOut.Close();

        }

        public static void clientSocket()
        {
            try
            {
                string strRecvMsg;
                string strSendMsg;

                TcpClient sockClient = new TcpClient("127.0.0.1", 5000); //소켓생성,커넥트
                NetworkStream ns = sockClient.GetStream();
                StreamReader sr = new StreamReader(ns);
                StreamWriter sw = new StreamWriter(ns);

                //strRecvMsg = sr.ReadLine();         //서버로부터 접속 성공 메시지 수신
               // Console.WriteLine(strRecvMsg);

                while (true)
                {
                    strSendMsg = Console.ReadLine();

                    sw.WriteLine(strSendMsg);
                    sw.Flush();
                    if (strSendMsg == "exit")
                    {
                        break;
                    }
                    strRecvMsg = sr.ReadLine();
                    Console.WriteLine(strRecvMsg);
                }
                sr.Close();
                sw.Close();
                ns.Close();
                sockClient.Close();

                Console.WriteLine("접속 종료!");
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static public void ReadData(NetworkStream ns, int iLength, out string data)
        {
            data = "";
            byte[] bdata = new byte[iLength];

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
    }
