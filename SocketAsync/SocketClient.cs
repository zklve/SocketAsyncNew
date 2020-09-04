using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketAsync
{
    public class SocketClient
    {
        Socket client;

        public void Connect()
        {
            EndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 30011);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(endPoint);

            int receivedDataSize = 10;

            StateObject state = new StateObject();
            state.workSocket = client;

            // Begin receiving the data from the remote device.
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);

        }

        /// <summary>
        /// 回调
        /// </summary>
        /// <param name="ar"></param>
        public void AcceptReceiveDataCallback(IAsyncResult ar)
        {

            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


        }

        public void ReceiveCallback(IAsyncResult ar)
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
                                         new AsyncCallback(ReceiveCallback), so);

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
    }
}
