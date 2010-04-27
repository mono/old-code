//
// Booc.boo
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

class Booc (NetCompiler):

    def constructor ():
        pass
    
    def constructor (args as Hash):
        PopulateFromHash (args)

    override Compiler as string:
        get: return "booc"

    protected override def GenerateCustomArgs () as string:
        return "-nologo "

    protected override def GenerateDebugArg () as string:
        if Debug:
            return "-debug+ "
        return "-debug- "

def booc_target (output as string, sources as List):
    booc_target (output, sources, null)

def booc_target (output as string, sources as List, action as callable):
    booc_target (output, sources, null, action)

def booc_target (output as string, sources as List, extra_args as Hash):
    booc_target (output, sources, extra_args, null)

def booc_target (output as string, sources as List, extra_args as Hash, action as callable):
    if not extra_args:
        extra_args = Hash ()
    extra_args["out"] = output
    extra_args["sources"] = sources
    booc_target (extra_args, action)

def booc_target (args as Hash):
    booc_target (args, null)

def booc_target (args as Hash, action as callable):
    net_compile_target (typeof (Booc), args, action)

