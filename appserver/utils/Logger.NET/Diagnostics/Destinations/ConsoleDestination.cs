using System;

using TerWoord.Diagnostics.Messages;

namespace TerWoord.Diagnostics.Destinations
{
	/// <summary>
	///   <para>
	///     Puts messages to the console
	///   </para>
	/// </summary>             
	public class ConsoleDestination: Destination
	{
    private const char IndentChar = ' ';
    private string GetIndentString(int ShrinkIt)
    {
      string OneIndentString = "";
      for (int i = 1; i <= IndentSize; i++)
        OneIndentString += IndentChar;

      string TempString = "";
      for (int i = 1; i <= LogFactory.Indent; i++) 
        TempString += OneIndentString;

      if (TempString.Length < ShrinkIt)
      {
        TempString = "";
      }
      else
      {
        if (ShrinkIt != 0)
        {
          TempString = TempString.Remove(0, (IndentSize / ShrinkIt));
        }
      }
      return TempString;
    }

    private string GetSingleIndentString(int Shrink)
    {
      string OneIndentString = "";
      for (int i = 1; i <= IndentSize; i++)
        OneIndentString += IndentChar;
      if (Shrink != 0)
      {
        OneIndentString = OneIndentString.Remove(0, (IndentSize / Shrink));
      }
      return OneIndentString;
    }

    private string GetIndentString()
    {
      return GetIndentString(0);
    }

    public int IndentSize = 2;

    public override void EnterMethod(EnterMethodMessage EMM)
    {
      Console.WriteLine("LOG: " + GetIndentString() + GetSingleIndentString(2) + ">" + EMM.MethodName);
    }

    public override void ExitMethod(ExitMethodMessage EMM)
    {
      Console.WriteLine("LOG: " + GetIndentString(2) + "<" + EMM.MethodName);  
    }

    public override void SendString(StringMessage SM)
    {
      Console.WriteLine("LOG: " + GetIndentString() + SM.Message);
    }

    public override void SendValue(ValueMessage VM)
    {
      if (VM.Value == null)
      {      
        Console.WriteLine("LOG: " + GetIndentString() + String.Format("{0} = NULL", VM.Message));
      }
      else
      {
        Console.WriteLine("LOG: " + GetIndentString() + String.Format("{0} = '{1}'", VM.Message, VM.Value.ToString()));
      }
    }

    public override void SendError(ErrorMessage EM)
    {
      Console.WriteLine("LOG: " + GetIndentString() + " ERROR: " + EM.Message);
    }
  }
}
