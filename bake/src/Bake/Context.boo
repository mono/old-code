//
// Context.boo
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
import System.IO
import System.Text
import System.Reflection
import System.Collections

import Boo.Lang.Compiler
import Boo.Lang.Compiler.IO
import Boo.Lang.Compiler.Pipelines

class Context:

    bakefile as string
    args = []

    [getter (AutoBake)]
    autobake = AutoBakeInst ()

    [getter (Targets)]
    targets = []

    [property (CurrentTarget)]
    current_target as Target

    [property (Stack)]
    public static stack = Stack ()

    public static Current as Context:
        get: 
            if Stack.Count > 0:
                return Stack.Peek () as Context
            return null

    def constructor (argv as (string)):
        args = List (argv)
        bf_index = args.IndexOf ("-f")

        if bf_index >= 0 and args.Count > bf_index + 1:
            bakefile = args[bf_index + 1]
            args.RemoveAt (bf_index)
            args.RemoveAt (bf_index)
        elif bf_index >= 0:
            die ("No bakefile specified for -f.")
        elif File.Exists ("Bakefile"):
            bakefile = "Bakefile"
        elif File.Exists ("bakefile"):
            bakefile = "bakefile"

        if not bakefile or not File.Exists (bakefile):
            die ("No targets specified and no Bakefile found.")

        booc_input as duck

        if "-pure" in args:
            booc_input = FileInput (bakefile)
        else:
            builder = StringBuilder ()
            builder.Append ("import Bake\n\n")
            using file = File.OpenText (bakefile):
                line as string
                while true:
                    line = file.ReadLine ()
                    if not line:
                        break
                    builder.Append (line)
                    builder.Append ("\n")

            builder.Append ("\nbake ()")
            booc_input = StringInput (bakefile, builder.ToString ())

        _booc = BooCompiler ()
        _booc.Parameters.Input.Add (booc_input)
        _booc.Parameters.Pipeline = CompileToMemory ()
        _booc.Parameters.Ducky = true
        
        for assembly as Assembly in AppDomain.CurrentDomain.GetAssemblies ():
            _booc.Parameters.References.Add (assembly)

        context = _booc.Run ()

        if context.GeneratedAssembly is null:
            print join (e for e in context.Errors, "\n")
        else:
            if not Globals.CommandLineArgs:
                Globals.CommandLineArgs = array (string, args)

            Stack.Push (self)
            entry = context.GeneratedAssembly.EntryPoint
            entry.Invoke (null, array (object, entry.GetParameters ().Length))
            Stack.Pop ()
