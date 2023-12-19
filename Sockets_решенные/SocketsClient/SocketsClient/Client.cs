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
        private TcpClient Client = new TcpClient();     // клиентский сокет
        private IPAddress IP;                           // IP-адрес клиента
        private string Login;
        private bool _continue = true;

        // конструктор формы
        public frmMain()
        {
            InitializeComponent();

            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());    // информация об IP-адресах и имени машины, на которой запущено приложение
            IP = hostEntry.AddressList[0];                                  // IP-адрес, который будет указан в заголовке окна для идентификации клиента

            // определяем IP-адрес машины в формате IPv4
            foreach (IPAddress address in hostEntry.AddressList)
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    IP = address;
                    break;
                }

            this.Text += "     " + IP.ToString();                           // выводим IP-адрес текущей машины в заголовок формы

            new Thread(ReadMessages).Start();
        }

        private void ReadMessages()
        {
            while (_continue)
            {
                if (Client.Client.ReceiveBufferSize > 0 && Client.Client.Connected)
                {
                    try
                    {
                        byte[] buffer = new byte[1024];                           // буфер прочитанных из сокета байтов
                        Client.Client.Receive(buffer);                     // получаем последовательность байтов из сокета в буфер buff
                        string msg = System.Text.Encoding.Unicode.GetString(buffer);     // выполняем преобразование байтов в последовательность символов
                        richTextBox.Invoke((MethodInvoker)delegate
                        {
                            richTextBox.Text += "\n >> " + msg;             // выводим полученное сообщение на форму
                        });
                    }
                    catch (Exception ex)
                    {
                        Client.Close();
                    }
                }
            }
        }

        // подключение к серверному сокету
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                int Port = 1010;                                // номер порта, через который выполняется обмен сообщениями
                IPAddress IP = IPAddress.Parse(tbIP.Text);      // разбор IP-адреса сервера, указанного в поле tbIP
                Client.Connect(IP, Port);                       // подключение к серверному сокету
                btnConnect.Enabled = false;
                btnSend.Enabled = true;
                Login = tbLogin.Text;
            }
            catch
            {
                MessageBox.Show("Введен некорректный IP-адрес");
            }
        }

        // отправка сообщения
        private void btnSend_Click(object sender, EventArgs e)
        {
            byte[] buff = Encoding.Unicode.GetBytes(Login + " >> " + tbMessage.Text);   // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт
            Stream stm = Client.GetStream();                                                    // получаем файловый поток клиентского сокета
            stm.Write(buff, 0, buff.Length);                                                    // выполняем запись последовательности байт
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _continue = false;
            Client.Close();         // закрытие клиентского сокета
        }
    }
}