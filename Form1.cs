using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace UDP_Asynchronous
{
    //The commands for interaction between the server and the client
    enum Command
    {
        Login,      //Log into the server
        Logout,     //Logout of the server
        Message,    //Send a text message to all the chat clients
        List,       //Get a list of users in the chat room from the server
        Null        //No command
    }
    public partial class Form1 : Form
    {
        Socket serverSocket;
        public Socket clientSocket; //The main client socket
        public EndPoint epServer;   //The EndPoint of the server

        byte[] byteData = new byte[1024];
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {}
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                CheckForIllegalCrossThreadCalls = false;

                //We are using UDP sockets
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //Assign the any IP of the machine and listen on port number 1000
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 1000);

                //Bind this address to the server
                serverSocket.Bind(ipEndPoint);

                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 1000);
                //The epSender identifies the incoming clients
                EndPoint epSender = (EndPoint)ipeSender;

                //Start receiving data
                serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epSender, new AsyncCallback(OnReceive), epSender);
            }
            if (comboBox1.SelectedIndex == 1)
            {
                CheckForIllegalCrossThreadCalls = false;
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);       //IP address of the server machine
                IPAddress ipAddress = IPAddress.Parse(textBox1.Text);            //Server is listening on port 1000
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);
                epServer = (EndPoint)ipEndPoint;

                Data msgToSend = new Data();
                byteData = msgToSend.ToByte();

                clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
                byteData = new byte[1024];
                //Start listening to the data asynchronously
                clientSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epServer, new AsyncCallback(OnReceive), null);
            }
        }
        private void OnReceive(IAsyncResult ar)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                try
                {
                    IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 1000);
                    EndPoint epSender = (EndPoint)ipeSender;

                    serverSocket.EndReceiveFrom(ar, ref epSender);

                    //Transform the array of bytes received from the user into an object.
                    Data msgReceived = new Data(byteData);

                    //We will send this object in response the users request
                    Data msgToSend = new Data();
                    msgToSend.cmdCommand = msgReceived.cmdCommand;
                    msgToSend.strName = msgReceived.strName;

                    richTextBox1.Text = msgReceived.strMessage + "\r\n";
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                    serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epSender, new AsyncCallback(OnReceive), epSender);

                    Data msgToSend1 = new Data();
                    richTextBox2.Text = DateTime.UtcNow.ToString();
                    msgToSend1.strMessage = richTextBox2.ToString();
                    byte[] message;
                    message = msgToSend1.ToByte();
                    serverSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, epSender, new AsyncCallback(OnSend), epSender);
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SGSServerUDP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (comboBox1.SelectedIndex == 1)
            {
                try
                {
                    clientSocket.EndReceive(ar);

                    //Convert the bytes received into an object of type Data
                    Data msgReceived = new Data(byteData);

                    richTextBox3.Text = msgReceived.strMessage + "\r\n";
                    richTextBox3.SelectionStart = richTextBox3.Text.Length;
                    richTextBox3.ScrollToCaret();

                    //Start listening to receive more data from the user
                    clientSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epServer, new AsyncCallback(OnReceive), null);
                    Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SGSclient: ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void OnSend(IAsyncResult ar)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                try
                {
                    //IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 1000);
                    //EndPoint epSender = (EndPoint)ipeSender;
                    //Data msgToSend = new Data();
                    //byte[] message;
                    //txt_Send.Text = DateTime.UtcNow.ToString();
                    //msgToSend.strMessage = txt_Send.ToString();
                    //message = msgToSend.ToByte();
                    //serverSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, epSender, new AsyncCallback(OnSend), epSender);
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SGSServerUDP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (comboBox1.SelectedIndex == 1)
            {
                try
                {
                    //clientSocket.EndSend(ar);
                    Data msgToSend = new Data();
                    richTextBox4.Text = DateTime.Now.ToString();

                    msgToSend.strMessage = richTextBox4.Text;
                    //msgToSend.cmdCommand = Command.Message;

                    byte[] byteData = msgToSend.ToByte();

                    //Send it to the server
                    clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SGSclient: ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        //The data structure by which the server and the client interact with each other.
        class Data
        {
            //Default constructor
            public Data()
            {
                this.cmdCommand = Command.Null;
                this.strMessage = null;
                this.strName = null;
            }
            //Converts the bytes into an object of type Data
            public Data(byte[] data)
            {
                //The first four bytes are for the Command
                this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);

                //The next four store the length of the name
                int nameLen = BitConverter.ToInt32(data, 4);

                //The next four store the length of the message
                int msgLen = BitConverter.ToInt32(data, 8);

                //This check makes sure that strName has been passed in the array of bytes
                if (nameLen > 0)
                    this.strName = Encoding.UTF8.GetString(data, 12, nameLen);
                else
                    this.strName = null;

                //This checks for a null message field
                if (msgLen > 0)
                    this.strMessage = Encoding.UTF8.GetString(data, 12 + nameLen, msgLen);
                else
                    this.strMessage = null;
            }
            //Converts the Data structure into an array of bytes
            public byte[] ToByte()
            {
                List<byte> result = new List<byte>();

                //First four are for the Command
                result.AddRange(BitConverter.GetBytes((int)cmdCommand));

                //Add the length of the name
                if (strName != null)
                    result.AddRange(BitConverter.GetBytes(strName.Length));
                else
                    result.AddRange(BitConverter.GetBytes(0));

                //Length of the message
                if (strMessage != null)
                    result.AddRange(BitConverter.GetBytes(strMessage.Length));
                else
                    result.AddRange(BitConverter.GetBytes(0));

                //Add the name
                if (strName != null)
                    result.AddRange(Encoding.UTF8.GetBytes(strName));

                //And, lastly we add the message text to our array of bytes
                if (strMessage != null)
                    result.AddRange(Encoding.UTF8.GetBytes(strMessage));

                return result.ToArray();
            }
            public string strName;      //Name by which the client logs into the room
            public string strMessage;   //Message text
            public Command cmdCommand;  //Command type (login, logout, send message, etcetera)
        }
    }
}