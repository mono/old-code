using System;
using Mono.Net.Snmp;

namespace Mono.Net.Snmp.Samples
{
class MacAddress
{
   public static void Main(string[] argv)
   {
      int commlength, miblength, datastart, datalength;
      string nextmib, value;
      SNMP conn = new SNMP();
      string mib = "1.3.6.1.2.1.17.4.3.1.1";
      int orgmiblength = mib.Length;
      byte[] response = new byte[1024];

      nextmib = mib;

      while (true)
      {
         response = conn.get("getnext", argv[0], argv[1], nextmib);
         commlength = Convert.ToInt16(response[6]);
         miblength = Convert.ToInt16(response[23 + commlength]);
         datalength = Convert.ToInt16(response[25 + commlength + miblength]);
         datastart = 26 + commlength + miblength;
         value = BitConverter.ToString(response, datastart, datalength);
         nextmib = conn.getnextMIB(response);
         if (!(nextmib.Substring(0, orgmiblength) == mib))
            break;

         Console.WriteLine("{0} = {1}", nextmib, value);
      }
   }
}
}
