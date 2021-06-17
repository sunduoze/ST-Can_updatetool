using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
//using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

using System.Threading;
using System.Data.OleDb;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;
//using pdaTraceInfoPlat.beans;

/*------------兼容ZLG的数据类型---------------------------------*/

//1.ZLGCAN系列接口卡信息的数据类型。
public struct VCI_BOARD_INFO 
{ 
	public UInt16 hw_Version;
    public UInt16 fw_Version;
    public UInt16 dr_Version;
    public UInt16 in_Version;
    public UInt16 irq_Num;
    public byte   can_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst=20)] public byte []str_Serial_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
    public byte[] str_hw_Type;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Reserved;
}

/////////////////////////////////////////////////////
//2.定义CAN信息帧的数据类型。
unsafe public struct VCI_CAN_OBJ  //使用不安全代码
{
    public uint ID;
    public uint TimeStamp;        //时间标识
    public byte TimeFlag;         //是否使用时间标识
    public byte SendType;         //发送标志。保留，未用
    public byte RemoteFlag;       //是否是远程帧
    public byte ExternFlag;       //是否是扩展帧
    public byte DataLen;          //数据长度
    public fixed byte Data[8];    //数据
    public fixed byte Reserved[3];//保留位

}

//3.定义初始化CAN的数据类型
public struct VCI_INIT_CONFIG 
{
    public UInt32 AccCode;
    public UInt32 AccMask;
    public UInt32 Reserved;
    public byte Filter;   //0或1接收所有帧。2标准帧滤波，3是扩展帧滤波。
    public byte Timing0;  //波特率参数，具体配置，请查看二次开发库函数说明书。
    public byte Timing1;
    public byte Mode;     //模式，0表示正常模式，1表示只听模式,2自测模式
}

/*------------其他数据结构描述---------------------------------*/
//4.USB-CAN总线适配器板卡信息的数据类型1，该类型为VCI_FindUsbDevice函数的返回参数。
public struct VCI_BOARD_INFO1
{
    public UInt16 hw_Version;
    public UInt16 fw_Version;
    public UInt16 dr_Version;
    public UInt16 in_Version;
    public UInt16 irq_Num;
    public byte can_Num;
    public byte Reserved;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)] public byte []str_Serial_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] str_hw_Type;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] str_Usb_Serial;
}

/*------------数据结构描述完成---------------------------------*/

public struct CHGDESIPANDPORT 
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public byte[] szpwd;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] szdesip;
    public Int32 desport;

    public void Init()
    {
        szpwd = new byte[10];
        szdesip = new byte[20];
    }
}

namespace CJQTest
{

       


    public struct Frame_Stu
    {
        public byte ctrl;
        public byte afn;
        public byte[] addr;
        public int datalen;
        public ArrayList dataArray;
    }

    public partial class MainFrame : Form
    {

        const int DEV_USBCAN = 3;
        const int DEV_USBCAN2 = 4;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DeviceType"></param>
        /// <param name="DeviceInd"></param>
        /// <param name="Reserved"></param>
        /// <returns></returns>
        /*------------兼容ZLG的函数描述---------------------------------*/
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_INIT_CONFIG pInitConfig);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref VCI_BOARD_INFO pInfo);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pSend, UInt32 Len);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);

        /*------------其他函数描述---------------------------------*/

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ConnectDevice(UInt32 DevType, UInt32 DevIndex);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_UsbDeviceReset(UInt32 DevType, UInt32 DevIndex, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_FindUsbDevice(ref VCI_BOARD_INFO1 pInfo);
        /*------------函数描述结束---------------------------------*/

        static UInt32 m_devtype = 4;//USBCAN2

        UInt32 m_bOpen = 0;
        UInt32 m_devind = 0;
        UInt32 m_canind = 0;

        VCI_CAN_OBJ[] m_recobj = new VCI_CAN_OBJ[1000];

        UInt32[] m_arrdevtype = new UInt32[20];

        Int32 m_recv_time_tick_100ms = 0;




        public string configFilePath = Application.StartupPath + "\\config.xls";
        public string testerFilePath = (Application.StartupPath).Remove(Application.StartupPath.Length - 4) + "\\tester.xls";
        public string telnetRebootPath = Application.StartupPath + "\\tool\\";

        public FrmConfig schemeConfigFrm;
        public FrmKeyboard paramConfig;
        public FrmSet frmSet;
        public PowkeySceneSet powkeySceneSet;
        public FrmRcuState rcuStateQuq;
        public DoorcardSceneSet doorcardSceneSet;
        public DoorSceneSet doorSceneSet;
        public IrdtSceneSet irdtSceneSet;
        public SceneCompose sceneSet;
        public Information information;
        public int testTotalNum = 0;
        public IPAddress HostIP;
        public IPEndPoint point;
        public Socket socket;
        public AREA m_areaId;
        public CGwRule GwRule;//国网规约对象
        public CGdRule GdRule;//广东规约对象
        public CRule Rule;
        public string logstemp;
        Thread thread; //测试进程
        byte[] spaddr = { 0x88, 0x88, 0x88, 0x88, 0x88, 0x88 };
        public int UpRecDataLen;
        public const int GW_PW_LEN = 16;
        public const int GX_PW_LEN = 2;


        public enum REC_RESULT
        {

            OK = 0x00, // 确认帧
            TYPE2_OK = 0x95,// 确认帧
            ERROR = 0xc0, // 错误帧
            TIME_OUT = 0xd0//超时帧
        }

        public enum TEST_RESULT
        {
            PASS = 0x01,
            FAIL,
            COM_FAIL
        }

        public enum GW_AFN_CODE
        {
            OK = 0x00,
            SetClock = 0x05,
            GetClock = 0x0C,
            GetClockOK = 0x05,
            GetRelData = 0x0C,
            Down = 0x20,
            Up = 0x30,
            Beat = 0x02,
            SetDeviceNo = 0xaa,
            GetDeviceAddr
        }

        public enum GD_AFN_CODE
        {
            GetRealtimeParam = 0x01,
            GetRealParamOK = 0x81,
            SetRealtimeParam = 0x08,
            AddCJQ = 0x08,
            AddMeter = 0x08,
            AddCJQOK = 0x88,
            SetRealParamOK = 0x88,
            ReadRealTimeData = 0x11,
            ReadRealTimeDataOK = 0x91
        }

        public enum GX_AFN_CODE
        {
            OK = 0x00,
            SetClock = 0x04,
            GetClock = 0x0A,
            GetRelData = 0x8C,
            Down = 0x20,
            Up = 0x30,
            Beat = 0x02,
            SetDeviceNo = 0xaa,
            GetDeviceAddr
        }

        /// <summary>
        /// 错误编码
        /// </summary>
        public enum ErrorCode
        {
            OK = 0,
            PW_ERROR = 1,
            NO_VALID_DATA = 2,
            VERSION_CHANGE = 3,
            METER_NO_DUPLICATE = 4,
            METER_ADDR_DUPLICATE = 5,
            TERM_NO_DUPLICATE = 6,
        }
        public enum TESTID
        {
            TS_CLOCK = 1,
            TS_BAT,
            TS_4851,
            TS_4852,
            TS_4853,
            TS_DOWN,
            TS_GPRS,
            TS_WATCHDOG,
            TS_REDWIRE,
            TS_NONE = 0XFF
        }

        public enum AREA
        {
            AREA_CQ = 0,
            AREA_GD,
            AREA_GX,
            AREA_MAX
        }

        public struct IPPORT_SET
        {
            public string ipaddr;
            public string port;
        }
        public struct PORT_SET
        {
            public int portno;
            public int bps;
            public int stopbit;
            public int checkbit;
            public void GetCheckbit(string str)
            {
                if (str == "none")
                    checkbit = 0;
                else if (str == "odd")
                    checkbit = 1;
                else if (str == "even")
                    checkbit = 2;
            }

        }

        private UInt16 CRC16RTU(byte[] pszBuf, Int32 unLength)
        {
            UInt16 CRC = 0xffff;
            Int32 CRC_count;
            UInt16 VAL = 0x0001;
            for (CRC_count = 0; CRC_count < unLength; CRC_count++)
            {
                int i;

                CRC = (UInt16)(CRC ^ pszBuf[CRC_count]);

                for (i = 0; i < 8; i++)
                {

                    UInt16 TT;
                    TT = (UInt16)(CRC & 1);
                    CRC = (UInt16)(CRC >> 1);
                    CRC = (UInt16)(CRC & 0x7fff);
                    if (TT == 1)
                    {
                        CRC = (UInt16)(CRC ^ 0xa001);
                    }
                    CRC = (UInt16)(CRC & 0xffff);

                }
            }

            return CRC;
        }

        public struct testItem
        {
            public string testName;
            public int testID;
            public PORT_SET serialPort;
            public IPPORT_SET netPort;
            public string meterAddr;
            public int ModuleId;

            public int GetTestId(string str)
            {
                if (str == "系统时钟")
                    testID = (int)TESTID.TS_CLOCK;
                else if (str == "电池")
                    testID = (int)TESTID.TS_BAT;
                else if (str == "485-1")
                    testID = (int)TESTID.TS_4851;
                else if (str == "485-2")
                    testID = (int)TESTID.TS_4852;
                else if (str == "485-3")
                    testID = (int)TESTID.TS_4853;
                else if (str == "本地通讯模块")
                    testID = (int)TESTID.TS_DOWN;
                else if (str == "远程模块")
                    testID = (int)TESTID.TS_GPRS;
                else if (str == "看门狗")
                    testID = (int)TESTID.TS_WATCHDOG;
                else if (str == "远红外")
                    testID = (int)TESTID.TS_REDWIRE;
                else
                    testID = (int)TESTID.TS_NONE;
                return testID;
            }
        }

        bool bserialPortOpen = false;
      
        public testItem[] TestMember;
        public MainFrame()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false; // 这样一个线程可以调用父线程创建的控件


            //*******************88权限检测
            string authorFilePath = Application.StartupPath + "\\run.txt";
            if (File.Exists(authorFilePath) == false)
            {
                //                return;
            }

            string year = DateTime.Now.Year.ToString();

            byte[] date = new byte[7];
            while (true)
            {
                if (year.Length > 2)
                {
                    year = year.Remove(0, 1);
                }
                else
                {
                    break;
                }
            }

            date[5] = Convert.ToByte(year, 16);
            date[4] = Convert.ToByte(DateTime.Now.Month.ToString(), 16);
            //byte month = Convert.ToByte(GetWeekDay(DateTime.Now.DayOfWeek.ToString()));
            //month = (byte)(month << 5);
            //date[4] |= month;
            //month = date[4];
            date[3] = Convert.ToByte(DateTime.Now.Day.ToString(), 16);
            date[2] = Convert.ToByte(DateTime.Now.Hour.ToString(), 16);
            date[1] = Convert.ToByte(DateTime.Now.Minute.ToString(), 16);
            date[0] = Convert.ToByte(DateTime.Now.Second.ToString(), 16);
            //2018年4月1日过期
            /*if (date[5] >= 0x18 && date[4] >= 0x4 && date[3] >= 0x01)
            {
                File.Delete(authorFilePath);
                return;
            }*/
            //***************权限检测结束
            schemeConfigFrm = new FrmConfig(); // 创建配置窗口类型

            paramConfig = new FrmKeyboard();//创建档案配置窗口
            frmSet = new FrmSet();//创建设置参数窗口
            powkeySceneSet = new PowkeySceneSet();//总电源场景
            doorcardSceneSet = new DoorcardSceneSet();//门卡场景
            doorSceneSet = new DoorSceneSet();//房门场景设置
            irdtSceneSet = new IrdtSceneSet();//红外场景设置
            sceneSet = new SceneCompose();//普通场景
            rcuStateQuq = new FrmRcuState();//RCU设备状态查询

            paramConfig.cmbAirsub1Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbAirsub2Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbAirsub3Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbAirsub4Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK1Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK2Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK3Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK4Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK5Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK6Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK7Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK8Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK9Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK10Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK11Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK12Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK13Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK14Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK15Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK16Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK17Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK18Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK19Func.SelectedIndex = 0;//控件初始化
            paramConfig.cmbK20Func.SelectedIndex = 0;//控件初始化

            frmSet.cmbDoorCardType.SelectedIndex = 0;
            frmSet.cmbAirSeason.SelectedIndex = 0;
            frmSet.cmbDoorDisplaytype.SelectedIndex = 0;


            m_areaId = AREA.AREA_CQ;

            information = new Information(); // 创建通讯记录打印窗口
            GwRule = new CGwRule(); //创建国网规约对象
            GdRule = new CGdRule(); //创建广东规约对象
            //创建其他区域规约对象
            Rule = (CRule)GdRule;

            { ListViewItem item = new ListViewItem("温度"); item.SubItems.Add("");  this.testlv.Items.Add(item); }
            { ListViewItem item = new ListViewItem("电池组总电压"); item.SubItems.Add(""); this.testlv.Items.Add(item); }
            { ListViewItem item = new ListViewItem("实时电流"); item.SubItems.Add(""); this.testlv.Items.Add(item); }
            { ListViewItem item = new ListViewItem("SOC"); item.SubItems.Add(""); this.testlv.Items.Add(item); }
            { ListViewItem item = new ListViewItem("剩余容量"); item.SubItems.Add(""); this.testlv.Items.Add(item); }
            { ListViewItem item = new ListViewItem("保护故障"); item.SubItems.Add(""); this.testlv.Items.Add(item); }



            { ListViewItem item = new ListViewItem("最大放电电流"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            { ListViewItem item = new ListViewItem("最大充电电流"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            { ListViewItem item = new ListViewItem("最高温度"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            { ListViewItem item = new ListViewItem("最低温度"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            { ListViewItem item = new ListViewItem("单体电池最高电压"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            { ListViewItem item = new ListViewItem("单体电池最低电压"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            { ListViewItem item = new ListViewItem("电池循环次数"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            { ListViewItem item = new ListViewItem("放电SOC累计值"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            
            
        }


        private void config_Click(object sender, EventArgs e)
        {
            schemeConfigFrm.Visible = false;
            schemeConfigFrm.Focus();
            schemeConfigFrm.ShowDialog();
        }

        private void Start_Click(object sender, EventArgs e)
        {
            //LoaderConfigFile();
            // 创建数据处理线程
            //初始化系统硬件时间
            // SetSysInitTime();
            thread = new Thread(new ThreadStart(RunTest));
            thread.Start();
        }

        /// 接收数据缓冲器
        /// </summary>
        private ArrayList ReceivedDataBuff = new ArrayList();

        public ArrayList FrameBuff = new ArrayList();
       
        private void OnSerialReceivedData(object sender, System.EventArgs e)
        {
            System.Threading.Thread.Sleep(100);
            if (serialPort.BytesToRead > 0)
            {
                try
                {
                    byte[] data = new byte[serialPort.BytesToRead];
                    serialPort.Read(data, 0, data.Length);

                    logstemp = "\r\n接收：";
                    for (int i = 0; i < data.Length; i++)
                    {
                        logstemp += String.Format("{0:X2}", (byte)data[i]);
                        logstemp += " ";
                    }
                   // information.textBox1.Text += stemp;
                   // txtOutput.Show();
                    //tmrOutputDisplay.Enabled = true;
                    if (bTestWatchDog == true)
                    {
                        return;
                    }
                    ReceivedDataBuff.AddRange(data);
                }
                catch (SystemException er)
                {
                    MessageBox.Show(er.Message);
                }

                int startindex = 0, endindex = 0;
                Frame_Stu RuleInfo = new Frame_Stu();
                while (true)
                {
                   /* if (ReceivedDataBuff.Count == 0x12 && (byte)ReceivedDataBuff[4] == 0x68 && ((byte)ReceivedDataBuff[12] == 0x81 || (byte)ReceivedDataBuff[12] == 0x84) && (byte)ReceivedDataBuff[14] == 0x33 && (byte)ReceivedDataBuff[15] == 0x44)
                    {
                        RuleInfo.ctrl = 0x84;
                        FrameBuff.Add(RuleInfo);
                        ReceivedDataBuff.Clear();
                        return;
                    }*/
                    switch (Rule.ParsePackage(ref ReceivedDataBuff, ref RuleInfo, ref startindex, ref endindex))
                    {
                        case ParseResult.OK:
                            //ReceivedDataBuff.RemoveRange(0, endindex + 1);
                            //OnReceivedPackage(RuleInfo);
                            if (RuleInfo.afn != (byte)GW_AFN_CODE.Beat)
                            {
                                FrameBuff.Add(RuleInfo);    // 添加接受的数据帧到缓冲区
                            }
                            break;

                        case ParseResult.Waitting:
                            return;

                        case ParseResult.Error:
                            if (ReceivedDataBuff.Count > 0)
                            {
                                try
                                {
                                    ReceivedDataBuff.RemoveRange(0, endindex + 1);
                                }
                                catch (Exception ex)
                                {
                                    //MessageBox.Show(this,ex.Message.ToString());
                                }
                            }
                            return;
                        case ParseResult.Unintegrated:
                            return;
                        default:
                            return;
                    }
                }
            }

        }
        #region//进程
        private void OnNetReceivedData()
        {
            if (socket.Connected)
            {
                byte[] data = new byte[200];
                int recLen;
                while (true)
                {
                    try
                    {
                        recLen = socket.Receive(data, data.Length, 0);
                        if (recLen == 0)
                        {
                            continue;
                        }
                        byte[] recdata = new byte[recLen];
                        logstemp = "\r\n接收：";
                        for (int i = 0; i < recLen; i++)
                        {
                            recdata[i] = data[i];
                            logstemp += String.Format("{0:X2}", (byte)data[i]);
                            logstemp += " ";
                        }
                        //information.textBox1.Text += stemp;
                       // txtOutput.Text += stemp;
                        //tmrOutputDisplay.Enabled = true;
                        ReceivedDataBuff.AddRange(recdata);
                    }
                    catch (SystemException er)
                    {
                        MessageBox.Show(er.Message);
                    }

                    int startindex = 0, endindex = 0;
                    Frame_Stu RuleInfo = new Frame_Stu();
                    // while (true)
                    {
                        switch (Rule.ParsePackage(ref ReceivedDataBuff, ref RuleInfo, ref startindex, ref endindex))
                        {
                            case ParseResult.OK:
                                ReceivedDataBuff.RemoveRange(0, endindex + 1);
                                //OnReceivedPackage(RuleInfo);
                                if (RuleInfo.afn != (byte)GW_AFN_CODE.Beat)
                                {
                                    FrameBuff.Add(RuleInfo);    // 添加接受的数据帧到缓冲区
                                }
                                break;
                            case ParseResult.Waitting:
                                return;
                            case ParseResult.Error:
                                if (ReceivedDataBuff.Count > 0)
                                {
                                    try
                                    {
                                        ReceivedDataBuff.RemoveRange(0, endindex + 1);
                                    }
                                    catch (Exception ex)
                                    {
                                        //MessageBox.Show(this,ex.Message.ToString());
                                    }
                                }
                                return;
                            case ParseResult.Unintegrated:
                                return;
                            default:
                                return;
                        }
                    }
                }
            }


        }
        #endregion


        private REC_RESULT SendFrame(byte[] addr, byte ctl, byte[] data, ref Frame_Stu frame, int timeOutS, int retryTimes)
        {
            ArrayList sendframe = new ArrayList();
            int i;
            Rule.Makeframes(ref sendframe, addr, ctl, data);
            for (i = 0; i < retryTimes; i++)
            {
                sendDatas(sendframe);
                if (GetFrame(ref frame, timeOutS) == true)
                {
                    break;
                }
            }

            if (i == retryTimes)
            {
                return REC_RESULT.TIME_OUT;
            }
            if (m_areaId == AREA.AREA_GD)
            {
                if (frame.ctrl == (byte)GD_AFN_CODE.SetRealParamOK
                    || frame.ctrl == (byte)GD_AFN_CODE.GetRealParamOK
                    || frame.ctrl == (byte)GD_AFN_CODE.ReadRealTimeDataOK
                    )
                {
                    return REC_RESULT.OK;
                }
                else if (frame.ctrl == (byte)REC_RESULT.ERROR)
                {
                    return REC_RESULT.ERROR;
                }

            }
            else
            {
                if (frame.ctrl == (byte)GW_AFN_CODE.OK || frame.ctrl == (byte)GW_AFN_CODE.GetClock)
                {
                    return REC_RESULT.OK;
                }
                else if (frame.ctrl == (byte)REC_RESULT.ERROR)
                {
                    return REC_RESULT.ERROR;
                }
            }

            return REC_RESULT.TIME_OUT;
        }
        private REC_RESULT SendFrame(byte[] addr, byte ctl, byte[] data, int length, ref Frame_Stu frame, int timeOutS, int retryTimes)
        {
            ArrayList sendframe = new ArrayList();
            int i;
            Rule.Makeframes(ref sendframe, addr, ctl, data, length);
            for (i = 0; i < retryTimes; i++)
            {
                sendDatas(sendframe);
                
                if (GetFrame(ref frame, timeOutS) == true)
                {
                    break;
                }
            }

            if (i == retryTimes)
            {
                return REC_RESULT.TIME_OUT;
            }
            if (m_areaId == AREA.AREA_GD)
            {
                if (frame.ctrl == (byte)GD_AFN_CODE.SetRealParamOK
                    || frame.ctrl == (byte)GD_AFN_CODE.GetRealParamOK
                    || frame.ctrl == (byte)GD_AFN_CODE.ReadRealTimeDataOK
                    )
                {
                    return REC_RESULT.OK;
                }
                else if (frame.ctrl == (byte)REC_RESULT.ERROR)
                {
                    return REC_RESULT.ERROR;
                }

            }
            else
            {
                if (frame.ctrl == (byte)GW_AFN_CODE.OK || frame.ctrl == (byte)GW_AFN_CODE.GetClock)
                {
                    return REC_RESULT.OK;
                }
                else if (frame.ctrl == (byte)REC_RESULT.ERROR)
                {
                    return REC_RESULT.ERROR;
                }
            }

            return REC_RESULT.TIME_OUT;
        }

        private REC_RESULT SendFrame(byte[] addr, byte ctl, bool pw, int pwLen, byte[] pn, byte[] fn, byte[] data, ref Frame_Stu frame, int timeOutS, int retryTimes)
        {
            int i;
            ArrayList sendframe = new ArrayList();
            Rule.Makeframes(ref sendframe, addr, ctl, pw, pwLen, pn, fn, data);
            for (i = 0; i < retryTimes; i++)
            {
                sendDatas(sendframe);
                if (GetFrame(ref frame, timeOutS) == true)
                {
                    break;
                }
            }

            if (i == retryTimes)
            {
                return REC_RESULT.TIME_OUT;
            }

            if (frame.afn == (byte)REC_RESULT.OK
               || frame.afn == (byte)GW_AFN_CODE.GetClock
               || frame.afn == (byte)GW_AFN_CODE.GetRelData
               || frame.afn == (byte)GW_AFN_CODE.SetDeviceNo
               || frame.afn == (byte)GX_AFN_CODE.GetClock
               || frame.afn == (byte)GX_AFN_CODE.GetRelData
               )
            {
                return REC_RESULT.OK;
            }
            else if (frame.ctrl == (byte)REC_RESULT.ERROR)
            {
                return REC_RESULT.ERROR;
            }

            return REC_RESULT.TIME_OUT;
        }

        private REC_RESULT SendFrame(byte[] addr, byte ctl, bool pw, int pwLen, byte[] pn, byte[] fn, ref Frame_Stu frame, int timeOutS, int retryTimes)
        {
            int i;
            ArrayList sendframe = new ArrayList();
            Rule.Makeframes(ref sendframe, addr, ctl, pw, pwLen, pn, fn);

            for (i = 0; i < retryTimes; i++)
            {
                sendDatas(sendframe);
                if (GetFrame(ref frame, timeOutS) == true)
                {
                    break;
                }
            }

            if (i == retryTimes)
            {
                return REC_RESULT.TIME_OUT;
            }

            if (frame.afn == (byte)REC_RESULT.OK
                || frame.afn == (byte)GW_AFN_CODE.GetClock
                || frame.afn == (byte)GW_AFN_CODE.GetRelData
                || frame.afn == (byte)GW_AFN_CODE.SetDeviceNo
                || frame.afn == (byte)GX_AFN_CODE.GetClock
                || frame.afn == (byte)GX_AFN_CODE.GetRelData
                || frame.afn == (byte)GX_AFN_CODE.GetRelData
                )
            {
                return REC_RESULT.OK;
            }
            else if (frame.ctrl == (byte)REC_RESULT.ERROR)
            {
                return REC_RESULT.ERROR;
            }

            return REC_RESULT.TIME_OUT;
        }
        private bool GetFrame(ref Frame_Stu frame, int timeOutS)
        {
            int n = 0;
            m_recv_time_tick_100ms = 0;
            while (FrameBuff.Count == 0)
            {

                n += 100 * m_recv_time_tick_100ms;
                OnCanReceivedData();
                if (n > timeOutS * 1000)
                {
                    //txtOutput.Text += logstemp;
                   // txtOutput.Update();
                    return false;
                }
            }
            //txtOutput.Text += logstemp;
            //txtOutput.Update();
            WriteMessage(logstemp);
            frame = (Frame_Stu)FrameBuff[0];
            FrameBuff.Clear();
            return true;

        }

        public bool bNetOpenFlag = false;
        public Thread threadNet;
        private bool OpenNetPort(IPPORT_SET portSet)
        {
            HostIP = IPAddress.Parse(portSet.ipaddr);
            try
            {
                point = new IPEndPoint(HostIP, Int32.Parse(portSet.port));
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(point);
                //threadNet = new Thread(new ThreadStart(OnNetReceivedData));

                // 创建数据处理线程
                threadNet = new Thread(new ThreadStart(OnNetReceivedData));
                threadNet.Start();
                bNetOpenFlag = true;
                return true;
            }
            catch (Exception ey)
            {
                Console.WriteLine("服务器没有开启\r\n");
                return false;
            }
        }

        private void GetNetPort(ref IPPORT_SET portSet)
        {
            portSet.ipaddr = schemeConfigFrm.txtIP.Text.ToString();
            portSet.port = schemeConfigFrm.txtPort.Text.ToString();
        }

        private void GetSerialPort(ref PORT_SET portSet)
        {
            portSet.portno = Convert.ToInt16(schemeConfigFrm.comRedWire_no.Text.ToString());
            portSet.bps = Convert.ToInt32(schemeConfigFrm.comRedWire_bps.Text.ToString());
            portSet.GetCheckbit(schemeConfigFrm.comRedWire_checkbit.Text.ToString());
            portSet.stopbit = Convert.ToInt16(schemeConfigFrm.comRedWire_stopbit.Text.ToString());
        }

        private bool OpenSerialPort(PORT_SET portSet)
        {
            if (bserialPortOpen == true)
                return true;
            bserialPortOpen = true;

            serialPort.Close();
            serialPort.PortName = "com" + portSet.portno.ToString();
            serialPort.BaudRate = portSet.bps;
            serialPort.DataBits = 8;
            if (portSet.stopbit == 1)
            {
                serialPort.StopBits = StopBits.One;
            }
            else if (portSet.stopbit == 2)
            {
                serialPort.StopBits = StopBits.Two;
            }

            if (portSet.checkbit == 0)
            {
                serialPort.Parity = Parity.None;
            }
            else if (portSet.checkbit == 1)
            {
                serialPort.Parity = Parity.Odd;
            }
            else if (portSet.checkbit == 2)
            {
                serialPort.Parity = Parity.Even;
            }

            try
            {
                serialPort.Open();
                if (serialPort.IsOpen)
                {
                    //this.Hide();
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(OnSerialReceivedData);
                    return true;
                }
                else
                {
                    string str = "串口" + serialPort.PortName + "打开失败！";
                    MessageBox.Show(str);
                    bserialPortOpen = false;
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                bserialPortOpen = false;
                return false;
            }
        }

        private void ClosePort()
        {
            ///*if (serialPort.IsOpen == true /*&& schemeConfigFrm.chkRS485.Checked == true*/)
            //{
            //    serialPort.Close();
            //}*/

            if (bNetOpenFlag == true && schemeConfigFrm.chkNet.Checked == true)
            {
                socket.Close();
                bNetOpenFlag = false;
            }
        }

        public void sendDatas(ArrayList frame)
        {
            byte[] btp = new byte[frame.Count];
            frame.CopyTo(btp);
            if (serialPort.IsOpen == true)
            {
                for (int i = 0; i < frame.Count; i++)
                {
                    System.Threading.Thread.Sleep(10);
                    serialPort.Write(btp, i, 1);
                }
            }
            else if (bNetOpenFlag == true)
            {
                socket.Send(btp, frame.Count, 0);
            }

            string stemp = "\r\n发送：";
            for (int i = 0; i < btp.Length; i++)
            {
                stemp += String.Format("{0:X2}", (byte)btp[i]);
                stemp += " ";
            }
           // information.textBox1.Text += stemp;
            //txtOutput.Text += stemp;
            //tmrOutputDisplay.Enabled = true;
        }

        /**************** 重新配置CAN波特率命令  ****************************/
        private bool CanBaudSet(int Canbaud)
        {
            VCI_INIT_CONFIG config = new VCI_INIT_CONFIG();
            /*
            config.AccCode = System.Convert.ToUInt32("0x" + schemeConfigFrm.textBox_AccCode.Text, 16);
            config.AccMask = System.Convert.ToUInt32("0x" + schemeConfigFrm.textBox_AccMask.Text, 16);
            config.Timing0 = System.Convert.ToByte("0x" + schemeConfigFrm.textBox_Time0.Text, 16);
            config.Timing1 = System.Convert.ToByte("0x" + schemeConfigFrm.textBox_Time1.Text, 16);
            config.Filter = (Byte)(schemeConfigFrm.comboBox_Filter.SelectedIndex + 1);
            config.Mode = (Byte)schemeConfigFrm.comboBox_Mode.SelectedIndex;
            */

            config.AccCode = 0x00040000;
            config.AccMask = 0xFFFBFFFF;
            config.Timing1 = 0x1C;
            config.Filter = (Byte)(0 + 1);
            config.Mode = (Byte)0;

            if (Canbaud == 250)
            {
                config.Timing0 = 0x01; //can baud: 250k
            }
            else
            {
                config.Timing0 = 0x00; //can baud: 500k
            }
            VCI_InitCAN(m_devtype, m_devind, m_canind, ref config);

            //System.Threading.Thread.Sleep(1000);
            //启动CAN
            VCI_StartCAN(m_devtype, m_devind, m_canind);
            return true;
        }

        public void sendDatas(byte[] data, int len)
        {
            FrameBuff.Clear();
            if (serialPort.IsOpen == true)
            {
                for (int i = 0; i < len; i++)
                {
                    System.Threading.Thread.Sleep(10);
                    serialPort.Write(data, i, 1);
                }
            }
            else
            {
                socket.Send(data, len, 0);
            }
            logstemp = "\r\n发送：";
            for (int i = 0; i < len; i++)
            {
                logstemp += String.Format("{0:X2}", (byte)data[i]);
                logstemp += " ";
            }
           // information.textBox1.Text += stemp;
            txtOutput.Text += logstemp;
            txtOutput.Update();
           // txtOutput.Show();
            //tmrOutputDisplay.Enabled = true;
        }

        public unsafe void sendCanDatas(byte[] data, int len)
        {
            FrameBuff.Clear();

            VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
            //sendobj.Init();
            sendobj.RemoteFlag = (byte)comboBox_FrameFormat.SelectedIndex;
            sendobj.ExternFlag = (byte)comboBox_FrameType.SelectedIndex;
            sendobj.ID = System.Convert.ToUInt32("0x" + textBox_ID.Text, 16);
            sendobj.DataLen = System.Convert.ToByte(len);

            int sec_cnt;
            if (len % 8 == 0)
            {
                sec_cnt = len / 8;
            }
            else
            {
                sec_cnt = len / 8 + 1;
            }
            int pos = 0;
            for (int n = 0; n < sec_cnt; n++)
            {
                for (int i = 0; i < 8&& pos < len; i++)
                {
                    pos++;     
                    sendobj.Data[i] = System.Convert.ToByte(data[n*8+i]);
                    sendobj.DataLen = System.Convert.ToByte(i + 1);

                }

                if (VCI_Transmit(m_devtype, m_devind, m_canind, ref sendobj, 1) == 0)
                {
                    MessageBox.Show("发送失败", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

            }

            logstemp = "\r\n发送：";
            for (int i = 0; i < len; i++)
            {
                logstemp += String.Format("{0:X2}", (byte)data[i]);
                logstemp += " ";
            }
            // information.textBox1.Text += stemp;
            //txtOutput.Text += logstemp;
            //txtOutput.Update();
            WriteMessage(logstemp);
            // txtOutput.Show();
            //tmrOutputDisplay.Enabled = true;
        }

        public unsafe void sendCanDatas_Vcu(uint canid, byte[] data, int len)
        {
            FrameBuff.Clear();

            VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();

            sendobj.RemoteFlag = (byte)comboBox_FrameFormat.SelectedIndex;
            sendobj.ExternFlag = 1; //(byte)comboBox_FrameType.SelectedIndex;
            sendobj.ID = canid;
            sendobj.DataLen = System.Convert.ToByte(len);

            for (int i = 0; i < len; i++)
            {
                sendobj.Data[i] = System.Convert.ToByte(data[i]);
            }

            if (VCI_Transmit(m_devtype, m_devind, m_canind, ref sendobj, 1) == 0)
            {
                // MessageBox.Show("发送失败", "错误",
                //       MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }


        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out   int ID);
        private void LoaderConfigFile()
        {

        }

        private void GetConfig(ref testItem item, string st)
        {
            string[] str = st.Split(',');

            if (str.Length == 2)
            {
                item.netPort.ipaddr = str[0].ToString();
                item.netPort.port = str[1].ToString();
            }
            else if (str.Length == 3)
            {
                item.netPort.ipaddr = str[0].ToString();
                item.netPort.port = str[1].ToString();
                item.meterAddr = str[2].ToString();
            }
            else if (str.Length == 4)
            {
                item.serialPort.portno = Convert.ToInt16(str[0].ToString());
                item.serialPort.bps = Convert.ToInt32(str[1].ToString());
                item.serialPort.GetCheckbit(str[2].ToString());
                item.serialPort.stopbit = Convert.ToInt16(str[3].ToString());
            }

        }

        /******************************************  方法 ************************************/

        /// <summary>
        /// 用于获取电表配置
        /// </summary>
        /// <param name="MeterAddr">电表地址，长度必须为12字节，如果其中包含非BCD字符，则从左至右开始格式
        /// 化，取第一个BCD吗字符至结尾作为电表的通讯地址，并进行模糊查找</param>
        /// 
        /// <returns></returns>
        private AREA GetArea(string str)
        {
            if (str == "重庆")
            {
                return AREA.AREA_CQ;
            }
            else if (str == "广东")
            {
                return AREA.AREA_GD;
            }
            else if (str == "广西")
            {
                return AREA.AREA_GX;
            }
            else
            {
                return AREA.AREA_MAX;
            }
        }
        private bool GetAddress(ref Byte[] data, string addr)
        {
            if (addr.Length != 8)
            {
                MessageBox.Show("设备地址因设置为8位数字！");
                return false;
            }

            data[0] = Convert.ToByte(addr.Substring(2, 2), 16);
            data[1] = Convert.ToByte(addr.Substring(0, 2), 16);
            data[2] = Convert.ToByte(addr.Substring(6, 2), 16);
            data[3] = Convert.ToByte(addr.Substring(4, 2), 16);
            return true;
        }

        private bool GetGDAddress(ref Byte[] data, string addr)
        {
            if (addr.Length != 8)
            {
                MessageBox.Show("设备地址因设置为8位数字！");
                return false;
            }

            data[0] = Convert.ToByte(addr.Substring(0, 2), 16);
            data[1] = Convert.ToByte(addr.Substring(2, 2), 16);
            data[2] = Convert.ToByte(addr.Substring(6, 2), 16);
            data[3] = Convert.ToByte(addr.Substring(4, 2), 16);
            return true;
        }

        private bool GetGdMeterAddr(string addr, ref Byte[] data)
        {
            if (addr.Length != 12)
            {
                MessageBox.Show("电表地址因设置为12位数字！");
                return false;
            }

            data[5] = Convert.ToByte(addr.Substring(0, 2), 16);
            data[4] = Convert.ToByte(addr.Substring(2, 2), 16);
            data[3] = Convert.ToByte(addr.Substring(4, 2), 16);
            data[2] = Convert.ToByte(addr.Substring(6, 2), 16);
            data[1] = Convert.ToByte(addr.Substring(8, 2), 16);
            data[0] = Convert.ToByte(addr.Substring(10, 2), 16);
            return true;
        }


        void RecordTestResult(int row, int col, TEST_RESULT res)
        {

        }

        void RecordTestResult(int row, int col, string res)
        {

        }

        private int GetWeekDay(string str)
        {
            if (str == "Monday")
                return 1;
            else if (str == "Tuesday")
                return 2;
            else if (str == "Wednesday")
                return 3;
            else if (str == "Thursday")
                return 4;
            else if (str == "Friday")
                return 5;
            else if (str == "Saturday")
                return 6;
            else if (str == "Sunday")
                return 7;
            return 0;
        }

        private bool CompareArry(byte[] arry1, byte[] arry2, int len)
        {
            int i;
            for (i = 0; i < len; i++)
            {
                if (arry1[i] != arry2[i])
                {
                    break;
                }
            }

            if (i == len)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool GetWatchDogPortset(ref PORT_SET portset)
        {
            portset.portno = Convert.ToInt16(schemeConfigFrm.com_no.Text.ToString());
            portset.bps = Convert.ToInt32(schemeConfigFrm.com_bps.Text.ToString());
            portset.GetCheckbit(schemeConfigFrm.com_checkbit.Text.ToString());
            portset.stopbit = Convert.ToInt16(schemeConfigFrm.com_stopbit.Text.ToString());
            return true;
        }

        private void SetSysInitTime()
        {
            string resetCmd1 = "root\r";
            string resetCmd2 = "ptu@keli\r";
            string wclock = "hwclock -w\r";
            string month = "";
            string day = "";
            string hour = "";
            string minute = "";
            string second = "";
            PORT_SET portSet = new PORT_SET();

            GetWatchDogPortset(ref portSet);
            if (OpenSerialPort(portSet) == false)
            {
                MessageBox.Show("看门狗串口打开失败，系统时间初始化失败！");
                return;
            }

            char[] cmd1 = new char[resetCmd1.Length];
            byte[] dcmd1 = new byte[resetCmd1.Length];
            cmd1 = resetCmd1.ToCharArray();
            for (int i = 0; i < cmd1.Length; i++)
            {
                dcmd1[i] = Convert.ToByte(cmd1[i]);
            }
            sendDatas(dcmd1, dcmd1.Length);
            System.Threading.Thread.Sleep(1000);

            char[] cmd2 = new char[resetCmd2.Length];
            byte[] dcmd2 = new byte[resetCmd2.Length];
            cmd2 = resetCmd2.ToCharArray();
            for (int i = 0; i < cmd2.Length; i++)
            {
                dcmd2[i] = Convert.ToByte(cmd2[i]);
            }
            sendDatas(dcmd2, dcmd2.Length);
            System.Threading.Thread.Sleep(1000);

            string date = "date " + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Year.ToString() + "." + DateTime.Now.Second.ToString() + "\r";

            char[] cmd3 = new char[date.Length];
            byte[] dcmd3 = new byte[date.Length];
            cmd3 = date.ToCharArray();
            for (int i = 0; i < cmd3.Length; i++)
            {
                dcmd3[i] = Convert.ToByte(cmd3[i]);
            }
            sendDatas(dcmd3, dcmd3.Length);
            System.Threading.Thread.Sleep(1000);

            char[] cmd4 = new char[wclock.Length];
            byte[] dcmd4 = new byte[wclock.Length];
            cmd4 = wclock.ToCharArray();
            for (int i = 0; i < cmd4.Length; i++)
            {
                dcmd4[i] = Convert.ToByte(cmd4[i]);
            }
            sendDatas(dcmd4, dcmd4.Length);
            System.Threading.Thread.Sleep(1000);




        }
        private void telnetReboot()
        {
            //声明一个程序信息类 
            System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo();
            //设置外部程序名 
            Info.FileName = "telnetreboot.exe";
            //设置外部程序的启动参数（命令行参数）为test.txt 
            Info.Arguments = "";
            //设置外部程序工作目录为C:\\ 
            Info.WorkingDirectory = telnetRebootPath;
            //声明一个程序类 
            System.Diagnostics.Process Proc;
            try
            {
                Proc = System.Diagnostics.Process.Start(Info);
            }
            catch (SystemException er)
            {
                MessageBox.Show(er.Message);
            }

        }
        private void DelUserDir()
        {
            //声明一个程序信息类 
            System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo();
            //设置外部程序名 
            Info.FileName = "delGdUserDir.exe";
            //设置外部程序的启动参数（命令行参数）为test.txt 
            Info.Arguments = "";
            //设置外部程序工作目录为C:\\ 
            Info.WorkingDirectory = telnetRebootPath;
            //声明一个程序类 
            System.Diagnostics.Process Proc;
            try
            {
                Proc = System.Diagnostics.Process.Start(Info);
            }
            catch (SystemException er)
            {
                MessageBox.Show(er.Message);
            }

        }

        public void WriteMessage(string msg)
         {
            using (FileStream fs = new FileStream(@"d:\test.txt", FileMode.OpenOrCreate, FileAccess.Write))
             {
                 using (StreamWriter sw = new StreamWriter(fs))
                 {
                     sw.BaseStream.Seek(0, SeekOrigin.End);
                     sw.WriteLine("{0}\n", msg, DateTime.Now);
                     sw.Flush();
                 }
             }
         }


        /******************************************  测试项 ************************************/

        private void RunTest()
        {
            bool bRes;
            string getData = "";
            while (true)
            {
                int i = 0;
                foreach (testItem dt in TestMember)
                {

                    if (dt.testID == (int)TESTID.TS_WATCHDOG || dt.testID == (int)TESTID.TS_REDWIRE)
                    {
                        switch (dt.testID)
                        {
                            case (int)TESTID.TS_WATCHDOG:
                                if (OpenSerialPort(dt.serialPort) == true)
                                {
                                    if (test_watchdog() == true)
                                    {
                                        RecordTestResult(i, 2, TEST_RESULT.PASS);
                                    }
                                    else
                                    {
                                        RecordTestResult(i, 2, TEST_RESULT.FAIL);
                                    }

                                    ClosePort();
                                }
                                break;

                            case (int)TESTID.TS_REDWIRE:
                                if (OpenSerialPort(dt.serialPort) == true)
                                {
                                    string time = "";
                                    if (test_redwire(ref time) == true)
                                    {
                                        RecordTestResult(i, 2, TEST_RESULT.PASS);
                                        RecordTestResult(i, 3, time);
                                    }
                                    else
                                    {
                                        RecordTestResult(i, 2, TEST_RESULT.FAIL);
                                    }
                                    break;
                                    ClosePort();
                                }
                                break;

                            default:
                                break;

                        }

                    }
                    else
                    {
                        if (bNetOpenFlag == false)
                        {
                            if (OpenNetPort(dt.netPort) == false)
                            {
                                MessageBox.Show("网络连接失败！退出测试");
                                thread.Abort();
                                return;
                            }

                        }

                        switch (dt.testID)
                        {
                            case (int)TESTID.TS_CLOCK:
                                if (test_clock() == true)
                                {
                                    RecordTestResult(i, 2, TEST_RESULT.PASS);
                                }
                                else
                                {
                                    RecordTestResult(i, 2, TEST_RESULT.FAIL);
                                }
                                break;

                            case (int)TESTID.TS_BAT:
                                if (test_bat(dt.netPort) == true)
                                {
                                    RecordTestResult(i, 2, TEST_RESULT.PASS);
                                }
                                else
                                {
                                    RecordTestResult(i, 2, TEST_RESULT.FAIL);
                                }
                                break;

                            case (int)TESTID.TS_4851:
                                if (test_4851(dt.meterAddr, ref getData) == true)
                                {
                                    RecordTestResult(i, 2, TEST_RESULT.PASS);
                                    RecordTestResult(i, 3, getData);
                                }
                                else
                                {
                                    RecordTestResult(i, 2, TEST_RESULT.FAIL);
                                }
                                break;

                            case (int)TESTID.TS_4852:
                                if (test_4852(dt.meterAddr, ref getData) == true)
                                {
                                    RecordTestResult(i, 2, TEST_RESULT.PASS);
                                    RecordTestResult(i, 3, getData);
                                }
                                else
                                {
                                    RecordTestResult(i, 2, TEST_RESULT.FAIL);
                                }
                                break;

                            case (int)TESTID.TS_4853:
                                if (test_4853(dt.meterAddr, ref getData) == true)
                                {
                                    RecordTestResult(i, 2, TEST_RESULT.PASS);
                                    RecordTestResult(i, 3, getData);
                                }
                                else
                                {
                                    RecordTestResult(i, 2, TEST_RESULT.FAIL);
                                }
                                break;

                            case (int)TESTID.TS_DOWN:
                                if (test_down(dt.meterAddr, ref getData) == true)
                                {
                                    RecordTestResult(i, 2, TEST_RESULT.PASS);
                                    RecordTestResult(i, 3, getData);
                                }
                                else
                                {
                                    RecordTestResult(i, 2, TEST_RESULT.FAIL);
                                }
                                break;

                            default:
                                break;
                        }

                    }
                    i++;
                }
                MessageBox.Show("测试结束！");
                ClosePort();
                thread.Abort();
            }

        }
        private bool Get_guowang_clock(ref string strDate)
        {
            Frame_Stu frame = new Frame_Stu();
            byte[] date = new byte[6];
            byte[] addr = new byte[4];

            if (GetAddress(ref addr, txtAddr.Text) == false)
            {
                return false;
            }

            string year = DateTime.Now.Year.ToString();
            while (true)
            {
                if (year.Length > 2)
                {
                    year = year.Remove(0, 1);
                }
                else
                {
                    break;
                }
            }

            date[5] = Convert.ToByte(year, 16);
            date[4] = Convert.ToByte(DateTime.Now.Month.ToString(), 16);
            byte month = Convert.ToByte(GetWeekDay(DateTime.Now.DayOfWeek.ToString()));
            month = (byte)(month << 5);
            date[4] |= month;
            month = date[4];
            date[3] = Convert.ToByte(DateTime.Now.Day.ToString(), 16);
            date[2] = Convert.ToByte(DateTime.Now.Hour.ToString(), 16);
            date[1] = Convert.ToByte(DateTime.Now.Minute.ToString(), 16);
            date[0] = Convert.ToByte(DateTime.Now.Second.ToString(), 16);

            byte[] pn = { 0x00, 0x00 };
            byte[] fn = { 0x40, 0x03 };
            if (m_areaId == AREA.AREA_CQ)
            {

                pn[0] = 0x00;
                pn[1] = 0x00;
                fn[0] = 0x02;
                fn[1] = 0x00;

                FrameBuff.Clear();
                if (SendFrame(addr, (byte)GW_AFN_CODE.GetClock, true, GW_PW_LEN, pn, fn, ref frame, 20, 3) == REC_RESULT.OK)
                {
                    byte[] data = new byte[frame.dataArray.Count];
                    frame.dataArray.CopyTo(data);
                    strDate = String.Format("{0:X2}", (byte)data[5]) + "年" + String.Format("{0:X2}", (byte)(data[4] & 0x1f)) + "月" + String.Format("{0:X2}", (byte)data[3]) + "日";
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else if (m_areaId == AREA.AREA_GX)
            {

                pn[0] = 0x00;
                pn[1] = 0x00;
                fn[0] = 0x40;
                fn[1] = 0x03;

                FrameBuff.Clear();
                if (SendFrame(addr, (byte)GX_AFN_CODE.GetClock, false, GX_PW_LEN, pn, fn, ref frame, 20, 3) == REC_RESULT.OK)
                {
                    byte[] data = new byte[frame.dataArray.Count];
                    frame.dataArray.CopyTo(data);
                    strDate = String.Format("{0:X2}", (byte)data[5]) + "年" + String.Format("{0:X2}", (byte)(data[4] & 0x1f)) + "月" + String.Format("{0:X2}", (byte)data[3]) + "日";
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }


        private bool test_guowang_clock()
        {
            Frame_Stu frame = new Frame_Stu();
            byte[] date = new byte[6];
            byte[] addr = new byte[4];

            if (GetAddress(ref addr, txtAddr.Text) == false)
            {
                return false;
            }

            string year = DateTime.Now.Year.ToString();
            while (true)
            {
                if (year.Length > 2)
                {
                    year = year.Remove(0, 1);
                }
                else
                {
                    break;
                }
            }

            date[5] = Convert.ToByte(year, 16);
            date[4] = Convert.ToByte(DateTime.Now.Month.ToString(), 16);
            byte month = Convert.ToByte(GetWeekDay(DateTime.Now.DayOfWeek.ToString()));
            month = (byte)(month << 5);
            date[4] |= month;
            month = date[4];
            date[3] = Convert.ToByte(DateTime.Now.Day.ToString(), 16);
            date[2] = Convert.ToByte(DateTime.Now.Hour.ToString(), 16);
            date[1] = Convert.ToByte(DateTime.Now.Minute.ToString(), 16);
            date[0] = Convert.ToByte(DateTime.Now.Second.ToString(), 16);

            byte[] pn = { 0x00, 0x00 };
            byte[] fn = { 0x40, 0x03 };
            if (m_areaId == AREA.AREA_CQ)
            {
                if (SendFrame(addr, (byte)GW_AFN_CODE.SetClock, true, GW_PW_LEN, pn, fn, date, ref frame, 15, 3) == REC_RESULT.OK)
                {
                    pn[0] = 0x00;
                    pn[1] = 0x00;
                    fn[0] = 0x02;
                    fn[1] = 0x00;
                    System.Threading.Thread.Sleep(1000);
                    FrameBuff.Clear();
                    if (SendFrame(addr, (byte)GW_AFN_CODE.GetClock, true, GW_PW_LEN, pn, fn, ref frame, 20, 3) == REC_RESULT.OK)
                    {
                        year = DateTime.Now.Year.ToString();
                        while (true)
                        {
                            if (year.Length > 2)
                            {
                                year = year.Remove(0, 1);
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (/*((Byte)frame.dataArray[0] == Convert.ToByte(DateTime.Now.Second.ToString(), 16))
                            &&((Byte)frame.dataArray[1] == Convert.ToByte(DateTime.Now.Minute.ToString(), 16))
                            &&*/
                                ((Byte)frame.dataArray[2] == Convert.ToByte(DateTime.Now.Hour.ToString(), 16))
                            && ((Byte)frame.dataArray[3] == Convert.ToByte(DateTime.Now.Day.ToString(), 16))
                            && ((Byte)frame.dataArray[4] == month)
                            && ((Byte)frame.dataArray[5] == Convert.ToByte(year, 16))
                            )
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }


                }
                else
                {
                    return false;
                }
            }
            else if (m_areaId == AREA.AREA_GX)
            {
                if (SendFrame(addr, (byte)GX_AFN_CODE.SetClock, true, GX_PW_LEN, pn, fn, date, ref frame, 15, 3) == REC_RESULT.OK)
                {
                    pn[0] = 0x00;
                    pn[1] = 0x00;
                    fn[0] = 0x40;
                    fn[1] = 0x03;
                    System.Threading.Thread.Sleep(1000);
                    FrameBuff.Clear();
                    if (SendFrame(addr, (byte)GX_AFN_CODE.GetClock, false, GX_PW_LEN, pn, fn, ref frame, 20, 3) == REC_RESULT.OK)
                    {
                        year = DateTime.Now.Year.ToString();
                        while (true)
                        {
                            if (year.Length > 2)
                            {
                                year = year.Remove(0, 1);
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (/*((Byte)frame.dataArray[0] == Convert.ToByte(DateTime.Now.Second.ToString(), 16))
                            &&((Byte)frame.dataArray[1] == Convert.ToByte(DateTime.Now.Minute.ToString(), 16))
                            &&*/
                                ((Byte)frame.dataArray[2] == Convert.ToByte(DateTime.Now.Hour.ToString(), 16))
                            && ((Byte)frame.dataArray[3] == Convert.ToByte(DateTime.Now.Day.ToString(), 16))
                            && ((Byte)frame.dataArray[4] == month)
                            && ((Byte)frame.dataArray[5] == Convert.ToByte(year, 16))
                            )
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }


                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        private bool test_guangdong_clock()
        {
            Frame_Stu frame = new Frame_Stu();

            byte[] data = new byte[14];
            byte[] addr = new byte[4];

            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return false;
            }

            string year = DateTime.Now.Year.ToString();
            while (true)
            {
                if (year.Length > 2)
                {
                    year = year.Remove(0, 1);
                }
                else
                {
                    break;
                }
            }

            //测量点
            data[0] = 0x00;
            data[1] = 0x00;
            //权限密码
            data[2] = 0x11;
            data[3] = 0x11;
            data[4] = 0x11;
            data[5] = 0x11;
            //数据项8030
            data[6] = 0x30;
            data[7] = 0x80;
            //秒分时日月年
            data[8] = Convert.ToByte(DateTime.Now.Second.ToString(), 16);
            data[9] = Convert.ToByte(DateTime.Now.Minute.ToString(), 16);
            data[10] = Convert.ToByte(DateTime.Now.Hour.ToString(), 16);
            data[11] = Convert.ToByte(DateTime.Now.Day.ToString(), 16);

            data[12] = Convert.ToByte(DateTime.Now.Month.ToString(), 16);
            byte month = data[12];

            data[13] = Convert.ToByte(year, 16);

            if (SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, ref frame, 15, 3) == REC_RESULT.OK)
            {

                System.Threading.Thread.Sleep(1000);
                FrameBuff.Clear();
                byte[] dataArry = new byte[10];
                dataArry.Initialize();
                // 测量点
                dataArry[0] = 0x01;
                // 数据项
                dataArry[8] = 0x30;
                dataArry[9] = 0x80;

                if (SendFrame(addr, (byte)GD_AFN_CODE.GetRealtimeParam, dataArry, ref frame, 20, 3) == REC_RESULT.OK)
                {
                    year = DateTime.Now.Year.ToString();
                    while (true)
                    {
                        if (year.Length > 2)
                        {
                            year = year.Remove(0, 1);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (/*((Byte)frame.dataArray[0] == Convert.ToByte(DateTime.Now.Second.ToString(), 16))
                        &&((Byte)frame.dataArray[1] == Convert.ToByte(DateTime.Now.Minute.ToString(), 16))
                        &&*/
                            ((Byte)frame.dataArray[12] == Convert.ToByte(DateTime.Now.Hour.ToString(), 16))
                        && ((Byte)frame.dataArray[13] == Convert.ToByte(DateTime.Now.Day.ToString(), 16))
                        && ((Byte)frame.dataArray[14] == month)
                        && ((Byte)frame.dataArray[15] == Convert.ToByte(year, 16))
                        )
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool test_clock()
        {
            if (m_areaId == AREA.AREA_GD)
            {
                return test_guangdong_clock();
            }
            else
            {
                return test_guowang_clock();
            }
        }

        private bool test_GW_bat(IPPORT_SET portset)
        {
            Frame_Stu frame = new Frame_Stu();
            byte[] date = new byte[6];
            byte[] addr = new byte[4];

            if (GetAddress(ref addr, txtAddr.Text) == false)
            {
                return false;
            }

            string year = DateTime.Now.Year.ToString();
            while (true)
            {
                if (year.Length > 2)
                {
                    year = year.Remove(0, 1);
                }
                else
                {
                    break;
                }
            }

            date[5] = Convert.ToByte(year, 16);
            date[4] = Convert.ToByte(DateTime.Now.Month.ToString(), 16);
            byte month = Convert.ToByte(GetWeekDay(DateTime.Now.DayOfWeek.ToString()));
            month = (byte)(month << 5);
            date[4] |= month;
            month = date[4];
            date[3] = Convert.ToByte(DateTime.Now.Day.ToString(), 16);
            date[2] = Convert.ToByte(DateTime.Now.Hour.ToString(), 16);
            date[1] = Convert.ToByte(DateTime.Now.Minute.ToString(), 16);
            date[0] = Convert.ToByte(DateTime.Now.Second.ToString(), 16);

            byte[] pn = { 0x00, 0x00 };
            byte[] fn = { 0x40, 0x03 };

            if (m_areaId == AREA.AREA_CQ)
            {
                if (SendFrame(addr, (byte)GW_AFN_CODE.SetClock, true, GW_PW_LEN, pn, fn, date, ref frame, 5, 3) == REC_RESULT.OK)
                {
                    pn[0] = 0x00;
                    pn[1] = 0x00;
                    fn[0] = 0x02;
                    fn[1] = 0x00;
                    MessageBox.Show("请将集中器断电，五秒后再重新上电，待集中器正常运行后，点击提示框“确认”按钮继续测试！");
                    System.Threading.Thread.Sleep(15000);
                    if (OpenNetPort(portset) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        thread.Abort();
                        return false;
                    }

                    FrameBuff.Clear();
                    if (SendFrame(addr, (byte)GW_AFN_CODE.GetClock, true, GW_PW_LEN, pn, fn, ref frame, 20, 3) == REC_RESULT.OK)
                    {
                        year = DateTime.Now.Year.ToString();
                        while (true)
                        {
                            if (year.Length > 2)
                            {
                                year = year.Remove(0, 1);
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (/*((Byte)frame.dataArray[0] == Convert.ToByte(DateTime.Now.Second.ToString(), 16))
                            &&((Byte)frame.dataArray[1] == Convert.ToByte(DateTime.Now.Minute.ToString(), 16))
                            &&*/
                                ((Byte)frame.dataArray[2] == Convert.ToByte(DateTime.Now.Hour.ToString(), 16))
                            && ((Byte)frame.dataArray[3] == Convert.ToByte(DateTime.Now.Day.ToString(), 16))
                            && ((Byte)frame.dataArray[4] == month)
                            && ((Byte)frame.dataArray[5] == Convert.ToByte(year, 16))
                            )
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                    return true;

                }
                else
                {
                    return false;
                }
            }
            else if (m_areaId == AREA.AREA_GX)
            {
                if (SendFrame(addr, (byte)GX_AFN_CODE.SetClock, true, GX_PW_LEN, pn, fn, date, ref frame, 5, 3) == REC_RESULT.OK)
                {
                    pn[0] = 0x00;
                    pn[1] = 0x00;
                    fn[0] = 0x40;
                    fn[1] = 0x03;
                    MessageBox.Show("请将集中器断电，五秒后再重新上电，待集中器正常运行后，点击提示框“确认”按钮继续测试！");
                    System.Threading.Thread.Sleep(15000);
                    if (OpenNetPort(portset) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        thread.Abort();
                        return false;
                    }

                    FrameBuff.Clear();
                    if (SendFrame(addr, (byte)GX_AFN_CODE.GetClock, true, GX_PW_LEN, pn, fn, ref frame, 20, 3) == REC_RESULT.OK)
                    {
                        year = DateTime.Now.Year.ToString();
                        while (true)
                        {
                            if (year.Length > 2)
                            {
                                year = year.Remove(0, 1);
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (/*((Byte)frame.dataArray[0] == Convert.ToByte(DateTime.Now.Second.ToString(), 16))
                            &&((Byte)frame.dataArray[1] == Convert.ToByte(DateTime.Now.Minute.ToString(), 16))
                            &&*/
                                ((Byte)frame.dataArray[2] == Convert.ToByte(DateTime.Now.Hour.ToString(), 16))
                            && ((Byte)frame.dataArray[3] == Convert.ToByte(DateTime.Now.Day.ToString(), 16))
                            && ((Byte)frame.dataArray[4] == month)
                            && ((Byte)frame.dataArray[5] == Convert.ToByte(year, 16))
                            )
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                    return true;

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private bool test_GD_bat(IPPORT_SET portset)
        {
            Frame_Stu frame = new Frame_Stu();

            byte[] data = new byte[14];
            byte[] addr = new byte[4];

            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return false;
            }

            string year = DateTime.Now.Year.ToString();
            while (true)
            {
                if (year.Length > 2)
                {
                    year = year.Remove(0, 1);
                }
                else
                {
                    break;
                }
            }

            //测量点
            data[0] = 0x00;
            data[1] = 0x00;
            //权限密码
            data[2] = 0x11;
            data[3] = 0x11;
            data[4] = 0x11;
            data[5] = 0x11;
            //数据项8030
            data[6] = 0x30;
            data[7] = 0x80;
            //秒分时日月年
            data[8] = Convert.ToByte(DateTime.Now.Second.ToString(), 16);
            data[9] = Convert.ToByte(DateTime.Now.Minute.ToString(), 16);
            data[10] = Convert.ToByte(DateTime.Now.Hour.ToString(), 16);
            data[11] = Convert.ToByte(DateTime.Now.Day.ToString(), 16);

            data[12] = Convert.ToByte(DateTime.Now.Month.ToString(), 16);
            byte month = data[12];

            data[13] = Convert.ToByte(year, 16);

            if (SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, ref frame, 5, 3) == REC_RESULT.OK)
            {
                MessageBox.Show("请将集中器断电，五秒后再重新上电，待集中器正常运行后，点击提示框“确认”按钮继续测试！");
                System.Threading.Thread.Sleep(15000);
                if (OpenNetPort(portset) == false)
                {
                    MessageBox.Show("网络连接失败！退出测试");
                    thread.Abort();
                    return false;
                }
                System.Threading.Thread.Sleep(1000);
                FrameBuff.Clear();
                byte[] dataArry = new byte[10];
                dataArry.Initialize();
                // 测量点
                dataArry[0] = 0x01;
                // 数据项
                dataArry[8] = 0x30;
                dataArry[9] = 0x80;

                if (SendFrame(addr, (byte)GD_AFN_CODE.GetRealtimeParam, dataArry, ref frame, 20, 3) == REC_RESULT.OK)
                {
                    year = DateTime.Now.Year.ToString();
                    while (true)
                    {
                        if (year.Length > 2)
                        {
                            year = year.Remove(0, 1);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (/*((Byte)frame.dataArray[0] == Convert.ToByte(DateTime.Now.Second.ToString(), 16))
                        &&((Byte)frame.dataArray[1] == Convert.ToByte(DateTime.Now.Minute.ToString(), 16))
                        &&*/
                            ((Byte)frame.dataArray[12] == Convert.ToByte(DateTime.Now.Hour.ToString(), 16))
                        && ((Byte)frame.dataArray[13] == Convert.ToByte(DateTime.Now.Day.ToString(), 16))
                        && ((Byte)frame.dataArray[14] == month)
                        && ((Byte)frame.dataArray[15] == Convert.ToByte(year, 16))
                        )
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool test_bat(IPPORT_SET portset)
        {
            if (m_areaId == AREA.AREA_GD)
            {
                return test_GD_bat(portset);
            }
            else
            {
                return test_GW_bat(portset);
            }
        }

        private bool test_GW_4851(string meterAddr, ref string getData)
        {
            Frame_Stu frame = new Frame_Stu();
            byte[] date = new byte[6];
            byte[] addr = new byte[4];

            if (GetAddress(ref addr, txtAddr.Text) == false)
            {
                return false;
            }

            if (m_areaId == AREA.AREA_CQ)
            {
                byte[] pn = { 0x01, 0x01 };
                byte[] fn = { 0x01, 0x10 };

                if (SendFrame(addr, (byte)GW_AFN_CODE.GetRelData, true, GW_PW_LEN, pn, fn, ref frame, 15, 3) == REC_RESULT.OK)
                {
                    byte[] data = new byte[frame.dataArray.Count];
                    frame.dataArray.CopyTo(data);
                    if (data[6] == 0xee && data[7] == 0xee && data[8] == 0xee && data[9] == 0xee && data[10] == 0xee)
                    {
                        return false;
                    }
                    getData = String.Format("{0:X2}", (byte)data[4]) + "年" + String.Format("{0:X2}", (byte)data[3]) + "月" + String.Format("{0:X2}", (byte)data[2]) + "日" + String.Format("{0:X2}", (byte)data[1]) + "时" + String.Format("{0:X2}", (byte)data[1]) + "分";
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (m_areaId == AREA.AREA_GX)
            {
                byte[] pn = { 0x00, 0x00 };
                byte[] fn = { 0x01, 0x07 };
                byte[] sdata = new byte[5];
                sdata[0] = 0x00;//抄读方式

                sdata[1] = 0x01;//抄表数
                sdata[2] = 0x00;

                sdata[3] = 0x01;//抄表序号
                sdata[4] = 0x00;
                if (SendFrame(addr, (byte)GX_AFN_CODE.GetRelData, true, GX_PW_LEN, pn, fn, sdata, ref frame, 15, 3) == REC_RESULT.OK)
                {
                    byte[] data = new byte[frame.dataArray.Count];
                    frame.dataArray.CopyTo(data);
                    if (data[0] == 0x00 && data[1] == 0x00)
                    {
                        return false;
                    }
                    getData = String.Format("{0:X2}", (byte)data[7]) + String.Format("{0:X2}", (byte)data[6]) + String.Format("{0:X2}", (byte)data[5]) + "." + String.Format("{0:X2}", (byte)data[4]) + "KW";
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }
        private bool test_GD_485(string meterAddr, int tn, ref string getData)
        {
            Frame_Stu frame = new Frame_Stu();
            byte[] data = new byte[35];
            byte[] addr = new byte[4];

            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return false;
            }

            //表地址
            GetGdMeterAddr(meterAddr, ref data);
            //TN号
            data[6] = (byte)(tn & 0x0ff);
            data[7] = (byte)0x00; //data[7] = (byte)((tn >> 8) & 0xff);
            // 自动中继
            data[8] = 0xff;
            // 中继表号
            for (int i = 0; i < 24; i++)
            {
                data[9 + i] = 0x99;
            }
            data[33] = 0x10;
            data[34] = 0x90;

            if (SendFrame(addr, (byte)GD_AFN_CODE.ReadRealTimeData, data, ref frame, 30, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                if (rcdata[6] == 0xee && rcdata[7] == 0xee && rcdata[8] == 0xee && rcdata[9] == 0xee && rcdata[10] == 0xee)
                {
                    return false;
                }
                getData = String.Format("{0:X2}", (byte)rcdata[9]) + "." + String.Format("{0:X2}", (byte)rcdata[8]) + "KW";
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool test_4851(string meterAddr, ref string getData)
        {
            if (m_areaId == AREA.AREA_CQ || m_areaId == AREA.AREA_GX)
            {
                return test_GW_4851(meterAddr, ref getData);
            }
            else if (m_areaId == AREA.AREA_GD)
            {
                return test_GD_485(meterAddr, 1, ref getData);
            }
            else
            {
                return false;
            }

        }

        private bool test_GW_4852(string meterAddr, ref string getData)
        {
            Frame_Stu frame = new Frame_Stu();
            byte[] date = new byte[6];
            byte[] addr = new byte[4];

            if (GetAddress(ref addr, txtAddr.Text) == false)
            {
                return false;
            }

            if (m_areaId == AREA.AREA_CQ)
            {
                byte[] pn = { 0x02, 0x01 };
                byte[] fn = { 0x01, 0x10 };

                if (SendFrame(addr, (byte)GW_AFN_CODE.GetRelData, true, GW_PW_LEN, pn, fn, ref frame, 15, 3) == REC_RESULT.OK)
                {
                    byte[] data = new byte[frame.dataArray.Count];
                    frame.dataArray.CopyTo(data);
                    if (data[0] == 0x00 && data[1] == 0x00)
                    {
                        return false;
                    }
                    getData = String.Format("{0:X2}", (byte)data[7]) + String.Format("{0:X2}", (byte)data[6]) + String.Format("{0:X2}", (byte)data[5]) + "." + String.Format("{0:X2}", (byte)data[4]) + "KW";
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (m_areaId == AREA.AREA_GX)
            {
                byte[] pn = { 0x00, 0x00 };
                byte[] fn = { 0x01, 0x07 };
                byte[] sdata = new byte[5];
                sdata[0] = 0x00;//抄读方式

                sdata[1] = 0x01;//抄表数
                sdata[2] = 0x00;

                sdata[3] = 0x02;//抄表序号
                sdata[4] = 0x00;
                if (SendFrame(addr, (byte)GX_AFN_CODE.GetRelData, true, GX_PW_LEN, pn, fn, sdata, ref frame, 15, 3) == REC_RESULT.OK)
                {
                    byte[] data = new byte[frame.dataArray.Count];
                    frame.dataArray.CopyTo(data);
                    if (data[0] == 0x00 && data[1] == 0x00)
                    {
                        return false;
                    }
                    getData = String.Format("{0:X2}", (byte)data[7]) + String.Format("{0:X2}", (byte)data[6]) + String.Format("{0:X2}", (byte)data[5]) + "." + String.Format("{0:X2}", (byte)data[4]) + "KW";
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private bool test_GW_4853(string meterAddr, ref string getData)
        {
            Frame_Stu frame = new Frame_Stu();
            byte[] date = new byte[6];
            byte[] addr = new byte[4];

            if (GetAddress(ref addr, txtAddr.Text) == false)
            {
                return false;
            }

            if (m_areaId == AREA.AREA_CQ)
            {
                byte[] pn = { 0x08, 0x01 };
                byte[] fn = { 0x01, 0x10 };

                if (SendFrame(addr, (byte)GW_AFN_CODE.GetRelData, true, GW_PW_LEN, pn, fn, ref frame, 15, 3) == REC_RESULT.OK)
                {
                    byte[] data = new byte[frame.dataArray.Count];
                    frame.dataArray.CopyTo(data);
                    if (data[0] == 0x00 && data[1] == 0x00)
                    {
                        return false;
                    }
                    getData = String.Format("{0:X2}", (byte)data[7]) + String.Format("{0:X2}", (byte)data[6]) + String.Format("{0:X2}", (byte)data[5]) + "." + String.Format("{0:X2}", (byte)data[4]) + "KW";
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (m_areaId == AREA.AREA_GX)
            {
                byte[] pn = { 0x00, 0x00 };
                byte[] fn = { 0x01, 0x07 };
                byte[] sdata = new byte[5];
                sdata[0] = 0x00;//抄读方式

                sdata[1] = 0x01;//抄表数
                sdata[2] = 0x00;

                sdata[3] = 0x02;//抄表序号
                sdata[4] = 0x00;
                if (SendFrame(addr, (byte)GX_AFN_CODE.GetRelData, true, GX_PW_LEN, pn, fn, sdata, ref frame, 15, 3) == REC_RESULT.OK)
                {
                    byte[] data = new byte[frame.dataArray.Count];
                    frame.dataArray.CopyTo(data);
                    if (data[0] == 0x00 && data[1] == 0x00)
                    {
                        return false;
                    }
                    getData = String.Format("{0:X2}", (byte)data[7]) + String.Format("{0:X2}", (byte)data[6]) + String.Format("{0:X2}", (byte)data[5]) + "." + String.Format("{0:X2}", (byte)data[4]) + "KW";
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private bool test_4852(string meterAddr, ref string getData)
        {
            if (m_areaId == AREA.AREA_CQ || m_areaId == AREA.AREA_GX)
            {
                return test_GW_4852(meterAddr, ref getData);
            }
            else if (m_areaId == AREA.AREA_GD)
            {
                return test_GD_485(meterAddr, 2, ref getData);
            }
            else
            {
                return false;
            }

        }

        private bool test_4853(string meterAddr, ref string getData)
        {
            if (m_areaId == AREA.AREA_CQ || m_areaId == AREA.AREA_GX)
            {
                return test_GW_4853(meterAddr, ref getData);
            }
            else if (m_areaId == AREA.AREA_GD)
            {
                return test_GD_485(meterAddr, 4, ref getData);
            }
            else
            {
                return false;
            }
        }

        private bool test_GW_down(string meterAddr, ref string getData)
        {
            Frame_Stu frame = new Frame_Stu();
            byte[] date = new byte[6];
            byte[] addr = new byte[4];

            if (GetAddress(ref addr, txtAddr.Text) == false)
            {
                return false;
            }

            if (m_areaId == AREA.AREA_CQ)
            {
                byte[] pn = { 0x04, 0x01 };
                byte[] fn = { 0x01, 0x10 };
                if (SendFrame(addr, (byte)GW_AFN_CODE.GetRelData, true, GW_PW_LEN, pn, fn, ref frame, 15, 3) == REC_RESULT.OK)
                {
                    byte[] data = new byte[frame.dataArray.Count];
                    frame.dataArray.CopyTo(data);
                    if (data[6] == 0xee && data[7] == 0xee && data[8] == 0xee && data[9] == 0xee && data[10] == 0xee)
                    {
                        return false;
                    }
                    getData = String.Format("{0:X2}", (byte)data[4]) + "年" + String.Format("{0:X2}", (byte)data[3]) + "月" + String.Format("{0:X2}", (byte)data[2]) + "日" + String.Format("{0:X2}", (byte)data[1]) + "时" + String.Format("{0:X2}", (byte)data[1]) + "分";
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (m_areaId == AREA.AREA_GX)
            {
                byte[] pn = { 0x00, 0x00 };
                byte[] fn = { 0x01, 0x07 };
                byte[] sdata = new byte[5];
                sdata[0] = 0x00;//抄读方式

                sdata[1] = 0x01;//抄表数
                sdata[2] = 0x00;

                sdata[3] = 0x03;//抄表序号
                sdata[4] = 0x00;

                if (SendFrame(addr, (byte)GX_AFN_CODE.GetRelData, true, GX_PW_LEN, pn, fn, sdata, ref frame, 15, 3) == REC_RESULT.OK)
                {
                    byte[] data = new byte[frame.dataArray.Count];
                    frame.dataArray.CopyTo(data);
                    if (data[0] == 0x00 && data[1] == 0x00)
                    {
                        return false;
                    }
                    getData = String.Format("{0:X2}", (byte)data[7]) + String.Format("{0:X2}", (byte)data[6]) + String.Format("{0:X2}", (byte)data[5]) + "." + String.Format("{0:X2}", (byte)data[4]) + "KW";
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private bool test_down(string meterAddr, ref string getData)
        {
            if (m_areaId == AREA.AREA_CQ || m_areaId == AREA.AREA_GX)
            {
                return test_GW_down(meterAddr, ref getData);
            }
            else if (m_areaId == AREA.AREA_GD)
            {
                return test_GD_485(meterAddr, 3, ref getData);
            }
            else
            {
                return false;
            }
        }

        private bool test_redwire(ref string strDate)
        {
            /*            Frame_Stu frame = new Frame_Stu();
                        byte[] date = new byte[6];
                        byte[] addr = new byte[4];
                        byte[] pn = { 0x00, 0x00 };
                        byte[] fn = { 0x02, 0x00 };

                        if (GetAddress(ref addr, txtAddr.Text) == false)
                        {
                            return false;
                        }

                        FrameBuff.Clear();
                        if (SendFrame(addr, (byte)GW_AFN_CODE.GetClock, true, GW_PW_LEN, pn, fn, ref frame, 20, 3) == REC_RESULT.OK)
                        {

                            byte[] data = new byte[frame.dataArray.Count];
                            frame.dataArray.CopyTo(data);
                            strDate = String.Format("{0:X2}", (byte)data[2]) + "时" + String.Format("{0:X2}", (byte)data[1]) + "分" + String.Format("{0:X2}", (byte)data[0]) + "秒";
                            return true;
                        }
                        else
                        {
                            return false;
                        }*/
            if (m_areaId == AREA.AREA_CQ || m_areaId == AREA.AREA_GX)
            {
                return Get_guowang_clock(ref strDate);
            }
            else
            {
                // 广东红外测试 目前缺省
                return false;
            }
        }

        private bool bTestWatchDog = false;
        private bool test_watchdog()
        {
            string resetCmd1 = "root\r";
            string resetCmd2 = "ptu@keli\r";
            string resetCmd3 = "killall dyjc dyjcd\r";
            string returnKeyword = "52 6F 6D 42 4F 4F 54";//RomBOOT

            //char[] cmd1 = new char[resetCmd1.Length];
            //byte[] dcmd1 = new byte[resetCmd1.Length];
            //cmd1 = resetCmd1.ToCharArray();
            //for (int i = 0; i < cmd1.Length; i++)
            //{
            //    dcmd1[i] = Convert.ToByte(cmd1[i]);
            //}
            //sendDatas(dcmd1, dcmd1.Length);
            //System.Threading.Thread.Sleep(1000);

            //char[] cmd2 = new char[resetCmd2.Length];
            //byte[] dcmd2 = new byte[resetCmd2.Length];
            //cmd2 = resetCmd2.ToCharArray();
            //for (int i = 0; i < cmd2.Length; i++)
            //{
            //    dcmd2[i] = Convert.ToByte(cmd2[i]);
            //}
            //sendDatas(dcmd2, dcmd2.Length);
            //System.Threading.Thread.Sleep(1000);

            //char[] cmd3 = new char[resetCmd3.Length];
            //byte[] dcmd3 = new byte[resetCmd3.Length];
            //cmd3 = resetCmd3.ToCharArray();
            //for (int i = 0; i < cmd3.Length; i++)
            //{
            //    dcmd3[i] = Convert.ToByte(cmd3[i]);
            //}
            //sendDatas(dcmd3, dcmd3.Length);
            telnetReboot();
            bTestWatchDog = true; //开始侦听串口上报数据
            information.textBox1.Text = "";
            txtOutput.Text = "";
            System.Threading.Thread.Sleep(15000);


            for (int j = 0; j < 300; j++)
            {
                if (information.textBox1.Text.ToString().Contains(returnKeyword) == true)
                {

                    bTestWatchDog = false; //停止侦听串口上报数据
                    return true;
                }
                System.Threading.Thread.Sleep(100);
            }
            bTestWatchDog = false; //停止侦听串口上报数据
            return false;
        }

        private void Add33(ref byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] += 0x33;
            }
        }
        private void Sub33(ref byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] -= 0x33;
            }
        }


        private void Stop_Click(object sender, EventArgs e)
        {
            MessageBox.Show("测试停止！");
            thread.Abort();
        }

        //private string GetTester()
        //{
        //    // 创建Application对象
        //    Missing Miss = Missing.Value;
        //    int i = 2, j = 0;

        //    // 创建Application对象
        //    Excel.Application xlsApp = new Excel.Application();
        //    if (xlsApp == null)
        //    {
        //        return "";
        //    }

        //    if (xlsApp == null)
        //    {
        //        return "";
        //    }

        //    /* 打开文件 */
        //    Excel.Workbook xlsBook = xlsApp.Workbooks.Open(testerFilePath, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss);
        //    Excel.Worksheet xlsSheet = (Excel.Worksheet)xlsBook.Sheets[1];

        //    string name = (string)(((Excel.Range)xlsSheet.Cells[2, 1]).Value2);
        //    // 关闭XLS文件
        //    xlsBook.Close(false, Type.Missing, Type.Missing);
        //    xlsApp.Quit();

        //    // 任务管理器关闭excel.exe
        //    IntPtr t = new IntPtr(xlsApp.Hwnd);
        //    int k = 0;
        //    GetWindowThreadProcessId(t, out   k);
        //    System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(k);
        //    p.Kill();
        //    return name;
        //}

        private void saveResult_Click(object sender, EventArgs e)
        {

        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            information.Visible = false;
            information.Focus();
            information.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }


        Int16 GetDT(Int16 fn)
        {
            Int16 dt = 0;
            if (fn != 0)
            {
                fn -= 1;
                dt = (Int16)((((fn / 8) << 8) & 0xFF00) | ((1 << (fn % 8)) & 0x00FF));
            }
            return dt;
        }

        private bool GetSoftwareVersion(ref string version)
        {
            Frame_Stu frame = new Frame_Stu();
            byte[] addr = new byte[4];

            Int16 fcn = GetDT(13);

            byte[] fn = new byte[2];
            fn[0] = (byte)(fcn & 0xff);
            fn[1] = (byte)((fcn >> 8) & 0xff);
            byte[] pn = { 0x00, 0x00 };

            IPPORT_SET portSet = new IPPORT_SET();
            PORT_SET serialPortSet = new PORT_SET();

            if (schemeConfigFrm.chkNet.Checked == true)
            {
                GetNetPort(ref portSet);
                if (bNetOpenFlag == false)
                {
                    if (OpenNetPort(portSet) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        return false;
                    }

                }
            }
            else if (schemeConfigFrm.chkRS485.Checked == true)
            {
                GetSerialPort(ref serialPortSet);
                OpenSerialPort(serialPortSet);
            }
            else
            {
                return false;
            }

            if (GetAddress(ref addr, txtAddr.Text) == false)
            {
                return false;
            }
            if (SendFrame(addr, (byte)GW_AFN_CODE.SetDeviceNo, false, GW_PW_LEN, pn, fn, ref frame, 5, 3) == REC_RESULT.OK)
            {
                byte[] data = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(data);
                version += 'V';
                for (int j = 0; j < data.Length; j++)
                {
                    if (data[j] == 0x00)
                    {
                        break;
                    }
                    version += (char)data[j];
                }

                //   getData = String.Format("{0:X2}", (byte)data[4]) + "年" + String.Format("{0:X2}", (byte)data[3]) + "月" + String.Format("{0:X2}", (byte)data[2]) + "日" + String.Format("{0:X2}", (byte)data[1]) + "时" + String.Format("{0:X2}", (byte)data[1]) + "分";
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool SetDevNo(string str)
        {
            Frame_Stu frame = new Frame_Stu();
            //IPPORT_SET portSet = new IPPORT_SET();
            byte[] date = new byte[6];
            byte[] addr = new byte[4];


            long value = Convert.ToInt32(str);
            if (value < 20100001 || value > 20109999)
            {
                MessageBox.Show("请输入设备编号为20100001 ~ 20109999");
                return false;
            }

            char[] cmd1 = new char[str.Length];
            byte[] dcmd1 = new byte[str.Length];
            cmd1 = str.ToCharArray();
            for (int i = 0; i < cmd1.Length; i++)
            {
                dcmd1[i] = Convert.ToByte(cmd1[i]);
            }

            if (GetAddress(ref addr, txtAddr.Text) == false)
            {
                return false;
            }

            Int16 fcn = GetDT(16);

            byte[] fn = new byte[2];
            fn[0] = (byte)(fcn & 0xff);
            fn[1] = (byte)((fcn >> 8) & 0xff);
            byte[] pn = { 0x00, 0x00 };

            IPPORT_SET portSet = new IPPORT_SET();
            PORT_SET serialPortSet = new PORT_SET();

            if (schemeConfigFrm.chkNet.Checked == true)
            {
                GetNetPort(ref portSet);
                if (bNetOpenFlag == false)
                {
                    if (OpenNetPort(portSet) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        return false;
                    }

                }
            }
            else if (schemeConfigFrm.chkRS485.Checked == true)
            {
                GetSerialPort(ref serialPortSet); ;
                OpenSerialPort(serialPortSet);
            }
            else
            {
                return false;
            }

            if (SendFrame(addr, (byte)GW_AFN_CODE.SetDeviceNo, false, GW_PW_LEN, pn, fn, dcmd1, ref frame, 5, 3) == REC_RESULT.OK)
            {
                byte[] data = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(data);

                //   getData = String.Format("{0:X2}", (byte)data[4]) + "年" + String.Format("{0:X2}", (byte)data[3]) + "月" + String.Format("{0:X2}", (byte)data[2]) + "日" + String.Format("{0:X2}", (byte)data[1]) + "时" + String.Format("{0:X2}", (byte)data[1]) + "分";
                return true;
            }
            else
            {
                return false;
            }

        }

        private void btnRdSoftwareVersion_Click(object sender, EventArgs e)
        {

        }

        private void ABout_Click(object sender, EventArgs e)
        {
            CAbout aboutFrame = new CAbout();
            aboutFrame.Text = "软件说明";
            aboutFrame.labelProductName.Text = "软件名称：模拟量校准软件";
            aboutFrame.labelVersion.Text = String.Format("版本{0} ", " V1.3-2017-0505");
            aboutFrame.labelCopyright.Text = "Copyright";
            aboutFrame.labelCompanyName.Text = "广州众瑞能电子科技有限公司";
            aboutFrame.About.Text = "使用说明:\n"
                                     + "\r\n1.配置通讯串口参数"
                                     + "\r\n2.点击采样值读取按钮，获取采集板采样值信息"
                                     + "\r\n3.点击读取校数按钮，获取采样板校准系数"
                                     + "\r\n4.选择需要进行校数的选框，并输入对应参考源"
                                     + "\r\n5.点击 '计算校数'按钮，软件会根据参考源值和采样值计算出校准系数"
                                     + "\r\n6.点击‘下发校准参数’按钮，将校准参数下发给采集板"
                                     + "\r\n注.点击‘重置系数’按钮，将会对已选择的校数设置成初始系数1.0";
                                     
            aboutFrame.ShowDialog();
        }

        private void comArea_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            CTool toolfrm = new CTool();
            toolfrm.ShowDialog();
        }

        private void btnAddCJQ_Click(object sender, EventArgs e)
        {

        }

        private void btnClearUserDir_Click(object sender, EventArgs e)
        {

            return;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            paramConfig.Visible = false;
            paramConfig.Focus();
            paramConfig.ShowDialog();
        }
        public enum DEVICE_KIND
        {
            KIND_CONTROL = 1,
            KIND_DISPLAY
        }

        public enum DEVICE_TYPE
        {
            MD_RCU = 0,
            MD_AIRCONDITION = 1,
            MD_RELAY_8 = 2,
            MD_LIGHT_4 = 3,
            MD_RELAY_2 = 4,
            MD_LEDV12_3 = 5,
            MD_DOORDISPLAY = 6,
            MD_RFID_CARD = 7,
            MD_KEYBOARD_20 = 8,
            MD_TYPE_MAX,
        }

        public enum T_emFuncType
        {
            FUNC_NORMAL = 0,
            FUNC_LIGHT_UP = 1,//调亮
            FUNC_LIGHT_DOWN = 2,//调暗
            FUNC_LIGHT_AUTO = 3,
            FUNC_LIGHT_AUTO_UP = 4,//打开继电器调亮
            FUNC_LIGHT_AUTO_DOWN = 5,//调暗最后关掉继电器
            FUNC_POWER_KEY = 6,
            FUNC_KEYBACKLIGHT = 7,
            FUNC_NIGHT_LIGHT = 8,
            FUNC_RELAY_ON = 9,//继电器开
            FUNC_RELAY_OFF = 10,//继电器关
            FUNC_WINDOW_OPEN = 11,//门帘开电机打开
            FUNC_WINDOW_CLOSE = 12,//门帘关电机打开
            FUNC_SINGLE_BRAKE = 13,//空调单管阀
            FUNC_AIRCDI_DOUBLE_COLD_BRAKE = 14,//空调双管冷阀
            FUNC_AIRCDI_DOUBLE_HOT_BRAKE = 15,//空调双管热阀
            FUNC_AIRCDI_WIND_SPEED = 16,//空调风机速度控制
            FUNC_AIRCDI_DIGTIAL_BACKLIGHT = 17,//空调数码管背光控制
            FUNC_URGENT_KEY = 18,//紧急按钮
            FUNC_DOOR_CHECK = 19,//大门门磁检测
            FUNC_GALLERY_LIGHT = 20,//廊灯
            FUNC_KEY_CLEANROOM = 21,//清理房间
            FUNC_KEY_DONOT_DISTURB = 22,//请勿打扰
            FUNC_KEY_BELL = 23,//门铃
            FUNC_INPUT_PORT_TRIGLE = 24,//输入端电平吸合触发
            FUNC_INPUT_PORT_HIGH_LEVEL = 25,//输入端高电平触发
            FUNC_INPUT_PORT_LOW_LEVEL = 26,//输入端低电平触发
            FUNC_LIGHT_LEVEL = 27,//灯亮度设置
            FUNC_KEY_WASH_CLOSE = 28,//洗衣服
            FUNC_SCENE_COMPOSE1 = 29,//组合1灯光设置
            FUNC_SCENE_COMPOSE2 = 30,//组合2灯光设置
            FUNC_SCENE_COMPOSE3 = 31,//组合3灯光设置
            FUNC_SCENE_COMPOSE4 = 32,//组合4灯光设置
            FUNC_SCENE_COMPOSE5 = 33,//组合5灯光设置
            FUNC_SCENE_COMPOSE6 = 34,//组合6灯光设置
            FUNC_SCENE_COMPOSE7 = 35,//组合7灯光设置
            FUNC_LEFT_BED_LIGHT = 36,//左床头灯
            FUNC_RIGHT_BED_LIGHT = 37,//右床头灯
            FUNC_MAX
        }
        private UInt32 GetSubMapValue(string strText)
        {
            UInt32 mapdbs = 0;
            string[] str = strText.Split(',');
            for (int i = 0; i < str.Length; i++)
            {
                mapdbs |= (UInt32)(0x01 << (Convert.ToByte(str[i], 10) - 1));
            }
            return mapdbs;
        }
        private void btnParamterLoad_Click(object sender, EventArgs e)
        {
            Frame_Stu frame = new Frame_Stu();

            byte[] data = new byte[512];
            byte[] addr = new byte[4];
            byte[] deviceAddr = new byte[6];
            IPPORT_SET portSet = new IPPORT_SET();
            PORT_SET serialPortSet = new PORT_SET();

            if (schemeConfigFrm.chkNet.Checked == true)
            {
                GetNetPort(ref portSet);
                if (bNetOpenFlag == false)
                {
                    if (OpenNetPort(portSet) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        return;
                    }

                }
            }
            else if (schemeConfigFrm.chkRS485.Checked == true)
            {
                GetSerialPort(ref serialPortSet); ;
                OpenSerialPort(serialPortSet);
            }
            else
            {
                return;
            }

            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return;
            }


            //测量点
            int pos = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //权限密码
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //数据项895D
            data[pos++] = 0x5D;
            data[pos++] = 0x89;

            data[pos++] = 0x00;	//新增

            byte uncheckedmodulenum = 0;
            if (paramConfig.chkDoorcard.Checked == false)
            {
                uncheckedmodulenum++;
            }
            if (paramConfig.chkSelect.Checked == false)
            {
                uncheckedmodulenum++;
            }
            if (paramConfig.chkAircondition.Checked == false)
            {
                uncheckedmodulenum++;
            }
            if (paramConfig.chkOrelay.Checked == false)
            {
                uncheckedmodulenum++;
            }
            if (paramConfig.chkIORelay.Checked == false)
            {
                uncheckedmodulenum++;
            }

            if (paramConfig.chkDoorDisp.Checked == false)
            {
                uncheckedmodulenum++;
            }

            if (paramConfig.ORCheck.Checked == false)
            {
                uncheckedmodulenum++;
            }

            if (paramConfig.chk2IOEnable.Checked == false)
            {
                uncheckedmodulenum++;
            }

            if (paramConfig.chkLine4.Checked == false)
            {
                uncheckedmodulenum++;
            }

            data[pos++] = (byte)(9 - uncheckedmodulenum);//模块数
            UInt32 subno;
            /* 门卡模块*/
            if (paramConfig.chkDoorcard.Checked != false)
            {
                data[pos++] = Convert.ToByte(paramConfig.txtDCNo.Text.ToString(), 10);	//设备序号	
                GetGdMeterAddr(paramConfig.txtDoorCardAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//控制类型
                data[pos++] = (byte)DEVICE_TYPE.MD_RFID_CARD;	//设备类型
                data[pos++] = 0x04;//波特率		
                data[pos++] = 0x00;//关联设备数

            }

            /* 4	路继电器输出*/
            if (paramConfig.chkOrelay.Checked == true)
            {
                data[pos++] = Convert.ToByte(paramConfig.txt4RNo.Text.ToString(), 10);	//设备序号	
                GetGdMeterAddr(paramConfig.txt4RelayAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//控制类型
                data[pos++] = (byte)DEVICE_TYPE.MD_LIGHT_4;	//设备类型
                data[pos++] = 0x04;//波特率		
                data[pos++] = 0x04;//关联设备数
                /* 1路输入*/
                data[pos++] = 0x1;//子路号
                data[pos++] = (byte)(paramConfig.cmbORFunc1.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.txtORCtlNo1.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtORSubno1.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtORSubno1.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 2路输入*/
                data[pos++] = 0x2;//子路号
                data[pos++] = (byte)(paramConfig.cmbORFunc2.SelectedIndex); //特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.txtORCtlNo2.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtORSubno2.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtORSubno2.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 3路输入*/
                data[pos++] = 0x3;//子路号
                data[pos++] = (byte)(paramConfig.cmbORFunc3.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.txtORCtlNo3.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtORSubno3.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtORSubno3.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 4路输入*/
                data[pos++] = 0x4;//子路号
                data[pos++] = (byte)(paramConfig.cmbORFunc4.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.txtORCtlNo4.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtORSubno4.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtORSubno4.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
            }

            /* 空调模块*/
            if (paramConfig.chkAircondition.Checked != false)
            {
                data[pos++] = Convert.ToByte(paramConfig.txtAirNo.Text.ToString(), 10);	//设备序号	
                GetGdMeterAddr(paramConfig.txtAirAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//控制类型
                data[pos++] = (byte)DEVICE_TYPE.MD_AIRCONDITION;	//设备类型
                data[pos++] = 0x04;//波特率		
                data[pos++] = 0x04;//关联设备数
                /*关联设备1*/
                data[pos++] = 0x0;//子路号
                data[pos++] = (byte)(paramConfig.cmbAirsub1Func.SelectedIndex);//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.txtAirCtlDNo1.Text.ToString(), 10);	//关联设备号	        
                subno = 0;
                if (paramConfig.txtAirSNO1.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtAirSNO1.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*关联设备2*/
                data[pos++] = 0x0;//子路号
                data[pos++] = (byte)(paramConfig.cmbAirsub2Func.SelectedIndex);//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.txtAirCtlDNo2.Text.ToString(), 10);	//关联设备号	        
                subno = 0;
                if (paramConfig.txtAirSNO2.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtAirSNO2.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*关联设备3*/
                data[pos++] = 0x0;//子路号
                data[pos++] = (byte)(paramConfig.cmbAirsub3Func.SelectedIndex);//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.txtAirCtlDNo3.Text.ToString(), 10);	//关联设备号	        
                subno = 0;
                if (paramConfig.txtAirSNO3.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtAirSNO3.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*关联设备4*/
                data[pos++] = 0x0;//子路号
                data[pos++] = (byte)(paramConfig.cmbAirsub4Func.SelectedIndex);//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.txtAirCtlDNo4.Text.ToString(), 10);	//关联设备号	        
                subno = 0;
                if (paramConfig.txtAirSNO4.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtAirSNO4.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }

            }
            /* 键盘模块*/
            if (paramConfig.chkSelect.Checked == true)
            {
                data[pos++] = Convert.ToByte(paramConfig.txtKbNo.Text.ToString(), 10);	//设备序号	
                GetGdMeterAddr(paramConfig.txtKbAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//控制类型
                data[pos++] = (byte)DEVICE_TYPE.MD_KEYBOARD_20;	//设备类型
                data[pos++] = 0x04;//波特率		
                data[pos++] = 0x14;//关联设备数
                /*键1关联设备1*/
                data[pos++] = 0x1;//子路号
                data[pos++] = (byte)(paramConfig.cmbK1Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key1CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key1CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key1SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key1SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键2关联设备*/
                data[pos++] = 0x2;//子路号
                data[pos++] = (byte)(paramConfig.cmbK2Func.SelectedIndex);//特殊功能索引
                subno = 0;
                if (paramConfig.key2CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key2CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key2SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key2SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键3关联设备*/
                data[pos++] = 0x3;//子路号
                data[pos++] = (byte)(paramConfig.cmbK3Func.SelectedIndex);//特殊功能索引
                subno = 0;
                if (paramConfig.key3CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key3CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key3SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key3SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键4关联设备*/
                data[pos++] = 0x4;//子路号
                data[pos++] = (byte)(paramConfig.cmbK4Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key4CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key4CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key4SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key4SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键5关联设备*/
                data[pos++] = 0x5;//子路号
                data[pos++] = (byte)(paramConfig.cmbK5Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key5CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key5CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key5SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key5SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键6关联设备*/
                data[pos++] = 0x6;//子路号
                data[pos++] = (byte)(paramConfig.cmbK6Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key6CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key6CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key6SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key6SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键7关联设备*/
                data[pos++] = 0x7;//子路号
                data[pos++] = (byte)(paramConfig.cmbK7Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key7CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key7CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key7SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key7SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键8关联设备*/
                data[pos++] = 0x8;//子路号
                data[pos++] = (byte)(paramConfig.cmbK8Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key8CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key8CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key8SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key8SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键9关联设备*/
                data[pos++] = 0x9;//子路号
                data[pos++] = (byte)(paramConfig.cmbK9Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key9CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key9CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key9SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key9SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键10关联设备*/
                data[pos++] = 0xa;//子路号
                data[pos++] = (byte)(paramConfig.cmbK10Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key10CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key10CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key10SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key10SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键11关联设备*/
                data[pos++] = 0xb;//子路号
                data[pos++] = (byte)(paramConfig.cmbK11Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key11CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key11CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key11SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key11SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键12关联设备*/
                data[pos++] = 0xc;//子路号
                data[pos++] = (byte)(paramConfig.cmbK12Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key12CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key12CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key12SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key12SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键13关联设备*/
                data[pos++] = 0xd;//子路号
                data[pos++] = (byte)(paramConfig.cmbK12Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key13CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key13CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key13SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key13SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键14关联设备*/
                data[pos++] = 0xe;//子路号
                data[pos++] = (byte)(paramConfig.cmbK14Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key14CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key14CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key14SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key14SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键15关联设备*/
                data[pos++] = 0xf;//子路号
                data[pos++] = (byte)(paramConfig.cmbK15Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key15CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key15CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key15SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key15SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键16关联设备*/
                data[pos++] = 0x10;//子路号
                data[pos++] = (byte)(paramConfig.cmbK16Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key16CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key16CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key16SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key16SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键17关联设备*/
                data[pos++] = 0x11;//子路号
                data[pos++] = (byte)(paramConfig.cmbK17Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key17CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key17CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key17SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key17SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键18关联设备*/
                data[pos++] = 0x12;//子路号
                data[pos++] = (byte)(paramConfig.cmbK18Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key18CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key18CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key18SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key18SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键19关联设备*/
                data[pos++] = 0x13;//子路号
                data[pos++] = (byte)(paramConfig.cmbK19Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key19CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key19CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key19SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key19SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
                /*键20关联设备*/
                data[pos++] = 0x14;//子路号
                data[pos++] = (byte)(paramConfig.cmbK20Func.SelectedIndex);//特殊功能索引
                if (paramConfig.key20CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key20CtlDNo.Text.ToString(), 10);	//关联设备号	        
                    subno = 0;
                    if (paramConfig.key20SNO.Text != "")
                    {
                        subno = GetSubMapValue(paramConfig.key20SNO.Text.ToString());
                        data[pos++] = (byte)(subno & 0xff);
                        data[pos++] = (byte)((subno >> 8) & 0xff);
                        data[pos++] = (byte)((subno >> 16) & 0xff);
                        data[pos++] = (byte)((subno >> 24) & 0xff);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            data[pos++] = 0x00;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        data[pos++] = 0x00;
                    }
                }
            }
            /* 12路输入8路输出模块*/
            if (paramConfig.chkIORelay.Checked == true)
            {
                byte deviceno = Convert.ToByte(paramConfig.txt8RNo.Text.ToString(), 10);
                data[pos++] = deviceno;	//设备序号	
                GetGdMeterAddr(paramConfig.txt8RelayAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//控制类型
                data[pos++] = (byte)DEVICE_TYPE.MD_RELAY_8;	//设备类型
                data[pos++] = 0x04;//波特率		
                data[pos++] = 0xC;//关联设备数
                /* 1路输入*/
                data[pos++] = 0x1;//子路号
                data[pos++] = (byte)(paramConfig.cmbIn1Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.In1CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtR8SNO1.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtR8SNO1.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 2路输入*/
                data[pos++] = 0x2;//子路号
                data[pos++] = (byte)(paramConfig.cmbIn2Func.SelectedIndex); //特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.In2CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtR8SNO2.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtR8SNO2.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 3路输入*/
                data[pos++] = 0x3;//子路号
                data[pos++] = (byte)(paramConfig.cmbIn3Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.In3CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtR8SNO3.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtR8SNO3.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 4路输入*/
                data[pos++] = 0x4;//子路号
                data[pos++] = (byte)(paramConfig.cmbIn4Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.In4CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtR8SNO4.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtR8SNO4.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 5路输入*/
                data[pos++] = 0x5;//子路号
                data[pos++] = (byte)(paramConfig.cmbIn5Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.In5CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtR8SNO5.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtR8SNO5.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 6路输入*/
                data[pos++] = 0x6;//子路号
                data[pos++] = (byte)(paramConfig.cmbIn6Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.In6CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtR8SNO6.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtR8SNO6.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 7路输入*/
                data[pos++] = 0x7;//子路号
                data[pos++] = (byte)(paramConfig.cmbIn7Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.In7CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtR8SNO7.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtR8SNO7.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 8路输入*/
                data[pos++] = 0x8;//子路号
                data[pos++] = (byte)(paramConfig.cmbIn8Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.In8CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtR8SNO8.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtR8SNO8.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 9路输入*/
                data[pos++] = 0x9;//子路号
                data[pos++] = (byte)(paramConfig.cmbIn9Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.In9CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtR8SNO9.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtR8SNO9.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 10路输入*/
                data[pos++] = 0xa;//子路号
                data[pos++] = (byte)(paramConfig.cmbIn10Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.In10CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtR8SNO10.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtR8SNO10.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 11路输入*/
                data[pos++] = 0xb;//子路号
                data[pos++] = (byte)(paramConfig.cmbIn11Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.In11CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtR8SNO11.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtR8SNO11.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 12路输入*/
                data[pos++] = 0xC;//子路号
                data[pos++] = (byte)(paramConfig.cmbIn12Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.In12CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txtR8SNO12.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txtR8SNO12.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
            }
            /* 门牌显示模块*/
            if (paramConfig.chkDoorDisp.Checked == true)
            {
                byte deviceno = Convert.ToByte(paramConfig.txtDSNo.Text.ToString(), 10);
                data[pos++] = deviceno;	//设备序号	
                GetGdMeterAddr(paramConfig.txtDoorDispAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//控制类型
                data[pos++] = (byte)DEVICE_TYPE.MD_DOORDISPLAY; 	//设备类型
                data[pos++] = 0x04;//波特率		
                data[pos++] = 0x0;//关联设备数
            }

            /* 12路输入8路输出模块*/
            if (paramConfig.ORCheck.Checked == true)
            {
                byte deviceno = Convert.ToByte(paramConfig.ORtxt8RNo.Text.ToString(), 10);
                data[pos++] = deviceno;	//设备序号	
                GetGdMeterAddr(paramConfig.ORtxt8RelayAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//控制类型
                data[pos++] = (byte)DEVICE_TYPE.MD_RELAY_8;	//设备类型
                data[pos++] = 0x04;//波特率		
                data[pos++] = 0xC;//关联设备数
                /* 1路输入*/
                data[pos++] = 0x1;//子路号
                data[pos++] = (byte)(paramConfig.ORcmbIn1Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.ORIn1CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.ORtxtR8SNO1.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.ORtxtR8SNO1.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 2路输入*/
                data[pos++] = 0x2;//子路号
                data[pos++] = (byte)(paramConfig.ORcmbIn2Func.SelectedIndex); //特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.ORIn2CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.ORtxtR8SNO2.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.ORtxtR8SNO2.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 3路输入*/
                data[pos++] = 0x3;//子路号
                data[pos++] = (byte)(paramConfig.ORcmbIn3Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.ORIn3CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.ORtxtR8SNO3.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.ORtxtR8SNO3.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 4路输入*/
                data[pos++] = 0x4;//子路号
                data[pos++] = (byte)(paramConfig.ORcmbIn4Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.ORIn4CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.ORtxtR8SNO4.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.ORtxtR8SNO4.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 5路输入*/
                data[pos++] = 0x5;//子路号
                data[pos++] = (byte)(paramConfig.ORcmbIn5Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.ORIn5CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.ORtxtR8SNO5.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.ORtxtR8SNO5.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 6路输入*/
                data[pos++] = 0x6;//子路号
                data[pos++] = (byte)(paramConfig.ORcmbIn6Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.ORIn6CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.ORtxtR8SNO6.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.ORtxtR8SNO6.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 7路输入*/
                data[pos++] = 0x7;//子路号
                data[pos++] = (byte)(paramConfig.ORcmbIn7Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.ORIn7CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.ORtxtR8SNO7.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.ORtxtR8SNO7.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 8路输入*/
                data[pos++] = 0x8;//子路号
                data[pos++] = (byte)(paramConfig.ORcmbIn8Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.ORIn8CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.ORtxtR8SNO8.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.ORtxtR8SNO8.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 9路输入*/
                data[pos++] = 0x9;//子路号
                data[pos++] = (byte)(paramConfig.ORcmbIn9Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.ORIn9CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.ORtxtR8SNO9.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.ORtxtR8SNO9.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 10路输入*/
                data[pos++] = 0xa;//子路号
                data[pos++] = (byte)(paramConfig.ORcmbIn10Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.ORIn10CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.ORtxtR8SNO10.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.ORtxtR8SNO10.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 11路输入*/
                data[pos++] = 0xb;//子路号
                data[pos++] = (byte)(paramConfig.ORcmbIn11Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.ORIn11CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.ORtxtR8SNO11.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.ORtxtR8SNO11.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 12路输入*/
                data[pos++] = 0xC;//子路号
                data[pos++] = (byte)(paramConfig.ORcmbIn12Func.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.ORIn12CtlDNo.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.ORtxtR8SNO12.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.ORtxtR8SNO12.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
            }

            /* 4路输入2路输出模块*/
            if (paramConfig.chk2IOEnable.Checked == true)
            {
                byte deviceno = Convert.ToByte(paramConfig.txt2IONo.Text.ToString(), 10);
                data[pos++] = deviceno;	//设备序号	
                GetGdMeterAddr(paramConfig.txt2IODevAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//控制类型
                data[pos++] = (byte)DEVICE_TYPE.MD_RELAY_2;	//设备类型
                data[pos++] = 0x04;//波特率		
                data[pos++] = 0x4;//关联设备数
                /* 1路输入*/
                data[pos++] = 0x1;//子路号
                data[pos++] = (byte)(paramConfig.cmb2IOFunc1.SelectedIndex); ;//特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.txt2IOCtlNo1.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txt2IOSubno1.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txt2IOSubno1.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 2路输入*/
                data[pos++] = 0x2;//子路号
                data[pos++] = (byte)(paramConfig.cmb2IOFunc2.SelectedIndex); //特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.txt2IOCtlNo1.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txt2IOSubno2.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txt2IOSubno2.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 3路输入*/
                data[pos++] = 0x3;//子路号
                data[pos++] = (byte)(paramConfig.cmb2IOFunc3.SelectedIndex); //特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.txt2IOCtlNo3.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txt2IOSubno3.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txt2IOSubno3.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
                /* 4路输入*/
                data[pos++] = 0x4;//子路号
                data[pos++] = (byte)(paramConfig.cmb2IOFunc4.SelectedIndex); //特殊功能索引
                data[pos++] = Convert.ToByte(paramConfig.txt2IOCtlNo4.Text.ToString(), 10);
                subno = 0;
                if (paramConfig.txt2IOSubno4.Text != "")
                {
                    subno = GetSubMapValue(paramConfig.txt2IOSubno4.Text.ToString());
                    data[pos++] = (byte)(subno & 0xff);
                    data[pos++] = (byte)((subno >> 8) & 0xff);
                    data[pos++] = (byte)((subno >> 16) & 0xff);
                    data[pos++] = (byte)((subno >> 24) & 0xff);
                }
            }
            /* 四线插座模块*/
            if (paramConfig.chkLine4.Checked != false)
            {
                data[pos++] = Convert.ToByte(paramConfig.txtLine4DevNo.Text.ToString(), 10);	//设备序号	
                GetGdMeterAddr(paramConfig.txtLine4Addr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//控制类型
                data[pos++] = (byte)DEVICE_TYPE.MD_LIGHT_4;	//设备类型
                data[pos++] = 0x04;//波特率		
                data[pos++] = 0x00;//关联设备数

            }


            if (SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, pos, ref frame, 15, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                if (rcdata[2] == 0x5D && rcdata[3] == 0x89 && rcdata[4] == 0x00)
                {
                    MessageBox.Show("操作成功");
                }
                else
                {
                    MessageBox.Show("操作失败");
                }
            }
            else
            {
                MessageBox.Show("操作失败");
            }
            ClosePort();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Frame_Stu frame = new Frame_Stu();

            byte[] data = new byte[400];
            byte[] addr = new byte[4];
            byte[] deviceAddr = new byte[6];
            IPPORT_SET portSet = new IPPORT_SET();
            PORT_SET serialPortSet = new PORT_SET();

            if (schemeConfigFrm.chkNet.Checked == true)
            {
                GetNetPort(ref portSet);
                if (bNetOpenFlag == false)
                {
                    if (OpenNetPort(portSet) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        return;
                    }

                }
            }
            else if (schemeConfigFrm.chkRS485.Checked == true)
            {
                GetSerialPort(ref serialPortSet); ;
                OpenSerialPort(serialPortSet);
            }
            else
            {
                return;
            }


            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return;
            }


            //测量点
            int pos = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //权限密码
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //数据项895D
            data[pos++] = 0x5D;
            data[pos++] = 0x89;

            data[pos++] = 0x02;	//删除
            data[pos++] = 0;//全部删除

            if (SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, pos, ref frame, 30, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                if (rcdata[2] == 0x5D && rcdata[3] == 0x89 && rcdata[4] == 0x00)
                {
                    MessageBox.Show("操作成功");
                }
                else
                {
                    MessageBox.Show("操作成功");
                }
            }
            else
            {
                MessageBox.Show("操作失败");
            }
            ClosePort();
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            frmSet.Visible = false;
            frmSet.Focus();
            frmSet.ShowDialog();
        }

        private void btnSetAction_Click(object sender, EventArgs e)
        {
            Frame_Stu frame = new Frame_Stu();

            byte[] data = new byte[400];
            byte[] addr = new byte[4];
            byte[] deviceAddr = new byte[6];
            IPPORT_SET portSet = new IPPORT_SET();
            PORT_SET serialPortSet = new PORT_SET();

            if (schemeConfigFrm.chkNet.Checked == true)
            {
                GetNetPort(ref portSet);
                if (bNetOpenFlag == false)
                {
                    if (OpenNetPort(portSet) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        return;
                    }

                }
            }
            else if (schemeConfigFrm.chkRS485.Checked == true)
            {
                GetSerialPort(ref serialPortSet); ;
                OpenSerialPort(serialPortSet);
            }
            else
            {
                return;
            }

            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return;
            }


            //测量点
            int pos = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //权限密码
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //数据项0101
            data[pos++] = 0x01;
            data[pos++] = 0x01;
            data[pos++] = Convert.ToByte(frmSet.txtNightEndMin.Text.ToString(), 16);
            data[pos++] = Convert.ToByte(frmSet.txtNightEndHour.Text.ToString(), 16);
            data[pos++] = Convert.ToByte(frmSet.txtNightStartMin.Text.ToString(), 16);
            data[pos++] = Convert.ToByte(frmSet.txtNightStartHour.Text.ToString(), 16);


            //数据项0102
            data[pos++] = 0x02;
            data[pos++] = 0x01;
            data[pos++] = Convert.ToByte(frmSet.txtLightLeve.Text.ToString(), 16);

            //数据项0103
            data[pos++] = 0x03;
            data[pos++] = 0x01;
            data[pos++] = Convert.ToByte(frmSet.txtMotorRunTime.Text.ToString(), 16);

            //数据项0104
            data[pos++] = 0x04;
            data[pos++] = 0x01;
            data[pos++] = (byte)(frmSet.cmbAirSeason.SelectedIndex + 1);


            //数据项0105
            data[pos++] = 0x05;
            data[pos++] = 0x01;
            data[pos++] = Convert.ToByte(frmSet.txtSumerDegree.Text.ToString(), 16);
            data[pos++] = Convert.ToByte(frmSet.txtWinterDegree.Text.ToString(), 16);

            //数据项0108
            data[pos++] = 0x08;
            data[pos++] = 0x01;
            if (frmSet.cmbDoorCardType.SelectedIndex == 1)
            {
                data[pos] |= (byte)0x01;
            }

            if (frmSet.cmbDoorDisplaytype.SelectedIndex == 1)
            {
                data[pos] |= (byte)0x02;
            }

            if (frmSet.comBusRule.SelectedIndex == 1)
            {
                data[pos] |= (byte)0x04;
            }
            else if (frmSet.comBusRule.SelectedIndex == 2)
            {
                data[pos] |= (byte)0x08;
            }
            pos++;

            //数据项0109
            data[pos++] = 0x09;
            data[pos++] = 0x01;
            data[pos++] = Convert.ToByte(frmSet.txtIrdaTime.Text.ToString(), 10);

            if (SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, pos, ref frame, 15, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                MessageBox.Show("操作成功");
                /*if (rcdata[2] == 0x5D && rcdata[3] == 0x89 && rcdata[4] == 0x00)
                {
                    MessageBox.Show("操作成功");    
                }
                else
                {
                    MessageBox.Show("操作失败");
                }*/
            }
            else
            {
                MessageBox.Show("操作失败");
            }
            ClosePort();
        }
        private bool Quereflag = false;
        private void btnQuere_Click(object sender, EventArgs e)
        {
            //if (Quereflag == false)
            //{
            //    Quereflag = true;
            //    thread = new Thread(new ThreadStart(RunInfoInquiry));
            //    thread.Start();
            //    btnQuere.Text = "停止查询";
            //    btnClear.Enabled = false;
            //    btnParamterLoad.Enabled = false;
            //    btnSetAction.Enabled = false;
            //    btnReadSet.Enabled = false;
            //    btnLoad.Enabled = false;
            //    btnSet.Enabled = false;
            //    btnRcuState.Enabled = false;
            //    btnIrdtSet.Enabled = false;
            //    btnDoorSet.Enabled = false;
            //    btnSceneSet.Enabled = false;
            //    btnPowKeyScene.Enabled = false;
            //    btnDrSet.Enabled = false;
            //    btnCtlCommand.Enabled = false;
            //}
            //else
            //{
            //    Quereflag = false;
            //    ClosePort();
            //    thread.Abort();
            //    btnQuere.Text = "启动查询";
            //    btnClear.Enabled = true;
            //    btnParamterLoad.Enabled = true;
            //    btnSetAction.Enabled = true;
            //    btnReadSet.Enabled = true;
            //    btnLoad.Enabled = true;
            //    btnSet.Enabled = true;
            //    btnRcuState.Enabled = true;
            //    btnIrdtSet.Enabled = true;
            //    btnDoorSet.Enabled = true;
            //    btnSceneSet.Enabled = true;
            //    btnPowKeyScene.Enabled = true;
            //    btnDrSet.Enabled = true;
            //    btnCtlCommand.Enabled = true;
            //}

        }


        private bool SetSystemclock()
        {
            Frame_Stu frame = new Frame_Stu();

            byte[] data = new byte[14];
            byte[] addr = new byte[4];

            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return false;
            }

            string year = DateTime.Now.Year.ToString();
            while (true)
            {
                if (year.Length > 2)
                {
                    year = year.Remove(0, 1);
                }
                else
                {
                    break;
                }
            }

            //测量点
            data[0] = 0x00;
            data[1] = 0x00;
            //权限密码
            data[2] = 0x11;
            data[3] = 0x11;
            data[4] = 0x11;
            data[5] = 0x11;
            //数据项8030
            data[6] = 0x30;
            data[7] = 0x80;
            //秒分时日月年
            data[8] = Convert.ToByte(DateTime.Now.Second.ToString(), 16);
            data[9] = Convert.ToByte(DateTime.Now.Minute.ToString(), 16);
            data[10] = Convert.ToByte(DateTime.Now.Hour.ToString(), 16);
            data[11] = Convert.ToByte(DateTime.Now.Day.ToString(), 16);

            data[12] = Convert.ToByte(DateTime.Now.Month.ToString(), 16);
            byte month = data[12];

            data[13] = Convert.ToByte(year, 16);

            SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, ref frame, 15, 3);
            return true;
        }

        private void RunInfoInquiry()
        {
            Frame_Stu frame = new Frame_Stu();

            byte[] data = new byte[400];
            byte[] addr = new byte[4];
            byte[] deviceAddr = new byte[6];
            IPPORT_SET portSet = new IPPORT_SET();
            PORT_SET serialPortSet = new PORT_SET();

            if (schemeConfigFrm.chkNet.Checked == true)
            {
                GetNetPort(ref portSet);
                if (bNetOpenFlag == false)
                {
                    if (OpenNetPort(portSet) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        return;
                    }

                }
            }
            else if (schemeConfigFrm.chkRS485.Checked == true)
            {
                GetSerialPort(ref serialPortSet); ;
                OpenSerialPort(serialPortSet);
            }
            else
            {
                return;
            }

            SetSystemclock();

            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return;
            }


            //测量点
            int pos = 0;
            data[pos++] = 0x01;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;

            //数据项0106
            data[pos++] = 0x06;
            data[pos++] = 0x01;

            //数据项0107
            data[pos++] = 0x07;
            data[pos++] = 0x01;

            while (true)
            {
                if (SendFrame(addr, (byte)GD_AFN_CODE.GetRealtimeParam, data, pos, ref frame, 15, 3) == REC_RESULT.OK)
                {
                    byte[] rcdata = new byte[frame.dataArray.Count];
                    frame.dataArray.CopyTo(rcdata);
                    /*if (frame.dataArray.Count > 9 && rcdata[8] == 0x06 && rcdata[9] == 0x01)
                    {
                        /*
                            bit0紧急按钮状态：0，未按下，1，已按下
                            Bit1房间门卡插入状态：0，未插卡，1已插卡
                            Bit2 房间大门磁检测：0，大门关，1大门开
                            Bit3 总电源开关状态：0：关，1：开。
                            Bit4 廊灯开关状态 0：关，1：开。
                            Bit5 夜灯开关状态 0：关，1：开。
                            Bit6 空调季节状态 0：夏季，1：冬季。
                            Bit7 清理房间按钮状态0：关，1：开。
                            Bit8 洗衣服务按钮状态0：关，1：开。
                            Bit9 请勿打扰状态0：关，1：开。

                            Bit8~31 保留。
                         *
                        if ((byte)(rcdata[10] & 0x01) == (byte)0x01)
                        {
                            lvRunInfo.Items[0].SubItems[1].Text = "已按下";
                        }
                        else
                        {
                            lvRunInfo.Items[0].SubItems[1].Text = "未按下";
                        }

                        if ((byte)(rcdata[10] & 0x02) == (byte)0x02)
                        {
                            lvRunInfo.Items[1].SubItems[1].Text = "已插卡";
                        }
                        else
                        {
                            lvRunInfo.Items[1].SubItems[1].Text = "未插卡";
                        }

                        if ((byte)(rcdata[10] & 0x04) == (byte)0x04)
                        {
                            lvRunInfo.Items[2].SubItems[1].Text = "大门开";
                        }
                        else
                        {
                            lvRunInfo.Items[2].SubItems[1].Text = "大门关";
                        }
                        if ((byte)(rcdata[10] & 0x08) == (byte)0x08)
                        {
                            lvRunInfo.Items[3].SubItems[1].Text = "开";
                        }
                        else
                        {
                            lvRunInfo.Items[3].SubItems[1].Text = "关";
                        }
                        if ((byte)(rcdata[10] & 0x10) == (byte)0x10)
                        {
                            lvRunInfo.Items[4].SubItems[1].Text = "开";
                        }
                        else
                        {
                            lvRunInfo.Items[4].SubItems[1].Text = "关";
                        }
                        if ((byte)(rcdata[10] & 0x20) == (byte)0x20)
                        {
                            lvRunInfo.Items[5].SubItems[1].Text = "开";
                        }
                        else
                        {
                            lvRunInfo.Items[5].SubItems[1].Text = "关";
                        }
                        if ((byte)(rcdata[10] & 0x40) == (byte)0x40)
                        {
                            lvRunInfo.Items[6].SubItems[1].Text = "冬季";
                        }
                        else
                        {
                            lvRunInfo.Items[6].SubItems[1].Text = "夏季";
                        }
                        if ((byte)(rcdata[10] & 0x80) == (byte)0x80)
                        {
                            lvRunInfo.Items[7].SubItems[1].Text = "开";
                        }
                        else
                        {
                            lvRunInfo.Items[7].SubItems[1].Text = "关";
                        }
                        if ((byte)(rcdata[11] & 0x01) == (byte)0x01)
                        {
                            lvRunInfo.Items[8].SubItems[1].Text = "是";
                        }
                        else
                        {
                            lvRunInfo.Items[8].SubItems[1].Text = "否";
                        }
                        if ((byte)(rcdata[11] & 0x02) == (byte)0x02)
                        {
                            lvRunInfo.Items[9].SubItems[1].Text = "开";
                        }
                        else
                        {
                            lvRunInfo.Items[9].SubItems[1].Text = "关";
                        }

                        lvRunInfo.Items[10].SubItems[1].Text = String.Format("{0:D2}", (byte)rcdata[16]) + " 度";
                        lvRunInfo.Items[11].SubItems[1].Text = String.Format("{0:D2}", (byte)rcdata[17]) + " 度";
                        lvRunInfo.Items[12].SubItems[1].Text = String.Format("{0:D2}", (byte)rcdata[18]) + " 档";
                    }
                    else
                    {

                    }*/
                }
                System.Threading.Thread.Sleep(10);
            }


            ClosePort();
            thread.Abort();
        }

        private void btnReadSet_Click(object sender, EventArgs e)
        {
            Frame_Stu frame = new Frame_Stu();

            byte[] data = new byte[400];
            byte[] addr = new byte[4];
            byte[] deviceAddr = new byte[6];
            IPPORT_SET portSet = new IPPORT_SET();
            PORT_SET serialPortSet = new PORT_SET();

            if (schemeConfigFrm.chkNet.Checked == true)
            {
                GetNetPort(ref portSet);
                if (bNetOpenFlag == false)
                {
                    if (OpenNetPort(portSet) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        return;
                    }

                }
            }
            else if (schemeConfigFrm.chkRS485.Checked == true)
            {
                GetSerialPort(ref serialPortSet); ;
                OpenSerialPort(serialPortSet);
            }
            else
            {
                return;
            }

            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return;
            }


            //测量点
            int pos = 0;
            data[pos++] = 0x01;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;

            //数据项0101
            data[pos++] = 0x01;
            data[pos++] = 0x01;

            //数据项0102
            data[pos++] = 0x02;
            data[pos++] = 0x01;

            //数据项0103
            data[pos++] = 0x03;
            data[pos++] = 0x01;

            //数据项0104
            data[pos++] = 0x04;
            data[pos++] = 0x01;

            //数据项0105
            data[pos++] = 0x05;
            data[pos++] = 0x01;

            //数据项0108
            data[pos++] = 0x08;
            data[pos++] = 0x01;
            //数据项0109
            data[pos++] = 0x09;
            data[pos++] = 0x01;


            if (SendFrame(addr, (byte)GD_AFN_CODE.GetRealtimeParam, data, pos, ref frame, 15, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                if (rcdata[8] == 0x01 && rcdata[9] == 0x01)
                {
                    frmSet.txtNightEndMin.Text = String.Format("{0:X2}", (byte)rcdata[10]);
                    frmSet.txtNightEndHour.Text = String.Format("{0:X2}", (byte)rcdata[11]);
                    frmSet.txtNightStartMin.Text = String.Format("{0:X2}", (byte)rcdata[12]);
                    frmSet.txtNightStartHour.Text = String.Format("{0:X2}", (byte)rcdata[13]);
                }
                if (rcdata[14] == 0x02 && rcdata[15] == 0x01)
                {
                    frmSet.txtLightLeve.Text = String.Format("{0:X2}", (byte)rcdata[16]);
                }
                if (rcdata[17] == 0x03 && rcdata[18] == 0x01)
                {
                    frmSet.txtMotorRunTime.Text = String.Format("{0:X2}", (byte)rcdata[19]);
                }

                if (rcdata[20] == 0x04 && rcdata[21] == 0x01)
                {
                    frmSet.cmbAirSeason.SelectedIndex = (byte)(rcdata[22] - 1);
                }
                if (rcdata[23] == 0x05 && rcdata[24] == 0x01)
                {
                    frmSet.txtSumerDegree.Text = String.Format("{0:X2}", (byte)rcdata[25]);
                    frmSet.txtWinterDegree.Text = String.Format("{0:X2}", (byte)rcdata[26]);
                }
                if (rcdata[27] == 0x08 && rcdata[28] == 0x01)
                {
                    frmSet.cmbDoorCardType.SelectedIndex = (byte)(rcdata[29] & 0x01);
                    frmSet.cmbDoorDisplaytype.SelectedIndex = (byte)((rcdata[29] >> 1) & 0x01);
                    frmSet.comBusRule.SelectedIndex = (byte)((rcdata[29] >> 2) & 0x01);
                }
                if (rcdata[30] == 0x09 && rcdata[31] == 0x01)
                {
                    frmSet.txtIrdaTime.Text = String.Format("{0:X2}", (byte)rcdata[32]);
                }
            }
            ClosePort();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            powkeySceneSet.Visible = false;
            powkeySceneSet.Focus();
            powkeySceneSet.ShowDialog();
        }

        private void btnPowKeyScene_Click(object sender, EventArgs e)
        {
            Frame_Stu frame = new Frame_Stu();

            byte[] data = new byte[400];
            byte[] addr = new byte[4];
            byte[] deviceAddr = new byte[6];
            IPPORT_SET portSet = new IPPORT_SET();
            PORT_SET serialPortSet = new PORT_SET();

            if (schemeConfigFrm.chkNet.Checked == true)
            {
                GetNetPort(ref portSet);
                if (bNetOpenFlag == false)
                {
                    if (OpenNetPort(portSet) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        return;
                    }

                }
            }
            else if (schemeConfigFrm.chkRS485.Checked == true)
            {
                GetSerialPort(ref serialPortSet); ;
                OpenSerialPort(serialPortSet);
            }
            else
            {
                return;
            }

            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return;
            }


            //测量点
            int pos = 0;
            byte memberNum = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //权限密码
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //数据项0140
            data[pos++] = 0x40;
            data[pos++] = 0x01;

            data[pos++] = memberNum;

            if (powkeySceneSet.PowChk1.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo1.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO1.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK1cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam1.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (powkeySceneSet.PowChk2.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo2.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO2.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK2cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam2.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (powkeySceneSet.PowChk3.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo3.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO3.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK3cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam3.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (powkeySceneSet.PowChk4.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo4.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO4.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK4cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam4.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (powkeySceneSet.PowChk5.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo5.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO5.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK5cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam5.Text.ToString(), 10);
                data[pos++] = (byte)((byte)(dataArea & 0xff));
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (powkeySceneSet.PowChk6.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo6.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO6.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK6cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam6.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (powkeySceneSet.PowChk7.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo7.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO7.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK7cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam7.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (powkeySceneSet.PowChk8.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo8.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO8.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK8cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam8.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (powkeySceneSet.PowChk9.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo9.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO9.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK9cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam9.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (powkeySceneSet.PowChk10.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo10.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO10.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK10cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam10.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (powkeySceneSet.PowChk11.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo11.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO11.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK11cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam11.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (powkeySceneSet.PowChk12.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo12.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO12.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK12cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam12.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }
            if (powkeySceneSet.PowChk13.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo13.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO13.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK13cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam13.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (powkeySceneSet.PowChk14.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo14.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO14.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK14cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam14.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }
            if (powkeySceneSet.PowChk15.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo15.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO15.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK15cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam15.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (powkeySceneSet.PowChk16.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo16.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO16.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK16cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam16.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (powkeySceneSet.PowChk17.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo17.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO17.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK17cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam17.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (powkeySceneSet.PowChk18.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo18.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO18.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK18cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam18.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (powkeySceneSet.PowChk19.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo19.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO19.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK19cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam19.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (powkeySceneSet.PowChk20.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo20.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO20.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK20cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(powkeySceneSet.powkeyParam20.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            data[8] = memberNum;
            if (SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, pos, ref frame, 15, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                if (rcdata[2] == 0x40 && rcdata[3] == 0x01 && rcdata[4] == 0x00)
                {
                    MessageBox.Show("操作成功");
                }
                else
                {
                    MessageBox.Show("操作失败");
                }
            }
            else
            {
                MessageBox.Show("操作失败");
            }
            ClosePort();

        }

        private void btnDoorcardScene_Click(object sender, EventArgs e)
        {
            doorcardSceneSet.Visible = false;
            doorcardSceneSet.Focus();
            doorcardSceneSet.ShowDialog();
        }

        private void btnDrSet_Click(object sender, EventArgs e)
        {
            Frame_Stu frame = new Frame_Stu();

            byte[] data = new byte[400];
            byte[] addr = new byte[4];
            byte[] deviceAddr = new byte[6];
            IPPORT_SET portSet = new IPPORT_SET();
            PORT_SET serialPortSet = new PORT_SET();

            if (schemeConfigFrm.chkNet.Checked == true)
            {
                GetNetPort(ref portSet);
                if (bNetOpenFlag == false)
                {
                    if (OpenNetPort(portSet) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        return;
                    }

                }
            }
            else if (schemeConfigFrm.chkRS485.Checked == true)
            {
                GetSerialPort(ref serialPortSet); ;
                OpenSerialPort(serialPortSet);
            }
            else
            {
                return;
            }

            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return;
            }


            //测量点
            int pos = 0;
            byte memberNum = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //权限密码
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //数据项0142
            data[pos++] = 0x42;
            data[pos++] = 0x01;

            data[pos++] = memberNum;

            if (doorcardSceneSet.DrChk1.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo1.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO1.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK1cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardParam1.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrChk2.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo2.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO2.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK2cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardParam2.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrChk3.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo3.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO3.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK3cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardParam3.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (doorcardSceneSet.DrChk4.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo4.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO4.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK4cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardParam4.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (doorcardSceneSet.DrChk5.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo5.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO5.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK5cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardParam5.Text.ToString(), 10);
                data[pos++] = (byte)((byte)(dataArea & 0xff));
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (doorcardSceneSet.DrChk6.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo6.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO6.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK6cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardParam6.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (doorcardSceneSet.DrChk7.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo7.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO7.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK7cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardParam7.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (doorcardSceneSet.DrChk8.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo8.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO8.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK8cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardParam8.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (doorcardSceneSet.DrChk9.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo9.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO9.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK9cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardParam9.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (doorcardSceneSet.DrChk10.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo10.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO10.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK10cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardyParam10.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrChk11.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo11.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO11.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK11cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardyParam11.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrChk12.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo12.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO12.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK12cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardyParam12.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }
            if (doorcardSceneSet.DrChk13.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo13.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO13.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK13cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardyParam13.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrChk14.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo14.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO14.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK14cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardyParam14.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrChk15.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo15.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO15.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK15cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardyParam15.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrChk16.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo16.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO16.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK16cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardyParam16.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrChk17.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo17.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO17.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK17cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardyParam17.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrChk18.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo18.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO18.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK18cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardyParam18.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrChk19.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo19.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO19.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK19cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardyParam19.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrChk20.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo20.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO20.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK20cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardyParam20.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }
            data[8] = memberNum;

            //数据项0143
            data[pos++] = 0x43;
            data[pos++] = 0x01;
            int tmppos = pos;
            memberNum = 0;
            data[pos++] = memberNum;

            if (doorcardSceneSet.DrOutChk1.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo1.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO1.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK1cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam1.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrOutChk2.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo2.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO2.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK2cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam2.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrOutChk3.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo3.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO3.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK3cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardParam3.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (doorcardSceneSet.DrOutChk4.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo4.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO4.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK4cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam4.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (doorcardSceneSet.DrOutChk5.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo5.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO5.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK5cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam5.Text.ToString(), 10);
                data[pos++] = (byte)((byte)(dataArea & 0xff));
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (doorcardSceneSet.DrOutChk6.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo6.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO6.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK6cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam6.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (doorcardSceneSet.DrOutChk7.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo7.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO7.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK7cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam7.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (doorcardSceneSet.DrOutChk8.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo8.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO8.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK8cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam8.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (doorcardSceneSet.DrOutChk9.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo9.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO9.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK9cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam9.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (doorcardSceneSet.DrOutChk10.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo10.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO10.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK10cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam10.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrOutChk11.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo11.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO11.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK11cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam11.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrOutChk12.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo12.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO12.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK12cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam12.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrOutChk13.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo13.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO13.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK13cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam13.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrOutChk14.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo14.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO14.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK14cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam14.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrOutChk15.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo15.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO15.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK15cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam15.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrOutChk16.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo16.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO16.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK16cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam16.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrOutChk17.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo17.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO17.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK17cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam17.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrOutChk18.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo18.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO18.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK18cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam18.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrOutChk19.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo19.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO19.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK19cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam19.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorcardSceneSet.DrOutChk20.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutCtlDNo20.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardOutSNO20.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK20cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardOutParam20.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }
            data[tmppos] = memberNum;

            if (SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, pos, ref frame, 15, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                if (rcdata[2] == 0x42 && rcdata[3] == 0x01 && rcdata[4] == 0x00)
                {
                    MessageBox.Show("操作成功");
                }
                else
                {
                    MessageBox.Show("操作失败");
                }
            }
            else
            {
                MessageBox.Show("操作失败");
            }
            ClosePort();
        }

        private void btnScene_Click(object sender, EventArgs e)
        {
            sceneSet.Visible = false;
            sceneSet.Focus();
            sceneSet.ShowDialog();
        }

        private void btnSceneSet_Click(object sender, EventArgs e)
        {
            Frame_Stu frame = new Frame_Stu();

            byte[] data = new byte[400];
            byte[] addr = new byte[4];
            byte[] deviceAddr = new byte[6];
            IPPORT_SET portSet = new IPPORT_SET();
            PORT_SET serialPortSet = new PORT_SET();

            if (schemeConfigFrm.chkNet.Checked == true)
            {
                GetNetPort(ref portSet);
                if (bNetOpenFlag == false)
                {
                    if (OpenNetPort(portSet) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        return;
                    }

                }
            }
            else if (schemeConfigFrm.chkRS485.Checked == true)
            {
                GetSerialPort(ref serialPortSet); ;
                OpenSerialPort(serialPortSet);
            }
            else
            {
                return;
            }

            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return;
            }


            //测量点
            int pos = 0;
            int memberNumPos = 0;
            byte memberNum = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //权限密码
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //数据项0111
            data[pos++] = 0x11;
            data[pos++] = 0x01;
            memberNumPos = pos;
            data[pos++] = memberNum;

            if (sceneSet.Scene1Chk1.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene1CtlDNo1.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene1SNO1.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene1K1cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene1Param1.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (sceneSet.Scene1Chk2.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene1CtlDNo2.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene1SNO2.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene1K2cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene1Param2.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (sceneSet.Scene1Chk3.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene1CtlDNo3.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene1SNO3.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene1K3cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene1Param3.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (sceneSet.Scene1Chk4.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene1CtlDNo4.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene1SNO4.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene1K4cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene1Param4.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (sceneSet.Scene1Chk5.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene1CtlDNo5.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene1SNO5.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene1K5cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene1Param5.Text.ToString(), 10);
                data[pos++] = (byte)((byte)(dataArea & 0xff));
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (sceneSet.Scene1Chk6.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene1CtlDNo6.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene1SNO6.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene1K6cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene1Param6.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }



            data[memberNumPos] = memberNum;


            /* 场景二参数设置*/
            data[pos++] = 0x12;
            data[pos++] = 0x01;
            memberNumPos = pos;
            memberNum = 0;
            data[pos++] = memberNum;

            if (sceneSet.Scene2Chk1.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene2CtlDNo1.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene2SNO1.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene2K1cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene2Param1.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (sceneSet.Scene2Chk2.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene2CtlDNo2.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene2SNO2.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene2K2cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene2Param2.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (sceneSet.Scene2Chk3.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene2CtlDNo3.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene2SNO3.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene2K3cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene2Param3.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (sceneSet.Scene2Chk4.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene2CtlDNo4.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene2SNO4.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene2K4cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene2Param4.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (sceneSet.Scene2Chk5.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene2CtlDNo5.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene2SNO5.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene2K5cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene2Param5.Text.ToString(), 10);
                data[pos++] = (byte)((byte)(dataArea & 0xff));
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (sceneSet.Scene2Chk6.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene2CtlDNo6.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene2SNO6.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene2K6cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene2Param6.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            data[memberNumPos] = memberNum;

            /* 场景三参数设置*/
            data[pos++] = 0x13;
            data[pos++] = 0x01;
            memberNumPos = pos;
            memberNum = 0;
            data[pos++] = memberNum;

            if (sceneSet.Scene3Chk1.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene3CtlDNo1.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene3SNO1.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene3K1cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene3Param1.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (sceneSet.Scene3Chk2.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene3CtlDNo2.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene3SNO2.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene3K2cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene3Param2.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (sceneSet.Scene3Chk3.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene3CtlDNo3.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene3SNO3.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene3K3cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene3Param3.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (sceneSet.Scene3Chk4.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene3CtlDNo4.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene3SNO4.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene3K4cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene3Param4.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (sceneSet.Scene3Chk5.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene3CtlDNo5.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene3SNO5.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene3K5cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene3Param5.Text.ToString(), 10);
                data[pos++] = (byte)((byte)(dataArea & 0xff));
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (sceneSet.Scene3Chk6.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene3CtlDNo6.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene3SNO6.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene3K6cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene3Param6.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            data[memberNumPos] = memberNum;

            if (SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, pos, ref frame, 15, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                if (rcdata[2] == 0x11 && rcdata[3] == 0x01 && rcdata[4] == 0x00)
                {
                    MessageBox.Show("操作成功");
                }
                else
                {
                    MessageBox.Show("操作失败");
                }
            }
            else
            {
                MessageBox.Show("操作失败");
            }
            ClosePort();
        }

        private void btnCtlCommand_Click(object sender, EventArgs e)
        {
            Frame_Stu frame = new Frame_Stu();

            byte[] data = new byte[400];
            byte[] addr = new byte[4];
            byte[] deviceAddr = new byte[6];
            IPPORT_SET portSet = new IPPORT_SET();
            PORT_SET serialPortSet = new PORT_SET();

            if (schemeConfigFrm.chkNet.Checked == true)
            {
                GetNetPort(ref portSet);
                if (bNetOpenFlag == false)
                {
                    if (OpenNetPort(portSet) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        return;
                    }

                }
            }
            else if (schemeConfigFrm.chkRS485.Checked == true)
            {
                GetSerialPort(ref serialPortSet); ;
                OpenSerialPort(serialPortSet);
            }
            else
            {
                return;
            }

            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return;
            }


            //测量点
            int pos = 0;
            byte memberNum = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //权限密码
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //数据项0140
            data[pos++] = 0x01;
            data[pos++] = 0x02;

            data[pos++] = memberNum;

            memberNum++;
            data[pos++] = Convert.ToByte(txtCtlDNo.Text.ToString(), 10);
            data[pos++] = Convert.ToByte(txtSubno.Text.ToString(), 10);
            data[pos++] = (byte)(cmbFunc.SelectedIndex); //特殊功能索引 

            UInt32 dataArea = Convert.ToUInt32(txtParam.Text.ToString(), 10);
            data[pos++] = (byte)(dataArea & 0xff);
            data[pos++] = (byte)((dataArea >> 8) & 0xff);
            data[pos++] = (byte)((dataArea >> 16) & 0xff);
            data[pos++] = (byte)((dataArea >> 24) & 0xff);

            data[8] = memberNum;
            if (SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, pos, ref frame, 15, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                if (rcdata[2] == 0x01 && rcdata[3] == 0x02 && rcdata[4] == 0x00)
                {
                    MessageBox.Show("操作成功");
                }
                else
                {
                    MessageBox.Show("操作失败");
                }
            }
            else
            {
                MessageBox.Show("操作失败");
            }
            ClosePort();

        }

        private void MainFrame_Load(object sender, EventArgs e)
        {
            comboBox_FrameFormat.SelectedIndex = 0;
            comboBox_FrameType.SelectedIndex = 1;
            textBox_ID.Text = "0051B101";
            textBox_Data.Text = "00 01 02 03 04 05 06 07 ";

        }

        private void DSScenebtn_Click(object sender, EventArgs e)
        {
            doorSceneSet.Visible = false;
            doorSceneSet.Focus();
            doorSceneSet.ShowDialog();
        }

        private void btnDoorSet_Click(object sender, EventArgs e)
        {
            Frame_Stu frame = new Frame_Stu();

            byte[] data = new byte[400];
            byte[] addr = new byte[4];
            byte[] deviceAddr = new byte[6];
            IPPORT_SET portSet = new IPPORT_SET();
            PORT_SET serialPortSet = new PORT_SET();

            if (schemeConfigFrm.chkNet.Checked == true)
            {
                GetNetPort(ref portSet);
                if (bNetOpenFlag == false)
                {
                    if (OpenNetPort(portSet) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        return;
                    }

                }
            }
            else if (schemeConfigFrm.chkRS485.Checked == true)
            {
                GetSerialPort(ref serialPortSet); ;
                OpenSerialPort(serialPortSet);
            }
            else
            {
                return;
            }

            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return;
            }


            //测量点
            int pos = 0;
            byte memberNum = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //权限密码
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //数据项0144
            data[pos++] = 0x44;
            data[pos++] = 0x01;

            data[pos++] = memberNum;

            if (doorSceneSet.DSChk1.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorSceneSet.DSCtlDNo1.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorSceneSet.DSSNO1.Text.ToString(), 10);
                data[pos++] = (byte)(doorSceneSet.DSK1cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorSceneSet.DSParam1.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorSceneSet.DSChk2.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorSceneSet.DSCtlDNo2.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorSceneSet.DSSNO2.Text.ToString(), 10);
                data[pos++] = (byte)(doorSceneSet.DSK2cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorSceneSet.DSParam2.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (doorSceneSet.DSChk3.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorSceneSet.DSCtlDNo3.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorSceneSet.DSSNO3.Text.ToString(), 10);
                data[pos++] = (byte)(doorSceneSet.DSK3cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorSceneSet.DSParam3.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (doorSceneSet.DSChk4.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorSceneSet.DSCtlDNo4.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorSceneSet.DSSNO4.Text.ToString(), 10);
                data[pos++] = (byte)(doorSceneSet.DSK4cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(doorSceneSet.DSParam4.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            data[8] = memberNum;
            if (SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, pos, ref frame, 15, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                if (rcdata[2] == 0x44 && rcdata[3] == 0x01 && rcdata[4] == 0x00)
                {
                    MessageBox.Show("操作成功");
                }
                else
                {
                    MessageBox.Show("操作失败");
                }
            }
            else
            {
                MessageBox.Show("操作失败");
            }
            ClosePort();
        }

        private void btnIrdtEdit_Click(object sender, EventArgs e)
        {
            irdtSceneSet.Visible = false;
            irdtSceneSet.Focus();
            irdtSceneSet.ShowDialog();
        }

        private void btnIrdtSet_Click(object sender, EventArgs e)
        {
            Frame_Stu frame = new Frame_Stu();

            byte[] data = new byte[400];
            byte[] addr = new byte[4];
            byte[] deviceAddr = new byte[6];
            IPPORT_SET portSet = new IPPORT_SET();
            PORT_SET serialPortSet = new PORT_SET();

            if (schemeConfigFrm.chkNet.Checked == true)
            {
                GetNetPort(ref portSet);
                if (bNetOpenFlag == false)
                {
                    if (OpenNetPort(portSet) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        return;
                    }

                }
            }
            else if (schemeConfigFrm.chkRS485.Checked == true)
            {
                GetSerialPort(ref serialPortSet); ;
                OpenSerialPort(serialPortSet);
            }
            else
            {
                return;
            }

            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return;
            }


            //测量点
            int pos = 0;
            byte memberNum = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //权限密码
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //数据项0145
            data[pos++] = 0x45;
            data[pos++] = 0x01;

            data[pos++] = memberNum;

            if (irdtSceneSet.IrdtInChk1.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInCtlDNo1.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInSNO1.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtInK1cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtInParam1.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (irdtSceneSet.IrdtInChk2.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInCtlDNo2.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInSNO2.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtInK2cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtInParam2.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (irdtSceneSet.IrdtInChk3.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInCtlDNo3.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInSNO3.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtInK3cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtInParam3.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (irdtSceneSet.IrdtInChk4.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInCtlDNo4.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInSNO4.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtInK4cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtInParam4.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (irdtSceneSet.IrdtInChk5.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInCtlDNo5.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInSNO5.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtInK5cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtInParam5.Text.ToString(), 10);
                data[pos++] = (byte)((byte)(dataArea & 0xff));
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (irdtSceneSet.IrdtInChk6.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInCtlDNo6.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInSNO6.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtInK6cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtInParam6.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (irdtSceneSet.IrdtInChk7.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInCtlDNo7.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInSNO7.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtInK7cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtInParam7.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (irdtSceneSet.IrdtInChk8.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInCtlDNo8.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInSNO8.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtInK8cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtInParam8.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (irdtSceneSet.IrdtInChk9.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInCtlDNo9.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInSNO9.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtInK9cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtInParam9.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (irdtSceneSet.IrdtInChk10.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInCtlDNo10.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInSNO10.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtInK10cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtInParam10.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            data[8] = memberNum;

            //数据项0146
            data[pos++] = 0x46;
            data[pos++] = 0x01;
            int tmppos = pos;
            memberNum = 0;
            data[pos++] = memberNum;

            if (irdtSceneSet.IrdtOutChk1.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutCtlDNo1.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutSNO1.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK1cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtOutParam1.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (irdtSceneSet.IrdtOutChk2.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutCtlDNo2.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutSNO2.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK2cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtOutParam2.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            if (irdtSceneSet.IrdtOutChk3.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutCtlDNo3.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutSNO3.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK3cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtOutParam3.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (irdtSceneSet.IrdtOutChk4.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutCtlDNo4.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutSNO4.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK4cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtOutParam4.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (irdtSceneSet.IrdtOutChk5.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutCtlDNo5.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutSNO5.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK5cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtOutParam5.Text.ToString(), 10);
                data[pos++] = (byte)((byte)(dataArea & 0xff));
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (irdtSceneSet.IrdtOutChk6.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutCtlDNo6.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutSNO6.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK6cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtOutParam6.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (irdtSceneSet.IrdtOutChk7.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutCtlDNo7.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutSNO7.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK7cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtOutParam7.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (irdtSceneSet.IrdtOutChk8.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutCtlDNo8.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutSNO8.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK8cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtOutParam8.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (irdtSceneSet.IrdtOutChk9.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutCtlDNo9.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutSNO9.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK9cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtOutParam9.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            if (irdtSceneSet.IrdtOutChk10.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutCtlDNo10.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtOutSNO10.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK10cmbFunc.SelectedIndex); //特殊功能索引 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtOutParam10.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }
            data[tmppos] = memberNum;

            if (SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, pos, ref frame, 15, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                if (rcdata[2] == 0x45 && rcdata[3] == 0x01 && rcdata[4] == 0x00 && rcdata[5] == 0x46 && rcdata[6] == 0x01 && rcdata[7] == 0x00)
                {
                    MessageBox.Show("操作成功");
                }
                else
                {
                    MessageBox.Show("操作失败");
                }
            }
            else
            {
                MessageBox.Show("操作失败");
            }
            ClosePort();
        }

        byte lvLinePos;
        void PraseRcuState(ref byte[] rcdata)
        {
            /*
                第一字节：
                bit0紧急按钮状态：0，未按下，1，已按下
                Bit1房间门卡插入状态：0，未插卡，1已插卡
                Bit2 房间大门磁检测：0，大门关，1大门开
                Bit3 总电源开关状态：0：关，1：开。
                Bit4 清理房间按钮状态0：关，1：开。
                Bit5 洗衣服务按钮状态0：关，1：开。
                Bit6 请勿打扰状态0：关，1：开。
                Bit7 请稍后状态0：关，1：开。
            */
            /* lvRcuStateInfo控件初始化*/
            { ListViewItem item = new ListViewItem("紧急按钮状态"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x01) == (byte)0x01)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "已按下";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "未按下";
            }

            { ListViewItem item = new ListViewItem("房间门卡插入状态"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x02) == (byte)0x02)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "已插卡";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "未插卡";
            }

            { ListViewItem item = new ListViewItem("房间大门磁检测"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x04) == (byte)0x04)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "开";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "关";
            }

            { ListViewItem item = new ListViewItem("总电源开关状态"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x08) == (byte)0x08)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "开";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "关";
            }

            { ListViewItem item = new ListViewItem("清理房间按钮状态"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x10) == (byte)0x10)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "开";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "关";
            }

            { ListViewItem item = new ListViewItem("洗衣服务按钮状态"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x20) == (byte)0x20)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "开";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "关";
            }

            { ListViewItem item = new ListViewItem("请勿打扰状态"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x40) == (byte)0x40)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "开";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "关";
            }

            { ListViewItem item = new ListViewItem("请稍后状态"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x80) == (byte)0x80)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "开";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "关";
            }
            /*
             * 第二字节：
            bit0红外检测：0，无人，1，有人
            Bit1请求服务：0，无请求，1请求服务
            Bit2 阳台门磁：0，阳台门关，1阳台门开
            Bit3 夜床：0：关，1：开。
            Bit4 保险箱0：关，1：开。
            Bit5 请求退房0：关，1：开。
            Bit6 结账 0：关，1：开。
            Bit7 维修中0：关，1：开。
            */
            { ListViewItem item = new ListViewItem("红外检测"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x01) == (byte)0x01)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "有人";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "无人";
            }

            { ListViewItem item = new ListViewItem("请求服务"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x02) == (byte)0x02)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "请求服务";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "无请求";
            }

            { ListViewItem item = new ListViewItem("阳台门磁"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x04) == (byte)0x04)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "阳台门开";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "阳台门关";
            }

            { ListViewItem item = new ListViewItem("夜床"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x08) == (byte)0x08)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "开";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "关";
            }

            { ListViewItem item = new ListViewItem("保险箱"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x10) == (byte)0x10)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "开";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "关";
            }

            { ListViewItem item = new ListViewItem("请求退房"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x20) == (byte)0x20)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "开";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "关";
            }

            { ListViewItem item = new ListViewItem("结账"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x40) == (byte)0x40)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "开";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "关";
            }

            { ListViewItem item = new ListViewItem("维修中"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x80) == (byte)0x80)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "开";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "关";
            }
        }

        void PraseAirconditionState(ref byte[] rcdata)
        {
            { ListViewItem item = new ListViewItem("空调季节模式"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0]) == (byte)0x01)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "冬季";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "夏季";
            }

            { ListViewItem item = new ListViewItem("空调工作模式"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1]) == (byte)0x01)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "制热";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "制冷";
            }

            { ListViewItem item = new ListViewItem("空调设置温度"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = String.Format("{0:D2}", (byte)rcdata[2]) + " 度";

            { ListViewItem item = new ListViewItem("房间温度"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = String.Format("{0:D2}", (byte)rcdata[3]) + " 度";

            { ListViewItem item = new ListViewItem("空调风速"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = String.Format("{0:D2}", (byte)rcdata[4]) + " 档";

        }
        void PraseRelay8State(ref byte[] rcdata)
        {
            BitArray bitarray = new BitArray(rcdata);

            for (int i = 13; i <= 20; i++)
            {
                { ListViewItem item = new ListViewItem("第" + i.ToString() + "路继电器"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
                if (bitarray[i] == false)
                {
                    rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "关";
                }
                else
                {
                    rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "开";
                }
            }
        }

        void PraseLight4State(ref byte[] rcdata)
        {

        }

        void PraseLed3State(ref byte[] rcdata)
        {

        }

        void PraseRelay2State(ref byte[] rcdata)
        {
            BitArray bitarray = new BitArray(rcdata);

            for (int i = 5; i <= 6; i++)
            {
                { ListViewItem item = new ListViewItem("第" + i.ToString() + "路继电器"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
                if (bitarray[i] == false)
                {
                    rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "关";
                }
                else
                {
                    rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "开";
                }
            }
        }






        private void btnReadRcuState()
        {
            Frame_Stu frame = new Frame_Stu();

            byte[] data = new byte[400];
            byte[] addr = new byte[4];
            byte[] deviceAddr = new byte[6];
            IPPORT_SET portSet = new IPPORT_SET();
            PORT_SET serialPortSet = new PORT_SET();

            if (schemeConfigFrm.chkNet.Checked == true)
            {
                GetNetPort(ref portSet);
                if (bNetOpenFlag == false)
                {
                    if (OpenNetPort(portSet) == false)
                    {
                        MessageBox.Show("网络连接失败！退出测试");
                        return;
                    }

                }
            }
            else if (schemeConfigFrm.chkRS485.Checked == true)
            {
                GetSerialPort(ref serialPortSet); ;
                OpenSerialPort(serialPortSet);
            }
            else
            {
                return;
            }

            if (GetGDAddress(ref addr, txtAddr.Text) == false)
            {
                return;
            }

            /* 初始化列表*/
            lvLinePos = 0;
            rcuStateQuq.lvRcuStateInfo.Items.Clear();

            //测量点
            int pos = 0;
            data[pos++] = 0x01;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;

            //数据项0202
            data[pos++] = 0x02;
            data[pos++] = 0x02;

            if (SendFrame(addr, (byte)GD_AFN_CODE.GetRealtimeParam, data, pos, ref frame, 15, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                if (frame.dataArray.Count > 9 && rcdata[8] == 0x02 && rcdata[9] == 0x02)
                {
                    byte deviceNum;
                    deviceNum = rcdata[10];//设备数
                    byte[] tempdata = new byte[8];

                    /* 字节长度计算 */

                    /* 模块数据解析*/
                    for (int i = 0; i < deviceNum; i++)
                    {
                        switch (rcdata[12 + i * 10])
                        {
                            case (byte)DEVICE_TYPE.MD_RCU:
                                { ListViewItem item = new ListViewItem("<RCU主控模块状态>" + "设备序号:" + String.Format("{0:D2}", (byte)rcdata[11 + i * 10])); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
                                lvLinePos++;
                                System.Array.Copy(rcdata, 12 + i * 10 + 1, tempdata, 0, 8);
                                PraseRcuState(ref tempdata);
                                break;

                            case (byte)DEVICE_TYPE.MD_AIRCONDITION:
                                { ListViewItem item = new ListViewItem("<空调模块状态>" + "设备序号:" + String.Format("{0:D2}", (byte)rcdata[11 + i * 10])); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
                                lvLinePos++;
                                System.Array.Copy(rcdata, 12 + i * 10 + 1, tempdata, 0, 8);
                                PraseAirconditionState(ref tempdata);
                                break;

                            case (byte)DEVICE_TYPE.MD_RELAY_8:
                                { ListViewItem item = new ListViewItem("<8路继电器模块状态>" + "设备序号:" + String.Format("{0:D2}", (byte)rcdata[11 + i * 10])); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
                                lvLinePos++;
                                System.Array.Copy(rcdata, 12 + i * 10 + 1, tempdata, 0, 8);
                                PraseRelay8State(ref tempdata);
                                break;

                            case (byte)DEVICE_TYPE.MD_LIGHT_4:
                                { ListViewItem item = new ListViewItem("<4路调光模块状态>" + "设备序号:" + String.Format("{0:D2}", (byte)rcdata[11 + i * 10])); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
                                lvLinePos++;
                                System.Array.Copy(rcdata, 12 + i * 10 + 1, tempdata, 0, 8);
                                PraseLight4State(ref tempdata);
                                break;

                            case (byte)DEVICE_TYPE.MD_RELAY_2:
                                { ListViewItem item = new ListViewItem("<2路继电器模块状态>" + "设备序号:" + String.Format("{0:D2}", (byte)rcdata[11 + i * 10])); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
                                lvLinePos++;
                                System.Array.Copy(rcdata, 12 + i * 10 + 1, tempdata, 0, 8);
                                PraseRelay2State(ref tempdata);
                                break;

                            case (byte)DEVICE_TYPE.MD_LEDV12_3:
                                { ListViewItem item = new ListViewItem("<12VLED调光模块状态>" + "设备序号:" + String.Format("{0:D2}", (byte)rcdata[11 + i * 10])); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
                                lvLinePos++;
                                System.Array.Copy(rcdata, 12 + i * 10 + 1, tempdata, 0, 8);
                                PraseLed3State(ref tempdata);
                                break;

                            default:
                                break;
                        }
                    }
                }

            }
            ClosePort();
        }

        private void btnRcuState_Click(object sender, EventArgs e)
        {
            btnReadRcuState();
            rcuStateQuq.Visible = false;
            rcuStateQuq.Focus();
            rcuStateQuq.ShowDialog();
        }

        private void btnDevAddrSet_Click(object sender, EventArgs e)
        {
            PORT_SET serialPortSet = new PORT_SET();    
            GetSerialPort(ref serialPortSet); ;
            if (OpenSerialPort(serialPortSet) == false)
            {
                return ;
            }

            byte[] deviceAddr = new byte[6];
            byte[] data = new byte[22];
            int pos=0;
            data[pos++] = 0x7f;
            data[pos++] = 0x7e;
            data[pos++] = 0x68;
            data[pos++] = 0xFF;
            data[pos++] = 0xFF;
            data[pos++] = 0xFF;
            data[pos++] = 0xFF;
            data[pos++] = 0xFF;
            data[pos++] = 0xFF;
            data[pos++] = 0x68;
            data[pos++] = 0x04;
            data[pos++] = 0x08;
            data[pos++] = 0x33;
            data[pos++] = 0x44;

            GetGdMeterAddr(txtDevAddr.Text.ToString(), ref deviceAddr);
            for (int i = 0; i < 6; i++)
            {
                data[pos++] = deviceAddr[i];
            }

            byte cs=0;
            for (int i=2; i<pos; i++)
            {
                cs += data[i];
            }
            data[pos++] = cs;
            data[pos++] = 0x16;

            sendDatas(data, pos);
            
            Frame_Stu frame = new Frame_Stu();
            if (GetFrame(ref frame, 1) == true && frame.ctrl == 0x84)
            {
                MessageBox.Show("设置成功");
                return;
            }
            else
            {
                MessageBox.Show("设置失败");
                return;
            }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click_2(object sender, EventArgs e)
        {
            int datalen=0;
            byte[] data = new byte[400];
            
            double dfSrcBatVol;
            double dfActBatVol;
            double dfRatioBatVol;
            
            double dfSrcDcVol;
            double dfActDcVol;
            double dfRatioDcVol;

            double dfSrcPositiveR;
            double dfActPositiveR;
            double dfRatioPositiveR;

            double dfSrcNegativeR;
            double dfActNegativeR;
            double dfRatioNegativeR;

            double dfSrcCC1Vol;
            double dfActCC1Vol;
            double dfRatioCC1Vol;

            double dfSrcCC2Vol;
            double dfActCC2Vol;
            double dfRatioCC2Vol;

            double dfSrcTemperature;
            double dfActTemperature;
            double dfRatioTemperature;

            double dfSrcTemperature2;
            double dfActTemperature2;
            double dfRatioTemperature2;


            if (this.chkBatVol.Checked == true)
            {
                if (this.txtSrcBatVol.Text == "")
                {
                    MessageBox.Show("电池电压参考源值不能为空", "提示");
                    return;
                }
                dfSrcBatVol = Convert.ToDouble(this.txtSrcBatVol.Text.ToString());
                if (dfSrcBatVol < 10.0 || dfSrcBatVol > 800.0)
                {
                    MessageBox.Show("电池电压参考源值超过有效范围10.0~800.0伏", "提示");
                    return;
                }

                dfActBatVol = Convert.ToDouble(this.txtActBatVol.Text.ToString());
                dfRatioBatVol = dfSrcBatVol / dfActBatVol * 1.0000;
                
                this.txtRatioBatVol.Text = String.Format("{0:f3}", dfRatioBatVol);
                             
            }

            if (this.chkDcVol.Checked == true)
            {
                if (this.txtSrcDcVol.Text == "")
                {
                    MessageBox.Show("母线电压参考源值不能为空", "提示");
                    return;
                }
                dfSrcDcVol = Convert.ToDouble(this.txtSrcDcVol.Text.ToString());
                if (dfSrcDcVol < 10.0 || dfSrcDcVol > 800.0)
                {
                    MessageBox.Show("母线电压参考源值超过有效范围10.0~800.0伏", "提示");
                    return;
                }
                dfActDcVol = Convert.ToDouble(this.txtActDcVol.Text.ToString());
                dfRatioDcVol = dfSrcDcVol / dfActDcVol;
                this.txtRatioDcVol.Text = String.Format("{0:f3}", dfRatioDcVol);
                         
            }

            if (this.chkPositiveR.Checked == true)
            {
                if (this.txtSrcPositiveR.Text == "")
                {
                    MessageBox.Show("正母线绝缘电阻参考源值不能为空", "提示");
                    return;
                }
                dfSrcPositiveR = Convert.ToDouble(this.txtSrcPositiveR.Text.ToString());
                if (dfSrcPositiveR < 0.0 || dfSrcPositiveR >999.0)
                {
                    MessageBox.Show("正母线绝缘电阻参考源值超过有效范围0~999千欧", "提示");
                    return;
                }
                dfActPositiveR = Convert.ToDouble(this.txtActPositiveR.Text.ToString());
                dfRatioPositiveR = dfSrcPositiveR / dfActPositiveR;
                this.txtRatioPositiveR.Text = String.Format("{0:f3}", dfRatioPositiveR); 
                         
            }

            if (this.chkNegativeR.Checked == true)
            {
                if (this.txtSrcNegativeR.Text == "")
                {
                    MessageBox.Show("负母线绝缘电阻参考源值不能为空", "提示");
                    return;
                }
                dfSrcNegativeR = Convert.ToDouble(this.txtSrcNegativeR.Text.ToString());
                if (dfSrcNegativeR < 0.0 || dfSrcNegativeR > 999.0)
                {
                    MessageBox.Show("负母线绝缘电阻参考源值超过有效范围0~999千欧", "提示");
                    return;
                }
                dfActNegativeR = Convert.ToDouble(this.txtActNegativeR.Text.ToString());
                dfRatioNegativeR = dfSrcNegativeR / dfActNegativeR;
                this.txtRatioNegativeR.Text = String.Format("{0:f3}", dfRatioNegativeR); 
                           
            }

            if (this.chkCC1.Checked == true)
            {
                if (this.txtSrcCC1Vol.Text == "")
                {
                    MessageBox.Show("CC1连接确认电压参考源值不能为空", "提示");
                    return;
                }
                dfSrcCC1Vol = Convert.ToDouble(this.txtSrcCC1Vol.Text.ToString());
                if (dfSrcCC1Vol < 0.0 || dfSrcCC1Vol > 13.0)
                {
                    MessageBox.Show("CC1连接确认电压参考源值超过有效范围0~13.0伏", "提示");
                    return;
                }
                dfActCC1Vol = Convert.ToDouble(this.txtActCC1Vol.Text.ToString());
                dfRatioCC1Vol = dfSrcCC1Vol / dfActCC1Vol;
                this.txtRatioCC1Vol.Text = String.Format("{0:f3}", dfRatioCC1Vol); 
                        
            }

             if (this.chkTemperature.Checked == true)
            {
                if (this.txtSrcTemperature.Text == "")
                {
                    MessageBox.Show("温度参考源值不能为空", "提示");
                    return;
                }
                dfSrcTemperature = Convert.ToDouble(this.txtSrcTemperature.Text.ToString());
                if (dfSrcTemperature < -50.0 || dfSrcTemperature > 200.0)
                {
                    MessageBox.Show("温度参考源值超过有效范围-50~200.0伏", "提示");
                    return;
                }
                dfActTemperature = Convert.ToDouble(this.txtActTemperature.Text.ToString());
                dfRatioTemperature = dfSrcTemperature / dfActTemperature;
                this.txtRatioTemperature.Text = String.Format("{0:f1}", dfRatioTemperature); 
                           
            }

            if (this.chkTemperature2.Checked == true)
            {
                if (this.txtSrcTemperature2.Text == "")
                {
                    MessageBox.Show("温度2参考源值不能为空", "提示");
                    return;
                }
                dfSrcTemperature2 = Convert.ToDouble(this.txtSrcTemperature2.Text.ToString());
                if (dfSrcTemperature2 < -50.0 || dfSrcTemperature2 > 200.0)
                {
                    MessageBox.Show("温度2参考源值超过有效范围0~100.0伏", "提示");
                    return;
                }
                dfActTemperature2 = Convert.ToDouble(this.txtActTemperature2.Text.ToString());
                dfRatioTemperature2 = dfSrcTemperature2 / dfActTemperature2;
                this.txtRatioTemperature2.Text = String.Format("{0:f1}", dfRatioTemperature2);

            }


        }

        private void btnCheckSet_Click(object sender, EventArgs e)
        {
            if (OpenCalibrationDetect() == false)
            {
                return;
            }
            PORT_SET serialPortSet = new PORT_SET();    
            GetSerialPort(ref serialPortSet); ;
            if (OpenSerialPort(serialPortSet) == false)
            {
                return ;
            }
            //txtRatioBatVol.Text = flBatRate.ToString();
            //txtRatioDcVol.Text = flMastRate.ToString();
            //txtRatioPositiveR.Text = flToGndRfRate.ToString();
            //txtRatioNegativeR.Text = flNegRfRate.ToString();
            //txtRatioCC1Vol.Text = flCC1Rate.ToString();
            //txtRatioCC2Vol.Text = flCC2Rate.ToString();
            //txtRatioTemperature.Text = flTemperatureRate.ToString();

            string errStr = "";
            if (txtRatioBatVol.Text == "")
            {
                errStr += "电池电压系数不能为空";
            }

            if (txtRatioDcVol.Text == "")
            {
               errStr += "电池电压系数不能为空";
            }
            if (txtRatioPositiveR.Text == "")
            {
                errStr += "正对地绝缘电阻系数不能为空";
            }
            if (txtRatioNegativeR.Text == "")
            {
                errStr += "负对地绝缘电阻系数不能为空";
            }
            if (txtRatioCC1Vol.Text == "")
            {
                errStr += "CC1电压系数不能为空";
            }
        
            if (txtRatioTemperature.Text == "")
            {
                errStr += "温度1系数不能为空";
            }
            if (txtRatioTemperature2.Text == "")
            {
                errStr += "温度2系数不能为空";
            }
            if (errStr != "")
            {
                MessageBox.Show(errStr);
                return;
            }

            /**********************************************************************/
            int datalen = 0;
            Int32 val;
            byte[] data = new byte[50];

            data[datalen++] = 0x7F;
            data[datalen++] = 0x7E;
            data[datalen++] = 0x68;
            data[datalen++] = 0xA1;
            data[datalen++] = 0x68;
            data[datalen++] = 0x04;
            data[datalen++] = 0x10;
            data[datalen++] = 0x20;
            data[datalen++] = 0x03;
    
            //if (this.chkBatVol.Checked == true)
            {
                double dfRatioBatVol;
                dfRatioBatVol = Convert.ToDouble(this.txtRatioBatVol.Text.ToString());
                val = Convert.ToInt16(dfRatioBatVol * 1000);
                data[datalen++] = (byte)(val & 0xff);
                data[datalen++] = (byte)((val >> 8) & 0xff);
            }

            //if (this.chkDcVol.Checked == true)
            {
                double dfRatioDcVol;
                dfRatioDcVol = Convert.ToDouble(this.txtRatioDcVol.Text.ToString());
                val = Convert.ToInt16(dfRatioDcVol * 1000);
                data[datalen++] = (byte)(val & 0xff);
                data[datalen++] = (byte)((val >> 8) & 0xff);
            }

            //if (this.chkPositiveR.Checked == true)
            {
                double dfRatioPositiveR;
                dfRatioPositiveR = Convert.ToDouble(this.txtRatioPositiveR.Text.ToString());
                val = Convert.ToInt16(dfRatioPositiveR * 1000);
                data[datalen++] = (byte)(val & 0xff);
                data[datalen++] = (byte)((val >> 8) & 0xff);
            }

           // if (this.chkNegativeR.Checked == true)
            {
                double dfRatioNegativeR;
                dfRatioNegativeR = Convert.ToDouble(this.txtRatioNegativeR.Text.ToString());
                val = Convert.ToInt16(dfRatioNegativeR * 1000);
                data[datalen++] = (byte)(val & 0xff);
                data[datalen++] = (byte)((val >> 8) & 0xff);
            }

            //if (this.chkCC1.Checked == true)
            {
                double dfRatioCC1Vol;
                dfRatioCC1Vol = Convert.ToDouble(this.txtRatioCC1Vol.Text.ToString());
                val = Convert.ToInt16(dfRatioCC1Vol * 1000);
                data[datalen++] = (byte)(val & 0xff);
                data[datalen++] = (byte)((val >> 8) & 0xff);
            }

           
           // if (this.chkTemperature.Checked == true)
            {
                double dfRatioTemperature;
                dfRatioTemperature = Convert.ToDouble(this.txtRatioTemperature.Text.ToString());
                val = Convert.ToInt16(dfRatioTemperature * 1000);
                data[datalen++] = (byte)(val & 0xff);
                data[datalen++] = (byte)((val >> 8) & 0xff);
            }

            {
                double dfRatioTemperature2;
                dfRatioTemperature2 = Convert.ToDouble(this.txtRatioTemperature2.Text.ToString());
                val = Convert.ToInt16(dfRatioTemperature2 * 1000);
                data[datalen++] = (byte)(val & 0xff);
                data[datalen++] = (byte)((val >> 8) & 0xff);
            }

            byte cs = 0;
            for (int i = 2; i < datalen; i++)
            {
                cs += data[i];
            }
            data[datalen++] = cs;
            data[datalen++] = 0x16;

            sendDatas(data, datalen);
            
            Frame_Stu frame = new Frame_Stu();
            if (GetFrame(ref frame, 2) == true && frame.ctrl == 0x84)
            {
                MessageBox.Show("校准参数设置成功", "提示");
                CloseInsulateDetect();
                return;
            }
            else
            {
                MessageBox.Show("校准参数设置失败", "提示");
                CloseInsulateDetect();
                return;
            }

        }

        private bool OpenInsulateDetect()
        {
            PORT_SET serialPortSet = new PORT_SET();
            GetSerialPort(ref serialPortSet); ;
            if (OpenSerialPort(serialPortSet) == false)
            {
                return false;
            }

            byte[] data = new byte[32];
            int pos = 0;

            //打开绝缘检测
            data[pos++] = 0x7F;
            data[pos++] = 0x7E;
            data[pos++] = 0x68;
            data[pos++] = 0xA1;
            data[pos++] = 0x68;
            data[pos++] = 0x04;
            data[pos++] = 0x03;
            data[pos++] = 0x80;
            data[pos++] = 0x0A;
            data[pos++] = 0x01;
            data[pos++] = 0x03;
            data[pos++] = 0x16;
            sendDatas(data, pos);

            Frame_Stu frame = new Frame_Stu();
            if (GetFrame(ref frame, 1) != true || frame.ctrl != 0x84)
            {
                sendDatas(data, pos);
                if (GetFrame(ref frame, 1) != true || frame.ctrl != 0x84)
                {
                    MessageBox.Show("进入校准采样失败", "提示");
                    return false;
                }
            }
            return true;
        }
        private bool OpenCalibrationDetect()
        {
            PORT_SET serialPortSet = new PORT_SET();
            GetSerialPort(ref serialPortSet); ;
            if (OpenSerialPort(serialPortSet) == false)
            {
                return false;
            }

            byte[] data = new byte[32];
            int pos = 0;

            //打开绝缘检测
            data[pos++] = 0x7F;
            data[pos++] = 0x7E;
            data[pos++] = 0x68;
            data[pos++] = 0xA1;
            data[pos++] = 0x68;
            data[pos++] = 0x04;
            data[pos++] = 0x03;
            data[pos++] = 0x80;
            data[pos++] = 0x0A;
            data[pos++] = 0x02;
            data[pos++] = 0x04;
            data[pos++] = 0x16;
            sendDatas(data, pos);

            Frame_Stu frame = new Frame_Stu();
            if (GetFrame(ref frame, 1) != true || frame.ctrl != 0x84)
            {
                sendDatas(data, pos);
                if (GetFrame(ref frame, 1) != true || frame.ctrl != 0x84)
                {
                    MessageBox.Show("进入校准采样失败", "提示");
                    return false;
                }
            }
            return true;
        }
        private bool CloseInsulateDetect()
        {
            PORT_SET serialPortSet = new PORT_SET();
            GetSerialPort(ref serialPortSet); ;
            if (OpenSerialPort(serialPortSet) == false)
            {
                return false;
            }

            byte[] data = new byte[32];
            int pos = 0;

            //关闭绝缘检测
            data[pos++] = 0x7F;
            data[pos++] = 0x7E;
            data[pos++] = 0x68;
            data[pos++] = 0xA1;
            data[pos++] = 0x68;
            data[pos++] = 0x04;
            data[pos++] = 0x03;
            data[pos++] = 0x80;
            data[pos++] = 0x0A;
            data[pos++] = 0x00;
            data[pos++] = 0x02;
            data[pos++] = 0x16;
            sendDatas(data, pos);

            Frame_Stu frame = new Frame_Stu();
            if (GetFrame(ref frame, 1) != true || frame.ctrl != 0x84)
            {
                sendDatas(data, pos);
                if (GetFrame(ref frame, 1) != true || frame.ctrl != 0x84)
                {
                    MessageBox.Show("绝缘检测关闭失败", "提示");
                    return false;
                }
            }
            return true;
        }

        private bool ReadInsulateRfData()
        {
            PORT_SET serialPortSet = new PORT_SET();
            GetSerialPort(ref serialPortSet); ;
            if (OpenSerialPort(serialPortSet) == false)
            {
                return false;
            }

            byte[] data = new byte[32];
            Frame_Stu frame = new Frame_Stu();
            int pos = 0;
            data[pos++] = 0x7F;
            data[pos++] = 0x7E;
            data[pos++] = 0x68;
            data[pos++] = 0xA1;
            data[pos++] = 0x68;
            data[pos++] = 0x01;
            data[pos++] = 0x02;
            data[pos++] = 0xC0;
            data[pos++] = 0x02;
            data[pos++] = 0x36;
            data[pos++] = 0x16;

            sendDatas(data, pos);

            if (GetFrame(ref frame, 1) != true || frame.ctrl != 0x81)
            {
                sendDatas(data, pos);

                if (GetFrame(ref frame, 1) != true || frame.ctrl != 0x81)
                {
                    MessageBox.Show("读取绝缘电阻数据失败", "提示");
                    return false;
                }
            }
                Int16 id;
                Int16 dlval;
                int index = 0;

                //电池电压，放大系数10
                dlval = Convert.ToInt16((byte)(frame.dataArray[15]) | (byte)(frame.dataArray[16]) << 8); 
                double flBatVol = dlval / 10.0;

                ////柜体母线电压*10
                dlval = Convert.ToInt16((byte)(frame.dataArray[0]) | (byte)(frame.dataArray[1]) << 8); 
                double flMastVol = dlval / 10.0;

                //正对地电阻 千欧
                Int16 ToGndRf = Convert.ToInt16((byte)(frame.dataArray[17]) | (byte)(frame.dataArray[18]) << 8);

                //负对地电阻 千欧
                Int16 NegGndRf = Convert.ToInt16((byte)(frame.dataArray[19]) | (byte)(frame.dataArray[20]) << 8); 

                //CC1连接确认电压 *100
                dlval = Convert.ToInt16((byte)(frame.dataArray[13]) | (byte)(frame.dataArray[14]) << 8);
                double flCC1Vol = dlval / 100.0;
                //温度1 
                dlval = Convert.ToInt16((byte)(frame.dataArray[9]) | (byte)(frame.dataArray[10]) << 8);
                double flTemperatureVol = dlval - 50;
                //温度2  
                dlval = Convert.ToInt16((byte)(frame.dataArray[11]) | (byte)(frame.dataArray[12]) << 8);
                double flTemperatureVol2 = dlval -50;
           
                txtActBatVol.Text = flBatVol.ToString();
                txtActDcVol.Text = flMastVol.ToString();
                txtActPositiveR.Text = ToGndRf.ToString();
                txtActNegativeR.Text = NegGndRf.ToString();
                txtActCC1Vol.Text = flCC1Vol.ToString();
               // txtActCC2Vol.Text = flCC2Vol.ToString();
                txtSrcTemperature.Text = flTemperatureVol.ToString();
                txtSrcTemperature2.Text = flTemperatureVol2.ToString();
                MessageBox.Show("读取成功");
                return true;
            
        }
        private void button4_Click(object sender, EventArgs e)
        {
            txtActBatVol.Clear();
            txtActBatVol.Show();
            txtActBatVol.Clear();
            txtActDcVol.Text = " ";
            txtActPositiveR.Text = " ";
            txtActNegativeR.Text = " ";
            txtActCC1Vol.Text = " ";
            txtActTemperature.Text = " ";
            txtActTemperature2.Text = " ";

 
            if (OpenInsulateDetect() == false)
            {
   
                return;
            }
           
           
            System.Threading.Thread.Sleep(7000);
            if (ReadRealData() == false)
            {
 //               CloseInsulateDetect();

                return;
            }
            //btmPrt.Text = "绝缘检测数据读取完成，绝缘监测已关闭";
            // System.Threading.Thread.Sleep(100);
            //if (CloseInsulateDetect() == false)
            //    return;

            //System.Threading.Thread.Sleep(200);
            //ReadRealData();

        }
        private bool ReadRealData()
        {
             PORT_SET serialPortSet = new PORT_SET();    
            GetSerialPort(ref serialPortSet); ;
            if (OpenSerialPort(serialPortSet) == false)
            {
                return false;
            }

           

            byte[] data = new byte[16];
            Frame_Stu frame = new Frame_Stu();
            int pos = 0;
            data[pos++] = 0x7F;
            data[pos++] = 0x7E;
            data[pos++] = 0x68;
            data[pos++] = 0xA1;
            data[pos++] = 0x68;
            data[pos++] = 0x01;
            data[pos++] = 0x02;
            data[pos++] = 0xC0;
            data[pos++] = 0x02;
            data[pos++] = 0x36;
            data[pos++] = 0x16;

            sendDatas(data, pos);
            
            if (GetFrame(ref frame, 1) != true || frame.ctrl != 0x81)
            {
                sendDatas(data, pos);

                if (GetFrame(ref frame, 1) != true || frame.ctrl != 0x81)
                {
                    MessageBox.Show("读取实时数据失败", "提示");
                    return false;
                }
            }
                Int16 id;
                Int32 dlval;
                int index=0;

                //电池电压，放大系数10
                dlval = Convert.ToInt16((byte)(frame.dataArray[0]) | (byte)(frame.dataArray[1]) << 8);
                double flBatVol = dlval / 10.0;

                ////柜体母线电压*10
                dlval = Convert.ToInt16((byte)(frame.dataArray[15]) | (byte)(frame.dataArray[16]) << 8);
                double flMastVol = dlval / 10.0;

                //正对地电阻 千欧
                UInt16 ToGndRf = Convert.ToUInt16((byte)(frame.dataArray[17]) | (byte)(frame.dataArray[18]) << 8);
       
                //负对地电阻 千欧
                UInt16 NegGndRf = Convert.ToUInt16((byte)(frame.dataArray[19]) | (byte)(frame.dataArray[20]) << 8);

                //CC1连接确认电压 *100
                dlval = Convert.ToInt16((byte)(frame.dataArray[13]) | (byte)(frame.dataArray[14]) << 8);
                double flCC1Vol = dlval / 100.0;
                //温度1 
                dlval = Convert.ToInt16((byte)(frame.dataArray[11]));
                double flTemperatureVol = dlval - 50;
                //温度2  
                dlval = Convert.ToInt16((byte)(frame.dataArray[12]));
                double flTemperatureVol2 = dlval - 50;
      
                txtActBatVol.Text = flBatVol.ToString();
                txtActDcVol.Text = flMastVol.ToString();
                double flToGndRf, flNegGndRf;
                flToGndRf = ToGndRf * 1.0;
                flNegGndRf = NegGndRf * 1.0;
                txtActPositiveR.Text = flToGndRf.ToString();
                txtActNegativeR.Text = flNegGndRf.ToString();
                txtActCC1Vol.Text = flCC1Vol.ToString();
                txtActTemperature.Text = flTemperatureVol.ToString();
                txtActTemperature2.Text = flTemperatureVol2.ToString();
                MessageBox.Show("读取成功","提示");
                return true;
            
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            if (OpenCalibrationDetect() == false)
            {
                return;
            }

            PORT_SET serialPortSet = new PORT_SET();
            GetSerialPort(ref serialPortSet); ;
            if (OpenSerialPort(serialPortSet) == false)
            {
                return;
            }



            byte[] data = new byte[16];
            int pos = 0;

            data[pos++] = 0x7F;
            data[pos++] = 0x7E;
            data[pos++] = 0x68;
            data[pos++] = 0xA1;
            data[pos++] = 0x68;
            data[pos++] = 0x01;
            data[pos++] = 0x02;
            data[pos++] = 0x20;
            data[pos++] = 0x03;
            data[pos++] = 0x97;
            data[pos++] = 0x16;
            sendDatas(data, pos);

            Frame_Stu frame = new Frame_Stu();
            if (GetFrame(ref frame, 2) == true && frame.ctrl == 0x81)
            {
                Int16 id;
                int dlval;
                int index = 0;

                //电池电压，放大系数1000
                dlval = Convert.ToInt16((byte)(frame.dataArray[index]) | (byte)(frame.dataArray[index + 1]) << 8); index += 2;
                double flBatRate = dlval / 1000.0;

                ////柜体母线电压*1000
                dlval = Convert.ToInt16((byte)(frame.dataArray[index]) | (byte)(frame.dataArray[index + 1]) << 8); index += 2;
                double flMastRate = dlval / 1000.0;

                //正对地电阻 千欧
                dlval = Convert.ToInt16((byte)(frame.dataArray[index]) | (byte)(frame.dataArray[index + 1]) << 8); index += 2;
                double flToGndRfRate = dlval / 1000.0;
                //负对地电阻 千欧
                dlval = Convert.ToInt16((byte)(frame.dataArray[index]) | (byte)(frame.dataArray[index + 1]) << 8); index += 2;
                double flNegRfRate = dlval / 1000.0;
                //CC1连接确认电压 *1000
                dlval = Convert.ToInt16((byte)(frame.dataArray[index]) | (byte)(frame.dataArray[index + 1]) << 8); index += 2;
                double flCC1Rate = dlval / 1000.0;

                ////温度1 *1000 
                dlval = Convert.ToInt16((byte)(frame.dataArray[index]) | (byte)(frame.dataArray[index + 1]) << 8); index += 2;
                double flTemperatureRate = dlval / 1000.0;

                ////温度2 *1000 
                dlval = Convert.ToInt16((byte)(frame.dataArray[index]) | (byte)(frame.dataArray[index + 1]) << 8); index += 2;
                double flTemperatureRate2 = dlval / 1000.0;

                txtRatioBatVol.Text = flBatRate.ToString();
                txtRatioDcVol.Text = flMastRate.ToString();
                txtRatioPositiveR.Text = flToGndRfRate.ToString();
                txtRatioNegativeR.Text = flNegRfRate.ToString();
                txtRatioCC1Vol.Text = flCC1Rate.ToString();
                txtRatioTemperature.Text = flTemperatureRate.ToString();
                txtRatioTemperature2.Text = flTemperatureRate2.ToString();
                MessageBox.Show("读取成功", "提示");
                CloseInsulateDetect();
                return;
            }
            else
            {
                MessageBox.Show("读取失败", "提示");
                CloseInsulateDetect();
                return;
            }
        }

        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAll.Checked == true)
            {
                chkBatVol.Checked = true;
                chkDcVol.Checked = true;
                chkPositiveR.Checked = true;
                chkNegativeR.Checked = true;
                chkCC1.Checked = true;
                chkTemperature.Checked = true;
                chkTemperature2.Checked = true;
            }
        }

        private void chkDcVol_CheckedChanged(object sender, EventArgs e)
        {
            chkAll.Checked = false;
        }

        private void chkPositiveR_CheckedChanged(object sender, EventArgs e)
        {
            chkAll.Checked = false;
        }

        private void chkNegativeR_CheckedChanged(object sender, EventArgs e)
        {
            chkAll.Checked = false;
        }

        private void chkCC1_CheckedChanged(object sender, EventArgs e)
        {
            chkAll.Checked = false;
        }

        private void chkCC2_CheckedChanged(object sender, EventArgs e)
        {
            chkAll.Checked = false;
        }

        private void chkTemperature_CheckedChanged(object sender, EventArgs e)
        {
            chkAll.Checked = false;
        }

        private void chkBatVol_CheckedChanged(object sender, EventArgs e)
        {
            chkAll.Checked = false;
           

        }

        private void btnReadVer_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[32];
            int pos = 0;
            CanBaudSet(500);
            pos = 0;
            data[pos++] = 0x03;
            data[pos++] = 0x45;
            data[pos++] = 0xA0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;

            UInt16 crc = CRC16RTU(data, (short)pos);
            data[pos++] = (byte)(crc & 0xff);
            data[pos++] = (byte)((crc >> 8) & 0xff);
            int len = pos;


            sendCanDatas(data, pos);
            Frame_Stu frame = new Frame_Stu();
            //     System.Threading.Thread.Sleep(300);

            if (GetFrame(ref frame, 1) == true && frame.ctrl == 0xA0)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                txtVer.Text = System.Text.Encoding.ASCII.GetString(rcdata,4,16);
                MessageBox.Show("读取成功 Read success");
                //return;
            }
            else
            {
                MessageBox.Show("读取失败 Read failure");
                //return;
            }
         
            CanBaudSet(250);
            return;

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (chkBatVol.Checked == true)
                txtRatioBatVol.Text = "1.0";

            if (chkDcVol.Checked == true)
                txtRatioDcVol.Text = "1.0";

            if (chkPositiveR.Checked == true)
                txtRatioPositiveR.Text = "1.0";

            if (chkNegativeR.Checked == true)
                txtRatioNegativeR.Text = "1.0";

            if (chkCC1.Checked == true)
                txtRatioCC1Vol.Text = "1.0";

          
            if (chkTemperature.Checked == true)
                txtRatioTemperature.Text = "1.0";

            if (chkTemperature2.Checked == true)
                txtRatioTemperature2.Text = "1.0";
        }

        private void txtSrcBatVol_TextChanged(object sender, EventArgs e)
        {

        }

        private void chkTemperature2_CheckedChanged(object sender, EventArgs e)
        {
            chkAll.Checked = false;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            lab.Text = "测试中，请等待...";
            lab.Update();
            
            testlv.Items[0].SubItems[1].Text = "";
            testlv.Items[1].SubItems[1].Text = "";
            testlv.Items[2].SubItems[1].Text = "";
            testlv.Items[3].SubItems[1].Text = "";
            testlv.Items[4].SubItems[1].Text = "";
            testlv.Items[5].SubItems[1].Text = "";

            lvView2.Items[0].SubItems[1].Text = "";
            lvView2.Items[1].SubItems[1].Text = "";
            lvView2.Items[2].SubItems[1].Text = "";
            lvView2.Items[3].SubItems[1].Text = "";
            lvView2.Items[4].SubItems[1].Text = "";
            lvView2.Items[5].SubItems[1].Text = "";
            lvView2.Items[6].SubItems[1].Text = "";
            lvView2.Items[7].SubItems[1].Text = "";
            testlv.Update();
            lvView2.Update();


            PORT_SET serialPortSet = new PORT_SET();
            GetSerialPort(ref serialPortSet); ;
            if (OpenSerialPort(serialPortSet) == false)
            {
                return ;
            }

            byte[] data = new byte[32];
            int pos = 0;

            pos = 0;
            data[pos++] = 0x3A;
            data[pos++] = 0x16;
            data[pos++] = 0xEF;
            data[pos++] = 0x01;
            data[pos++] = 0x00;
            data[pos++] = 0x06;
            data[pos++] = 0x01;
            data[pos++] = 0x0D;
            data[pos++] = 0x0A;
            
            sendDatas(data, pos);
            Frame_Stu frame = new Frame_Stu();
            if (GetFrame(ref frame, 1) != true || frame.ctrl != 0xEF)
            {

                MessageBox.Show("退出自动测试失败", "提示");
                return ;

            }

            Int16 id;
            Int32 dlval;
            int index = 0;

            //电池温度
            dlval = Convert.ToInt16((byte)(frame.dataArray[0]) | (byte)(frame.dataArray[1]) << 8);
            double flTemp = (dlval-2731) / 10.0;

            //电池电压
            dlval = Convert.ToInt16((byte)(frame.dataArray[2]) | (byte)(frame.dataArray[3]) << 8);
            double flBatVol = dlval*10.0;

            //电流
            dlval = Convert.ToInt16((byte)(frame.dataArray[4]) | (byte)(frame.dataArray[5]) << 8);
            double flCur = dlval * 10.0;

            //SOC 
            dlval = Convert.ToByte(frame.dataArray[6]);
            double flSoc = dlval * 1.0;

            //剩余容量
            dlval = Convert.ToInt16((byte)(frame.dataArray[7]) | (byte)(frame.dataArray[8]) << 8);
            double flRemainCap = dlval * 1.0;

            //保护
            string protext;
            dlval = Convert.ToInt16((byte)(frame.dataArray[9]) | (byte)(frame.dataArray[10]) << 8);
            Int16 protect = (Int16)dlval;

            //最大放电电流
            dlval = Convert.ToInt16((byte)(frame.dataArray[11]) | (byte)(frame.dataArray[12]) << 8);
            double flMaxDsgCur = dlval * 10.0;

            //最大充电电流
            dlval = Convert.ToInt16((byte)(frame.dataArray[13]) | (byte)(frame.dataArray[14]) << 8);
            double flMaxChgCur = dlval * 10.0;

            //电池最高温度
            dlval = Convert.ToInt16((byte)(frame.dataArray[15]) | (byte)(frame.dataArray[16]) << 8);
            double flMaxTemp = (dlval - 2731) / 10.0;

            //电池最低温度
            dlval = Convert.ToInt16((byte)(frame.dataArray[17]) | (byte)(frame.dataArray[18]) << 8);
            double flMinTemp = (dlval - 2731) / 10.0;

            //电池最高电压
            dlval = Convert.ToInt16((byte)(frame.dataArray[19]) | (byte)(frame.dataArray[20]) << 8);
            double flBatMaxVol = dlval * 10.0;

            //电池最低电压
            dlval = Convert.ToInt16((byte)(frame.dataArray[21]) | (byte)(frame.dataArray[22]) << 8);
            double flBatMinVol = dlval * 10.0;

            //电池循环次数
            dlval = Convert.ToInt32((byte)(frame.dataArray[23]) | (byte)(frame.dataArray[24]) << 8);
            Int32 flBatCycle = dlval;

            //电池放电SOC累计值
            dlval = Convert.ToInt32((byte)(frame.dataArray[25]) | (byte)(frame.dataArray[26]) << 8| (byte)(frame.dataArray[27]) << 16| (byte)(frame.dataArray[28]) << 24);
            Int32 flBatDsgSocSum = dlval;

              

            testlv.Items[0].SubItems[1].Text = flTemp.ToString()+"度";
            testlv.Items[1].SubItems[1].Text = flBatVol.ToString()+"毫伏";
            testlv.Items[2].SubItems[1].Text = flCur.ToString()+"毫安";
            testlv.Items[3].SubItems[1].Text = flSoc.ToString()+"%";
            testlv.Items[4].SubItems[1].Text = flRemainCap.ToString()+"mAh";
            testlv.Items[5].SubItems[1].Text = protect.ToString();


            lvView2.Items[0].SubItems[1].Text = flMaxDsgCur.ToString() + "毫安";
            lvView2.Items[1].SubItems[1].Text = flMaxChgCur.ToString() + "毫安";
            lvView2.Items[2].SubItems[1].Text = flMaxTemp.ToString() + "度";
            lvView2.Items[3].SubItems[1].Text = flMinTemp.ToString() + "度";
            lvView2.Items[4].SubItems[1].Text = flBatMaxVol.ToString() + "毫伏";
            lvView2.Items[5].SubItems[1].Text = flBatMinVol.ToString() + "毫伏";
            lvView2.Items[6].SubItems[1].Text = flBatCycle.ToString();
            lvView2.Items[7].SubItems[1].Text = flBatDsgSocSum.ToString();

//            MessageBox.Show("自动测试结束", "提示");
            return ;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            PORT_SET serialPortSet = new PORT_SET();
            GetSerialPort(ref serialPortSet); ;
            if (OpenSerialPort(serialPortSet) == false)
            {
                return;
            }

            byte[] data = new byte[32];
            int pos = 0;

            pos = 0;
            data[pos++] = 0x3A;
            data[pos++] = 0x16;
            data[pos++] = 0xEA;
            data[pos++] = 0x01;
            data[pos++] = 0x00;
            data[pos++] = 0x01;
            data[pos++] = 0x01;
            data[pos++] = 0x0D;
            data[pos++] = 0x0A;

            sendDatas(data, pos);
            Frame_Stu frame = new Frame_Stu();
            System.Threading.Thread.Sleep(5000);
            if (GetFrame(ref frame, 1) != true || frame.ctrl != 0xEA)
            {

                MessageBox.Show("退出自动测试失败", "提示");
                return;

            }
            MessageBox.Show("eeprom数据清除成功", "提示");
        }

        public byte[] updateFileBuf = new byte[256 * 1024];//允许最大升级bin文件为 256 K
        public int updateFileLen;
        public UInt16 updateFileCrc;
        public UInt16 reqBlockNo;
        public short BlockLen;
        public short lastBlockLen;
        public short firmwareBlockNum;
/**************** 升级请求命令  ****************************/
        unsafe  bool RequestUpdate()
        {
            txtOutput.Text += "\r\nsend update request command：";
            WriteMessage("\r\nsend update request command：");
            //PORT_SET serialPortSet = new PORT_SET();
            //GetSerialPort(ref serialPortSet); ;
            //if (OpenSerialPort(serialPortSet) == false)
            //{
            //    return false;
            //}

            byte[] data = new byte[32];
            if (m_bOpen == 0)
                return false ;

          
            int pos = 0;

            pos = 0;
            data[pos++] = 0x03;
            data[pos++] = 0x45;
            data[pos++] = 0x11;
            data[pos++] = 0x00;
            data[pos++] = 0x06;
            //*
            try
            {
                data[pos++] = Convert.ToByte(Convert.ToInt16(HV_H.Text, 16)); //0x03;
                data[pos++] = Convert.ToByte(Convert.ToInt16(HV_M.Text, 16)); //0x02;
                data[pos++] = Convert.ToByte(Convert.ToInt16(HV_L.Text, 16)); //0x01;
                data[pos++] = Convert.ToByte(Convert.ToInt16(SV_H.Text, 16)); //0x00;
                data[pos++] = Convert.ToByte(Convert.ToInt16(SV_M.Text, 16)); //0x02;
                data[pos++] = Convert.ToByte(Convert.ToInt16(SV_L.Text, 16)); //0x01;
            }
            catch (Exception ex)
            {
                return false;
            }
            //*/
            UInt16 crc = CRC16RTU(data, (short)pos);
            data[pos++] = (byte)(crc & 0xff);
            data[pos++] = (byte)((crc >> 8) & 0xff);
            int len = pos ;


            sendCanDatas(data, pos);
            Frame_Stu frame = new Frame_Stu();
       //     System.Threading.Thread.Sleep(300);
            if (GetFrame(ref frame, 1) != true )
            {
                txtOutput.Text += "\r\nUpdate request fail：";
                WriteMessage("\r\nUpdate request fail：");
                return false;

            }
            if (frame.ctrl != 0x11)
            {
                txtOutput.Text += "\r\nUpdate request fail：";
                WriteMessage("\r\nUpdate request fail：");
            }
            else
            {
                txtOutput.Text += "\r\nUpdate request sucessfully：";
                WriteMessage("\r\nUpdate request sucessfully：");
            }


            return true;
        }
/**************** 升级固件信息命令  ****************************/
        private bool FirmwareInformation()
        {
            txtOutput.Text += "\r\nsend firmware information：";
            WriteMessage("\r\nsend firmware information：");
            if (m_bOpen == 0)
                return false;

            byte[] data = new byte[32];
            int pos = 0;

            pos = 0;
            data[pos++] = 0x03;
            data[pos++] = 0x45;
            data[pos++] = 0x12;//cmd
            data[pos++] = 0x00;//len
            data[pos++] = 0x16;//len
            /* 厂家信息 */
            data[pos++] = (byte)'T';
            data[pos++] = (byte)'W';
            data[pos++] = (byte)'S';
            data[pos++] = 0X0;
            data[pos++] = 0X0;
            data[pos++] = 0X0;
            data[pos++] = 0X0;
            data[pos++] = 0X0;
            data[pos++] = 0X0;
            data[pos++] = 0X0;
            /* 硬件版本号 */
            data[pos++] = 0X01;
            /* 客户编号 */
            data[pos++] = 0X0A;

            /* 固件类型 */
            data[pos++] = 0X04;
            /* 版本号 */
            try
            {
                data[pos++] = Convert.ToByte(Convert.ToInt16(SV_H.Text, 16)); //0x00;
                data[pos++] = Convert.ToByte(Convert.ToInt16(SV_M.Text, 16)); //0x02;
                data[pos++] = Convert.ToByte(Convert.ToInt16(SV_L.Text, 16)); //0x01;
            }
            catch (Exception ex)
            {
                return false;
            }  

            /* 固件长度 */
            data[pos++] = (byte)((updateFileLen>>24)&0xff);
            data[pos++] = (byte)((updateFileLen>>16)&0xff);
            data[pos++] = (byte)((updateFileLen>>8)&0xff);
            data[pos++] = (byte)((updateFileLen&0xff));
            /* CRC校验 */
            data[pos++] = (byte)((updateFileCrc >> 8) & 0xff);
            data[pos++] = (byte)(updateFileCrc & 0xff);
            UInt16 crc = CRC16RTU(data, (short)pos);
            data[pos++] = (byte)(crc & 0xff);
            data[pos++] = (byte)((crc >> 8) & 0xff);


            sendCanDatas(data, pos);
            Frame_Stu frame = new Frame_Stu();
            if (GetFrame(ref frame, 1) != true || frame.ctrl != 0x12)
            {

                txtOutput.Text += "\r\nfirmware information request fail";
                WriteMessage("\r\nfirmware information request fail");
                return false;

            }
            //回读确认升级完成跳转至APP ok
            if ((byte)frame.dataArray[0] == 0x02)
            {
                txtOutput.Text += "\r\n<<<<<<               firmware update completed                   >>>>>>>>>>>>>>>>>>>>>>>>";
                WriteMessage("\r\n<<<<<<               firmware update completed                   >>>>>>>>>>>>>>>>>>>>>>>>");
                txtOutput.Text += "《firmware version：" + (byte)frame.dataArray[4] + (byte)frame.dataArray[5] + (byte)frame.dataArray[6] + " 》";
                return true;
            }


            txtOutput.Text += "\r\nfirmware information request sucessfully";
            WriteMessage("\r\nfirmware information request sucessfully");
            switch ((byte)frame.dataArray[0])
            {
                case 0x00:
                    txtOutput.Text += "《no need update》";
                    WriteMessage("《no need update》");
                    break;

                case 0x01:
                    txtOutput.Text += "《block erasing》";
                    WriteMessage("《block erasing》");
                    break;
                
                case 0x02:
                    txtOutput.Text += "《update completed》";
                    WriteMessage("《update completed》");
                    break;

                case 0x03:
                    txtOutput.Text += "《update fault,app can't run 》";
                    WriteMessage("《update fault,app can't run 》");
                    break;

                case 0x04:
                    txtOutput.Text += "《block crc error》";
                    WriteMessage("《block crc error》");
                    break;

                case 0x05:
                    txtOutput.Text += "《block erase fail》";
                    WriteMessage("《block erase fail》");
                    break;
                
                case 0x06:
                    txtOutput.Text += "《block requesting》";
                    WriteMessage("《block requesting》");
                    break;
            }

            switch ((byte)frame.dataArray[1])
            {
                case 0x01:
                    txtOutput.Text += "《Block size 64 Bytes》";
                    WriteMessage("《Block size 64 Bytes》");
                    BlockLen = 64;
                    break;

                case 0x02:
                    txtOutput.Text += "《Block size 128 Bytes》";
                    WriteMessage("《Block size 128 Bytes》");
                    BlockLen = 128;
                    break;

                case 0x03:
                    txtOutput.Text += "《Block size 256 Bytes》";
                    WriteMessage("《Block size 256 Bytes》");
                    BlockLen = 240;
                    break;

                case 0x04:
                    txtOutput.Text += "《Block size 512 Bytes》";
                    WriteMessage("《Block size 512 Bytes》");
                    BlockLen = 512;
                    break;

                case 0x05:
                    txtOutput.Text += "《Block size 1024 Bytes》";
                    WriteMessage("《Block size 1024 Bytes》");
                    BlockLen = 1024;
                    break;
            }
            if (updateFileLen % BlockLen == 0)
            {
                firmwareBlockNum = (short)(updateFileLen / BlockLen);
                lastBlockLen = 0;
            }
            else
            {
                firmwareBlockNum = (short)(updateFileLen / BlockLen + 1);
                lastBlockLen = (short)(updateFileLen % BlockLen);
            }

            reqBlockNo = (UInt16)((byte)frame.dataArray[2] << 8 | (byte)frame.dataArray[3]);
            txtOutput.Text += "《total blocks num：" + firmwareBlockNum.ToString() + "》" + "《request block number：" + reqBlockNo.ToString() + "》";
            WriteMessage("《total blocks num：" + firmwareBlockNum.ToString() + "》" + "《request block number：" + reqBlockNo.ToString() + "》");
            return true;
        }


        private bool MakeBlockPackage(ref byte[] ver, ref byte[] blockno, ref byte[] FirmwareBlockNum, ref byte[] blockCrc, ref byte[] blockdata, short blockdatalen, ref byte[] package, ref short packagelen)
        {
            packagelen = 0;
            package[packagelen++] = 0x03;
            package[packagelen++] = 0x45;
            package[packagelen++] = 0x13;//cmd
            package[packagelen++] = (byte)(((blockdatalen + 10) >> 8) & 0xff);//len
            package[packagelen++] = (byte)((blockdatalen + 10) & 0xff);//len
            
            /* 版本号 */
            package[packagelen++] = ver[0];
            package[packagelen++] = ver[1];
            package[packagelen++] = ver[2];
            /* 固件类型 */
            package[packagelen++] = 0X04;
            /* 数据块编号 */
            package[packagelen++] = blockno[0];
            package[packagelen++] = blockno[1];
            /* 固件总数据块数 */
            package[packagelen++] = FirmwareBlockNum[0];
            package[packagelen++] = FirmwareBlockNum[1];
            /* 固件数据块校验 */
            package[packagelen++] = blockCrc[0];
            package[packagelen++] = blockCrc[1];

            for (int i = 0; i < blockdatalen; i++)
            {
                package[packagelen++] = blockdata[i];
            }
            UInt16 crc = CRC16RTU(package, (short)packagelen);
            package[packagelen++] = (byte)(crc & 0xff);
            package[packagelen++] = (byte)((crc >> 8) & 0xff);

            return true;
        }
/**************** 数据下载命令  ****************************/
        private bool BlockSending()
        {
            txtOutput.Text += "\r\nsend firmware block information：";
            WriteMessage("\r\nsend firmware block information：");
            if (m_bOpen == 0)
                return false;

            byte[] package = new byte[2048];
            short packagelen=0;
            byte[] ver = new byte[3];
            byte[] blockno = new byte[2];
            byte[] FirmwareBlockNum = new byte[2];
            byte[] blockCrc = new byte[2];
            UInt16 crc;
            byte[] blockdata = new byte[1024];


            /* wqs debug */
            /*reqBlockNo = 0;
            firmwareBlockNum = 50;
            BlockLen = 128;*/

            
            short blockdatalen = BlockLen;
            /* 准备固件版本号 */
            try
            {
                ver[0] = Convert.ToByte(Convert.ToInt16(SV_H.Text, 16)); //0x00;
                ver[1] = Convert.ToByte(Convert.ToInt16(SV_M.Text, 16)); //0x02;
                ver[2] = Convert.ToByte(Convert.ToInt16(SV_L.Text, 16)); //0x01;
            }
            catch (Exception ex)
            {
                return false;
            }         

            blockno[0] = (byte)(reqBlockNo>>8&0xff);
            blockno[1] = (byte)(reqBlockNo&0xff);

            FirmwareBlockNum[0] = (byte)(firmwareBlockNum>>8&0xff);
            FirmwareBlockNum[1] = (byte)(firmwareBlockNum&0xff);
            short idx;
            for (idx=0; idx< firmwareBlockNum; idx++)
            {
                blockno[0] = (byte)(reqBlockNo >> 8 & 0xff);
                blockno[1] = (byte)(reqBlockNo & 0xff);

                if ((lastBlockLen != 0) && (idx == (firmwareBlockNum-1)))
                {
                    for (int n=0; n<lastBlockLen; n++)
                    {
                        blockdata[n] = (byte)updateFileBuf[idx*BlockLen+n];
                        blockdatalen = lastBlockLen;
                    }

                    crc = CRC16RTU(blockdata, Convert.ToInt32(blockdatalen));
                   
                     blockCrc[0] = (byte)((crc>>8)&0xff);
                     blockCrc[1] = (byte)(crc);
                     MakeBlockPackage(ref ver, ref blockno, ref FirmwareBlockNum, ref blockCrc, ref blockdata, blockdatalen, ref package, ref packagelen);
                     sendCanDatas(package, packagelen);
                    Frame_Stu frame = new Frame_Stu();
                    System.Threading.Thread.Sleep(200);//debug 20200316 lin //200
                    if (GetFrame(ref frame, 2) != true || frame.ctrl != 0x13)
                    {
                        txtOutput.Text += "\r\nfirmware block receive fail";
                        WriteMessage("\r\nfirmware block receive fail");
                        return false;
                    }
                    else
                    {
                        txtOutput.Text += "\r\nfirmware block receive successfully";
                        WriteMessage("\r\nfirmware block receive successfully");
                        txtOutput.Text += "\r\n<<<<<<               firmware updated successfully                   >>>>>>>>>>>>>>>>>>>>>>>>";
                        WriteMessage("\r\n<<<<<<               firmware updated successfully                   >>>>>>>>>>>>>>>>>>>>>>>>");
                        return true;
                    }
                }
                else
                {
                    for (int n=0; n<BlockLen; n++)
                    {
                        blockdata[n] = (byte)updateFileBuf[idx*BlockLen+n];

                    }
                    crc = CRC16RTU(blockdata, (Int32)BlockLen);
                     blockCrc[0] = (byte)((crc>>8)&0xff);
                     blockCrc[1] = (byte)(crc);
                     MakeBlockPackage(ref ver, ref blockno, ref FirmwareBlockNum, ref blockCrc, ref blockdata, blockdatalen, ref package, ref packagelen);
                    // System.Threading.Thread.Sleep(1000);
                     sendCanDatas(package, packagelen);
                    Frame_Stu frame = new Frame_Stu();
                   
                    if (GetFrame(ref frame, 2) != true || frame.ctrl != 0x13)
                    {
                        txtOutput.Text += "\r\nfirmware block request fail：";
                        WriteMessage("\r\nfirmware block request fail：");
                        return false;
                    }

                    if ((byte)frame.dataArray[0] == 0x00)
                    {
                        reqBlockNo = (UInt16)(((byte)frame.dataArray[4] << 8) | (byte)frame.dataArray[5]);
                        txtOutput.Text += "\r\nfirmware block receive successfully：《total blocks num = "+ firmwareBlockNum.ToString()+"》"+"《request next BlockNo=" + reqBlockNo.ToString()+">>";
                        WriteMessage("\r\nfirmware block receive successfully：《total blocks num = " + firmwareBlockNum.ToString() + "》" + "《request next BlockNo=" + reqBlockNo.ToString() + ">>");
                    }
                    else
                    {
                        txtOutput.Text += "\r\nfirmware block receive fail ";
                        WriteMessage("\r\nfirmware block receive fail ");
                        return false;
                    }
                }
                //WriteMessage(this.txtOutput.Text);
                this.txtOutput.Focus();//获取焦点
                this.txtOutput.Select(this.txtOutput.TextLength, 0);//光标定位到文本最后
                this.txtOutput.ScrollToCaret();//滚动到光标处   
                this.txtOutput.Update();
              //  System.Threading.Thread.Sleep(300);
            }
            txtOutput.Text += "\r\nfirmware block receive successfully";
            WriteMessage("\r\nfirmware block receive successfully");
            if (idx == firmwareBlockNum)
            {
                txtOutput.Text += "\r\nfirmware updated successfully";
                WriteMessage("\r\nfirmware updated successfully");
            }
           
            return true;
        }

        
         public Thread threadUpdate;
         private bool updateflag = false;
  #region//进程
         private void OnUpdateFirmware()
         {
            if (RequestUpdate() == false)
             {
                 updateflag = false;
                 return;
             }

            System.Threading.Thread.Sleep(5 * 1000);//unit：1s

             if (FirmwareInformation() == false)
             {
                 updateflag = false;
                 return;
             }

             if (BlockSending() == false)
             {
                 updateflag = false;
                 return;
             }

       //*  
             System.Threading.Thread.Sleep(3 * 1000);//unit：1s

            if (FirmwareInformation() == false)
             {
                 updateflag = false;
                 return;
             }
        //*/
            MessageBox.Show(" 升级完成" + '\n' + "App update succeed", "Succeed");
            updateflag = false;

         }
        #endregion
        private void UpdateFirmware(object sender, EventArgs e)
        {
            if (updateflag == false)
            {
                // 创建数据处理线程
                threadUpdate = new Thread(new ThreadStart(OnUpdateFirmware));
                threadUpdate.Start();
                updateflag = true;
            }
                       
        }


        private void btnUpdateFile_Click(object sender, EventArgs e)
        {
           /* if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtUpdateFileDir.Text = folderBrowserDialog1.SelectedPath;
            }*/

            openFileDialog1.Filter = "(*.bin)|*.bin";
            if (DialogResult.OK != openFileDialog1.ShowDialog())
                return;
            txtUpdateFileDir.Text = openFileDialog1.FileName;
      
            System.Collections.Generic.List<byte[]> array = new List<byte[]>();
            System.IO.FileStream fs = new System.IO.FileStream(openFileDialog1.FileName, System.IO.FileMode.Open);
            System.IO.BinaryReader read = new System.IO.BinaryReader(fs);
            updateFileBuf = read.ReadBytes(Convert.ToInt32(fs.Length));
               
            updateFileLen = Convert.ToInt32(fs.Length);
            updateFileCrc = CRC16RTU(updateFileBuf, updateFileLen);
        }

        public void tmrOutputDisplay_Tick(object sender, EventArgs e)
        {
//            this.txtOutput.Focus();//获取焦点
//            this.txtOutput.Select(this.txtOutput.TextLength, 0);//光标定位到文本最后
//            this.txtOutput.ScrollToCaret();//滚动到光标处   
        }

        private void btnClear_Click_1(object sender, EventArgs e)
        {
            this.txtOutput.Clear();    
        }

        private void button_StartCAN_Click(object sender, EventArgs e)
        {

        }
        public Thread threadCan;
        private void buttonConnect_Click(object sender, EventArgs e)
        {

            if (m_bOpen == 1)
            {
                VCI_CloseDevice(m_devtype, m_devind);
                m_bOpen = 0;
            }
            else
            {
                m_arrdevtype[0] = DEV_USBCAN;
                m_arrdevtype[1] = DEV_USBCAN2;
                try
                {
                    m_devtype = m_arrdevtype[schemeConfigFrm.comboBox_devtype.SelectedIndex];
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                
                if(m_devtype == null)
                {
                    MessageBox.Show("请配置comset");
                }

                m_devind = (UInt32)schemeConfigFrm.comboBox_DevIndex.SelectedIndex; //comboBox_DevIndex.SelectedIndex;
                m_canind = (UInt32)(UInt32)schemeConfigFrm.comboBox_CANIndex.SelectedIndex; //comboBox_CANIndex.SelectedIndex;
                if (VCI_OpenDevice(m_devtype, m_devind, 0) == 0)
                {
                    MessageBox.Show("打开设备失败,请检查设备类型和设备索引号是否正确", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                m_bOpen = 1;
                VCI_INIT_CONFIG config = new VCI_INIT_CONFIG();
                config.AccCode = System.Convert.ToUInt32("0x" + schemeConfigFrm.textBox_AccCode.Text, 16);
                config.AccMask = System.Convert.ToUInt32("0x" + schemeConfigFrm.textBox_AccMask.Text, 16);
                config.Timing0 = System.Convert.ToByte("0x" + schemeConfigFrm.textBox_Time0.Text, 16);
                config.Timing1 = System.Convert.ToByte("0x" + schemeConfigFrm.textBox_Time1.Text, 16);
                config.Filter = (Byte)(schemeConfigFrm.comboBox_Filter.SelectedIndex + 1);
                config.Mode = (Byte)schemeConfigFrm.comboBox_Mode.SelectedIndex;
                VCI_InitCAN(m_devtype, m_devind, m_canind, ref config);
            }
            buttonConnect.Text = m_bOpen == 1 ? "断开" : "连接";
            timer_rec.Enabled = m_bOpen == 1 ? true : false;


            //            threadCan = new Thread(new ThreadStart(OnCanReceivedData));
            //threadCan.Start();

          /*  if (m_bOpen == 1)
            {
                MessageBox.Show("关闭设备");
                VCI_CloseDevice(m_devtype, m_devind);
                m_bOpen = 0;
            }
            else
            {
               
                m_arrdevtype[0] = DEV_USBCAN;
                   m_arrdevtype[1] = DEV_USBCAN2;
                m_devtype = m_arrdevtype[schemeConfigFrm.comboBox_devtype.SelectedIndex];

                m_devind = (UInt32)schemeConfigFrm.comboBox_DevIndex.SelectedIndex;
                m_canind = (UInt32)schemeConfigFrm.comboBox_CANIndex.SelectedIndex;
                if (VCI_OpenDevice(m_devtype, m_devind, 0) == 0)
                {
                    MessageBox.Show("打开设备失败,请检查设备类型和设备索引号是否正确", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }



                //System.Threading.Thread.Sleep(1000);
                //启动CAN
                VCI_StartCAN(m_devtype, m_devind, m_canind);
                // 创建数据处理线程
         
            }
            buttonConnect.Text = m_bOpen == 1 ? "disconnect" : "connect";

           timer_rec.Enabled = m_bOpen == 1 ? true : false;
           */
        }

        private void button_Send_Click(object sender, EventArgs e)
        {

        }

    //   #region//进程

        //private void timer_rec_Tick()
       unsafe private void OnCanReceivedData()
        {
 
            UInt32 res = new UInt32();

           //System.Threading.Thread.Sleep(300);//默认设置
            System.Threading.Thread.Sleep(200);//dubug lin 20191105

                //res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 400);
            res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 200);//dubug lin 20191113 20200615


                if (res == 0xffffffff)
                    return;
                String str = "";
                for (UInt32 i = 0; i < res; i++)
                {

                    if (m_recobj[i].RemoteFlag == 0)
                    {
                        //                    str += "数据: ";
                        byte len = (byte)(m_recobj[i].DataLen );
                        byte j = 0;
                        fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
                        {
                            if (j++ < len)
                                //str += " " + System.Convert.ToString(m_recobj1->Data[0], 16);
                                ReceivedDataBuff.Add(m_recobj1->Data[0]);
                            if (j++ < len)
                                //str += " " + System.Convert.ToString(m_recobj1->Data[1], 16);
                                ReceivedDataBuff.Add(m_recobj1->Data[1]);
                            if (j++ < len)
                                //str += " " + System.Convert.ToString(m_recobj1->Data[2], 16);
                                ReceivedDataBuff.Add(m_recobj1->Data[2]);
                            if (j++ < len)
                                // str += " " + System.Convert.ToString(m_recobj1->Data[3], 16);
                                ReceivedDataBuff.Add(m_recobj1->Data[3]);
                            if (j++ < len)
                                //str += " " + System.Convert.ToString(m_recobj1->Data[4], 16);
                                ReceivedDataBuff.Add(m_recobj1->Data[4]);
                            if (j++ < len)
                                //str += " " + System.Convert.ToString(m_recobj1->Data[5], 16);
                                ReceivedDataBuff.Add(m_recobj1->Data[5]);
                            if (j++ < len)
                                //str += " " + System.Convert.ToString(m_recobj1->Data[6], 16);
                                ReceivedDataBuff.Add(m_recobj1->Data[6]);
                            if (j++ < len)
                                //str += " " + System.Convert.ToString(m_recobj1->Data[7], 16);
                                ReceivedDataBuff.Add(m_recobj1->Data[7]);
                        }
                    }
                }
                     try
                {
                    
                    logstemp = "\r\n接收：";
                    for (int i = 0; i < ReceivedDataBuff.Count; i++)
                    {
                        logstemp += String.Format("{0:X2}", (byte)ReceivedDataBuff[i]);
                        logstemp += " ";
                    }
                    // information.textBox1.Text += stemp;
                    // txtOutput.Show();
                    //tmrOutputDisplay.Enabled = true;
                   
                }
                catch (SystemException er)
                {
                    MessageBox.Show(er.Message);
                }

                int startindex = 0, endindex = 0;
                Frame_Stu RuleInfo = new Frame_Stu();
                while (true)
                {
                    /* if (ReceivedDataBuff.Count == 0x12 && (byte)ReceivedDataBuff[4] == 0x68 && ((byte)ReceivedDataBuff[12] == 0x81 || (byte)ReceivedDataBuff[12] == 0x84) && (byte)ReceivedDataBuff[14] == 0x33 && (byte)ReceivedDataBuff[15] == 0x44)
                     {
                         RuleInfo.ctrl = 0x84;
                         FrameBuff.Add(RuleInfo);
                         ReceivedDataBuff.Clear();
                         return;
                     }*/
                    switch (Rule.ParsePackage(ref ReceivedDataBuff, ref RuleInfo, ref startindex, ref endindex))
                    {
                        case ParseResult.OK:
                            //ReceivedDataBuff.RemoveRange(0, endindex + 1);
                            //OnReceivedPackage(RuleInfo);
                            if (RuleInfo.afn != (byte)GW_AFN_CODE.Beat)
                            {
                                FrameBuff.Add(RuleInfo);    // 添加接受的数据帧到缓冲区
                            }
                            break;

                        case ParseResult.Waitting:
                            return;

                        case ParseResult.Error:
                            if (ReceivedDataBuff.Count > 0)
                            {
                                try
                                {
                                    ReceivedDataBuff.RemoveRange(0, endindex + 1);
                                }
                                catch (Exception ex)
                                {
                                    //MessageBox.Show(this,ex.Message.ToString());
                                }
                            }
                            return;
                        case ParseResult.Unintegrated:
                            return;
                        default:
                            return;
                    }
                }

        }

        private void timer_rec_Tick(object sender, EventArgs e)
        {
            m_recv_time_tick_100ms++;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[256];
            if (m_bOpen == 0)
                return ;


            int pos = 0;

            pos = 0;
            for (int i=0; i<168; i++)
            {

                data[pos++] = 0x03;
            }



            sendCanDatas(data, pos);
          
        }

        private void RequestButton_Click(object sender, EventArgs e)
        {
            if (RequestUpdate() == false)
            {
                return;
            }
            Application.DoEvents();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (FirmwareInformation() == false)
            {
                return;
            }
            Application.DoEvents();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (BlockSending() == false)
            {
                return;
            }
            Application.DoEvents();
        }


//升级压力测试
        private void UpdateFileInfo(string path)
        {
            System.Collections.Generic.List<byte[]> array = new List<byte[]>();
            //System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open);
            System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
            System.IO.BinaryReader read = new System.IO.BinaryReader(fs);
            updateFileBuf = read.ReadBytes(Convert.ToInt32(fs.Length));

            updateFileLen = Convert.ToInt32(fs.Length);
            updateFileCrc = CRC16RTU(updateFileBuf, updateFileLen);
        }

        #region//进程
        private void OnUpdateFirmwareRepeat()
        {
            int finishedTimes = 0;
            int sucessTimes = 0;
            int failTimes = 0;
            int[] failTimes_COM = new int[4];
            int txtOutputOkCnt = 0;
            int txtOutputCnt = 0;

            //UpdateFileInfo(txtUpdateFileDir.Text); //更新bin文档

            for (int i = 0; i < Convert.ToInt64(textBox3.Text, 10); i++)
            {
                txtFinishedTimes.Text = finishedTimes.ToString();
                txtSucessTimes.Text = sucessTimes.ToString();
                txtFailTimes.Text = failTimes.ToString();
                txtFailTimes_COM11.Text = failTimes_COM[0].ToString();
                txtFailTimes_COM12.Text = failTimes_COM[1].ToString();
                txtFailTimes_COM13.Text = failTimes_COM[2].ToString();
                txtFailTimes_COM12_2.Text = failTimes_COM[3].ToString();

                if (updateflag == false)
                {
                    break;
                }

                if (i > 0)
                {
                    System.Threading.Thread.Sleep((Convert.ToInt16(textBox4.Text, 10)) * 1000);//unit：1s
                }

                finishedTimes++;
                txtFinishedTimes.Text = finishedTimes.ToString();
                txtSucessTimes.Text = sucessTimes.ToString();
                txtFailTimes.Text = failTimes.ToString();

                //this.txtOutput.Clear();//清显示缓存
                if (++txtOutputCnt > 10)//异常情况 每10次正常升级清屏一次
                {
                    txtOutputCnt = 0;
                    this.txtOutput.Clear();
                }

                if (txtOutputOkCnt > 0)
                {
                    txtOutputCnt = 0;
                    txtOutputOkCnt = 0;
                    this.txtOutput.Clear();
                }

                if (RequestUpdate() == false) //COM11
                {
                    failTimes++;
                    failTimes_COM[0]++;
                    txtFailTimes.Text = failTimes.ToString();
                    continue;
                }
                Application.DoEvents();
                System.Threading.Thread.Sleep(7 * 1000);//unit：1s
                if (FirmwareInformation() == false) //COM12
                {
                    failTimes++;
                    failTimes_COM[1]++;
                    txtFailTimes.Text = failTimes.ToString();
                    continue;
                }
                Application.DoEvents();
                if (BlockSending() == false)  //COM13
                {
                    failTimes++;
                    failTimes_COM[2]++;
                    txtFailTimes.Text = failTimes.ToString();
                    continue;
                }
                Application.DoEvents();
                System.Threading.Thread.Sleep(2 * 1000);//unit：1s
                if (FirmwareInformation() == false)  //COM12
                {
                    failTimes++;
                    failTimes_COM[3]++;
                    txtFailTimes.Text = failTimes.ToString();
                    continue;
                }
                Application.DoEvents();

                //this.txtOutput.Clear();//清显示缓存

                //System.Threading.Thread.Sleep(Convert.ToByte(textBox4.Text, 10));

                sucessTimes++;
                txtSucessTimes.Text = sucessTimes.ToString();

                txtOutputOkCnt++;
                /*
                                if (++txtOutputCnt > 1)//每1次正常升级清屏一次
                                {
                                    txtOutputCnt = 0;
                                    this.txtOutput.Clear();
                                }
                 */
            }

            txtFinishedTimes.Text = finishedTimes.ToString();
            txtSucessTimes.Text = sucessTimes.ToString();
            txtFailTimes.Text = failTimes.ToString();
            txtFailTimes_COM11.Text = failTimes_COM[0].ToString();
            txtFailTimes_COM12.Text = failTimes_COM[1].ToString();
            txtFailTimes_COM13.Text = failTimes_COM[2].ToString();
            txtFailTimes_COM12_2.Text = failTimes_COM[3].ToString();


            updateflag = false;
        }
        #endregion

        private void StatUpdate_Click(object sender, EventArgs e)
        {
            //*
            if (updateflag == false)
            {
                // 创建数据处理线程
                threadUpdate = new Thread(new ThreadStart(OnUpdateFirmwareRepeat));
                threadUpdate.Start();
                updateflag = true;
            }
            //*/
            /*
            int finishedTimes = 0;
            int sucessTimes = 0;
            int failTimes = 0;
            int[] failTimes_COM = new int[4];
            int txtOutputOkCnt = 0;
            int txtOutputCnt = 0;

            //UpdateFileInfo(txtUpdateFileDir.Text); //更新bin文档

            for (int i = 0; i < Convert.ToInt64(textBox3.Text, 10); i++)
            {
                txtFinishedTimes.Text = finishedTimes.ToString();
                txtSucessTimes.Text = sucessTimes.ToString();
                txtFailTimes.Text = failTimes.ToString();
                txtFailTimes_COM11.Text = failTimes_COM[0].ToString();
                txtFailTimes_COM12.Text = failTimes_COM[1].ToString();
                txtFailTimes_COM13.Text = failTimes_COM[2].ToString();
                txtFailTimes_COM12_2.Text = failTimes_COM[3].ToString();

                if (i > 0)
                {
                    System.Threading.Thread.Sleep((Convert.ToInt16(textBox4.Text, 10)) * 1000);//unit：1s
                }

                finishedTimes++;
                txtFinishedTimes.Text = finishedTimes.ToString();
                txtSucessTimes.Text = sucessTimes.ToString();
                txtFailTimes.Text = failTimes.ToString();

                //this.txtOutput.Clear();//清显示缓存
                if (++txtOutputCnt > 10)//异常情况 每10次正常升级清屏一次
                {
                    txtOutputCnt = 0;
                    this.txtOutput.Clear();
                }

                if (txtOutputOkCnt > 0)
                {
                    txtOutputCnt = 0;
                    txtOutputOkCnt = 0;
                    this.txtOutput.Clear();
                }

                if (RequestUpdate() == false) //COM11
                {
                    failTimes++;
                    failTimes_COM[0]++;
                    txtFailTimes.Text = failTimes.ToString();
                    continue;
                }
                Application.DoEvents();
                System.Threading.Thread.Sleep(5 * 1000);//unit：1s
                if (FirmwareInformation() == false) //COM12
                {
                    failTimes++;
                    failTimes_COM[1]++;
                    txtFailTimes.Text = failTimes.ToString();
                    continue;
                }
                Application.DoEvents();
                if (BlockSending() == false)  //COM13
                {
                    failTimes++;
                    failTimes_COM[2]++;
                    txtFailTimes.Text = failTimes.ToString();
                    continue;
                }
                Application.DoEvents();
                System.Threading.Thread.Sleep(2 * 1000);//unit：1s
                if (FirmwareInformation() == false)  //COM12
                {
                    failTimes++;
                    failTimes_COM[3]++;
                    txtFailTimes.Text = failTimes.ToString();
                    continue;
                }
                Application.DoEvents();

                //this.txtOutput.Clear();//清显示缓存

                //System.Threading.Thread.Sleep(Convert.ToByte(textBox4.Text, 10));

                sucessTimes++;
                txtSucessTimes.Text = sucessTimes.ToString();

                txtOutputOkCnt++;

            }

            txtFinishedTimes.Text = finishedTimes.ToString();
            txtSucessTimes.Text = sucessTimes.ToString();
            txtFailTimes.Text = failTimes.ToString();
            txtFailTimes_COM11.Text = failTimes_COM[0].ToString();
            txtFailTimes_COM12.Text = failTimes_COM[1].ToString();
            txtFailTimes_COM13.Text = failTimes_COM[2].ToString();
            txtFailTimes_COM12_2.Text = failTimes_COM[3].ToString();
             */
        }

        private void updateStopTest_Click(object sender, EventArgs e)
        {
            txtOutput.Text += "\r\n<<<<<<<<   Stop update    >>>>>>>>>>>>>>>>>";
            WriteMessage("\r\n<<<<<<<<   Stop update    >>>>>>>>>>>>>>>>>");
            updateflag = false;
        }
//升级压力测试结束

        public Thread threadApolloUpdate;
        private bool ApolloUpdateflag = false;
  #region//进程
        private void OnApolloUpdateFirmware()
        {
        	 int delayTimes = 5 * 1000;
			 
            if (RequestUpdate() == false)
            {
                CanBaudSet(500);
                delayTimes = 1000;
                if (RequestUpdate() == false)
                {
                    ApolloUpdateflag = false;
                    CanBaudSet(250);
                    return;
                }
            }
			
            CanBaudSet(500);
            System.Threading.Thread.Sleep(delayTimes);//unit：1ms

            if (FirmwareInformation() == false)
            {
                ApolloUpdateflag = false;
                CanBaudSet(250);
                return;
            }

            if (BlockSending() == false)
            {
                ApolloUpdateflag = false;
                CanBaudSet(250);
                return;
            }
            CanBaudSet(250);
            //*  
            System.Threading.Thread.Sleep(3 * 1000);//unit：1ms

            if (FirmwareInformation() == false)
            {
                ApolloUpdateflag = false;
                return;
            }

            //MessageBox.Show("APP Update Success");
            MessageBox.Show(" 升级完成"+ '\n'+ "App update succeed", "Succeed");
            //*/
            ApolloUpdateflag = false;
        }
  #endregion
        private void button_apollo_update_Click(object sender, EventArgs e)
        {
            if (ApolloUpdateflag == false)
            {
                // 创建数据处理线程
                threadApolloUpdate = new Thread(new ThreadStart(OnApolloUpdateFirmware));
                threadApolloUpdate.Start();
                ApolloUpdateflag = true;
            }
        }

        private void label190_Click(object sender, EventArgs e)
        {

        }

        private void txtFailTimes_COM12_2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label189_Click(object sender, EventArgs e)
        {

        }

        private void txtFailTimes_COM13_TextChanged(object sender, EventArgs e)
        {

        }

        private void label188_Click(object sender, EventArgs e)
        {

        }

        private void txtFailTimes_COM12_TextChanged(object sender, EventArgs e)
        {

        }

        private void label187_Click(object sender, EventArgs e)
        {

        }

        private void txtFailTimes_COM11_TextChanged(object sender, EventArgs e)
        {

        }

        private void label186_Click(object sender, EventArgs e)
        {

        }

        private void txtFinishedTimes_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void label185_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label184_Click(object sender, EventArgs e)
        {

        }

        private void label183_Click(object sender, EventArgs e)
        {

        }

        private void txtFailTimes_TextChanged(object sender, EventArgs e)
        {

        }

        private void label182_Click(object sender, EventArgs e)
        {

        }

        private void txtSucessTimes_TextChanged(object sender, EventArgs e)
        {

        }

        private void SV_L_TextChanged(object sender, EventArgs e)
        {

        }

        private void SV_M_TextChanged(object sender, EventArgs e)
        {

        }

        private void label154_Click(object sender, EventArgs e)
        {

        }

        private void SV_H_TextChanged(object sender, EventArgs e)
        {

        }

        private void HV_L_TextChanged(object sender, EventArgs e)
        {

        }

        private void HV_M_TextChanged(object sender, EventArgs e)
        {

        }

        private void label153_Click(object sender, EventArgs e)
        {

        }

        private void HV_H_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtUpdateFileDir_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtOutput_TextChanged(object sender, EventArgs e)
        {

        }

        private void SocCalibration_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[32];

            uint canid;
            canid = 0x00515606;

            data[0] = 0x0A;  // soc校准
            //data[1] = 0x00;  // hall zero校准
            int len = 1;

            sendCanDatas_Vcu(canid, data, len);

        }


  //     #endregion
    }
      
}

      
