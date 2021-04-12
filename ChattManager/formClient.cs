using myLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChattManager
{
    public partial class formClient : Form
    {
        public formClient()
        {
            InitializeComponent();
        }


        // addtext에 대한 콜백 함수 구현
        delegate void cbAddText(string s);


        void AddText(string str)
        {
            if(tbReceive.InvokeRequired)
            {
                cbAddText cb = new cbAddText(AddText);
                object[] oArr = { str };
                Invoke(cb, oArr);
            }
            else
            {
                tbReceive.AppendText(str);
            }
        }


        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            int tSize = 200; // splitContainer1.Panel2 의 사이즈

            /* 
             int tSize = splitContainer1.Size.Width; // split 컨테이너 전체 사이즈
             int dSize = splitContainer1.SplitterDistance; // split distance
             int pTwoSize = tSize - dSize; // splitContainer1.Panel2 의 사이즈
            */

            splitContainer1.SplitterDistance = splitContainer1.Size.Width - tSize;
        }


        Thread threadClient = null;
        Socket sock = null;
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if(sock == null)
            {
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }

            try
            {
                sock.Connect(tbIP.Text, int.Parse(tbPort.Text));
            } 
            
            catch
            {

            }

            sbLabelIP.Text = ((IPEndPoint)(sock.RemoteEndPoint)).Address.ToString();
            sbLabelPort.Text = ((IPEndPoint)(sock.RemoteEndPoint)).Port.ToString();
            // sock.RemoteEndPoint.ToString(); // ex) 127.0.0.1:12345 (12345는 명목상 포트번호, 실제적으로는 세션값)

            if(threadClient == null)
            {
                threadClient = new Thread(ClientProcess);
                threadClient.Start();
            }
        }


        void ClientProcess() // Thread 등록 프로세스, 쓰레드가 등록이 되었다고 하여 자동적으로 수행되도록 하지 않는 것이 중요
        {
            while(true)
            {
                int num = sock.Available;
                if (num > 0)
                {
                    byte[] bArr = new byte[num]; // C#에서의 통신은 byte[] 배열로 주고받음, C/C++는 char
                    sock.Receive(bArr);
                    AddText(Encoding.Default.GetString(bArr));

                    // tbReceive.Text += Encoding.Default.GetString(bArr); // byte[] 배열의 크기만큼 변환(NULL 포함)
                }
            }
        }


        private void btnSend_Click(object sender, EventArgs e)
        {
            if(sock.Connected == true)
            {
                string str = tbSend.Text;
                string[] sArr = str.Split('\r'); // multi line \r\n
                string sLast = sArr[sArr.Length - 1];
                sock.Send(Encoding.Default.GetBytes(sLast));
            }
        }


        iniFile inif = new iniFile(".\\ChatClient.ini");

        private void formClient_Load(object sender, EventArgs e)
        {
            int locX1, locY1, sizeX, sizeY;
            tbIP.Text = inif.GetString("Server", "IP", "127.0.0.1");
            tbPort.Text = inif.GetString("Server", "Port", "9001");

            locX1 = int.Parse(inif.GetString("Form", "LocationX", "0"));
            locY1 = int.Parse(inif.GetString("Form", "LocationY", "0"));
            this.Location = new Point(locX1, locY1);

            sizeX = int.Parse(inif.GetString("Form", "SizeX", "500"));
            sizeY = int.Parse(inif.GetString("Form", "SizeY", "500"));
            this.Size = new Size(sizeX, sizeY);
        }


        private void formClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            inif.SetString("Server", "IP", tbIP.Text);
            inif.SetString("Server", "Port", tbPort.Text);

            inif.SetString("Form", "LocationX", $"{Location.X}");
            inif.SetString("Form", "LocationY", $"{Location.Y}");

            inif.SetString("Form", "SizeX", $"{Size.Width}");
            inif.SetString("Form", "SizeY", $"{Size.Height}");
        }
    }
}
