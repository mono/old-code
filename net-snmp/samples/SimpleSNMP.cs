using System;
using System.Text;
using Mono.Net.Snmp;


namespace Mono.Net.Snmp.Samples
{
class SimpleSNMP
{
   public static void Main(string[] argv)
   {
      int commlength, miblength, datatype, datalength, datastart;
      int uptime = 0;
      string output;
      SNMP conn = new SNMP();
      byte[] response = new byte[1024];

      Console.WriteLine("Device SNMP information:");

      // Send sysName SNMP request
      response = conn.get("get", argv[0], argv[1], "1.3.6.1.2.1.1.5.0");
      if (response[0] == 0xff)
      {
         Console.WriteLine("No response from {0}", argv[0]);
         return;
      }

      // If response, get the community name and MIB lengths
      commlength = Convert.ToInt16(response[6]);
      miblength = Convert.ToInt16(response[23 + commlength]);

      // Extract the MIB data from the SNMP response
      datatype = Convert.ToInt16(response[24 + commlength + miblength]);
      datalength = Convert.ToInt16(response[25 + commlength + miblength]);
      datastart = 26 + commlength + miblength;
      output = Encoding.ASCII.GetString(response, datastart, datalength);
      Console.WriteLine("  sysName - Datatype: {0}, Value: {1}",
              datatype, output);

      // Send a sysLocation SNMP request
      response = conn.get("get", argv[0], argv[1], "1.3.6.1.2.1.1.6.0");
      if (response[0] == 0xff)
      {
         Console.WriteLine("No response from {0}", argv[0]);
         return;
      }

      // If response, get the community name and MIB lengths
      commlength = Convert.ToInt16(response[6]);
      miblength = Convert.ToInt16(response[23 + commlength]);

      // Extract the MIB data from the SNMP response
      datatype = Convert.ToInt16(response[24 + commlength + miblength]);
      datalength = Convert.ToInt16(response[25 + commlength + miblength]);
      datastart = 26 + commlength + miblength;
      output = Encoding.ASCII.GetString(response, datastart, datalength);
      Console.WriteLine("  sysLocation - Datatype: {0}, Value: {1}", datatype, output);

      // Send a sysContact SNMP request
      response = conn.get("get", argv[0], argv[1], "1.3.6.1.2.1.1.4.0");
      if (response[0] == 0xff)
      {
         Console.WriteLine("No response from {0}", argv[0]);
         return;
      }

      // Get the community and MIB lengths
      commlength = Convert.ToInt16(response[6]);
      miblength = Convert.ToInt16(response[23 + commlength]);

      // Extract the MIB data from the SNMP response
      datatype = Convert.ToInt16(response[24 + commlength + miblength]);
      datalength = Convert.ToInt16(response[25 + commlength + miblength]);
      datastart = 26 + commlength + miblength;
      output = Encoding.ASCII.GetString(response, datastart, datalength);
      Console.WriteLine("  sysContact - Datatype: {0}, Value: {1}",
              datatype, output);
      
      // Send a SysUptime SNMP request
      response = conn.get("get", argv[0], argv[1], "1.3.6.1.2.1.1.3.0");
      if (response[0] == 0xff)
      {
         Console.WriteLine("No response from {0}", argv[0]);
         return;
      }

      // Get the community and MIB lengths of the response
      commlength = Convert.ToInt16(response[6]);
      miblength = Convert.ToInt16(response[23 + commlength]);

      // Extract the MIB data from the SNMp response
      datatype = Convert.ToInt16(response[24 + commlength + miblength]);
      datalength = Convert.ToInt16(response[25 + commlength + miblength]);
      datastart = 26 + commlength + miblength;

      // The sysUptime value may by a multi-byte integer
      // Each byte read must be shifted to the higher byte order
      while(datalength > 0)
      {
         uptime = (uptime << 8) + response[datastart++];
         datalength--;
      }
      Console.WriteLine("  sysUptime - Datatype: {0}, Value: {1}",
             datatype, uptime);

   }
}
}
