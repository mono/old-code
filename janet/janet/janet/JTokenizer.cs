// JTokenizer.cs: JANET source code tokenizer
//
// Author: Steve Newman (steve@snewman.net)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Bitcraft, Inc.


#define TRACE

using System;
using System.IO;
using System.Text;
using System.Diagnostics;


// Namespace for classes associated with tokenizing ECMAScript source code.
namespace JANET.Compiler {

// Class used to represent source code positions
public struct SrcLoc
	{
	public int lineNum;	    // 1-based line number
	public int colNum;      // 1-based column number within line
	public int absPosition; // 0-based character position within file
	public int len;         // Number of characters occupied by this token.
	}


// Abstract base class for objects returned by the tokenizer.
public abstract class Token
	{
	public enum Type { number, id, keyword, reservedWord, op, stringLit };
	
	public string rawText; // The raw text of the token
	public SrcLoc loc;     // Location where the token came from.
	public Type   type;    // What sort of token this is.
	}

// Token class for numeric literals.  Note that the token will not
// include any leading '+' or '-' (that will have been parsed as
// an operator token).
public class NumLit : Token
	{
	public double value; // Numeric value of the literal
	
	public NumLit() { this.type = Token.Type.number; }
	}


// Token class for identifiers.
public class Identifier : Token
	{
	public Identifier() { this.type = Token.Type.id; }
	}

// Token class for keywords.
public class Keyword : Token
	{
	public Keyword() { this.type = Token.Type.keyword; }
	}

// Token class for reserved words.
public class ReservedWord : Token
	{
	public ReservedWord() { this.type = Token.Type.reservedWord; }
	}

// Token class for operators.
public class Operator : Token
	{
	public Operator() { this.type = Token.Type.op; }
	}

// Token class for string literals.
public class StringLit : Token
	{
	public enum QuoteType { singleQ, doubleQ, regexp }
	
	public string parsedText; // Contents of the literal, with the quotes removed
							  // and escape sequences processed.
	public QuoteType quoteType; // What sort of quoting was used for this literal.
	
	public StringLit() { this.type = Token.Type.stringLit; }
	}


// Exception thrown by Tokenizer when a syntax error is detected.
public class ParseError : ApplicationException
	{
	public ParseError(string message, SrcLoc loc) : base(message) { this.loc = loc; }
	
	public SrcLoc loc; // Position where error occurred
	}


// Utilities used throughout the file.
internal class CharUtils
	{
	// Return true if the given character can be a non-initial character in
	// an identifier.
	public static bool IsIdentifierChar(char c)
		{
		// HACK SN 7/13/01: should probably optimize this, e.g. with a
		// flags array.  Also, this list may not be complete per the spec.
		// Need to keep it in sync with CreateMatcher.
		return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
			   (c >= '0' && c <= '9') || (c == '_');
		} // IsIdentifierChar
	
	
	// Return true if the given character is a digit.
	public static bool IsDigitChar(char c)
		{
		// HACK SN 7/13/01: should probably optimize this, e.g. with a
		// flags array.
		return (c >= '0' && c <= '9');
		} // IsDigitChar
	
	
	// Return true if the given character is considered whitespace by the
	// ECMAScript spec.
	public static bool IsWhitespaceChar(char c)
		{
		// HACK SN 7/13/01: should probably optimize this, e.g. with a
		// flags array.  Also, this list isn't complete per the spec.
		// Need to keep it in sync with CreateMatcher.
		return (c == ' ' || c == '\t' || c == '\n' || c == '\r');
		} // IsWhitespaceChar
	
	
	// Return true if the given character is a line ender (and hence
	// terminates a "//" comment).
	public static bool IsEOLChar(char c)
		{
		// HACK SN 7/13/01: should probably optimize this, e.g. with a
		// flags array.  Also, this list may not be complete per the spec.
		return (c == '\n' || c == '\r');
		} // IsEOLChar
	
	} // CharUtils


// This utility class maintains an accessible buffer over a character stream
// for convenient lexing.  It also tracks line numbers and column offsets
// within the line.
internal class TextReaderBuffer
	{
	public TextReaderBuffer(TextReader reader, int bufSize)
		{
		this.reader    = reader;
		this.bufSize   = bufSize;
		this.buffer    = new char[bufSize];
		}
	
	
	// This attribute is our current position in the stream, i.e. the
	// number of characters consumed so far.
	public int pos { get { return bufOffset + bufStart; }}
	
	// These attributes hold the 1-based line and column numbers for the
	// next character to be read.
	public int lineNum { get { return lineNum_;            }}
	public int colNum  { get { return pos - lineStart + 1; }}
	
	
	// This attribute is true if we have reached the end of the stream,
	// false if there is more data to be read.
	public bool atEnd { get
		{
		if (bufEnd > bufStart)
			return false;
		else
			{
			FillBuffer();
			return (bufEnd <= bufStart);
			}
		
		}} // atEnd
	
	
	// Read the next character from the stream.  Throw InvalidOperationException
	// if there are no more characters to be read.
	public char Read()
		{
		if (bufEnd <= bufStart)
			{
			FillBuffer();
			if (bufEnd <= bufStart)
				throw new InvalidOperationException("read past end of stream");
			}
		
		char c = buffer[bufStart++];
		
		// Update our line tracking.
		if (c == '\n' || c == '\r')
			{
			// Don't increment the line number if this is the second character
			// of a CRLF pair.
			if (c != '\n' || lineStartChar != '\r' || colNum != 1)
				lineNum_++;
			
			lineStart = pos;
			lineStartChar = c;
			}
		
		return c;
		} // Read
	
	
	// Return the next character from the stream without consuming it.
	// If there are no more characters to be read, return 0.
	public char PeekOrZero()
		{
		if (bufEnd > bufStart)
			return buffer[bufStart];
		else
			{
			FillBuffer();
			if (bufEnd > bufStart)
				return buffer[bufStart];
			else
				return (char)0;
			
			}
		
		} // PeekOrZero
	
	
	// This method is used to get access to the subsequent text in the
	// stream, without consuming it.  We set buffer to a character array
	// in which text is found, and startPos to the index of the next
	// unread character in that buffer.  We return the number of characters
	// available in the buffer.  The return value will be at least equal to
	// minLen (it might be larger), unless there is not enough data remaining
	// in the stream, in which case the return value will be the number of
	// characters remaining in the stream.
	// 
	// It is illegal for minLen to be larger than the bufSize value passed
	// to our constructor.  For best efficiency, minLen should be no greater
	// than half of bufSize.
	public int GetBuffer(int minLen, out char[] buffer, out int startPos)
		{
		// Throw if we're asked for more than fits in our buffer.
		if (minLen > bufSize)
			throw new ArgumentOutOfRangeException("minLen greater than bufSize");
		
		// If we've already got enough data in the buffer, return it.
		buffer = this.buffer;
		if (bufEnd - bufStart >= minLen)
			{
			startPos = bufStart;
			return bufEnd-bufStart;
			}
		
		// Otherwise, attempt to fill the buffer.
		FillBuffer();
		
		// Now return whatever we've got.
		startPos = bufStart;
		return bufEnd-bufStart;
		} // GetBuffer
	
	
	// Consume the specified number of characters from the stream, and
	// return them in a string.  If there are fewer than len characters
	// remaining in the stream, consume as many characters as remain.
	// 
	// NOTE: the current implementation is limited to lengths no larger
	// than our bufSize.
	public string ConsumeString(int len)
		{
		char[] localBuffer;
		int localStartPos;
		int availLen = GetBuffer(len, out localBuffer, out localStartPos);
		Trace.Assert(availLen >= len);
		
		UpdateLineTracking(localBuffer, localStartPos, len);
		bufStart += len;
		return new String(localBuffer, localStartPos, len);
		} // ConsumeString
	
	
	// Consume the specified number of characters from the stream, and
	// discard them.  If there are fewer than len characters remaining
	// in the stream, consume as many characters as remain.
	// 
	// NOTE: the current implementation is limited to lengths no larger
	// than our bufSize.
	public void Skip(int len)
		{
		// NOTE: this could be rewritten to not call GetBuffer; the result
		// could be a bit more efficient, and capable of handling lengths
		// larger than bufSize.  This isn't important for our application,
		// though.
		char[] localBuffer;
		int localStartPos;
		int availLen = GetBuffer(len, out localBuffer, out localStartPos);
		Trace.Assert(availLen >= len);
		
		UpdateLineTracking(localBuffer, localStartPos, len);
		bufStart += len;
		} // Skip
	
	
	// Update our line tracking to reflect that we're about to consume
	// positions pos through pos+len-1 in buf.
	private void UpdateLineTracking(char[] buf, int pos, int len)
		{
		int relStreamPos = this.pos - pos;
		
		if (colNum != 1)
			lineStartChar = (char)0;
		
		int stopPos = pos+len;
		while (pos < stopPos)
			{
			char c = buf[pos++];
			if (c == '\n' || c == '\r')
				{
				// Don't increment the line number if this is the second character
				// of a CRLF pair.
				if (c != '\n' || lineStartChar != '\r')
					lineNum_++;
				
				lineStart = relStreamPos + pos;
				lineStartChar = c;
				}
			else
				lineStartChar = (char)0;
			
			} // pos loop
		
		} // UpdateLineTracking
	
	
	// If there is more data available in the stream, then fill the buffer
	// with as much data as possible.
	private void FillBuffer()
		{
		if (readEnd)
			return;
		
		// Slide the existing buffer contents down to the beginning.
		if (bufStart > 0)
			{
			bufOffset += bufStart;
			bufEnd -= bufStart;
			Array.Copy(buffer, bufStart, buffer, 0, bufEnd);
			bufStart = 0;
			}
		
		int requestedLen = bufSize-bufEnd;
		int charsRead = reader.ReadBlock(buffer, bufEnd, requestedLen);
		if (charsRead < requestedLen)
			readEnd = true;
		bufEnd += charsRead;
		} // FillBuffer
	
	
	private TextReader reader;
	
	// These variables define a buffer in which we "read ahead" some text from
	// the TextReader.  Valid data is in buffer[bufStart ... bufEnd-1].
	private char[] buffer;
	private int bufSize;
	private int bufStart = 0;
	private int bufEnd = 0;
	private int bufOffset = 0; // The stream position corresponding to buffer[0]
	private bool readEnd = false; // True if the buffer reaches to EOF.
	
	private int lineNum_ = 1;  // One-based line number of the next character to be read
	private int lineStart = 0; // Character position where the current line began.
	private char lineStartChar = (char)0; // If we're immediately after a CR or LF character,
							   // this holds that character.  Otherwise holds either
							   // the previous CR or LF character, or 0.  Used to
							   // detect CRLF sequences.
	} // TextReaderBuffer


// This utility class is used to store a list of strings which are
// "interesting" to the lexer, and quickly determine which (if any) is
// currently present in an input stream.
internal class StringMatcher
	{
	// This struct stores information about a single string registered
	// in the matcher.
	private class StringEntry
		{
		public char[] headlessString; // The string, minus its first character.
		public bool isIdentifier;	  // If true, then this entry only matches
									  // when followed by a character that can't
									  // form part of an identifier.
		public int info;              // The info parameter passed to AddString.
		public StringEntry next;	  // Next entry in the linked list for a
									  // given initial character.
		}
	
	public StringMatcher()
		{
		strings = new StringEntry[0];
		maxLen = 0;
		}
	
	public void AddString(string theString, int info)
		{ AddString(theString, info, false); }
	
	// Register the given string with this matcher.  The info parameter will
	// be returned from our Match method when this string is detected.
	// 
	// If isIdentifier is true, then we only match this string when it is
	// followed by a non-identifier character (or the end of the stream).
	public void AddString(string theString, int info, bool isIdentifier)
		{
		int tempLen = (isIdentifier) ? theString.Length+1 : theString.Length;
		if (maxLen < tempLen)
			maxLen = tempLen;
		
		// If necessary, extend the major axis of our strings table to
		// accomodate theString's first character.  We extend the array
		// by more than strictly necessary, to avoid an excessive number
		// of small extensions.
		char initialChar = theString[0];
		if (initialChar >= strings.Length)
			{
			StringEntry[] newStrings = new StringEntry[(initialChar+1)*2];
			Array.Copy(strings, 0, newStrings, 0, strings.Length);
			strings = newStrings;
			}
		
		// Create a StringEntry for this string, and store it in our
		// strings table.  Make sure to preserve the "decreasing length"
		// rule.
		StringEntry entry = new StringEntry();
		entry.headlessString = theString.ToCharArray(1, theString.Length-1);
		entry.isIdentifier = isIdentifier;
		entry.info = info;
		
		if ( strings[initialChar] == null ||
			 entry.headlessString.Length >= strings[initialChar].headlessString.Length )
			{
			entry.next = strings[initialChar];
			strings[initialChar] = entry;
			}
		else
			{
			StringEntry predecessor = strings[initialChar];
			while ( predecessor.next != null &&
					entry.headlessString.Length < predecessor.next.headlessString.Length )
				predecessor = predecessor.next;
			
			entry.next = predecessor.next;
			predecessor.next = entry;
			}
		
		} // AddString
	
	
	// If the current position of the TextReaderBuffer matches any
	// of our registered strings, then set length to the length of the
	// longest such string, set info to the value supplied in AddString,
	// and return true.  Otherwise leave length and info undefined, and
	// return false.
	public bool Match(TextReaderBuffer reader, out int length, out int info)
		{
		// We don't really need to assign length and info here, but we do so
		// so that the compiler won't complain on any of the "return false"
		// paths.
		length = 0;
		info = 0;
		
		char[] buffer;
		int bufStart;
		int bufLen = reader.GetBuffer(maxLen, out buffer, out bufStart);
		if (bufLen <= 0)
			return false;
		
		// NOTE (snewman 7/12/01): this is currently implemented as a
		// simple linear search over all registered strings, after
		// discriminating by the initial character.  Many more efficient
		// algorithms are possible, such as binary search or tries.  Doesn't
		// seem worthwhile.
		int initialChar = buffer[bufStart];
		bufStart++;
		bufLen--;
		
		if (initialChar >= strings.Length)
			return false;
		
		for ( StringEntry candidate = strings[initialChar];
			  candidate != null; candidate = candidate.next )
			if ( bufLen >= candidate.headlessString.Length &&
				 MatchChars(buffer, bufStart, candidate.headlessString) )
				if ( !candidate.isIdentifier ||
					 bufLen == candidate.headlessString.Length ||
					 !CharUtils.IsIdentifierChar(buffer[bufStart+candidate.headlessString.Length]) )
					{
					length = candidate.headlessString.Length + 1;
					info = candidate.info;
					return true;
					}
		
		return false;
		} // Match
	
	
	// Return true if the contents of the buffer array, beginning at bufStart,
	// are equal to the contents of the match array.  We assume that the
	// buffer array is long enough to "fit" a complete match.
	private bool MatchChars(char[] buffer, int bufStart, char[] match)
		{
		// NOTE SN 7/12/01: there's gotta be a library call for this, but
		// I haven't found it yet.
		for (int i=0; i<match.Length; i++)
			if (buffer[bufStart+i] != match[i])
				return false;
		
		return true;
		} // MatchChars
	
	
	// This table stores all of the strings registered in this matcher.
	// They are organized by initial character, and then in order of
	// decreasing length, so strings['a'] holds the longest string
	// that begins with 'a', and the "next" field chains through the other
	// strings beginning with 'a'.  Strings of identical length are stored
	// in arbitrary order.  The array is initially of length zero; it is
	// extended as necessary to accomodate the highest-ordinal initial
	// character of any registered string.
	private StringEntry[] strings;
	
	private int maxLen; // Maximum length of any registered string.  (For
						// this purpose, we add one to the length of any
						// string whose isIdentifier flag is set.)
	} // StringMatcher


// This class returns a series of Token objects, and provides utility
// methods for peeking ahead in the token stream.  It requires a subclass
// to implement low-level tokenization.
public abstract class TokenizerBase
	{
	protected TokenizerBase()
		{
		last = null;
		peek = null;
		peek2 = null;
		} // TokenizerBase constructor
	
	
	// This attribute is true if there are more tokens to be read.
	public bool atEnd { get
		{
		return (Peek() == null);
		}} // atEnd
	
	
	// Consume and return the next token.  If there are no more tokens,
	// return null.
	public Token Match()
		{
		if (peek == null)
			{
			Token token = ReadNextToken();
			if (token == null)
				return null;
			
			last = token;
			}
		else
			{
			last = peek;
			peek = peek2;
			peek2 = null;
			}
		
		return last;
		}
	
	
	// Return the next token, without consuming it.  If there are no more
	// tokens, return null.
	public Token Peek()
		{
		if (peek == null)
			peek = ReadNextToken();
		
		return peek;
		} // Peek
	
	
	// Return the next-but-one token, without consuming any tokens.  If there
	// are one or fewer remaining tokens, return null.
	public Token Peek2()
		{
		Peek();
		if (peek2 == null)
			peek2 = ReadNextToken();
		
		return peek2;
		} // Peek2
	
	
	// Return the most recently consumed token, or null if we have not yet
	// consumed any tokens.
	public Token Prev() { return last; }
	
	
	// Consume the next token.  If it is not the given operator, throw a
	// parse error.
	public void MatchOp(string op)
		{
		Token token = Match();
		if ( token == null || token.type != Token.Type.op ||
			 ((Operator)token).rawText != op )
			ThrowError("expected \"" + op + "\"");
		
		} // MatchOp
	
	
	// Consume the next token.  If it is not the given keyword, throw a
	// parse error.
	public void MatchKeyword(string keyword)
		{
		Token token = Match();
		if ( token == null || token.type != Token.Type.keyword ||
			 ((Keyword)token).rawText != keyword )
			ThrowError("expected \"" + keyword + "\"");
		
		} // MatchKeyword
	
	
	// Consume the next token.  If it is an identifier, return the identifier.
	// Otherwise throw a parse error.
	public string MatchID()
		{
		Token token = Match();
		if (token != null && token.type == Token.Type.id)
			return ((Identifier)token).rawText;
		else
			{
			ThrowError("expected an identifier");
			return null; // never reached
			}
		
		} // MatchID
	
	
	// Consume the next token.  If it is a numeric literal, return the number.
	// Otherwise throw a parse error.
	public double MatchNumLit()
		{
		Token token = Match();
		if (token != null && token.type == Token.Type.number)
			return ((NumLit)token).value;
		else
			{
			ThrowError("expected a numeric literal");
			return 0; // never reached
			}
		
		} // MatchNumLit
	
	
	// If the next token is the given operator, consume it and return true.
	// Otherwise do nothing and return false.
	public bool TryMatchOp(string op)
		{
		if (PeekOp(op))
			{
			Match();
			return true;
			}
		else
			return false;
		
		} // TryMatchOp
	
	
	// If the next token is the given keyword, consume it and return true.
	// Otherwise do nothing and return false.
	public bool TryMatchKeyword(string keyword)
		{
		if (PeekKeyword(keyword))
			{
			Match();
			return true;
			}
		else
			return false;
		
		} // TryMatchKeyword
	
	
	// If the next token is an identifier, consume it, set id accordingly,
	// and return true.  Otherwise set keyword to null and return false.
	public bool TryMatchID(out string id)
		{
		Token token = Peek();
		if (token != null && token.type == Token.Type.id)
			{
			id = ((Identifier)token).rawText;
			Match();
			return true;
			}
		else
			{
			id = null;
			return false;
			}
		
		} // TryMatchID
	
	
	// If the next token is a numeric literal, consume it, set d accordingly,
	// and return true.  Otherwise set d to 0 and return false.
	public bool TryMatchNumLit(out double d)
		{
		Token token = Peek();
		if (token != null && token.type == Token.Type.number)
			{
			d = ((NumLit)token).value;
			Match();
			return true;
			}
		else
			{
			d = 0;
			return false;
			}
		
		} // TryMatchNumLit
	
	
	// If the next token is a string literal, consume it, set s accordingly,
	// and return true.  Otherwise set s to null and return false.
	public bool TryMatchStringLit(out string s)
		{
		Token token = Peek();
		if (token != null && token.type == Token.Type.stringLit)
			{
			s = ((StringLit)token).parsedText;
			Match();
			return true;
			}
		else
			{
			s = null;
			return false;
			}
		
		} // TryMatchStringLit
	
	
	// Return true if the next token is the given operator.
	public bool PeekOp(string op)
		{
		Token token = Peek();
		return ( token != null && token.type == Token.Type.op &&
				 ((Operator)token).rawText == op );
		} // PeekOp
	
	
	// Return true if the next-but-one token is the given operator.
	public bool Peek2Op(string op)
		{
		Token token = Peek2();
		return ( token != null && token.type == Token.Type.op &&
				 ((Operator)token).rawText == op );
		} // Peek2Op
	
	
	// Return true if the next token is the given keyword.
	public bool PeekKeyword(string keyword)
		{
		Token token = Peek();
		return ( token != null && token.type == Token.Type.keyword &&
				 ((Keyword)token).rawText == keyword );
		} // PeekKeyword
	
	
	private Token last;  // Most-recently-consumed token, or null if no token
						 // has yet been consumer.
	private Token peek;  // A token fetched from the stream but not yet
						 // consumed, or null if we haven't read ahead.
	private Token peek2; // Another fetched-but-not-consumed token, or null if
						 // we haven't read ahead by 2 tokens yet.
	
	
	// Read the next token from the stream.  Skip whitespace and comments.
	// If there are no further tokens in the stream, return null.
	protected abstract Token ReadNextToken();
	
	
	// Throw a parse error associated with the most-recently-consumer token.
	protected void ThrowError(string message)
		{
		throw new ParseError(message, last.loc);
		}
	  
	} // TokenizerBase


// This class implements TokenizerBase by actually tokenizing text from
// a TextReader.
public class Tokenizer : TokenizerBase
	{
	// This constant gives the maximum length we can parse for an identifier,
	// string literal, or other token.  (It does not limit comments.)
	// SN HACK 7/13/01: this is pretty generous, but someday we should remove
	// the fixed-length limitation, at least for strings.
	private const int maxTokenLen = 5000;
												
	private const int readerBufSize = maxTokenLen*2;
	
	
	public Tokenizer(TextReader reader)
		{
		this.reader = reader;
		this.buffer = new TextReaderBuffer(reader, readerBufSize);
		this.readToEnd = false;
		} // Tokenizer constructor
	
	
	// Read the next token from the stream.  Skip whitespace and comments.
	// If there are no further tokens in the stream, return null.
	protected override Token ReadNextToken()
		{
		// Wrap the entire function in a loop so we can come back around
		// after matching a comment or whitespace.
		while (true)
			{
			if (readToEnd)
				return null;
			
			if (buffer.atEnd)
				{
				readToEnd = true;
				return null;
				}
			
			SrcLoc loc;
			loc.lineNum = buffer.lineNum;
			loc.colNum  = buffer.colNum;
			loc.absPosition = buffer.pos;
			loc.len = 1; // this will typically be overridden later
			
			int tokenLen, tokenInfo;
			if (matcher.Match(buffer, out tokenLen, out tokenInfo))
				{
				switch ((TokDispatch)tokenInfo)
					{
					case TokDispatch.ident:
						return ParseIdentifier(loc);
					
					case TokDispatch.digit:
						return ParseNumber(loc);
					
					case TokDispatch.stringLit:
						return ParseStringLiteral(loc);
					
					case TokDispatch.op:
						{
						Operator token = new Operator();
						token.rawText = buffer.ConsumeString(tokenLen);
						loc.len = tokenLen;
						token.loc = loc;
						return token;
						}
					
					case TokDispatch.keyword:
						{
						Keyword token = new Keyword();
						token.rawText = buffer.ConsumeString(tokenLen);
						loc.len = tokenLen;
						token.loc = loc;
						return token;
						}
					
					case TokDispatch.reserved:
						{
						ReservedWord token = new ReservedWord();
						token.rawText = buffer.ConsumeString(tokenLen);
						loc.len = tokenLen;
						token.loc = loc;
						return token;
						}
					
					case TokDispatch.comment:
						{
						SkipComment(loc);
						
						// Now fall through to the end of the main loop so
						// we will parse the next token.
						break;
						}
					
					case TokDispatch.ws:
						{
						// This is a whitespace character, skip it.  For
						// efficiency, we suck up all subsequent whitespace
						// at the same time (up to one buffer's worth).
						SkipWhitespace();
						
						// Now fall through to the end of the main loop so
						// we will parse the next token.
						break;
						}
					
					default:
						Trace.Assert(false);
						break;
					
					} // switch (tokenInfo)
				
				}
			else
				{
				string msg = "unexpected input character '" +
							 new String(buffer.PeekOrZero(), 1) +
							 "'";
				throw new ParseError(msg, loc);
				}
		
			} // while (true)
		
		} // ReadNextToken
	
	
	// Parse an identifier beginning at the current stream location.
	private Token ParseIdentifier(SrcLoc loc)
		{
		// Get a buffer from which we can parse the identifier.
		char[] charBuf;
		int startPos;
		int bufLen = buffer.GetBuffer( maxTokenLen, out charBuf,
									   out startPos );
		if (bufLen > maxTokenLen)
			bufLen = maxTokenLen;
		
		// Find the last character in the identifier.  We begin
		// at 1 because we know (from matcher.Match returning
		// TokDispatch.ident) that the identifier is at least
		// one character long.
		int tokenLen = 1;
		while ( tokenLen < bufLen &&
				CharUtils.IsIdentifierChar(charBuf[startPos+tokenLen]) )
			tokenLen++;
		
		Identifier token = new Identifier();
		token.rawText = buffer.ConsumeString(tokenLen);
		loc.len = tokenLen;
		token.loc = loc;
		return token;
		} // ParseIdentifier
	
	
	// Parse a numeric literal beginning at the current stream location.
	private Token ParseNumber(SrcLoc loc)
		{
		// Get a buffer from which we can parse the number.
		char[] charBuf;
		int startPos;
		int bufLen = buffer.GetBuffer( maxTokenLen, out charBuf,
									   out startPos );
		if (bufLen > maxTokenLen)
			bufLen = maxTokenLen;
		
		// Find the end of the number.  We match for the following
		// components:
		// 
		//    integer part (1 or more digits)
		//    optional fractional part ('.' plus one or more digits)
		//    optional exponent ('E' or 'e', optional '+' or '-', and 1-3 digits)
		// 
		// HACK SN 7/14/01: for now, we aren't enforcing that the fractional
		// part must have at least one digit, or that the exponent must have
		// at least 1 and no more than 3 digits.  Review the ECMAScript spec for
		// the correct rules.  Also, should we allow numbers that begin with a '.'
		// (no integer part)?
		
		int tokenLen = 0;
		while ( tokenLen < bufLen &&
				CharUtils.IsDigitChar(charBuf[startPos+tokenLen]) )
			tokenLen++;
		
		if (tokenLen < bufLen && charBuf[startPos+tokenLen] == '.')
			{
			tokenLen++;
			while ( tokenLen < bufLen &&
					CharUtils.IsDigitChar(charBuf[startPos+tokenLen]) )
				tokenLen++;
			}
		
		if ( tokenLen < bufLen &&
			 ( charBuf[startPos+tokenLen] == 'E' ||
			   charBuf[startPos+tokenLen] == 'e' ) )
			{
			tokenLen++;
			if ( tokenLen < bufLen &&
				 ( charBuf[startPos+tokenLen] == '+' ||
				   charBuf[startPos+tokenLen] == '-' ) )
				tokenLen++;
			
			while ( tokenLen < bufLen &&
					CharUtils.IsDigitChar(charBuf[startPos+tokenLen]) )
				tokenLen++;
			}
		
		NumLit token = new NumLit();
		token.rawText = buffer.ConsumeString(tokenLen);
		loc.len = tokenLen;
		token.loc = loc;
		
		try
			{
			token.value = Double.Parse(token.rawText);
			}
		catch (FormatException e)
			{
			throw new ParseError( "FormatException parsing numeric literal \"" +
								  token.rawText + "\": " + e.ToString(),
								  loc );
			}
		catch (OverflowException e)
			{
			throw new ParseError( "OverflowException parsing numeric literal \"" +
								  token.rawText + "\": " + e.ToString(),
								  loc );
			}
		
		return token;
		} // ParseNumber
	
	
	// Parse a string literal or regexp expression beginning at the current
	// stream location.
	private Token ParseStringLiteral(SrcLoc loc)
		{
		// HACK SN 7/14/01: rewrite this to support literals longer
		// than maxTokenLen.
		
		// HACK SN 7/14/01: parse regexps as well as single- or double-quoted strings
		
		// Get a buffer from which we can parse the string.
		char[] charBuf;
		int startPos;
		int bufLen = buffer.GetBuffer(maxTokenLen, out charBuf, out startPos);
		Trace.Assert(bufLen > 1);
		
		// Scan to find the end of the string.
		char quoteChar = charBuf[startPos];
		int tokenLen = 1;
		StringBuilder parsedValueBuilder = new StringBuilder();
		while (tokenLen < bufLen)
			{
			char curChar = charBuf[startPos+tokenLen];
			if (curChar == quoteChar)
				break;
			else if (curChar == '\\')
				{
				// HACK SN 7/14/01: extend this to parse all ECMAScript backslash
				// sequences.
				
				if (tokenLen+1 >= bufLen)
					throw new ParseError("unterminated string literal or regexp", loc);
				
				curChar = charBuf[startPos+tokenLen+1];
				switch (curChar)
					{
					case 'b': curChar = '\b'; break;
					case 'f': curChar = '\f'; break;
					case 'n': curChar = '\n'; break;
					case 'r': curChar = '\r'; break;
					case 't': curChar = '\t'; break;
					case 'v': curChar = '\v'; break;
					}
				
				parsedValueBuilder.Append(curChar);
				tokenLen += 2;
				}
			else
				{
				parsedValueBuilder.Append(curChar);
				tokenLen++;
				}
			
			} // while (tokenLen < bufLen)
		
		if (tokenLen >= bufLen)
			throw new ParseError("unterminated string literal or regexp", loc);
		
		loc.len = tokenLen + 1;
		StringLit litToken = new StringLit();
		litToken.rawText    = buffer.ConsumeString(tokenLen+1);
		litToken.loc        = loc;
		litToken.parsedText = parsedValueBuilder.ToString();
		
		switch (quoteChar)
			{
			case '\'': litToken.quoteType = StringLit.QuoteType.singleQ; break;
			case '"':  litToken.quoteType = StringLit.QuoteType.doubleQ; break;
			case '/':  litToken.quoteType = StringLit.QuoteType.regexp;  break;
			
			default:
				Trace.Assert(false);
				break;
			}
		
		return litToken;
		} // ParseStringLiteral
	
	
	// Advance the stream past a comment beginning at the current stream
	// location.
	private void SkipComment(SrcLoc loc)
		{
		// HACK SN 7/13/01: rewrite this to support comments longer
		// than maxTokenLen.
		
		// Get a buffer from which we can parse the comment.
		char[] charBuf;
		int startPos;
		int bufLen = buffer.GetBuffer( maxTokenLen, out charBuf,
									   out startPos );
		
		int tokenLen = 2;
		if (charBuf[startPos+1] == '/')
			{
			// C++ style comment; search for end of line
			while ( tokenLen < bufLen &&
					!CharUtils.IsEOLChar(charBuf[startPos+tokenLen]) )
				tokenLen++;
			}
		else
			{
			// C style comment; search for "*/"
			// NOTE: we might extend this to warn for nested comments.
			while ( tokenLen < bufLen-1 &&
					( charBuf[startPos+tokenLen  ] != '*' ||
					  charBuf[startPos+tokenLen+1] != '/' ) )
				tokenLen++;
			
			if (tokenLen >= bufLen-1)
				{
				loc.len = tokenLen;
				throw new ParseError("unterminated /* comment", loc);
				}
			
			// Include the trailing "*/" in tokenLen.
			tokenLen += 2;
			}
		
		buffer.Skip(tokenLen);
		} // SkipComment
	
	
	// Advance the stream past a block of whitespace beginning at the
	// current stream location.
	// 
	// NOTE: the current implementation of this method will only skip
	// up to one buffer's worth of whitespace.  This simplifies the
	// implementation, and is OK because we'll just skip any remaining
	// whitespace in the next go-around of the tokenizer loop.
	private void SkipWhitespace()
		{
		// Get a buffer from which we can search for whitespace.
		char[] charBuf;
		int startPos;
		int bufLen = buffer.GetBuffer( maxTokenLen, out charBuf,
									   out startPos );
		
		int tokenLen = 1;
		while ( tokenLen < bufLen &&
				CharUtils.IsWhitespaceChar(charBuf[startPos+tokenLen]) )
			tokenLen++;
		
		buffer.Skip(tokenLen);
		} // SkipWhitespace
	
	
	private static readonly StringMatcher matcher = CreateMatcher();
	
	
	enum TokDispatch : int
		{
		ident,    // A character which begins an identifier (but not a keyword
				  // or reserved word)
		digit,    // digit: 0-9
		stringLit,// string literal or regexp
		op,       // operator
		keyword,  // keyword
		reserved, // reserved word
		comment,  // comment: // or /*
		ws        // whitespace
		}
	
	
	// Create a StringMatcher object populated with rules to help match
	// all ECMAScript tokens.
	private static StringMatcher CreateMatcher()
		{
		StringMatcher matcher = new StringMatcher();
		
		string[] operators = new string[]
			{
			"{", "}", "(", ")", "[", "]",
			".", ";", ",",
			"<", ">", ">=", "<=", "==", "!=", "===", "!==",
			"+", "-", "*", "%", "++", "--", "<<", ">>", ">>>",
			"&", "|", "^", "!", "~", "&&", "||", "?", ":",
			"=", "+=", "-=", "*=", "%=", "<<=", ">>=", ">>>=",
			"&=", "|=", "^=",
			"/", "/="
			};
		
		string[] keywords = new string[]
			{
			"break", "else", "new", "var", "case", "finally", "return", "void",
			"catch", "for", "switch", "while", "continue", "function", "this",
			"with", "default", "if", "throw", "delete", "in", "try", "do",
			"instanceof", "typeof", "null", "true", "false"
			};
			
		string[] reserveds = new string[]
			{
			"abstract", "enum", "int", "short", "boolean", "export", "interface",
			"static", "byte", "extends", "long", "super", "char", "final",
			"native", "synchronized", "class", "float", "package", "throws",
			"const", "goto", "private", "transient", "debugger", "implements",
			"protected", "volatile", "double", "import", "public"
			};
		
		int i;
		for (i=0; i<26; i++)
			{
			matcher.AddString(new String((char)('a' + i), 1), (int)TokDispatch.ident);
			matcher.AddString(new String((char)('A' + i), 1), (int)TokDispatch.ident);
			}
		
		matcher.AddString("_", (int)TokDispatch.ident);
		
		for (i=0; i<10; i++)
			{
			matcher.AddString(new String((char)('0' + i), 1), (int)TokDispatch.digit);
			}
		
		matcher.AddString("'",  (int)TokDispatch.stringLit);
		matcher.AddString("\"", (int)TokDispatch.stringLit);
		matcher.AddString("/",  (int)TokDispatch.stringLit);
		
		// HACK SN 7/14/01: distinguish between '/', '/=', and regexps
		
		for (i=0; i<operators.Length; i++)
			matcher.AddString(operators[i], (int)TokDispatch.op);
		
		for (i=0; i<keywords.Length; i++)
			matcher.AddString(keywords[i], (int)TokDispatch.keyword, true);
		
		for (i=0; i<reserveds.Length; i++)
			matcher.AddString(reserveds[i], (int)TokDispatch.reserved, true);
		
		matcher.AddString("/*", (int)TokDispatch.comment);
		matcher.AddString("//", (int)TokDispatch.comment);
		
		// HACK SN 7/12/01: verify that this is the complete set of whitespace chars
		matcher.AddString(" ",  (int)TokDispatch.ws);
		matcher.AddString("\t", (int)TokDispatch.ws);
		matcher.AddString("\n", (int)TokDispatch.ws);
		matcher.AddString("\r", (int)TokDispatch.ws);
		
		return matcher;
		} // CreateMatcher
	
	
	private TextReader       reader;    // Input stream
	private TextReaderBuffer buffer;    // Buffering class
	private bool             readToEnd; // Set to true when ReadNextToken
									    // reaches the end of the input text.
	} // Tokenizer


// This class is used to string tokens together in a linked list.
public class TokenListNode
	{
	public Token         token;
	public TokenListNode next;
	} // TokenListNode


// This class implements TokenizerBase by scanning through a list of
// preparsed tokens.
public class Retokenizer : TokenizerBase
	{
	// Construct a Retokenizer to scan a linked list of tokens with the
	// given head node.
	public Retokenizer(TokenListNode tokens)
		{
		nextToken = tokens;
		} // Retokenizer constructor
	
	
	// Read the next token from the stream.  Skip whitespace and comments.
	// If there are no further tokens in the stream, return null.
	protected override Token ReadNextToken()
		{
		if (nextToken == null)
			return null;
		
		Token result = nextToken.token;
		nextToken = nextToken.next;
		return result;
		} // ReadNextToken
	
	TokenListNode nextToken; // Next entry in the linked list of tokens.
	} // Retokenizer


} // namespace JANET.Compiler
