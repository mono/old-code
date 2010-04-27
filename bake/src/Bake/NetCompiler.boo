//
// NetCompiler.boo
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace Bake

import System
import System.Collections

enum OutputTarget:
    Exe
    Library

abstract class NetCompiler:

    abstract Compiler as string:
        get: pass

    [Property (OutputFile)]
    output_file as string

    [Property (Sources)]
    sources as List

    [Property (References)]
    references as List

    [Property (Resources)]
    resources as object

    [Property (Debug)]
    debug as bool = false

    [Property (Unsafe)]
    unsafe as bool = false

    [Property (Target)]
    target as OutputTarget = OutputTarget.Exe

    def constructor ():
        pass

    def constructor (args as Hash):
        PopulateFromHash (args)

    public virtual def PopulateFromHash (args as Hash):
        OutputFile = args["out"] if args["out"]
        Sources = args["sources"] if args["sources"]
        References = args["references"] if args["references"]
        Resources = args["resources"] if args["resources"]
        Debug = args["debug"] if args["debug"]
        Unsafe = args["unsafe"] if args["unsafe"]
        Target = args["target"] if args["target"]

    protected virtual def GenerateCustomArgs () as string:
        return null

    protected virtual def GenerateOutputFileArg () as string:
        return "-out:${OutputFile} "

    protected virtual def GenerateDebugArg () as string:
        if Debug:
            return "-debug "
        return null

    protected virtual def GenerateUnsafeArg () as string:
        if Unsafe:
            return "-unsafe "
        return null

    protected virtual def GenerateTargetArg () as string:
        return "-target:${Target.ToString ().ToLower ()} "

    protected virtual def GenerateSourcesArgs () as string:
        return join (Sources).Trim ()

    protected virtual def GenerateReferencesArgs () as string:
        if not References:
            return null

        output = []
        for reference as string in References:
            if reference.StartsWith ('-'):
                output.Add (reference)
            else:
                output.Add ("-r:${reference}")

        return join (output).Trim () + " "

    protected virtual def GenerateResourcesArgs () as string:
        if not Resources:
            return null

        if not Resources isa Hash and not Resources isa List:
            raise ArgumentException ("Resources must be a Hash or a List")

        output = []
        for resource in Resources:
            if resource isa DictionaryEntry:
                res = cast (DictionaryEntry, resource)
                output.Add ("-resource:${res.Value},${res.Key}")
            else:
                output.Add ("-resource:${resource}")

        return join (output).Trim () + " "

    virtual def Compile ():
        command = GenerateCustomArgs ()
        command += GenerateDebugArg ()
        command += GenerateUnsafeArg ()
        command += GenerateOutputFileArg ()
        command += GenerateTargetArg ()
        
        refs = GenerateReferencesArgs ()
        if refs:
            command += refs

        res = GenerateResourcesArgs ()
        if res:
            command += res

        command += GenerateSourcesArgs ()

        exec (Compiler, command.Trim ())

def net_compile_target (type as Type, args as Hash, action as callable):
    target (args["out"] as string, args["sources"] as List) do:
        compiler = cast (NetCompiler, Activator.CreateInstance (type))
        compiler.PopulateFromHash (args)
        compiler.Compile ()
        if action:
            action ()

