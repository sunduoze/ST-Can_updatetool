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

/*------------����ZLG����������---------------------------------*/

//1.ZLGCANϵ�нӿڿ���Ϣ���������͡�
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
//2.����CAN��Ϣ֡���������͡�
unsafe public struct VCI_CAN_OBJ  //ʹ�ò���ȫ����
{
    public uint ID;
    public uint TimeStamp;        //ʱ���ʶ
    public byte TimeFlag;         //�Ƿ�ʹ��ʱ���ʶ
    public byte SendType;         //���ͱ�־��������δ��
    public byte RemoteFlag;       //�Ƿ���Զ��֡
    public byte ExternFlag;       //�Ƿ�����չ֡
    public byte DataLen;          //���ݳ���
    public fixed byte Data[8];    //����
    public fixed byte Reserved[3];//����λ

}

//3.�����ʼ��CAN����������
public struct VCI_INIT_CONFIG 
{
    public UInt32 AccCode;
    public UInt32 AccMask;
    public UInt32 Reserved;
    public byte Filter;   //0��1��������֡��2��׼֡�˲���3����չ֡�˲���
    public byte Timing0;  //�����ʲ������������ã���鿴���ο����⺯��˵���顣
    public byte Timing1;
    public byte Mode;     //ģʽ��0��ʾ����ģʽ��1��ʾֻ��ģʽ,2�Բ�ģʽ
}

/*------------�������ݽṹ����---------------------------------*/
//4.USB-CAN�����������忨��Ϣ����������1��������ΪVCI_FindUsbDevice�����ķ��ز�����
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

/*------------���ݽṹ�������---------------------------------*/

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
        /*------------����ZLG�ĺ�������---------------------------------*/
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

        /*------------������������---------------------------------*/

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ConnectDevice(UInt32 DevType, UInt32 DevIndex);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_UsbDeviceReset(UInt32 DevType, UInt32 DevIndex, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_FindUsbDevice(ref VCI_BOARD_INFO1 pInfo);
        /*------------������������---------------------------------*/

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
        public CGwRule GwRule;//������Լ����
        public CGdRule GdRule;//�㶫��Լ����
        public CRule Rule;
        public string logstemp;
        Thread thread; //���Խ���
        byte[] spaddr = { 0x88, 0x88, 0x88, 0x88, 0x88, 0x88 };
        public int UpRecDataLen;
        public const int GW_PW_LEN = 16;
        public const int GX_PW_LEN = 2;


        public enum REC_RESULT
        {

            OK = 0x00, // ȷ��֡
            TYPE2_OK = 0x95,// ȷ��֡
            ERROR = 0xc0, // ����֡
            TIME_OUT = 0xd0//��ʱ֡
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
        /// �������
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
                if (str == "ϵͳʱ��")
                    testID = (int)TESTID.TS_CLOCK;
                else if (str == "���")
                    testID = (int)TESTID.TS_BAT;
                else if (str == "485-1")
                    testID = (int)TESTID.TS_4851;
                else if (str == "485-2")
                    testID = (int)TESTID.TS_4852;
                else if (str == "485-3")
                    testID = (int)TESTID.TS_4853;
                else if (str == "����ͨѶģ��")
                    testID = (int)TESTID.TS_DOWN;
                else if (str == "Զ��ģ��")
                    testID = (int)TESTID.TS_GPRS;
                else if (str == "���Ź�")
                    testID = (int)TESTID.TS_WATCHDOG;
                else if (str == "Զ����")
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
            Control.CheckForIllegalCrossThreadCalls = false; // ����һ���߳̿��Ե��ø��̴߳����Ŀؼ�


            //*******************88Ȩ�޼��
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
            //2018��4��1�չ���
            /*if (date[5] >= 0x18 && date[4] >= 0x4 && date[3] >= 0x01)
            {
                File.Delete(authorFilePath);
                return;
            }*/
            //***************Ȩ�޼�����
            schemeConfigFrm = new FrmConfig(); // �������ô�������

            paramConfig = new FrmKeyboard();//�����������ô���
            frmSet = new FrmSet();//�������ò�������
            powkeySceneSet = new PowkeySceneSet();//�ܵ�Դ����
            doorcardSceneSet = new DoorcardSceneSet();//�ſ�����
            doorSceneSet = new DoorSceneSet();//���ų�������
            irdtSceneSet = new IrdtSceneSet();//���ⳡ������
            sceneSet = new SceneCompose();//��ͨ����
            rcuStateQuq = new FrmRcuState();//RCU�豸״̬��ѯ

            paramConfig.cmbAirsub1Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbAirsub2Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbAirsub3Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbAirsub4Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK1Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK2Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK3Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK4Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK5Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK6Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK7Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK8Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK9Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK10Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK11Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK12Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK13Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK14Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK15Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK16Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK17Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK18Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK19Func.SelectedIndex = 0;//�ؼ���ʼ��
            paramConfig.cmbK20Func.SelectedIndex = 0;//�ؼ���ʼ��

            frmSet.cmbDoorCardType.SelectedIndex = 0;
            frmSet.cmbAirSeason.SelectedIndex = 0;
            frmSet.cmbDoorDisplaytype.SelectedIndex = 0;


            m_areaId = AREA.AREA_CQ;

            information = new Information(); // ����ͨѶ��¼��ӡ����
            GwRule = new CGwRule(); //����������Լ����
            GdRule = new CGdRule(); //�����㶫��Լ����
            //�������������Լ����
            Rule = (CRule)GdRule;

            { ListViewItem item = new ListViewItem("�¶�"); item.SubItems.Add("");  this.testlv.Items.Add(item); }
            { ListViewItem item = new ListViewItem("������ܵ�ѹ"); item.SubItems.Add(""); this.testlv.Items.Add(item); }
            { ListViewItem item = new ListViewItem("ʵʱ����"); item.SubItems.Add(""); this.testlv.Items.Add(item); }
            { ListViewItem item = new ListViewItem("SOC"); item.SubItems.Add(""); this.testlv.Items.Add(item); }
            { ListViewItem item = new ListViewItem("ʣ������"); item.SubItems.Add(""); this.testlv.Items.Add(item); }
            { ListViewItem item = new ListViewItem("��������"); item.SubItems.Add(""); this.testlv.Items.Add(item); }



            { ListViewItem item = new ListViewItem("���ŵ����"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            { ListViewItem item = new ListViewItem("��������"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            { ListViewItem item = new ListViewItem("����¶�"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            { ListViewItem item = new ListViewItem("����¶�"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            { ListViewItem item = new ListViewItem("��������ߵ�ѹ"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            { ListViewItem item = new ListViewItem("��������͵�ѹ"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            { ListViewItem item = new ListViewItem("���ѭ������"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            { ListViewItem item = new ListViewItem("�ŵ�SOC�ۼ�ֵ"); item.SubItems.Add(""); this.lvView2.Items.Add(item); }
            
            
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
            // �������ݴ����߳�
            //��ʼ��ϵͳӲ��ʱ��
            // SetSysInitTime();
            thread = new Thread(new ThreadStart(RunTest));
            thread.Start();
        }

        /// �������ݻ�����
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

                    logstemp = "\r\n���գ�";
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
                                FrameBuff.Add(RuleInfo);    // ��ӽ��ܵ�����֡��������
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
        #region//����
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
                        logstemp = "\r\n���գ�";
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
                                    FrameBuff.Add(RuleInfo);    // ��ӽ��ܵ�����֡��������
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

                // �������ݴ����߳�
                threadNet = new Thread(new ThreadStart(OnNetReceivedData));
                threadNet.Start();
                bNetOpenFlag = true;
                return true;
            }
            catch (Exception ey)
            {
                Console.WriteLine("������û�п���\r\n");
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
                    string str = "����" + serialPort.PortName + "��ʧ�ܣ�";
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

            string stemp = "\r\n���ͣ�";
            for (int i = 0; i < btp.Length; i++)
            {
                stemp += String.Format("{0:X2}", (byte)btp[i]);
                stemp += " ";
            }
           // information.textBox1.Text += stemp;
            //txtOutput.Text += stemp;
            //tmrOutputDisplay.Enabled = true;
        }

        /**************** ��������CAN����������  ****************************/
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
            //����CAN
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
            logstemp = "\r\n���ͣ�";
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
                    MessageBox.Show("����ʧ��", "����",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

            }

            logstemp = "\r\n���ͣ�";
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
                // MessageBox.Show("����ʧ��", "����",
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

        /******************************************  ���� ************************************/

        /// <summary>
        /// ���ڻ�ȡ�������
        /// </summary>
        /// <param name="MeterAddr">����ַ�����ȱ���Ϊ12�ֽڣ�������а�����BCD�ַ�����������ҿ�ʼ��ʽ
        /// ����ȡ��һ��BCD���ַ�����β��Ϊ����ͨѶ��ַ��������ģ������</param>
        /// 
        /// <returns></returns>
        private AREA GetArea(string str)
        {
            if (str == "����")
            {
                return AREA.AREA_CQ;
            }
            else if (str == "�㶫")
            {
                return AREA.AREA_GD;
            }
            else if (str == "����")
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
                MessageBox.Show("�豸��ַ������Ϊ8λ���֣�");
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
                MessageBox.Show("�豸��ַ������Ϊ8λ���֣�");
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
                MessageBox.Show("����ַ������Ϊ12λ���֣�");
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
                MessageBox.Show("���Ź����ڴ�ʧ�ܣ�ϵͳʱ���ʼ��ʧ�ܣ�");
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
            //����һ��������Ϣ�� 
            System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo();
            //�����ⲿ������ 
            Info.FileName = "telnetreboot.exe";
            //�����ⲿ��������������������в�����Ϊtest.txt 
            Info.Arguments = "";
            //�����ⲿ������Ŀ¼ΪC:\\ 
            Info.WorkingDirectory = telnetRebootPath;
            //����һ�������� 
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
            //����һ��������Ϣ�� 
            System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo();
            //�����ⲿ������ 
            Info.FileName = "delGdUserDir.exe";
            //�����ⲿ��������������������в�����Ϊtest.txt 
            Info.Arguments = "";
            //�����ⲿ������Ŀ¼ΪC:\\ 
            Info.WorkingDirectory = telnetRebootPath;
            //����һ�������� 
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


        /******************************************  ������ ************************************/

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
                                MessageBox.Show("��������ʧ�ܣ��˳�����");
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
                MessageBox.Show("���Խ�����");
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
                    strDate = String.Format("{0:X2}", (byte)data[5]) + "��" + String.Format("{0:X2}", (byte)(data[4] & 0x1f)) + "��" + String.Format("{0:X2}", (byte)data[3]) + "��";
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
                    strDate = String.Format("{0:X2}", (byte)data[5]) + "��" + String.Format("{0:X2}", (byte)(data[4] & 0x1f)) + "��" + String.Format("{0:X2}", (byte)data[3]) + "��";
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

            //������
            data[0] = 0x00;
            data[1] = 0x00;
            //Ȩ������
            data[2] = 0x11;
            data[3] = 0x11;
            data[4] = 0x11;
            data[5] = 0x11;
            //������8030
            data[6] = 0x30;
            data[7] = 0x80;
            //���ʱ������
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
                // ������
                dataArry[0] = 0x01;
                // ������
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
                    MessageBox.Show("�뽫�������ϵ磬������������ϵ磬���������������к󣬵����ʾ��ȷ�ϡ���ť�������ԣ�");
                    System.Threading.Thread.Sleep(15000);
                    if (OpenNetPort(portset) == false)
                    {
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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
                    MessageBox.Show("�뽫�������ϵ磬������������ϵ磬���������������к󣬵����ʾ��ȷ�ϡ���ť�������ԣ�");
                    System.Threading.Thread.Sleep(15000);
                    if (OpenNetPort(portset) == false)
                    {
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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

            //������
            data[0] = 0x00;
            data[1] = 0x00;
            //Ȩ������
            data[2] = 0x11;
            data[3] = 0x11;
            data[4] = 0x11;
            data[5] = 0x11;
            //������8030
            data[6] = 0x30;
            data[7] = 0x80;
            //���ʱ������
            data[8] = Convert.ToByte(DateTime.Now.Second.ToString(), 16);
            data[9] = Convert.ToByte(DateTime.Now.Minute.ToString(), 16);
            data[10] = Convert.ToByte(DateTime.Now.Hour.ToString(), 16);
            data[11] = Convert.ToByte(DateTime.Now.Day.ToString(), 16);

            data[12] = Convert.ToByte(DateTime.Now.Month.ToString(), 16);
            byte month = data[12];

            data[13] = Convert.ToByte(year, 16);

            if (SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, ref frame, 5, 3) == REC_RESULT.OK)
            {
                MessageBox.Show("�뽫�������ϵ磬������������ϵ磬���������������к󣬵����ʾ��ȷ�ϡ���ť�������ԣ�");
                System.Threading.Thread.Sleep(15000);
                if (OpenNetPort(portset) == false)
                {
                    MessageBox.Show("��������ʧ�ܣ��˳�����");
                    thread.Abort();
                    return false;
                }
                System.Threading.Thread.Sleep(1000);
                FrameBuff.Clear();
                byte[] dataArry = new byte[10];
                dataArry.Initialize();
                // ������
                dataArry[0] = 0x01;
                // ������
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
                    getData = String.Format("{0:X2}", (byte)data[4]) + "��" + String.Format("{0:X2}", (byte)data[3]) + "��" + String.Format("{0:X2}", (byte)data[2]) + "��" + String.Format("{0:X2}", (byte)data[1]) + "ʱ" + String.Format("{0:X2}", (byte)data[1]) + "��";
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
                sdata[0] = 0x00;//������ʽ

                sdata[1] = 0x01;//������
                sdata[2] = 0x00;

                sdata[3] = 0x01;//�������
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

            //���ַ
            GetGdMeterAddr(meterAddr, ref data);
            //TN��
            data[6] = (byte)(tn & 0x0ff);
            data[7] = (byte)0x00; //data[7] = (byte)((tn >> 8) & 0xff);
            // �Զ��м�
            data[8] = 0xff;
            // �м̱��
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
                sdata[0] = 0x00;//������ʽ

                sdata[1] = 0x01;//������
                sdata[2] = 0x00;

                sdata[3] = 0x02;//�������
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
                sdata[0] = 0x00;//������ʽ

                sdata[1] = 0x01;//������
                sdata[2] = 0x00;

                sdata[3] = 0x02;//�������
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
                    getData = String.Format("{0:X2}", (byte)data[4]) + "��" + String.Format("{0:X2}", (byte)data[3]) + "��" + String.Format("{0:X2}", (byte)data[2]) + "��" + String.Format("{0:X2}", (byte)data[1]) + "ʱ" + String.Format("{0:X2}", (byte)data[1]) + "��";
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
                sdata[0] = 0x00;//������ʽ

                sdata[1] = 0x01;//������
                sdata[2] = 0x00;

                sdata[3] = 0x03;//�������
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
                            strDate = String.Format("{0:X2}", (byte)data[2]) + "ʱ" + String.Format("{0:X2}", (byte)data[1]) + "��" + String.Format("{0:X2}", (byte)data[0]) + "��";
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
                // �㶫������� Ŀǰȱʡ
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
            bTestWatchDog = true; //��ʼ���������ϱ�����
            information.textBox1.Text = "";
            txtOutput.Text = "";
            System.Threading.Thread.Sleep(15000);


            for (int j = 0; j < 300; j++)
            {
                if (information.textBox1.Text.ToString().Contains(returnKeyword) == true)
                {

                    bTestWatchDog = false; //ֹͣ���������ϱ�����
                    return true;
                }
                System.Threading.Thread.Sleep(100);
            }
            bTestWatchDog = false; //ֹͣ���������ϱ�����
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
            MessageBox.Show("����ֹͣ��");
            thread.Abort();
        }

        //private string GetTester()
        //{
        //    // ����Application����
        //    Missing Miss = Missing.Value;
        //    int i = 2, j = 0;

        //    // ����Application����
        //    Excel.Application xlsApp = new Excel.Application();
        //    if (xlsApp == null)
        //    {
        //        return "";
        //    }

        //    if (xlsApp == null)
        //    {
        //        return "";
        //    }

        //    /* ���ļ� */
        //    Excel.Workbook xlsBook = xlsApp.Workbooks.Open(testerFilePath, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss);
        //    Excel.Worksheet xlsSheet = (Excel.Worksheet)xlsBook.Sheets[1];

        //    string name = (string)(((Excel.Range)xlsSheet.Cells[2, 1]).Value2);
        //    // �ر�XLS�ļ�
        //    xlsBook.Close(false, Type.Missing, Type.Missing);
        //    xlsApp.Quit();

        //    // ����������ر�excel.exe
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
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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

                //   getData = String.Format("{0:X2}", (byte)data[4]) + "��" + String.Format("{0:X2}", (byte)data[3]) + "��" + String.Format("{0:X2}", (byte)data[2]) + "��" + String.Format("{0:X2}", (byte)data[1]) + "ʱ" + String.Format("{0:X2}", (byte)data[1]) + "��";
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
                MessageBox.Show("�������豸���Ϊ20100001 ~ 20109999");
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
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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

                //   getData = String.Format("{0:X2}", (byte)data[4]) + "��" + String.Format("{0:X2}", (byte)data[3]) + "��" + String.Format("{0:X2}", (byte)data[2]) + "��" + String.Format("{0:X2}", (byte)data[1]) + "ʱ" + String.Format("{0:X2}", (byte)data[1]) + "��";
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
            aboutFrame.Text = "���˵��";
            aboutFrame.labelProductName.Text = "������ƣ�ģ����У׼���";
            aboutFrame.labelVersion.Text = String.Format("�汾{0} ", " V1.3-2017-0505");
            aboutFrame.labelCopyright.Text = "Copyright";
            aboutFrame.labelCompanyName.Text = "���������ܵ��ӿƼ����޹�˾";
            aboutFrame.About.Text = "ʹ��˵��:\n"
                                     + "\r\n1.����ͨѶ���ڲ���"
                                     + "\r\n2.�������ֵ��ȡ��ť����ȡ�ɼ������ֵ��Ϣ"
                                     + "\r\n3.�����ȡУ����ť����ȡ������У׼ϵ��"
                                     + "\r\n4.ѡ����Ҫ����У����ѡ�򣬲������Ӧ�ο�Դ"
                                     + "\r\n5.��� '����У��'��ť���������ݲο�Դֵ�Ͳ���ֵ�����У׼ϵ��"
                                     + "\r\n6.������·�У׼��������ť����У׼�����·����ɼ���"
                                     + "\r\nע.���������ϵ������ť���������ѡ���У�����óɳ�ʼϵ��1.0";
                                     
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
            FUNC_LIGHT_UP = 1,//����
            FUNC_LIGHT_DOWN = 2,//����
            FUNC_LIGHT_AUTO = 3,
            FUNC_LIGHT_AUTO_UP = 4,//�򿪼̵�������
            FUNC_LIGHT_AUTO_DOWN = 5,//�������ص��̵���
            FUNC_POWER_KEY = 6,
            FUNC_KEYBACKLIGHT = 7,
            FUNC_NIGHT_LIGHT = 8,
            FUNC_RELAY_ON = 9,//�̵�����
            FUNC_RELAY_OFF = 10,//�̵�����
            FUNC_WINDOW_OPEN = 11,//�����������
            FUNC_WINDOW_CLOSE = 12,//�����ص����
            FUNC_SINGLE_BRAKE = 13,//�յ����ܷ�
            FUNC_AIRCDI_DOUBLE_COLD_BRAKE = 14,//�յ�˫���䷧
            FUNC_AIRCDI_DOUBLE_HOT_BRAKE = 15,//�յ�˫���ȷ�
            FUNC_AIRCDI_WIND_SPEED = 16,//�յ�����ٶȿ���
            FUNC_AIRCDI_DIGTIAL_BACKLIGHT = 17,//�յ�����ܱ������
            FUNC_URGENT_KEY = 18,//������ť
            FUNC_DOOR_CHECK = 19,//�����Ŵż��
            FUNC_GALLERY_LIGHT = 20,//�ȵ�
            FUNC_KEY_CLEANROOM = 21,//������
            FUNC_KEY_DONOT_DISTURB = 22,//�������
            FUNC_KEY_BELL = 23,//����
            FUNC_INPUT_PORT_TRIGLE = 24,//����˵�ƽ���ϴ���
            FUNC_INPUT_PORT_HIGH_LEVEL = 25,//����˸ߵ�ƽ����
            FUNC_INPUT_PORT_LOW_LEVEL = 26,//����˵͵�ƽ����
            FUNC_LIGHT_LEVEL = 27,//����������
            FUNC_KEY_WASH_CLOSE = 28,//ϴ�·�
            FUNC_SCENE_COMPOSE1 = 29,//���1�ƹ�����
            FUNC_SCENE_COMPOSE2 = 30,//���2�ƹ�����
            FUNC_SCENE_COMPOSE3 = 31,//���3�ƹ�����
            FUNC_SCENE_COMPOSE4 = 32,//���4�ƹ�����
            FUNC_SCENE_COMPOSE5 = 33,//���5�ƹ�����
            FUNC_SCENE_COMPOSE6 = 34,//���6�ƹ�����
            FUNC_SCENE_COMPOSE7 = 35,//���7�ƹ�����
            FUNC_LEFT_BED_LIGHT = 36,//��ͷ��
            FUNC_RIGHT_BED_LIGHT = 37,//�Ҵ�ͷ��
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
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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


            //������
            int pos = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //Ȩ������
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //������895D
            data[pos++] = 0x5D;
            data[pos++] = 0x89;

            data[pos++] = 0x00;	//����

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

            data[pos++] = (byte)(9 - uncheckedmodulenum);//ģ����
            UInt32 subno;
            /* �ſ�ģ��*/
            if (paramConfig.chkDoorcard.Checked != false)
            {
                data[pos++] = Convert.ToByte(paramConfig.txtDCNo.Text.ToString(), 10);	//�豸���	
                GetGdMeterAddr(paramConfig.txtDoorCardAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//��������
                data[pos++] = (byte)DEVICE_TYPE.MD_RFID_CARD;	//�豸����
                data[pos++] = 0x04;//������		
                data[pos++] = 0x00;//�����豸��

            }

            /* 4	·�̵������*/
            if (paramConfig.chkOrelay.Checked == true)
            {
                data[pos++] = Convert.ToByte(paramConfig.txt4RNo.Text.ToString(), 10);	//�豸���	
                GetGdMeterAddr(paramConfig.txt4RelayAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//��������
                data[pos++] = (byte)DEVICE_TYPE.MD_LIGHT_4;	//�豸����
                data[pos++] = 0x04;//������		
                data[pos++] = 0x04;//�����豸��
                /* 1·����*/
                data[pos++] = 0x1;//��·��
                data[pos++] = (byte)(paramConfig.cmbORFunc1.SelectedIndex); ;//���⹦������
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
                /* 2·����*/
                data[pos++] = 0x2;//��·��
                data[pos++] = (byte)(paramConfig.cmbORFunc2.SelectedIndex); //���⹦������
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
                /* 3·����*/
                data[pos++] = 0x3;//��·��
                data[pos++] = (byte)(paramConfig.cmbORFunc3.SelectedIndex); ;//���⹦������
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
                /* 4·����*/
                data[pos++] = 0x4;//��·��
                data[pos++] = (byte)(paramConfig.cmbORFunc4.SelectedIndex); ;//���⹦������
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

            /* �յ�ģ��*/
            if (paramConfig.chkAircondition.Checked != false)
            {
                data[pos++] = Convert.ToByte(paramConfig.txtAirNo.Text.ToString(), 10);	//�豸���	
                GetGdMeterAddr(paramConfig.txtAirAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//��������
                data[pos++] = (byte)DEVICE_TYPE.MD_AIRCONDITION;	//�豸����
                data[pos++] = 0x04;//������		
                data[pos++] = 0x04;//�����豸��
                /*�����豸1*/
                data[pos++] = 0x0;//��·��
                data[pos++] = (byte)(paramConfig.cmbAirsub1Func.SelectedIndex);//���⹦������
                data[pos++] = Convert.ToByte(paramConfig.txtAirCtlDNo1.Text.ToString(), 10);	//�����豸��	        
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
                /*�����豸2*/
                data[pos++] = 0x0;//��·��
                data[pos++] = (byte)(paramConfig.cmbAirsub2Func.SelectedIndex);//���⹦������
                data[pos++] = Convert.ToByte(paramConfig.txtAirCtlDNo2.Text.ToString(), 10);	//�����豸��	        
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
                /*�����豸3*/
                data[pos++] = 0x0;//��·��
                data[pos++] = (byte)(paramConfig.cmbAirsub3Func.SelectedIndex);//���⹦������
                data[pos++] = Convert.ToByte(paramConfig.txtAirCtlDNo3.Text.ToString(), 10);	//�����豸��	        
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
                /*�����豸4*/
                data[pos++] = 0x0;//��·��
                data[pos++] = (byte)(paramConfig.cmbAirsub4Func.SelectedIndex);//���⹦������
                data[pos++] = Convert.ToByte(paramConfig.txtAirCtlDNo4.Text.ToString(), 10);	//�����豸��	        
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
            /* ����ģ��*/
            if (paramConfig.chkSelect.Checked == true)
            {
                data[pos++] = Convert.ToByte(paramConfig.txtKbNo.Text.ToString(), 10);	//�豸���	
                GetGdMeterAddr(paramConfig.txtKbAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//��������
                data[pos++] = (byte)DEVICE_TYPE.MD_KEYBOARD_20;	//�豸����
                data[pos++] = 0x04;//������		
                data[pos++] = 0x14;//�����豸��
                /*��1�����豸1*/
                data[pos++] = 0x1;//��·��
                data[pos++] = (byte)(paramConfig.cmbK1Func.SelectedIndex);//���⹦������
                if (paramConfig.key1CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key1CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��2�����豸*/
                data[pos++] = 0x2;//��·��
                data[pos++] = (byte)(paramConfig.cmbK2Func.SelectedIndex);//���⹦������
                subno = 0;
                if (paramConfig.key2CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key2CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��3�����豸*/
                data[pos++] = 0x3;//��·��
                data[pos++] = (byte)(paramConfig.cmbK3Func.SelectedIndex);//���⹦������
                subno = 0;
                if (paramConfig.key3CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key3CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��4�����豸*/
                data[pos++] = 0x4;//��·��
                data[pos++] = (byte)(paramConfig.cmbK4Func.SelectedIndex);//���⹦������
                if (paramConfig.key4CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key4CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��5�����豸*/
                data[pos++] = 0x5;//��·��
                data[pos++] = (byte)(paramConfig.cmbK5Func.SelectedIndex);//���⹦������
                if (paramConfig.key5CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key5CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��6�����豸*/
                data[pos++] = 0x6;//��·��
                data[pos++] = (byte)(paramConfig.cmbK6Func.SelectedIndex);//���⹦������
                if (paramConfig.key6CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key6CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��7�����豸*/
                data[pos++] = 0x7;//��·��
                data[pos++] = (byte)(paramConfig.cmbK7Func.SelectedIndex);//���⹦������
                if (paramConfig.key7CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key7CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��8�����豸*/
                data[pos++] = 0x8;//��·��
                data[pos++] = (byte)(paramConfig.cmbK8Func.SelectedIndex);//���⹦������
                if (paramConfig.key8CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key8CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��9�����豸*/
                data[pos++] = 0x9;//��·��
                data[pos++] = (byte)(paramConfig.cmbK9Func.SelectedIndex);//���⹦������
                if (paramConfig.key9CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key9CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��10�����豸*/
                data[pos++] = 0xa;//��·��
                data[pos++] = (byte)(paramConfig.cmbK10Func.SelectedIndex);//���⹦������
                if (paramConfig.key10CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key10CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��11�����豸*/
                data[pos++] = 0xb;//��·��
                data[pos++] = (byte)(paramConfig.cmbK11Func.SelectedIndex);//���⹦������
                if (paramConfig.key11CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key11CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��12�����豸*/
                data[pos++] = 0xc;//��·��
                data[pos++] = (byte)(paramConfig.cmbK12Func.SelectedIndex);//���⹦������
                if (paramConfig.key12CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key12CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��13�����豸*/
                data[pos++] = 0xd;//��·��
                data[pos++] = (byte)(paramConfig.cmbK12Func.SelectedIndex);//���⹦������
                if (paramConfig.key13CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key13CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��14�����豸*/
                data[pos++] = 0xe;//��·��
                data[pos++] = (byte)(paramConfig.cmbK14Func.SelectedIndex);//���⹦������
                if (paramConfig.key14CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key14CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��15�����豸*/
                data[pos++] = 0xf;//��·��
                data[pos++] = (byte)(paramConfig.cmbK15Func.SelectedIndex);//���⹦������
                if (paramConfig.key15CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key15CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��16�����豸*/
                data[pos++] = 0x10;//��·��
                data[pos++] = (byte)(paramConfig.cmbK16Func.SelectedIndex);//���⹦������
                if (paramConfig.key16CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key16CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��17�����豸*/
                data[pos++] = 0x11;//��·��
                data[pos++] = (byte)(paramConfig.cmbK17Func.SelectedIndex);//���⹦������
                if (paramConfig.key17CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key17CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��18�����豸*/
                data[pos++] = 0x12;//��·��
                data[pos++] = (byte)(paramConfig.cmbK18Func.SelectedIndex);//���⹦������
                if (paramConfig.key18CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key18CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��19�����豸*/
                data[pos++] = 0x13;//��·��
                data[pos++] = (byte)(paramConfig.cmbK19Func.SelectedIndex);//���⹦������
                if (paramConfig.key19CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key19CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
                /*��20�����豸*/
                data[pos++] = 0x14;//��·��
                data[pos++] = (byte)(paramConfig.cmbK20Func.SelectedIndex);//���⹦������
                if (paramConfig.key20CtlDNo.Text != "")
                {
                    data[pos++] = Convert.ToByte(paramConfig.key20CtlDNo.Text.ToString(), 10);	//�����豸��	        
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
            /* 12·����8·���ģ��*/
            if (paramConfig.chkIORelay.Checked == true)
            {
                byte deviceno = Convert.ToByte(paramConfig.txt8RNo.Text.ToString(), 10);
                data[pos++] = deviceno;	//�豸���	
                GetGdMeterAddr(paramConfig.txt8RelayAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//��������
                data[pos++] = (byte)DEVICE_TYPE.MD_RELAY_8;	//�豸����
                data[pos++] = 0x04;//������		
                data[pos++] = 0xC;//�����豸��
                /* 1·����*/
                data[pos++] = 0x1;//��·��
                data[pos++] = (byte)(paramConfig.cmbIn1Func.SelectedIndex); ;//���⹦������
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
                /* 2·����*/
                data[pos++] = 0x2;//��·��
                data[pos++] = (byte)(paramConfig.cmbIn2Func.SelectedIndex); //���⹦������
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
                /* 3·����*/
                data[pos++] = 0x3;//��·��
                data[pos++] = (byte)(paramConfig.cmbIn3Func.SelectedIndex); ;//���⹦������
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
                /* 4·����*/
                data[pos++] = 0x4;//��·��
                data[pos++] = (byte)(paramConfig.cmbIn4Func.SelectedIndex); ;//���⹦������
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
                /* 5·����*/
                data[pos++] = 0x5;//��·��
                data[pos++] = (byte)(paramConfig.cmbIn5Func.SelectedIndex); ;//���⹦������
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
                /* 6·����*/
                data[pos++] = 0x6;//��·��
                data[pos++] = (byte)(paramConfig.cmbIn6Func.SelectedIndex); ;//���⹦������
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
                /* 7·����*/
                data[pos++] = 0x7;//��·��
                data[pos++] = (byte)(paramConfig.cmbIn7Func.SelectedIndex); ;//���⹦������
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
                /* 8·����*/
                data[pos++] = 0x8;//��·��
                data[pos++] = (byte)(paramConfig.cmbIn8Func.SelectedIndex); ;//���⹦������
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
                /* 9·����*/
                data[pos++] = 0x9;//��·��
                data[pos++] = (byte)(paramConfig.cmbIn9Func.SelectedIndex); ;//���⹦������
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
                /* 10·����*/
                data[pos++] = 0xa;//��·��
                data[pos++] = (byte)(paramConfig.cmbIn10Func.SelectedIndex); ;//���⹦������
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
                /* 11·����*/
                data[pos++] = 0xb;//��·��
                data[pos++] = (byte)(paramConfig.cmbIn11Func.SelectedIndex); ;//���⹦������
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
                /* 12·����*/
                data[pos++] = 0xC;//��·��
                data[pos++] = (byte)(paramConfig.cmbIn12Func.SelectedIndex); ;//���⹦������
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
            /* ������ʾģ��*/
            if (paramConfig.chkDoorDisp.Checked == true)
            {
                byte deviceno = Convert.ToByte(paramConfig.txtDSNo.Text.ToString(), 10);
                data[pos++] = deviceno;	//�豸���	
                GetGdMeterAddr(paramConfig.txtDoorDispAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//��������
                data[pos++] = (byte)DEVICE_TYPE.MD_DOORDISPLAY; 	//�豸����
                data[pos++] = 0x04;//������		
                data[pos++] = 0x0;//�����豸��
            }

            /* 12·����8·���ģ��*/
            if (paramConfig.ORCheck.Checked == true)
            {
                byte deviceno = Convert.ToByte(paramConfig.ORtxt8RNo.Text.ToString(), 10);
                data[pos++] = deviceno;	//�豸���	
                GetGdMeterAddr(paramConfig.ORtxt8RelayAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//��������
                data[pos++] = (byte)DEVICE_TYPE.MD_RELAY_8;	//�豸����
                data[pos++] = 0x04;//������		
                data[pos++] = 0xC;//�����豸��
                /* 1·����*/
                data[pos++] = 0x1;//��·��
                data[pos++] = (byte)(paramConfig.ORcmbIn1Func.SelectedIndex); ;//���⹦������
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
                /* 2·����*/
                data[pos++] = 0x2;//��·��
                data[pos++] = (byte)(paramConfig.ORcmbIn2Func.SelectedIndex); //���⹦������
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
                /* 3·����*/
                data[pos++] = 0x3;//��·��
                data[pos++] = (byte)(paramConfig.ORcmbIn3Func.SelectedIndex); ;//���⹦������
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
                /* 4·����*/
                data[pos++] = 0x4;//��·��
                data[pos++] = (byte)(paramConfig.ORcmbIn4Func.SelectedIndex); ;//���⹦������
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
                /* 5·����*/
                data[pos++] = 0x5;//��·��
                data[pos++] = (byte)(paramConfig.ORcmbIn5Func.SelectedIndex); ;//���⹦������
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
                /* 6·����*/
                data[pos++] = 0x6;//��·��
                data[pos++] = (byte)(paramConfig.ORcmbIn6Func.SelectedIndex); ;//���⹦������
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
                /* 7·����*/
                data[pos++] = 0x7;//��·��
                data[pos++] = (byte)(paramConfig.ORcmbIn7Func.SelectedIndex); ;//���⹦������
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
                /* 8·����*/
                data[pos++] = 0x8;//��·��
                data[pos++] = (byte)(paramConfig.ORcmbIn8Func.SelectedIndex); ;//���⹦������
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
                /* 9·����*/
                data[pos++] = 0x9;//��·��
                data[pos++] = (byte)(paramConfig.ORcmbIn9Func.SelectedIndex); ;//���⹦������
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
                /* 10·����*/
                data[pos++] = 0xa;//��·��
                data[pos++] = (byte)(paramConfig.ORcmbIn10Func.SelectedIndex); ;//���⹦������
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
                /* 11·����*/
                data[pos++] = 0xb;//��·��
                data[pos++] = (byte)(paramConfig.ORcmbIn11Func.SelectedIndex); ;//���⹦������
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
                /* 12·����*/
                data[pos++] = 0xC;//��·��
                data[pos++] = (byte)(paramConfig.ORcmbIn12Func.SelectedIndex); ;//���⹦������
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

            /* 4·����2·���ģ��*/
            if (paramConfig.chk2IOEnable.Checked == true)
            {
                byte deviceno = Convert.ToByte(paramConfig.txt2IONo.Text.ToString(), 10);
                data[pos++] = deviceno;	//�豸���	
                GetGdMeterAddr(paramConfig.txt2IODevAddr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//��������
                data[pos++] = (byte)DEVICE_TYPE.MD_RELAY_2;	//�豸����
                data[pos++] = 0x04;//������		
                data[pos++] = 0x4;//�����豸��
                /* 1·����*/
                data[pos++] = 0x1;//��·��
                data[pos++] = (byte)(paramConfig.cmb2IOFunc1.SelectedIndex); ;//���⹦������
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
                /* 2·����*/
                data[pos++] = 0x2;//��·��
                data[pos++] = (byte)(paramConfig.cmb2IOFunc2.SelectedIndex); //���⹦������
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
                /* 3·����*/
                data[pos++] = 0x3;//��·��
                data[pos++] = (byte)(paramConfig.cmb2IOFunc3.SelectedIndex); //���⹦������
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
                /* 4·����*/
                data[pos++] = 0x4;//��·��
                data[pos++] = (byte)(paramConfig.cmb2IOFunc4.SelectedIndex); //���⹦������
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
            /* ���߲���ģ��*/
            if (paramConfig.chkLine4.Checked != false)
            {
                data[pos++] = Convert.ToByte(paramConfig.txtLine4DevNo.Text.ToString(), 10);	//�豸���	
                GetGdMeterAddr(paramConfig.txtLine4Addr.Text.ToString(), ref deviceAddr);
                for (int i = 0; i < 6; i++)
                {
                    data[pos++] = deviceAddr[i];
                }
                data[pos++] = (byte)DEVICE_KIND.KIND_CONTROL;//��������
                data[pos++] = (byte)DEVICE_TYPE.MD_LIGHT_4;	//�豸����
                data[pos++] = 0x04;//������		
                data[pos++] = 0x00;//�����豸��

            }


            if (SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, pos, ref frame, 15, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                if (rcdata[2] == 0x5D && rcdata[3] == 0x89 && rcdata[4] == 0x00)
                {
                    MessageBox.Show("�����ɹ�");
                }
                else
                {
                    MessageBox.Show("����ʧ��");
                }
            }
            else
            {
                MessageBox.Show("����ʧ��");
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
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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


            //������
            int pos = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //Ȩ������
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //������895D
            data[pos++] = 0x5D;
            data[pos++] = 0x89;

            data[pos++] = 0x02;	//ɾ��
            data[pos++] = 0;//ȫ��ɾ��

            if (SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, pos, ref frame, 30, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                if (rcdata[2] == 0x5D && rcdata[3] == 0x89 && rcdata[4] == 0x00)
                {
                    MessageBox.Show("�����ɹ�");
                }
                else
                {
                    MessageBox.Show("�����ɹ�");
                }
            }
            else
            {
                MessageBox.Show("����ʧ��");
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
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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


            //������
            int pos = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //Ȩ������
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //������0101
            data[pos++] = 0x01;
            data[pos++] = 0x01;
            data[pos++] = Convert.ToByte(frmSet.txtNightEndMin.Text.ToString(), 16);
            data[pos++] = Convert.ToByte(frmSet.txtNightEndHour.Text.ToString(), 16);
            data[pos++] = Convert.ToByte(frmSet.txtNightStartMin.Text.ToString(), 16);
            data[pos++] = Convert.ToByte(frmSet.txtNightStartHour.Text.ToString(), 16);


            //������0102
            data[pos++] = 0x02;
            data[pos++] = 0x01;
            data[pos++] = Convert.ToByte(frmSet.txtLightLeve.Text.ToString(), 16);

            //������0103
            data[pos++] = 0x03;
            data[pos++] = 0x01;
            data[pos++] = Convert.ToByte(frmSet.txtMotorRunTime.Text.ToString(), 16);

            //������0104
            data[pos++] = 0x04;
            data[pos++] = 0x01;
            data[pos++] = (byte)(frmSet.cmbAirSeason.SelectedIndex + 1);


            //������0105
            data[pos++] = 0x05;
            data[pos++] = 0x01;
            data[pos++] = Convert.ToByte(frmSet.txtSumerDegree.Text.ToString(), 16);
            data[pos++] = Convert.ToByte(frmSet.txtWinterDegree.Text.ToString(), 16);

            //������0108
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

            //������0109
            data[pos++] = 0x09;
            data[pos++] = 0x01;
            data[pos++] = Convert.ToByte(frmSet.txtIrdaTime.Text.ToString(), 10);

            if (SendFrame(addr, (byte)GD_AFN_CODE.SetRealtimeParam, data, pos, ref frame, 15, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                MessageBox.Show("�����ɹ�");
                /*if (rcdata[2] == 0x5D && rcdata[3] == 0x89 && rcdata[4] == 0x00)
                {
                    MessageBox.Show("�����ɹ�");    
                }
                else
                {
                    MessageBox.Show("����ʧ��");
                }*/
            }
            else
            {
                MessageBox.Show("����ʧ��");
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
            //    btnQuere.Text = "ֹͣ��ѯ";
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
            //    btnQuere.Text = "������ѯ";
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

            //������
            data[0] = 0x00;
            data[1] = 0x00;
            //Ȩ������
            data[2] = 0x11;
            data[3] = 0x11;
            data[4] = 0x11;
            data[5] = 0x11;
            //������8030
            data[6] = 0x30;
            data[7] = 0x80;
            //���ʱ������
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
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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


            //������
            int pos = 0;
            data[pos++] = 0x01;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;

            //������0106
            data[pos++] = 0x06;
            data[pos++] = 0x01;

            //������0107
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
                            bit0������ť״̬��0��δ���£�1���Ѱ���
                            Bit1�����ſ�����״̬��0��δ�忨��1�Ѳ忨
                            Bit2 ������Ŵż�⣺0�����Źأ�1���ſ�
                            Bit3 �ܵ�Դ����״̬��0���أ�1������
                            Bit4 �ȵƿ���״̬ 0���أ�1������
                            Bit5 ҹ�ƿ���״̬ 0���أ�1������
                            Bit6 �յ�����״̬ 0���ļ���1��������
                            Bit7 �����䰴ť״̬0���أ�1������
                            Bit8 ϴ�·���ť״̬0���أ�1������
                            Bit9 �������״̬0���أ�1������

                            Bit8~31 ������
                         *
                        if ((byte)(rcdata[10] & 0x01) == (byte)0x01)
                        {
                            lvRunInfo.Items[0].SubItems[1].Text = "�Ѱ���";
                        }
                        else
                        {
                            lvRunInfo.Items[0].SubItems[1].Text = "δ����";
                        }

                        if ((byte)(rcdata[10] & 0x02) == (byte)0x02)
                        {
                            lvRunInfo.Items[1].SubItems[1].Text = "�Ѳ忨";
                        }
                        else
                        {
                            lvRunInfo.Items[1].SubItems[1].Text = "δ�忨";
                        }

                        if ((byte)(rcdata[10] & 0x04) == (byte)0x04)
                        {
                            lvRunInfo.Items[2].SubItems[1].Text = "���ſ�";
                        }
                        else
                        {
                            lvRunInfo.Items[2].SubItems[1].Text = "���Ź�";
                        }
                        if ((byte)(rcdata[10] & 0x08) == (byte)0x08)
                        {
                            lvRunInfo.Items[3].SubItems[1].Text = "��";
                        }
                        else
                        {
                            lvRunInfo.Items[3].SubItems[1].Text = "��";
                        }
                        if ((byte)(rcdata[10] & 0x10) == (byte)0x10)
                        {
                            lvRunInfo.Items[4].SubItems[1].Text = "��";
                        }
                        else
                        {
                            lvRunInfo.Items[4].SubItems[1].Text = "��";
                        }
                        if ((byte)(rcdata[10] & 0x20) == (byte)0x20)
                        {
                            lvRunInfo.Items[5].SubItems[1].Text = "��";
                        }
                        else
                        {
                            lvRunInfo.Items[5].SubItems[1].Text = "��";
                        }
                        if ((byte)(rcdata[10] & 0x40) == (byte)0x40)
                        {
                            lvRunInfo.Items[6].SubItems[1].Text = "����";
                        }
                        else
                        {
                            lvRunInfo.Items[6].SubItems[1].Text = "�ļ�";
                        }
                        if ((byte)(rcdata[10] & 0x80) == (byte)0x80)
                        {
                            lvRunInfo.Items[7].SubItems[1].Text = "��";
                        }
                        else
                        {
                            lvRunInfo.Items[7].SubItems[1].Text = "��";
                        }
                        if ((byte)(rcdata[11] & 0x01) == (byte)0x01)
                        {
                            lvRunInfo.Items[8].SubItems[1].Text = "��";
                        }
                        else
                        {
                            lvRunInfo.Items[8].SubItems[1].Text = "��";
                        }
                        if ((byte)(rcdata[11] & 0x02) == (byte)0x02)
                        {
                            lvRunInfo.Items[9].SubItems[1].Text = "��";
                        }
                        else
                        {
                            lvRunInfo.Items[9].SubItems[1].Text = "��";
                        }

                        lvRunInfo.Items[10].SubItems[1].Text = String.Format("{0:D2}", (byte)rcdata[16]) + " ��";
                        lvRunInfo.Items[11].SubItems[1].Text = String.Format("{0:D2}", (byte)rcdata[17]) + " ��";
                        lvRunInfo.Items[12].SubItems[1].Text = String.Format("{0:D2}", (byte)rcdata[18]) + " ��";
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
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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


            //������
            int pos = 0;
            data[pos++] = 0x01;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;

            //������0101
            data[pos++] = 0x01;
            data[pos++] = 0x01;

            //������0102
            data[pos++] = 0x02;
            data[pos++] = 0x01;

            //������0103
            data[pos++] = 0x03;
            data[pos++] = 0x01;

            //������0104
            data[pos++] = 0x04;
            data[pos++] = 0x01;

            //������0105
            data[pos++] = 0x05;
            data[pos++] = 0x01;

            //������0108
            data[pos++] = 0x08;
            data[pos++] = 0x01;
            //������0109
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
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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


            //������
            int pos = 0;
            byte memberNum = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //Ȩ������
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //������0140
            data[pos++] = 0x40;
            data[pos++] = 0x01;

            data[pos++] = memberNum;

            if (powkeySceneSet.PowChk1.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeyCtlDNo1.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(powkeySceneSet.powkeySNO1.Text.ToString(), 10);
                data[pos++] = (byte)(powkeySceneSet.powK1cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK2cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK3cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK4cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK5cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK6cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK7cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK8cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK9cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK10cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK11cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK12cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK13cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK14cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK15cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK16cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK17cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK18cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK19cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(powkeySceneSet.powK20cmbFunc.SelectedIndex); //���⹦������ 

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
                    MessageBox.Show("�����ɹ�");
                }
                else
                {
                    MessageBox.Show("����ʧ��");
                }
            }
            else
            {
                MessageBox.Show("����ʧ��");
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
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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


            //������
            int pos = 0;
            byte memberNum = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //Ȩ������
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //������0142
            data[pos++] = 0x42;
            data[pos++] = 0x01;

            data[pos++] = memberNum;

            if (doorcardSceneSet.DrChk1.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardCtlDNo1.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorcardSceneSet.doorcardSNO1.Text.ToString(), 10);
                data[pos++] = (byte)(doorcardSceneSet.doorcardK1cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK2cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK3cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK4cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK5cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK6cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK7cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK8cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK9cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK10cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK11cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK12cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK13cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK14cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK15cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK16cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK17cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK18cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK19cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardK20cmbFunc.SelectedIndex); //���⹦������ 

                UInt32 dataArea = Convert.ToUInt32(doorcardSceneSet.doorcardyParam20.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }
            data[8] = memberNum;

            //������0143
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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK1cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK2cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK3cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK4cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK5cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK6cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK7cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK8cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK9cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK10cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK11cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK12cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK13cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK14cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK15cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK16cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK17cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK18cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK19cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorcardSceneSet.doorcardOutK20cmbFunc.SelectedIndex); //���⹦������ 

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
                    MessageBox.Show("�����ɹ�");
                }
                else
                {
                    MessageBox.Show("����ʧ��");
                }
            }
            else
            {
                MessageBox.Show("����ʧ��");
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
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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


            //������
            int pos = 0;
            int memberNumPos = 0;
            byte memberNum = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //Ȩ������
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //������0111
            data[pos++] = 0x11;
            data[pos++] = 0x01;
            memberNumPos = pos;
            data[pos++] = memberNum;

            if (sceneSet.Scene1Chk1.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(sceneSet.scene1CtlDNo1.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(sceneSet.scene1SNO1.Text.ToString(), 10);
                data[pos++] = (byte)(sceneSet.scene1K1cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(sceneSet.scene1K2cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(sceneSet.scene1K3cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(sceneSet.scene1K4cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(sceneSet.scene1K5cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(sceneSet.scene1K6cmbFunc.SelectedIndex); //���⹦������ 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene1Param6.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }



            data[memberNumPos] = memberNum;


            /* ��������������*/
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
                data[pos++] = (byte)(sceneSet.scene2K1cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(sceneSet.scene2K2cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(sceneSet.scene2K3cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(sceneSet.scene2K4cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(sceneSet.scene2K5cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(sceneSet.scene2K6cmbFunc.SelectedIndex); //���⹦������ 

                UInt32 dataArea = Convert.ToUInt32(sceneSet.scene2Param6.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }


            data[memberNumPos] = memberNum;

            /* ��������������*/
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
                data[pos++] = (byte)(sceneSet.scene3K1cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(sceneSet.scene3K2cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(sceneSet.scene3K3cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(sceneSet.scene3K4cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(sceneSet.scene3K5cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(sceneSet.scene3K6cmbFunc.SelectedIndex); //���⹦������ 

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
                    MessageBox.Show("�����ɹ�");
                }
                else
                {
                    MessageBox.Show("����ʧ��");
                }
            }
            else
            {
                MessageBox.Show("����ʧ��");
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
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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


            //������
            int pos = 0;
            byte memberNum = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //Ȩ������
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //������0140
            data[pos++] = 0x01;
            data[pos++] = 0x02;

            data[pos++] = memberNum;

            memberNum++;
            data[pos++] = Convert.ToByte(txtCtlDNo.Text.ToString(), 10);
            data[pos++] = Convert.ToByte(txtSubno.Text.ToString(), 10);
            data[pos++] = (byte)(cmbFunc.SelectedIndex); //���⹦������ 

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
                    MessageBox.Show("�����ɹ�");
                }
                else
                {
                    MessageBox.Show("����ʧ��");
                }
            }
            else
            {
                MessageBox.Show("����ʧ��");
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
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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


            //������
            int pos = 0;
            byte memberNum = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //Ȩ������
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //������0144
            data[pos++] = 0x44;
            data[pos++] = 0x01;

            data[pos++] = memberNum;

            if (doorSceneSet.DSChk1.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(doorSceneSet.DSCtlDNo1.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(doorSceneSet.DSSNO1.Text.ToString(), 10);
                data[pos++] = (byte)(doorSceneSet.DSK1cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorSceneSet.DSK2cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorSceneSet.DSK3cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(doorSceneSet.DSK4cmbFunc.SelectedIndex); //���⹦������ 

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
                    MessageBox.Show("�����ɹ�");
                }
                else
                {
                    MessageBox.Show("����ʧ��");
                }
            }
            else
            {
                MessageBox.Show("����ʧ��");
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
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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


            //������
            int pos = 0;
            byte memberNum = 0;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            //Ȩ������
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            data[pos++] = 0x11;
            //������0145
            data[pos++] = 0x45;
            data[pos++] = 0x01;

            data[pos++] = memberNum;

            if (irdtSceneSet.IrdtInChk1.Checked == true)
            {
                memberNum++;
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInCtlDNo1.Text.ToString(), 10);
                data[pos++] = Convert.ToByte(irdtSceneSet.IrdtInSNO1.Text.ToString(), 10);
                data[pos++] = (byte)(irdtSceneSet.IrdtInK1cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtInK2cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtInK3cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtInK4cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtInK5cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtInK6cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtInK7cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtInK8cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtInK9cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtInK10cmbFunc.SelectedIndex); //���⹦������ 

                UInt32 dataArea = Convert.ToUInt32(irdtSceneSet.IrdtInParam10.Text.ToString(), 10);
                data[pos++] = (byte)(dataArea & 0xff);
                data[pos++] = (byte)((dataArea >> 8) & 0xff);
                data[pos++] = (byte)((dataArea >> 16) & 0xff);
                data[pos++] = (byte)((dataArea >> 24) & 0xff);
            }

            data[8] = memberNum;

            //������0146
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
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK1cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK2cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK3cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK4cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK5cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK6cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK7cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK8cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK9cmbFunc.SelectedIndex); //���⹦������ 

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
                data[pos++] = (byte)(irdtSceneSet.IrdtOutK10cmbFunc.SelectedIndex); //���⹦������ 

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
                    MessageBox.Show("�����ɹ�");
                }
                else
                {
                    MessageBox.Show("����ʧ��");
                }
            }
            else
            {
                MessageBox.Show("����ʧ��");
            }
            ClosePort();
        }

        byte lvLinePos;
        void PraseRcuState(ref byte[] rcdata)
        {
            /*
                ��һ�ֽڣ�
                bit0������ť״̬��0��δ���£�1���Ѱ���
                Bit1�����ſ�����״̬��0��δ�忨��1�Ѳ忨
                Bit2 ������Ŵż�⣺0�����Źأ�1���ſ�
                Bit3 �ܵ�Դ����״̬��0���أ�1������
                Bit4 �����䰴ť״̬0���أ�1������
                Bit5 ϴ�·���ť״̬0���أ�1������
                Bit6 �������״̬0���أ�1������
                Bit7 ���Ժ�״̬0���أ�1������
            */
            /* lvRcuStateInfo�ؼ���ʼ��*/
            { ListViewItem item = new ListViewItem("������ť״̬"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x01) == (byte)0x01)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "�Ѱ���";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "δ����";
            }

            { ListViewItem item = new ListViewItem("�����ſ�����״̬"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x02) == (byte)0x02)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "�Ѳ忨";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "δ�忨";
            }

            { ListViewItem item = new ListViewItem("������Ŵż��"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x04) == (byte)0x04)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }

            { ListViewItem item = new ListViewItem("�ܵ�Դ����״̬"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x08) == (byte)0x08)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }

            { ListViewItem item = new ListViewItem("�����䰴ť״̬"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x10) == (byte)0x10)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }

            { ListViewItem item = new ListViewItem("ϴ�·���ť״̬"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x20) == (byte)0x20)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }

            { ListViewItem item = new ListViewItem("�������״̬"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x40) == (byte)0x40)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }

            { ListViewItem item = new ListViewItem("���Ժ�״̬"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0] & 0x80) == (byte)0x80)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }
            /*
             * �ڶ��ֽڣ�
            bit0�����⣺0�����ˣ�1������
            Bit1�������0��������1�������
            Bit2 ��̨�Ŵţ�0����̨�Źأ�1��̨�ſ�
            Bit3 ҹ����0���أ�1������
            Bit4 ������0���أ�1������
            Bit5 �����˷�0���أ�1������
            Bit6 ���� 0���أ�1������
            Bit7 ά����0���أ�1������
            */
            { ListViewItem item = new ListViewItem("������"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x01) == (byte)0x01)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "����";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "����";
            }

            { ListViewItem item = new ListViewItem("�������"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x02) == (byte)0x02)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "�������";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "������";
            }

            { ListViewItem item = new ListViewItem("��̨�Ŵ�"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x04) == (byte)0x04)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��̨�ſ�";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��̨�Ź�";
            }

            { ListViewItem item = new ListViewItem("ҹ��"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x08) == (byte)0x08)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }

            { ListViewItem item = new ListViewItem("������"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x10) == (byte)0x10)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }

            { ListViewItem item = new ListViewItem("�����˷�"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x20) == (byte)0x20)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }

            { ListViewItem item = new ListViewItem("����"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x40) == (byte)0x40)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }

            { ListViewItem item = new ListViewItem("ά����"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1] & 0x80) == (byte)0x80)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
            }
        }

        void PraseAirconditionState(ref byte[] rcdata)
        {
            { ListViewItem item = new ListViewItem("�յ�����ģʽ"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[0]) == (byte)0x01)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "����";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "�ļ�";
            }

            { ListViewItem item = new ListViewItem("�յ�����ģʽ"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            if ((byte)(rcdata[1]) == (byte)0x01)
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "����";
            }
            else
            {
                rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "����";
            }

            { ListViewItem item = new ListViewItem("�յ������¶�"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = String.Format("{0:D2}", (byte)rcdata[2]) + " ��";

            { ListViewItem item = new ListViewItem("�����¶�"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = String.Format("{0:D2}", (byte)rcdata[3]) + " ��";

            { ListViewItem item = new ListViewItem("�յ�����"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
            rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = String.Format("{0:D2}", (byte)rcdata[4]) + " ��";

        }
        void PraseRelay8State(ref byte[] rcdata)
        {
            BitArray bitarray = new BitArray(rcdata);

            for (int i = 13; i <= 20; i++)
            {
                { ListViewItem item = new ListViewItem("��" + i.ToString() + "·�̵���"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
                if (bitarray[i] == false)
                {
                    rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
                }
                else
                {
                    rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
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
                { ListViewItem item = new ListViewItem("��" + i.ToString() + "·�̵���"); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
                if (bitarray[i] == false)
                {
                    rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
                }
                else
                {
                    rcuStateQuq.lvRcuStateInfo.Items[lvLinePos++].SubItems[1].Text = "��";
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
                        MessageBox.Show("��������ʧ�ܣ��˳�����");
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

            /* ��ʼ���б�*/
            lvLinePos = 0;
            rcuStateQuq.lvRcuStateInfo.Items.Clear();

            //������
            int pos = 0;
            data[pos++] = 0x01;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;
            data[pos++] = 0x00;

            //������0202
            data[pos++] = 0x02;
            data[pos++] = 0x02;

            if (SendFrame(addr, (byte)GD_AFN_CODE.GetRealtimeParam, data, pos, ref frame, 15, 3) == REC_RESULT.OK)
            {
                byte[] rcdata = new byte[frame.dataArray.Count];
                frame.dataArray.CopyTo(rcdata);
                if (frame.dataArray.Count > 9 && rcdata[8] == 0x02 && rcdata[9] == 0x02)
                {
                    byte deviceNum;
                    deviceNum = rcdata[10];//�豸��
                    byte[] tempdata = new byte[8];

                    /* �ֽڳ��ȼ��� */

                    /* ģ�����ݽ���*/
                    for (int i = 0; i < deviceNum; i++)
                    {
                        switch (rcdata[12 + i * 10])
                        {
                            case (byte)DEVICE_TYPE.MD_RCU:
                                { ListViewItem item = new ListViewItem("<RCU����ģ��״̬>" + "�豸���:" + String.Format("{0:D2}", (byte)rcdata[11 + i * 10])); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
                                lvLinePos++;
                                System.Array.Copy(rcdata, 12 + i * 10 + 1, tempdata, 0, 8);
                                PraseRcuState(ref tempdata);
                                break;

                            case (byte)DEVICE_TYPE.MD_AIRCONDITION:
                                { ListViewItem item = new ListViewItem("<�յ�ģ��״̬>" + "�豸���:" + String.Format("{0:D2}", (byte)rcdata[11 + i * 10])); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
                                lvLinePos++;
                                System.Array.Copy(rcdata, 12 + i * 10 + 1, tempdata, 0, 8);
                                PraseAirconditionState(ref tempdata);
                                break;

                            case (byte)DEVICE_TYPE.MD_RELAY_8:
                                { ListViewItem item = new ListViewItem("<8·�̵���ģ��״̬>" + "�豸���:" + String.Format("{0:D2}", (byte)rcdata[11 + i * 10])); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
                                lvLinePos++;
                                System.Array.Copy(rcdata, 12 + i * 10 + 1, tempdata, 0, 8);
                                PraseRelay8State(ref tempdata);
                                break;

                            case (byte)DEVICE_TYPE.MD_LIGHT_4:
                                { ListViewItem item = new ListViewItem("<4·����ģ��״̬>" + "�豸���:" + String.Format("{0:D2}", (byte)rcdata[11 + i * 10])); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
                                lvLinePos++;
                                System.Array.Copy(rcdata, 12 + i * 10 + 1, tempdata, 0, 8);
                                PraseLight4State(ref tempdata);
                                break;

                            case (byte)DEVICE_TYPE.MD_RELAY_2:
                                { ListViewItem item = new ListViewItem("<2·�̵���ģ��״̬>" + "�豸���:" + String.Format("{0:D2}", (byte)rcdata[11 + i * 10])); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
                                lvLinePos++;
                                System.Array.Copy(rcdata, 12 + i * 10 + 1, tempdata, 0, 8);
                                PraseRelay2State(ref tempdata);
                                break;

                            case (byte)DEVICE_TYPE.MD_LEDV12_3:
                                { ListViewItem item = new ListViewItem("<12VLED����ģ��״̬>" + "�豸���:" + String.Format("{0:D2}", (byte)rcdata[11 + i * 10])); item.SubItems.Add(""); rcuStateQuq.lvRcuStateInfo.Items.Add(item); }
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
                MessageBox.Show("���óɹ�");
                return;
            }
            else
            {
                MessageBox.Show("����ʧ��");
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
                    MessageBox.Show("��ص�ѹ�ο�Դֵ����Ϊ��", "��ʾ");
                    return;
                }
                dfSrcBatVol = Convert.ToDouble(this.txtSrcBatVol.Text.ToString());
                if (dfSrcBatVol < 10.0 || dfSrcBatVol > 800.0)
                {
                    MessageBox.Show("��ص�ѹ�ο�Դֵ������Ч��Χ10.0~800.0��", "��ʾ");
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
                    MessageBox.Show("ĸ�ߵ�ѹ�ο�Դֵ����Ϊ��", "��ʾ");
                    return;
                }
                dfSrcDcVol = Convert.ToDouble(this.txtSrcDcVol.Text.ToString());
                if (dfSrcDcVol < 10.0 || dfSrcDcVol > 800.0)
                {
                    MessageBox.Show("ĸ�ߵ�ѹ�ο�Դֵ������Ч��Χ10.0~800.0��", "��ʾ");
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
                    MessageBox.Show("��ĸ�߾�Ե����ο�Դֵ����Ϊ��", "��ʾ");
                    return;
                }
                dfSrcPositiveR = Convert.ToDouble(this.txtSrcPositiveR.Text.ToString());
                if (dfSrcPositiveR < 0.0 || dfSrcPositiveR >999.0)
                {
                    MessageBox.Show("��ĸ�߾�Ե����ο�Դֵ������Ч��Χ0~999ǧŷ", "��ʾ");
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
                    MessageBox.Show("��ĸ�߾�Ե����ο�Դֵ����Ϊ��", "��ʾ");
                    return;
                }
                dfSrcNegativeR = Convert.ToDouble(this.txtSrcNegativeR.Text.ToString());
                if (dfSrcNegativeR < 0.0 || dfSrcNegativeR > 999.0)
                {
                    MessageBox.Show("��ĸ�߾�Ե����ο�Դֵ������Ч��Χ0~999ǧŷ", "��ʾ");
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
                    MessageBox.Show("CC1����ȷ�ϵ�ѹ�ο�Դֵ����Ϊ��", "��ʾ");
                    return;
                }
                dfSrcCC1Vol = Convert.ToDouble(this.txtSrcCC1Vol.Text.ToString());
                if (dfSrcCC1Vol < 0.0 || dfSrcCC1Vol > 13.0)
                {
                    MessageBox.Show("CC1����ȷ�ϵ�ѹ�ο�Դֵ������Ч��Χ0~13.0��", "��ʾ");
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
                    MessageBox.Show("�¶Ȳο�Դֵ����Ϊ��", "��ʾ");
                    return;
                }
                dfSrcTemperature = Convert.ToDouble(this.txtSrcTemperature.Text.ToString());
                if (dfSrcTemperature < -50.0 || dfSrcTemperature > 200.0)
                {
                    MessageBox.Show("�¶Ȳο�Դֵ������Ч��Χ-50~200.0��", "��ʾ");
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
                    MessageBox.Show("�¶�2�ο�Դֵ����Ϊ��", "��ʾ");
                    return;
                }
                dfSrcTemperature2 = Convert.ToDouble(this.txtSrcTemperature2.Text.ToString());
                if (dfSrcTemperature2 < -50.0 || dfSrcTemperature2 > 200.0)
                {
                    MessageBox.Show("�¶�2�ο�Դֵ������Ч��Χ0~100.0��", "��ʾ");
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
                errStr += "��ص�ѹϵ������Ϊ��";
            }

            if (txtRatioDcVol.Text == "")
            {
               errStr += "��ص�ѹϵ������Ϊ��";
            }
            if (txtRatioPositiveR.Text == "")
            {
                errStr += "���Եؾ�Ե����ϵ������Ϊ��";
            }
            if (txtRatioNegativeR.Text == "")
            {
                errStr += "���Եؾ�Ե����ϵ������Ϊ��";
            }
            if (txtRatioCC1Vol.Text == "")
            {
                errStr += "CC1��ѹϵ������Ϊ��";
            }
        
            if (txtRatioTemperature.Text == "")
            {
                errStr += "�¶�1ϵ������Ϊ��";
            }
            if (txtRatioTemperature2.Text == "")
            {
                errStr += "�¶�2ϵ������Ϊ��";
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
                MessageBox.Show("У׼�������óɹ�", "��ʾ");
                CloseInsulateDetect();
                return;
            }
            else
            {
                MessageBox.Show("У׼��������ʧ��", "��ʾ");
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

            //�򿪾�Ե���
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
                    MessageBox.Show("����У׼����ʧ��", "��ʾ");
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

            //�򿪾�Ե���
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
                    MessageBox.Show("����У׼����ʧ��", "��ʾ");
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

            //�رվ�Ե���
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
                    MessageBox.Show("��Ե���ر�ʧ��", "��ʾ");
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
                    MessageBox.Show("��ȡ��Ե��������ʧ��", "��ʾ");
                    return false;
                }
            }
                Int16 id;
                Int16 dlval;
                int index = 0;

                //��ص�ѹ���Ŵ�ϵ��10
                dlval = Convert.ToInt16((byte)(frame.dataArray[15]) | (byte)(frame.dataArray[16]) << 8); 
                double flBatVol = dlval / 10.0;

                ////����ĸ�ߵ�ѹ*10
                dlval = Convert.ToInt16((byte)(frame.dataArray[0]) | (byte)(frame.dataArray[1]) << 8); 
                double flMastVol = dlval / 10.0;

                //���Եص��� ǧŷ
                Int16 ToGndRf = Convert.ToInt16((byte)(frame.dataArray[17]) | (byte)(frame.dataArray[18]) << 8);

                //���Եص��� ǧŷ
                Int16 NegGndRf = Convert.ToInt16((byte)(frame.dataArray[19]) | (byte)(frame.dataArray[20]) << 8); 

                //CC1����ȷ�ϵ�ѹ *100
                dlval = Convert.ToInt16((byte)(frame.dataArray[13]) | (byte)(frame.dataArray[14]) << 8);
                double flCC1Vol = dlval / 100.0;
                //�¶�1 
                dlval = Convert.ToInt16((byte)(frame.dataArray[9]) | (byte)(frame.dataArray[10]) << 8);
                double flTemperatureVol = dlval - 50;
                //�¶�2  
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
                MessageBox.Show("��ȡ�ɹ�");
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
            //btmPrt.Text = "��Ե������ݶ�ȡ��ɣ���Ե����ѹر�";
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
                    MessageBox.Show("��ȡʵʱ����ʧ��", "��ʾ");
                    return false;
                }
            }
                Int16 id;
                Int32 dlval;
                int index=0;

                //��ص�ѹ���Ŵ�ϵ��10
                dlval = Convert.ToInt16((byte)(frame.dataArray[0]) | (byte)(frame.dataArray[1]) << 8);
                double flBatVol = dlval / 10.0;

                ////����ĸ�ߵ�ѹ*10
                dlval = Convert.ToInt16((byte)(frame.dataArray[15]) | (byte)(frame.dataArray[16]) << 8);
                double flMastVol = dlval / 10.0;

                //���Եص��� ǧŷ
                UInt16 ToGndRf = Convert.ToUInt16((byte)(frame.dataArray[17]) | (byte)(frame.dataArray[18]) << 8);
       
                //���Եص��� ǧŷ
                UInt16 NegGndRf = Convert.ToUInt16((byte)(frame.dataArray[19]) | (byte)(frame.dataArray[20]) << 8);

                //CC1����ȷ�ϵ�ѹ *100
                dlval = Convert.ToInt16((byte)(frame.dataArray[13]) | (byte)(frame.dataArray[14]) << 8);
                double flCC1Vol = dlval / 100.0;
                //�¶�1 
                dlval = Convert.ToInt16((byte)(frame.dataArray[11]));
                double flTemperatureVol = dlval - 50;
                //�¶�2  
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
                MessageBox.Show("��ȡ�ɹ�","��ʾ");
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

                //��ص�ѹ���Ŵ�ϵ��1000
                dlval = Convert.ToInt16((byte)(frame.dataArray[index]) | (byte)(frame.dataArray[index + 1]) << 8); index += 2;
                double flBatRate = dlval / 1000.0;

                ////����ĸ�ߵ�ѹ*1000
                dlval = Convert.ToInt16((byte)(frame.dataArray[index]) | (byte)(frame.dataArray[index + 1]) << 8); index += 2;
                double flMastRate = dlval / 1000.0;

                //���Եص��� ǧŷ
                dlval = Convert.ToInt16((byte)(frame.dataArray[index]) | (byte)(frame.dataArray[index + 1]) << 8); index += 2;
                double flToGndRfRate = dlval / 1000.0;
                //���Եص��� ǧŷ
                dlval = Convert.ToInt16((byte)(frame.dataArray[index]) | (byte)(frame.dataArray[index + 1]) << 8); index += 2;
                double flNegRfRate = dlval / 1000.0;
                //CC1����ȷ�ϵ�ѹ *1000
                dlval = Convert.ToInt16((byte)(frame.dataArray[index]) | (byte)(frame.dataArray[index + 1]) << 8); index += 2;
                double flCC1Rate = dlval / 1000.0;

                ////�¶�1 *1000 
                dlval = Convert.ToInt16((byte)(frame.dataArray[index]) | (byte)(frame.dataArray[index + 1]) << 8); index += 2;
                double flTemperatureRate = dlval / 1000.0;

                ////�¶�2 *1000 
                dlval = Convert.ToInt16((byte)(frame.dataArray[index]) | (byte)(frame.dataArray[index + 1]) << 8); index += 2;
                double flTemperatureRate2 = dlval / 1000.0;

                txtRatioBatVol.Text = flBatRate.ToString();
                txtRatioDcVol.Text = flMastRate.ToString();
                txtRatioPositiveR.Text = flToGndRfRate.ToString();
                txtRatioNegativeR.Text = flNegRfRate.ToString();
                txtRatioCC1Vol.Text = flCC1Rate.ToString();
                txtRatioTemperature.Text = flTemperatureRate.ToString();
                txtRatioTemperature2.Text = flTemperatureRate2.ToString();
                MessageBox.Show("��ȡ�ɹ�", "��ʾ");
                CloseInsulateDetect();
                return;
            }
            else
            {
                MessageBox.Show("��ȡʧ��", "��ʾ");
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
                MessageBox.Show("��ȡ�ɹ� Read success");
                //return;
            }
            else
            {
                MessageBox.Show("��ȡʧ�� Read failure");
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
            lab.Text = "�����У���ȴ�...";
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

                MessageBox.Show("�˳��Զ�����ʧ��", "��ʾ");
                return ;

            }

            Int16 id;
            Int32 dlval;
            int index = 0;

            //����¶�
            dlval = Convert.ToInt16((byte)(frame.dataArray[0]) | (byte)(frame.dataArray[1]) << 8);
            double flTemp = (dlval-2731) / 10.0;

            //��ص�ѹ
            dlval = Convert.ToInt16((byte)(frame.dataArray[2]) | (byte)(frame.dataArray[3]) << 8);
            double flBatVol = dlval*10.0;

            //����
            dlval = Convert.ToInt16((byte)(frame.dataArray[4]) | (byte)(frame.dataArray[5]) << 8);
            double flCur = dlval * 10.0;

            //SOC 
            dlval = Convert.ToByte(frame.dataArray[6]);
            double flSoc = dlval * 1.0;

            //ʣ������
            dlval = Convert.ToInt16((byte)(frame.dataArray[7]) | (byte)(frame.dataArray[8]) << 8);
            double flRemainCap = dlval * 1.0;

            //����
            string protext;
            dlval = Convert.ToInt16((byte)(frame.dataArray[9]) | (byte)(frame.dataArray[10]) << 8);
            Int16 protect = (Int16)dlval;

            //���ŵ����
            dlval = Convert.ToInt16((byte)(frame.dataArray[11]) | (byte)(frame.dataArray[12]) << 8);
            double flMaxDsgCur = dlval * 10.0;

            //��������
            dlval = Convert.ToInt16((byte)(frame.dataArray[13]) | (byte)(frame.dataArray[14]) << 8);
            double flMaxChgCur = dlval * 10.0;

            //�������¶�
            dlval = Convert.ToInt16((byte)(frame.dataArray[15]) | (byte)(frame.dataArray[16]) << 8);
            double flMaxTemp = (dlval - 2731) / 10.0;

            //�������¶�
            dlval = Convert.ToInt16((byte)(frame.dataArray[17]) | (byte)(frame.dataArray[18]) << 8);
            double flMinTemp = (dlval - 2731) / 10.0;

            //�����ߵ�ѹ
            dlval = Convert.ToInt16((byte)(frame.dataArray[19]) | (byte)(frame.dataArray[20]) << 8);
            double flBatMaxVol = dlval * 10.0;

            //�����͵�ѹ
            dlval = Convert.ToInt16((byte)(frame.dataArray[21]) | (byte)(frame.dataArray[22]) << 8);
            double flBatMinVol = dlval * 10.0;

            //���ѭ������
            dlval = Convert.ToInt32((byte)(frame.dataArray[23]) | (byte)(frame.dataArray[24]) << 8);
            Int32 flBatCycle = dlval;

            //��طŵ�SOC�ۼ�ֵ
            dlval = Convert.ToInt32((byte)(frame.dataArray[25]) | (byte)(frame.dataArray[26]) << 8| (byte)(frame.dataArray[27]) << 16| (byte)(frame.dataArray[28]) << 24);
            Int32 flBatDsgSocSum = dlval;

              

            testlv.Items[0].SubItems[1].Text = flTemp.ToString()+"��";
            testlv.Items[1].SubItems[1].Text = flBatVol.ToString()+"����";
            testlv.Items[2].SubItems[1].Text = flCur.ToString()+"����";
            testlv.Items[3].SubItems[1].Text = flSoc.ToString()+"%";
            testlv.Items[4].SubItems[1].Text = flRemainCap.ToString()+"mAh";
            testlv.Items[5].SubItems[1].Text = protect.ToString();


            lvView2.Items[0].SubItems[1].Text = flMaxDsgCur.ToString() + "����";
            lvView2.Items[1].SubItems[1].Text = flMaxChgCur.ToString() + "����";
            lvView2.Items[2].SubItems[1].Text = flMaxTemp.ToString() + "��";
            lvView2.Items[3].SubItems[1].Text = flMinTemp.ToString() + "��";
            lvView2.Items[4].SubItems[1].Text = flBatMaxVol.ToString() + "����";
            lvView2.Items[5].SubItems[1].Text = flBatMinVol.ToString() + "����";
            lvView2.Items[6].SubItems[1].Text = flBatCycle.ToString();
            lvView2.Items[7].SubItems[1].Text = flBatDsgSocSum.ToString();

//            MessageBox.Show("�Զ����Խ���", "��ʾ");
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

                MessageBox.Show("�˳��Զ�����ʧ��", "��ʾ");
                return;

            }
            MessageBox.Show("eeprom��������ɹ�", "��ʾ");
        }

        public byte[] updateFileBuf = new byte[256 * 1024];//�����������bin�ļ�Ϊ 256 K
        public int updateFileLen;
        public UInt16 updateFileCrc;
        public UInt16 reqBlockNo;
        public short BlockLen;
        public short lastBlockLen;
        public short firmwareBlockNum;
/**************** ������������  ****************************/
        unsafe  bool RequestUpdate()
        {
            txtOutput.Text += "\r\nsend update request command��";
            WriteMessage("\r\nsend update request command��");
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
                txtOutput.Text += "\r\nUpdate request fail��";
                WriteMessage("\r\nUpdate request fail��");
                return false;

            }
            if (frame.ctrl != 0x11)
            {
                txtOutput.Text += "\r\nUpdate request fail��";
                WriteMessage("\r\nUpdate request fail��");
            }
            else
            {
                txtOutput.Text += "\r\nUpdate request sucessfully��";
                WriteMessage("\r\nUpdate request sucessfully��");
            }


            return true;
        }
/**************** �����̼���Ϣ����  ****************************/
        private bool FirmwareInformation()
        {
            txtOutput.Text += "\r\nsend firmware information��";
            WriteMessage("\r\nsend firmware information��");
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
            /* ������Ϣ */
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
            /* Ӳ���汾�� */
            data[pos++] = 0X01;
            /* �ͻ���� */
            data[pos++] = 0X0A;

            /* �̼����� */
            data[pos++] = 0X04;
            /* �汾�� */
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

            /* �̼����� */
            data[pos++] = (byte)((updateFileLen>>24)&0xff);
            data[pos++] = (byte)((updateFileLen>>16)&0xff);
            data[pos++] = (byte)((updateFileLen>>8)&0xff);
            data[pos++] = (byte)((updateFileLen&0xff));
            /* CRCУ�� */
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
            //�ض�ȷ�����������ת��APP ok
            if ((byte)frame.dataArray[0] == 0x02)
            {
                txtOutput.Text += "\r\n<<<<<<               firmware update completed                   >>>>>>>>>>>>>>>>>>>>>>>>";
                WriteMessage("\r\n<<<<<<               firmware update completed                   >>>>>>>>>>>>>>>>>>>>>>>>");
                txtOutput.Text += "��firmware version��" + (byte)frame.dataArray[4] + (byte)frame.dataArray[5] + (byte)frame.dataArray[6] + " ��";
                return true;
            }


            txtOutput.Text += "\r\nfirmware information request sucessfully";
            WriteMessage("\r\nfirmware information request sucessfully");
            switch ((byte)frame.dataArray[0])
            {
                case 0x00:
                    txtOutput.Text += "��no need update��";
                    WriteMessage("��no need update��");
                    break;

                case 0x01:
                    txtOutput.Text += "��block erasing��";
                    WriteMessage("��block erasing��");
                    break;
                
                case 0x02:
                    txtOutput.Text += "��update completed��";
                    WriteMessage("��update completed��");
                    break;

                case 0x03:
                    txtOutput.Text += "��update fault,app can't run ��";
                    WriteMessage("��update fault,app can't run ��");
                    break;

                case 0x04:
                    txtOutput.Text += "��block crc error��";
                    WriteMessage("��block crc error��");
                    break;

                case 0x05:
                    txtOutput.Text += "��block erase fail��";
                    WriteMessage("��block erase fail��");
                    break;
                
                case 0x06:
                    txtOutput.Text += "��block requesting��";
                    WriteMessage("��block requesting��");
                    break;
            }

            switch ((byte)frame.dataArray[1])
            {
                case 0x01:
                    txtOutput.Text += "��Block size 64 Bytes��";
                    WriteMessage("��Block size 64 Bytes��");
                    BlockLen = 64;
                    break;

                case 0x02:
                    txtOutput.Text += "��Block size 128 Bytes��";
                    WriteMessage("��Block size 128 Bytes��");
                    BlockLen = 128;
                    break;

                case 0x03:
                    txtOutput.Text += "��Block size 256 Bytes��";
                    WriteMessage("��Block size 256 Bytes��");
                    BlockLen = 240;
                    break;

                case 0x04:
                    txtOutput.Text += "��Block size 512 Bytes��";
                    WriteMessage("��Block size 512 Bytes��");
                    BlockLen = 512;
                    break;

                case 0x05:
                    txtOutput.Text += "��Block size 1024 Bytes��";
                    WriteMessage("��Block size 1024 Bytes��");
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
            txtOutput.Text += "��total blocks num��" + firmwareBlockNum.ToString() + "��" + "��request block number��" + reqBlockNo.ToString() + "��";
            WriteMessage("��total blocks num��" + firmwareBlockNum.ToString() + "��" + "��request block number��" + reqBlockNo.ToString() + "��");
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
            
            /* �汾�� */
            package[packagelen++] = ver[0];
            package[packagelen++] = ver[1];
            package[packagelen++] = ver[2];
            /* �̼����� */
            package[packagelen++] = 0X04;
            /* ���ݿ��� */
            package[packagelen++] = blockno[0];
            package[packagelen++] = blockno[1];
            /* �̼������ݿ��� */
            package[packagelen++] = FirmwareBlockNum[0];
            package[packagelen++] = FirmwareBlockNum[1];
            /* �̼����ݿ�У�� */
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
/**************** ������������  ****************************/
        private bool BlockSending()
        {
            txtOutput.Text += "\r\nsend firmware block information��";
            WriteMessage("\r\nsend firmware block information��");
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
            /* ׼���̼��汾�� */
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
                        txtOutput.Text += "\r\nfirmware block request fail��";
                        WriteMessage("\r\nfirmware block request fail��");
                        return false;
                    }

                    if ((byte)frame.dataArray[0] == 0x00)
                    {
                        reqBlockNo = (UInt16)(((byte)frame.dataArray[4] << 8) | (byte)frame.dataArray[5]);
                        txtOutput.Text += "\r\nfirmware block receive successfully����total blocks num = "+ firmwareBlockNum.ToString()+"��"+"��request next BlockNo=" + reqBlockNo.ToString()+">>";
                        WriteMessage("\r\nfirmware block receive successfully����total blocks num = " + firmwareBlockNum.ToString() + "��" + "��request next BlockNo=" + reqBlockNo.ToString() + ">>");
                    }
                    else
                    {
                        txtOutput.Text += "\r\nfirmware block receive fail ";
                        WriteMessage("\r\nfirmware block receive fail ");
                        return false;
                    }
                }
                //WriteMessage(this.txtOutput.Text);
                this.txtOutput.Focus();//��ȡ����
                this.txtOutput.Select(this.txtOutput.TextLength, 0);//��궨λ���ı����
                this.txtOutput.ScrollToCaret();//��������괦   
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
  #region//����
         private void OnUpdateFirmware()
         {
            if (RequestUpdate() == false)
             {
                 updateflag = false;
                 return;
             }

            System.Threading.Thread.Sleep(5 * 1000);//unit��1s

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
             System.Threading.Thread.Sleep(3 * 1000);//unit��1s

            if (FirmwareInformation() == false)
             {
                 updateflag = false;
                 return;
             }
        //*/
            MessageBox.Show(" �������" + '\n' + "App update succeed", "Succeed");
            updateflag = false;

         }
        #endregion
        private void UpdateFirmware(object sender, EventArgs e)
        {
            if (updateflag == false)
            {
                // �������ݴ����߳�
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
//            this.txtOutput.Focus();//��ȡ����
//            this.txtOutput.Select(this.txtOutput.TextLength, 0);//��궨λ���ı����
//            this.txtOutput.ScrollToCaret();//��������괦   
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
                    MessageBox.Show("������comset");
                }

                m_devind = (UInt32)schemeConfigFrm.comboBox_DevIndex.SelectedIndex; //comboBox_DevIndex.SelectedIndex;
                m_canind = (UInt32)(UInt32)schemeConfigFrm.comboBox_CANIndex.SelectedIndex; //comboBox_CANIndex.SelectedIndex;
                if (VCI_OpenDevice(m_devtype, m_devind, 0) == 0)
                {
                    MessageBox.Show("���豸ʧ��,�����豸���ͺ��豸�������Ƿ���ȷ", "����",
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
            buttonConnect.Text = m_bOpen == 1 ? "�Ͽ�" : "����";
            timer_rec.Enabled = m_bOpen == 1 ? true : false;


            //            threadCan = new Thread(new ThreadStart(OnCanReceivedData));
            //threadCan.Start();

          /*  if (m_bOpen == 1)
            {
                MessageBox.Show("�ر��豸");
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
                    MessageBox.Show("���豸ʧ��,�����豸���ͺ��豸�������Ƿ���ȷ", "����",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }



                //System.Threading.Thread.Sleep(1000);
                //����CAN
                VCI_StartCAN(m_devtype, m_devind, m_canind);
                // �������ݴ����߳�
         
            }
            buttonConnect.Text = m_bOpen == 1 ? "disconnect" : "connect";

           timer_rec.Enabled = m_bOpen == 1 ? true : false;
           */
        }

        private void button_Send_Click(object sender, EventArgs e)
        {

        }

    //   #region//����

        //private void timer_rec_Tick()
       unsafe private void OnCanReceivedData()
        {
 
            UInt32 res = new UInt32();

           //System.Threading.Thread.Sleep(300);//Ĭ������
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
                        //                    str += "����: ";
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
                    
                    logstemp = "\r\n���գ�";
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
                                FrameBuff.Add(RuleInfo);    // ��ӽ��ܵ�����֡��������
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


//����ѹ������
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

        #region//����
        private void OnUpdateFirmwareRepeat()
        {
            int finishedTimes = 0;
            int sucessTimes = 0;
            int failTimes = 0;
            int[] failTimes_COM = new int[4];
            int txtOutputOkCnt = 0;
            int txtOutputCnt = 0;

            //UpdateFileInfo(txtUpdateFileDir.Text); //����bin�ĵ�

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
                    System.Threading.Thread.Sleep((Convert.ToInt16(textBox4.Text, 10)) * 1000);//unit��1s
                }

                finishedTimes++;
                txtFinishedTimes.Text = finishedTimes.ToString();
                txtSucessTimes.Text = sucessTimes.ToString();
                txtFailTimes.Text = failTimes.ToString();

                //this.txtOutput.Clear();//����ʾ����
                if (++txtOutputCnt > 10)//�쳣��� ÿ10��������������һ��
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
                System.Threading.Thread.Sleep(7 * 1000);//unit��1s
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
                System.Threading.Thread.Sleep(2 * 1000);//unit��1s
                if (FirmwareInformation() == false)  //COM12
                {
                    failTimes++;
                    failTimes_COM[3]++;
                    txtFailTimes.Text = failTimes.ToString();
                    continue;
                }
                Application.DoEvents();

                //this.txtOutput.Clear();//����ʾ����

                //System.Threading.Thread.Sleep(Convert.ToByte(textBox4.Text, 10));

                sucessTimes++;
                txtSucessTimes.Text = sucessTimes.ToString();

                txtOutputOkCnt++;
                /*
                                if (++txtOutputCnt > 1)//ÿ1��������������һ��
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
                // �������ݴ����߳�
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

            //UpdateFileInfo(txtUpdateFileDir.Text); //����bin�ĵ�

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
                    System.Threading.Thread.Sleep((Convert.ToInt16(textBox4.Text, 10)) * 1000);//unit��1s
                }

                finishedTimes++;
                txtFinishedTimes.Text = finishedTimes.ToString();
                txtSucessTimes.Text = sucessTimes.ToString();
                txtFailTimes.Text = failTimes.ToString();

                //this.txtOutput.Clear();//����ʾ����
                if (++txtOutputCnt > 10)//�쳣��� ÿ10��������������һ��
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
                System.Threading.Thread.Sleep(5 * 1000);//unit��1s
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
                System.Threading.Thread.Sleep(2 * 1000);//unit��1s
                if (FirmwareInformation() == false)  //COM12
                {
                    failTimes++;
                    failTimes_COM[3]++;
                    txtFailTimes.Text = failTimes.ToString();
                    continue;
                }
                Application.DoEvents();

                //this.txtOutput.Clear();//����ʾ����

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
//����ѹ�����Խ���

        public Thread threadApolloUpdate;
        private bool ApolloUpdateflag = false;
  #region//����
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
            System.Threading.Thread.Sleep(delayTimes);//unit��1ms

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
            System.Threading.Thread.Sleep(3 * 1000);//unit��1ms

            if (FirmwareInformation() == false)
            {
                ApolloUpdateflag = false;
                return;
            }

            //MessageBox.Show("APP Update Success");
            MessageBox.Show(" �������"+ '\n'+ "App update succeed", "Succeed");
            //*/
            ApolloUpdateflag = false;
        }
  #endregion
        private void button_apollo_update_Click(object sender, EventArgs e)
        {
            if (ApolloUpdateflag == false)
            {
                // �������ݴ����߳�
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

            data[0] = 0x0A;  // socУ׼
            //data[1] = 0x00;  // hall zeroУ׼
            int len = 1;

            sendCanDatas_Vcu(canid, data, len);

        }


  //     #endregion
    }
      
}

      
