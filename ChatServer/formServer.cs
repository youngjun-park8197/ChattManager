using myLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatServer
{
    public partial class FormServer : Form
    {
        public FormServer()
        {
            InitializeComponent();
        }

        delegate void FuncCB(string str);

        void Fcb(string str)
        {
            if(tbReceive.InvokeRequired)
            {
                FuncCB cb = new FuncCB(Fcb);
                Invoke(cb, new object[] { str });
            }
            else
            {
                tbReceive.AppendText(str);
            }
        }


        string tmpString = "";
        Thread threadServer  = null;
        Thread threadRead = null;
        TcpListener listener = null;
        TcpClient tcp = null;

        private void btnServStart_Click(object sender, EventArgs e)
        {
            if(listener == null)
            {
                listener = new TcpListener(int.Parse(tbServPort.Text));
                listener.Start();
                sbStatusLabel.Text = "Server Started";
            }

            if (threadServer == null)
            {
                threadServer = new Thread(ServerProcess);
                threadServer.Start();
                threadRead = new Thread(ReadProcess);
            }
        }


        void ServerProcess() // 외부인자 X, 리턴값 X
        {
            while(true)
            {
                if (listener.Pending() == true) // 접속 요청이 있는 경우
                {
                    tcp = listener.AcceptTcpClient(); // 블로킹 모드
                    Fcb($"Connected from Remote EndPoint : {tcp.Client.RemoteEndPoint.ToString()}. \r\n"); // 화면상에 어디에 접속되었는지를 알 수 있게끔 해줌
                    threadRead.Start();
                }
                Thread.Sleep(100);
            }
        }


        void ReadProcess()
        {
            while (true) // 소켓이 없어지지 않는 한, 오류 발생 X, tcp에 대한 전용 루프(dedicated) : 권장 X
            {
                if (tcp != null)
                {
                    NetworkStream ns = tcp.GetStream(); // NetworkStream을 활용하여 Stream을 Read/Write 수행하도록 함
                    byte[] bArr = new byte[50];

                    while (ns.DataAvailable) // 데이터가 있는 동안
                    {
                        int n = ns.Read(bArr, 0, 50); // 데이터를 읽어서 처리
                        tmpString = Encoding.Default.GetString(bArr, 0, n);
                        // Fcb(tmpString);
                        // tbReceive.Text += Encoding.Default.GetString(bArr, 0, n); // form에 있는 컴포넌트
                    }

                }
            }
        }


        iniFile inif = new iniFile(".\\ChatServer.ini");
        private void FormServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            inif.SetString("Server", "Port", tbServPort.Text);
            inif.SetString("Form", "LocationX", $"{Location.X}");
            inif.SetString("Form", "LocationY", $"{Location.Y}");
            inif.SetString("Form", "SizeX", $"{Size.Width}");
            inif.SetString("Form", "SizeY", $"{Size.Height}");

            if (threadServer != null) threadServer.Abort();
            if (threadRead != null) threadRead.Abort();
        }

        private void FormServer_Load(object sender, EventArgs e)
        {
            int locX1, locY1, sizeX, sizeY;
            tbServPort.Text = inif.GetString("Server", "Port", "9001");

            locX1 = int.Parse(inif.GetString("Form", "LocationX", "0"));
            locY1 = int.Parse(inif.GetString("Form", "LocationY", "0"));
            this.Location = new Point(locX1, locY1);

            sizeX = int.Parse(inif.GetString("Form", "SizeX", "500"));
            sizeY = int.Parse(inif.GetString("Form", "SizeY", "500"));
            this.Size = new Size(sizeX, sizeY);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            /*
            if(tcp != null) // tcp가 null이 아니라면,
            {
                NetworkStream ns = tcp.GetStream(); // NetworkStream을 활용하여 Stream을 Read/Write 수행하도록 함


                byte[] bArr = Encoding.Default.GetBytes(tbSend.Text);
                ns.Write(bArr, 0, bArr.Length);

                while (true) // 소켓이 없어지지 않는 한, 오류 발생 X, tcp에 대한 전용 루프(dedicated) : 권장 X
                {
                    while (ns.DataAvailable) // 데이터가 있는 동안
                    {
                        int n = ns.Read(bArr, 0, 50); // 데이터를 읽어서 처리
                        tmpString = Encoding.Default.GetString(bArr, 0, n);
                        // Fcb(tmpString);
                        // tbReceive.Text += Encoding.Default.GetString(bArr, 0, n); // form에 있는 컴포넌트
                    }
                }
            }
            */
            strSend(tbSend.Text);
        }

        private void strSend(string str)
        {
            if(tcp != null)
            {
                NetworkStream ns = tcp.GetStream();

                byte[] bArr = Encoding.Default.GetBytes(str);
                ns.Write(bArr, 0, bArr.Length);
            }
        }

        private void tbSend_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int lineNo = tbSend.GetLineFromCharIndex(tbSend.SelectionStart);
                string[] sArr = tbSend.Text.Split('\n');
                string str = sArr[lineNo - 1] + "\n";
                strSend(str);

                // 한줄로 표현할 시 ==> strSend(tbSend.Text.Split('\n')[tbSend.GetLineFromCharIndex(tbSend.SelectionStart) - 1] + "\n");
            }
        }

        private void mnuSend_Click(object sender, EventArgs e)
        {
            strSend(tbSend.SelectedText);
        }
    }
}
