using System;
using System.Collections.Generic;
using System.Text;

namespace Mono.VisualStudio.Debugger
{
    public static class COM
    {
        public const int S_OK = 0;
        public const int S_FALSE = 1;
        public const int E_NOTIMPL = unchecked ((int)0x80004001);
		public const int RPC_E_SERVERFAULT = unchecked ((int)0x80010105);
    }
}
