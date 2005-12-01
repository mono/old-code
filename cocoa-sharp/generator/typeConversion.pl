# Please, oh please, add to this - C.J.
#  $Header: /home/miguel/third-conversion/public/cocoa-sharp/generator/typeConversion.pl,v 1.3 2004/06/18 22:36:18 gnorton Exp $

%typeConversion = 
    # ObjC                 => C#
    ( "int"                => "int",
      "SInt32"             => "Int32",
      "int *"              => "IntPtr",
      "NSString "          => "Apple.Foundation.NSString",
      "id"                 => "IntPtr",
      "unichar"            => "char",
      "char"               => "char",
      "unsigned char"      => "uchar",
      "short"              => "short",
      "unsigned short"     => "ushort",
      "unsigned int"       => "uint",
      "int32_t"            => "Int32",
      "long"               => "long",
      "unsigned long"      => "ulong",
      "long long"          => "Int64",
      "int64_t"            => "Int64",
      "unsigned long long" => "UInt64",
      "void"               => "void",
      "void*"              => "IntPtr",
      "BOOL"               => "bool",
      "Boolean"            => "bool",
      "unsigned"           => "uint",
      "double"             => "double",
      "float"              => "float",
      "Class"              => "IntPtr",
      "IMP"                => "IntPtr",
      "OSType"             => "int",
      "OSErr"              => "int",
      "SEL"                => "IntPtr",
      "CFRunLoopRef"       => "IntPtr",
      "AEEventClass"       => "IntPtr",
      "AEEventID"          => "int",
      "AEReturnID"         => "int",
      "AETransactionID"    => "int",
      "AEKeyword"          => "void",
      "IBAction"           => "void",
      "DescType"           => "void",
      "UTF32Char"          => "char",
#Method Names
      "delegate"           => "objcDelegate",
      "string"             => "objcString",
      "class"              => "objcClass",
      "object"             => "objcObject",
    )

#  $Log: typeConversion.pl,v $
#  Revision 1.3  2004/06/18 22:36:18  gnorton
#  Better .cs handling; still broken but closer
#
#  Revision 1.2  2004/06/17 13:06:27  urs
#  - release cleanup: only call release when requested
#  - loader cleanup
#
#  Revision 1.1  2004/06/17 04:32:43  cjcollier
#  A map between objC and C# types
#
