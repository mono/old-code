
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices.ComTypes;

namespace Mono.VisualStudio.Debugger
{
	[ComVisible (true)]
	[Guid (Guids.PortSupplierClass)]
	public class PortSupplier : IDebugPortSupplier2
	{
		List<Port> ports = new List<Port>();

		/// <summary>The default port, we will have more if we debug multiple systems</summary>
		static public Port MainPort { get; private set; }

		// use Server as first argument when firing events
		public IDebugCoreServer2 Server { get; private set; }

		public const string PortSupplierName = "MonoPortSupplier";

		public PortSupplier()
		{
			MainPort = new Port (this, MonoAddin.Settings.Instance ().ServerURL);
			ports.Add(MainPort);
		}

		#region IDebugPortSupplier2 Members

		public int AddPort(IDebugPortRequest2 request, out IDebugPort2 debug_port)
		{
			string name;
			Utils.RequireOk(request.GetPortName(out name));
			debug_port = MainPort;
			return COM.S_OK;
		}

		public int CanAddPort()
		{
			return COM.S_OK;
		}

		public int EnumPorts(out IEnumDebugPorts2 ppEnum)
		{
			throw new NotImplementedException();
		}

		public int GetPort(ref Guid guidPort, out IDebugPort2 port)
		{
			foreach (Port p in ports)
			{
				if (p.Guid == guidPort)
				{
					port = p;
					return COM.S_OK;
				}
			}
			port = null;
			return COM.S_FALSE;
		}

		public int GetPortSupplierId(out Guid portSupplierGuid)
		{
			portSupplierGuid = new Guid(Guids.PortSupplierClass);
			return COM.S_OK;
		}

		public int GetPortSupplierName(out string name)
		{
			name = PortSupplierName;
			return COM.S_OK;
		}

		public int RemovePort(IDebugPort2 pPort)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
