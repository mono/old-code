// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceCollectionWrappers.cs
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
using System.Collections;
using System.Reflection;

namespace Ice {

  // an Ice.Dictionary is really just a hash table
  // that has two extra fields to keep track of the key and
  // value type.  It needs to be initialized in the default
  // constructor of any class or struct that contains a
  // dictionary, so that the unmarshaller can correctly
  // read in enclosing dictionaries.
  public class DictionaryInfo : ICloneable {
    public Type keyType;
    public Type valueType;

    object ICloneable.Clone () {
      return this.Clone();
    }

    public DictionaryInfo Clone () {
      DictionaryInfo n = new DictionaryInfo ();
      n.keyType = keyType;
      n.valueType = valueType;
      return n;
    }
  }

  // note that this is flagged as abstract because instantiating
  // a Dictionary by itself isn't valid -- you must derive from this
  // class, and create a default constructor that will set the Types;
  // for example:
  //
  // public class StringStringDictionary : Ice.Dictionary {
  //    public StringStringDictionary ()
  //      : base (typeof(string), typeof(string))
  //    { }
  // }

  public abstract class Dictionary : System.Collections.Hashtable {
    public Ice.DictionaryInfo IceDictInfo;
    public Dictionary (Ice.DictionaryInfo dinfo) {
      IceDictInfo = dinfo.Clone();
    }

    public Dictionary (Type k, Type v) {
      IceDictInfo = new Ice.DictionaryInfo ();
      IceDictInfo.keyType = k;
      IceDictInfo.valueType = v;
    }
  }

}
