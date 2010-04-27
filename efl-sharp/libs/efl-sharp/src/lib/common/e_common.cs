namespace Enlightenment
{
   using System;
   using System.IO;
   using System.Runtime.InteropServices;  
   
   public class Common
     {
	public static string PtrToString (IntPtr p)
	  {
	     // TODO: deal with character set issues.  Will PtrToStringAnsi always
	     // "Do The Right Thing"?
	     if (p == IntPtr.Zero)
	       return null;
	     return Marshal.PtrToStringAnsi (p);
	  }
	
	
	/*
	 * Marshal a C `char **'.  ANSI C `main' requirements are assumed:
	 *
	 *   stringArray is an array of pointers to C strings
	 *   stringArray has a terminating NULL string.
	 *
	 * For example:
	 *   stringArray[0] = "string 1";
	 *   stringArray[1] = "string 2";
	 *   stringArray[2] = NULL
	 *
	 * The terminating NULL is required so that we know when to stop looking
	 * for strings.
	 */
	public static string[] PtrToStringArray (IntPtr stringArray)
	  {
	     if (stringArray == IntPtr.Zero)
	       return new string[]{};
	     
	     int argc = CountStrings (stringArray);
	     return PtrToStringArray (argc, stringArray);
	  }
	
	private static int CountStrings (IntPtr stringArray)
	  {
	     int count = 0;
	     while (Marshal.ReadIntPtr (stringArray, count*IntPtr.Size) != IntPtr.Zero)
	       ++count;
	     return count;
	  }
	
	/*
	 * Like PtrToStringArray(IntPtr), but it allows the user to specify how
	 * many strings to look for in the array.  As such, the requirement for a
	 * terminating NULL element is not required.
	 *
	 * Usage is similar to ANSI C `main': count is argc, stringArray is argv.
	 * stringArray[count] is NOT accessed (though ANSI C requires that 
	 * argv[argc] = NULL, which PtrToStringArray(IntPtr) requires).
	 */
	public static string[] PtrToStringArray (int count, IntPtr stringArray)
	  {
	     if (count < 0)
	       throw new ArgumentOutOfRangeException ("count", "< 0");
	     if (stringArray == IntPtr.Zero)
	       return new string[count];
	     
	     string[] members = new string[count];
	     for (int i = 0; i < count; ++i) {
		IntPtr s = Marshal.ReadIntPtr (stringArray, i * IntPtr.Size);
		members[i] = PtrToString (s);
	     }
	     
	     return members;
	  }
     }
}
