/// COPYRIGHT 2005(C) ZACBOWLING
/// LICENCE: X11
///
/// I got a lot from the book "C# Networking"
/// the ASN.1 docs. 
/// 
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mono.Net.Snmp
{
public class SNMP
{
   public SNMP()
   {

   }

   public byte[] get(string request, string host, string community, string OIDstring)
   {
      byte[] oid, packet;
      int snmplen, comlen, OIDlen, cnt = 0, temp, i, pos = 0, orgoidlen;
      string[] OIDvals;
      
      packet = new byte[1024]; //if its bigger too bad :-)
      oid = new byte[1024]; //to store our OID
     
      comlen = community.Length;
      OIDvals = OIDstring.Split('.');
      OIDlen = OIDvals.Length;
      orgoidlen = OIDvals.Length;

      // Convert the OID string into a byte array of integer values
      // Values over 128 require multiple bytes which also increases the
      // OID length because its ASN.1.. no way to know before to well before
      for (i = 0; i < orgoidlen; i++)
      {
         temp = Convert.ToInt16(OIDvals[i]);
         if (temp > 127)
         {
            oid[cnt] = Convert.ToByte(128 + (temp / 128));
            oid[cnt + 1] = Convert.ToByte(temp - ((temp / 128) * 128));
            cnt += 2;
            OIDlen++;
         } else
         {
            oid[cnt] = Convert.ToByte(temp);
            cnt++;
         }
      }
      snmplen = 29 + comlen + OIDlen - 1;  //Length of entire SNMP packet

      //The SNMP sequence start
      packet[pos++] = 0x30; //Sequence start
      packet[pos++] = Convert.ToByte(snmplen - 2);  //sequence size

      //SNMP version
      packet[pos++] = 0x02; //Integer type
      packet[pos++] = 0x01; //length
      packet[pos++] = 0x00; //SNMP version 1

      //Community name
      packet[pos++] = 0x04; // String type
      packet[pos++] = Convert.ToByte(comlen); //length
      //Convert community name to byte array
      byte[] data = Encoding.ASCII.GetBytes(community);
      for (i = 0; i < data.Length; i++)
      {
         packet[pos++] = data[i];
      }

      //Add GetRequest or GetNextRequest value
      if (request == "get")
         packet[pos++] = 0xA0;
      else
         packet[pos++] = 0xA1;

      packet[pos++] = Convert.ToByte(20 + OIDlen - 1); //Size of total OID

      //Request ID
      packet[pos++] = 0x02; //Integer type
      packet[pos++] = 0x04; //length
      packet[pos++] = 0x00; //SNMP request ID
      packet[pos++] = 0x00;
      packet[pos++] = 0x00;
      packet[pos++] = 0x01;

      //Error status
      packet[pos++] = 0x02; //Integer type
      packet[pos++] = 0x01; //length
      packet[pos++] = 0x00; //SNMP error status

      //Error index
      packet[pos++] = 0x02; //Integer type
      packet[pos++] = 0x01; //length
      packet[pos++] = 0x00; //SNMP error index

      //Start of variable bindings
      packet[pos++] = 0x30; //Start of variable bindings sequence

      packet[pos++] = Convert.ToByte(6 + OIDlen - 1); // Size of variable binding

      packet[pos++] = 0x30; //Start of first variable bindings sequence
      packet[pos++] = Convert.ToByte(6 + OIDlen - 1 - 2); // size
      packet[pos++] = 0x06; //Object type
      packet[pos++] = Convert.ToByte(OIDlen - 1); //length

      //Start of MIB
      packet[pos++] = 0x2b;
      //Place MIB array in packet
      for(i = 2; i < OIDlen; i++)
         packet[pos++] = Convert.ToByte(oid[i]);
      packet[pos++] = 0x05; //Null object value
      packet[pos++] = 0x00; //Null

      //Send packet to destination
      Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                       ProtocolType.Udp);
      sock.SetSocketOption(SocketOptionLevel.Socket,
                      SocketOptionName.ReceiveTimeout, 5000);
      IPHostEntry ihe = Dns.Resolve(host);
      IPEndPoint iep = new IPEndPoint(ihe.AddressList[0], 161);
      EndPoint ep = (EndPoint)iep;
      sock.SendTo(packet, snmplen, SocketFlags.None, iep);

      //Now we check for the response :-) 
      //TODO: Maybe make this an async or not necissary. Blocking...
      try
      {
      	int recv = sock.ReceiveFrom(packet, ref ep);
      } catch (SocketException)
      {
         packet[0] = 0xff;
      }
      return packet;
   }

   public string getnextMIB(byte[] OID)
   {
      string outputStr;
      int commlength,  OIDstart, OIDlength, OIDvalue;
      
      commlength = OID[6];
      OIDstart = 6 + commlength + 17; //we don't care, just jump to the oid 
      outputStr = "1.3"; //Prepend the starting OID
 
      //The MIB length is the length-1 trimming the .0
      OIDlength = OID[OIDstart] - 1;
      OIDstart += 2; //skip over the length and 0x2b values
      
      for(int i = OIDstart; i < OIDstart + OIDlength; i++)
      {
         OIDvalue = Convert.ToInt16(OID[i]);
         if (OIDvalue > 128)
         {
            OIDvalue = (OIDvalue/128)*128 + Convert.ToInt16(OID[i+1]);
            i++;
         }
         outputStr += "." + OIDvalue;
      }
      return outputStr;
   }
}
}
