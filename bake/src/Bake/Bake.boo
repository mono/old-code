//
// Bake.boo
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

def die (message as string):
    log (message)
    Environment.Exit (1)

def log (message as string):
    print "bake: ${message}"

def log_action (message as string):
    print message if Globals.LogActions

def bake ():
    for subdir as string in AutoBake.SUBDIRS:
        pushd (subdir)
        Context (array (string, Globals.TargetArgs))
        popd ()

    if Globals.TargetArgs.Count > 0:
        for target_name in Globals.TargetArgs:
            target as Target = Target.Find (target_name)
            if not target:
                die ("No rule to bake target `${target_name}'.")
            target.Update ()
    elif Target.First:
        Target.First.Update ()
    elif not AutoBake.SUBDIRS or AutoBake.SUBDIRS.Count == 0:
        die ("No targets.")

Context (argv)

