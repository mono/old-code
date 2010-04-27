namespace Monotalk.SourceView
{
    using System;
    using System.Collections;

    public class Token
    {
	public int sindex;
	public int eindex;
	public Style style;

	public Token (Pattern pattern, int eidx)
	{
	    this.sindex = eidx - pattern.pattern.Length + 1;
	    this.eindex = eidx + 1;
	    this.style  = pattern.style;
	}

	public Token (Pattern pattern, int sidx, int eidx)
	{
	    this.sindex = sidx - pattern.pattern.Length + 1;
	    this.eindex = eidx + 1;
	    this.style  = pattern.style;
	}
    }


    public class Highlights : Trie
    {
	protected int [] fail;
	protected Pattern [] patterns;


	public Highlights (Pattern[] patterns) : base()
	{
	    string[] patternTexts = new string [patterns.Length];

            for (int i = 0; i < patterns.Length; i ++)
		    patternTexts [i] = patterns [i].pattern;

	    BuildTrie (patternTexts);
	    BuildFail();

	    this.patterns = patterns;
	}


	protected int SpansTo (Pattern pattern, string buffer, int soffset)
	{
            bool found;

	    for (int i=soffset; i < buffer.Length - pattern.spanto.Length; i++)
	    {
		found = true;

		for (int j=0; j<pattern.spanto.Length; j++)
		{
		    if ( pattern.spanto[j] != buffer[i+j] ) 
		    {
		        found = false;
		        break;
		    }
		 }

		if ( found ) 
		{
			if ( pattern.signore == null )
		    		return i + pattern.spanto.Length - 1;

			if ( pattern.signore[0] == pattern.spanto[0] )
			{
				int k = i+1;

				for (int j=pattern.signore.Length - pattern.spanto.Length; j<pattern.signore.Length; j++)
					if ( k >= buffer.Length || buffer[k++] != pattern.signore[j] ) 
						return i + pattern.spanto.Length - 1;

				i = k-1;
			}

			else 
			{
				int k = i;

				for (int j=pattern.signore.Length - 1; j>=0; j--)
					if ( k < 0 || buffer[k--] != pattern.signore[j] )
						return  i + pattern.spanto.Length - 1;
			}
		}
	    }
              
            return -1;
	}


	public Token[] Search (string text)
	{
	    ArrayList matches = new ArrayList();

	    int q = 0;
	    int fin;

	    for (int i=0; i < text.Length; i++)
	    {
		int next = Next(q, text[i], out fin);

		//Console.Write("{0}: from state {1} via {2} to {3}", i, q, text[i], next);
		//if ( fin != 0 ) Console.Write(" <fin>");
		//Console.Write("\n");
		if ( fin != 0 )
		{
		    if ( patterns[-fin-1].FalseAlarm(text, i) )
		    {
			    ;//Console.WriteLine("false alarm {0} at pos {1}, <{2} via {3} to {4}>", -fin, i, q, text[i], next);
		    }
		    else
		    {
			int s = -fin-1;

			if ( patterns[s].spanto != null )
			{
			    int j = SpansTo(patterns[s], text, i+1);

			    if ( j == -1 ) j = text.Length - 1;

			    //Console.WriteLine("{0} spans {1} - {2}", patterns[s].pattern, i, j);
			    matches.Add( new Token(patterns[s], i, j) );

			    i = j + 1;
			}
			else
			    matches.Add( new Token(patterns[s], i) );
		    }
	}

		if ( next >= 0 )
		{
		    q = next;
		    continue;
		}

		while ( NextNonfinal(q, text[i]) < 0 )
		    q = fail[q];

		i--;
	    }

	    return (Token []) matches.ToArray(typeof(Token));
	}


	public int Next (int state, char value, out int final)
	{
	    int result = -1;

	    int from = idx[state];
	    int to   = ( state+1 < idx.Length ) ? idx[state+1] : next.Length;

	    final = 0;

	    for (int i=from; i<to; i++)
		if ( via[i] == value )
		{
		    if ( next[i] < 0 ) 
			final  = next[i];
		    else 
			result = next[i];
		}

	    if ( result == -1 && state == 0 ) return 0;

	    return result;
	}    


	public int NextNonfinal (int state, char value)
	{
	    int from = idx[state];
	    int to   = ( state+1 < idx.Length ) ? idx[state+1] : next.Length;

	    for (int i=from; i<to; i++)
		if ( via[i] == value && next[i] >= 0 ) return next[i];

	    if ( state == 0 ) return 0;

	    return -1;
	}    


	protected void BuildFail()
	{
	    fail = new int [idx.Length];

	    int from = idx[0];
	    int to   = ( 1 < idx.Length ) ? idx[1] : next.Length;

	    for (int i=from; i<to; i++)
		fail[i] = 0;

	    Queue fifo = new Queue (idx.Length);

	    fifo.Enqueue (0);

	    while ( fifo.Count != 0 )
	    {
		int up = (int) fifo.Dequeue();

		from   = idx[up];
		to     = ( up+1 < idx.Length ) ? idx[up+1] : next.Length;

		for (int i=from; i<to; i++)
		{
		    if ( next[i] >= 0 ) 
		    {
			if ( up != 0 ) BuildFail(up, next[i], via[i]);
			fifo.Enqueue(next[i]);
		    }
		}
	    }
	}


	protected void BuildFail(int prev, int state, char value)
	{
	    int q = fail[prev];

	    while ( NextNonfinal(q, value) < 0 )
		q = fail[q];

	    fail [state] = NextNonfinal(q, value);
	}


	new public void DebugPrint()
	{
	    for (int i=0; i<patterns.Length; i++)
		Console.WriteLine("{0} = {1}", i+1, patterns[i].pattern);
	    
	    for (int i=0; i<next.Length; i++) 
		Console.WriteLine("next[{0}] = {1} \t via[{0}] = {2}", i, next[i], via[i]);

	    for (int i=0; i<idx.Length; i++) 
		Console.WriteLine("idx[{0}] = {1} \t fail[{0}] = {2}", i, idx[i], fail[i]);

/*
	    int j = 0;
	    for (int i=0; i<next.Length; i++)
	    {
		if ( j < idx.Length && i >= idx[j] )
		{
		    Console.WriteLine("{0}: ", j);
		    Console.WriteLine("   if fail -> {0}", fail[j]);

		    j++;
		}

		Console.WriteLine("   {0} via {1}", next[i], via[i]);
	    }
*/
	}
    }


    internal class State
    {
	public int  id;
	public int  next;
	public char value;
	public bool final;

	protected static int next_id = 0;

	public State (char value)
	{
	    this.id     = next_id++;
	    this.value  = value;
	    this.next   = -1;
	    this.final  = false;
	}

	public State (int id, char value)
	{
	    this.id     = id;
	    this.value  = value;
	    this.next   = -1;
	    this.final  = false;
	}
    }	


    public class Trie
    {
	protected int  [] idx;
	protected int  [] next;
	protected char [] via;

	public Trie () {}

	public Trie (string [] strings)
	{
	    BuildTrie (strings);
	}

	protected void BuildTrie (string [] strings)
	{
	    DynamicTrie dt = new DynamicTrie (strings);

	    idx  = new int  [dt.idx.Count];
	    next = new int  [dt.list.Count];
	    via  = new char [dt.list.Count];

	    State curr = (State) dt.list[0];
	    int i = 0;
	    int j = 0;

	    idx[0] = 0;

	    while (j < dt.list.Count)
	    {
		State s = (State) dt.list [j];

		next[j] = s.next;
		via[j]  = s.value;

		if ( s.id != curr.id )
		{
		    idx[++i] = j;
		    curr = s;
		}

		j++;
	    }

//	    DebugPrint();
	}


    	protected void DebugPrint()
	{
	    int j = 0;
	    for (int i=0; i<next.Length; i++)
	    {
		if ( j < idx.Length && i >= idx[j] )
		{
		    Console.WriteLine("{0}: ", j);
		    j++;
		}

		Console.WriteLine("   {0} via {1}", next[i], via[i]);
	    }
	}
    }
    


    internal class DynamicTrie 
    {
	public ArrayList list = new ArrayList ();
	public ArrayList idx  = new ArrayList ();

	public DynamicTrie (string [] strings)
	{
	    for (int i=0; i < strings.Length; i++)
		Insert (strings[i], i+1);
	}

	
	protected void DebugPrint ()
	{
	    for (int i=0; i<list.Count; i++)
	    {
		State s = (State) list[i];
		Console.Write("list[{0}] --> {1} {2} {3}", i, s.id, s.value, s.next);
		if ( s.final ) Console.Write(" <fin>");
		Console.WriteLine("");
	    }
	}


	protected int GetNext (State state, char value)
	{
	    int i = list.IndexOf(state);

	    while ( i < list.Count && state.id == ((State) list[i]).id )
		if ( value != ((State) list[i]).value || ((State) list[i]).next < 0 ) 
		    i++;
		else 
		    return ((State) list[i]).next; 

	    return -1;
	}


	protected void Insert (string text, int pattern_no)
	{
	    if ( idx.Count == 0 )
	    {
		State prev = null;

		for (int i=0; i < text.Length; i++)
		{
		    State s = new State (text[i]);

		    idx.Add (s);
		    list.Add (s);

		    if (prev != null) prev.next = s.id;

		    prev = s;
		}

		prev.final = true;
		prev.next  = -pattern_no;

		return;
	    }

	    int state = 0;
	    int y = 0;

	    while ( y < text.Length )
	    {
		int next = GetNext ((State) idx[state], text[y]);

		if ( next < 0 )
		{
		    AppendSuffix (state, text.Substring(y), pattern_no);
		    return;
		}
		else if ( y == text.Length - 1 )
		{
			State s = new State ( ((State) idx[state]).id, text[y]);

			s.final = true;
			s.next  = -pattern_no;

			list.Insert ( list.IndexOf(idx[state]), s);

			return;
		}

		state = next;
		y++;
	    }

Console.WriteLine("here we go, not matched: pat {0}", text);
	}



	protected void AppendSuffix (int state, string text, int pattern_no)
	{
	    State prev = new State(state, text[0]);

	    list.Insert ( list.IndexOf(idx[state]) + 1, prev );

	    for (int i=1; i<text.Length; i++)
	    {
		State s = new State(text[i]);

		list.Add(s);
		if ( s.id >= idx.Count ) idx.Add (s);

		prev.next = s.id;

		prev = s;
	    }

	    prev.final = true;
	    prev.next  = -pattern_no;
	}
    }
}
