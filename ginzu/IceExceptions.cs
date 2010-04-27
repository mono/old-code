// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceExceptions.cs
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


namespace Ice {
    
  public class UnknownException : System.Exception {
    public string unknown;
  }
    
    
  public class UnknownLocalException : System.Exception {
  }
    
    
  public class UnknownUserException : System.Exception {
  }
    
    
  public class VersionMismatchException : System.Exception {
  }
    
    
  public class CommunicatorDestroyedException : System.Exception {
  }
    
    
  public class ObjectAdapterDeactivatedException : System.Exception {
    public string name;
  }
    
    
  public class ObjectAdapterIdInUseException : System.Exception {
    public string id;
  }
    
    
  public class NoEndpointException : System.Exception {
    public string proxy;
  }
    
    
  public class EndpointParseException : System.Exception {
    public string str;
  }
    
    
  public class IdentityParseException : System.Exception {
    public string str;
  }
    
    
  public class ProxyParseException : System.Exception {
    public string str;
  }
    
    
  public class IllegalIdentityException : System.Exception {
    public Ice.Identity id;
  }
    
    
  public class RequestFailedException : System.Exception {
    public Ice.Identity id;
    public string facet;
    public string operation;
  }
    
    
  public class ObjectNotExistException : System.Exception {
  }
    
    
  public class FacetNotExistException : System.Exception {
  }
    
    
  public class OperationNotExistException : System.Exception {
  }
    
    
  public class SyscallException : System.Exception {
    public int error;
  }
    
    
  public class SocketException : System.Exception {
  }
    
    
  public class ConnectFailedException : System.Exception {
  }
    
    
  public class ConnectionLostException : System.Exception {
  }
    
    
  public class DNSException : System.Exception {
    public int error;
    public string host;
  }
    
    
  public class TimeoutException : System.Exception {
  }
    
    
  public class ConnectTimeoutException : System.Exception {
  }
    
    
  public class CloseTimeoutException : System.Exception {
  }
    
    
  public class ConnectionTimeoutException : System.Exception {
  }
    
    
  public class ProtocolException : System.Exception {
  }
    
    
  public class BadMagicException : System.Exception {
    public byte badMagic;
  }
    
    
  public class UnsupportedProtocolException : System.Exception {
    public int badMajor;
    public int badMinor;
    public int major;
    public int minor;
  }
    
    
  public class UnsupportedEncodingException : System.Exception {
    public int badMajor;
    public int badMinor;
    public int major;
    public int minor;
  }
    
    
  public class UnknownMessageException : System.Exception {
  }
    
    
  public class ConnectionNotValidatedException : System.Exception {
  }
    
    
  public class UnknownRequestIdException : System.Exception {
  }
    
    
  public class UnknownReplyStatusException : System.Exception {
  }
    
    
  public class CloseConnectionException : System.Exception {
  }
    
    
  public class AbortBatchRequestException : System.Exception {
  }
    
    
  public class IllegalMessageSizeException : System.Exception {
  }
    
    
  public class CompressionNotSupportedException : System.Exception {
  }
    
    
  public class CompressionException : System.Exception {
    public string reason;
  }
    
    
  public class MarshalException : System.Exception {
  }
    
    
  public class NoObjectFactoryException : System.Exception {
    public string type;
  }
    
    
  public class ProxyUnmarshalException : System.Exception {
  }
    
    
  public class UnmarshalOutOfBoundsException : System.Exception {
  }
    
    
  public class IllegalIndirectionException : System.Exception {
  }
    
    
  public class MemoryLimitException : System.Exception {
  }
    
    
  public class EncapsulationException : System.Exception {
  }
    
    
  public class NegativeSizeException : System.Exception {
  }
    
    
  public class PluginInitializationException : System.Exception {
    public string reason;
  }
    
    
  public class CollocationOptimizationException : System.Exception {
  }
    
    
  public class AlreadyRegisteredException : System.Exception {
    public string kindOfObject;
    public string id;
  }
    
    
  public class NotRegisteredException : System.Exception {
    public string kindOfObject;
    public string id;
  }
    
    
  public class TwowayOnlyException : System.Exception {
    public string operation;
  }
}
