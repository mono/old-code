// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceProtocol.cs
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
using System.Runtime.InteropServices;

namespace Ice {
  public enum MessageType : byte {
    Request,
    BatchRequest,
    Reply,
    ValidateConnection,
    CloseConnection,
    Invalid
  };

  public enum MessageReplyType : byte {
    Success,
    UserException,
    ObjectDoesNotExit,
    FacetDoesNotExist,
    OperationDoesNotExist,
    UnknownIceLocalException,
    UnknownIceUserException,
    UnknownException
  }

  public enum ProxyMode : byte {
    Twoway,
    Oneway,
    BatchOneway,
    Datagram,
    BatchDatagram
  };

  public struct ProxyData {
    public Ice.Identity id;
    public string[] facet;
    public ProxyMode mode;
    public bool secure;
  }

  //
  // note that these structures are used for reading only;
  // the bits are hand-marshalled for sending for speed
  // purposes.
  //
  // if this becomes a speed issue, the same may happen
  // for reading as well.
  //
  [StructLayout(LayoutKind.Sequential)]
  public struct MessageHeader {
    public int magic;
    public byte protocolMajor;
    public byte protocolMinor;
    public byte encodingMajor;
    public byte encodingMinor;
    public MessageType messageType;
    public byte compressionStatus;
    public int messageSize;

    public void ice_marshal (Ice.ProtocolWriter pw) {
      pw.Write (magic);
      pw.Write (protocolMajor);
      pw.Write (protocolMinor);
      pw.Write (encodingMajor);
      pw.Write (encodingMinor);
      pw.Write (Convert.ToByte (messageType));
      pw.Write (compressionStatus);
      pw.Write (messageSize);
    }
    public void ice_unmarshal (Ice.ProtocolReader pr) {
      magic = pr.ReadInt32();
      protocolMajor = pr.ReadByte();
      protocolMinor = pr.ReadByte();
      encodingMajor = pr.ReadByte();
      encodingMinor = pr.ReadByte();
      messageType = (MessageType) Enum.ToObject(typeof(MessageType), pr.ReadByte());
      compressionStatus = pr.ReadByte();
      messageSize = pr.ReadInt32();
    }
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct MessageRequest {
    public int requestId;
    public Ice.Identity id;
    public string[] facet;
    public string operation;
    public byte mode;
    public Ice.Context context;

    public void ice_marshal (Ice.ProtocolWriter pw) {
      pw.Write (requestId);
      pw.Write (id.name);
      pw.Write (id.category);
      pw.WriteSize (facet.Length);
      foreach (string f in facet) {
        pw.Write (f);
      }
      pw.Write (operation);
      pw.Write ((byte) mode);
      pw.WriteSize (context.Count);
      IDictionaryEnumerator iter = context.GetEnumerator();
      while (iter.MoveNext()) {
        pw.Write ((string) iter.Key);
        pw.Write ((string) iter.Value);
      }
    }
    public void ice_unmarshal (Ice.ProtocolReader pr) {
      requestId = pr.ReadInt32 ();
      id.name = pr.ReadString ();
      id.category = pr.ReadString ();
      int facetlen = pr.ReadSize();
      facet = new string[facetlen];
      for (int i = 0; i < facetlen; i++)
        facet[i] = pr.ReadString();
      operation = pr.ReadString();
      mode = pr.ReadByte();
      int count = pr.ReadSize();
      context = new Ice.Context();
      for (int i = 0; i < count; i++) {
        string k = pr.ReadString();
        string v = pr.ReadString();
        context.Add (k, v);
      }
    }
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct MessageBatchRequest {
    public Ice.Identity id;
    public string[] facet;
    public string operation;
    public byte mode;
    public Ice.Context context;

    public void ice_marshal (Ice.ProtocolWriter pw) {
      pw.Write (id.name);
      pw.Write (id.category);
      pw.WriteSize (facet.Length);
      foreach (string f in facet) {
        pw.Write (f);
      }
      pw.Write (operation);
      pw.Write (mode);
      IDictionaryEnumerator iter = context.GetEnumerator();
      while (iter.MoveNext()) {
        pw.Write ((string) iter.Key);
        pw.Write ((string) iter.Value);
      }
    }

    public void ice_unmarshal (Ice.ProtocolReader pr) {
      id.name = pr.ReadString ();
      id.category = pr.ReadString ();
      int facetlen = pr.ReadSize();
      facet = new string[facetlen];
      for (int i = 0; i < facetlen; i++)
        facet[i] = pr.ReadString();
      operation = pr.ReadString();
      mode = pr.ReadByte();
      int count = pr.ReadSize();
      context = new Ice.Context();
      for (int i = 0; i < count; i++) {
        string k = pr.ReadString();
        string v = pr.ReadString();
        context.Add (k, v);
      }
    }
  }

  //
  // Note that all the MessageReply* structs
  // start first with the requestId int32,
  // followed by a MessageReplyType byte.
  // this byte determines how the rest of the
  // data will be read.

  [StructLayout(LayoutKind.Sequential)]
  public struct MessageReplyHeader {
    public int requestId;
    public MessageReplyType replyType;

    public void ice_marshal (Ice.ProtocolWriter pw) {
      pw.Write (requestId);
      pw.Write ((byte) replyType);
    }

    public void ice_unmarshal (Ice.ProtocolReader pr) {
      requestId = pr.ReadInt32();
      replyType = (MessageReplyType) Enum.ToObject (typeof(MessageReplyType), pr.ReadByte());
    }
  }

  // Success
  [StructLayout(LayoutKind.Sequential)]
  public struct MessageReplySuccess {
    public void ice_marshal (Ice.ProtocolWriter pw) { }
    public void ice_unmarshal (Ice.ProtocolReader pr) { }
  }

  // UserException
  [StructLayout(LayoutKind.Sequential)]
  public struct MessageReplyUserException {
    public void ice_marshal (Ice.ProtocolWriter pw) { }
    public void ice_unmarshal (Ice.ProtocolReader pr) { }
  }

  // {Object, Facet, Operation} Does Not Exist
  [StructLayout(LayoutKind.Sequential)]
  public struct MessageReplyDoesNotExist {
    public Ice.Identity id;
    public string[] facet;
    public string operation;

    public void ice_marshal (Ice.ProtocolWriter pw) {
      pw.Write (id.name);
      pw.Write (id.category);
      pw.WriteSize (facet.Length);
      foreach (string f in facet) {
        pw.Write (f);
      }
      pw.Write (operation);
    }
    public void ice_unmarshal (Ice.ProtocolReader pr) {
      id.name = pr.ReadString();
      id.category = pr.ReadString();
      int facetlen = pr.ReadSize();
      facet = new string[facetlen];
      for (int i = 0; i < facetlen; i++)
        facet[i] = pr.ReadString();
      operation = pr.ReadString();
    }
  }

  // Unknown * Exception
  [StructLayout(LayoutKind.Sequential)]
  public struct MessageReplyUnknownException {
    public string msg;
    public void ice_marshal (Ice.ProtocolWriter pw) {
      pw.Write (msg);
    }
    public void ice_unmarshal (Ice.ProtocolReader pr) {
      msg = pr.ReadString();
    }
  }

  
}
