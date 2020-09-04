using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketAsync
{
    /// <summary>
    /// 享元模式 连接程序池
    /// </summary>
    public class SocketServer
    {
        Socket listener;
        AutoResetEvent allDone = new AutoResetEvent(false);

        public void StartListen()
        {
            EndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 30011);
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(endPoint);
            listener.Listen(10);
            while (true)
            {
                // Set the event to nonsignaled state.
                allDone.Reset();

                // Start an asynchronous socket to listen for connections and receive data from the client.
                Console.WriteLine("Waiting for a connection...");

                // Accept the connection and receive the first 10 bytes of data.
                // BeginAccept() creates the accepted socket.
                int receivedDataSize = 10;
                listener.BeginAccept(null, receivedDataSize, new AsyncCallback(AcceptReceiveDataCallback), listener);

                // Wait until a connection is made and processed before continuing.
                allDone.WaitOne();
            }
        }

        /// <summary>
        /// 回调
        /// </summary>
        /// <param name="ar"></param>
        public void AcceptReceiveDataCallback(IAsyncResult ar)
        {
            
            Socket listener = (Socket)ar.AsyncState;

            // End the operation and display the received data on the console.
            byte[] Buffer;
            int bytesTransferred;
            Socket handler = listener.EndAccept(out Buffer, out bytesTransferred, ar);

            //继续接受新的连接
            allDone.Set();

            string stringTransferred = Encoding.ASCII.GetString(Buffer, 0, bytesTransferred);

            Console.WriteLine(stringTransferred);
            Console.WriteLine("Size of data transferred is {0}", bytesTransferred);

            // Create the state object for the asynchronous receive.
            StateObject state = new StateObject();
            state.workSocket = handler;
            //初始连接 接受的信息不处理,只触发此操作
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);

        }


        public  void ReadCallback(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;
            so.sb.Clear();
            Socket s = so.workSocket;

            int read = s.EndReceive(ar);

            if (read > 0)
            {
                //流 如果缓冲区太小,需要处理分帧 接收不完 此处会继续接收
                so.sb.Append(Encoding.ASCII.GetString(so.buffer, 0, read));
                s.BeginReceive(so.buffer, 0, StateObject.BufferSize, 0,
                                         new AsyncCallback(ReadCallback), so);


                Console.WriteLine($"接收信息:{so.sb.ToString()}");
            }
            else
            {
                if (so.sb.Length > 1)
                {
                    //All of the data has been read, so displays it to the console
                    string strContent;
                    strContent = so.sb.ToString();
                    Console.WriteLine(String.Format("Read {0} byte from socket" +
                                     "data = {1} ", strContent.Length, strContent));
                }
                s.Close();
            }
        }

        public void ReceiveAsync(IAsyncResult asyncResult)
        {

        }

        public void Send(string msg)
        {
         
        }

        public void Send(byte[] byteData)
        {

        }
    }


    public class StateObject
    {
        public Socket workSocket = null;
        
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }
}
