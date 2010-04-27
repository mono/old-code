// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceCore.cs
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
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;

namespace Ice {

  // Attribute that can be applied to Ice operations to
  // indicate how the operation is to be executed (nonmutating,
  // idempotent, etc.).  Note that the C# server runtime currently
  // doesn't do anything with these.
  [AttributeUsage(AttributeTargets.Method)]
  public class OperationModeAttribute : Attribute {
    public OperationModeAttribute() : this (OperationMode.Normal) {
    }

    public OperationModeAttribute(OperationMode m) {
      mode = m;
    }

    public OperationMode mode;
  }

  // AsProxy attribute is applied to parameters and
  // return values that expect a proxy instead of an instance;
  // i.e. "Foo*" in slice instead of "Foo".  Using AsProxy on
  // a parameter or return value causes an ObjRef to be generated
  // and marshalled.
  [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
  public class AsProxy : Attribute {
    public AsProxy () { }
  }

  // IceMarshallingAttribute is set on types (classes and structs)
  // for which an explicit marshal/unmarshal is defined, instead of
  // going through the generic Reader/Writer.
  public delegate void IceMarshalDelegate (object o, Stream outStream);
  public delegate void IceUnmarshalDelegate (object o, Stream inStream);

  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
  public class IceMarshallingAttribute : Attribute {
    private IceMarshalDelegate _marshal;
    private IceUnmarshalDelegate _unmarshal;

    public IceMarshallingAttribute() {
      _marshal = null;
      _unmarshal = null;
    }

    public IceMarshallingAttribute (IceMarshalDelegate m,
                                    IceUnmarshalDelegate u)
    {
      _marshal = m;
      _unmarshal = u;
    }

    public IceMarshalDelegate Marshal {
      get {
        return _marshal;
      }
      set {
        _marshal = value;
      }
    }

    public IceUnmarshalDelegate Unmarshal {
      get {
        return _unmarshal;
      }
      set {
        _unmarshal = value;
      }
    }
  }

  // generic Ice User Exception
  public class UserException : Exception {
  }

  // Sequential layout isn't strictly necessary for this, since it
  // won't ever be marshalled, but whatever.
  public class LocalObject {
    // nothing
  }

  // Generic Ice identity, with some convenience functions.
  [StructLayout(LayoutKind.Sequential)]
  public struct Identity {
    public string name;
    public string category;

    public Identity (string n) { name = n; category = String.Empty; }

    public Identity (string n, string c) { name = n; category = c; }

    public string ToUri () {
      if (category == String.Empty || category == null)
        return name;
      return name + "/" + category;
    }

    public override bool Equals (object other) {
      Ice.Identity oi = (Ice.Identity) other;
      return (oi.name == name && oi.category == category);
    }

    public override int GetHashCode() {
      return name.GetHashCode() + category.GetHashCode();
    }

    public override string ToString() {
      if (category != null && category != String.Empty)
        return "[" + name + ":" + category + "]";
      else
        return "[" + name + "]";
    }
  }


  // from Current.ice 
  public class Context : Ice.Dictionary {
    public Context ()
      : base (typeof(string), typeof(string))
    { }
  }

  public enum OperationMode : byte {
    Normal,
    Nonmutating,
    Idempotent
  }

} // namespace Ice

