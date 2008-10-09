using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;

using Mono.VisualStudio.Mdb;

namespace Mono.VisualStudio.Debugger
{
	public class StackFrame : IDebugStackFrame2, IDebugExpressionContext2
	{
		public Engine Engine
		{
			get;
			private set;
		}

		internal IServerStackFrame Frame
		{
			get;
			private set;
		}

		public StackFrame (Engine engine, IServerStackFrame frame)
		{
			this.Engine = engine;
			this.Frame = frame;
		}

		// Construct a FRAMEINFO for this stack frame with the requested information.
		public void SetFrameInfo (uint dwFieldSpec, out FRAMEINFO frameInfo)
		{
			frameInfo = new FRAMEINFO();

			// The debugger is asking for the formatted name of the function which is displayed in the callstack window.
			// There are several optional parts to this name including the module, argument types and values, and line numbers.
			// The optional information is requested by setting flags in the dwFieldSpec parameter.
			if ((dwFieldSpec & (uint)enum_FRAMEINFO_FLAGS.FIF_FUNCNAME) != 0)
			{
				frameInfo.m_bstrFuncName = Frame.Name;
				frameInfo.m_dwValidFields |= (uint)enum_FRAMEINFO_FLAGS.FIF_FUNCNAME;
			}
			
			// The debugger is requesting the name of the module for this stack frame.
			if ((dwFieldSpec & (uint)enum_FRAMEINFO_FLAGS.FIF_MODULE) != 0)
			{
				frameInfo.m_bstrModule = "Module";
				frameInfo.m_dwValidFields |= (uint)enum_FRAMEINFO_FLAGS.FIF_MODULE;
			}

			if ((dwFieldSpec & (uint) enum_FRAMEINFO_FLAGS.FIF_LANGUAGE) != 0) {
				frameInfo.m_bstrModule = "C#";
				frameInfo.m_dwValidFields |= (uint) enum_FRAMEINFO_FLAGS.FIF_LANGUAGE;
			}

			// The debugger is requesting the range of memory addresses for this frame.
			// For the sample engine, this is the contents of the frame pointer.
			if ((dwFieldSpec & (uint)enum_FRAMEINFO_FLAGS.FIF_STACKRANGE) != 0)
			{
				frameInfo.m_addrMin = Frame.StackPointer;
				frameInfo.m_addrMax = Frame.StackPointer;
				frameInfo.m_dwValidFields |= (uint)enum_FRAMEINFO_FLAGS.FIF_STACKRANGE;
			}

			// The debugger is requesting the IDebugStackFrame2 value for this frame info.
			if ((dwFieldSpec & (uint)enum_FRAMEINFO_FLAGS.FIF_FRAME) != 0)
			{
				frameInfo.m_pFrame = this;
				frameInfo.m_dwValidFields |= (uint)enum_FRAMEINFO_FLAGS.FIF_FRAME;
			}

			// Does this stack frame of symbols loaded?
			if ((dwFieldSpec & (uint)enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO) != 0)
			{
				frameInfo.m_fHasDebugInfo = 1;
				frameInfo.m_dwValidFields |= (uint)enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO;
			}

			// Is this frame stale?
			if ((dwFieldSpec & (uint)enum_FRAMEINFO_FLAGS.FIF_STALECODE) != 0)
			{
				frameInfo.m_fStaleCode = 0;
				frameInfo.m_dwValidFields |= (uint)enum_FRAMEINFO_FLAGS.FIF_STALECODE;
			}
		}

		#region IDebugStackFrame2 Members

		int IDebugStackFrame2.EnumProperties(uint dwFields, uint nRadix, ref Guid guidFilter, uint dwTimeout, out uint pcelt, out IEnumDebugPropertyInfo2 ppEnum)
		{
			pcelt = 0;
			ppEnum = null;
			return COM.E_NOTIMPL;
		}

		internal AD7MemoryAddress GetCodeContext ()
		{
			return new AD7MemoryAddress (Engine, Frame.Address);
		}

		int IDebugStackFrame2.GetCodeContext(out IDebugCodeContext2 context)
		{			
			context = GetCodeContext ();
			return COM.S_OK;
		}

		int IDebugStackFrame2.GetDebugProperty(out IDebugProperty2 ppProperty)
		{
			ppProperty = null;
			return COM.E_NOTIMPL;
		}

		public int GetDocumentContext (out IDebugDocumentContext2 context)
		{
			context = null;
			ISourceLocation location = Frame.GetLocation ();
			if (location == null)
				return COM.S_FALSE;

			Utils.Message ("GET DOCUMENT CONTEXT: {0} {1}", location.FileName, location.Line);
			TEXT_POSITION start, end;
			start.dwLine = (uint) location.Line - 1; start.dwColumn = 0;
			end.dwLine = (uint) location.Line; end.dwColumn = 0;
			context = new AD7DocumentContext (location.FileName, start, end, GetCodeContext ());
			return COM.S_OK;
		}

		int IDebugStackFrame2.GetExpressionContext(out IDebugExpressionContext2 ppExprCxt)
		{
			ppExprCxt = null;
			return COM.E_NOTIMPL;
		}

		int IDebugStackFrame2.GetInfo (uint dwFieldSpec, uint nRadix, FRAMEINFO[] pFrameInfo)
		{
			SetFrameInfo (dwFieldSpec, out pFrameInfo[0]);
			return COM.S_OK;
		}

		int IDebugStackFrame2.GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
		{
			pbstrLanguage = "C++";
			pguidLanguage = Guids.guidLanguageCpp;
			return COM.S_OK;
		}

		int IDebugStackFrame2.GetName(out string pbstrName)
		{
			throw new NotImplementedException();
		}

		int IDebugStackFrame2.GetPhysicalStackRange(out ulong paddrMin, out ulong paddrMax)
		{
			throw new NotImplementedException();
		}

		int IDebugStackFrame2.GetThread(out IDebugThread2 ppThread)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IDebugExpressionContext2 Members

		int IDebugExpressionContext2.GetName(out string pbstrName)
		{
			throw new NotImplementedException();
		}

		int IDebugExpressionContext2.ParseText(string pszCode, uint dwFlags, uint nRadix, out IDebugExpression2 ppExpr, out string pbstrError, out uint pichError)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}

