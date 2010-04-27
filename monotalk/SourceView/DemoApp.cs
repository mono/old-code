using Monotalk.SourceView;
using System;
using System.IO;
using System.Collections;
using Gtk;
using Gdk;
using Glade;
using Gnome;

class DemoApp : Program
{
    [GladeWidget("win")]
    Gtk.Window win;

    protected string config = null;
    protected string file   = null;

    public static void Main (string[] args)
    {
        new DemoApp (args).Run ();
    }

    public void ParseArgs (string[] args)
    {
	int i;

	for (i=0; i<args.Length; i++)
	{
	    if ( String.Compare("--config", args[i]) == 0 )
		config = args[++i];

	    else if ( String.Compare("--file", args[i]) == 0 )
		file = args[++i];
	    
	    else 
		file = args[i];

	}
    }
    

    protected void ConnectTextTagTable (Gtk.TextTagTable table, Monotalk.SourceView.Style [] styles)
    {
	foreach (Monotalk.SourceView.Style s in styles)
	{
            Gtk.TextTag tag = new TextTag(s.path);
            tag.Foreground  = s.color;

            table.Add ( tag );
	}
    }


    public DemoApp (string[] args, params object[] props) : base ("DemoApp", "0.1", Modules.UI, args, props)
    {
	ParseArgs(args);

        Glade.XML gxml = new Glade.XML ("sourceview.glade", "win", null);
        gxml.Autoconnect (this);

        if (win == null) throw new Exception("GladeWidgetAttribute is broken.");

	Gtk.TextView tw = (Gtk.TextView) gxml.GetWidget ("tw");

	System.IO.StreamReader stream = new StreamReader(this.file);
	string text = stream.ReadToEnd();
	tw.Buffer.Insert (tw.Buffer.EndIter, text);

Console.WriteLine("here we go");
        Config conf   = new Config ();
        Highlights hl = new Highlights (conf.patterns);

	ConnectTextTagTable(tw.Buffer.TagTable, conf.styles);

hl.DebugPrint();
	Token [] tokens = hl.Search(text);
	
	foreach (Token t in tokens)
	{
	    Gtk.TextIter siter, eiter;

	    tw.Buffer.GetIterAtOffset(out siter, t.sindex);
            tw.Buffer.GetIterAtOffset(out eiter, t.eindex);

	    //Console.WriteLine("*** {3} - {4}*** <{0}>{1}</{0}:{2}>", 
	    //t.style.name, tw.Buffer.GetText(siter, eiter, false), t.style.pattern, t.sindex, t.eindex + 1);

            tw.Buffer.ApplyTag ( tw.Buffer.TagTable.Lookup(t.style.path), siter, eiter);
	}
    }


    static void delete_event (object obj, EventArgs args)
    {
        Application.Quit ();
    }
}

