using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace server
{
	public class ClientSocket
	{
		public string aesKey;
		public string rsaPKey;
		public Socket clientSocket;
		public bool isRsaVerify=false;

		public ClientSocket ()
		{
			
		}

		public ClientSocket (Socket clientSocket)
		{
			this.clientSocket = clientSocket;
			aesKey=randString(20);
		}

		protected string randString(int len)
		{
			string str = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ~!@#$%^&*()_+";
			Random r = new Random();
			string result = string.Empty;

			for (int i = 0; i < len; i++)
			{
				int m = r.Next(0, 75);
				string s = str.Substring(m, 1);
				result += s;
			}

			return result;
		}

	}
}

