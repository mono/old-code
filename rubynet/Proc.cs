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
    public class RProc : RData
    {
        internal RProc(NetRuby rb) :
            base(rb, rb.cProc)
        {
            CreateBlock(rb);
        }
        internal RProc(NetRuby rb, RMetaObject meta) :
            base(rb, meta)
        {
            CreateBlock(rb);
        }
        private void CreateBlock(NetRuby rb)
        {
/*        
            RThread th = rb.GetCurrentContext();
            block = th.block.Clone();
            block.origThread = th;
            block.wrapper = th.wrapper;
            block.iter = (block.prev != null) ?  ITER.PRE : ITER.NOT;
            block.frame = block.frame.Dup();
            if (th.frame.prev != null)
            {
                block.frame.lastFunc = th.frame.prev.lastFunc;
                block.frame.lastClass = th.frame.prev.lastClass;
            }
            if (block.iter != 0)
            {
                block.DupPrev();
            }
            else
            {
                block.prev = null;
            }
            block.scope.Dup();
            safeLevel = th.safeLevel;
*/            
        }
/*        
        internal int SafeLevel
        {
            get { return safeLevel; }
            set {
                if (IsTainted)
                {
                    ruby.GetCurrentContext().safeLevel = safeLevel;
                }
            }
        }
*/        
        internal Block block;
        //internal int safeLevel;

        public override string ToString()
        {
            string name = ruby.ClassOf(this).Name;
            ////return String.Format("#<{0}:0x{1:x8}>", name, block.TagID);
            return String.Format("#<{0}>", name);
        }
        public int Arity
        {
            get {
/*            
                int n = -1;
                if (block.var != null)
                {
                    if (block.var is RNBlockNoArg)
                    {
                        n = 0;
                    }
                    else if (block.var is RNMAsgn)
                    {
                        RNode list = block.var.head;
                        n = 0;
                        while (list != null)
                        {
                            n++;
                            list = list.next;
                        }
                        if (block.var.args != null)
                            n = -n - 1;
                    }
                }
                return n;
*/
                return 0; //PH
            }
        }
        public object Call(params object[] args)
        {
            return ruby.Call(this, args);
        }
    }
    
    public class RProcClass : RClass
    {
        private RProcClass(NetRuby rb) :
            base(rb, "Proc", rb.cObject)
        {
        }

        static public RProc NewProc(object[] argv, RMetaObject meta)
        {
/*        
            NetRuby ruby = meta.ruby;
            if (ruby.IsBlockGiven == false && (bool)ruby.ruby_block_given_p(null) == false)
            {
                throw new ArgumentException("tried to create Proc object without a block");
            }
            RProc proc = new RProc(ruby, meta);
            ruby.CallInit(proc, argv);
            return proc;
*/
            return null; //PH 
        }

        internal static object proc_new(RBasic r, params object[] args)
        {
            RBasic o = NewProc(args, (RMetaObject)r);
            return o;
        }
        internal static object proc_call(RBasic r, params object[] args)
        {
/*        
            NetRuby ruby = r.ruby;
            RThread th = ruby.GetCurrentContext();
            if (th.IsBlockGiven && th.frame.lastFunc != 0)
            {
                ruby.warning(String.Format("block for {0}#{1} is useless",
                                           ruby.ClassOf(r).Name,
                                           ruby.id2name(th.frame.lastFunc)));
            }
            return ruby.Call((RProc)r, args);
*/
            return null; //PH
        }
        internal static object proc_arity(RBasic r, params object[] args)
        {
            return ((RProc)r).Arity;
        }
        internal static object proc_lambda(RBasic r, params object[] args)
        {
/*        
            return r.ruby.Lambda();
*/
            return null; //PH
        }
        
        internal static void Init(NetRuby rb)
        {
            RProcClass prc = new RProcClass(rb);
            prc.DefineClass("Proc", rb.cObject);
            rb.cProc = prc;
            prc.DefineSingletonMethod("new", new RMethod(proc_new), -1);
            prc.DefineMethod("call", new RMethod(proc_call), -2);
            prc.DefineMethod("arity", new RMethod(proc_arity), 0);
            prc.DefineMethod("[]", new RMethod(proc_call), -2);
            rb.DefineGlobalFunction("proc", new RMethod(proc_lambda), 0);
            rb.DefineGlobalFunction("lambda", new RMethod(proc_lambda), 0);
        }
    }
}
