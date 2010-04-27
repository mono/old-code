// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceReceiverDispatcher.cs
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
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.Diagnostics;

namespace Ice {

  public delegate void MessageRequestDelegate (Ice.Endpoint e,
                                               Stream msgStream,
                                               Ice.MessageType mtype);

  public class ReceiverDispatcher {
    private Ice.Endpoint _e;
    private ProtocolReader _r;
    private bool _valid;

    // the object manager that's attached to this endpoint
    private MessageRequestDelegate _requestHandler;

    // (int) requestId -> (ManualResetEvent) notification
    private Hashtable _outstandingRequests;
    // (int) requestId -> (MemoryStream) msg
    private Hashtable _notifiedRequests;
    // reader thread
    private Thread _readerThread;

    public ReceiverDispatcher (Ice.Endpoint e)
      : this (e, null)
    {
    }

    public ReceiverDispatcher (Ice.Endpoint e, MessageRequestDelegate mrd)
    {
      _e = e;
      _r = e.ProtocolReader;
      _requestHandler = mrd;
      _valid = true;

      // if the endpoint is connected and this endpoint isn't incoming,
      // then we need to wait for the validate message before
      // sending any requests
      if (_e.HasConnection && !_e.Incoming)
        _valid = false;

      _outstandingRequests = new Hashtable();
      _notifiedRequests = new Hashtable();

      _readerThread = new Thread(new ThreadStart (Reader));
      _readerThread.IsBackground = true;
      _readerThread.Start();
    }

    public void Activate () {
      if (_readerThread.IsAlive)
        return;

      _readerThread.Start();
    }

    public ManualResetEvent RegisterReplyNotification (int requestId)
    {
      ManualResetEvent mre;

      lock (_notifiedRequests) {
        // if we've already received this reply,
        // then we create an already-notified MRE
        if (_notifiedRequests.Contains(requestId)) {
          mre = new ManualResetEvent (true);
        } else {
          mre = new ManualResetEvent (false);
          lock(_outstandingRequests) {
            _outstandingRequests[requestId] = mre;
          }
        }
      }

      return mre;
    }

    public MemoryStream GetMessageStream (int requestId)
    {
      MemoryStream ms;

      lock (_notifiedRequests) {
        ms = (MemoryStream) _notifiedRequests[requestId];
        _notifiedRequests.Remove (requestId);
      }

      return ms;
    }

    //
    // Reader thread function for this endpoint/dispatcher.
    // Reads in blocking mode.
    //
    private void Reader () {

      MessageHeader mhdr;

      while (true) {
        try {
          // read/wait for an incoming message
          mhdr = _r.ReadMessageHeader();

        } catch (System.IO.EndOfStreamException) {
          // the stream got closed, so we're going to shut down.
          // FIXME -- what does "shut down" entail?  How do
          // I destroy everything that's involved with this
          // thread's handler?
          return;
        }

        // If this connection hasn't been validated, then we expect
        // a validate -- but only if the endpoint has a notion
        // of a connection (see constructor)
        if (!_valid) {
          if (mhdr.messageType != MessageType.ValidateConnection) {
            throw new InvalidOperationException ("Received " + mhdr.messageType + ", was expecting ValidateConnection");
          }
          _valid = true;
        } else {
          Trace.WriteLine ("Received: " + mhdr.messageType);
          // Process an incoming message.  
          if (mhdr.messageType == MessageType.Request || mhdr.messageType == MessageType.BatchRequest)
          {
            if (_requestHandler == null) {
              throw new InvalidOperationException ("Received request, but there's no MRD attached to dispatcher!");
            }

            byte[] msg_rest = _r.ReadBytes (mhdr.messageSize - 14); // 14 is the header
            MemoryStream ms = new MemoryStream (msg_rest);

            // this MUST be executed asynchronously, otherwise we are unable to
            // receive any further requests on this endpoint until it returns.

            // we might want some better control over this, but for now, let the
            // runtime take care of scheduling it.
            _requestHandler.BeginInvoke (_e, ms, mhdr.messageType, null, null);


          }
          else if (mhdr.messageType == MessageType.Reply)
          {
            int requestId =  _r.ReadInt32();

            lock (_notifiedRequests) {
              ManualResetEvent mre;
              lock (_outstandingRequests) {
                 mre = (ManualResetEvent) _outstandingRequests[requestId];
                _outstandingRequests.Remove (requestId);
              }

              int bufSize = mhdr.messageSize;
              byte[] msg = _r.ReadBytes (mhdr.messageSize - 18);
              // we don't send along the message header, OR the requestId.
              // the next thing that is read is the reply type, which we
              // don't care about at this point -- we just suck up the
              // rest of the message and pass it along.
              MemoryStream ms = new MemoryStream (msg);
              _notifiedRequests[requestId] = ms;

              if (mre == null) {
                Console.WriteLine ("Got Reply for requestId " + requestId + " but had no MRE!");
              } else {
                mre.Set();
              }
            }
          }
          else if (mhdr.messageType == MessageType.CloseConnection)
          {
            // err -- what do I do here?
            return;
          }
          else
          {
            Console.WriteLine ("Dispatcher got unhandled message of type " + mhdr.messageType);
          }
        }
      }
    }


  }

}
