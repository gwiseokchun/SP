namespace DigitalTwinIOServer
{

    public partial class DTIOServer : CControlServer
    {
        readonly object _sync = new object();
        private CSocketServer _driver = null; // Serial Driver 인스턴스
        private StringBuilder _strBuffer = new StringBuilder(); // 소켓통신 시리얼 버퍼
        List<JObject> _JsonLogData = null;
        private double _speed = 1.0;
        private string _selectedFileName = null;
        private int _startCnt = 1;
        //private System.Timers.Timer _sendVaccumTimer = new System.Timers.Timer();

        private DataTable _logTable = null;

        private Timer _sendVaccumTimer = null;
        //        private Timer _sendLogTimer = null;
        private System.Timers.Timer _sendLogTimer = null;

        protected static readonly object _InstanceLock = new object();

        Dictionary<int, string> PASS_TM_LOAD = new Dictionary<int, string>();
        Dictionary<int, string> PASS_TM_UNLOAD = new Dictionary<int, string>();
        Dictionary<int, string> PASS_TM_UNLOADWAIT = new Dictionary<int, string>();

        Dictionary<int, string> TM_LOAD = new Dictionary<int, string>();
        Dictionary<int, string> TM_UNLOAD = new Dictionary<int, string>();
        Dictionary<int, string> TM_EXCHANGE = new Dictionary<int, string>();

        LinkedList<string> TM_LOAD_LIST = new LinkedList<string>();

        List<FileInfo> _fileList = new List<FileInfo>();

        Dictionary<int, string> PASS_TM = new Dictionary<int, string>();
        Dictionary<int, string> TM = new Dictionary<int, string>();

        Queue<string> msgQVaccum = new Queue<string>();
        Queue<DataRow> msgQLog = new Queue<DataRow>();

        int cnt = 0;

        protected override void OnInstancingCompleted()
        {
            base.OnInstancingCompleted();
        }

        protected override void OnInitializeCompleted()
        {
            base.OnInitializeCompleted();

            this.Variables["Start_SendLog"].OnBooleanChanged += DTIOServer_OnBooleanChanged;
            this.Variables["Start_VaccumLog"].OnBooleanChanged += DTIOServer_OnBooleanChanged1; ;
            this.Variables["Speed"].OnIntegerChanged += DTIOServer_OnIntegerChanged;

            //_sendLogTimer = new System.Timers.Timer();
            //_sendLogTimer.Elapsed += _sendLogTimer_Elapsed;

            //TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 9005);
            //server.Start();

            _driver = new CSocketServer();
            _driver.ConnectionInfoString = string.Format("MODE={0},PORT={1}", "TCP_PASSIVE", this.Variables["PORT_ID"].ToString()); ;
            //_driver.EnableLog(Variables[CVariableName.V_DRIVER_ENABLELOG_ASCII].AsBoolean, Variables[CVariableName.V_DRIVER_ENABLELOG_HEX].AsBoolean);
            _driver.OpenRetrySec = 300;
            _driver.OnReceived += _driver_OnReceived1;
            _driver.EnableLog(true, false);

            _driver.OnClientConnected += _driver_OnClientConnected;
            _driver.OnClientDisconnected += _driver_OnClientDisconnected;

            _driver.ConnectionStateChanged += _driver_ConnectionStateChanged;

            string localIP = "Not available, please check your network seetings!";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }

            _driver.ActiveOpen(); // Socket Driver Open
            SystemLogger.Log(Level.Debug, "(IP : "+localIP + ", PORT : " + this.Variables["PORT_ID"].ToString() + ") IO Server LISTENING...");

            PASS_TM_LOAD.Add(1, "Robot Command.Load");
            PASS_TM_LOAD.Add(2, "I_ORIGIN_LOWER_HAND.OFF");
            PASS_TM_LOAD.Add(3, "Glass 정보 이동 완료");
            PASS_TM_LOAD.Add(4, "I_COMMAND_RUNNING.OFF");

            PASS_TM_UNLOADWAIT.Add(1, "Robot Command.UnloadWait");
            PASS_TM_UNLOADWAIT.Add(2, "I_COMMAND_RUNNING.OFF");

            PASS_TM_UNLOAD.Add(1, "Robot Command.Unload");
            PASS_TM_UNLOAD.Add(2, "I_ORIGIN_LOWER_HAND.OFF");
            PASS_TM_UNLOAD.Add(3, "Glass 정보 이동 완료");
            PASS_TM_UNLOAD.Add(4, "I_COMMAND_RUNNING.OFF");

            TM_LOAD.Add(1, "Robot Command.Load");
            TM_LOAD.Add(2, "I_ORIGIN_UPPER_HAND.OFF");
            TM_LOAD.Add(3, "Glass 정보 이동 완료");
            TM_LOAD.Add(4, "I_COMMAND_RUNNING.OFF");

            TM_UNLOAD.Add(1, "Robot Command.Unload");
            TM_UNLOAD.Add(2, "I_ORIGIN_UPPER_HAND.OFF");
            TM_UNLOAD.Add(3, "Glass 정보 이동 완료");
            TM_UNLOAD.Add(4, "I_COMMAND_RUNNING.OFF");

            TM_EXCHANGE.Add(1, "Robot Command.ContinuousExchange");
            TM_EXCHANGE.Add(2, "I_ORIGIN_UPPER_HAND.OFF");
            TM_EXCHANGE.Add(3, "Glass 정보 이동 완료");
            TM_EXCHANGE.Add(4, "I_ORIGIN_UPPER_HAND.OFF");
            TM_EXCHANGE.Add(5, "Glass 정보 이동 완료");
            TM_EXCHANGE.Add(6, "I_COMMAND_RUNNING.OFF");

            loadingFileList();
        }

        private void DTIOServer_OnIntegerChanged(CVariable sender, int value)
        {

        }

        private void _driver_OnClientDisconnected(CSocketClient client)
        {

        }

        private void _driver_OnClientConnected(CSocketClient client)
        {
            sendFileInfo(client);
        }

        private void _sendLogTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _sendLogTimer.Stop();
            _sendLogTimer.Enabled = false;

            //msgQLog.CopyTo(msgQLog.ToArray(), 30);


            //DataRow[] array2 = msgQLog.ToArray();
            //msgQLog.CopyTo(array2,);

            // Create a second queue, using the constructor that accepts an
            // IEnumerable(Of T).



            //IEnumerable<DataRow> selected_msgQLog = msgQLog.Select(x => Convert.ToInt32(x["NO"]) > _startCnt);
            //msgQLog = new Queue<DataRow>(msgQLog.Select(x => x[  _startCnt.ToString()));


            DataRow dr = msgQLog.Dequeue();


            //msgQLog = msgQLog.Select(x => Convert.ToInt32(x["NO"].ToString()) > _startCnt);


            if (dr != null)
            {

                sendLogData(dr);

                if (dr["DIFF_TIME"] != null)
                {

                    TimeSpan diffTtime = TimeSpan.Parse(dr["DIFF_TIME"].ToString());
                    //  SystemLogger.Log(Level.Debug, diffTtime.ToString());



                    if (diffTtime.Ticks != 0)
                    {
                        _sendLogTimer.Interval = diffTtime.TotalMilliseconds / _speed;
                    }
                }
                //else
                //{
                //    sendLogData(dr);
                //}
                _sendLogTimer.Enabled = true;
                _sendLogTimer.Start();
            }
            //                  Thread.Sleep(Convert.ToInt32(diffTtime.TotalMilliseconds));
            //this.Wait_Sleep(Convert.ToInt32(diffTtime.TotalMilliseconds));
        }

        private void DTIOServer_OnBooleanChanged(CVariable sender, bool value)
        {
            if (value)
            {
                startLogTimer();
            }
            else
            {
                stopLogTimer();
            }
        }

        private void DTIOServer_OnBooleanChanged1(CVariable sender, bool value)
        {

        }

        private void loadingFileList()
        {
            try
            {
                SystemLogger.Log(Level.Debug, "loadingLogFileList");
                //string inPath = @"C:\LGCNS.ezControl\FactovaControl_Release_L_2.0.9.1(21_01_25)\EDM_(일반진행)LogExtract_전Unit_20210513_v0.1.xlsx";
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                String FolderName = di.ToString();
                //System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(FolderName);
                foreach (System.IO.FileInfo File in di.GetFiles())
                {
                    if (File.Extension.ToLower().CompareTo(".xlsx") == 0)
                    {
                        String FileNameOnly = File.Name.Substring(0, File.Name.Length - 4);
                        String FullFileName = File.FullName;

                        DataTable dttemp = ReadDataExcel(FullFileName);

                        FileInfo fi = new FileInfo();

                        fi.Name = FileNameOnly;
                        fi.rowCount = dttemp.Rows.Count;
                        fi.Data = dttemp;

                        _fileList.Add(fi);

                    }
                }
            }
            catch (Exception ex)
            {
                SystemLogger.Log(ex);
            }
        }
        private void loadingLogFile()
        {
            try
            {

                string eqpid = this.Variables["EQPID"].AsString;


                if (_selectedFileName != null)
                {
                    //string inPath = @"..\FactovaControl_Release_L_2.0.9.1(21_01_25)\EDM_LogExtract_전Unit_20210513_v0.1.xlsx";

                    List<Dictionary<string, object>> rows;
                    Dictionary<string, object> row;
                    _JsonLogData = new List<JObject>();


                    FileInfo fi = _fileList.Find(x => x.Name.Contains(_selectedFileName));

                    DataTable dt = fi.Data;

                    DateTime start_time_PassTM = new DateTime();
                    DateTime start_time_TM = new DateTime();
                    _logTable = dt;


                    //TactTime 컬럼 추가
                    if (!_logTable.Columns.Contains("NO")) _logTable.Columns.Add("NO");
                    if (!_logTable.Columns.Contains("VALUE6")) _logTable.Columns.Add("VALUE6");
                    if (!_logTable.Columns.Contains("DIFF_TIME")) _logTable.Columns.Add("DIFF_TIME");

                    int seqPass_TM = 1;
                    int seqTM = 1;

                    DataRow passTMtactTimeDr = null;
                    DataRow TMtactTimeDr = null;

                    DateTime tick = new DateTime();
                    TimeSpan diffTtime = new TimeSpan();

                    //Robot TactTime 계산
                    foreach (DataRow dr in _logTable.Rows)
                    {

                        System.Reflection.FieldInfo fInfo = dr.GetType().GetField("_rowID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        int rowID = Convert.ToInt32(fInfo.GetValue(dr));
                        dr["NO"] = rowID;

                        //rowID = rowID - 1;

                        if (rowID < _logTable.Rows.Count)
                        {
                            DataRow drNext = _logTable.Rows[rowID];

                            diffTtime = Convert.ToDateTime(drNext["TIME"]) - Convert.ToDateTime(dr["TIME"]);
                            dr["DIFF_TIME"] = diffTtime;
                        }
                        // PASS_TM Tack 계산 추가 
                        if (dr["UNIT"].ToString() == "PASS_TM")
                        {
                            if (dr[3].ToString() == "Robot Command")
                            {
                                start_time_PassTM = Convert.ToDateTime(dr[0]);
                                seqPass_TM = 1;
                                switch (dr[4].ToString())
                                {
                                    case "Load":
                                        PASS_TM = PASS_TM_LOAD;
                                        break;
                                    case "Unload":
                                        PASS_TM = PASS_TM_UNLOAD;
                                        break;
                                    case "UnloadWait":
                                        PASS_TM = PASS_TM_UNLOADWAIT;
                                        break;
                                    default:
                                        break;
                                }
                                seqPass_TM++;
                                passTMtactTimeDr = dr;
                                continue;
                            }
                            else if (dr[3].ToString().Contains("정보 이동 완료"))
                            {
                                passTMtactTimeDr["VALUE6"] = Convert.ToDateTime(dr[0]) - start_time_PassTM;
                                passTMtactTimeDr = dr;
                                start_time_PassTM = Convert.ToDateTime(dr[0]);
                                seqPass_TM++;
                                continue;
                            }


                            if (PASS_TM.Count < seqPass_TM)
                            {
                                continue;
                            }

                            if (dr[3].ToString() + "." + dr[4].ToString() == PASS_TM[seqPass_TM])
                            {
                                passTMtactTimeDr["VALUE6"] = Convert.ToDateTime(dr[0]) - start_time_PassTM;
                                passTMtactTimeDr = dr;
                                //cal = false;
                                if (PASS_TM[seqPass_TM] == "I_COMMAND_RUNNING.OFF")
                                {
                                    start_time_PassTM = new DateTime();
                                }
                                else
                                {
                                    start_time_PassTM = Convert.ToDateTime(dr[0]);
                                }
                                seqPass_TM++;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        // TM Tack 계산 
                        else if (dr["UNIT"].ToString() == "TM")
                        {
                            if (dr[3].ToString() == "Robot Command")
                            {
                                if (dr[4].ToString().Contains("HandRetract")) continue;

                                start_time_TM = Convert.ToDateTime(dr[0]);

                                //seq 초기화
                                seqTM = 1;
                                switch (dr[4].ToString())
                                {
                                    case "Load":
                                        TM = TM_LOAD;
                                        break;
                                    case "Unload":
                                        TM = TM_UNLOAD;
                                        break;
                                    case "ContinuousExchange":
                                        TM = TM_EXCHANGE;
                                        break;
                                    default:
                                        break;
                                }
                                seqTM++;
                                TMtactTimeDr = dr;
                                continue;
                            }
                            else if (dr[3].ToString().Contains("정보 이동 완료"))
                            {
                                TMtactTimeDr["VALUE6"] = Convert.ToDateTime(dr[0]) - start_time_TM;
                                start_time_TM = Convert.ToDateTime(dr[0]);
                                TMtactTimeDr = dr;
                                seqTM++;
                                continue;
                            }

                            if (TM.Count < seqTM)
                            {
                                continue;
                            }

                            if (dr[3].ToString() + "." + dr[4].ToString() == TM[seqTM])
                            {
                                TMtactTimeDr["VALUE6"] = Convert.ToDateTime(dr[0]) - start_time_TM;

                                //cal = false;
                                if (TM[seqTM] == "I_COMMAND_RUNNING.OFF")
                                {
                                    start_time_TM = new DateTime();
                                }
                                else
                                {
                                    start_time_TM = Convert.ToDateTime(dr[0]);
                                }
                                TMtactTimeDr = dr;
                                seqTM++;
                            }
                        }
                    }

                    foreach (DataRow dr in _logTable.Rows)
                    {
                        msgQLog.Enqueue(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLogger.Log(ex);
            }
        }
        //private void loadingLogFile()
        //{
        //    try
        //    {
        //        if (_selectedFileName != null)
        //        {
        //           // if(_selectedFileName is null) string inPath = @"..\FactovaControl_Release_L_2.0.9.1(21_01_25)\EDM_LogExtract_전Unit_20210513_v0.1.xlsx";

        //            List<Dictionary<string, object>> rows;
        //            Dictionary<string, object> row;
        //            _JsonLogData = new List<JObject>();

        //            FileInfo fi = _fileList.Find(x => x.Name.Contains(_selectedFileName));

        //            DataTable dt = fi.Data;

        //            DateTime start_time_PassTM = new DateTime();
        //            DateTime start_time_TM = new DateTime();
        //            _logTable = dt;


        //            //TactTime 컬럼 추가
        //            if (!_logTable.Columns.Contains("NO")) _logTable.Columns.Add("NO");
        //            if (!_logTable.Columns.Contains("VALUE6")) _logTable.Columns.Add("VALUE6");
        //            if (!_logTable.Columns.Contains("DIFF_TIME")) _logTable.Columns.Add("DIFF_TIME");

        //            int seqPass_TM = 1;
        //            int seqTM = 1;

        //            DataRow passTMtactTimeDr = null;
        //            DataRow TMtactTimeDr = null;

        //            DateTime tick = new DateTime();
        //            TimeSpan diffTtime = new TimeSpan();

        //            //Robot TactTime 계산
        //            foreach (DataRow dr in _logTable.Rows)
        //            {                      

        //                System.Reflection.FieldInfo fInfo = dr.GetType().GetField("_rowID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        //                int rowID = Convert.ToInt32(fInfo.GetValue(dr));
        //                dr["NO"] = rowID;

        //                //rowID = rowID - 1;

        //                if (rowID < _logTable.Rows.Count)
        //                {
        //                    DataRow drNext = _logTable.Rows[rowID];

        //                    diffTtime = Convert.ToDateTime(drNext["TIME"]) - Convert.ToDateTime(dr["TIME"]);
        //                      dr["DIFF_TIME"] = diffTtime;
        //                }
        //                // PASS_TM Tack 계산 추가 
        //                if (dr["UNIT"].ToString() == "PASS_TM")
        //                {
        //                    if (dr[3].ToString() == "Robot Command")
        //                    {
        //                        start_time_PassTM = Convert.ToDateTime(dr[0]);
        //                        seqPass_TM = 1;
        //                        switch (dr[4].ToString())
        //                        {
        //                            case "Load":
        //                                PASS_TM = PASS_TM_LOAD;
        //                                break;
        //                            case "Unload":
        //                                PASS_TM = PASS_TM_UNLOAD;
        //                                break;
        //                            case "UnloadWait":
        //                                PASS_TM = PASS_TM_UNLOADWAIT;
        //                                break;
        //                            default:
        //                                break;
        //                        }
        //                        seqPass_TM++;
        //                        passTMtactTimeDr = dr;
        //                        continue;
        //                    }
        //                    else if (dr[3].ToString().Contains("정보 이동 완료"))
        //                    {
        //                        passTMtactTimeDr["VALUE6"] = Convert.ToDateTime(dr[0]) - start_time_PassTM;
        //                        passTMtactTimeDr = dr;
        //                        start_time_PassTM = Convert.ToDateTime(dr[0]);
        //                        seqPass_TM++;
        //                        continue;
        //                    }


        //                    if (PASS_TM.Count < seqPass_TM)
        //                    {
        //                        continue;
        //                    }

        //                    if (dr[3].ToString() + "." + dr[4].ToString() == PASS_TM[seqPass_TM])
        //                    {
        //                        passTMtactTimeDr["VALUE6"] = Convert.ToDateTime(dr[0]) - start_time_PassTM;
        //                        passTMtactTimeDr = dr;
        //                        //cal = false;
        //                        if (PASS_TM[seqPass_TM] == "I_COMMAND_RUNNING.OFF")
        //                        {
        //                            start_time_PassTM = new DateTime();
        //                        }
        //                        else
        //                        {
        //                            start_time_PassTM = Convert.ToDateTime(dr[0]);
        //                        }
        //                        seqPass_TM++;
        //                    }
        //                    else
        //                    {
        //                        continue;
        //                    }
        //                }
        //                // TM Tack 계산 
        //                else if (dr["UNIT"].ToString() == "TM")
        //                {
        //                    if (dr[3].ToString() == "Robot Command")
        //                    {
        //                        if (dr[4].ToString().Contains("HandRetract")) continue;

        //                        start_time_TM = Convert.ToDateTime(dr[0]);

        //                        //seq 초기화
        //                        seqTM = 1;
        //                        switch (dr[4].ToString())
        //                        {
        //                            case "Load":
        //                                TM = TM_LOAD;
        //                                break;
        //                            case "Unload":
        //                                TM = TM_UNLOAD;
        //                                break;
        //                            case "ContinuousExchange":
        //                                TM = TM_EXCHANGE;
        //                                break;
        //                            default:
        //                                break;
        //                        }
        //                        seqTM++;
        //                        TMtactTimeDr = dr;
        //                        continue;
        //                    }
        //                    else if (dr[3].ToString().Contains("정보 이동 완료"))
        //                    {
        //                        TMtactTimeDr["VALUE6"] = Convert.ToDateTime(dr[0]) - start_time_TM;
        //                        start_time_TM = Convert.ToDateTime(dr[0]);
        //                        TMtactTimeDr = dr;
        //                        seqTM++;
        //                        continue;
        //                    }

        //                    if (TM.Count < seqTM)
        //                    {
        //                        continue;
        //                    }

        //                    if (dr[3].ToString() + "." + dr[4].ToString() == TM[seqTM])
        //                    {
        //                        TMtactTimeDr["VALUE6"] = Convert.ToDateTime(dr[0]) - start_time_TM;

        //                        //cal = false;
        //                        if (TM[seqTM] == "I_COMMAND_RUNNING.OFF")
        //                        {
        //                            start_time_TM = new DateTime();
        //                        }
        //                        else
        //                        {
        //                            start_time_TM = Convert.ToDateTime(dr[0]);
        //                        }
        //                        TMtactTimeDr = dr;
        //                        seqTM++;
        //                    }
        //                }
        //            }

        //            foreach (DataRow dr in _logTable.Rows)
        //            {
        //                msgQLog.Enqueue(dr);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        SystemLogger.Log(ex);
        //    }           
        //}

        private void loadingVacuumFile()
        {
            try
            {
                SystemLogger.Log(Level.Debug, "loadingVacuumFile");
                //string inPath = @"C:\LGCNS.ezControl\FactovaControl_Release_L_2.0.9.1(21_01_25)\EDM_(일반진행)LogExtract_전Unit_20210513_v0.1.xlsx";

                string inPath = @"..\FactovaControl_Release_L_2.0.9.1(21_01_25)\LOG_전Unit_진공도_20210601.xlsx";

                List<Dictionary<string, object>> rows;
                Dictionary<string, object> row;
                _JsonLogData = new List<JObject>();

                DataTable dt = ReadDataExcel(inPath);

                foreach (DataRow dr in dt.Rows)
                {
                    string SendJson = JsonConvert.SerializeObject(makeJsonForVacuum(dr)) + ";";
                    msgQVaccum.Enqueue(SendJson);
                }
            }
            catch (Exception ex)
            {
                SystemLogger.Log(ex);
            }
        }

        public void startVaccumTimer()
        {
//            loadingVacuumFile();
            //object msg = null;
            //_sendVaccumTimer = new Timer(sendVaccumData, msg, 0, 1000);

            //  t.Change(dueTime, 0);
        }

        public void stopVaccumTimer()
        {
            msgQVaccum.Clear();
            if (_sendVaccumTimer != null) _sendVaccumTimer.Dispose();
        }

        public void sendVaccumData(object msg)
        {
            if (msgQVaccum.Count > 0)
            {
                string sendMsg = msgQVaccum.Dequeue();

                if (sendMsg == null) return;

                //SystemLogger.Log(Level.Debug, " : JSonData := " + sendMsg.ToString());
                foreach (CSocketClient socketClint in _driver.Clients.Values)
                {
                    _driver.Send(socketClint, sendMsg.ToString() + ";");
                }
            }
        }

        private readonly object _Lock = new object();
        public void startLogTimer()
        {
            stopLogTimer();
            loadingLogFile();


            // Jump Count
            //if (msgQLog.Count != 0)
            //{
            //    DataRow[] drs = new DataRow[msgQLog.Count];
            //    drs = msgQLog.ToArray();
            //    msgQLog.Clear();

            //    //Jump 앞 구간 전송
            //    //lock (_Lock)
            //    //{
            //    //    for (int i = 0; i < _startCnt; i++)
            //    //    {
            //    //        sendLogData(drs[i]);
            //    //    }
            //    //}

            //    for (int i = _startCnt; i < drs.Length; i++)
            //    {
            //        msgQLog.Enqueue(drs[i]);
            //    }
            //}

            DataRow dr = msgQLog.Dequeue();

            if (dr != null)
            {
                sendLogData(dr);

                _sendLogTimer = new System.Timers.Timer();
                _sendLogTimer.Elapsed += _sendLogTimer_Elapsed;

                _sendLogTimer.Start();

                object msg = null;
            }

            CreateExcelFile(_logTable);
            //            _sendLogTimer = new Timer(sendLogData, msg, 0, 0);

            //  t.Change(dueTime, 0);
        }


        public void startJumpLogTimer()
        {
            DataRow dr = msgQLog.Dequeue();

            if (dr != null)
            {
                sendLogData(dr);

                _sendLogTimer = new System.Timers.Timer();
                _sendLogTimer.Elapsed += _sendLogTimer_Elapsed;

                _sendLogTimer.Start();

                object msg = null;
            }
        }



        public void stopLogTimer()
        {
            msgQLog.Clear();
            if (_sendLogTimer != null)
            {
                _sendLogTimer.Stop();
                _sendLogTimer.Dispose();
            }
        }


        public void sendFileInfo(CSocketClient socketClint)
        {

            JObject msg = makeJsonFileInfo();
            _driver.Send(socketClint, msg.ToString() + ";");
        }

        public void sendLogData(DataRow dr)
        {
            TimeSpan diffTtime = new TimeSpan();
            //foreach (DataRow dr in _logTable.Rows)
            //{
            //
            //if (msgQLog.Count > 0)
            //{
            //DataRow dr = msgQLog.Dequeue();

            if (dr == null) return;

            System.Reflection.FieldInfo fInfo = dr.GetType().GetField("_rowID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            int rowID = Convert.ToInt32(fInfo.GetValue(dr));
            rowID = rowID - 1;


            
            //diffTtime = TimeSpan.Parse(dr["DIFF_TIME"].ToString());

            //if (diffTtime.Ticks > 0)
            //    //                  Thread.Sleep(Convert.ToInt32(diffTtime.TotalMilliseconds));
            //    this.Wait_Sleep(Convert.ToInt32(diffTtime.TotalMilliseconds));

            string sendJson = JsonConvert.SerializeObject(makeJsonForAction_IO(dr)) + ";";
            //sendJson = sendJson.Replace("\\r\\n", "");
            //sendJson = sendJson.Replace("\\","");
            //sendJson = sendJson.Remove(sendJson.IndexOf("[") - 1,1);
            //sendJson = sendJson.Remove(sendJson.IndexOf("]") + 1,1);
            //sendJson = sendJson.Trim();

            //SystemLogger.Log(Level.Debug, rowID.ToString() + " : JSonData := " + sendJson);

            foreach (CSocketClient socketClint in _driver.Clients.Values)
            {
                _driver.Send(socketClint, sendJson);
                //SystemLogger.Log(Level.Info, "sendData := " + sendJson);
            }
            //}

        }
        public DataTable ReadDataExcel(string filepath)
        {
            try
            {

                FileStream stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                DataSet result = excelReader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true
                    }
                });

                DataTable dt = new DataTable();
                dt = result.Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                SystemLogger.Log(Level.Exception,ex.Message);
                return null;
            }
        }

        public void CreateExcelFile(DataTable dt)
        {
           // string FileName = $"엑셀내보내기_"+_selectedFileName+".xlsx";


            /*Set up work book, work sheets, and excel application*/
            Excel.Application oexcel = new Excel.Application();
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                object misValue = System.Reflection.Missing.Value;
                Excel.Workbook obook = oexcel.Workbooks.Add(misValue);
                Excel.Worksheet osheet = new Excel.Worksheet();

                //  obook.Worksheets.Add(misValue);
                osheet = (Excel.Worksheet)obook.Sheets["Sheet1"];
                int colIndex = 0;
                int rowIndex = 1;

                foreach (DataColumn dc in dt.Columns)
                {
                    colIndex++;
                    osheet.Cells[1, colIndex] = dc.ColumnName;
                }
                foreach (DataRow dr in dt.Rows)
                {
                    rowIndex++;
                    colIndex = 0;

                    foreach (DataColumn dc in dt.Columns)
                    {
                        colIndex++;
                        osheet.Cells[rowIndex, colIndex] = dr[dc.ColumnName];
                    }
                }

                osheet.Columns.AutoFit();

                string filepath = this.Variables["exportFilePath"].AsString + _selectedFileName + "ADD Value6.csv";

                //Release and terminate excel
                obook.SaveAs(filepath);
                obook.Close();
                oexcel.Quit();
                
                releaseObject(osheet);
                releaseObject(obook);
                releaseObject(oexcel);
                GC.Collect();
            }
            catch (Exception ex)
            {
                oexcel.Quit();
                SystemLogger.Log(Level.Exception, "CreateExcelFile Exception" + ex.Message);
            }
        }

        private void releaseObject(object o) 
        { 
            try 
            { 
                while 
                    (System.Runtime.InteropServices.Marshal.ReleaseComObject(o) > 0) 
                { }
            } 
            catch { } 
            finally { o = null; }
        }

        private void DTIOServer_SendDataOnValueChanged(object sender, object value)
        {
            if ((Boolean)value)
            { 
                foreach (CSocketClient socketClint in _driver.Clients.Values)
                {
                    foreach (var item in _JsonLogData)
                    {
                        _driver.Send(socketClint, item.ToString() + ";");
                    }
                }
            }
        }

        private void DTIOServer_OnValueChanged(object sender, object value)
        {

        }

        private string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        private JObject makeJsonForAction_IO(DataRow dr)
        {
            JArray jAction = new JArray();

            Dictionary<string, object>  row = new Dictionary<string, object>();

            foreach (DataColumn col in dr.Table.Columns)
            {
                if (col.ColumnName != "TIME" && col.ColumnName != "UNIT" && col.ColumnName != "TYPE" && col.ColumnName != "DIFF_TIME")
                {
                    if (col.ColumnName == "VALUE1") dr[col] = dr[col].ToString();
                    row.Add(col.ColumnName, dr[col]);
                }
            }
            jAction.Add(JObject.Parse(JsonConvert.SerializeObject(row)));
            JObject jeqp = new JObject();
            jeqp.Add("EQPID", dr["UNIT"].ToString());
            jeqp.Add(dr["TYPE"].ToString(), jAction);

            JObject jmsg = new JObject();
            jmsg.Add("EEER",jeqp);

            return jmsg;
        }

        private JObject makeJsonForAction_IO_JUMP()
        {
            JObject jAction = new JObject();
            JArray jeeerArray = new JArray();

            if (msgQLog.Count != 0)
            {
                for (int i = 0; i < _startCnt; i++)
                {
                    DataRow dr = msgQLog.Dequeue();

                    JObject jeeer = new JObject();
                    jeeer = makeJsonForAction_IO(dr);
                    jeeerArray.Add(jeeer);
                }
            }
            JObject jjump = new JObject();
            jjump.Add("JUMP", jeeerArray);

            JObject jmsg = new JObject();
            jmsg.Add("ECMD", jjump);

            return jmsg;
        }


        private JObject makeJsonForVacuum(DataRow dr)
        {

            JObject jVacuum = new JObject();

            Dictionary<string, object> row = new Dictionary<string, object>();

            foreach (DataColumn col in dr.Table.Columns)
            {
                row.Add(col.ColumnName, dr[col]);
            }

            jVacuum = JObject.Parse(JsonConvert.SerializeObject(row));

            return jVacuum;
        }

        private JObject makeJsonFileInfo()
        {
            JArray fileList = new JArray();
            foreach (FileInfo item in _fileList)
            {
                JObject fileInfo = new JObject();
                fileInfo.Add("NAME", item.Name);
                fileInfo.Add("VALUE1", item.Type);
                fileInfo.Add("VALUE2", item.rowCount.ToString());

                fileList.Add(fileInfo);
            }

            JObject file = new JObject();
            file.Add("FILE", fileList);

            JObject eeer = new JObject();
            eeer.Add("EEER", file);

            return eeer;        
        }



        private JObject makeJsonForReplay(string cmd)
        {
            JObject jeqp = new JObject();
            jeqp.Add("EQPID", "TM4");
            jeqp.Add("REPLAY", "OK");
            jeqp.Add("RCMD", cmd);
            jeqp.Add("TYPE", "LOG");

            JObject jmsg = new JObject();
            jmsg.Add("ECMD", jeqp);

            return jmsg;
        }

        private void _driver_OnReceived1(CSocketServer driver, CSocketClient client, string strMessage)
        {
            try
            {
                string[] strMessageList = strMessage.Split(';');

                //Command Start, Stop
                string output;
                foreach (var item in strMessageList)
                {
                    if (item == "") continue; 

                    string message = item.ToString().Trim();

                    //ECMD
                    var jo = JObject.Parse(item);
                    var firstNode = jo.Properties().First();
                    var secondNode = (JProperty)firstNode.Value.First();
                    if (firstNode.Name.ToString() == "ECMD")
                    {

                        ECMD_lv1 ecmd = JsonConvert.DeserializeObject<ECMD_lv1>(item);

                        if (ecmd.ECMD.CONFIG != null)
                        {
                            foreach (var config in ecmd.ECMD.CONFIG)
                            {
                                if (config.SPEED != null)
                                {
                                    _speed = Convert.ToDouble(config.SPEED);
                                    this.Variables["Speed"].AsDouble = _speed;
                                }

                                if (config.TYPE == "LOG")
                                {
                                    if (config.RCMD == "START")
                                    {
                                        JObject msgReplay = makeJsonForReplay("START");


                                        this.Variables["Monitoring"].AsBoolean = false;

                                        //send Replay
                                        foreach (CSocketClient socketClint in driver.Clients.Values)
                                        {
                                            if (socketClint.Id == client.Id)
                                                driver.Send(socketClint, msgReplay.ToString() + ";");
                                        }

                                        if (config.TIME != null && config.TIME != "" && config.TIME.ToString() != "1")
                                        {
                                            // JUMP
                                            stopLogTimer();
                                            loadingLogFile();
                                            _startCnt = Convert.ToInt32(config.TIME.ToString());

                                            JObject msg = makeJsonForAction_IO_JUMP();

                                            //send JUMP
                                            foreach (CSocketClient socketClint in driver.Clients.Values)
                                            {
                                                if (socketClint.Id == client.Id)
                                                    driver.Send(socketClint, msg.ToString() + ";");
                                            }

                                            startJumpLogTimer();
                                            //startVaccumTimer();

                                            return;

                                        }

                                        //startVaccumTimer();
                                        startLogTimer();

                                        return;
                                    }
                                    else if (config.RCMD == "STOP")
                                    {
                                        JObject msg = makeJsonForReplay("STOP");

                                        this.Variables["Monitoring"].AsBoolean = true;

                                        //send Replay
                                        foreach (CSocketClient socketClint in driver.Clients.Values)
                                        {
                                            if (socketClint.Id == client.Id)
                                                driver.Send(socketClint, msg.ToString() + ";");
                                        }
                                        stopVaccumTimer();
                                        stopLogTimer();

                                        return;
                                    }

                                    //JUMP Start
                                    //else if (config.RCMD == "JUMP")
                                    //{
                                    //    JObject msgReplay = makeJsonForReplay("JUMP");

                                    //    //send Replay
                                    //    foreach (CSocketClient socketClint in driver.Clients.Values)
                                    //    {
                                    //        if (socketClint.Id == client.Id)
                                    //            driver.Send(socketClint, msgReplay.ToString() + ";");
                                    //    }


                                    //    if (config.TIME != null && config.TIME != "")
                                    //    {
                                    //        _startCnt = Convert.ToInt32(config.TIME.ToString());
                                    //    }

                                    //    loadingLogFile();
                                    //    JObject msg = makeJsonForAction_IO_JUMP();                                       

                                    //    //send Replay
                                    //    foreach (CSocketClient socketClint in driver.Clients.Values)
                                    //    {
                                    //        if (socketClint.Id == client.Id)
                                    //            driver.Send(socketClint, msg.ToString() + ";");
                                    //    }

                                    //    startVaccumTimer();
                                    //    startLogTimer();

                                    //    return;
                                    //}
                                }
                                else if (config.TYPE == "MON")
                                {
                                    if (config.RCMD == "START")
                                    {
                                        this.Variables["Monitoring"].AsBoolean = true;


                                        foreach (CSocketClient socketClint in driver.Clients.Values)
                                        {
                                            if (socketClint.Id != client.Id)
                                            {
                                                driver.Send(socketClint, message.ToString() + ";");
                                            }
                                        }

                                        //stopVaccumTimer();
                                        //stopLogTimer();
                                    }
                                    else if (config.RCMD == "STOP")
                                    {
                                        this.Variables["Monitoring"].AsBoolean = false;

                                        foreach (CSocketClient socketClint in driver.Clients.Values)
                                        {
                                            if (socketClint.Id != client.Id)
                                            {
                                                driver.Send(socketClint, message.ToString() + ";");
                                            }
                                        }

                                    }
                                }
                                // response from Simulation
                                else
                                {
                                    foreach (CSocketClient socketClint in driver.Clients.Values)
                                    {
                                        if (socketClint.Id != client.Id)
                                        {
                                            driver.Send(socketClint, message.ToString() + ";");
                                        }
                                    }
                                }
                            }
                        }
                        //replay Send
                        else
                        {
                            foreach (CSocketClient socketClint in driver.Clients.Values)
                            {
                                if (socketClint.Id != client.Id)
                                {
                                    driver.Send(socketClint, message.ToString() + ";");
                                }
                            }

                        }
                    }
                    //EEER : File Info
                    else if (secondNode.Name == "FILE")
                    {
                        EEER_lv1 EEER = JsonConvert.DeserializeObject<EEER_lv1>(item);
                        _selectedFileName = EEER.EEER.FILE.NAME;
                    }
                    //EEER
                    else
                    {
                        foreach (CSocketClient socketClint in driver.Clients.Values)
                        {
                            if (socketClint.Id != client.Id)
                            {
                                driver.Send(socketClint, message.ToString() + ";");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLogger.Log(Level.Exception, "socketOn: undelivered " + ex.Message);
            }
        }

        private void _driver_ConnectionStateChanged(CDriver driver, LGCNS.ezControl.Common.enumConnectionState connectionState)
        {
            this.Variables["CONNECTION_STATE"].AsString = connectionState.ToString();
        }
