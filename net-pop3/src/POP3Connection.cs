using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;

namespace Mono.Net.POP3
{
  public class POP3Connection
  {
    private TcpClient mailclient       = null;
    private NetworkStream ns	       = null;
    protected StreamReader BaseReader  = null;
    protected StreamWriter BaseWriter  = null;
    
    private bool connected    = false;
    private int port	      = 110;
    private string server     = "";
    private string username   = "";
    private string password   = "";
    
    
    public POP3Connection()
    {
      //Maybe something can go here?
    }  

    public POP3Connection(string sUsername, string sPassword, string sServer)
      : this (sUsername,sPassword,sServer,110)
    {
    }
    
    public POP3Connection(string sUsername, string sPassword, string sServer, int sPort)
    {
      username = sUsername;
      password = sPassword;
      server = sServer;
      port = sPort;
    }
    
    public bool Connected()
    {
      bool retval;
      if ( mailclient != null && connected != false )
	retval = true;
      else
	retval = false;
      return retval;
    }

    public string Username
    {
      set
      {
	if (!Connected()) 
	  username = value;
	else 
	  throw new System.MethodAccessException();
      }
      get
      {
	return username;
      }    
    }
    
    public string Password
    {
      set
      {
	if (!Connected()) 
	  password = value;
	else 
	  throw new System.MethodAccessException();
      }
      get
      {
	return password;
      }    
    }
	
    public string Server
    {
      set
      {
	if (!Connected()) 
	  server = value;
	else 
	  throw new System.MethodAccessException();
      }
      get
      {
	return server;
      }    
    }

    public int Port
    {
      set
      {
	if (!Connected()) 
	  port = value;
	else 
	  throw new System.MethodAccessException();
      }
      get
      {
	return port;
      }    
    }

  
    public void Open()
    {
      string response;
      mailclient = new TcpClient(server, port); //Blocking and may throw exception also
      
      ns = mailclient.GetStream(); // Lock and load :-)
      BaseReader = new StreamReader(ns);
      BaseWriter = new StreamWriter(ns);

      BaseReader.ReadLine(); //Ingore the POP3 opening banner
      
      BaseWriter.WriteLine("User " + username); //Send username;
      BaseWriter.Flush();

      response = BaseReader.ReadLine();
      if (response.Substring(0,1) == "-")
        throw new System.UnauthorizedAccessException();

      BaseWriter.WriteLine("Pass " + password); //Send password;
      BaseWriter.Flush();

      response = BaseReader.ReadLine();
      if (response.Substring(0,1) == "-")
      	throw new System.UnauthorizedAccessException();
      
      
      connected = true;      
    }

    public void Close()
    {
      BaseWriter.WriteLine("quit");
      BaseWriter.Flush();
      ns.Close();
    }
    
    public short MessageCount()
    {
      string response;
      bool disconnect = false;
      
      //If we are not connected then connect and disconnect when done
      if (!Connected()){
	disconnect = true;
	Open();
      }
      
      //Send stat command to get number of messages
      BaseWriter.WriteLine("stat");
      BaseWriter.Flush();

      response = BaseReader.ReadLine();
      string[] nummess = response.Split(' ');
      short totmessages;
      totmessages = Convert.ToInt16(nummess[1]);	
      
      if (disconnect)
	Close();
      
      return totmessages;
    }

    public POP3Message Top(short msgID)
    {
	return this.Top(msgID, 0);
    }
    
    ///<description>
    ///read header of the message
    ///</description>
    public POP3Message Top(short msgID,int lines)
    {
      string response;
      
      BaseWriter.WriteLine("top " + msgID.ToString() + " " +  lines.ToString());
      BaseWriter.Flush();
      StringBuilder sb = new StringBuilder();
      response = BaseReader.ReadLine();
      if ( response.StartsWith("-") )
	return null;
      else {
	//sb.Append(response);
        while ((response = BaseReader.ReadLine()).Trim() !=".")
	  sb.Append(response + "\n");
	return new POP3Message(sb.ToString());
      }
    }

    public POP3Message Retr(short msgID)
    {
      string response;
      
      BaseWriter.WriteLine("retr " + msgID.ToString());
      BaseWriter.Flush();
      StringBuilder sb = new StringBuilder();
      response = BaseReader.ReadLine();
      if ( response.StartsWith("-") )
	return null;
      else {
        while ((response = BaseReader.ReadLine()).Trim() !=".")
	  sb.Append(response + "\n");
	return new POP3Message(sb.ToString());
      }
	
    }


    public String RetrRaw(short msgID)
    {
        string response;

        BaseWriter.WriteLine("retr " + msgID.ToString());
        BaseWriter.Flush();
        StringBuilder sb = new StringBuilder();
        response = BaseReader.ReadLine();
        if (response.StartsWith("-"))
        {
            return null;
        }
        else
        {
            while ((response = BaseReader.ReadLine()).Trim() != ".")
            {
                sb.Append(response + "\n");
            }
            return sb.ToString();
        }
    }


    public bool Delete (short msgID)
    {
      string response;
      
      BaseWriter.WriteLine("dele " + msgID.ToString());
      BaseWriter.Flush();
      
      response = BaseReader.ReadLine();
      return response.StartsWith("-");
    }

    public POP3Message FirstMessage()
    {
      Int16[] q = this.List();
      return Retr((short) q[q.GetLowerBound(0)]);
    }
    
    public POP3Message LastMessage()
    {
      Int16[] q = this.List();
      return Retr((short) q[q.GetUpperBound(0)]);
    }

    public POP3Message[] GetAllMessages( bool delete )
    {
      Int16[] q = this.List();
      return GetMessageRange( q.GetLowerBound(0), q.GetUpperBound(0), delete );
    }
    
    public POP3Message[] GetHeaderRange( int min, int max, bool delete )
    {
      return GetMessageRange(min,max,delete,true);
    }
    
    public POP3Message[] GetMessageRange( int min, int max, bool delete )
    {
      return GetMessageRange(min,max,delete,false);
    }
    
    public POP3Message[] GetMessageRange( int min, int max, bool delete, bool headerOnly )
    {
      //string response;
      bool disconnect = false;
      
      //If we are not connected then connect and disconnect when done
      if (!Connected()){
	disconnect = true;
	Open();
      }
      Int16[] q = this.List();
      ArrayList msgBuff = new ArrayList();
      POP3Message msg;
      for (int i = min; i<max; i++)
      {
        try{
	  if (headerOnly)  msg = this.Top(q[i]);
	  else             msg = this.Retr(q[i]);
	  msgBuff.Add(msg);
	  if (delete) Delete((short)q[i]);
	}
	catch{ 
	  // this is bad but we are going to ignore errors for missing message now until list support
	}
      }

      if (disconnect)
        Close();

      return (POP3Message[]) msgBuff.ToArray(typeof(POP3Message));
    }

    public short[] List()
    {
      ArrayList list = new ArrayList();
      string response;
      
      BaseWriter.WriteLine("list");
      BaseWriter.Flush();
      response = BaseReader.ReadLine();
      if ( response.StartsWith("-") )
	return new short[] {}; //This is better then return an error
      else {
	//sb.Append(response);
        while ((response = BaseReader.ReadLine()).Trim() !=".")
	  list.Add(Convert.ToInt16(response.Split(' ')[0]));
	return (short[]) list.ToArray(typeof(short));
      }
      
    }
    
    public static short MessageCount(string sUsername, 
		    				string sPassword, 
						string sServer)
    {
      return MessageCount(sUsername, sPassword, sServer, 110);
    }

    
    public static short MessageCount(string sUsername, 
		    				string sPassword, 
						string sServer,
						int sPort)
    {
      POP3Connection conn = new POP3Connection(sUsername, sPassword, sServer, sPort); 
      return conn.MessageCount();
    }
    
    public static POP3Message[] GetHeaderRange(string sUsername, 
		    				string sPassword, 
						string sServer,
						int min, int max)
    {
      return GetMessageRange(sUsername, sPassword, sServer, 110, min, max, false, true);
    }

    public static POP3Message[] GetMessageRange(string sUsername, 
		    				string sPassword, 
						string sServer,
						int min, int max, bool delete)
    {
      return GetMessageRange(sUsername, sPassword, sServer, 110, min, max, delete, false);
    }
    
    
    public static POP3Message[] GetMessageRange(string sUsername, 
		    				string sPassword, 
						string sServer,
						int sPort,
						int min, int max, bool delete, bool HeaderOnly)
    {
      POP3Connection conn = new POP3Connection(sUsername, sPassword, sServer, sPort);
      return conn.GetMessageRange(min,max,delete,HeaderOnly);
    }
    
    public static POP3Message[] GetAllMessages(string sUsername, 
		    				string sPassword, 
						string sServer,
						bool delete)
    {
      return GetAllMessages(sUsername, sPassword, sServer, 110, delete);
    }
    
    public static POP3Message[] GetAllMessages(string sUsername, 
		    				string sPassword, 
						string sServer,
						int sPort,
						bool delete)
    {
      POP3Connection conn = new POP3Connection(sUsername, sPassword, sServer, sPort);
      return conn.GetAllMessages(delete);
    }
  }
  
}
