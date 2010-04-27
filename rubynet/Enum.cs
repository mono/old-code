/*
Copyright (C) 2001-2002 arton
Copyright (C) 2005      Jaen Saul
*/

using System;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Reflection;
using System.Security;

namespace NETRuby
{
    public class REnumerable : RModule
    {
        private REnumerable(NetRuby rb) :
            base(rb, "Enumerable")
        {
        }
        internal static object Each(RBasic r)
        {
            return r.ruby.Funcall(r, "each");
        }
        private static object collect_i(object it, object data, object self)
        {
/*        
            RArray a = (RArray)data;
            a.Add(a.ruby.Yield(it));
*/            
            return null;
        }
        private static object find_all_i(object it, object data, object self)
        {
/*        
            RArray a = (RArray)data;
            object o = a.ruby.Yield(it);
            if (RTest(o))
            {
                a.Add(o);
            }
*/                        
            return null;
        }
        private static object enum_all(object it, object data, object self)
        {
            ((RArray)data).Add(it);
            return null;
        }
        private static object find_all(RBasic r, params object[] args)
        {
/*        
            NetRuby ruby = r.ruby;
            RArray ary = new RArray(ruby, true);
            ruby.Iterate(new NetRuby.IterationProc(Each), r,
                         new NetRuby.BlockProc(find_all_i), ary);
            return ary;
*/
            return null; //PH
        }
        private static object collect(RBasic r, params object[] args)
        {
/*        
            NetRuby ruby = r.ruby;
            RArray ary = new RArray(ruby, true);
            ruby.Iterate(new NetRuby.IterationProc(Each), r,
                         (ruby.IsBlockGiven) ? new NetRuby.BlockProc(collect_i) :
                                               new NetRuby.BlockProc(enum_all), ary);
            return ary;
*/
            return null; //PH
        }
        
        internal static void Init(NetRuby rb)
        {
            REnumerable en = new REnumerable(rb);
            rb.mEnumerable = en;

            en.DefineMethod("find_all", new RMethod(find_all), 0);
            en.DefineMethod("select", new RMethod(find_all), 0);
        
            en.DefineMethod("collect", new RMethod(collect), 0);
            en.DefineMethod("map", new RMethod(collect), 0);
        }
    }
}
