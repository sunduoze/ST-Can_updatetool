using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using System.Threading;
using System.Data.OleDb;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace CJQTest
{
    public partial class CTool : Form
    {
        public string meter97Path = Application.StartupPath + "\\tool\\MeterGB97\\";
        public string meter07Path = Application.StartupPath + "\\tool\\MeterGB2007\\";
        
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out   int ID);   

        public CTool()
        {
            InitializeComponent();
        }

        private void btnMeter2007_Click(object sender, EventArgs e)
        {
            //声明一个程序信息类 
            System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo(); 
            //设置外部程序名 
            Info.FileName = "MeterGB2007.exe"; 
            //设置外部程序的启动参数（命令行参数）为test.txt 
            Info.Arguments=""; 
            //设置外部程序工作目录为C:\\ 
            Info.WorkingDirectory = meter07Path; 
            //声明一个程序类 
            System.Diagnostics.Process Proc ; 
            try 
            { 
                Proc=System.Diagnostics.Process.Start(Info); 
            } 
            catch (SystemException er)
            {
                MessageBox.Show(er.Message);
            }

        }

        private void btnMeter97_Click(object sender, EventArgs e)
        {
            //声明一个程序信息类 
            System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo();
            //设置外部程序名 
            Info.FileName = "MeterGB97.exe";
            //设置外部程序的启动参数（命令行参数）为test.txt 
            Info.Arguments = "";
            //设置外部程序工作目录为C:\\ 
            Info.WorkingDirectory = meter97Path;
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
    }
}