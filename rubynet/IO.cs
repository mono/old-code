/*
Copyright (C) 2001-2002 arton
Copyright (C) 2005      Jaen Saul
*/
using System;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Security;

namespace NETRuby
{
    public class RIOClass : RClass
    {
        private RIOClass(NetRuby rb) :
            base(rb, "IO", rb.cObject)
        {
        }
        
        public object ruby_print(RBasic r, params object[] o)
        {
            string s = String.Empty;
            if (o.Length == 0)
            {
                s = obj_to_s(NetRuby.lastLineGetter(0, null, ruby));
            }
            else
            {
                foreach (object x in o)
                {
                    s += obj_to_s(x);
                }
            }
            System.Console.Out.Write(s);
            return null;
        }

        public object ruby_printf(RBasic r, params object[] o)
        {
            object result = RKernel.sprintf(r, o);
            return ruby_print(r, result.ToString());
        }
        
        public object ruby_p(RBasic r, params object[] o)
        {
            foreach (object x in o)
            {
                System.Console.Out.Write(RString.AsString(ruby, ruby.Inspect(x)) + ruby.defaultRecSep);
            }
            // check defout and if file, then flush
            return null;
        }
        
        internal static void Init(NetRuby rb)
        {
            RIOClass io = new RIOClass(rb);
            io.DefineClass("IO", rb.cObject);
            rb.cIO = io;
        
            rb.DefineGlobalFunction("printf", new RMethod(io.ruby_printf), -1);
            rb.DefineGlobalFunction("print", new RMethod(io.ruby_print), -1);
        
            rb.DefineGlobalFunction("p", new RMethod(io.ruby_p), -1);

        }
    }
}
