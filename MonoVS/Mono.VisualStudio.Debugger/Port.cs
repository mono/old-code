using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices.ComTypes;

using Mono.VisualStudio.Mdb;

namespace Mono.VisualStudio.Debugger
{
	public class Port : IDebugPort2, IDebugPortNotify2, IConnectionPointContainer
	{
		List<IDebugProgramNode2> nodes = new List<IDebugProgramNode2>();

		internal DebugPortEvents2ConnectionPoint PortEventsCP { get; private set; }

		// The system program publisher
		static IDebugProgramPublisher2 program_publisher;

		static Port()
		{
			program_publisher = (IDebugProgramPublisher2)Activator.CreateInstance(
			Type.GetTypeFromCLSID(new Guid(Guids.VS_ProgramPublisher)));
		}

		public Port(PortSupplier supplier, string baseurl)
		{
			PortSupplier = supplier;
			PortAddress = baseurl;
			Guid = Guid.NewGuid();
			PortEventsCP = new DebugPortEvents2ConnectionPoint(this);
		}

		public PortSupplier PortSupplier { get; private set; }

		public Guid Guid { get; private set; }
		public string PortAddress { get; private set; }

		#region IDebugPort2: Describes the port and can enumerate processes running on the port

		public int EnumProcesses(out IEnumDebugProcesses2 ppEnum)
		{
			throw new NotImplementedException();
		}

		public int GetPortId(out Guid guidport)
		{
			guidport = Guid;
			return COM.S_OK;
		}

		public int GetPortName(out string name)
		{
			name = PortAddress;
			return COM.S_OK;
		}

		public int GetPortRequest(out IDebugPortRequest2 ppRequest)
		{
			throw new NotImplementedException();
		}

		public int GetPortSupplier(out IDebugPortSupplier2 portSupplier)
		{
			portSupplier = PortSupplier;
			return COM.S_OK;
		}

		public int GetProcess(AD_PROCESS_ID ProcessId, out IDebugProcess2 ppProcess)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IDebugPortNotify2

		public int AddProgramNode(IDebugProgramNode2 node)
		{
			program_publisher.PublishProgramNode(node);
			nodes.Add(node);
			return COM.S_OK;
		}

		public int RemoveProgramNode(IDebugProgramNode2 node)
		{
			nodes.Remove(node);
			program_publisher.UnpublishProgramNode(node);
			return COM.S_OK;
		}

		#endregion

		#region IConnectionPointContainer Members

		public void EnumConnectionPoints(out IEnumConnectionPoints ppEnum)
		{
			throw new NotImplementedException();
		}

		public void FindConnectionPoint(ref Guid riid, out IConnectionPoint ppCP)
		{
			ppCP = null;
			if (riid == typeof(IDebugPortEvents2).GUID)
			{
				ppCP = PortEventsCP;
			} else
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		internal class DebugPortEvents2ConnectionPoint : IConnectionPoint
		{
			IConnectionPointContainer container;
			// fire events to these sinks
			List<IDebugPortEvents2> sinks = new List<IDebugPortEvents2>();

			public DebugPortEvents2ConnectionPoint(IConnectionPointContainer container)
			{
				this.container = container;
			}

			public void Event(IDebugCoreServer2 pServer, IDebugPort2 pPort, IDebugProcess2 pProcess, IDebugProgram2 pProgram, IDebugEvent2 pEvent, ref Guid riidEvent)
			{
				foreach (IDebugPortEvents2 sink in sinks)
				{
					// sink can be null, UnAdvise simply nulls out value in list
					if (sink != null)
						sink.Event(pServer, pPort, pProcess, pProgram, pEvent, ref riidEvent);
					// TODO: Should we check return value?
				}
			}

			#region IConnectionPoint Members

			public void Advise(object pUnkSink, out int pdwCookie)
			{
				IDebugPortEvents2 events = pUnkSink as IDebugPortEvents2;
				if (events != null)
				{
					// index for sink will be pdwCookie - 1
					sinks.Add(events);
					pdwCookie = sinks.Count;
					return;
				}
				throw new ArgumentException();
			}

			public void EnumConnections(out IEnumConnections ppEnum)
			{
				throw new NotImplementedException();
			}

			public void GetConnectionInterface(out Guid pIID)
			{
				pIID = typeof(IDebugPortEvents2).GUID;
			}

			public void GetConnectionPointContainer(out IConnectionPointContainer ppCPC)
			{
				ppCPC = container;
			}

			public void Unadvise(int dwCookie)
			{
				if (dwCookie > 0 && dwCookie <= sinks.Count)
					sinks[dwCookie - 1] = null;
			}

			#endregion
		}
	}
}
