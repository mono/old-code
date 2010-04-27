// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceChannelUtils.cs
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
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using System.Reflection;
using System.Diagnostics;

namespace Ice {

  internal sealed class IceChannelUtils {
    private static int _nextRequestId = 1;

    // NextRequestId
    //
    // Returns a unique request id; FIXME -- should roll over to 1.
    //
    public static int NextRequestId {
      get {
        int nextId = System.Threading.Interlocked.Increment (ref _nextRequestId);
        return nextId;
      }
    }

    // MessageToProtocolRequest
    //
    // Converts a IMessage representing a method call (IMethodCallMessage)
    // into the Ice protocol representation, and writes it to stream outStream.
    public static void MessageToProtocolRequest (Stream outStream, IMessage msg)
    {
      IMethodCallMessage mcall = msg as IMethodCallMessage;
      Ice.ProtocolWriter pw = new Ice.ProtocolWriter(outStream);

      // extract message bits from mcall
      Ice.Identity id = (Ice.Identity) mcall.LogicalCallContext.GetData("__iceIdentity");
      string[] facetPath = (string[]) mcall.LogicalCallContext.GetData("__iceFacetPath");
      Ice.OperationMode opMode = (Ice.OperationMode) mcall.LogicalCallContext.GetData("__iceOperationMode");
      Ice.Context ctx = (Ice.Context) mcall.LogicalCallContext.GetData("__iceContext");
      bool oneWay = (bool) mcall.LogicalCallContext.GetData("__iceOneWay");


      int thisRequestId = 0;
      if (!oneWay)
        thisRequestId = IceChannelUtils.NextRequestId;

      mcall.LogicalCallContext.SetData("__iceRequestId", thisRequestId);

      ParameterInfo[] paramInfos = mcall.MethodBase.GetParameters();

      // Create an Ice protocol message
      pw.BeginMessage (Ice.MessageType.Request);
      pw.WriteRequestMessageHeader (thisRequestId,
                                    id,
                                    facetPath,
                                    mcall.MethodName,
                                    Ice.OperationMode.Normal,
                                    ctx);

      // now write the args
      pw.BeginEncapsulation();
      for (int i = 0; i < mcall.ArgCount; i++) {
        if (!paramInfos[i].IsOut) {
          if (Attribute.GetCustomAttribute (paramInfos[i], typeof(Ice.AsProxy)) != null)
            pw.WriteProxy (mcall.Args[i], paramInfos[i].ParameterType);
          else
            pw.WriteObject (mcall.Args[i], paramInfos[i].ParameterType);
        }
      }
      pw.WriteClassInstances();
      pw.EndEncapsulation();
      pw.EndMessage();
    }

    // ProtocolReplyToMessage
    //
    // Given a response message stream respStream, and the message that
    // originated to call msg, returns an IMethodReturnMessage describing
    // the return value of the function.
    //
    public static IMethodReturnMessage ProtocolReplyToMessage (Stream respStream,
                                                               IMessage msg)
    {
      IMethodCallMessage mcall = msg as IMethodCallMessage;

      // set up some stuff that we'll need for parsing
      ParameterInfo[] paramInfos = mcall.MethodBase.GetParameters();
      Type returnType = ((MethodInfo) mcall.MethodBase).ReturnType;

      // at this point, respStream should be pointing to the first byte after the requestId
      Ice.ProtocolReader pr = new Ice.ProtocolReader (respStream);

      // the first byte here is the MessageReplyType
      byte b = pr.ReadByte();
      MessageReplyType replyType = (MessageReplyType) Enum.ToObject (typeof(MessageReplyType), b);

      if (replyType == MessageReplyType.Success) {
        // what follows is an encapsulation with return value and out params, if any.
        int encapsBytes = pr.ReadEncapsulationHeader();
        if (encapsBytes == 0) {
          // no reply value(s) follow
          return new ReturnMessage (null, null, 0, mcall.LogicalCallContext, mcall);
        } else {
          // reply values follow: first the out-values in order of declaration,
          // then the return value
          object returnValue = null;
          ArrayList outArgs = new ArrayList();
          int readOutArgs = 0;

          for (int i = 0; i < mcall.ArgCount; i++) {
            if (paramInfos[i].IsOut) {
              object o;
              if (Attribute.GetCustomAttribute (paramInfos[i], typeof(Ice.AsProxy)) != null) {
                o = pr.ReadObjectProxy (paramInfos[i].ParameterType);
                outArgs.Add (o);
              } else if (!IceByValue (paramInfos[i].ParameterType)) {
                // add placeholder for patch later
                outArgs.Add (null);
                pr.ReadClassInstanceParameterRef (outArgs, readOutArgs);
              } else {
                o = pr.ReadObject (paramInfos[i].ParameterType);
                outArgs.Add (o);
              }
              readOutArgs++;
            }
          }

          if (returnType != null && returnType != typeof(void)) {
            returnValue = pr.ReadObject (returnType);
          }

          pr.ReadClassInstancesAndPatch();

          return new ReturnMessage (returnValue, outArgs.ToArray(), outArgs.Count, mcall.LogicalCallContext, mcall);
        }
      } else {
        // message was not a success; we ought to parse the exception. TODO FIXME
        Ice.UnknownException e = new Ice.UnknownException();
        e.unknown = "Message reply type was " + replyType;

        return new ReturnMessage (e, mcall);
      }
    }

    // ProtocolRequestToMessage
    //
    // Converts an incoming Ice Protocol Request stream from requestStream
    // into a new IMethodCallMessage.  isBatched specifies if the request
    // is a batched request or not.
    public static IMessage ProtocolRequestToMessage (Stream requestStream, bool isBatched)
    {
      Ice.ProtocolReader pr = new Ice.ProtocolReader (requestStream);

      Ice.Identity id;
      string methodname;
      int requestId;
      Ice.OperationMode mode;
      Ice.Context context;

      
      if (!isBatched) {
        Ice.MessageRequest req = pr.ReadMessageRequest();
        requestId = req.requestId;
        id = req.id;
        methodname = req.operation;
        mode = (Ice.OperationMode) Enum.ToObject (typeof(Ice.OperationMode), req.mode);
        context = req.context;
      } else {
        Ice.MessageBatchRequest req = pr.ReadMessageBatchRequest();
        requestId = 0;
        id = req.id;
        methodname = req.operation;
        mode = (Ice.OperationMode) Enum.ToObject (typeof(Ice.OperationMode), req.mode);
        context = req.context;

        // FIXME -- if this is a batch, we really want to return multiple
        // messages.  We need to extend both this function and the callee
        // to understand an array return.
        throw new NotImplementedException ("Batch execution detected");
      }

      string uri = id.ToUri();

      Type svrType = RemotingServices.GetServerTypeForUri (uri);
      if (svrType == null)
        throw new RemotingException ("No registered server for uri " + uri);

      MethodInfo mi = svrType.GetMethod (methodname);
      if (mi == null)
        throw new Ice.OperationNotExistException();

      int encapsSize = pr.ReadEncapsulationHeader();

      Trace.WriteLine ("ProtocolRequestToMessage method: " + methodname + " -> " + mi);

      ParameterInfo[] paramInfos = mi.GetParameters();
      int inParams = 0, outParams = 0;
      int readInParams = 0;
      object[] methodArgs;

      if (encapsSize == 0) {
        for (int i = 0; i < paramInfos.Length; i++) {
          if (paramInfos[i].IsOut)
            outParams++;
          else
            inParams++;
        }
        methodArgs = new object[0];
      } else {
        ArrayList args = new ArrayList();
        for (int i = 0; i < paramInfos.Length; i++) {
          if (!paramInfos[i].IsOut) {
            object o;
            if (Attribute.GetCustomAttribute (paramInfos[i], typeof(Ice.AsProxy)) != null) {
              o = pr.ReadObjectProxy (paramInfos[i].ParameterType);
              args.Add (o);
            } else if (!IceByValue (paramInfos[i].ParameterType)) {
              // add a placeholder that will get replaced by
              // patch call below.
              args.Add (null);
              pr.ReadClassInstanceParameterRef (args, readInParams);
            } else {
              o = pr.ReadObject (paramInfos[i].ParameterType);
              args.Add (o);
            }

            inParams++;
            readInParams++;
          } else {
            outParams++;
          }
        }

        pr.ReadClassInstancesAndPatch();

        methodArgs = args.ToArray();
      }

      if (readInParams != inParams) {
        // FIXME
        throw new InvalidOperationException("Wrong number of parameters for operation, expected " + inParams + " got " + readInParams);
      }

      // I need: uri, typeNAme, methodName, object[] args
      System.Runtime.Remoting.Messaging.Header[] hs =
        new System.Runtime.Remoting.Messaging.Header[4];
      hs[0] = new Header ("__TypeName", svrType.FullName);
      hs[1] = new Header ("__MethodName",methodname);
      hs[2] = new Header ("__Uri", uri);
      hs[3] = new Header ("__Args", methodArgs);

      MethodCall mc = new MethodCall (hs);

      mc.Properties["__iceRequestId"] = requestId;
      mc.Properties["__iceIdentity"] = id;
      mc.Properties["__iceMode"] = mode;
      mc.Properties["__iceContext"] = context;

      return mc;
    }

    // MessageToProtocolReply
    //
    // Given a IMethodReturnMessage replyMsg, which originated from the
    // original IMethodMessage callMsg, serialize the reply using the
    // Ice protocol to outStream
    public static void MessageToProtocolReply (IMessage callMsg, IMessage replyMsg, Stream outStream)
    {
      IMethodMessage cmsg = callMsg as IMethodMessage;
      IMethodReturnMessage retmsg = replyMsg as IMethodReturnMessage;

      if (cmsg == null || retmsg == null)
        throw new InvalidOperationException ("no IMethodReturnMessage????");

      if (retmsg.Exception != null) {
        Trace.WriteLine ("retmsg.Exception != null, don't know what to do");
        Console.WriteLine (retmsg.Exception);
        return;
      }

      int requestId = (int) callMsg.Properties["__iceRequestId"];

      Ice.ProtocolWriter pw = new Ice.ProtocolWriter (outStream);
      pw.BeginMessage (Ice.MessageType.Reply);
      pw.WriteReplyMessageHeader (MessageReplyType.Success,
                                  requestId);

      pw.BeginEncapsulation();
      // first write the out params
      object[] outArgs = retmsg.OutArgs;
      int curOutArg = 0;

      MethodInfo mi = cmsg.MethodBase as MethodInfo;
      ParameterInfo[] paramInfos = mi.GetParameters();
      foreach (ParameterInfo pinfo in paramInfos) {
        if (pinfo.IsOut) {
          if (Attribute.GetCustomAttribute (pinfo, typeof(Ice.AsProxy)) != null)
            pw.WriteProxy (outArgs[curOutArg++], pinfo.ParameterType);
          else
            pw.WriteObject (outArgs[curOutArg++], pinfo.ParameterType);
        }
      }

      Type rtype = mi.ReturnType;
      if (rtype != null && rtype != typeof(void)) {
        if (mi.ReturnTypeCustomAttributes.IsDefined (typeof(Ice.AsProxy), true))
          pw.WriteProxy (retmsg.ReturnValue, rtype);
        else
          pw.WriteObject (retmsg.ReturnValue, rtype);
      }
      pw.WriteClassInstances();
      pw.EndEncapsulation();
      pw.EndMessage ();
    }


    //
    // converts URLs of the form:
    //
    //   ice://host:port/name/category
    //
    // into "host", (int) port, and "name/category"
    //
    public static bool ParseIceURL (string url, out string host, out int port, out string uri)
    {
      host = null;
      port = 0;
      uri = null;

      if (!url.StartsWith ("ice://")) {
        return false;
      }

      string hostinfo = url.Substring (6);
      int slash = hostinfo.IndexOf("/");

      if (slash > 0) {
        uri = hostinfo.Substring (slash + 1);
        hostinfo = hostinfo.Substring (0, slash);
      }

      int colon = hostinfo.IndexOf(":");
      if (colon == -1) {
        // port is required
        return false;
      }

      port = Convert.ToInt32 (hostinfo.Substring (colon+1));
      host = hostinfo.Substring (0, colon);

      return true;
    }

    // returns true if type t is to be passed by value, otherwise
    // passed by reference
    public static bool IceByValue (Type t) {
      if (t.IsArray || t.IsEnum || t == typeof(string) || t.IsSubclassOf (typeof (Ice.Dictionary)))
        return true;

      if (t.IsClass && Attribute.GetCustomAttribute(t, typeof(Ice.AsProxy)) == null)
        return false;

      return true;
    }
  }
}

