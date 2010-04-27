//
// Exec.boo
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

def _ (command as string):
    return _ (command, true)

def __ (command as string):
    return _ (command, false)

def _ (command as string, echo as bool):
    ofs = command.IndexOf (" ")
    if ofs < 0:
        return exec (command)
    return exec (command[:ofs], command[ofs + 1:], echo)

def exec (command as string):
    return exec (command, null as string, true)

def exec (command as string, args as List):
    return exec (command, join (args), true)

def exec (command as string, args as string):
    return exec (command, args, true)

def exec (command as string, args as string, echo as bool) as string:
    if echo:
        print "${command} ${args}"
    proc = shellp (command, args)
    proc.WaitForExit ()
    stderr = proc.StandardError.ReadToEnd ()
    if not String.IsNullOrEmpty (stderr):
        Console.Error.Write (stderr)
    if proc.ExitCode != 0:
        die ("*** [${Target.Current}] Error ${proc.ExitCode}")
    return proc.StandardOutput.ReadToEnd ().Trim ()

