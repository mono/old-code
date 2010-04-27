using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace Mono.Net.POP3
{
  public class POP3Message
  {
    public readonly string To = "";
    public readonly string CC = "";
    public readonly string From = "";
    public readonly string ReplyTo = "";
    public readonly string Subject = "";
    public readonly string ContentType = "";
    public readonly string Status = "";
    public readonly string Date;
    public readonly StringDictionary Headers = new StringDictionary();
    public readonly string Message = "";
    
    public POP3Message(string rawMsg)
    {
#if DEBUG
      Console.WriteLine("Length: {0}", rawMsg.Length);
      Console.WriteLine("POP3Message creation");
      Console.Write("..........\n{0}", rawMsg);
#endif      
      
      StringReader reader = new StringReader(rawMsg);
      string currentKey = "";
      string currentItem = "";

      while ( true )
      {
	string hline = reader.ReadLine();
	if ( hline.StartsWith("\t") || hline.StartsWith(" ")){		
      	  currentItem += "\n" + hline.Trim();
	}
	else {
	  if (currentItem != "")
	  {
	    switch(currentKey.ToLower()){
	      case "to":
	        if (this.To.Length == 0)
	          this.To = currentItem;
	        else 
	          this.To += "\n" + currentItem;
	        break;
	      case "from":
	        if (this.From.Length == 0)
	          this.From = currentItem;
		else 
	          this.From += "\n" + currentItem;
	        break;
	      case "reply-to":
	      case "replyto":
	        if (this.ReplyTo.Length == 0)
	          this.ReplyTo = currentItem;
	        else 
	          this.ReplyTo += "\n" + currentItem;
	        break;
	      case "cc":
	        if (this.CC.Length == 0)
	          this.CC = currentItem;
	        else 
	          this.CC += "\n" + currentItem;
	        break;
	      case "subject":
	        this.Subject = currentItem;
	        break;
	      case "content-type":
	      case "contenttype":
	        this.ContentType = currentItem;
	        break;
	      case "date":
	        this.Date = currentItem;
	        break;
	    };    
	    if (Headers.ContainsKey(currentKey))
	      Headers[currentKey] += "\n\n" + currentItem;
	    else
	      Headers.Add(currentKey,currentItem);
	  }
	  if (hline.Trim() == "" || hline.Trim() == ".") 
	    break;
	  string[] items = hline.Split(new Char[] {':'},2);
	  currentKey = items[0];
	  currentItem = items[1];
	}
	if (hline.Trim() == "" || hline.Trim() == ".") 
	    break;
      }
      this.Message = reader.ReadToEnd();
    }
  }
}
