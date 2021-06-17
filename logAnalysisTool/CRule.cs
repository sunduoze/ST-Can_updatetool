using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO.Ports;
using System.Text.RegularExpressions;


namespace CJQTest
{
    /// <summary>
    /// ���������ָ����֡ͷ��֡β��
    /// </summary>
    public enum ParseResult
    {
        /// <summary>
        /// ��ȷ�ı���
        /// </summary>
        OK = 0,
        /// <summary>
        /// ������ʱδ֪��;
        /// </summary>
        Waitting = 1,
        /// <summary>
        /// �������ı���
        /// </summary>
        Unintegrated = 2,
        /// <summary>
        /// ����ı��� û��68 ��CS����
        /// </summary>
        Error = -1,
    }

      public class CRule
    {
        virtual public bool Makeframes(ref ArrayList frame, byte[] addr, byte ctl, byte[] data){return true;}
	 virtual public bool Makeframes(ref ArrayList frame, byte[] addr, byte ctl, byte[] data,int length){return true;}
        virtual public bool Makeframes(ref ArrayList frame, byte[] addr, byte AFN, bool pw, int pwLen, byte[] pn, byte[] fn, byte[] data){return true;}
        virtual public bool Makeframes(ref ArrayList frame, byte[] addr, byte AFN, bool pw, int pwLen, byte[] pn, byte[] fn) { return true; }

        virtual public ParseResult ParsePackage(ref ArrayList irReceived, ref Frame_Stu ruleinfo, ref int startindex, ref int endindex) { return ParseResult.OK; }

    }
    public class CGwRule : CRule
    {
        public const int GW_PW_LEN = 16;
        public const int GX_PW_LEN = 2;
        /// <summary>
        /// ����У���
        /// </summary>
        /// <returns></returns>
        public byte getCS(ArrayList frame)
        {
            byte temp = 0;
            foreach (byte item in frame)
                temp += item;
            return temp;
        }

        public byte getCS(Array frame)
        {
            byte temp = 0;
            foreach (byte item in frame)
                temp += item;
            return temp;
        }
        public override ParseResult ParsePackage(ref ArrayList irReceived, ref Frame_Stu ruleinfo, ref int startindex, ref int endindex)
        {
            ruleinfo.dataArray = new ArrayList();
            ArrayList temp = new ArrayList();
            temp.AddRange(irReceived);
            //�����в������κ����ݶ�ʱ����̳���Ϊ15�ֽ�
            if (irReceived.Count <= 15)
            {
                startindex = 0;
                endindex = 0;
                return ParseResult.Unintegrated;
            }
            int indexof3A = 0;

            while (startindex < irReceived.Count)
            {
                //������ʼ0x3A
                startindex = irReceived.IndexOf((byte)0x3A, indexof3A);
                if (startindex < 0)
                    return ParseResult.Error;

                endindex = startindex;
                indexof3A = startindex;

                if ((byte)irReceived[startindex + 1] != 0x16)
                {
                    irReceived.RemoveRange(0, 1);
                    continue;
                }
                else
                    endindex = startindex + 3;//endindexָ���˵ڶ���0x68

                int length = 0;
                length = Convert.ToInt32((byte)irReceived[startindex + 2]);

           
                ruleinfo.ctrl = (byte)irReceived[startindex + 3];

                endindex = startindex + 8 + length;//endindexָ�����û����ݶε�ĩβ
                if (irReceived.Count < endindex)
                    return ParseResult.Unintegrated;

                if ((byte)irReceived[endindex] != 0x0A)
                    return ParseResult.Error;

                ruleinfo.dataArray.Clear();
                ruleinfo.dataArray.AddRange(irReceived.GetRange(startindex + 4, length).ToArray(typeof(byte)));
                return ParseResult.OK;
            }
            return ParseResult.Unintegrated;
        }
        /// <summary>
        /// ������֯����
        /// </summary>
        /// <param name="frame">�������������ڼ�¼��֯�õı���</param>
        /// <param name="shm">�����ڴ�����</param>
        /// <returns>trueΪ�ɹ� falseΪʧ��</returns>
        public override bool Makeframes(ref ArrayList frame, byte[] addr, byte ctl, byte[] data)
        {
            ArrayList ftp = new ArrayList();
            frame.Clear();
            frame.Add((byte)0x68);
            for (int i = 0; i < 6; i++)
                frame.Add((byte)addr[i]);
            frame.Add((byte)0x68);

            frame.Add(ctl);

            int framelength = data.Length;
            frame.Add((byte)(framelength));

            for (int j = 0; j < data.Length; j++)
            {
                frame.Add((byte)(data[j]));
            }
            //frame.AddRange(framedatas.GetRange(0, framedatas.Count));
            //foreach (byte bt in framedatas)
            //    frame.Add(bt);

            //frame.Add(getCS(framedatas));
            frame.Add(getCS(frame));
            frame.Add((byte)0x16);

            return true;
        }
	 public override bool Makeframes(ref ArrayList frame, byte[] addr, byte ctl, byte[] data,int length)
        {
            ArrayList ftp = new ArrayList();
            frame.Clear();
            frame.Add((byte)0x68);
            for (int i = 0; i < 6; i++)
                frame.Add((byte)addr[i]);
            frame.Add((byte)0x68);

            frame.Add(ctl);

            int framelength = length;
            frame.Add((byte)(framelength));

            for (int j = 0; j < length; j++)
            {
                frame.Add((byte)(data[j]));
            }
            //frame.AddRange(framedatas.GetRange(0, framedatas.Count));
            //foreach (byte bt in framedatas)
            //    frame.Add(bt);

            //frame.Add(getCS(framedatas));
            frame.Add(getCS(frame));
            frame.Add((byte)0x16);

            return true;
        }

        /*
        68
        E2 00 E2 00 ���ĳ���
        68 
        4B ������
        00 51 02 00 �������߼���ַBCD��
        02 		��վ��ţ�������Ŀǰ����֤���ַ���
        04 	AFN ������ 04Ϊ���ò���
        70 SEQ
        00 00 04 00	������PN=0 FN=3
        */
        public override bool Makeframes(ref ArrayList frame, byte[] addr, byte AFN, bool pw, int pwLen, byte[] pn, byte[] fn, byte[] data)
        {
            ArrayList ftp = new ArrayList();
            frame.Clear();
            frame.Add((byte)0x68);
            frame.Add((byte)0x68);

            frame.Add((byte)0x4B);  // ������
            /* ��������ַ */
            frame.AddRange(addr);
            /* ��վ��� */
            frame.Add((byte)0x02);
            /* AFN */
            frame.Add(AFN);
            /* MSTA&SEQ */
            frame.Add((byte)0x70);
            /* PN */
            frame.AddRange(pn);
            /* FN */
            frame.AddRange(fn);
            /* data */
            frame.AddRange(data);
            /* PW */
            if (pw == true)
            {
                for (int n = 0; n < pwLen; n++)
                {
                    frame.Add((byte)0x00);
                }
            }

            int len = frame.Count;
            if (pwLen == GW_PW_LEN)
            {
                len = ((len - 2) << 2) | 0x02;
            }
            else
            {
                len = ((len - 2) << 2) | 0x01;
            }
            frame.Insert(1, (byte)(len & 0xff));
            frame.Insert(2, (byte)((len >> 8) & 0xff));
            frame.Insert(3, (byte)(len & 0xff));
            frame.Insert(4, (byte)((len >> 8) & 0xff));

            frame.Add(getCS(frame.GetRange(6, frame.Count - 6)));
            frame.Add((byte)0x16);

            return true;
        }


        public override bool Makeframes(ref ArrayList frame, byte[] addr, byte AFN, bool pw, int pwLen, byte[] pn, byte[] fn)
        {
            ArrayList ftp = new ArrayList();
            frame.Clear();
            frame.Add((byte)0x68);
            frame.Add((byte)0x68);

            frame.Add((byte)0x4B);  // ������
            /* ��������ַ */
            frame.AddRange(addr);
            /* ��վ��� */
            frame.Add((byte)0x02);
            /* AFN */
            frame.Add(AFN);
            /* MSTA&SEQ */
            frame.Add((byte)0x70);
            /* PN */
            frame.AddRange(pn);
            /* FN */
            frame.AddRange(fn);
            ///* PW */
            if (pw == true)
            {
                for (int n = 0; n < pwLen; n++)
                {
                    frame.Add((byte)0x00);
                }
            }
            int len = frame.Count;
            if (pwLen == GW_PW_LEN)
            {
                len = ((len - 2) << 2) | 0x02;
            }
            else
            {
                len = ((len - 2) << 2) | 0x01;
            }
            frame.Insert(1, (byte)(len & 0xff));
            frame.Insert(2, (byte)((len >> 8) & 0xff));
            frame.Insert(3, (byte)(len & 0xff));
            frame.Insert(4, (byte)((len >> 8) & 0xff));

            frame.Add(getCS(frame.GetRange(6, frame.Count - 6)));
            frame.Add((byte)0x16);

            return true;
        }

    }

    public class CGdRule : CRule
    {
      
        public byte getCS(ArrayList frame)
        {
            byte temp = 0;
            foreach (byte item in frame)
                temp += item;
            return temp;
        }

        public byte getCS(Array frame)
        {
            byte temp = 0;
            foreach (byte item in frame)
                temp += item;
            return temp;
        }

        private UInt16 CRC16RTU(byte[] pszBuf, Int16 unLength)
        {
            UInt16 CRC = 0xffff;
            Int32 CRC_count;
            UInt16 VAL = 0x0001;
	        for(CRC_count=0;CRC_count<unLength;CRC_count++)
	        {
		        int i;

                CRC = (UInt16)(CRC ^ pszBuf[CRC_count]);

		        for(i=0;i<8;i++)
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

        public override ParseResult ParsePackage(ref ArrayList irReceived, ref Frame_Stu ruleinfo, ref int startindex, ref int endindex)
        {
            ruleinfo.dataArray = new ArrayList();
            ArrayList temp = new ArrayList();
            temp.AddRange(irReceived);
            //�����в������κ����ݶ�ʱ����̳���Ϊ15�ֽ�
            if (irReceived.Count <= 5)
            {
                startindex = 0;
                endindex = 0;
                return ParseResult.Unintegrated;
            }
            int indexof11 = 0;

            while (startindex < irReceived.Count)
            {
                //������ʼ0x03
                startindex = irReceived.IndexOf((byte)0x03, indexof11);
                if (startindex < 0)
                    return ParseResult.Error;

                endindex = startindex;
                indexof11 = startindex;

                if ((byte)irReceived[startindex + 1] != 0x45)
                {
                    irReceived.RemoveRange(0, 1);
                    continue;
                }
              

                ruleinfo.ctrl = (byte)irReceived[startindex + 2];

                int length = 0;
                length = Convert.ToInt32((byte)irReceived[startindex + 3] << 8 | (byte)irReceived[startindex + 4]);

                byte[] data = new byte[Convert.ToInt16(length + 5)];
                //debug 20200615 lin
                //*
                if (startindex + 5 + length + 1 > irReceived.Count)
                {
                    return ParseResult.Error;
                }

                //*/
                irReceived.GetRange(startindex, length + 5).CopyTo(data);
                //*
                UInt16 crc = CRC16RTU(data, Convert.ToInt16(length + 5));

                if (crc != (((byte)irReceived[startindex + 5+length] | (byte)irReceived[startindex + 5+length+1]<<8)))
                {
                   return ParseResult.Error;
                }
                 //*/ //debug 20200316 lin
                ruleinfo.dataArray.Clear();
                ruleinfo.dataArray.AddRange(irReceived.GetRange(startindex + 5, length).ToArray(typeof(byte)));
                irReceived.Clear();
                return ParseResult.OK;
            }
            return ParseResult.Unintegrated;
        }
        /// <summary>
        /// ������֯����
        /// </summary>
        /// <param name="frame">�������������ڼ�¼��֯�õı���</param>
        /// <param name="shm">�����ڴ�����</param>
        /// <returns>trueΪ�ɹ� falseΪʧ��</returns>
        /// 68 81 41 09 00 9E 00 68 01 0A 00 01 00 00 00 00 00 00 00 30 80 F5 16 
        public override bool Makeframes(ref ArrayList frame, byte[] addr, byte ctl, byte[] data)
        {
            ArrayList ftp = new ArrayList();
            int FSEQ = 1;

            frame.Clear();
            frame.Add((byte)0x68);
            for (int i = 0; i < 4; i++)
            {
                frame.Add((byte)addr[i]);
            }

            int nMSTA = 30 + (FSEQ << 6);	//��д��վ��ַ���������
            if (FSEQ == 0X7F) //ѭ���������
            {
                FSEQ = 0X01;
            }
            else
            {
                FSEQ++;
            }
            frame.Add((byte)(nMSTA & 0XFF));
            frame.Add((byte)((nMSTA >> 8) & 0XFF));

            frame.Add((byte)0x68);

            frame.Add(ctl);

            int framelength = data.Length;
            frame.Add((byte)(framelength&0xff));
            frame.Add((byte)((framelength>>8)&0xff));

            for (int j = 0; j < data.Length; j++)
            {
                frame.Add((byte)(data[j]));
            }
            //frame.AddRange(framedatas.GetRange(0, framedatas.Count));
            //foreach (byte bt in framedatas)
            //    frame.Add(bt);

            //frame.Add(getCS(framedatas));
            frame.Add(getCS(frame));
            frame.Add((byte)0x16);

            return true;
        }
        public override bool Makeframes(ref ArrayList frame, byte[] addr, byte ctl, byte[] data, int Length)
        {
            ArrayList ftp = new ArrayList();
            int FSEQ = 1;

            frame.Clear();
            frame.Add((byte)0x68);
            for (int i = 0; i < 4; i++)
            {
                frame.Add((byte)addr[i]);
            }

            int nMSTA = 30 + (FSEQ << 6);	//��д��վ��ַ���������
            if (FSEQ == 0X7F) //ѭ���������
            {
                FSEQ = 0X01;
            }
            else
            {
                FSEQ++;
            }
            frame.Add((byte)(nMSTA & 0XFF));
            frame.Add((byte)((nMSTA >> 8) & 0XFF));

            frame.Add((byte)0x68);

            frame.Add(ctl);

            int framelength = Length;
            frame.Add((byte)(framelength & 0xff));
            frame.Add((byte)((framelength >> 8) & 0xff));

            for (int j = 0; j < Length; j++)
            {
                frame.Add((byte)(data[j]));
            }
            //frame.AddRange(framedatas.GetRange(0, framedatas.Count));
            //foreach (byte bt in framedatas)
            //    frame.Add(bt);

            //frame.Add(getCS(framedatas));
            frame.Add(getCS(frame));
            frame.Add((byte)0x16);

            return true;
        }
    }
}
