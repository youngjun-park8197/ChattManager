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

        delegate void FuncCB();

        void Fcb()
        {
            if(tbReceive.InvokeRequired)
            {
                FuncCB cb = new FuncCB(Fcb);
                Invoke(cb);
            }
            else
            {
                tbReceive.AppendText(tmpString);
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
                    threadRead.Start();
                }
                Thread.Sleep(100);
            }
        }


        void ReadProcess()
        {
            if(tcp != null)
            {
                NetworkStream ns = tcp.GetStream(); // NetworkStream을 활용하여 Stream을 Read/Write 수행하도록 함
                byte[] bArr = new byte[50];
                
                while (true) // 소켓이 없어지지 않는 한, 오류 발생 X, tcp에 대한 전용 루프(dedicated) : 권장 X
                {
                    while (ns.DataAvailable) // 데이터가 있는 동안
                    {
                        int n = ns.Read(bArr, 0, 50); // 데이터를 읽어서 처리
                        tmpString = Encoding.Default.GetString(bArr, 0, n);
                        Fcb();
                        // tbReceive.Text += Encoding.Default.GetString(bArr, 0, n); // form에 있는 컴포넌트
                    }
                }
            }
        }


        private void FormServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (threadServer != null) threadServer.Abort();
            if (threadRead != null) threadRead.Abort();
        }
    }
}
