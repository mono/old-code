/*
Copyright (C) 2001-2002 arton
Copyright (C) 2005      Jaen Saul
*/

using System;
using System.Collections;
using System.Reflection;
using System.Security;

namespace NETRuby
{
    public class Symbol : RBasic
    {
        const uint SYMBOL_FLAG = 16;

        private Symbol(NetRuby rb) :
            base(rb, rb.cSymbol)
        {
            val = 0;
        }
        internal Symbol(NetRuby rb, uint v) :
            base(rb, rb.cSymbol)
        {
            val = v;
        }
        static internal uint ID2SYM(uint u)
        {
            return (u << 8) | SYMBOL_FLAG;
        }

        static internal uint SYM2ID(uint u)
        {
            return u >> 8;
        }

        static internal bool IsSymbol(object o)
        {
            if (o is uint)
            {
                uint u = (uint)o;
                if ((u & 0xff) == SYMBOL_FLAG)
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            return ruby.id2name(val >> 8);
        }

        uint val;
        internal Symbol SetData(uint u)
        {
            val = u;
            return this;
        }

        static object to_i(RBasic rcver, params object[] args)
        {
            int i = (int)SYM2ID(((Symbol)rcver).val);
            return i;
        }
        static object inspect(RBasic rcver, params object[] args)
        {
            return new RString(rcver.ruby, ":" + rcver.ToString());
        }
        static object to_s(RBasic rcver, params object[] args)
        {
            return new RString(rcver.ruby, rcver.ToString());
        }
        static object id2name(RBasic rcver, params object[] args)
        {
            return new RString(rcver.ruby, rcver.ToString());
        }
        static object intern(RBasic rcver, params object[] args)
        {
            return ((Symbol)rcver).val;
        }

        static internal void Init(NetRuby rb)
        {
            RClass sym = rb.DefineClass("Symbol", rb.cObject);
            rb.cSymbol = sym;
            rb.oSymbol = new Symbol(rb);
            sym.DefineMethod("to_i", new RMethod(to_i), 0);
            sym.DefineMethod("to_int", new RMethod(to_i), 0);
            sym.DefineMethod("inspect", new RMethod(inspect), 0);
            sym.DefineMethod("to_s", new RMethod(to_s), 0);
            sym.DefineMethod("id2name", new RMethod(to_s), 0);
        }
    }
}
