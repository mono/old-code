//
// Paths.boo
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
import System.Text.RegularExpressions

def combine (path1 as string, path2 as string):
    return Path.Combine (path1, path2)

def fullpath (path as string):
    return Path.GetFullPath (path)

def filename (path as string):
    return Path.GetFileName (path)

def pwd ():
    return Directory.GetCurrentDirectory ()

def cd (directory as string):
    Directory.SetCurrentDirectory (directory)

def pushd (path1 as string, path2 as string):
    pushd (Path.Combine (path1, path2))

def pushd (directory as string):
    Globals.PathStack.Push (pwd ())
    try:
        directory = fullpath (directory)
        cd (directory)
        print "bake[${Globals.PathStack.Count}]: Entering directory `${directory}'"
    except:
        die ("Directory `${directory}' does not exist.")

def popd ():
    if Globals.PathStack.Count > 0:
        print "bake[${Globals.PathStack.Count}]: Leaving directory `${pwd ()}'"
        cd (Globals.PathStack.Pop ())

def find () as List:
    return find (".")

def find (path as string) as List:
    return find (path, null)

def find (path as string, filter as callable) as List:
    paths = []
    try:
        for dir in Directory.GetDirectories (path):
            paths.Extend (find (dir, filter))
        for file in Directory.GetFiles (path):
            file = file[2:] if file.StartsWith ("./")
            if not filter or (filter and filter (file)):
                paths.Add (file)
        return paths
    except:
        return paths

def wildcard (mask as string) as List:
    return wildcard (".", mask)

def wildcard (mask as Regex) as List:
    return wildcard (".", mask)

def wildcard (path as string, mask as string) as List:
    mask = mask[2:] if mask.StartsWith ("./")
    mask = mask.Replace (".", "\\.")
    mask = mask.Replace ("*", ".*")
    mask = mask.Replace ("/", "\\/")
    return wildcard (path, Regex ("^${mask}$"))

def wildcard (path as string, mask as Regex) as List:
    return find (path) do (file as string):
        return file =~ mask

