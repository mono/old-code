//
// Target.boo
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

class Target:

    [getter (Name)]
    name as string

    [getter (Dependencies)]
    dependencies = []

    [getter (Action)]
    action as callable

    Dependency as string:
        get: return Dependencies[0]

    static Current as Target:
        get: return Context.Current.CurrentTarget
        set: Context.Current.CurrentTarget = value

    private static targets:
        get: return Context.Current.Targets
        
    static First as Target:
        get: 
            if targets.Count > 0:
                return targets[0] as Target
            return null

    static TargetCount:
        get: return targets.Count

    def constructor (name as string, dependencies as List, action as callable):
        self.dependencies = dependencies
        self.name = name
        self.action = action

    override def ToString () as string:
        return name

    def InvokeAction ():
        action ()

    def Update () as bool:
        if not dependencies and action:
            InvokeAction ()
            return true
        elif not dependencies and not action:
            return false

        for dep in dependencies:
            dep_target = Target.Find (dep)
            if dep_target:
                UpdateDependency (dep_target)
            elif not File.Exists (dep):
                die ("No rule to bake target `${dep}', needed by `${self}'.")

        if action and (not File.Exists (name) or TargetOlderThan (dependencies)):
            Current = self
            InvokeAction ()
            UpdateDependencies ()
            return true

        UpdateDependencies ()
        return false

    def UpdateDependencies ():
        for target as Target in targets:
            continue if target != self
            if target.Dependencies.Contains (name):
                UpdateDependency (target)

    def UpdateDependency (target as Target):
        if not target.Update () and File.Exists (target.Name) and target.Dependencies:
            log ("`${target}' is up to date.")

    def TargetOlderThan (file as string):
        time_1 = File.GetLastWriteTime (file)
        time_2 = File.GetLastWriteTime (name)
        return time_1 > time_2

    def TargetOlderThan (files as List):
        for file as string in files:
            if TargetOlderThan (file):
                return true
        return false

    static def Add (name as string, dependencies as List, action as callable):
        targets.Add (Target (name, dependencies, action))

    static def Find (name as string):
        for target as Target in targets:
            if target.Name == name:
                return target
        return null

private def NameIsRule (name as string):
    return name and len (name) > 2 and name[:2] == "%."

def target (name as string, dependencies as List, action as callable):
    deps = flatten (dependencies)
    if NameIsRule (name):
        for dep as string in deps:
            Target.Add (ch_ext (dep, name[2:]), [dep], action)
    else:
        Target.Add (name, deps, action)

def target (name as string, dependency as string, action as callable):
    if NameIsRule (name) and NameIsRule (dependency):
        output_ext = name[2:]
        for input_file as string in wildcard ("*.${dependency[2:]}"):
            Target.Add (ch_ext (input_file, output_ext), [input_file], action)
    else:
        Target.Add (name, [dependency], action)

def target (name as string, dependencies as List):
    Target.Add (name, dependencies, null)

def target (name as string, dependency as string):
    target (name, dependency, null as callable)

def target (name as string, action as callable):
    Target.Add (name, null, action)

def target (name as string):
    Target.Add (name, null, null)

