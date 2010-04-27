// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceProtocolWriter.cs
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

using System.Reflection;

namespace Ice {
  public class ProtocolWriter : BinaryWriter {
    // Stack used for keeping track of encapsulations;
    // stack contains the previous stream to which to
    // write the currently-active encapsulation to.
    private Stack _encapsulationStack;

    // similar function as previous, but slices can't appear
    // within other slices, so there's no need for a stack.
    private System.IO.Stream _lastSliceStream;

    // used for keeping track of class ID marshalling;
    private Hashtable _classHash;
    private int _classCounter;

    // used for keeping track of ID marshalling
    private Hashtable _instanceHash;
    private ArrayList _instancesToSend;
    private int _instanceCounter;

    // keep track of the current message being built
    private MessageType _currentMessageType;
    private System.IO.Stream _lastMessageStream;
    private bool _usesClasses;

    private int _nextRequestId;

    public ProtocolWriter (System.IO.Stream s) : base (s, Ice.IceUtil.UTF8) {
      _nextRequestId = 1;
      ResetProtocol();
    }

    public virtual void ResetProtocol() {
      _encapsulationStack = new Stack();
      _lastSliceStream = null;
      _classHash = new Hashtable();
      _classCounter = 1;
      _instanceHash = new Hashtable();
      _instancesToSend = new ArrayList();
      _instanceCounter = 1;
      _currentMessageType = MessageType.Invalid;
      _usesClasses = false;
    }

    public int NextRequestId {
      get {
        return _nextRequestId++;
      }
    }

    // Returns true if WriteClassRef() was called
    // at any point during the current message's
    // construction.
    public bool CurrentMessageUsesClasses {
      get {
        return _usesClasses;
      }
    }

    public void BeginEncapsulation() {
      _encapsulationStack.Push (OutStream);
      OutStream = new MemoryStream();
    }

    public void EndEncapsulation() {
      if (_encapsulationStack.Count == 0)
        throw new InvalidOperationException ("EndEncapsulation with no encapsulation on stack");

      MemoryStream encaps = OutStream as MemoryStream;
      int capslength = (int) encaps.Length + 6;
      OutStream = (System.IO.Stream) _encapsulationStack.Pop ();
      Write (capslength);       // length
      Write ((byte) 1);         // encapsulationMajor
      Write ((byte) 0);         // encapsulationMinor
      encaps.WriteTo(OutStream);
      ((IDisposable) encaps).Dispose();
    }

    public virtual void WriteSize (int sz) {
      if (sz < 255) {
        Write ((byte) sz);
      } else {
        Write ((byte) 255);
        Write (sz);
      }
    }

    public void BeginSlice() {
      if (_lastSliceStream != null)
        throw new InvalidOperationException ("BeginSlice within an existing slice");
      _lastSliceStream = OutStream;
      OutStream = new MemoryStream();
    }

    public void EndSlice() {
      if (_lastSliceStream == null)
        throw new InvalidOperationException ("EndSlice outside of slice");
      MemoryStream slice = OutStream as MemoryStream;
      int slicelength = (int) slice.Length + 4;
      OutStream = _lastSliceStream;

      Write (slicelength);
      slice.WriteTo (OutStream);
      ((IDisposable) slice).Dispose();

      _lastSliceStream = null;
    }

    public virtual void WriteClassSlice (object o, Type ot) {
      // First check to see how this slice name should be encoded;
      // if we've already sent it once, then it's true + classId as
      // a size. Otherwise, it's false + string
      if (_classHash.Contains (ot)) {
        int classID = (int) _classHash[ot];
        Write (true);
        WriteSize (classID);
      } else {
        int classID = _classCounter++;
        _classHash[ot] = classID;

        string icename = Ice.IceUtil.TypeToIceName (ot);

        Write (false);
        Write (icename);
      }

      BeginSlice();
      MethodInfo marshal = ot.GetMethod ("ice_marshal",
                                         BindingFlags.Instance |
                                         BindingFlags.Public |
                                         BindingFlags.DeclaredOnly);
      if (marshal != null) {
        object[] args = new object[1];
        args[0] = this;
        try {
          marshal.Invoke (o, args);
        } catch (TargetInvocationException te) {
          throw te.InnerException;
        }
      } else {
        FieldInfo[] fields = ot.GetFields(BindingFlags.Instance |
                                          BindingFlags.Public |
                                          BindingFlags.DeclaredOnly);
        foreach (FieldInfo field in fields) {
          WriteObject (field.GetValue(o), field.FieldType);
        }
      }
      EndSlice();
    }

    public override void Write (string s) {
      if (s == null || s.Length == 0) {
        WriteSize (0);
        return;
      }

      WriteSize (s.Length);
      byte[] enc = Ice.IceUtil.UTF8.GetBytes(s);
      Write (enc, 0, enc.Length);
    }

    public virtual void WriteObject (object o) {
      WriteObject (o, o.GetType(), true);
    }

    public virtual void WriteObject (object o, bool writeClassRefs) {
      WriteObject (o, o.GetType(), writeClassRefs);
    }

    public virtual void WriteObject (object o, Type t) {
      WriteObject (o, t, true);
    }

    // if writeClassRefs is true, classes will be written as
    // refs (negative numbers); otherwise they will be marshalled
    // as full classes.
    public virtual void WriteObject (object o, Type t, bool writeClassRefs) {
      if (t.IsPrimitive || t == typeof(string)) {
        if (t == typeof(bool)) {
          Write ((bool) o);
          return;
        }
        if (t == typeof(byte)) {
          Write ((byte) o);
          return;
        }
        if (t == typeof(short)) {
          Write ((short) o);
          return;
        }
        if (t == typeof(int)) {
          Write ((int) o);
          return;
        }
        if (t == typeof(long)) {
          Write ((long) o);
          return;
        }
        if (t == typeof(float)) {
          Write ((float) o);
          return;
        }
        if (t == typeof(double)) {
          Write ((double) o);
          return;
        }
        if (t == typeof(string)) {
          Write ((string) o);
          return;
        }

        throw new InvalidOperationException ("WriteObject can't write primitive type " + t);
      }

      if (t.IsEnum) {
        WriteEnum ((Enum) o);
        return;
      }

      if (t.IsArray) {
        WriteArray ((Array) o);
        return;
      }

      // this catches only stucts; the other value types
      // are primitives and enums which are caught before
      if (t.IsValueType) {
        WriteStruct (o);
        return;
      }

      if (t.IsSubclassOf (typeof(Ice.Dictionary))) {
        WriteDictionary ((Ice.Dictionary) o);
        return;
      }

      if (t.IsClass) {
        if (writeClassRefs) {
          WriteClassRef (o);
        } else {
          WriteClass (o);
        }
        return;
      }

      throw new NotSupportedException ("IceProtocolWriter: can't marshal type " + t);
    }

    
    // This depends on the underlying type being set correctly.  The
    // alternative is iterating through the values of the enum and
    // seeing what the max is.  This is ok, as long as enums are
    // produced by slice2cs; any user-defined enums will have to be
    // careful.
    public virtual void WriteEnum (Enum e) {
      Type ue = Enum.GetUnderlyingType (e.GetType());

      if (ue == typeof(byte)) {
        Write (Convert.ToByte(e));
        return;
      }

      if (ue == typeof(short)) {
        Write (Convert.ToInt16(e));
        return;
      }

      if (ue == typeof(int)) {
        Write (Convert.ToInt32(e));
        return;
      }

      throw new ArgumentException ("enum must have underlying byte, short, or int type; this one has " + ue.Name);
    }

    public virtual void WriteProxy (object o, Type ot) {
      if (!RemotingServices.IsTransparentProxy (o)) {
        Console.WriteLine ("o {0} ot {1} is not transparent proxy!", o, ot);
        throw new InvalidOperationException ("object which is not a transparent proxy passed to WriteProxy, type " + ot);
      }

      ObjRef or = RemotingServices.GetObjRefForProxy ((System.MarshalByRefObject) o);
      Ice.Identity proxy_ident = new Ice.Identity (or.URI.StartsWith("/") ? or.URI.Substring(1) : or.URI);
      Ice.Endpoint proxy_ep = null;

      foreach (object cdo in or.ChannelInfo.ChannelData) {
        ChannelDataStore cd = cdo as ChannelDataStore;
        if (cd == null)
          continue;
        foreach (string ch_uri in cd.ChannelUris) {
          string host;
          int port;
          string uri;

          if (IceChannelUtils.ParseIceURL (ch_uri, out host, out port, out uri)) {
            proxy_ep = new Ice.TcpEndpoint (host, port);
            proxy_ep.Incoming = true;
            break;
          }
        }
      }
      
      if (proxy_ep == null) {
        throw new InvalidOperationException ("Couldn't find valid Ice endpoint/channel for " + o);
      }

      Ice.ProxyData pd = new Ice.ProxyData();
      pd.id = proxy_ident;
      pd.facet = new string[0];
      pd.mode = ProxyMode.Twoway; // FIXME -- do I need to send multiple proxy things here?
      pd.secure = false;

      WriteStruct (pd);
      WriteSize (1);            // one endpoint follows

      // tcp endpoint encapsulation
      WriteObject ((short) 1);  // it's a TCP endpoint
      BeginEncapsulation();
      Ice.TcpEndpoint te = proxy_ep as Ice.TcpEndpoint;
      Write (te.host);
      WriteObject (te.port);
      WriteObject (te.timeout);
      WriteObject (te.compress);
      EndEncapsulation();
    }

    public virtual void WriteStruct (object o) {
      Type t = o.GetType();
      MethodInfo marshal = t.GetMethod ("ice_marshal",
                                        BindingFlags.Instance |
                                        BindingFlags.Public |
                                        BindingFlags.DeclaredOnly);
      if (marshal != null) {
        object[] args = new object[1];
        args[0] = this;
        try {
          marshal.Invoke (o, args);
        } catch (TargetInvocationException te) {
          throw te.InnerException;
        }
      } else {
        foreach (FieldInfo field in t.GetFields()) {
          WriteObject (field.GetValue (o), field.FieldType);
        }
      }
    }

    public virtual void WriteDictionary (Ice.Dictionary dict) {
      WriteSize (dict.Count);
      IDictionaryEnumerator iter = dict.GetEnumerator();
      while (iter.MoveNext()) {
        // FIXME -- these checks are expensive
        if (iter.Key.GetType() != dict.IceDictInfo.keyType)
          throw new InvalidOperationException ("Dictionary contains key with type " + iter.Key + " but keyType is " + dict.IceDictInfo.keyType);
        if (iter.Value.GetType() != dict.IceDictInfo.valueType)
          throw new InvalidOperationException ("Dictionary contains value with type " + iter.Value + " but valueType is " + dict.IceDictInfo.valueType);

        WriteObject (iter.Key, dict.IceDictInfo.keyType);
        WriteObject (iter.Value, dict.IceDictInfo.valueType);
      }
    }

    public virtual void WriteArray (Array ar) {
      if (ar == null) {
        WriteSize (0);
        return;
      }

      int len = ar.GetLength(0);
      WriteSize (len);
      for (int i = 0; i < len; i++) {
        WriteObject (ar.GetValue(i));
      }
    }

    public virtual void WriteClassRef (object o) {
      if (o == null) {
        Write ((int) 0);
        return;
      }

      _usesClasses = true;

      int instanceID;

      if (_instanceHash.Contains(o)) {
        instanceID = (int) _instanceHash[o];
      } else {
        instanceID = _instanceCounter++;
        _instanceHash.Add(o, instanceID);
        _instancesToSend.Add(o);
      }

      // We marshal the negative instance ID to
      // indicate that the declaration will be coming
      // later.
      Write (- instanceID);
    }

    public virtual void WriteClass (object o) {
      Type ot = null;
      Type nt = o.GetType();

      do {
        ot = nt;
        WriteClassSlice (o, ot);
        nt = ot.BaseType;
      } while (ot != typeof(Ice.Object));
    }

    public virtual void WriteClassInstances () {
      while (_instancesToSend.Count != 0) {
        ArrayList theseInstances = _instancesToSend;
        _instancesToSend = new ArrayList();

        WriteSize (theseInstances.Count);
        foreach (object o in theseInstances) {
          int instanceID = (int) _instanceHash[o];
          Write(instanceID);
          WriteClass (o);
        }
      }
    }

    public virtual void WriteEndpoint (Endpoint e) {
      if (e is TcpEndpoint) {
        TcpEndpoint te = e as TcpEndpoint;
        Write ((short) 1);      // Endpoint Type
        BeginEncapsulation ();
        Write (te.host);
        Write (te.port);
        Write (te.timeout);
        Write (te.compress);
        EndEncapsulation ();
      } else if (e is SslEndpoint) {
        SslEndpoint se = e as SslEndpoint;
        Write ((short) 2);      // Endpoint Type
        BeginEncapsulation ();
        Write (se.host);
        Write (se.port);
        Write (se.timeout);
        Write (se.compress);
        EndEncapsulation ();
      } else if (e is UdpEndpoint) {
        UdpEndpoint ue = e as UdpEndpoint;
        Write ((short) 3);      // Endpoint Type
        BeginEncapsulation ();
        Write (ue.host);
        Write (ue.port);
        Write (ue.protocolMajor);
        Write (ue.protocolMinor);
        Write (ue.encodingMajor);
        Write (ue.encodingMinor);
        Write (ue.compress);
        EndEncapsulation();
      }
    }

    //
    // protocol message headers
    //
    public virtual void BeginMessage (MessageType mt) {
      // first check to make sure we have no current outstanding message
      if (_currentMessageType != MessageType.Invalid) {
        throw new InvalidOperationException ("BeginMessage called with message already in progress");
      }

      _lastMessageStream = OutStream;
      _currentMessageType = mt;
      OutStream = new MemoryStream();
    }

    public virtual void EndMessage () {
      if (_currentMessageType == MessageType.Invalid) {
        throw new InvalidOperationException ("EndMessage called with no message in progress");
      }

      MemoryStream msg = OutStream as MemoryStream;
      int length = (int) msg.Length + 14; // message body length plus size of header

      OutStream = _lastMessageStream;
      // Write an Ice message header as a prefix for the message data
      // 'I' 'c' 'e' 'P'
      byte[] IceP = {0x49, 0x63, 0x65, 0x50};
      Write (IceP);
      Write ((byte) 1);         // protocolMajor
      Write ((byte) 0);         // protocolMinor
      Write ((byte) 1);         // encodingMajor
      Write ((byte) 0);         // encodingMinor
      Write (Convert.ToByte (_currentMessageType)); // messageType
      Write ((byte) 0);         // compressionStatus; not supported for now
      Write (length);

      msg.WriteTo (OutStream);
      ((IDisposable) msg).Dispose();

      _currentMessageType = MessageType.Invalid;
    }

    public virtual void WriteRequestMessageHeader (int requestId,
                                                   Ice.Identity id,
                                                   string[] facet,
                                                   string operation,
                                                   OperationMode omode,
                                                   Ice.Context ctx)
    {
      if (_currentMessageType != MessageType.Request)
        throw new InvalidOperationException ("WriteRequestMessageHeader called with current message other than Request");

      Write (requestId);
      WriteObject (id);
      WriteObject (facet);
      Write (operation);
      Write (Convert.ToByte (omode));
      WriteObject (ctx);

      // params in an encapsulation need to follow this message
    }

    public virtual void WriteBatchRequestMessageHeader (int numRequests)
    {
      if (_currentMessageType != MessageType.BatchRequest)
        throw new InvalidOperationException ("WriteBatchRequestBegin called with current message other than BatchRequest");

      // this starts a sequence, so we just encode the number of requests
      WriteSize (numRequests);

      // follow this up with numRequests WriteBatchRequestMessageEntry's and encapsulations
    }

    public virtual void WriteBatchRequestMessageEntry (Ice.Identity id,
                                                       string[] facet,
                                                       string operation,
                                                       OperationMode omode,
                                                       Ice.Context ctx)
    {
      if (_currentMessageType != MessageType.BatchRequest)
        throw new InvalidOperationException ("WriteRequestMessageHeader called with current message other than Request");

      WriteObject (id);
      WriteObject (facet);
      Write (operation);
      Write (Convert.ToByte (omode));
      WriteObject (ctx);

      // params in an encapsulation need to follow this
    }

    // Since the reply messages are so similar, we provide a few overloaded
    // functions that handle the common params
    public virtual void WriteReplyMessageHeader (MessageReplyType rt,
                                                 int requestId)
    {
      if (_currentMessageType != MessageType.Reply)
        throw new InvalidOperationException ("WriteReplyMessageHeader called with current message type other than Reply");

      if (rt == MessageReplyType.Success ||
          rt == MessageReplyType.UserException)
      {
        Write (requestId);
        Write (Convert.ToByte(rt));
        // appropriate encapsulation follows
      } else {
        throw new InvalidOperationException ("WriteReplyMessageHeader: wrong arguments");
      }
    }

    public virtual void WriteReplyMessageHeader (MessageReplyType rt,
                                                 int requestId,
                                                 Ice.Identity id,
                                                 string[] facet,
                                                 string operation)
    {
      if (_currentMessageType != MessageType.Reply)
        throw new InvalidOperationException ("WriteReplyMessageHeader called with current message type other than Reply");

      if (rt == MessageReplyType.ObjectDoesNotExit ||
          rt == MessageReplyType.FacetDoesNotExist ||
          rt == MessageReplyType.OperationDoesNotExist)
      {
        Write (requestId);
        Write (Convert.ToByte(rt));
        WriteObject (id);
        WriteObject (facet);
        Write (operation);
      } else {
        throw new InvalidOperationException ("WriteReplyMessageHeader: wrong arguments");
      }
    }

    public virtual void WriteReplyMessageHeader (MessageReplyType rt,
                                                 int requestId,
                                                 string msg)
    {
      if (_currentMessageType != MessageType.Reply)
        throw new InvalidOperationException ("WriteReplyMessageHeader called with current message type other than Reply");

      if (rt == MessageReplyType.UnknownIceLocalException ||
          rt == MessageReplyType.UnknownIceUserException ||
          rt == MessageReplyType.UnknownException)
      {
        Write (requestId);
        Write (Convert.ToByte(rt));
        Write (msg);
      } else {
        throw new InvalidOperationException ("WriteReplyMessageHeader: wrong arguments");
      }
    }
  }
} // namespace Ice
