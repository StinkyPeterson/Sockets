using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Sockets
{
    public partial class frmMain : Form
    {
        Socket udpSocket;
        private IPEndPoint remotePoint;
        private bool _continue = true;

        // конструктор формы
        public frmMain()
        {
            InitializeComponent();

            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            remotePoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1111);

            byte[] buff = Encoding.UTF8.GetBytes("connect:system_message");
            int bytes = udpSocket.SendTo(buff, remotePoint);

            new Thread(ReadMessages).Start();
        }

        private void ReadMessages()
        {
            while (_continue)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    udpSocket.Receive(buffer);
                    var message = Encoding.UTF8.GetString(buffer);
                    richTB.Invoke((MethodInvoker)delegate
                    {
                        if (message.Replace("\0", "") != "")
                        {
                            if (message.StartsWith("\n"))
                            {
                                richTB.Text += message;             // выводим полученное сообщение на форму
                            }
                            else
                            {
                                richTB.Text += "\n >> " + message;             // выводим полученное сообщение на форму
                            }
                        }
                    });
                }
                catch (Exception ex) { }
            }
        }


        // отправка сообщения
        private void btnSend_Click(object sender, EventArgs e)
        {
            byte[] buff = Encoding.UTF8.GetBytes(tbLogin.Text + " >> " + tbMessage.Text);   // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт
            udpSocket.SendTo(buff, remotePoint);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            udpSocket.Close();  
            _continue = false;           
        }
    }
}