#!/usr/bin/perl -w
#
#  genstubs.pl
#
#  Authors
#    - C.J. Collier, Collier Technologies, <cjcollier@colliertech.org>
#    - Urs C. Muff, Quark Inc., <umuff@quark.com>
#    - Kangaroo, Geoff Norton
#    - Adham Findlay
#
#  Copyright (c) 2004 Quark Inc. and Collier Technologies.  All rights reserved.
#
#	$Header: /home/miguel/third-conversion/public/cocoa-sharp/generator/genstubs.pl,v 1.20 2004/06/19 02:34:32 urs Exp $
#

use strict;
use File::Basename;
use FindBin;

# Read the type map from typeConversion.pl
my %typeMap;
{
    no strict "vars";
    my $f = "$FindBin::Bin/typeConversion.pl";
    open TYPES, "<$f" or die "Couldn't open $f: $!";
    (%typeMap) = (eval join("", <TYPES>));
    close TYPES;
    use strict "vars";
}


$| = 1;
my %protocols = ();
my %imported = ();

makeDirs();

# TODO: don't hardcode those paths, this should work for any objc file, 
# for instance a client project header file
my $foundationPath = "/System/Library/Frameworks/Foundation.framework/Headers";
my $appKitPath     = "/System/Library/Frameworks/AppKit.framework/Headers";

# output interfaces
parseDir($foundationPath, "foundation");
parseDir($appKitPath, "appkit");


# Some ideas for ParseMethod:
# - Only parse the method!  Store method parts in the %objC hash
sub parseMethod {
    my $origmethod = shift();
    my $class      = shift();
    my $methodHash = shift();
    my @return     = ();

    chomp($origmethod);

    # Check for unsupported methods and return commented function
    # Unsupported methods include:
    # <.*>
    if($origmethod =~ /<.*>/ or
       # varargs don't work.
       # Need another method of passing variable number of args (...)
       # until then, comment such methods as UNSUPPORTED
       $origmethod =~ /\.\.\./
      ) {
        return ("unsupported" => $origmethod);
    }

    # It seems that methods take one of two formats.  Zero arguments:
    # - (RETURNTYPE)MethodName;
    # or N arguments
    # - (RETURNTYPE)MethodName:(TYPE0)Arg0 ... ArgNName:(TYPEN)ArgN;

    unless($origmethod =~ /\s*([+-])\s*(?:\(([^\)]+)\))?(.+)/ ){
        print("Couldn't parse method: $origmethod\n");

        return ("unsupported" => $origmethod);
    }

    my $methodType = $1;
    my $retType = ($2 ? $2 : "id");
    my $remainder = $3;

    my $isClassMethod =
        (defined($methodType) ? ($methodType eq "+") : 0);

    $retType =~ s/oneway //;

    # get rid of comments
    $remainder =~ s://.*::;
    $remainder =~ s:/\*.*\*/::;
    
    # These arrays store our method names, their arg names and types
    my(@methodName, @name, @type);

    my $noarg_rx = '^\s*(\w+)\s*([;\{]|$)';
    my $arg_rx   = '(\w+):\s*(?:\(([^\)]+)\))?\s*(\w+)?(?:\s+|;)';

    # If there are no arguments (only matches method name)
    if($remainder =~ /$noarg_rx/){
        push(@methodName, $1);

    # If there are arguments, parse them
    }elsif($remainder =~ /$arg_rx/){
        (my(@remainder)) = ($remainder =~ /$arg_rx/g);

        # Fill our arrays from the remainder of the parsed method
        while(@remainder){
            push( @methodName,  shift @remainder );

            my $argType = shift @remainder;
            my $argName = shift @remainder;

            $argType = "id" unless $argType;

            unless ($argName){
                $argName = $argType;
                $argType = "id";
            }
            
            push( @type,        $argType );
            push( @name,        $argName );
        }

    # If we can't parse the method, complain
    }else{
        print("Couldn't parse method: $origmethod\n");
        return("unsupported" => $origmethod);
    }

    # Who receives this message?
    # What object will we be sending messages to?
    my($receiver, $logLine);

    # Build the params and message
    my(@message, @params);

    if(int(@methodName) == 1 && int(@name) == 0){
        push(@message, $methodName[0]);

    }else{
        for(my $i = 0; $i < int @methodName; $i++){
            push(@params, "$type[$i] p$i");
            push(@message, "$methodName[$i]:p$i");
        }
    }

    # The objc message to send the object
    my $message = join(" ",  @message);

    # If the method is a class method
    if($isClassMethod){
        unshift(@params, "Class CLASS");
        $receiver = "CLASS";
        $logLine =
            "\tif (!CLASS) CLASS = [$class class];\n";
        $class .= '_';

    # If the method is an instance method
    }else{
        unshift(@params, "$class* THIS");
        $receiver = "THIS";
        $logLine = "";

    }

    # The fully-qualified C function name separated by _s (:s don't work)
    my $methodName = join("_",  $class, @methodName);

    # Add the log call
    $logLine .= "\tNSLog(\@\"$methodName: \%\@\\n\", $receiver);";

    # The parameters to the C function
    my $params     = join(", ", @params);

    if(exists $methodHash->{$methodName}){
        print("\t\tDuplicate method name: $methodName\n");
        return ("dup", $origmethod);
    }
    
    $methodHash->{$methodName} = "1";

    return ( "method name"        => $methodName,
             "method parts"       => [ @methodName ],
             "arg names"          => [ @name ],
             "arg types"          => [ @type ],
             "message parts"      => [ @message ],
             "message"            => $message,
             "is class method"    => $isClassMethod,
             "log line"           => $logLine,
             "params"             => $params,
             "method name"        => $methodName,
             "receiver"           => $receiver,
             "return type"        => $retType,
             "param list"         => [ @params ],
             "original method"    => $origmethod,

           );

}

# Parse file
# TODO: Read the entire file in and for GOD'S SAKE! Don't parse line by line!
my %parsedFiles = ();
sub parseFile {
    # The name of the file we will be parsing
    my $filename = shift();

    # Classes that have been imported in this traversal
    my $currentImports = (defined $_[0] ? shift() : {});

    # Note that we have imported this file so that we don't do it again
    $currentImports->{$filename} = 1;

    if(exists $imported{$filename}){
        return @{ $imported{$filename} };
    }

    # Set to undef when started, 1 when finished
    $parsedFiles{$filename} = undef;

    my %methods = ();
    my $genDate = scalar localtime;

    my @protocolOut = ();

    (my($name, $path, $suffix)) = fileparse($filename, ".h");

    $filename =~ m:.*/([^\.]*)\.[^/]+/:;
    my $dirpart = $1;

    my $skip = 0;
    my $isProtocol = 0;
    my $isInterface = 0;
    my $protocol;

    my @imported = ();
    my @objC;

    my %common = 
        ( interface            => $name,
          super                => undef,
          isInterface          => undef,
          isProtocol           => undef,
        );

    open(my $fh, "<$filename") or die "Couldn't open $filename: $!";

    while(my $line = <$fh>) {
        chomp $line;

        commentsBeGone(\$line, $fh);

        my %objC;
        my %cSharp;

        # Traverse import lines
        if($line =~ m:#import\s+[<"]([^>"]+)[>"]:){
            my $importString = $1;
            (my($importName, $importDir, $importSuffix)) =
                fileparse($importString, ".h");

            my($fqImportDir, $fqImportFile) = ("", "");

            # Are we importing from the Appkit or the Foundation dirs?
            if($importDir eq "AppKit/"){
                $fqImportDir = $appKitPath;
            }elsif($importDir eq "Foundation/"){
                $fqImportDir = $foundationPath;
            }

            $fqImportFile = "$fqImportDir/$importName.h";

            # If the import dir is either AppKit or Foundation
            # And we haven't already imported this file, do so now
            unless($fqImportDir){
                # Not an appkit or foundation include file.

            }elsif($fqImportDir && 
                   !exists($currentImports->{$fqImportFile})){

                # Verify that this file exists
                print(" ----------------------- \n",
                      " This SHOULD NOT HAPPEN! \n",
                      " ----------------------- \n",
                      " '$fqImportFile' does not exist \n",
                      " But import string is '$importString' \n",
                     ) unless -f "$fqImportFile";

                # Cache the results of the parse
                if(!exists $imported{$fqImportFile}){

                    # This would cause an infinite loop.
                    if(exists $parsedFiles{$fqImportFile} &&
                       $parsedFiles{$fqImportFile} == undef){
                        die "Infinite loop detected";
                    }

                    # Note that we have already imported this file
                    $currentImports->{$fqImportFile} = 1;
                    
                    $imported{$fqImportFile} =
                        [ parseFile($fqImportFile, { %$currentImports }) ];

                }
            }
        # Determine the interface we are in
        }elsif($line =~ /^\s*\@interface\s+(\w+)(.*)/){
            @common{'isInterface', 'interface'} = (1, $1);
            
            my $remainder = $2;

            $remainder =~ /\s*(?::\s*(\w+)\s*?)?(?:<([^>]+)>)?/;

            # Capture superclass and protocols
            @common{'super', 'protocols'} = ($1, $2);

            # If the interface has a superclass
            if(exists $common{super} && defined $common{super}){
                # TODO: Do something in this case.
            }

            # If the interface follows a particular protocol
            if(exists $common{protocols} && defined $common{protocols}){
                my @protocols = split(/,\s*/, $common{protocols});

                print(" $common{interface} implements: $common{protocols}" );

                # Place the protocol definitions directly into the interface
                foreach my $p (@protocols){
                    if(! exists $protocols{$p} ) {
                        print(" WARNING: Protocol $p is missing\n");
                        next;
                    }
                    print(" lines read from protocol $p: ",
                          int @{ $protocols{$p} }, "\n");

                    foreach my $protoLine (@{ $protocols{$p} }){
                        # DONE (since already filtered when collected): only parseMethod on /^\s*[+-]/ lines
                        push(@objC,
                             { parseMethod($protoLine, $common{interface}, \%methods),
                               %common 
                             });
                    }
                }
            }

        # Are we processing a @protocol line?
        }elsif($line =~ /\@protocol\s+(\w+)/){
            my $remainder = $1;

            $remainder =~ /(\w+)\s*(?:<([^>]+)>)?/;
            $protocol = $1;

            # TODO: Do something with extended class information
            my $extendedClasses = $2;
            my @extendedClasses;

            if($extendedClasses){
                @extendedClasses = split(/,\s*/, $extendedClasses);
            }

            $isProtocol = 1;

        }elsif($line =~ /\@end/ ){

            @common{'class', 'super'} = (undef, undef);
            
            if($isProtocol == 1){
                $protocols{$protocol} = [ @protocolOut ];
                $isProtocol = 0;
            }elsif($isInterface == 1){
                $isInterface = 0;
            }

        # If this is a class or instance method definition
        }elsif($line =~ /^\s*[+-]/){
            # For lines that end in a definition,
            # Replace { ... } with a semicolon
            $line =~ s/\{[^\}]*\}\s*/;/;

            # If the line doesn't end with a semicolon, whitespace, end of line
            # Do the following until it does
            while($line !~ /;\s*$/ ){

                $line =~ s://.*::;
                # Append the next line
                $line .= <$fh>;
                # Remove trailing newline
                chomp $line;
                # Get rid of comments
                commentsBeGone(\$line, $fh);
                # Replace { ... } with a semicolon
                $line =~ s/\{[^\}]*\}/;/;
            }

            if($isProtocol){
                push(@protocolOut, $line);

            }else{
                push(@objC,
                     { parseMethod($line, $common{interface}, \%methods),
                       %common 
                     });
            }
        }
    }

    $filename =~ m:.*/([^\.]*)\.[^/]+/:;
    my $destdir = lc $1;
    my @uniq;
    
    {
        my @objCOut = ("/* Generated by genstubs.pl",
                   " * (c) 2004 kangaroo, C.J. and Urs",
                   " * Generation date: $genDate",
                   " */",
                   "",
                   "",
                   "#import <$dirpart/$name.h>",
                   "#import <Foundation/NSString.h>",
                   "",
                  );
    
        # Generate the objC/C wrapper
        foreach my $objC (@objC){
            if(exists $objC->{unsupported}){
                push(@objCOut, "/* UNSUPPORTED: \n$objC->{unsupported}\n */\n\n");
                next;
            }
    
            push(@objCOut, genObjCStub(\%methods, %$objC));
            push(@uniq, $objC);
        }
    
        my $stubfile = "src/$destdir/${name}_stub.m";
        
        open OUT, ">$stubfile" or die "Can't open $stubfile: $!";
        print OUT join($/, @objCOut);
        close OUT;
    }

    {
        # TODO: don't hardcode mappings: things from /System/Library/Frameworks will be prefixed with Apple
        my $namespace;

        if($destdir eq "appkit"){
            $namespace = "Apple.Appkit";
        }elsif($destdir eq "foundation"){
            $namespace = "Apple.Foundation";
        }else{
            print("This shouldn't happen.  \$destdir = '$destdir'\n");
        }

        my @csOut = ("/* Generated by genstubs.pl",
                   " * (c) 2004 kangaroo, C.J. and Urs",
                   " * Generation date: $genDate",
                   " */",
                   "",
                   "using System;",
                   "using System.Collections;",
                   "using System.Runtime.InteropServices;",
                   "",
                   "namespace $namespace {",
                   "    using Apple.Foundation;",
                   "",
	               "    public class $name : NSObject {",
	               "        protected internal static IntPtr ${name}_class = Class.Get(\"$name\");",
                   "",
                  );

        # Generate the C# wrapper
        push(@csOut, "        #region -- Generated Stubs --", "");
        foreach my $objC (@uniq){
            push(@csOut, genCSharpStub(%$objC));
        }
        push(@csOut, "        #endregion", "");

        push(@csOut, "        #region -- Instance Methods --", "");
        foreach my $objC (@uniq){
            push(@csOut, genCSharpInstanceMethod(%$objC));
        }
        push(@csOut, "        #endregion", "");

        push(@csOut, 
            "",
            "    }",
            "}");

        my $wrapperFile = "src/$namespace/$name.cs";

        #if (!(-r "$wrapperFile")) {
        #    print "\n$wrapperFile does not exist: creating\n";
            open OUT, ">tmp/$wrapperFile" or die "Can't open tmp/$wrapperFile: $!";
            print OUT join($/, @csOut);
            close OUT;
        #}
    }

    my $numMethods = int(keys %methods);
    print " $numMethods methods in $name.\n";

    $parsedFiles{$filename} = 1;

    return keys %{ $currentImports };

}

sub parseDir {
    my $sourcedir = shift();

    # Hack to parse NSO before anything else
    opendir(my $dh, $sourcedir);

    my($name, $path, $suffix);
    print "Processing directory: $sourcedir:\n";

    foreach my $filename (readdir($dh)) {
        next if $filename =~ /^\./;
        next unless $filename =~ /^.*\.h$/;

        ($name, $path, $suffix) = fileparse("$sourcedir/$filename", ".h");

        parseFile("$path/$filename");
    }

}

sub makeDirs {
    #temporary while not clobbering old .cs 
    unless(-d "tmp"){
        mkdir "tmp" or die "Couldn't make dir 'tmp': $!";
    }
    #temporary while not clobbering old .cs 
    unless(-d "tmp/src"){
        mkdir "tmp/src" or die "Couldn't make dir 'tmp': $!";
    }
    #temporary while not clobbering old .cs 
    unless(-d "tmp/src/Apple.Foundation"){
        mkdir "tmp/src/Apple.Foundation" or die "Couldn't make dir 'tmp': $!";
    }
    #temporary while not clobbering old .cs 
    unless(-d "tmp/src/Apple.AppKit"){
        mkdir "tmp/src/Apple.AppKit" or die "Couldn't make dir 'tmp': $!";
    }
    unless(-d "src"){
        mkdir "src" or die "Couldn't make dir 'src': $!";
    }
    unless(-d "src/appkit"){
        mkdir "src/appkit" or die "Couldn't make dir 'src/appkit': $!";
    }
    unless(-d "src/foundation"){
        mkdir "src/foundation" or die "Couldn't make dir 'src/foundation': $!";
    }
}

sub commentsBeGone()
{
    my $line = shift();
    my $FH = shift();

    # Rid ourselves of multi-line comments
    if( $$line =~ m:/\*: ){
        while( $$line !~ m:/\*.*\*/:){
            $$line .= <$FH>;
            chomp $$line;
        }

        $^W = 0;
        $$line =~ s{
                     /\*         ##  Start of /* ... */ comment
                     [^*]*\*+    ##  Non-* followed by 1-or-more *'s
                     (
                       [^/*][^*]*\*+
                     )*          ##  0-or-more things which don't start with /
                                 ##    but do end with '*'
                     /           ##  End of /* ... */ comment

                   |         ##     OR  various things which aren't comments:

                     (
                       "           ##  Start of " ... " string
                       (
                         \\.           ##  Escaped char
                       |               ##    OR
                         [^"\\]        ##  Non "\
                       )*
                       "           ##  End of " ... " string

                     |         ##     OR

                       '           ##  Start of ' ... ' string
                       (
                         \\.           ##  Escaped char
                       |               ##    OR
                         [^'\\]        ##  Non '\
                       )*
                       '           ##  End of ' ... ' string

                     |         ##     OR

                       .           ##  Anything other char
                       [^/"'\\]*   ##  Chars which doesn't start a comment, string or escape
                     )
                   }{$2}gxs;
        $^W = 1;

        $$line =~ s://.*::;
    }
}

sub genObjCStub {
    my $metods = shift();
    my %objC = @_;

    if(exists $objC{dup}){
        # Duplicate.  Don't return anything
        return ();
    }

    # Will we be returning?
    my $retter = ($objC{"return type"} =~ /void/) ? "" : "return ";

    # Return the lines of the wrapper
    return ( "$objC{'return type'} $objC{'method name'} ($objC{params}) {",
             $objC{"log line"},
             "\t${retter}[$objC{receiver} $objC{message}];",
             "}",
             "",
           );
}

sub genCSharpStub {
    my %objC = @_;

    # BUG: Why are we getting empty method names in here?
    return unless defined($objC{'method name'});

    if(  ( ($objC{'return type'} !~ /(\w+)\s+\*/) && (!defined($typeMap{$objC{'return type'}})))) {
        print "WARNING: Not processing " . $objC{'method name'} . " because I dont know how to map: " . $objC{'return type'} . "\n";
        return;
    }
    my $type = (($objC{'return type'} =~ /(\w+)\s*\*/) ? "IntPtr /*$1*/" : $typeMap{$objC{'return type'}});
    my @params = ();
    my @names = defined $objC{'arg names'} ? @{ $objC{'arg names'} } : ();
    my @types = defined $objC{'arg types'} ? @{ $objC{'arg types'} } : ();

    if ( $objC{'is class method'}) {
        push(@params,"IntPtr CLASS");
    } else {
        push(@params,"IntPtr THIS");
    }

    for(my $i = 0; $i < int @types; $i++){
        if(  ( ($types[$i] !~ /(\w+)\s+\*/) && (!defined($typeMap{$types[$i]})))) {
            print "WARNING: Not processing " . $objC{'method name'} . " because I dont know how to map: " . $types[$i] . "\n";
            return;
        }
        my $t = (($types[$i] =~ /(\w+)\s*\*/) ? "IntPtr /*$1*/" : $typeMap{$types[$i]});
        push(@params, "$t p$i/*$names[$i]*/");
    }

    my $params = join(", ", @params);

    # [DllImport("AppKitGlue")]
    # protected internal static extern void NSButton_setTitle(IntPtr THIS, IntPtr aString);
    return (
        "        [DllImport(\"AppKitGlue\")]",
        "        protected internal static extern $type $objC{'method name'} ($params);"
    );
}


sub genCSharpInstanceMethod {
    my %objC = @_;

    #BUG: Why are we getting undefined method names in here
    return unless defined($objC{'method name'});

    if(  ( ($objC{'return type'} !~ /(\w+)\s+\*/) && (!defined($typeMap{$objC{'return type'}})))) {
        print "WARNING: Not processing " . $objC{'method name'} . " because I dont know how to map: " . $objC{'return type'} . "\n";
        return;
    }
    my $type = (($objC{'return type'} =~ /(\w+)\s*\*/) ? "IntPtr /*$1*/" : $typeMap{$objC{'return type'}});
    my $retter = ($type =~ /void/) ? "" : "return ";
    my @args = ();
    my @params = ();
    my @names = defined $objC{'arg names'} ? @{ $objC{'arg names'} } : ();
    my @types = defined $objC{'arg types'} ? @{ $objC{'arg types'} } : ();
    my @messageParts = @{ $objC{'message parts'} };
    my $methodName = substr($objC{'method name'}, index($objC{'method name'}, "_")+1);
    $methodName = $1 if ($methodName =~ /^_(.+)/);
    $methodName = (defined($typeMap{$methodName}) ? $typeMap{$methodName} : $methodName);

    if ($objC{'is class method'}) {
        $type = "static $type";
        push(@params, "IntPtr.Zero");
    } else {
        push(@params, "Raw");
    }

    for(my $i = 0; $i < int @types; $i++){
        if(  ( ($types[$i] !~ /(\w+)\s+\*/) && (!defined($typeMap{$types[$i]})))) {
            print "WARNING: Not processing " . $objC{'method name'} . " because I dont know how to map: " . $types[$i] . "\n";
            return;
        }
        my $t = (($types[$i] =~ /(\w+)\s*\*/) ? "IntPtr /*$1*/" : $typeMap{$types[$i]});
        push(@args, "$t p$i/*$names[$i]*/");
        push(@params, "p$i/*$names[$i]*/");
    }

    my $args = join(", ", @args);
    my $params = join(", ", @params);

    # void setTitle(string aString);
    return (
        "        public $type $methodName ($args) {",
        "            $retter$objC{'method name'} ($params);",
        "        }"
    );
}

#sub convertTypeGlue {
#    my $type = shift();
#    
#    return "IntPtr /*(??)*/" unless defined $type;
#    
#    if ($type eq "BOOL") {
#        return "bool";
#    } elsif ($type eq "long long") {
#        return "Int64";
#    } elsif ($type eq "unsigned long long") {
#        return "UInt64";
#    } elsif ($type eq "unsigned") {
#        return "uint";
#    } elsif($type eq "id" || $type eq "Class" || $type eq "SEL" || $type eq "IMP" || $type =~ /.*\*$/) {
#        return "IntPtr /*($type)*/";
#    }
#    if($type =~ /^unsigned (\w+)$/) {
#        $type = "u" . $1;
#    } 
#    
#    return $type;
#}

#sub convertType {
#    my $type = shift();
#    
#    return "object /*(??)*/" unless defined $type;
#    
#    if ($type eq "BOOL") {
#        return "bool";
#    } elsif ($type eq "unsigned") {
#        return "uint";
#    } elsif($type eq "id") {
#        return "object /*($type)*/";
#    } elsif($type eq "Class") {
#        return "Class";
#    } elsif($type eq "SEL") {
#        return "string /*SEL*/";
#    } elsif($type eq "IMP") {
#        return "IntPtr /*IMP*/";
#    } elsif($type =~ /NSString.*\*$/) {
#        return "string /*($type)*/";
#    } elsif($type =~ /(\w+).*\*$/) {
#        return "$1 /*($type)*/";
#    }
#    
#    return $type;
#}

sub getCSharpHash {
    my %objC = @_;

    my %cSharp =
	    ( "arg names" => $objC{"arg names"},
	    );

    return (
        
    );
}

#	$Log: genstubs.pl,v $
#	Revision 1.20  2004/06/19 02:34:32  urs
#	some cleanup
#
#	Revision 1.19  2004/06/18 22:41:40  gnorton
#	Forgot one case for __ statics
#	
#	Revision 1.18  2004/06/18 22:36:18  gnorton
#	Better .cs handling; still broken but closer
#	
#	Revision 1.17  2004/06/18 17:52:52  urs
#	Some .cs file gen improv.
#	
#	Revision 1.16  2004/06/18 15:09:31  gnorton
#	* Resolve some warning in the.cs generation
#	* Temporarily make our tmp directories if needed
#	* Why are we getting undefined %objC{'method name'} into our generators?
#	
#	Revision 1.15  2004/06/18 13:54:57  urs
#	*** empty log message ***
#	
#	Revision 1.14  2004/06/17 06:01:15  cjcollier
#		* typeConversion.pl
#		- Created.  Enter type mapping from ObjC to C#
#	
#		* genstubs.pl
#		- Reading in typeConversion.pl and building a hash from it
#		- Cleaned up unsupported function handling slightyl
#		- Created a %cSharp hash that will eventually be populated by %objC.  This is what inspired typeConversion.pl
#	
#	Revision 1.13  2004/06/17 02:55:38  urs
#	Some cleanup and POC of glue change
#	
#	Revision 1.12  2004/06/16 12:20:26  urs
#	Add CVS headers comments, authors and Copyright info, feel free to add your name or change what is appropriate
#	
#	Revision 1.11  2004/06/15 19:02:09  urs
#	Add headers
#	
#
