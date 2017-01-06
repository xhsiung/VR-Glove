﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            MyTcpIpClient client = new MyTcpIpClient();
            while (true)
            {
                //Console.WriteLine("1111111");
            }
        }
    }

    /// MyTcpIpClient 提供在Net TCP_IP 协议上基于消息的客户端 
    public class MyTcpIpClient : System.ComponentModel.Component
    {
        private int bufferSize = 2048;
        private string tcpIpServerIP = "101.200.45.113";
        private int tcpIpServerPort = 8080;
        private Socket ClientSocket = null;
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);


        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);

            }
            catch (Exception e)
            {
                OnErrorEvent(new ErrorEventArgs(e));
            }
            finally
            {
                connectDone.Set();
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
            }
            catch (Exception e)
            {
                OnErrorEvent(new ErrorEventArgs(e));
            }
            finally
            {
                sendDone.Set();
            }
        }


        /// 数据接收处理//////// ,bytesRead是这次接收到的包的一部分数据size，如果没接收完，继续调用自己，直到没有数据
        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket handler = null;
            try
            {
                lock (ar)
                {
                    StateObject state = (StateObject)ar.AsyncState;
                    handler = state.workSocket;
                    int bytesRead = handler.EndReceive(ar);
                    Console.WriteLine("cnt:"+state.cnt);
                    int readCnt = 0;//the length of data which has been read in this callback func

                    if (bytesRead > 0)
                    {
                        //Console.WriteLine("read:"+bytesRead);
                        if (state.cnt + bytesRead < 5)
                        {
                            //Console.WriteLine("11111111111");
                            for (int i = state.cnt; i < bytesRead + state.cnt; ++i)
                            {
                                state.lt_record[i] = state.buffer[i - state.cnt];
                                Console.WriteLine(state.lt_record[i]);
                            }

                            readCnt += bytesRead;
                        }
                        else
                        {
                            if (state.packSize == -1)
                            {
                                Console.WriteLine("2222222222222");
                                for (int i = state.cnt; i < 5; ++i)
                                {
                                    state.lt_record[i] = state.buffer[i - state.cnt];
                                    Console.WriteLine(state.lt_record[i]);
                                }

                                state.packSize = (int)((state.lt_record[0] & 0xFF << 24)
                                                    | ((state.lt_record[1] & 0xFF) << 16)
                                                    | ((state.lt_record[2] & 0xFF) << 8)
                                                    | ((state.lt_record[3] & 0xFF)));
                                state.dataType = (int)state.lt_record[4];
                                readCnt += (5 - state.cnt);
                               // Console.WriteLine(state.lt_record[0] + " " + state.lt_record[1] + " " + state.lt_record[2] + " ");
                                Console.WriteLine(state.packSize + " " + state.dataType);
                            }

                        }
                        Console.WriteLine("byt--xxx:" + bytesRead);
                        state.cnt += bytesRead;
                        Console.WriteLine("cnt--wwww:" + state.cnt);
                        while (readCnt < bytesRead)
                        {
                            
                            //Console.Write(state.buffer[readCnt]);  
                            readCnt++;
                        }
                        Console.WriteLine(readCnt);

                    }
                    else
                    {
                        Console.WriteLine("----end-----");
                        throw (new Exception("读入的数据小于1bit"));
                    }
                    if (handler.Connected == true && (state.packSize == -1 || (state.cnt - 5 < state.packSize)))
                    {
                        handler.BeginReceive(state.buffer, 0, bufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                        Console.WriteLine("****" + state.packSize);
                    }
                    else
                    {
                        Console.WriteLine("kkkkkkkkkkk:"+state.packSize+" "+state.cnt);
                    }
                }
            }
            catch (Exception e)
            {
                OnErrorEvent(new ErrorEventArgs(e));

            }
        }

        /// 连接服务器
        public void Conn()
        {
            try
            {
                ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = IPAddress.Parse(tcpIpServerIP);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, tcpIpServerPort);
                connectDone.Reset();
                ClientSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), ClientSocket);
            }
            catch (Exception e)
            {
                OnErrorEvent(new ErrorEventArgs(e));
            }

        }

        /// 断开连接
        public void Close()
        {
            try
            {
                if (ClientSocket.Connected == true)
                {
                    ClientSocket.Shutdown(SocketShutdown.Both);
                    ClientSocket.Close();
                }
            }
            catch (Exception e)
            {
                OnErrorEvent(new ErrorEventArgs(e));
            }

        }

        /// 发送一个流数据
        public void Send(Stream Astream)
        {
            try
            {
                if (ClientSocket.Connected == false)
                {
                    throw (new Exception("没有连接服务器不可以发送信息!"));
                }
                Astream.Position = 0;
                byte[] byteData = new byte[bufferSize];
                int bi1 = (int)((Astream.Length + 8) / bufferSize);
                int bi2 = (int)Astream.Length;
                if (((Astream.Length + 8) % bufferSize) > 0)
                {
                    bi1 = bi1 + 1;
                }
                bi1 = bi1 * bufferSize;

                byteData[0] = System.Convert.ToByte(bi1 >> 24);
                byteData[1] = System.Convert.ToByte((bi1 & 0x00ff0000) >> 16);
                byteData[2] = System.Convert.ToByte((bi1 & 0x0000ff00) >> 8);
                byteData[3] = System.Convert.ToByte((bi1 & 0x000000ff));

                byteData[4] = System.Convert.ToByte(bi2 >> 24);
                byteData[5] = System.Convert.ToByte((bi2 & 0x00ff0000) >> 16);
                byteData[6] = System.Convert.ToByte((bi2 & 0x0000ff00) >> 8);
                byteData[7] = System.Convert.ToByte((bi2 & 0x000000ff));

                int n = Astream.Read(byteData, 8, byteData.Length - 8);

                while (n > 0)
                {
                    ClientSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), ClientSocket);
                    //Console.WriteLine("发送字节长度【{0}】", byteData.Length);
                    sendDone.WaitOne();
                    byteData = new byte[bufferSize];
                    n = Astream.Read(byteData, 0, byteData.Length);
                }
            }
            catch (Exception e)
            {
                OnErrorEvent(new ErrorEventArgs(e));
            }
        }

        /// 构造
        public MyTcpIpClient(System.ComponentModel.IContainer container)
        {
            container.Add(this);
            //InitializeComponent();

            //
            // TODO: 在 InitializeComponent 调用后添加任何构造函数代码
            //
        }

        /// 构造
        public MyTcpIpClient()
        {
            Conn();

            //连接完成后开始循环接收服务端返回数据
            while (true)
            {
                connectDone.WaitOne();
                StateObject Cstate = new StateObject(bufferSize, ClientSocket);
                ClientSocket.BeginReceive(Cstate.buffer, 0, bufferSize, 0,
                    new AsyncCallback(ReceiveCallback), Cstate);
                //Console.Write("receive "+cnt+": ");
                //for (int i = 0; i < 10; ++i)
                //    Console.Write(Cstate.buffer[i]);
                //Console.Write(".......");
                //for (int i = bufferSize - 10; i < bufferSize; ++i)
                //    Console.Write(Cstate.buffer[i]);
                Console.WriteLine("----------");
                //cnt = 0;
                Thread.Sleep(100);
            }
        }

        //#region Component Designer generated code
        ///// <summary>
        ///// 设计器支持所需的方法 - 不要使用代码编辑器修改
        ///// 此方法的内容。
        ///// </summary>
        //private void InitializeComponent()
        //{
        //    Conn();
        //}
        //#endregion


        /// 要连接的服务器IP地址
        public string TcpIpServerIP
        {
            get
            {
                return tcpIpServerIP;
            }
            set
            {
                tcpIpServerIP = value;
            }
        }

        /// 要连接的服务器所使用的端口
        public int TcpIpServerPort
        {
            get
            {
                return tcpIpServerPort;
            }
            set
            {
                tcpIpServerPort = value;
            }
        }

        /// 缓冲器大小
        public int BufferSize
        {
            get
            {
                return bufferSize;
            }
            set
            {
                bufferSize = value;
            }
        }

        /// 连接的活动状态
        public bool Activ
        {
            get
            {
                if (ClientSocket == null)
                {
                    return false;
                }
                return ClientSocket.Connected;
            }
        }

        /// 接收到数据引发的事件
        public event InceptEvent Incept;
        /// 引发接收数据事件
        protected virtual void OnInceptEvent(InceptEventArgs e)
        {
            if (Incept != null)
            {
                Incept(this, e);
            }
        }

        /// 发生错误引发的事件
        public event ErrorEvent Error;

        protected virtual void OnErrorEvent(ErrorEventArgs e)
        {
            if (Error != null)
            {
                Error(this, e);
            }
        }

    }

    /// 状态对象
    public class StateObject
    {
        /// 构造
        /// <param name="bufferSize">缓存</param>
        /// <param name="WorkSocket">工作的插座</param>
        public StateObject(int bufferSize, Socket WorkSocket)
        {
            buffer = new byte[bufferSize];
            workSocket = WorkSocket;
        }

        public byte []lt_record = new byte[5];//record length,type buffer

        /// 缓存
        public byte[] buffer = null;
        /// 工作插座
        public Socket workSocket = null;
        /// 数据流
        public Stream Datastream = new MemoryStream();


        public long packSize = -1;/// 数据包大小//-1表示还未收数据
        public int dataType = -1;

        public int cnt = 0;/// 已接受的数据长度
    }

    /// 接收数据事件
    public class InceptEventArgs : EventArgs
    {
        private readonly Stream datastream;
        private readonly Socket clientSocket;
        /// 构造
        /// <param name="Astream">接收到的数据</param>
        /// <param name="ClientSocket">接收的插座</param>
        public InceptEventArgs(Stream Astream, Socket ClientSocket)
        {
            datastream = Astream;
            clientSocket = ClientSocket;
        }
        /// 接受的数据流
        public Stream Astream
        {
            get { return datastream; }
        }
        /// 接收的插座
        public Socket ClientSocket
        {
            get { return clientSocket; }
        }
    }

    /// 定义接收委托
    public delegate void InceptEvent(object sender, InceptEventArgs e);
    /// 错处事件
    public class ErrorEventArgs : EventArgs
    {
        private readonly Exception error;
        /// 构造
        /// <param name="Error">错误信息对象</param>
        public ErrorEventArgs(Exception Error)
        {
            error = Error;
        }
        /// 错误信息对象
        public Exception Error
        {
            get { return error; }
        }
    }
    /// 错误委托
    public delegate void ErrorEvent(object sender, ErrorEventArgs e);



}
