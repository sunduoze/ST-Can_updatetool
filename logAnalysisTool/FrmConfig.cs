/* 使用EXCEL时要在项目-》添加引用-》COM-》MICORSOFT EXCEL OBJECT LIBRARY */
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

namespace CJQTest
{

    public partial class FrmConfig : Form
    {
        /* 数据成员定义 */
        // public int AnalysisConfigMemberNum = 0;
  //      public string configFilePath = Application.StartupPath + "\\config.xls";
        //public string IpFilePath = Application.StartupPath + "\\ipaddress.txt";

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out   int ID);
    
        private void LoadCheckedItem(string testName,string param)
        {
            if (testName == chkSysTime.Text)
            {
                chkSysTime.Checked = true;
                string[] str = param.Split(',');
                if (str.Length == 2)
                {
                    txtIP.Text = str[0].ToString();
                    txtPort.Text = str[1].ToString();
                }
            }
            else if (testName == chkBattery.Text)
            {
                chkBattery.Checked = true;
                string[] str = param.Split(',');
                if (str.Length == 2)
                {
                    txtIP.Text = str[0].ToString();
                    txtPort.Text = str[1].ToString();
                }

            }
            else if (testName == chk4851.Text)
            {
                chk4851.Checked = true;
                string[] str = param.Split(',');
                if (str.Length == 3)
                {
                    txtIP.Text = str[0].ToString();
                    txtPort.Text = str[1].ToString();
                    txtmeter1.Text = str[2].ToString();
                }
            }
            else if (testName == chk4852.Text)
            {
                chk4852.Checked = true;
                string[] str = param.Split(',');
                if (str.Length == 3)
                {
                    txtIP.Text = str[0].ToString();
                    txtPort.Text = str[1].ToString();
                    txtmeter2.Text = str[2].ToString();
                }
            }
            /*else if (testName == chkGPRS.Text)
            {
                chkGPRS.Checked = true;
                string[] str = param.Split(',');
                if (str.Length == 2)
                {
                    txtIP.Text = str[0].ToString();
                    txtPort.Text = str[1].ToString();
                }
            }*/
            else if (testName == chkRedWire.Text)
            {
                chkRedWire.Checked = true;
                string[] str = param.Split(',');
                if (str.Length == 4)
                {
                    comRedWire_no.Text = str[0].ToString();
                    comRedWire_bps.Text = str[1].ToString();
                    comRedWire_checkbit.Text = str[2].ToString();
                    comRedWire_stopbit.Text = str[3].ToString();
                }
            }
            else if (testName == chkDown.Text)
            {
                chkDown.Checked = true;
                string[] str = param.Split(',');
                if (str.Length == 3)
                {
                    txtIP.Text = str[0].ToString();
                    txtPort.Text = str[1].ToString();
                    txtmeter3.Text = str[2].ToString();
                }
            }
            else if (testName == chkWatchDog.Text)
            {
                chkWatchDog.Checked = true;
                string[] str = param.Split(',');
                if (str.Length == 4)
                {
                    com_no.Text = str[0].ToString();
                    com_bps.Text = str[1].ToString();
                    com_checkbit.Text = str[2].ToString();
                    com_stopbit.Text = str[3].ToString();
                }
            }
            
        }

        private void LoaderConfigFile()
        {
      
            //// 创建Application对象
            //Missing Miss = Missing.Value;
            //int i = 2, j = 0;

            //if (File.Exists(configFilePath) == false)
            //{
            //    return;
            //}
            //// 创建Application对象
            //Excel.Application xlsApp = new Excel.Application();
            //if (xlsApp == null)
            //{
            //    return;
            //}

            //if (xlsApp == null)
            //{
            //    return;
            //}

            ///* 打开文件 */
            //Excel.Workbook xlsBook = xlsApp.Workbooks.Open(configFilePath, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss, Miss);

            //Excel.Worksheet xlsSheet = (Excel.Worksheet)xlsBook.Sheets[1];

            //i = 2;
            //while ((string)(((Excel.Range)xlsSheet.Cells[i, 1]).Value2) != null)
            //{
            //    LoadCheckedItem((string)(((Excel.Range)xlsSheet.Cells[i, 1]).Value2),(string)(((Excel.Range)xlsSheet.Cells[i, 2]).Value2));
            //    i++;
            //}
            //// 关闭XLS文件
            //xlsBook.Close(false, Type.Missing, Type.Missing);
            //xlsApp.Quit();

            //// 任务管理器关闭excel.exe
            //IntPtr t = new IntPtr(xlsApp.Hwnd);
            //int k = 0;
            //GetWindowThreadProcessId(t, out   k);
            //System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(k);
            //p.Kill();

        }

        public FrmConfig()
        {
            InitializeComponent();
            Information.Text = "";
    //        LoaderConfigFile();
        }

        public string GetLogAnalysisConfigFilePath()
        {
            //return configFilePath;
            return "";
        }
        private void save_Click(object sender, EventArgs e)
        {
            int matrixHeight;
            int matrixWidth;
            string str;
            int row = 1;
            int col = 4;// 测试项|配置参数|测试结果|测试记录
            int MaxTestNum = 9 + 1;
            string ipaddr = "";

            string[,] martix = new string[MaxTestNum, col];

            // 表头信息
            martix[0, 0] = "测试项";
            martix[0, 1] = "配置参数";
            martix[0, 2] = "测试结果";
            martix[0, 3] = "信息记录";

            if (txtIP.Text == "")
            {
                MessageBox.Show("请输入IP地址");
            }

            // 保存ip 地址
            FileStream mystream = new FileStream("IpFilePath", FileMode.OpenOrCreate);
            StreamWriter Mywriter = new StreamWriter(mystream, Encoding.Default);
            Mywriter.Write(txtIP.Text);
            Mywriter.Close();
            mystream.Close();

            if (txtPort.Text == "")
            {
                MessageBox.Show("请输入端口号");
            }
            
            ipaddr = txtIP.Text + "," + txtPort.Text;

            if (chkSysTime.Checked == true)
            {
                martix[row, 0] = chkSysTime.Text;
                martix[row, 1] = ipaddr;
                row++;
            }

            if (chkBattery.Checked == true)
            {
                martix[row, 0] = chkBattery.Text;
                martix[row, 1] = ipaddr;
                row++;
            }

            if (chk4851.Checked == true)
            {
                martix[row, 0] = chk4851.Text;
                martix[row, 1] = ipaddr + "," + txtmeter1.Text;
                row++;
            }

            if (chk4852.Checked == true)
            {
                martix[row, 0] = chk4852.Text;
                martix[row, 1] = ipaddr + "," + txtmeter2.Text; ;
                row++;
            }

            if (chk4853.Checked == true)
            {
                martix[row, 0] = chk4853.Text;
                martix[row, 1] = ipaddr + "," + txtmeter4.Text; ;
                row++;
            }

            /*if (chkGPRS.Checked == true)
            {
                martix[row, 0] = chkGPRS.Text;
                martix[row, 1] = ipaddr;
                row++;
            }*/

            if (chkRedWire.Checked == true)
            {
                martix[row, 0] = chkRedWire.Text;
                martix[row, 1] = comRedWire_no.Text + "," + comRedWire_bps.Text + "," + comRedWire_checkbit.Text + "," + comRedWire_stopbit.Text;
                row++;
            } 
            
            if (chkDown.Checked == true)
            {
                martix[row, 0] = chkDown.Text;
                martix[row, 1] = ipaddr + "," + txtmeter3.Text; ;
                row++;
            }

            if (chkWatchDog.Checked == true)
            {
                martix[row, 0] = chkWatchDog.Text;
                martix[row, 1] = com_no.Text + "," + com_bps.Text + "," + com_checkbit.Text + "," + com_stopbit.Text;
                row++;
            }
           
            saveFile(martix, MaxTestNum, col);
            
        }

        private void saveFile(string[,] martix,int row, int col)
        {
            //if (File.Exists(configFilePath) == true)
            //{
            //    File.Delete(configFilePath);
            //}
            //else
            //{
            //}

            //// 创建Application对象
            //Excel.Application xlsApp = new Excel.Application();
            //if (xlsApp == null)
            //{
            //    return;
            //}
            //xlsApp.Visible = false;
            //Excel.Workbook xlsBook = xlsApp.Workbooks.Add(Missing.Value);
            //Excel.Worksheet xlsSheet = (Excel.Worksheet)xlsBook.Sheets[1];

            //Excel.Range range = xlsSheet.get_Range("A1", Type.Missing);
            //range = range.get_Resize(row, col);
            //range.Value2 = martix;

            //xlsBook.SaveCopyAs(configFilePath);
            //// 关闭XLS文件
            //xlsBook.Close(false, Type.Missing, Type.Missing);
            //xlsApp.Quit();
            //GC.Collect();

            //// 任务管理器关闭excel.exe
            //IntPtr t = new IntPtr(xlsApp.Hwnd);
            //int k = 0;
            //GetWindowThreadProcessId(t, out   k);
            //System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(k);
            //p.Kill();
            ////MessageBox.Show("配置文件保存成功！");
            //this.Close();
            
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void allchecked_CheckedChanged(object sender, EventArgs e)
        {
               
            if (allchecked.Checked == true)
            {
               chkSysTime.Checked = true;
               chkBattery.Checked = true;
               chk4851.Checked = true;
               chk4852.Checked = true;
               chk4853.Checked = true;
               //chkGPRS.Checked = true;
               chkRedWire.Checked = true;
               chkDown.Checked = true;
               chkWatchDog.Checked = true;
               
            }
            else
            {
                chkSysTime.Checked = false;
                chkBattery.Checked = false;
                chk4851.Checked = false;
                chk4852.Checked = false;
                chk4853.Checked = false;
                //chkGPRS.Checked = false;
                chkRedWire.Checked = false;
                chkDown.Checked = false;
                chkWatchDog.Checked = false;
            }
        }

        private void FrmConfig_Load(object sender, EventArgs e)
        {
            comboBox_DevIndex.SelectedIndex = 0;
            comboBox_CANIndex.SelectedIndex = 0;
            textBox_AccCode.Text = "00040000";//"00000000"; //debug 20191113 lin
            textBox_AccMask.Text = "FFFBFFFF";//"FFFFFFFF";  //debug 20191113 lin
            textBox_Time0.Text = "01";
            textBox_Time1.Text = "1C";
            comboBox_Filter.SelectedIndex = 0;              //接收所有类型
            comboBox_Mode.SelectedIndex = 0;                //还回测试模式
            //comboBox_FrameFormat.SelectedIndex = 0;
            //comboBox_FrameType.SelectedIndex = 0;
            //textBox_ID.Text = "00000123";
            //textBox_Data.Text = "00 01 02 03 04 05 06 07 ";
            
            Int32 curindex = 0;
            comboBox_devtype.Items.Clear();

            curindex = comboBox_devtype.Items.Add("DEV_USBCAN");
         //   m_arrdevtype[curindex] = DEV_USBCAN;
            //comboBox_devtype.Items[2] = "VCI_USBCAN1";
            //m_arrdevtype[2]=  VCI_USBCAN1 ;

            curindex = comboBox_devtype.Items.Add("DEV_USBCAN2");
          //  m_arrdevtype[curindex] = DEV_USBCAN2;
            //comboBox_devtype.Items[3] = "VCI_USBCAN2";
            //m_arrdevtype[3]=  VCI_USBCAN2 ;

            comboBox_devtype.SelectedIndex = 1;
            comboBox_devtype.MaxDropDownItems = comboBox_devtype.Items.Count;
        }

        private void chkWatchDog_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkNet_CheckedChanged(object sender, EventArgs e)
        {
            if (chkNet.Checked == true)
            {
                chkRS485.Checked = false;
            }

            if (chkRS485.Checked == true)
            {
                chkNet.Checked = false;
            }
        }

        private void chkRS485_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRS485.Checked == true)
            {
                chkNet.Checked = false;
            }

            if (chkNet.Checked == true)
            {
                chkRS485.Checked = false;
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

       
    }
}