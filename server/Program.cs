using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.Text;

namespace server
{
	class MainClass
	{
		static List<ClientSocket> ClientSocketList=new List<ClientSocket>();
		static Socket serverSocket;

		public static void Main (string[] args)
		{
			//输入ip和port
			Console.Write ("ip:");
			string host = Console.ReadLine ();
			Console.Write ("port:");
			string portstring=Console.ReadLine ();
			int port = int.Parse (portstring);
		
			//绑定ip和port
			IPAddress ip = IPAddress.Parse(host);
			IPEndPoint ipe = new IPEndPoint(ip,port);

			//生成服务器socket
			serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
			serverSocket.Bind(ipe);  
			serverSocket.Listen(10);    

			Console.WriteLine("等待客户端连接");

			//开启接受客户端链接的线程
			Thread AcceptsSocket=new Thread (Accepts);
			AcceptsSocket.Start();          
		}


		//接受客户端socket方法
		static void Accepts()
		{
			while(true)
			{
				Socket temps = serverSocket.Accept();
				ClientSocket temp=new ClientSocket(temps);
				ClientSocketList.Add(temp);


//				Thread receiveMessage = new Thread (receive);
				Thread receiveMessage = new Thread (new ThreadStart(delegate{receive(temp);}));

				receiveMessage.Start(temp);   
			}
		}


		//接收客户端消息方法
		static void receive(ClientSocket cs)
		{	
			while (true) 
			{	
				byte[] recvBytes = new byte[1024];	
				int ReceiveCount=cs.clientSocket.Receive (recvBytes, recvBytes.Length, 0);
				if (cs.isRsaVerify == false) 
				{
					if (ReceiveCount != 0)
					{
						cs.rsaPKey = Encoding.UTF8.GetString (recvBytes);
						RSACryptoServiceProvider provider = new RSACryptoServiceProvider ();
						provider.FromXmlString (cs.rsaPKey);
						byte[] bytes = Encoding.UTF8.GetBytes (cs.aesKey);
						//加密aes私钥
						string str = Convert.ToBase64String (provider.Encrypt (bytes, false));
						byte[] sss = Encoding.UTF8.GetBytes (str);
						//发送aes加密过的的私钥
						cs.clientSocket.Send (sss, sss.Length, 0);
						cs.isRsaVerify = true;
						Console.WriteLine ("建立连接");
					}
				} 
				else
				{				
					if (ReceiveCount != 0) 
					{
						//接收密文
						byte[] recvByte = new byte[ReceiveCount];
						for(int t=0;t<ReceiveCount;t++)
						{
							recvByte [t] = recvBytes [t];
						}
						string ciphertext = Convert.ToBase64String (recvByte);

						Decryptor d  = new Decryptor();
						Encryptor e = new Encryptor ();
						//解密消息
						string message=d.Decrypt(ciphertext,cs.aesKey);
						foreach(ClientSocket b in ClientSocketList)	
						{
							//用对应的key加密后再发出给每个客户端
							string ciphermessage = e.Encrypt (message, b.aesKey);
							Console.WriteLine (ciphermessage);
							byte[] sendBytes = Convert.FromBase64String (ciphermessage);
							b.clientSocket.Send(sendBytes, sendBytes.Length, 0);
						}
					}
				}
			}
		}
	}
}
