using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using System.Xml;


namespace ProcessNRAmsg
{
    public partial class FormMain : Form
    {
        private static string TaxTerminalConnectionString = "";

        public enum eventType
        {
            Z,
            Zpurge,
            Zunreg,
            X,
            ReSend,
            Test,
            AppError
        }

        class LogData
        {
            public DateTime eventDateTime;
            public eventType evnt;
            public string IasutdID;
            public string evntAddInfo;
            public int ResponseCode;
            public string ResponseStr;
        }


        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {   // Стартира обработката на съобщения

            try
            {
                if (setSQLservers() == false)
                {
                    Close();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Проблем с правата на NetFramework! Обадете се на администратора!", "Грешка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            processTasks();

            Close();
            Application.Exit();
            Environment.Exit(0);
        }

        private bool setSQLservers()
        {   // Установява SQL-сървърите
            try
            {
                string SqlServerNameTaxTerminal = "";
                try
                {
                    StreamReader ln = new StreamReader("DB_server.cfg");
                    SqlServerNameTaxTerminal = ln.ReadLine();
                    ln.Close();
                }
                catch
                {
                    MessageBox.Show("Липсва файлът DB_server.cfg или файлът не е с правилната структура!", "Грешка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (String.IsNullOrEmpty(SqlServerNameTaxTerminal))
                {
                    MessageBox.Show("Файлът DB_server.cfg е повреден!", "Грешка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    string SqlConnectionString;
                    SqlConnectionString = "User ID=FPNAPUSER;Password=SM0905pk;Initial Catalog=TaxTerminal;Data Source=" + SqlServerNameTaxTerminal;
                    TaxTerminalConnectionString = SqlConnectionString;
                }
                catch
                {
                    MessageBox.Show("Грешка при установяване имената на SQL-сървърите!", "Грешка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Има ли връзка със SQL-сървъра
                SqlConnection TaxTerminalConnection;
                try
                {
                    TaxTerminalConnection = new SqlConnection(TaxTerminalConnectionString);
                    TaxTerminalConnection.Open();
                    TaxTerminalConnection.Close();
                }
                catch
                {
                    MessageBox.Show("Няма връзка с SQL-сървъра " + SqlServerNameTaxTerminal + "!", "Грешка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }
            catch
            {
                MessageBox.Show("Wrong NetFramework rights!", "Грешка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void processTasks()
        {   // Обработка на съобщенията от/към НАП

            MySqlConn conn = new MySqlConn(TaxTerminalConnectionString);
            DataTable dt = null;
            long loopCounter = 0L;
            DateTime startDT, endDT;
            TimeSpan interval;
            while (checkBoxClose.Checked == false)
            {
                startDT = DateTime.Now;
                try
                {
                    dt = conn.ExecSProcDS("procGetActualTasks").Tables[0];
                }
                catch
                {
                    addLogData(eventType.AppError, "exeption", "procGetActualTasks", -1, "");
                    continue;
                }
                foreach(DataRow r in dt.Rows)
                {
                    int taskType = Convert.ToInt32(r["TaskType"].ToString());
                    int shopID = Convert.ToInt32(r["shopID"].ToString());
                    long recNo = Convert.ToInt64(r["RecNo"].ToString());
                    string taskID = r["TaskID"].ToString();
                    string iasutdID = r["FDIN"].ToString();
                    string fdrID = r["FDRID"].ToString();
                    string imsi = r["IMSI"].ToString();
                    string msisdn = r["MSISDN"].ToString();
                    switch (taskType)
                    {
                        case 1:   // Стандартна Z-задача
                            doZstdTask(shopID, taskID, fdrID, iasutdID, imsi, taskID);
                            break;
                        case 3:   // Z-Purge задача
                            doZpurgeTask(shopID, taskID, fdrID, iasutdID, imsi, taskID);
                            break;
                        case 4:   // X-отчет
                            break;
                        case 5:   // Z-задача при дерегистрация
                            doZunregTask(shopID, taskID, fdrID, iasutdID, imsi, taskID);
                            break;
                        case 100: // Повторно изпращане на файл
                            break;
                        case 200: // Проверка, дали програмата работи
                            doTestTask(recNo);
                            break;
                    }
                }

                loopCounter++;
                if (loopCounter == Int64.MaxValue) loopCounter = 0L;
                labelLoopCounter.Text = loopCounter.ToString();
                do
                {
                    Application.DoEvents();
                    endDT = DateTime.Now;
                    interval = endDT - startDT;
                } while (interval.Seconds < 5);

            }
        }

        private void doTestTask(long recNo)
        {   // Обработва тестовата задача (200)
            MySqlConn conn = new MySqlConn(TaxTerminalConnectionString);
            try
            {
                conn.ExecSProc("procDelOneTask", recNo);
                addLogData(eventType.Test, "Info", "Програмата работи нормално", -1, "");
            }
            catch
            {
                addLogData(eventType.AppError, "exeption", "procDelOneTask", -1, "");
            }
        }

        private void doZstdTask(int shopID, string taskID, string fdrID, string iasutdID, string imsi, string rzptID)
        {   // Обработва стандартна Z-задача

            MySqlConn conn = new MySqlConn(TaxTerminalConnectionString);
            DataTable dt = null;
            try
            {
                dt = conn.ExecSProcDS("procGetZreports2send", shopID).Tables[0];
            }
            catch
            {
                addLogData(eventType.AppError, "exeption", "procGetZreports2send", -1, "");
            }
            if (dt.Rows.Count < 1) return;

            string logStr = "", xmlStr = "";
            if (fill_z_data(0, taskID, fdrID, iasutdID, imsi, rzptID, dt, out logStr, out xmlStr) == false)
            {
                addLogData(eventType.AppError, "error", "fill_z_data", -2, "");
                return;
            }

            addLogData(eventType.Z, iasutdID, logStr, 0, "OK");
        }

        private void doZpurgeTask(int shopID, string taskID, string fdrID, string iasutdID, string imsi, string rzptID)
        {   // Обработва Zpurge-задача

            MySqlConn conn = new MySqlConn(TaxTerminalConnectionString);
            DataTable dt = null;
            try
            {
                dt = conn.ExecSProcDS("procGetZreports2send", shopID).Tables[0];
            }
            catch
            {
                addLogData(eventType.AppError, "exeption", "procGetZreports2send", -1, "");
            }
            if (dt.Rows.Count < 1) return;

            string logStr = "", xmlStr = "";
            if (fill_z_data(1, taskID, fdrID, iasutdID, imsi, rzptID, dt, out logStr, out xmlStr) == false)
            {
                addLogData(eventType.AppError, "error", "fill_z_data", -2, "");
                return;
            }

            addLogData(eventType.Zpurge, iasutdID, logStr, 0, "OK");
        }

        private void doZunregTask(int shopID, string taskID, string fdrID, string iasutdID, string imsi, string rzptID)
        {   // Обработва Zpurge-задача

            MySqlConn conn = new MySqlConn(TaxTerminalConnectionString);
            DataTable dt = null;
            try
            {
                dt = conn.ExecSProcDS("procGetZreports2send", shopID).Tables[0];
            }
            catch
            {
                addLogData(eventType.AppError, "exeption", "procGetZreports2send", -1, "");
            }
            if (dt.Rows.Count < 1) return;

            string logStr = "", xmlStr = "";
            if (fill_z_data(2, taskID, fdrID, iasutdID, imsi, rzptID, dt, out logStr, out xmlStr) == false)
            {
                addLogData(eventType.AppError, "error", "fill_z_data", -2, "");
                return;
            }

            addLogData(eventType.Zunreg, iasutdID, logStr, 0, "OK");
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            labelMsg.Visible = true;
        }

        private string DateTime2NAPstr(DateTime dt)
        {   // Връща дата/час като стринг във формата, изискван от НАП
            string dtStr = string.Format("{0:00}.{1:00}.{2:0000}T{3:00}:{4:00}:{5:00}", dt.Day, dt.Month, dt.Year, dt.Hour, dt.Minute, dt.Second);
            return dtStr;
        }

        private XmlNode AddXmlNode(XmlDocument doc, string NodeName, string NodeValue)
        {
            XmlNode NewNode = doc.CreateNode(XmlNodeType.Element, NodeName, doc.DocumentElement.NamespaceURI);
            NewNode.InnerText = NodeValue;
            return NewNode;
        }

        private void addLogData(eventType evnt,
    string IASUTDIDstring,                  // IASUTDID
    string taskLogInfo,                     // Информация за задачата, която да се запише в LOG-a
    int responceCode,                       // ResponceCode from NRA
    string responceStr)                     // Response string from NRA
        {
            LogData ld = new LogData();
            ld.eventDateTime = DateTime.Now;
            ld.IasutdID = IASUTDIDstring;
            ld.ResponseCode = responceCode;
            ld.ResponseStr = responceStr;
            ld.evnt = evnt;
            ld.evntAddInfo = taskLogInfo.Trim();

            try
            {
                dataGridViewLog.Rows.Add(ld.eventDateTime.ToString("dd.MM.yyyy HH:mm:ss"), ld.IasutdID, ld.evnt.ToString(), ld.evntAddInfo, ld.ResponseCode, ld.ResponseStr);
                dataGridViewLog.CurrentCell = dataGridViewLog[0, dataGridViewLog.RowCount - 1];
                while (dataGridViewLog.RowCount > 100) dataGridViewLog.Rows.RemoveAt(0);
            }
            catch { }

            try
            {
                using (StreamWriter sw = File.AppendText("NRA.LOG"))
                {
                    sw.WriteLine(ld.eventDateTime.ToString("dd.MM.yyyy HH:mm:ss") + ";" +
                        ld.IasutdID + ";" +
                        ld.evnt.ToString() + ";" +
                        ld.evntAddInfo + ";" +
                        ld.ResponseCode.ToString() + ";" +
                        ld.ResponseStr);
                }
            }
            catch { }
            Application.DoEvents();
        }

        private bool fill_z_data(int z_type,        // 0 = Стандартна Z-задача, 1 = Zpurge-задача, 2 = Z-задача при дерегистрация
            string TIDstring,                       // TID
            string FDRIDstring,                     // FDRID
            string IASUTDIDstring,                  // IASUTDID
            string IMSIstring,                      // IMSI
            string RZPTIDstring,                    // RZPTID

            DataTable ZRdata,                       // Данни за отчетите (от таблицата [TaxTerminal].[dbo].[Z_Reports])

            out string taskLogInfo,                 // Информация за задачата, която да се запише в LOG-a
            out string xmlString)                   // Попълнен XML, като стринг
        {   // Попълва XML за Z-отчет. Връща: true=OK, False=грешка
            xmlString = "";
            taskLogInfo = "";

            try
            {
                XmlDocument doc = new XmlDocument();
                switch (z_type)
                {
                    case 0: doc.Load("Z_Task.xml"); break;
                    case 1: doc.Load("ZPurge_Task.xml"); break;
                    case 2: doc.Load("ZUnreg_Task.xml"); break;
                    default: return false;
                }

                doc.GetElementsByTagName("TID").Item(0).InnerText = TIDstring;
                doc.GetElementsByTagName("FDRID").Item(0).InnerText = FDRIDstring;
                doc.GetElementsByTagName("IASUTDID").Item(0).InnerText = IASUTDIDstring;
                doc.GetElementsByTagName("IMSI").Item(0).InnerText = IMSIstring;
                doc.GetElementsByTagName("SD").Item(0).InnerText = DateTime2NAPstr(DateTime.Now);
                doc.GetElementsByTagName("RC").Item(0).InnerText = z_type.ToString();
                if (z_type == 1) doc.GetElementsByTagName("RZPTID").Item(0).InnerText = RZPTIDstring;

                long ln;        // Bigint DB fields
                DateTime dt;    // DateTime DB fields
                int n;          // Int DB fields
                decimal d;      // Decimal DB fields
                string s;       // Varchar DB fields

                int zReportNum = 0;
                foreach (DataRow r in ZRdata.Rows)
                {
                    doc.GetElementsByTagName("ZTIAS").Item(0).AppendChild(AddXmlNode(doc, "ZR", ""));

                    ln = Convert.ToInt64(r["ReportNumber"].ToString());
                    doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "ZC", ln.ToString()));
                    taskLogInfo += ("ZC=" + ln.ToString() + " ");

                    dt = Convert.ToDateTime(r["ReportCreatedDT"].ToString());
                    doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "ZD", DateTime2NAPstr(dt)));

                    n = Convert.ToInt32(r["CountNC"].ToString());
                    doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "NC", n.ToString()));

                    n = Convert.ToInt32(r["CountND"].ToString());
                    if (n > 0)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "ND", n.ToString()));
                    }

                    d = Convert.ToDecimal(r["SumSD"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "SD", d.ToString("N2")));
                    }

                    n = Convert.ToInt32(r["CountNcorr"].ToString());
                    if (n > 0)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "NCorr", n.ToString()));
                    }

                    d = Convert.ToDecimal(r["SumSCorr"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "SCorr", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumScash"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "SCash", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSChecks"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "SChecks", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumST"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "ST", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSOT"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "SOT", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSP"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "SP", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSSelf"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "SSelf", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSDmg"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "SDmg", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSCards"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "SCards", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSW"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "SW", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSR1"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "SR1", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSR2"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "SR2", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTA"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "TA", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTB"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "TB", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTV"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "TV", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTG"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "TG", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTD"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "TD", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTE"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "TE", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTJ"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "TJ", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTZ"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "TZ", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumDT"].ToString());
                    doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "DT", d.ToString("N2")));

                    n = Convert.ToInt32(r["klenID"].ToString());
                    doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "KNo", n.ToString()));

                    ln = Convert.ToInt64(r["LastBonNumber"].ToString());
                    doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "LCN", ln.ToString()));

                    ln = Convert.ToInt64(r["ReportBonNumber"].ToString());
                    doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "ChN", ln.ToString()));

                    s = r["SHA1_CS"].ToString();
                    doc.GetElementsByTagName("ZR").Item(zReportNum).AppendChild(AddXmlNode(doc, "CS", s));

                    zReportNum++;
                }

                xmlString = doc.InnerXml;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool fill_x_data(string TIDstring,  // TID
            string FDRIDstring,                     // FDRID
            string IASUTDIDstring,                  // IASUTDID
            string IMSIstring,                      // IMSI

            DataTable XRdata,                       // Данни за отчетите (от таблицата [TaxTerminal].[dbo].[X_Reports])

            out string taskLogInfo,                 // Информация за задачата, която да се запише в LOG-a
            out string xmlString)                   // Попълнен XML, като стринг
        {   // Попълва XML за X-отчет. Връща: true=OK, False=грешка
            xmlString = "";
            taskLogInfo = "";

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("X_Task.xml");

                doc.GetElementsByTagName("TID").Item(0).InnerText = TIDstring;
                doc.GetElementsByTagName("FDRID").Item(0).InnerText = FDRIDstring;
                doc.GetElementsByTagName("IASUTDID").Item(0).InnerText = IASUTDIDstring;
                doc.GetElementsByTagName("IMSI").Item(0).InnerText = IMSIstring;
                doc.GetElementsByTagName("SD").Item(0).InnerText = DateTime2NAPstr(DateTime.Now);
                doc.GetElementsByTagName("RC").Item(0).InnerText = "0";

                long ln;        // Bigint DB fields
                DateTime dt;    // DateTime DB fields
                int n;          // Int DB fields
                decimal d;      // Decimal DB fields

                int xReportNum = 0;
                foreach (DataRow r in XRdata.Rows)
                {
                    doc.GetElementsByTagName("XTIAS").Item(0).AppendChild(AddXmlNode(doc, "XR", ""));

                    ln = Convert.ToInt64(r["ReportNumber"].ToString());
                    doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "XC", ln.ToString()));
                    taskLogInfo += (ln.ToString() + " ");

                    dt = Convert.ToDateTime(r["ReportCreatedDT"].ToString());
                    doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "XD", DateTime2NAPstr(dt)));

                    n = Convert.ToInt32(r["CountNC"].ToString());
                    doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "NC", n.ToString()));

                    n = Convert.ToInt32(r["CountND"].ToString());
                    if (n > 0)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "ND", n.ToString()));
                    }

                    d = Convert.ToDecimal(r["SumSD"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "SD", d.ToString("N2")));
                    }

                    n = Convert.ToInt32(r["CountNcorr"].ToString());
                    if (n > 0)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "NCorr", n.ToString()));
                    }

                    d = Convert.ToDecimal(r["SumSCorr"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "SCorr", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumScash"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "SCash", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSChecks"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "SChecks", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumST"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "ST", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSOT"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "SOT", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSP"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "SP", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSSelf"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "SSelf", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSDmg"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "SDmg", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSCards"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "SCards", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSW"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "SW", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSR1"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "SR1", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumSR2"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "SR2", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTA"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "TA", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTB"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "TB", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTV"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "TV", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTG"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "TG", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTD"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "TD", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTE"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "TE", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTJ"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "TJ", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumTZ"].ToString());
                    if (d > 0M)
                    {
                        doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "TZ", d.ToString("N2")));
                    }

                    d = Convert.ToDecimal(r["SumDT"].ToString());
                    doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "DT", d.ToString("N2")));

                    n = Convert.ToInt32(r["klenID"].ToString());
                    doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "KNo", n.ToString()));

                    ln = Convert.ToInt64(r["LastBonNumber"].ToString());
                    doc.GetElementsByTagName("XR").Item(xReportNum).AppendChild(AddXmlNode(doc, "LCN", ln.ToString()));

                    xReportNum++;
                }

                xmlString = doc.InnerXml;
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
