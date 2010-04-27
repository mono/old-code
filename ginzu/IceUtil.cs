// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceUtil.cs
//
// Written by:
//   Vladimir Vukicevic <vladimir@sparklestudios.com>
//
// Copyright (C) 2003 Sparkle Studios, LLC
//
// This file is distributed under the terms of the license
// agreement contained in the LICENSE file in the top level
// of this distribution.
//

using System;
using System.Text;
using System.Reflection;

namespace Ice {
  internal sealed class IceUtil {
    // the UTF8 encoding to be used in slice
    private static Encoding _utf8;
    internal static Encoding UTF8 {
      get {
        if (_utf8 == null) {
          lock (typeof (Encoding)) {
            if (_utf8 == null) {
              _utf8 = new UTF8Encoding (false, true);
            }
          }
        }
        return _utf8;
      }
    }

    internal static Type IceNameToType (string s) {
      string tname = s.TrimStart(':').Replace("::", ".");
      return FindType (tname);
    }

    internal static string TypeToIceName (Type t) {
      // string icename = "::" + t.FullName.Replace(Type.Delimiter, "::");
      string icename = "::" + t.FullName.Replace(".", "::");
      return icename;
    }

    internal static Type FindType (string typeName) {
      Assembly [] assemblies = AppDomain.CurrentDomain.GetAssemblies();
      foreach (Assembly ass in assemblies) {
        Type t = ass.GetType (typeName);
        if (t != null)
          return t;
      }
      return null;
    }
  }
}
