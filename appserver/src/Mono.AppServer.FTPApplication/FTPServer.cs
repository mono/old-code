//
// Mono.AppServer.FTPClientCollection
//
// Authors:
//   Pramod Singh (pramodkumarsingh@hotmail.com)
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Pramod Singh, 2001
// Copyright (C) Brian Ritchie, 2003
//
namespace FTP_Server
{
	using System;
	using System.Net;
	using System.Net.Sockets;
	using System.IO;
	using System.Text;
	using System.Threading;
	/// <summary>
	///    Server Implementing some of the FTP Protocols
	/// </summary>
	public delegate void CloseFTP();	
	public delegate bool Authenticate(int ClientID,string Username, string Password);
	public delegate void Connect(int ClientID,string msg);
	public delegate void Connected(int ClientID,string IP);
	public delegate void Disconnected(int ClientID,string IP);
	public delegate void Disconnect(int ClientID);
	public class FTPServer
	{
		private TcpListener		FTPCommandListner;
		private Socket			ClientSocket;
		private static int		ClientID=0;
		private string			ClientIP;
		private static int		intPort;
		protected int			_Port; // BLR
		protected string		_BaseDirectory; //BLR
		private bool			bClose=false;
		//Declare a Private Delegate that will be raised when client ask for Passive mode
		//for Data Transfer
		//private delegate Socket PassiveSocket(ref TCPListener clientDataListner);
		//Declare a reference to an Client delegate
		//Register the delegate to raise event for the Console update

		// Declare Events
		//		public event Connected OnMsgArrived;
		public event Connect OnConnect;
		public event Connected OnConnected;
		public event Disconnect OnDisconnect;
		public event Disconnected OnDisconnected;
		public event Authenticate OnAuthenticate;

		private void ClientConnect(int ClientID, string msg)
		{
			if (OnConnect!=null)
				OnConnect(ClientID,msg);
		}

		private void ClientConnected(int ClientID, string IP)
		{
			if (OnConnected!=null)
				OnConnected(ClientID,IP);
		}

		private void ClientDisconnect(int ClientID)
		{
			if (OnDisconnect!=null)
				OnDisconnect(ClientID);
		}

		private void ClientDisconnected(int ClientID, string IP)
		{
			if (OnDisconnected!=null)
				OnDisconnected(ClientID, IP);
		}

		private bool ClientAuthenticate(int ClientID,string Username, string Password)
		{
			if (OnAuthenticate!=null)
				return OnAuthenticate(ClientID,Username,Password);
			else
				return false;
		}

		public FTPServer()
		{
			
		}

		public void Dispose()
		{
			if (FTPCommandListner!=null)
			{
				bClose=true;
				Thread.CurrentThread.Join(1000);
				if(FTPCommandListner.Pending())
				{
					FTPCommandListner.Stop();
				}
				//FTPCommandListner.Stop();
			}
		}

		public int Port
		{
			get
			{
				return _Port;
			}
		}

		public string BaseDirectory
		{
			get
			{
				return _BaseDirectory;
			}
		}

		public bool Close
		{
			set
			{
				bClose=value;
			}
		}
		public Thread Start(int Port, string BaseDirectory)
		{
			_Port=Port;
			_BaseDirectory=BaseDirectory;
			Thread startFTPServer=new Thread(new ThreadStart(Run));
			startFTPServer.Start();
			return startFTPServer;
		}

		public void EndSession()
		{
			if (FTPCommandListner!=null)
			{
				bClose=true;
				FTPCommandListner = null;
			}
		}
		
		private void  Run()
		{
			Thread ClientThread;
			FTPCommandListner=new TcpListener(Port);
			FTPCommandListner.Start();
			try
			{
				while (true)
				{
					ClientSocket=FTPCommandListner.AcceptSocket();
					ClientThread=new Thread(new ThreadStart(FTPClientThread));
					ClientIP=ClientSocket.RemoteEndPoint.ToString();
					//Raise Event
					ClientID++;
					//					onMsgArrived(ClientID,ClientIP);
					// BLR
					ClientConnected(ClientID,ClientIP);
					//					Connected clientConnected=new Connected(Console.Connected);
					//					clientConnected(ClientThread,ClientIP);
					ClientThread.Start();
				}
			}
			catch(ThreadInterruptedException e)
			{
				Thread.CurrentThread.Abort();
			}
			catch(Exception e) //ThreadStopException e)
			{
				System.Console.WriteLine("Thread killed");
			}
		}

		private void FTPClientThread()
		{
			int Port=0;
			string user="",dir="/";
			string PresentDirOnFTP="/";
			string rootDirOnSystem=BaseDirectory; //BLR
			int ClientID =  FTPServer.ClientID;
			TcpListener		FTPDataListner=null;
			TcpClient		FTPDataClient=null;
			Socket ClientSocket=this.ClientSocket;
			Socket PassiveClientDataSocket=null;
			bool PassiveMode=false;
			bool IsLoggedIn=false;
			string serverMsg="";
			string strIP=ClientSocket.RemoteEndPoint.ToString();
			try
			{ 
				//Open a Buffered Network Stream for PI(Protocol Interpreter) IN.
				NetworkStream inBuffer=new NetworkStream(ClientSocket,FileAccess.Read);
				//Open a Buffered Network Stream for PI(Protocol Interpreter) OUT.
				NetworkStream outBuffer=new NetworkStream(ClientSocket,FileAccess.Write);
				//Open a Buffered Network Stream for DTP (Data Transfer Protocol).
				NetworkStream nwClientData;
				string oldFileName="";
				//Send Greeting to Client
				serverMsg="220 FTP server[FTP Server in C# by Pramod Kumar Singh]\r\n" ;
				SendMsg(serverMsg,ref outBuffer);
				bool done = false;
				while(!done)
				{  
					//break for some time
					//other wise with the loop the CPU is 100%
					Thread.Sleep(100);
					if(!bClose)
					{
						string clientMsg ;
						//Poll on the Socket to see do we have data to read
						clientMsg=ReadMsgFromBuffer(ref ClientSocket,ref inBuffer);
						string strSwitchExpression=clientMsg.Length==0?"": clientMsg.Substring(0,4).Trim();
						//Parse the FTP Command from Client.
						//Tested all the Commands here with Microsoft Command Prompt FTP .
						strSwitchExpression=strSwitchExpression.ToUpper();
						if (strSwitchExpression!="USER" && 
							strSwitchExpression!="PORT" && 
							strSwitchExpression!="QUIT" && 
							strSwitchExpression!="PASS" && !IsLoggedIn)
						{
							if (clientMsg.Length!=0)
								SendMsg("530 Please login with USER and PASS.\r\n",ref outBuffer);
						}
						else
						switch(strSwitchExpression)
						{
							case "USER":
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								user = clientMsg.Substring(4).Trim();
								SendMsg("331 Password\r\n",ref outBuffer);
								break;
							case "PASS":
								//BLR 
								//    Replaced: Console.ClientConnect(ClientID,clientMsg);
								//    Added: Authentication
								ClientConnect(ClientID,clientMsg);
								string password=clientMsg.Substring(4).Trim();
								if (ClientAuthenticate(ClientID,user,password))
								{
									SendMsg("230 User "+ user +" logged in\r\n",ref outBuffer);
									IsLoggedIn=true;
								}
								else
									SendMsg("530 User "+ user +" cannot log in.\r\n",ref outBuffer);
								break;
							case "XCWD":
								goto case "CWD";
							case "CDUP":
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								//Move up one directory
								string changeToDir ="..";
								SendMsg(ChangeDirectory(rootDirOnSystem,ref PresentDirOnFTP,changeToDir),ref outBuffer);
								break;
							case "XCUP":
								//Move up one directory
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								changeToDir ="..";
								SendMsg(ChangeDirectory(rootDirOnSystem,ref PresentDirOnFTP,changeToDir),ref outBuffer);
								break;
							case "CWD":
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								changeToDir =clientMsg.Substring(3).Trim();
								SendMsg(ChangeDirectory(rootDirOnSystem,ref PresentDirOnFTP,changeToDir),ref outBuffer);
								break;
							case "PORT":
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								PortData(clientMsg,out Port,out strIP);
								PassiveMode=false;
								SendMsg("200 PORT command successful\r\n",ref outBuffer); 
								break;
							case "PASV":
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								FTPDataListner=null;
								int intPassivePort=PassiveModePort(ref FTPDataListner);
								string strIPAddress=ClientSocket.RemoteEndPoint.ToString();
								//Remove the client Port
								strIPAddress=strIPAddress.IndexOf(":")>0?strIPAddress.Substring(0,strIPAddress.IndexOf(":")):strIPAddress;
								strIPAddress=strIPAddress.Replace('.',',');
								strIPAddress = strIPAddress + "," + intPassivePort / 256 + "," + (intPassivePort % 256);
								SendMsg("227 Entering Passive Mode (" + strIPAddress + ")\r\n",ref outBuffer);
								PassiveClientDataSocket=PassiveClientSocket(ref FTPDataListner,intPassivePort);	
								if(PassiveClientDataSocket==null)
								{
									SendMsg("425 Error in Passive Mode connection\r\n",ref outBuffer);
								}
								PassiveMode=true;
								break;
							case "TYPE":
								//Emulate the TYPE Command
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								SendMsg("200 type set\r\n",ref outBuffer);
								break;
							case "RETR":
								//Send the requested file to Client.
								//Raise event for the Console Window to register the request
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								SendMsg("150 Binary data connection\r\n",ref outBuffer);
								clientMsg = clientMsg.Substring(4).Trim();
								//Open the requested file for Transfer.
								clientMsg=clientMsg.Replace(@"\","/");
								clientMsg=(clientMsg.Substring(0,1)=="/"?clientMsg.Substring(1):clientMsg);
								string strPath = rootDirOnSystem+ PresentDirOnFTP;
								strPath=(strPath.Substring(strPath.Length-1,1)=="/"?strPath:strPath+"/");
								strPath+=clientMsg;
								nwClientData= Mode(PassiveMode,ref FTPDataClient,ref PassiveClientDataSocket,strIP,Port);
								if(SendFile(strPath,ref nwClientData))
								{
									SendMsg("226 transfer complete\r\n",ref outBuffer);
								}
								else
								{
									SendMsg("550 file not found, or no access.\r\n",ref outBuffer);
								}
								PassiveMode=false;
								break;
							case "STOR":
								//Create File on the FTP Server
								SendMsg("150 Binary data connection\r\n",ref outBuffer);
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								clientMsg= clientMsg.Substring(4).Trim();
								nwClientData= Mode(PassiveMode,ref FTPDataClient,ref PassiveClientDataSocket,strIP,Port);
								//Open the requested file for Transfer.
								clientMsg=clientMsg.Replace(@"\","/");
								clientMsg=(clientMsg.Substring(0,1)=="/"?clientMsg.Substring(1):clientMsg);
								strPath = rootDirOnSystem+ PresentDirOnFTP;
								strPath=(strPath.Substring(strPath.Length-1,1)=="/"?strPath:strPath+"/");
								strPath+=clientMsg;
								//Open the requested file for Transfer.
								if(CreateFile(strPath,ref nwClientData))
								{
									SendMsg("226 transfer complete \r\n",ref outBuffer);
								}
								else
								{
									SendMsg("550 file not found, or no access.\r\n",ref outBuffer);
								}
								PassiveMode=false;
								break;
							case "NLST":
								goto case "LIST";
							case "LIST":
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								SendMsg("150 ASCII data\r\n",ref outBuffer);
								nwClientData=Mode(PassiveMode,ref FTPDataClient,ref PassiveClientDataSocket,strIP,Port);
								ListDirectory(rootDirOnSystem,PresentDirOnFTP,ref nwClientData);
								//Close the socket to signal the End of Data Transfer
								if(!PassiveMode)
								{
									nwClientData.Close();
								}
								else
								{
									if(PassiveClientDataSocket!=null)
									{
										nwClientData.Close();
									}
								}
								SendMsg("226 Transfer complete.\r\n",ref outBuffer);
								break;
							case "XPWD":
								goto case "PWD"; 
							case "PWD":
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								SendMsg("257 " + "\"" + PresentDirOnFTP + "\"" + " is current directory \r\n" ,ref outBuffer);
								break;
							case "DELE":
								//Delete the file from the Server and reply back.
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								clientMsg= clientMsg.Substring(4).Trim();
								SendMsg(DeleteFileForServer(rootDirOnSystem,PresentDirOnFTP,clientMsg),ref outBuffer);
								break;
							case "XRMD":
								goto case "RMD";
							case "RMD":
								//Remove Directory
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								clientMsg= clientMsg.Substring(4).Trim();
								SendMsg(RemoveDirectory(rootDirOnSystem,PresentDirOnFTP,clientMsg),ref outBuffer);
								break;
							case "XMKD":
								goto case "MKD";
							case "MKD":
								//Make Directory
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								clientMsg= clientMsg.Substring(4).Trim();
								SendMsg(CreateDirectory(rootDirOnSystem,PresentDirOnFTP,clientMsg),ref outBuffer);
								break;
							case "NOOP":
								SendMsg("200 NOOP command executed.\r\n",ref outBuffer);
								break;
							case "HELP":
								//214 list of supported command
								break;
							case "ABOR":
								goto case "QUIT";
							case "QUIT":
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								if(PassiveClientDataSocket!=null)
								{
									PassiveClientDataSocket.Close();
								}
								if(FTPDataClient!=null)
								{
									FTPDataClient.Close();
								}
								SendMsg("221 GOOD BYE\r\n",ref outBuffer);
								done = true; 
								break;
							case "RNFR":
								//Rename File
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								oldFileName=FilePath(rootDirOnSystem,PresentDirOnFTP,clientMsg.Substring(4).Trim());
								if(File.Exists(oldFileName))
								{
									SendMsg("350 Requested file action pending further info \r\n",ref outBuffer);
								}
								else
								{
									SendMsg("550 Requested file not found\r\n",ref outBuffer);
									oldFileName="";
								}
								break;
							case "RNTO":
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								if(oldFileName!="")
								{
									string strNewFile=FilePath(rootDirOnSystem,PresentDirOnFTP,clientMsg.Substring(4).Trim());
									File.Copy(oldFileName,strNewFile);
									File.Delete(oldFileName);
								}
								SendMsg("250 Requested file action completed. \r\n",ref outBuffer);
								break;
							case "REST":
								SendMsg("350 Requested file action pending further info \r\n",ref outBuffer);
								break;
							
							case "": //We have recieved blank 
								break;
							default: //We Don't understand the Command
								ClientConnect(ClientID,clientMsg);
								//BLR Replaced: Console.ClientConnect(ClientID,clientMsg);
								SendMsg("500 Command not understood\r\n",ref outBuffer);
								break;
						}
					}
					else
					{
						done=true;
						SendMsg("221 Connection forcefull closed. \r\n",ref outBuffer);
						outBuffer.Close();
						inBuffer.Close();
					}
				}
				// BLR: replaced with events
				//				Disconnected clientDisconnted=new Disconnected(Console.Disconnected);
				//				clientDisconnted(Thread.CurrentThread,ClientSocket.RemoteEndPoint.ToString());
				ClientDisconnected(ClientID,ClientSocket.RemoteEndPoint.ToString());
				ClientSocket.Close();
			}
			catch(ThreadInterruptedException e)
			{
				//SendMsg("221 Connection forcefull closed. \r\n",ref outBuffer);
				//outBuffer.Close();
				//inBuffer.Close();
				// BLR: replaced with events
				//				Disconnected clientDisconnted=new Disconnected(Console.Disconnected);
				//				clientDisconnted(Thread.CurrentThread,ClientSocket.RemoteEndPoint.ToString());
				ClientDisconnected(ClientID,ClientSocket.RemoteEndPoint.ToString());
				bClose=false;
				Thread.CurrentThread.Abort();
			}
			catch (Exception e)
			{
				//System.out.println(e);
				ClientConnect(ClientID,e.ToString());
				//BLR Replaced: Console.ClientConnect(ClientID,e.ToString());
			}
			//At the End make the flag true;
			bClose=false;
		}
		//Interpreat the File Path
		private string FilePath(string rootDirOnServer,string PresentDirOfFTP,string fileName)
		{
			Thread oThread =Thread.CurrentThread;
			lock(oThread)
			{
				//Change to proper file format..
				fileName= fileName.Trim();
				rootDirOnServer=rootDirOnServer.Trim();
				PresentDirOfFTP=PresentDirOfFTP.Trim();
				rootDirOnServer=rootDirOnServer.Replace(@"\","/");
				PresentDirOfFTP=PresentDirOfFTP.Replace(@"\","/");
				fileName=fileName.Replace(@"\","/");
				//Remove the PresentDirOfFTP from filename
				string root="";
				string changeTo="";
				//???? Open Point
				if(fileName.LastIndexOf("/")>0)
				{
					//fileName=fileName.Remove(0,PresentDirOfFTP.Length);
					fileName=fileName.Substring(0,1)!="/"?"/" +fileName : fileName;
					root=rootDirOnServer + fileName;
					changeTo=fileName;
				}
				else
				{
					root=rootDirOnServer + PresentDirOfFTP;	
					if (root.Substring(root.Length-1,1)!="/")
					{
						root+="/";
					}	
					if(fileName.Substring(0,1)=="/")
					{
						fileName=fileName.Substring(1).Trim();
					}
					//Error Correction Logic
					//??
					string tmpRoot=root+fileName;
					try
					{
						if(Directory.Exists(tmpRoot))
						{
							//Change to this Directory
							root=tmpRoot;
						}
						else
						{
							//Not found try with System Root
							tmpRoot=fileName.Substring(0,1)!="/"?"/" +fileName : fileName;
							tmpRoot=rootDirOnServer + tmpRoot;
							if(Directory.Exists(tmpRoot))
							{
								root=tmpRoot;
							}
							else
							{
								//Directory doesn't exist...
								root+=fileName;
							}
						}
						
					}
					catch(Exception e)
					{
						//No Directory Exist
						root+=fileName;
					}
				}
				return root;
			}
		}
		//Rename the File
		private string RenameTheFile(string fileName)
		{
			Thread oThread=Thread.CurrentThread;
			lock(oThread)
			{
				try
				{
					if(File.Exists(fileName))
					{
					
					}
				}
				catch(Exception e)
				{
				}
				return null;
			}
		}
		//Remove Directory
		private string RemoveDirectory(string rootDirOnServer,string PresentDirOfFTP,string fileName)
		{
			Thread oThread=Thread.CurrentThread;
			lock(oThread)
			{
				rootDirOnServer=rootDirOnServer.Replace(@"\","/");
				PresentDirOfFTP=PresentDirOfFTP.Replace(@"\","/");
				fileName=fileName.Replace(@"\","/");
				string root="";
				root=rootDirOnServer + PresentDirOfFTP;
				if (root.Substring(root.Length-1,1)!="/")
				{
					root+="/";
				}	
				if(fileName.Substring(0,1)=="/")
				{
					fileName=fileName.Substring(1).Trim();
				}
				root+=fileName;
				try
				{
					if(Directory.Exists(root))
					{
						Directory.Delete(root,true);
						return "250 Directory deleted.\r\n";
					}
					else
					{
						return "550 Directory not found, or no access.\r\n";
					}
				}
				catch(Exception e)
				{
					return "550 Command can't be executed.\r\n";
				}
			}
		}
		//Create Directory Command
		private string CreateDirectory(string rootDirOnServer,string PresentDirOfFTP,string fileName)
		{
			Thread oThread =Thread.CurrentThread;
			lock(oThread)
			{
				//Change to proper file format..
				string root=FilePath(rootDirOnServer,PresentDirOfFTP,fileName);
				try
				{
					if(Directory.Exists(root))
					{
						return "550 Directory already exists!\r\n";
					}
					else
					{
						Directory.CreateDirectory(root);
						return "257 Directory Created.\r\n";
					}
				}
				catch(Exception e)
				{
					ClientConnect(ClientID,e.ToString());
					// BLR replaced: Console.ClientConnect(ClientID,e.ToString());
					return "550 Command can't executed.\r\n";
				}
			}
		}
		//Check for the directory
		private string ChangeDirectory(string rootDirOnServer,ref string PresentDirOfFTP,string fileName)
		{
			Thread oThread =Thread.CurrentThread;
			lock(oThread)
			{
				bool bUp =false;
				string root=FilePath(rootDirOnServer,PresentDirOfFTP,fileName);
				if(fileName.Length>=2)
				{
					if(fileName.Substring(0,2)==".." ||fileName.IndexOf("..")>0)
					{
						//change to parent dir..
						//int iPos=fileName.IndexOf("..");
						//string strChangeDir=fileName.Substring(iPos);
						if(PresentDirOfFTP.Length>1)
						{
							string temp="";
							string [] updir=fileName.Split(new char[] {'/'});
							int iDirectoryUp=updir.Length;
							int iStart=PresentDirOfFTP.Length;
							for(int i=0;i<iDirectoryUp;i++)
							{
								int iPosUnder = PresentDirOfFTP.LastIndexOf("/",iStart);
								temp = PresentDirOfFTP.Substring(0,iPosUnder);
								iStart=iPosUnder-1;
								if(iStart<=0)
								{
									break;
								}
							}
							//string reverse=PresentDirOfFTP;
							//StringBuilder t;
							fileName=temp;	
						}
						else if (PresentDirOfFTP.LastIndexOf("/")==0)
						{
							//We are at root
							fileName="";
						}
						root=rootDirOnServer + fileName;
						bUp=true	;
					}	
				}
				try
				{
					//Directory oDir=new Directory(root);	
					if(Directory.Exists(root))
					{
						//fileName=fileName.Substring(0,fileName.Length);
						if (PresentDirOfFTP.Substring(PresentDirOfFTP.Length-1,1)=="/")
						{
							PresentDirOfFTP=PresentDirOfFTP.Substring(0,PresentDirOfFTP.Length-1);
						}
						if(bUp)
						{
							PresentDirOfFTP=root.Remove(0,rootDirOnServer.Length);
							if(PresentDirOfFTP=="")
							{
								PresentDirOfFTP="/";
							}
						}
						else
						{
							PresentDirOfFTP=root.Remove(0,rootDirOnServer.Length);
						}
						return "250 CWD command succesful\r\n";
					}
					else
					{
						ClientConnect(ClientID,"Directory not found, or no access." + fileName);
						// BLR replaced: Console.ClientConnect(ClientID,"Directory not found, or no access." + fileName);
						return "550 Directory not found, or no access.\r\n";
					}
				}
				catch(IOException e)
				{
					ClientConnect(ClientID,e.ToString());
					// BLR replaced: Console.ClientConnect(ClientID,e.ToString());
					return "550 Directory not found, or no access.\r\n";
				}
			}
		}
		//Check for the FileName 
		private string DeleteFileForServer(string rootDirOnServer,string PresentDirOfFTP,string fileName)
		{
			//check for seperator
			Thread oThread =Thread.CurrentThread;
			lock(oThread)
			{
				string root=FilePath(rootDirOnServer,PresentDirOfFTP,fileName);
				try
				{
					FileInfo oFile=new FileInfo(root);	
					if(oFile.FullName!="")
					{
						oFile.Delete();
						return "250 delete command successful\r\n";
					}
				}
				catch(FileNotFoundException e)
				{
					ClientConnect(ClientID,e.ToString());
					//BLR replaced: Console.ClientConnect(ClientID,e.ToString());
					return "550 file not found, or no access.\r\n";
				}
				catch(IOException e)
				{
					ClientConnect(ClientID,e.ToString());
					//BLR replaced: Console.ClientConnect(ClientID,e.ToString());
					return "550 file not found, or no access.\r\n";
				}
				ClientConnect(ClientID,"Error in Deleteing file " + fileName);
				//BLR replaced: Console.ClientConnect(ClientID,"Error in Deleteing file " + fileName);
				return "550 file not found, or no access.\r\n";
			}
		}
		//Directory Listing
		private bool ListDirectory(string rootDirOnSystem,string PresentDirOnFTP ,ref NetworkStream nw)
		{
			//Open the Directory
			string strPath=rootDirOnSystem + PresentDirOnFTP;
			string strFileNameTemp="";
			DirectoryInfo oDir=new DirectoryInfo(strPath);
			FileInfo[] oFiles=oDir.GetFiles();
			DirectoryInfo[] oDirectories= oDir.GetDirectories();
			try
			{
				foreach(FileInfo oFile in oFiles)
				{
					string strFile;
					strFile="-rwxr--r-- 1 owner group ";
					try
					{
						if(oFile.Name.Substring(oFile.Name.Length-4)!=".SYS")
						{
							strFileNameTemp=oFile.Name.Replace(@"\","/");
							//strFileNameTemp=strFileNameTemp.Remove(0,strPath.Length);
							strFile+= oFile.Length + " " + oFile.LastWriteTime.ToString("MMM dd  yyyy") + " "  + strFileNameTemp.Trim() + "\r\n";
							byte[] Buffer=Encoding.ASCII.GetBytes(strFile);
							try
							{
								if(nw.CanWrite)
								{
									nw.Write(Buffer,0,Buffer.Length);
								}
							}
							catch(Exception e)
							{
								
							}
						}
					}
					catch(Exception e)
					{
						//Some Error in Reading the Files Information
					}
				}
				foreach(DirectoryInfo od in oDirectories)
				{
					string strDirectory;
					strDirectory ="drwxr-xr-x 1 owner group ";
					//BLR: if(od.IsDirectory)
				{
					strFileNameTemp=od.Name.Replace(@"\","/");
					//strFileNameTemp=strFileNameTemp.Remove(0,strPath.Length);
					strDirectory+="  0  " + "  " + od.CreationTime.ToString("MMM dd  yyyy")+ "  " + strFileNameTemp.Trim() +  "\r\n";
					byte[] Buffer=Encoding.ASCII.GetBytes(strDirectory);
					try
					{
						if(nw.CanWrite)
						{
							nw.Write(Buffer,0,Buffer.Length);
						}
					}
					catch(Exception e)
					{
							
					}
				}
				}
			}
			catch(IOException e)
			{
				ClientConnect(ClientID,e.ToString());
				// BLR replaced: Console.ClientConnect(ClientID,e.ToString());
				return false;
			}
			return true;
		}
		//Check For the MODE Passive or Port
		private NetworkStream Mode(bool PassiveMode,ref TcpClient client,ref Socket clientSocket,string strIP,int Port)
		{
			Thread oThread=Thread.CurrentThread;
			NetworkStream nw=null;
			lock (oThread)
			{
				if(PassiveMode)
				{
					//Socket is Opened
					if (clientSocket!=null)
					{
						nw=new NetworkStream(clientSocket,FileAccess.ReadWrite);
					}
				}
				else
				{
					//Client Need to be Connected
					client=new TcpClient(strIP,Port);
					nw=client.GetStream();
				}
			}
			return nw;
		}
		//Read Message from Network Stream
		private string ReadMsgFromBuffer(ref Socket clientSocket,ref NetworkStream inBuffer)
		{
			string clientMsg="";
			StringBuilder clientTmp=new StringBuilder();
			byte[] buffer=new Byte[1024];
			int iBytes=0;
			//inBuffer.Flush();
			Thread oThread=Thread.CurrentThread;
			lock(oThread)
			{
				string tmp="";
				/*
				iBytes=inBuffer.Read(buffer,0,buffer.Length);
				tmp=Encoding.ASCII.GetString(buffer,0,iBytes);
				clientTmp.Append(tmp);
				*/
				if(clientSocket.Available > 0)
				{
					while(clientSocket.Available>0)
					{
						iBytes=inBuffer.Read(buffer,0,buffer.Length);
						tmp=Encoding.ASCII.GetString(buffer,0,iBytes);
						clientTmp.Append(tmp);
					}
				}
				clientMsg=clientTmp.ToString();
			} 
			return clientMsg;
		}
		//Send Message through Network Stream
		private void SendMsg(string msg,ref NetworkStream outBuffer)
		{
			Thread oThread=Thread.CurrentThread;
			lock(oThread)
			{
				byte[] buffer;
				buffer=Encoding.ASCII.GetBytes(msg);
				outBuffer.Write(buffer,0,buffer.Length);
			}
		}

		//Read Data from File and Send to the Client
		private bool SendFile(string strpath,ref NetworkStream nw)
		{
			
			Thread oThread=Thread.CurrentThread;
			try
			{
				lock(oThread)
				{
					StreamReader outFile=new StreamReader(strpath);
					
					char[] buff=new Char[1024];
					int amount;
					while((amount = outFile.Read(buff,0,1024)) != 0)
					{				      
						byte[] buffer = Encoding.ASCII.GetBytes(buff);
						nw.Write(buffer,0,amount);
					}
				}
			}
			catch( Exception e )
			{
				ClientConnect(ClientID,e.ToString());
				// BLR replaced: Console.ClientConnect(ClientID,e.ToString());
				nw.Close();
				return false;
			} 
			nw.Close();
			return true;
		}

		//Write Data to File and Send to the Client
		private bool CreateFile(string strpath,ref NetworkStream nw)
		{
			Thread oThread=Thread.CurrentThread;
			lock(oThread)
			{
				try
				{
					StreamWriter inFile=new StreamWriter(strpath);
					byte[] buffer=new Byte[128];
					int iBytes=1;
					while(iBytes != 0)
					{
						string tmp="";
						iBytes=nw.Read(buffer,0,buffer.Length);
						char[] buff=Encoding.ASCII.GetChars(buffer);
						inFile.Write(buff,0,iBytes);
					}
					inFile.Close();
				}
				catch(Exception e )
				{
					nw.Close();
					return false;
				}
				nw.Close();
				return true;
			}
		}

		//Passive Data MODE
		private int PassiveModePort(ref TcpListener clientDataListner)
		{
			Thread oThread = Thread.CurrentThread;
			lock(oThread)
			{
				int intPort=0; 
				bool done=true;
				while (done)
				{
					intPort=NewPort();
					try
					{
						if(clientDataListner!=null)
						{
							clientDataListner.Stop();
						}
						clientDataListner=new TcpListener(intPort);	
						clientDataListner.Start();
						//This Port is free
						done=false;
					}
					catch (Exception e)
					{
						//done=false;
					}
				}
				return intPort;
				/*
				strIPAddress = CStr(wscControl.LocalIP)
				strSend = "PORT " & Replace(strIPAddress, ".", ",")
				strSend = strSend & "," & intPort \ 256 & "," & (intPort Mod 256)
				strSend = strSend & vbCrLf
				wscControl.SendData strSend
				*/
			}
		}
		
		//Passive Mode Data Transfer Listner.
		private Socket PassiveClientSocket(ref TcpListener clientDataListner,int intPort)
		{
			Thread oThread=Thread.CurrentThread;
			lock(oThread)
			{
				try
				{
					if(clientDataListner.LocalEndpoint==null)
					{
						bool done=false;
						Socket s=null;
						try
						{
							s= clientDataListner.AcceptSocket();	
							done=true;
						}
						catch(Exception e)
						{
							
						}
						return s;	
					}
					else
					{
						Socket s= clientDataListner.AcceptSocket();
						return s;	
					}
				}
				catch(Exception e)
				{
					ClientConnect(ClientID,e.ToString());
					// BLR replaced: Console.ClientConnect(ClientID,e.StackTrace);
				}
				return null;
			}
		}
		//Get Free Port Numeber from Operating System
		private int NewPort()
		{
			if(intPort==0)
			{
				intPort = 1100;
			} 
			else
			{
				intPort++;
			}
			return  intPort;
		}
		//Process PORT Command and get the IP and Port to Connect 
		//for Data Transfer
		private void PortData(string clientMsg,out int Port,out string strIP)
		{
			Thread oThread=Thread.CurrentThread;
			lock (oThread)
			{
				//string strDataAddress = clientMsg.Replace(',', '.', 1, 3);
				Port=0;
				//Remove the FTP Command
				clientMsg=clientMsg.Remove(0,5);
				//Remove the last two character
				clientMsg=clientMsg.Substring(0,clientMsg.Length-2);
				int iLen =clientMsg.Length;
				int iCounter=0;
				StringBuilder IP=new StringBuilder();
				string clientPort="";
				for(int i=0;i<iLen;i++)
				{
					if (clientMsg.Substring(i,1)==",")
					{
						iCounter++;
						if(iCounter==4)
						{
							clientPort=clientMsg.Substring(i+1);
							break;
						} 
						else
						{
							IP.Append(clientMsg.Substring(i,1));
						}
					}
					else
					{
						IP.Append(clientMsg.Substring(i,1));
					}
				}
				//IP of the Client
				strIP=IP.ToString().Replace(',','.');
				string[] strPort=clientPort.Split(new Char[]{','});
				if (strPort.Length>0)
				{
					Port =(int)(Decimal.Parse(strPort[0]) * 256 + Decimal.Parse(strPort[1]));
				}
				/*
				iPos = InStr(1, strDataAddress, ",")
				strIP = Left$(strDataAddress, iPos - 1)
				lPort = CLng(Mid$(strDataAddress, iPos + 1, InStr(iPos + 1, strDataAddress, ",") - iPos))
				lPort = lPort * 256
				lPort = lPort + CLng(Mid$(strDataAddress, InStrRev(strDataAddress, ",") + 1))
				*/
			}
		}
	}
}
