// ActionLog.cs -- data logged about build operations and user commands

using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;

using Mono.Build;

namespace Monkeywrench {

    [Serializable]
    public class ActionLog : IBuildLogger {
	public static bool DebugEvents;

	[Serializable]
	public struct LogItem {
	    public string Category;
	    public string Message;
	    public string ExtraInfo;

	    public override string ToString ()
	    {
		if (ExtraInfo == null)
		    return String.Format ("[{0}]: {1}", Category, 
					  Message);

		return String.Format ("[{0}]: {1} ({2})", Category, 
				      Message, ExtraInfo);
	    }
	}

	LogItem[] items;
	int nextitem;

	void AppendItem (string category, string message, object extra) 
	{
	    items[nextitem].Category = category;
	    items[nextitem].Message = message;

	    if (extra != null)
		items[nextitem].ExtraInfo = extra.ToString ();
	    else
		items[nextitem].ExtraInfo = null;

	    if (DebugEvents)
		Console.WriteLine ("{0}", items[nextitem]);

	    if (++nextitem >= items.Length)
		nextitem = 0;
	}

	class ItemEnumerator : IEnumerable, IEnumerator {
	    ActionLog owner;
	    int idx;
	    int saved_nextitem;
	    bool first;

	    public ItemEnumerator (ActionLog owner)
	    {
		this.owner = owner;

		saved_nextitem = owner.nextitem;
		Reset ();
	    }

	    public IEnumerator GetEnumerator ()
	    {
		return this;
	    }

	    void Check ()
	    {
		if (owner.nextitem != saved_nextitem)
		    throw new InvalidOperationException ("Iterator sync error");
	    }

	    // items[saved_nextitem - 1] is the most recent item
	    // in the loop buffer.
	    // items[saved_nextitem] is the oldest item, so long
	    // as nothing is appended to log while we're iterating
	    // If the items[] array is not full, then the empty
	    // parts will be consecutive starting at 
	    // items[saved_nextitem] up until items[items.Length - 1]

	    public void Reset ()
	    {
		Check ();
		idx = saved_nextitem;

		if (owner.items[idx].Category == null)
		    // If there are any empty slots, they must
		    // be continguous to the end of the array,
		    // we have less than 40 items, and the 
		    // chronologically first item is just ...
		    idx = 0;

		// Position ourselves before it.
		idx--;

		first = true;
	    }

	    public bool MoveNext ()
	    {
		Check ();
		idx++;

		if (idx >= owner.items.Length)
		    idx = 0;

		if (!first)
		    return idx != saved_nextitem;

		first = false;
		return owner.items[idx].Category != null;
	    }

	    public object Current {
		get {
		    Check ();
		    return owner.items[idx];
		}
	    }
	}

	public IEnumerable SavedItems {
	    get { return new ItemEnumerator (this); }
	}

	// IBuildLogger.

	public void Log (string category, string value, object extra) 
	{
	    AppendItem (category, value, extra);
	}

	public void Log (string category, string value) 
	{
	    AppendItem (category, value, null);
	}

	// IWarningLogger
      
	[NonSerialized] IWarningLogger uilog;

	public void PushLocation (string loc)
	{
	    uilog.PushLocation (loc);
	}

	public void PopLocation ()
	{
	    uilog.PopLocation ();
	}

	string WarnErrMessage (int category, string text, string detail)
	{
	    string loc = uilog.Location;

	    if (loc != null && loc.Length > 0)
		return String.Format ("{0}: {1}: {2}", loc, category, text);

	    return String.Format ("{0}: {1}", category, text);
	}

	public void Warning (int category, string text, string detail)
	{
	    uilog.Warning (category, text, detail);

	    Log ("warning", WarnErrMessage (category, text, detail), detail);
	}

	public void Error (int category, string text, string detail)
	{
	    uilog.Error (category, text, detail);

	    Log ("error", WarnErrMessage (category, text, detail), detail);
	}

	public string Location { get { return uilog.Location; } }

	public int NumErrors { get { return uilog.NumErrors; } }

	// Loading and saving.

	ActionLog (int numitems) 
	{
	    this.items = new LogItem[numitems];
	    this.nextitem = 0;

	    // UIlog is set in Load to merge two 
	    // potential code paths.
	}

	ActionLog () : this (40) {}

	public const string LogName = "eventlog.dat";

	public static ActionLog Load (SourceSettings ss, IWarningLogger uilog)
	{
	    ActionLog ld = null;

	    if (uilog == null)
		throw new ArgumentNullException ();

	    string path = ss.PathToStateItem (LogName);

	    if (File.Exists (path))
		ld = (ActionLog) SafeFileSerializer.Load (path, uilog);

	    if (ld == null)
		// Didn't exist or failed to recover
		ld = new ActionLog ();

	    ld.uilog = uilog;
	    return ld;
	}

	public bool Save (SourceSettings ss)
	{
	    return SafeFileSerializer.Save (ss.PathToStateItem (LogName), this, uilog);
	}
    }
}
