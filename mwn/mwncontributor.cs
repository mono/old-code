using System;
using System.Xml;
using System.Text;
using System.Web.Mail;

namespace MonoWeeklyNews {
public class Contributor {
	static private string subject, body, contrib;

	public static void Main (string[] args)
	{
		string title;
		string response = null;
		bool send = false;
		string contrib = @"NoTe LoDigo <note@lod.igo>";
		int resp;
		
		if (args.Length > 0)
			if (args[0] == "--send")
				send = true;
	GETINPUT:
		Console.Write ("Enter subject: ");
		subject = Console.ReadLine ();
		Console.WriteLine ("Enter body (use a single line in HTML):");
		body = Console.ReadLine ();
		Console.WriteLine ("Enter your name followed by your email (default 'NoTe LoDigo <note@lod.igo>')");
		contrib = Console.ReadLine ();
		Print ();
		Console.Write ("Is that ok? (Y/n): ");
		response = (Console.ReadLine ()).ToUpper ();
		if (response == "N" || response == "NO")
			goto GETINPUT;
		title = SaveToFile ();
		if (send) {
			string server;
			Console.WriteLine ("Which server do you want to use to send the email (default 'localhost')?");
			if ((server = Console.ReadLine ()).Length < 2)
				server = null;
			resp = SendMail (contrib, title, server, title.Substring (0, 8) + ".mwnitem");
			if (resp == 1)
				Console.WriteLine ("Contribution called " + title + ".mwnitem sent to <jaime@gnome.org> \nThanks " + contrib + "!");
			else
				Console.WriteLine ("An error ocurred while trying to send email. Sorry. Send the file using another tool.");
		} else
			Console.WriteLine ("Send the file called " + title + ".mwnitem to me at <jaime@gnome.org> \nThanks " + contrib + " for contributing!");
	}

	private static void Print ()
	{
		Console.WriteLine ("Your candidate for MWN item is:\n");
		Console.WriteLine ("Subject: " + subject);
		Console.WriteLine ("Body:\n\n" + @body);
	}

	public static void SaveToFile (string inSubject, string inBody, string inContributor)
	{
		Subject = inSubject;
		Body = inBody;
		Contrib = inContributor;
		SaveToFile ();
	}
	
	public static string SaveToFile ()
	{
		string title;
		string[] fragments;
		XmlDocument doc;
		XmlElement item;
		XmlAttribute subject_attr, body_attr, contrib_attr;
		
		fragments = subject.Split (' ');
		title = null;
		foreach (string s in fragments)
		{
			title += s;
		}
		doc = new XmlDocument ();
		doc.AppendChild (doc.CreateProcessingInstruction ("xml", "version='1.0'"));
		item = doc.CreateElement ("mwnitem");
		subject_attr = doc.CreateAttribute ("subject");
		subject_attr.Value = subject;
		body_attr = doc.CreateAttribute ("body");
		body_attr.Value = @body;
		contrib_attr = doc.CreateAttribute ("contrib");
		contrib_attr.Value = @contrib;
		item.Attributes.Append (subject_attr);
		item.Attributes.Append (body_attr);
		item.Attributes.Append (contrib_attr);
		doc.AppendChild (item);
		doc.Save ((title = title.Substring (0, 8)) + ".mwnitem");
		return title;
	}

	public static byte SendMail (string sender, string title, string server, string file)
	{
		byte response;
		MailMessage msg = new MailMessage ();
		MailAttachment nitem = new MailAttachment (file);
		msg.From = sender;
		msg.Subject = "MWN Contribution: " + title;
		msg.To = "jaime@gnome.org";
		msg.Attachments.Add (nitem);
		if (server != null)
			SmtpMail.SmtpServer = server;
		Console.WriteLine ("Sending...");
		try {
			SmtpMail.Send (msg);
			response = 0;
		} catch (Exception e) {
			Console.WriteLine (e);
			response = 1;
		}
		return response;
	}
	
	public static string Subject { set { subject = value; } }
	public static string Body { set { body = value; } }
	public static string Contrib { set { contrib = value; } }
}
}
