// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceProtocolReader.cs
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
using System.Collections;
using System.Diagnostics;

using System.Reflection;

namespace Ice {

  // ProtocolReader
  //
  // A ProtocolReader is attached to a stream, and understands how to
  // read objects marshalled in the Ice format.  ReadObject() is the
  // main entry point, expecting a type.  The caller must know whether
  // a proxy is to be read and call ReadObjectProxy instead, as there
  // is no way to know whether a proxy is to be read from the type.
  //
  // Note that after a message has been read, the reader must follow
  // up the reading with a call to ReadClassInstancesAndPatch() for
  // the class tree to be successfully reconstructed.

  public class ProtocolReader : BinaryReader {
    // for keeping track of class ID marshalling
    private Hashtable _classHash;
    private int _classCounter;

    // for keeping track of instance ID marshalling
    private Hashtable _instanceHash;
    private ArrayList _instancePatchList;

    private bool _usesClasses;

    public ProtocolReader (System.IO.Stream s) : base (s, IceUtil.UTF8) {
      ResetProtocol ();
    }

    public void ResetProtocol () {
      _classHash = new Hashtable();
      _classCounter = 1;
      _instanceHash = new Hashtable();
      _instancePatchList = new ArrayList();
      _usesClasses = false;
    }

    public byte[] ReadEncapsulation () {
      int capslength = ReadInt32();
      byte encapsMajor = ReadByte();
      byte encapsMinor = ReadByte();

      if (encapsMajor != 1 || encapsMinor != 0)
        throw new InvalidOperationException ("Encapsulation has wrong major/minor (" + encapsMajor + "." + encapsMinor + ")");

      return ReadBytes (capslength - 6);
    }

    public int ReadEncapsulationHeader () {
      int capslength = ReadInt32();
      byte encapsMajor = ReadByte();
      byte encapsMinor = ReadByte();

      if (encapsMajor != 1 || encapsMinor != 0)
        throw new InvalidOperationException ("Encapsulation has wrong major/minor (" + encapsMajor + "." + encapsMinor + ")");

      return capslength - 6;
    }

    // read an object of type t from the stream
    // does NOT handle reading classes, since these
    // have to be read by ref and patched
    public object ReadObject (Type t) {
      if (t.IsPrimitive) {
        if (t == typeof(bool))
          return ReadBoolean();
        if (t == typeof(byte))
          return ReadByte();
        if (t == typeof(short))
          return ReadInt16();
        if (t == typeof(int))
          return ReadInt32();
        if (t == typeof(long))
          return ReadInt64();
        if (t == typeof(float))
          return ReadSingle();
        if (t == typeof(double))
          return ReadDouble();

        throw new NotImplementedException ("ReadObject can't read primitive type " + t);
      }

      if (t == typeof(string))
        return ReadString();

      if (t.IsEnum) {
        Type ue = Enum.GetUnderlyingType (t);
        if (ue == typeof(byte)) {
          byte i = ReadByte();
          return Enum.ToObject (t, i);
        }

        if (ue == typeof(short)) {
          short i = ReadInt16();
          return Enum.ToObject (t, i);
        }
        
        if (ue == typeof(int)) {
          int i = ReadInt32();
          return Enum.ToObject (t, i);
        }

        throw new NotSupportedException ("ReadObject can't read enum with underlying type " + ue);
      }

      if (t.IsSubclassOf (typeof(Ice.Dictionary))) {
        object o = Activator.CreateInstance (t);
        Ice.Dictionary dict = o as Ice.Dictionary;
        return ReadDictionary (dict);
      }

      if (t.IsArray) {
        int sz = ReadSize();
        Type eltype = t.GetElementType();
        // System.Console.WriteLine ("Reading Array: {0} {1}", sz, eltype);
        Array ao = Array.CreateInstance (eltype, sz);

        if (IceChannelUtils.IceByValue (eltype))
        {
          for (int i = 0; i < sz; i++) {
            object elem = ReadObject (eltype);
            ao.SetValue (elem, i);
            // System.Console.WriteLine (" {0}: {1}", i, elem);
          }
        } else {
          // this is a class type (and isn't a string or dictionary)
          for (int i = 0; i < sz; i++) {
            int r = ReadClassInstanceRef();
            if (r == 0) {
              ao.SetValue (null, i);
            } else {
              _instancePatchList.Add (new ArrayPatchInfo (r, ao, i));
            }
          }
        }

        return ao;
      }

      if (t.IsValueType) {
        object o = Activator.CreateInstance (t);

        MethodInfo unmarshal = t.GetMethod ("ice_unmarshal",
                                            BindingFlags.Instance |
                                            BindingFlags.Public |
                                            BindingFlags.DeclaredOnly);
        if (unmarshal != null) {
          object[] args = new object[1];
          args[0] = this;
          try {
            unmarshal.Invoke (o, args);
          } catch (TargetInvocationException te) {
            throw te.InnerException;
          }
        } else {
          foreach (FieldInfo field in t.GetFields()) {
            if (IceChannelUtils.IceByValue (field.FieldType))
            {
              object elem = ReadObject (field.FieldType);
              field.SetValue (o, elem);
            } else {
              // this is a class type
              int r = ReadClassInstanceRef ();
              if (r == 0) {
                field.SetValue (o, null);
              } else {
                _instancePatchList.Add (new FieldPatchInfo (r, o, field));
              }
            }
          }
        }
        return o;
      }

      Console.WriteLine ("ReadObject: can't read type " + t);
      throw new NotSupportedException ("ReadObject: can't read type " + t);
    }

    public Ice.Object ReadObjectProxy (Type t) {
      // read the proxy data header
      Ice.ProxyData pd = (Ice.ProxyData) ReadObject (typeof (Ice.ProxyData));

      // next read a size; if it's > 0, it's the number
      // of proxy endpoints that follow.  if it's == 0,
      // it's followed by a string identifying the object
      // locator.
      int sz = ReadSize();
      if (sz == 0) {
        string locator = ReadString();
        Console.WriteLine ("Proxy sz == 0, locator = " + locator);
        return null;
      }

      for (int i = 0; i < sz; i++) {
        short ptype = (short) ReadObject (typeof (short));
        if (ptype == 1) {                         // tcp endpoint
          int capSize = ReadEncapsulationHeader ();
          string host = ReadString();
          int port = ReadInt32();
          int timeout = ReadInt32();
          bool compress = ReadBoolean();

          string url = "ice://" + host + ":" + port + "/" + pd.id.name;
          Trace.WriteLine ("ReadObjectProxy: Activating " + url);
          // activate the object
          return (Ice.Object) Activator.GetObject (t, url);
        } else if (ptype == 2) {                  // ssl endpoint
          Console.WriteLine ("ReadObjectProxy: found SSL endpoint; ignoring");
        } else if (ptype == 3) {                  // udp endpoint
          Console.WriteLine ("ReadObjectProxy: found UDP endpoint; ignoring");
        }
      }

      return null;
    }

    public int ReadSize () {
      byte b = ReadByte();
      if (b < 255)
        return (int) b;

      return ReadInt32();
    }

    public override string ReadString () {
      int stringLen = ReadSize();
      // Console.WriteLine ("ReadString: stringLen: {0}", stringLen);
      byte[] buf = ReadBytes(stringLen);
      char[] strchars = IceUtil.UTF8.GetChars (buf, 0, stringLen);
      return new String (strchars);
    }

    // Read an instance ref and store a patch that will put it in
    // the params array at position pos
    public void ReadClassInstanceParameterRef (ArrayList args, int pos) {
      int r = ReadClassInstanceRef();
      if (r == 0) {
        args[pos] = null;
      } else {
        _instancePatchList.Add (new ArrayPatchInfo (r, args, pos));
      }
    }

    public int ReadClassInstanceRef () {
      int instID = ReadInt32();
      if (instID > 0)
        throw new InvalidOperationException ("ReadClassInstanceRef called, but instance ID is non-negative!");

      _usesClasses = true;

      return -instID;
    }

    // Read a Slice type name, which may be cached according
    // to the protocol rules.  Returns a name in Ice (C++)
    // form.
    public string ReadSliceName() {
      bool oldClass = ReadBoolean ();
      if (oldClass) {
        int id = ReadSize();
        return (String) _classHash[id];
      }

      int thisnum;
      thisnum = _classCounter++;
      string name = ReadString();

      _classHash[thisnum] = name;

      return name;
    }

    public object ReadSlice (object inst) {
      string sliceName = ReadSliceName();
      int sliceDataSize = ReadInt32();
      sliceDataSize -= 4;       // size includes the 4 bytes of the size itself

      Type sliceType = IceUtil.IceNameToType (sliceName);
      if (sliceType == null) {
        // this means we have no local definition of this slice;
        // we keep going.  Eventually we will hit an Ice.Object,
        // which is the terminating condition of this recursion,
        // since we know we have Ice.Object defined.
        ReadBytes (sliceDataSize);
        return ReadSlice (inst);
      }

      if (inst == null) {
        // if it's null, it has yet to be created, and this is
        // the first (i.e. most derived) slice that we
        // understand.
        inst = Activator.CreateInstance (sliceType);
      }

      MethodInfo unmarshal = sliceType.GetMethod ("ice_unmarshal",
                                                  BindingFlags.Instance |
                                                  BindingFlags.Public |
                                                  BindingFlags.DeclaredOnly);
      if (unmarshal != null) {
        object[] args = new object[1];
        args[0] = this;
        try {
          unmarshal.Invoke (inst, args);
        } catch (TargetInvocationException te) {
          throw te.InnerException;
        }
      } else {
        FieldInfo[] fields = sliceType.GetFields(BindingFlags.Instance |
                                                 BindingFlags.Public |
                                                 BindingFlags.DeclaredOnly);
        foreach (FieldInfo field in fields) {
          if (IceChannelUtils.IceByValue (field.FieldType))
          {
            object elem = ReadObject (field.FieldType);
            field.SetValue (inst, elem);
          } else {
            // this is a class type
            int r = ReadClassInstanceRef ();
            if (r == 0) {
              field.SetValue (inst, null);
            } else {
              _instancePatchList.Add (new FieldPatchInfo (r, inst, field));
            }
          }
        }
      }

      if (sliceType == typeof (Ice.Object))
        return inst;
      else
        return ReadSlice(inst);
    }

    public int ReadClassInstances() {
      if (!_usesClasses)
        return 0;

      int numInstances = ReadSize();
      if (numInstances == 0)
        return 0;

      for (int i = 0; i < numInstances; i++) {
        int instID = ReadInt32();
        object o = ReadSlice (null);

        _instanceHash[instID] = o;
      }

      return numInstances;
    }

    public void ReadClassInstancesAndPatch() {
      // read all instances
      while (ReadClassInstances() != 0)
        ;

      foreach (BasePatchInfo patch in _instancePatchList) {
        patch.Patch (_instanceHash);
      }
    }

    public object ReadDictionary (Ice.Dictionary dict) {
      Ice.DictionaryInfo dinfo = dict.IceDictInfo;

      int count = ReadSize();
      for (int i = 0; i < count; i++) {
        object k, v;
        bool haveKey, haveValue;

        if (IceChannelUtils.IceByValue (dinfo.keyType))
        {
          k = ReadObject (dinfo.keyType);
          haveKey = true;
        } else {
          k = ReadClassInstanceRef();
          if ((int) k == 0) {
            throw new InvalidOperationException ("Got NULL dictionary key, expected class ref for type " + dinfo.keyType);
          } else {
            haveKey = false;
          }
        }

        if (IceChannelUtils.IceByValue (dinfo.valueType))
        {
          v = ReadObject (dinfo.valueType);
          haveValue = true;
        } else {
          v = ReadClassInstanceRef();
          if ((int) v == 0) {
            v = null;
            haveValue = true;
          } else {
            haveValue = false;
          }
        }

        // if we have both the key and value, we put them in.
        // otherwise, we have to create a patch based on what
        // bits we have.
        if (haveKey && haveValue) {
          dict.Add (k, v);
        } else {
          if (haveKey) {
            _instancePatchList.Add (new DictionaryValuePatchInfo ((int) v, dict, k));
          } else if (haveValue) {
            _instancePatchList.Add (new DictionaryKeyPatchInfo ((int) k, dict, v));
          } else {
            _instancePatchList.Add (new DictionaryEntryPatchInfo ((int) k, dict, (int) v));
          }
        }
      }

      return dict;
    }


    // Protocol message handling
    public MessageHeader ReadMessageHeader() {
      return (MessageHeader) ReadObject (typeof(MessageHeader));
    }

    public MessageRequest ReadMessageRequest() {
      return (MessageRequest) ReadObject (typeof(MessageRequest));
    }

    public MessageBatchRequest ReadMessageBatchRequest() {
      return (MessageBatchRequest) ReadObject (typeof(MessageBatchRequest));
    }

    public MessageReplyHeader ReadMessageReplyHeader() {
      MessageReplyHeader mrh = new MessageReplyHeader();
      mrh.requestId = ReadInt32();
      byte b = ReadByte();
      mrh.replyType = (MessageReplyType) Enum.ToObject (typeof (MessageReplyType), b);
      return mrh;
    }

    public object ReadMessageReply (MessageReplyType mt) {
      if (mt == MessageReplyType.Success) {
        return (MessageReplySuccess) ReadObject (typeof(MessageReplySuccess));
      } else if (mt == MessageReplyType.UserException) {
        return (MessageReplyUserException) ReadObject (typeof(MessageReplyUserException));
      } else if (mt == MessageReplyType.ObjectDoesNotExit ||
                 mt == MessageReplyType.FacetDoesNotExist ||
                 mt == MessageReplyType.OperationDoesNotExist) {
        return (MessageReplyDoesNotExist) ReadObject (typeof(MessageReplyDoesNotExist));
      } else if (mt == MessageReplyType.UnknownIceLocalException ||
                 mt == MessageReplyType.UnknownIceUserException ||
                 mt == MessageReplyType.UnknownException) {
        return (MessageReplyUnknownException) ReadObject (typeof(MessageReplyUnknownException));
      }

      throw new InvalidOperationException ("ReadMessageReply with type " + mt);
    }

  } // class IceProtocolReader



  // PatchInfo bits are very similar to the fixup records
  // employed by System.Runtime.Serialization.ObjectManager.
  internal abstract class BasePatchInfo {
    public int instanceID;
    public object target;
  
    public BasePatchInfo (int id, object tgt) {
      instanceID = id;
      target = tgt;
    }
  
    public virtual void Patch (Hashtable instanceHash) {
      if (!instanceHash.Contains (instanceID)) {
        throw new InvalidOperationException
          ("BasePatchInfo: Attempted to patch, but object wasn't defined! (" + instanceID + ")");
      }

      PatchImpl (instanceHash[instanceID]);
    }
  
    protected virtual void PatchImpl (object o) {
    }
  }
  
  internal class FieldPatchInfo : BasePatchInfo {
    public FieldInfo field;
  
    public FieldPatchInfo (int id, object tgt, FieldInfo f) : base (id, tgt) {
      field = f;
    }
  
    protected override void PatchImpl (object o) {
      field.SetValue (target, o);
    }
  }

  internal class ArrayPatchInfo : BasePatchInfo {
    public int index;
  
    public ArrayPatchInfo (int id, object tgt, int idx) : base (id, tgt) {
      index = idx;
    }
  
    protected override void PatchImpl (object o) {
      ArrayList arl = target as ArrayList;
      if (arl != null) {
        arl[index] = o;
        return;
      }

      Array ar = target as Array;
      if (ar != null) {
        ar.SetValue (o, index);
        return;
      }
    }
  }
  
  // this patch refers to the value of an existing key
  internal class DictionaryValuePatchInfo : BasePatchInfo {
    public object key;
  
    public DictionaryValuePatchInfo (int id, object tgt, object k) : base (id, tgt) {
      key = k;
    }
  
    protected override void PatchImpl (object o) {
      Ice.Dictionary dict = target as Ice.Dictionary;
      dict[key] = o;
    }
  }
  
  // this patch refers to a key of a dictionary, with
  // a value that doesn't need to be patched, but needs
  // to be stored here until the key is obtained
  internal class DictionaryKeyPatchInfo : BasePatchInfo {
    public object value;
  
    public DictionaryKeyPatchInfo (int id, object tgt, object v) : base (id, tgt) {
      value = v;
    }
  
    protected override void PatchImpl (object o) {
      Ice.Dictionary dict = target as Ice.Dictionary;
      dict[o] = value;
    }
  }
  
  // this patch refers to a dictionary entry where both
  // the key and the value need to be patched.  The parent
  // will store the data for the key.
  internal class DictionaryEntryPatchInfo : BasePatchInfo {
    public int valueID;
  
    public DictionaryEntryPatchInfo (int id, object tgt, int vid) : base (id, tgt) {
      valueID = vid;
    }
  
    public override void Patch (Hashtable instanceHash) {
      if (!instanceHash.Contains (instanceID)) {
        throw new InvalidOperationException
          ("DictionaryEntryPatchInfo: Attempted to patch, but key object wasn't defined!");
      }

      if (!instanceHash.Contains (valueID)) {
        throw new InvalidOperationException
          ("DictionaryEntryPatchInfo: Attempted to patch, but value object wasn't defined!");
      }

      object keyo = instanceHash[instanceID];
      object valo = instanceHash[valueID];

      if (keyo == null) {
        throw new InvalidOperationException
          ("DictionaryEntryPatchInfo: Attempted to patch, but key object is null!");
      }
  
      Ice.Dictionary dict = target as Ice.Dictionary;
      dict[keyo] = valo;
    }
  }
  

} // namespace Ice
