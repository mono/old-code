// PrettyPrinter.cs: JANET structured output generator
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
namespace JANET.Printer {

public class PrettyPrinter
	{
	public PrettyPrinter(TextWriter writer)
		{
		this.writer = writer;
		} // PrettyPrinter constructor
	
	
	// Output a blank line.
	public void Line()
		{
		Line("");
		} // Line (blank)
	
	
	// Output the given text, on its own line.
	public void Line(string text)
		{
		if (inLine)
			writer.Write(lineBreakString);
		
		WriteIndentation();
		writer.Write(text);
		writer.Write(lineBreakString);
		inLine = false;
		} // Line (no format parameters)
	
	
	// Output the given text.
	public void Text(string text)
		{
		if (!inLine)
			WriteIndentation();
		
		writer.Write(text);
		
		inLine = true;
		} // Text (no format parameters)
	
	
	// Output the given format string, formatting it with one parameter.
	public void Text(string format, object param)
		{
		Text(String.Format(format, param));
		} // Text (one format parameter)
	
	
	// Output the given format string, formatting it with two parameters.
	public void Text(string format, object p1, object p2)
		{
		Text(String.Format(format, p1, p2));
		} // Text (two format parameters)
	
	
	// Output the given format string, formatting it with any number of parameters.
	public void Text(string format, params object[] p)
		{
		Text(String.Format(format, p));
		} // Text (N format parameters)
	
	
	// End the current line.  If we were not in a line, output an empty line.
	public void EndLine()
		{
		EndLine("");
		} // EndLine (no format parameters)
	
	
	// Output the given text, forcing it to end a line.
	public void EndLine(string text)
		{
		if (!inLine)
			WriteIndentation();
		
		writer.Write(text);
		writer.Write(lineBreakString);
		inLine = false;
		} // EndLine (no format parameters)
	
	
	// Output the given text, on its own line, formatting it with one parameter.
	public void Line(string format, object param)
		{
		Line(String.Format(format, param));
		} // Line (one format parameter)
	
	
	// Output the given text, on its own line, formatting it with two parameters.
	public void Line(string format, object p1, object p2)
		{
		Line(String.Format(format, p1, p2));
		} // Line (two format parameters)
	
	
	// Output the given text, on its own line, formatting it with any number
	// of parameters.
	public void Line(string format, params object[] p)
		{
		Line(String.Format(format, p));
		} // Line (N format parameters)
	
	
	// Output the given text, on its own line, formatting it with any number
	// of parameters.  This method uses nonstandard formatting conventions,
	// designed to support the synthesis of JavaScript code.  Currently
	// supported formatting directives:
	// 
	//    {n}         emit parameter n
	//    {'n}        emit parameter n, enclosed in single-quotes, escaping
	//				  all characters that would otherwise cause trouble in
	//				  a string literal.
	// 
	// We override ToString for boolean values to use lowercase "true" and
	// "false".
	public void JSLine(string format, params object[] p)
		{
		// HACK snewman 9/22/01: this code is inefficient, and doesn't
		// do enough error checking.  Might see if I can just call String.Format
		// with an appropriate "handler" or something.
		
		Trace.Assert(p.Length <= 10); // HACK snewman 9/22/01: only 10 params
									  // supported right now.
		
		for (int i=0; i<p.Length; i++)
			{
			string oldStr = "{" + (char)(i+'0') + "}";
			string newStr = p[i].ToString();
			if (p[i] is bool)
				newStr = ((bool)(p[i])) ? "true" : "false";
			
			format  = format.Replace(oldStr, newStr);
			
			oldStr = "{'" + (char)(i+'0') + "}";
			if (format.IndexOf(oldStr) >= 0)
				{
				newStr = "'" + BuildEscapedString(newStr) + "'";
				format = format.Replace(oldStr, newStr);
				}
			
			} // i loop
		
		Line(format);
		} // JSLine (N format parameters)
	
	
	// Return a copy of s, escaping all characters which can't appear
	// naked in an ECMAScript string literal.
	public static string BuildEscapedString(String s)
		{
		StringBuilder builder = new StringBuilder();
		
		// HACK snewman 9/20/01: encode all remaining nonprinting characters
		// numerically.  Need to fix ParseStringLiteral in JTokenizer.cs to
		// parse numeric escapes.
		
		foreach (char c in s)
			if (c == '"')
				builder.Append("\\\"");
			else if (c == '\'')
				builder.Append("\\'");
			else if (c == '\\')
				builder.Append("\\\\");
			else if (c == '\n')
				builder.Append("\\n");
			else if (c == '\r')
				builder.Append("\\r");
			else if (c == '\t')
				builder.Append("\\t");
			else
				builder.Append(c);
		
		return builder.ToString();
		} // BuildEscapedString
	
	
	// Indent all subsequent output by three characters.  These calls
	// can be nested; their effect is cumulative.
	public void Indent()
		{
		indentLevel += 3;
		}
	
	
	// Remove three characters from the indentation level (i.e. reverse
	// the effect of one call to Indent).
	public void Outdent()
		{
		indentLevel -= 3;
		Trace.Assert(indentLevel >= 0);
		}
	
	
	private void WriteIndentation()
		{
		// OPTIMIZATION snewman 8/15/01
		for (int i=0; i<indentLevel; i++)
			writer.Write(" ");
		} // WriteIndentation
	
	
	private TextWriter writer;
	private string lineBreakString = "\r\n";
	private int indentLevel = 0;
	private bool inLine = false; // True if we have output any text on the
								 // current line.
	} // PrettyPrinter

} // namespace JANET.Printer
