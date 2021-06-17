using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
namespace CJQTest
{
    class CTestMember
    {
        public string TestName;
        public CTestMember(string str)
        {
            TestName = str;
        }

        virtual public void init()
        {

        }

        virtual public void testRun()
        {

        }

        virtual public void record()
        {

        }

        virtual public bool MakePacket()
        {
            return true;
        }

        virtual public bool PrasePacket()
        {
            return true;
        }

    }

    class CTestClock:CTestMember
    {
        public CTestClock():base("TEST_CLOCK")
        {
          
        }
    }
}
