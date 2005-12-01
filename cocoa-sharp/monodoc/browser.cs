//
// browser.cs: Mono documentation browser
//
// Author:
//   Miguel de Icaza
//
// (C) 2003 Ximian, Inc.
//
// TODO:
//   Add support for printing.
//   Add search facility
//
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Web.Services.Protocols;
using System.Xml;
using System.Runtime.InteropServices;
using Apple.Foundation;
using Apple.AppKit;
using Apple.WebKit;

namespace Monodoc {
class Driver {

	
	static int Main (string [] args)
	{
		string topic = null;
		
		for (int i = 0; i < args.Length; i++){
			switch (args [i]){
			case "--html":
				if (i+1 == args.Length){
					Console.WriteLine ("--html needed argument");
					return 1; 
				}

				Node n;
				RootTree help_tree = RootTree.LoadTree();
				string res = help_tree.RenderUrl (args [i+1], out n);
				if (res != null){
					Console.WriteLine (res);
					return 0;
				} else {
					return 1;
				}
			case "--make-index":
				RootTree.MakeIndex ();
				return 0;
				
			case "--help":
				Console.WriteLine ("Options are:\n"+
						   "browser [--html TOPIC] [--make-index] [TOPIC] [--merge-changes CHANGE_FILE TARGET_DIR+]");
				return 0;
			
			case "--merge-changes":
				if (i+2 == args.Length) {
					Console.WriteLine ("--merge-changes 2+ args");
					return 1; 
				}
				
				ArrayList targetDirs = new ArrayList ();
				
				for (int j = i+2; j < args.Length; j++)
					targetDirs.Add (args [j]);
				
				EditMerger e = new EditMerger (
					GlobalChangeset.LoadFromFile (args [i+1]),
					targetDirs
				);

				e.Merge ();
				
				return 0;
			default:
				topic = args [i];
				break;
			}
			
		}
		Settings.RunningGUI = true;
		Browser browser = new Browser ();
		browser.Run();
		return 0;
	}
}

[Register("Controller")]
public class Controller : NSObject {

	[Connect]
	public NSDrawer drawer;
	[Connect]
	public NSOutlineView outlineView;
	[Connect]
	public WebView webView;
	[Connect]
	public NSSearchField searchBox;
	[Connect]
	public NSBrowser indexBrowser;
	[Connect]
	public NSMenuItem backMenuItem;
	[Connect]
	public NSMenuItem forwardMenuItem;

	static RootTree help_tree;

	static Controller() {
		help_tree = RootTree.LoadTree();
	}
	
	protected Controller (IntPtr raw, bool rel) : base(raw, rel) {}

	[Export("userDidSearch:")]
	public void UserDidSearch(object sender) {
		int index = IndexDataSource.FindClosest(searchBox.stringValue.ToString ());
		indexBrowser.selectRow_inColumn(index, 0);
	}

	[Export("applicationWillFinishLaunching:")]
	public void FinishLoading(NSNotification aNotification) {
		drawer.open();
		indexBrowser.target = this;
		indexBrowser.doubleAction = "browserdoubleAction";
		outlineView.target = this;
		outlineView.doubleAction = "doubleAction";
		// use history.
		webView.maintainsBackForwardList = true;
		webView.backForwardList.capacity = 100;
		forwardMenuItem.target = this;
		forwardMenuItem.action = "goForward:";
		backMenuItem.target = this;
		backMenuItem.action = "goBack:";
		Node match;
		string content = help_tree.RenderUrl("root:", out match);
		content=content.Replace("a href='", "a href='http://monodoc/load?");
		content=content.Replace("a href=\"", "a href=\"http://monodoc/load?");
		((WebFrame)webView.mainFrame).loadHTMLString_baseURL(content, null);
		addHistoryItem("root:");
	}
	
	[Export("browserdoubleAction")]
	public void browserDoubleAction() {
		IndexEntry entry = IndexDataSource.GetEntry(indexBrowser.selectedRowInColumn(0));
		if(entry != null) {
			Topic t = entry[0];
			Node match;
			string content = help_tree.RenderUrl(t.Url, out match);
			content=content.Replace("a href='", "a href='http://monodoc/load?");
			content=content.Replace("a href=\"", "a href=\"http://monodoc/load?");
			((WebFrame)webView.mainFrame).loadHTMLString_baseURL(content, null);
			addHistoryItem(t.Url);
		}
	}
	[Export("doubleAction")]
	public void outlineViewDoubleAction() {
		BrowserItem bi = outlineView.itemAtRow(outlineView.selectedRow) as BrowserItem;
		Console.WriteLine("Going to load {0}", bi);
		try {
			if(bi.node.URL != null)
			{
				Node n;
				string content = "";
				if(bi.node.tree != null && bi.node.tree.HelpSource != null)
					content = bi.node.tree.HelpSource.GetText(bi.node.URL, out n);
				if(content == null || content.Equals("") )
						content = help_tree.RenderUrl(bi.node.URL, out n);
				content=content.Replace("a href='", "a href='http://monodoc/load?");
				content=content.Replace("a href=\"", "a href=\"http://monodoc/load?");
				((WebFrame)webView.mainFrame).loadHTMLString_baseURL(content, null);
				addHistoryItem(bi.node.URL);

				outlineView.expandItem(bi);

			}
		} catch (Exception e) { Console.WriteLine("ERROR: " + e); }
	}

	[Export("webView:resource:willSendRequest:redirectResponse:fromDataSource:")]
	public NSURLRequest RequestHandler(WebView sender, object identifier, NSURLRequest initialRequest, NSURLResponse urlResponse, WebDataSource datasource) {
Console.WriteLine("\nDEBUG: URL=={0}\n", ((NSURL)initialRequest.urL).relativeString.ToString ());
		if ( ((NSURL)(initialRequest.urL)).relativeString.ToString().IndexOf("http://monodoc/load?") == 0) {
			string url = ((NSURL)initialRequest.urL).relativeString.ToString().Replace("http://monodoc/load?", "");
			string content = "";
			Node n;
			try {
				content = help_tree.RenderUrl(url, out n);
			} catch (Exception e) {
				content = "Exception Rendering the requested URL: " + e;
			}
			if(content != null && !content.Equals("")) {
				content=content.Replace("a href='", "a href='http://monodoc/load?");
				content=content.Replace("a href=\"", "a href=\"http://monodoc/load?");
Console.WriteLine("DEBUG: {0}", content);
				((WebFrame)webView.mainFrame).loadHTMLString_baseURL(content, null);
				addHistoryItem(url);
			}
			return null;
		}
		return initialRequest;
	}
	
	private void addHistoryItem(string url) {
		webView.backForwardList.addItem(new WebHistoryItem(url, "", 0));
	}
	
	private void loadHistoryItem(WebHistoryItem item) {
		string url = item.urlString;
		string content = "";
		Node n;
		try {
			content = help_tree.RenderUrl(url, out n);
		} catch (Exception e) {
			content = "Exception Rendering the requested URL: " + e;
		}
		if (content != null && !content.Equals("")) {
			content=content.Replace("a href='", "a href='http://monodoc/load?");
			content=content.Replace("a href=\"", "a href=\"http://monodoc/load?");
			webView.mainFrame.loadHTMLString_baseURL(content, null);
		}
	}
	
	[Export("validateMenuItem:")]
	public bool validateMenuItem(object sender) {
		NSMenuItem item = (NSMenuItem) sender;
		if (item.action.Equals("goBack:")) return webView.canGoBack;
		if (item.action.Equals("goForward:")) return webView.canGoForward;
		return true;
	}
	
	[Export("goBack:")]
	public void goBack(object sender) {
		WebBackForwardList history = webView.backForwardList;
		Console.WriteLine("webView.canGoBack = " + webView.canGoBack);
		Console.WriteLine("webView.backForwardList.backListCount = " + history.backListCount);
		if (history.backListCount > 0) {
			history.goBack();
			loadHistoryItem(history.currentItem);
		}
	}
	
	[Export("goForward:")]
	public void goForward(object sender) {
		Console.WriteLine("goForward:");
		WebBackForwardList history = webView.backForwardList;
		if (history.forwardListCount > 0) {
			history.goForward();
			loadHistoryItem(history.currentItem);
		}
	}
}

class Browser {
	public Browser() {}
	public void Run() {
Console.WriteLine ("initing: {0:x}", (int)Apple.Foundation.Class.Get("NSBundle"));
		Application.Init ();
Console.WriteLine ("initd");
		Application.LoadFramework ("WebKit");
		Application.LoadNib ("monodoc.nib");
		Application.Run ();
	}
}

class BrowserItem : NSObject {
	internal Node node;
	internal IList items = null;
	internal NSString caption;

	protected BrowserItem(IntPtr _ptr,bool release) : base(_ptr,release) {
Console.WriteLine("ERROR: BrowserItem.ctor(IntPtr,bool) is called: bad: Raw={0,8:x}", (int)_ptr);
	}
	public BrowserItem(Node _node) {
		node = _node;
		caption = new NSString (node.Caption);
		caption.retain ();
//Console.WriteLine("DEBUG: BrowserItem.ctor(" + node.Caption + ") is called: Raw{0,8:x}=", (int)Raw);
	}
	~ BrowserItem() {
Console.WriteLine("DEBUG: ~" + this + " Raw={0,8:x}", (int)Raw);
		SetRaw(IntPtr.Zero,false);
	}
	
	public int Count { 
		get { 
			if(node.Nodes == null)
				return 0;
			return node != null ? node.Nodes.Count : 0; 
		} 
	}
	public BrowserItem ItemAt(int ndx)
	{
		if (items == null && !node.IsLeaf) {
			items = new ArrayList();
			foreach (Node n in node.Nodes) 
				if (n != null) 
					items.Add(new BrowserItem(n));
		}
		return (BrowserItem)items[ndx];
	}
	public object ValueAt(object identifier)
	{
//Console.WriteLine("DEBUG: ValueAt: " + identifier + " for " + this);
		return caption;
	}
	public override string ToString()
	{
		return "BrowserItem: " + (node != null ? node.Caption : "null");
	}
}

[Register("IndexDataSource")]
class IndexDataSource : NSObject {
	static IndexReader index_reader;
	IndexEntry current_entry = null;

	static IndexDataSource() {
		index_reader = RootTree.LoadTree().GetIndex();
	}

	public IndexDataSource(IntPtr raw, bool rel) : base(raw, rel) {}

	public static IndexEntry GetEntry(int entry) {
		if(index_reader != null)
			return index_reader.GetIndexEntry(entry);
		else
			return null;
	}

	[Export("browser:numberOfRowsInColumn:")]
	public int NumberOfRowsInColumn(NSBrowser browser, int columnNumber) {
		if(index_reader == null)
			return 1;
		return index_reader.Rows;
	}
	[Export("browser:willDisplayCell:atRow:column:")]
	public void DisplayCell(NSBrowser browser, NSBrowserCell cell, int rowNumber, int columnNumber) {
		if(index_reader == null) 
			cell.stringValue = "Index Not Created";
		else
			cell.stringValue = index_reader.GetValue(rowNumber);
		cell.leaf = true;
	}

	public static int FindClosest (string text)
        {
		if(index_reader == null)
			return 1;

                int low = 0;
                int top = index_reader.Rows-1;
                int high = top;
                bool found = false;
                int best_rate_idx = Int32.MaxValue, best_rate = -1;

                while (low <= high){
                        int mid = (high + low) / 2;

                        //Console.WriteLine ("[{0}, {1}] -> {2}", low, high, mid);

                        string s;
                        int p = mid;
                        for (s = index_reader.GetValue (mid); s [0] == ' ';){
                                if (p == high){
                                        if (p == low){
                                                if (best_rate_idx != Int32.MaxValue){
                                                        //Console.WriteLine ("Bestrated: "+best_rate_idx);
                                                        //Console.WriteLine ("Bestrated: "+index_reader.GetValue(best_rate_idx));
                                                        return best_rate_idx;
                                                } else {
                                                        //Console.WriteLine ("Returning P="+p);
                                                        return p;
                                                }
                                        }

                                        high = mid;
                                        break;
                                }

                                if (p < 0)
                                        return 0;

                                s = index_reader.GetValue (++p);
                                //Console.WriteLine ("   Advancing to ->"+p);
                        }
                        if (s [0] == ' ')
                                continue;
                        int c, rate;
                        c = Rate (text, s, out rate);
                        //Console.WriteLine ("[{0}] Text: {1} at {2}", text, s, p);
                        //Console.WriteLine ("     Rate: {0} at {1}", rate, p);
                        //Console.WriteLine ("     Best: {0} at {1}", best_rate, best_rate_idx);
                        //Console.WriteLine ("     {0} - {1}", best_rate, best_rate_idx);
                        if (rate >= best_rate){
                                best_rate = rate;
                                best_rate_idx = p;
                        }
                        if (c == 0)
                                return mid;

                        if (low == high){
                                //Console.WriteLine ("THISPATH");
                                if (best_rate_idx != Int32.MaxValue)
                                        return best_rate_idx;
                                else
                                        return low;
                        }

                        if (c < 0){
                                high = mid;
                        } else {
                                if (low == mid)
                                        low = high;
                                else
                                        low = mid;
                        }
                }

                //              Console.WriteLine ("Another");
                if (best_rate_idx != Int32.MaxValue)
                        return best_rate_idx;
                else
                        return high;

        }
	public static int Rate (string user_text, string db_text, out int rate)
        {
                int c = String.Compare (user_text, db_text, true);
                if (c == 0){
                        rate = 0;
                        return 0;
                }

                int i;
                for (i = 0; i < user_text.Length; i++){
                        if (db_text [i] != user_text [i]){
                                rate = i;
                                return c;
                        }
                }
                rate = i;
                return c;
        }
}
[Register("BrowserDataSource")]
class BrowserDataSource : NSObject {

	internal RootTree help_tree;
	internal IList items = new ArrayList();

	public static BrowserItem BrowserItemForNode(Node n) {
		//WE NEED TO FIND A WAY TO DO THIS THAT ISN'T THIS EXPENSIVE
		/*
		foreach (BrowserItem bi in items) {
			if(bi.node == n)
				return bi;
			else
				BrowserItemForNode(bi.node);
		}*/
		return null;
	}

	public BrowserDataSource(RootTree _tree) {
		help_tree = _tree;
		foreach (Node node in help_tree.Nodes)
			items.Add(new BrowserItem(node));
Console.WriteLine("DEBUG: " + this + ".ctor Raw={0,8:x}", (int)Raw);
	}

	public BrowserDataSource(IntPtr raw, bool rel) : base(raw, rel) {
		help_tree = RootTree.LoadTree();
		foreach (Node node in help_tree.Nodes)
			items.Add(new BrowserItem(node));

	}
	~ BrowserDataSource () {
Console.WriteLine("DEBUG: ~" + this + " Raw={0,8:x}", (int)Raw);
	}

	[Export("outlineView:numberOfChildrenOfItem:")]
	public int OutlineViewNumberOfChildrenOfItem(NSOutlineView outlineView, object item)
	{
		BrowserItem bi = item as BrowserItem;
		int count = bi != null ? bi.Count : help_tree.Nodes.Count;
//Console.WriteLine("DEBUG: OutlineViewNumberOfChildrenOfItem: " + item + " --> " + count);
		return count;
	}

	[Export("outlineView:isItemExpandable:")]
	public bool OutlineViewIsItemExpandable(NSOutlineView outlineView, object item)
	{
		return OutlineViewNumberOfChildrenOfItem(outlineView,item) > 0;
	}

	[Export("outlineView:child:ofItem:")]
	public object OutlineViewChildOfItem(NSOutlineView outlineView, int index, object item)
	{
//Console.WriteLine("DEBUG: OutlineViewChildOfItem: " + index + ", item: " + item);
		BrowserItem bi = item as BrowserItem;
		if (bi != null)
			bi = bi.ItemAt(index);
		else
			bi = (BrowserItem)items[index];
		return bi;
	}

	[Export("outlineView:objectValueForTableColumn:byItem:")]
	public object OutlineViewObjectValueForTableColumnByItem(NSOutlineView outlineView, NSTableColumn tableColumn, object item)
	{
//Console.WriteLine("DEBUG: OutlineViewObjectValueForTableColumnByItem: " + item + ", for column: " + tableColumn.identifier);
		BrowserItem bi = item as BrowserItem;
		
		return bi == null ? null : bi.ValueAt(tableColumn.identifier);
	}
	
   }
}
