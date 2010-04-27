/*
Copyright (C) 2001-2002 arton
Copyright (C) 2005      Jaen Saul
*/

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Security;

namespace NETRuby
{
    public enum NOEX
    {
        PUBLIC = 0,
        UNDEF = 1,
        CFUNC = 1,
        PRIVATE = 2,
        PROTECTED = 4,
    }

    internal partial class RNode
    {
        internal RNode()
        {
#if _SCANNER_DEBUG
            System.Console.WriteLine("Create Node:" + ToString());
#endif        
        }
        internal RNode(RNode nd)
        {
            File = nd.File;
            Line = nd.Line;
        }
        internal RNode(RThread th)
        {
#if _SCANNER_DEBUG
            System.Console.WriteLine("Create Node:" + ToString());
#endif        
            if (th != null)
            {
                File = th.file;
                Line = th.line;
            }
            else
            {
                File = "internal";
                Line = 0;
            }
        }
/*
        internal virtual RNode Eval(NetRuby ruby, object self, out object result)
        {
            ruby.bug("Eval: unknown node type " + ToString());
        
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            result = null;
            return null;
        }
*/
/*LIB
        internal virtual object[] SetupArgs(NetRuby ruby, object self, RNode n)
        {
#if EVAL_TRACE
            System.Console.WriteLine(String.Format("SetupArgs self={0} node={1}",
                                                   (self == null) ? "null" : self.ToString(),
                                                   (n == null) ? "null" : n.ToString()));
#endif
            if (n == null) return new object[0];

            RThread th = ruby.GetCurrentContext();
        
            int argc = 0;
            object[] argv = null;
            if (n is RNArray)
            {
                argc = n.alen;
                if (argc > 0)
                {
                    string file = th.file;
                    int line = th.line;
                    argv = new object[argc];
                    for (int i = 0; i < argc; i++)
                    {
                        argv[i] = ruby.Eval(self, n.head);
#if EVAL_TRACE
                        if (argv[i] == null)
                            System.Console.WriteLine("arg" + i.ToString() + "=null");
                        else
                            System.Console.WriteLine("arg" + i.ToString() + "=" + argv[i].ToString());
#endif
                        n = n.next;
                    }
                    th.file = file;
                    th.line = line;
                }
                else
                {
                    argv = new object[0];
                }
            }
            else
            {
                object nargs = ruby.Eval(self, n);
                string file = th.file;
                int line = th.line;
                ArrayList ar;
                if (nargs is RArray)
                {
                    ar = ((RArray)nargs).ArrayList;
                }
                else if (nargs is ArrayList == false)
                {
                    object o = nargs;
                    ar = new ArrayList();
                    ar.Add(nargs);
                }
                else
                {
                    ar = (ArrayList)nargs;
                }
                argv = new object[ar.Count];
                for (int i = 0; i < ar.Count; i++)
                {
                    argv[i] = ar[i];
                }
                th.file = file;
                th.line = line;
            }
            return argv;
        }
*/
/*
        internal virtual object Funcall(RMetaObject klass, object recver, uint id, object[] args)
        {
            klass.ruby.bug("Funcall: unknown node type " + ToString());
            return null;
        }

        internal virtual object Assign(NetRuby ruby, object self, object val, bool check)
        {
            ruby.bug("bug in variable assignment");
            return null;
        }

        protected string ArgDefined(NetRuby ruby, object self, RNode node, string type)
        {
            if (node == null) return type;
            if (node is RNArray)
            {
                int argc = node.alen;
                if (argc > 0)
                {
                    for (int i = 0; i < argc; i++)
                    {
                        if (IsDefined(ruby, self, node.head) == null)
                            return null;
                        node = node.next;
                    }
                }
            }
            else if (IsDefined(ruby, self, node) == null)
            {
                return null;
            }
            return type;
        }
        static internal string IsDefined(NetRuby ruby, object self, RNode node)
        {
            if (node == null) return "expression";
            return node.IsDefined(ruby, self);
        }

        protected virtual string IsDefined(NetRuby ruby, object self)
        {
            RThread th = ruby.GetCurrentContext();
            string s = null;
            th.PushTag(Tag.TAG.PROT_NONE);
            try
            {
                ruby.Eval(self, this);
                s = "expression";
            }
            catch
            {
                th.errInfo = null;
            }
            th.PopTag(false);
            return s;
        }
*/

        internal virtual RNode head
        {
            get { throw new NotSupportedException("bug(head get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(head set):" + GetType().ToString()); }
        }
        internal virtual int alen
        {
            get { throw new NotSupportedException("bug(alen get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(alen set):" + GetType().ToString()); }
        }
        internal virtual RNode next
        {
            get { throw new NotSupportedException("bug(next get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(next set):" + GetType().ToString()); }
        }
        
        internal virtual RNode cond
        {
            get { throw new NotSupportedException("bug(cond get):" + GetType().ToString()); }
        }
        internal virtual RNode body
        {
            get { throw new NotSupportedException("bug(body get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(body set):" + GetType().ToString()); }
        }
        internal virtual RNode nd_else
        {
            get { throw new NotSupportedException("bug(nd_else get):" + GetType().ToString()); }
        }

/*
        internal virtual RMetaObject orig
        {
            get { throw new NotSupportedException("bug(orig get):" + GetType().ToString()); }
        }
*/
        internal virtual RNode resq
        {
            get { throw new NotSupportedException("bug(resq get):" + GetType().ToString()); }
        }
        internal virtual RNode ensr
        {
            get { throw new NotSupportedException("bug(ensr get):" + GetType().ToString()); }
        }

        internal virtual RNode nd_1st
        {
            get { throw new NotSupportedException("bug(nd_1st get):" + GetType().ToString()); }
        }
        internal virtual RNode nd_2nd
        {
            get { throw new NotSupportedException("bug(nd_2nd get):" + GetType().ToString()); }
        }

        internal virtual RNode stts
        {
            get { throw new NotSupportedException("bug(stts get):" + GetType().ToString()); }
        }

        internal virtual GlobalEntry entry
        {
            get { throw new NotSupportedException("bug(entry get):" + GetType().ToString()); }
        }

        internal virtual uint vid
        {
            get { throw new NotSupportedException("bug(vid get):" + GetType().ToString()); }
        }
        internal virtual uint cflag
        {
            get { throw new NotSupportedException("bug(cflag get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(cflag set):" + GetType().ToString()); }
        }
        internal virtual int cnt
        {
            get { throw new NotSupportedException("bug(cnt get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(cnt set):" + GetType().ToString()); }
        }
        internal virtual uint[] tbl
        {
            get { throw new NotSupportedException("bug(tbl get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(tbl set):" + GetType().ToString()); }
        }

        internal virtual RNode var
        {
            get { throw new NotSupportedException("bug(var get):" + GetType().ToString()); }
        }
/*        
        internal virtual RNode ibody
        {
            get { throw new NotSupportedException("bug(ibody get):" + GetType().ToString()); }
        }
*/        
        internal virtual RNode iter
        {
            get { throw new NotSupportedException("bug(iter get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(iter set):" + GetType().ToString()); }
        }

        internal virtual RNode val
        {
            get { throw new NotSupportedException("bug(val get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(val set):" + GetType().ToString()); }
        }
        internal virtual uint aid
        {
            get { throw new NotSupportedException("bug(aid get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(aid set):" + GetType().ToString()); }
        }

        internal virtual object lit
        {
            get { throw new NotSupportedException("bug(lit get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(lit set):" + GetType().ToString()); }
        }
/*
        internal virtual RNode frml
        {
            get { throw new NotSupportedException("bug(frml get):" + GetType().ToString()); }
        }
*/        
        internal virtual int rest
        {
            get { throw new NotSupportedException("bug(rest get):" + GetType().ToString()); }
        }
        
        internal virtual RNode opt
        {
            get { throw new NotSupportedException("bug(opt get):" + GetType().ToString()); }
        }

        internal virtual RNode recv
        {
            get { throw new NotSupportedException("bug(recv get):" + GetType().ToString()); }
        }
        internal virtual uint mid
        {
            get { throw new NotSupportedException("bug(mid get):" + GetType().ToString()); }
        }
        internal virtual RNode args
        {
            get { throw new NotSupportedException("bug(args get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(args set):" + GetType().ToString()); }
        }
        
        internal virtual NOEX noex
        {
            get { throw new NotSupportedException("bug(nodex get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(noex set):" + GetType().ToString()); }
        }
        internal virtual RNode defn
        {
            get { throw new NotSupportedException("bug(defn get):" + GetType().ToString()); }
        }

        internal virtual uint old
        {
            get { throw new NotSupportedException("bug(old get):" + GetType().ToString()); }
        }
        internal virtual uint nd_new
        {
            get { throw new NotSupportedException("bug(nd_new get):" + GetType().ToString()); }
        }
/*
        internal virtual MethodInfo cfunc
        {
            get { throw new NotSupportedException("bug(cfunc get):" + GetType().ToString()); }
        }
*/
        internal virtual int argc
        {
            get { throw new NotSupportedException("bug(argc get):" + GetType().ToString()); }
        }

        internal virtual uint cname
        {
            get { throw new NotSupportedException("bug(cname get):" + GetType().ToString()); }
        }
        internal virtual RNode super
        {
            get { throw new NotSupportedException("bug(super get):" + GetType().ToString()); }
        }
/*
        internal virtual uint modl
        {
            get { throw new NotSupportedException("bug(modl get):" + GetType().ToString()); }
        }
*/        
/*
        internal virtual RMetaObject clss
        {
            get { throw new NotSupportedException("bug(clss get):" + GetType().ToString()); }
        }
*/
        internal virtual RNode beg
        {
            get { throw new NotSupportedException("bug(beg get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(beg set):" + GetType().ToString()); }
        }
        internal virtual RNode end
        {
            get { throw new NotSupportedException("bug(end get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(end set):" + GetType().ToString()); }
        }
        internal virtual bool state
        {
            get { throw new NotSupportedException("bug(state get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(state set):" + GetType().ToString()); }
        }
        internal virtual RNode rval
        {
            get { throw new NotSupportedException("bug(rval get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(rval set):" + GetType().ToString()); }
        }

        internal virtual int nth
        {
            get { throw new NotSupportedException("bug(nth get):" + GetType().ToString()); }
            set { throw new NotSupportedException("bug(nth set):" + GetType().ToString()); }
        }
/*
        internal virtual uint tag
        {
            get { throw new NotSupportedException("bug(tag get):" + GetType().ToString()); }
        }
*/        
/*
        internal virtual object tval
        {
            get { throw new NotSupportedException("bug(tval get):" + GetType().ToString()); }
        }
*/        
        internal virtual string file
        {
            get { return File; }
            set { File = value; }
        }

        // list
        internal static RNode list_append(RThread th, RNode head, RNode tail)
        {
            if (head == null) return new RNArray(th, tail);

            RNode last = head;
            while (last.next != null) {
                last = last.next;
            }

            last.next = new RNArray(th, tail);
            head.alen += 1;
            return head;
        }

        internal static RNode list_concat(RNode head, RNode tail)
        {
            RNode last = head;
            while (last.next != null) {
                last = last.next;
            }

            last.next = tail;
            head.alen += tail.alen;

            return head;
        }

        internal static RNode block_append(RThread p, RNode head, RNode tail)
        {
            RNode end = null;

            if (tail == null) return head;
            if (head == null) return tail;

            if (head is RNBlock)
            {
                end = head.end;
            }
            else
            {
                end = new RNBlock(p, head);
                head = end;
            }

            if (p.ruby.verbose)
            {
                for (RNode nd = end.head;;)
                {
                    if (nd is RNNewLine)
                    {
                        nd = nd.next;
                    }
                    else if (nd is RNReturn
                          || nd is RNBreak
                          || nd is RNNext
                          || nd is RNRedo
                          || nd is RNRetry)
                    {
                        p.ruby.warn("statement not reached");
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
 
            if (tail is RNBlock == false) {
                tail = new RNBlock(p, tail);
            }
            end.next = tail;
            head.end = tail.end;
            return head;
        }
        
        internal void FixPos(RNode n)
        {
            if (n != null)
            {
                Line = n.Line;
                File = n.File;
            }
        }

        internal void SetLine(int l)
        {
            Line = l;            
            //RNODE(n)->flags=((RNODE(n)->flags&~(-1<<NODE_LSHIFT))|(((l)&NODE_LMASK)<<NODE_LSHIFT))            
        }

        protected bool RTest(object o)
        {
            return (o == null || (o is bool && ((bool)o) == false)) ? false : true;
        }

        internal int Line;
        internal string File;
    }

    internal partial class RNEOF : RNode
    {
        internal RNEOF()
        {
        }
    }

/*
    internal partial class RNMethod : RNode
    {
        internal RNMethod(RNode n, NOEX e) :
            base((RThread)null)
        {
            nx = e;
            bdy = n;
            ct = 0;
        }
        internal RNMethod(RNMethod m) :
            base(m)
        {
            nx = m.nx;
            ct = 0;
        }
        internal override NOEX noex
        {
            get { return nx; }
            set { nx = value; }
        }
        internal override RNode body
        {
            get { return bdy; }
            set { bdy = value; }
        }
        internal override int cnt
        {
            get { return ct; }
            set { ct = value; }
        }
        NOEX nx;
        RNode bdy;
        int ct;
    }
*/
/*
    internal partial class RNFBody : RNode
    {
        internal RNFBody(RNode n, uint i, RMetaObject origin) :
            base((RThread)null)
        {
            rv = n;
            id = i;
            or = origin;
        }
        internal override RNode head
        {
            get { return rv; }
        }
        internal override RNode recv
        {
            get { return rv; }
        }
        internal override uint mid
        {
            get { return id; }
        }
        internal override RMetaObject orig
        {
            get { return or; }
        }
        RMetaObject or;
        RNode rv;
        uint id;
    }
*/

/*
    internal partial class RNCFunc : RNode
    {
        internal RNCFunc(MethodInfo mi, NetRuby rb)
            : base(rb.GetCurrentContext())
        {
            ruby = rb;
            minf = mi;
            ct = 0;
        }
        internal override int cnt
        {
            get { return ct; }
            set { ct = value; }
        }
        internal override MethodInfo cfunc
        {
            get { return minf; }
        }
        int ct;
        MethodInfo minf;
        NetRuby ruby;
        
        internal override object Funcall(RMetaObject klass, object recver, uint id, object[] args)
        {
            MethodInfo mi = cfunc;
            object ret = null;
            if (ruby.TraceFunc != null)
            {
                ret = Call(mi, recver, args);
            }
            else
            {
                ret = Call(mi, recver, args);
            }
            return ret;
        }

        private object Call(MethodInfo mi, object recver, object[] args)
        {
#if INVOKE_DEBUG
            if (mi == null)
                System.Console.WriteLine("no methodinfo");
            if (recver == null)
                System.Console.WriteLine("no recver");
            if (args == null)
                System.Console.WriteLine("no args");
#endif
            object[] argv = args;
            ParameterInfo[] ps = mi.GetParameters();
            if (ps.Length == 0)
            {
                argv = null;
            }
            else if (ps.Length > 0)
            {
                int last = ps.Length - 1;
                Type tp = ps[last].ParameterType;
                if (tp.IsArray)
                {
                    argv = new object[ps.Length];
                    int i = 0;
                    int id = 0;
                    if (ps[0].ParameterType == typeof(NetRuby))
                    {
                        argv[0] = ruby;
                        id++;
                    }
                    for (; id < last; id++)
                    {
                        if (i < args.Length)
                            argv[id] = args[i++];
                        else
                            argv[id] = null;
                    }
                    int cnt = (args.Length <= i) ? 0 : args.Length - i;
                    object[] rest = new object[cnt];
                    argv[last] = rest;
                    for (id = 0; i < cnt; i++)
                    {
#if INVOKE_DEBUG
                        System.Console.WriteLine("rest" + id.ToString() + "=" + ((args[i] == null) ? "null" : args[i].ToString()));
#endif
                        rest[id++] = args[i];
                    }
                }
                else if (ps[0].ParameterType == typeof(NetRuby))
                {
                    argv = new object[args.Length + 1];
                    argv[0] = ruby;
                    if (args.Length > 0)
                    {
                        Array.Copy(args, 0, argv, 1, args.Length);
                    }
                }
            }
            object ret = null;
            try
            {
                if (mi.IsStatic)
                {
#if INVOKE_DEBUG
                    System.Console.WriteLine("Invoke(static:"+mi.Name+") by " + recver.ToString());
                    if (argv != null)
                    {
                        for (int i = 0; i < argv.Length; i++)
                        {
                            System.Console.WriteLine("param(" + i.ToString() + ")=" + ((argv[i] == null) ? "null" : argv[i].ToString()));
                        }
                    }
#endif
                    ret = mi.Invoke(null, argv);
#if INVOKE_DEBUG
                    System.Console.WriteLine("invoked(static:"+mi.Name+")=" + ((ret==null)?"null":ret.ToString()));
#endif
                }
                else
                {
                    recver = ruby.InstanceOf(recver);
#if INVOKE_DEBUG
                    System.Console.WriteLine("Invoke("+mi.Name+") by " + recver.ToString());
                    if (argv != null)
                    {
                        for (int i = 0; i < argv.Length; i++)
                        {
                            System.Console.WriteLine("param(" + i.ToString() + ")=" + ((argv[i] == null) ? "null" : argv[i].ToString()));
                        }
                    }
#endif        
                    ret = mi.Invoke(recver, argv);
#if INVOKE_DEBUG
                    System.Console.WriteLine("invoked("+mi.Name+")=" + ((ret==null)?"null":ret.ToString()));
#endif
                }
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is eTagJump == false)
                {
                    ruby.warn(e.InnerException.Message);
#if _DEBUG
                    ruby.warn(e.InnerException.StackTrace);
#endif
                }
                throw e.InnerException;
            }
            catch (Exception e)
            {
#if _DEBUG
                ruby.warn(e.StackTrace);
#endif
                string msg = String.Format("Exception for {0} --- {1}", mi.Name, e.Message);
                ruby.ruby_raise(ruby.InstanceOf(recver), ruby.eRuntimeError, msg);
            }
            return ret;
        }
    }
*/
/*
    internal partial class RNIFunc : RNode
    {
        internal RNIFunc(RThread p, NetRuby.BlockProc blk, object dat)
            : base(p)
        {
            blockProc = blk;
            data = dat;
        }
        NetRuby.BlockProc blockProc;
        object data;
        internal object Call(object val, object self)
        {
            return blockProc(val, data, self);
        }
    }
*/
/*
    internal partial class RNRFunc : RNode
    {
        internal RNRFunc(NetRuby rb, RBasic.RMethod mtd, int ac)
            : base(rb.GetCurrentContext())
        {
            ruby = rb;
            method = mtd;
            argcnt = ac;
        }

        event RBasic.RMethod method;
        NetRuby ruby;
        int argcnt;

        internal override int argc
        {
            get { return argcnt; }
        }
        internal override object Funcall(RMetaObject klass, object recver, uint id, object[] args)
        {
            int ln = (args == null) ? 0 : args.Length;
            if (argcnt >= 0 && argcnt != ln)
            {
                throw new ArgumentException(
                    String.Format("wrong # of arguments({0} for {1})", ln, argcnt));
            }
            object ret = method(ruby.InstanceOf(recver), args);
            return ret;
        }
    }
*/
    
    internal partial class RNBlock : RNode
    {
        internal RNBlock(RThread th, RNode n)
           : base(th)
        {
            FixPos(n);
            bg = n;
            ed = this;
            nxt = null;
        }

        RNode bg;
        RNode ed;
        RNode nxt;
        
        internal override RNode head
        {
            get { return bg; }
            set { bg = value; }
        }
        internal override RNode beg
        {
            get { return bg; }
            set { bg = value; }
        }
        internal override RNode end
        {
            get { return ed; }
            set { ed = value; }
        }
        internal override RNode next
        {
            get { return nxt; }
            set { nxt = value; }
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            RNode node = this;
            while (node.next != null)
            {
                ruby.Eval(self, node.head);
                node = node.next;
            }
            result = null;
            return node.head;
        }
*/
    }

    internal partial class RNNewLine : RNode
    {
        internal RNNewLine(RThread p, RNode n) :
            base(p)
        {
            FixPos(n);
            nt = n.Line;
            nd = n;
        }
        int nt;
        RNode nd;
        internal override int nth
        {
            get { return nt; }
            set { nt = value; }
        }
        internal override RNode next
        {
            get { return nd; }
            set { nd = value; }
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            RThread th = ruby.GetCurrentContext();
            th.file = File;
            th.line = nt;
            if (ruby.TraceFunc != null)
            {
                System.Console.WriteLine("Trace");
            }
            result = null;
            return next;
        }
*/        
    }

    internal partial class RNYield : RNode
    {
        internal RNYield(RThread p, RNode a) :
            base(p)
        {
            st = a;
        }
        
        internal RNYield(RThread p) :
            base(p)
        {
            st = null;
        }
        internal override RNode stts
        {
            get { return st; }
        }
        protected RNode st;
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            object val = null;
            if (st != null)
            {
                val = ruby.Eval(self, st);
                if (st is RNRestArgs && ((RArray)val).Count == 1)
                {
                    val = ((RArray)val)[0];
                }
            }
            result = ruby.Yield(val, null, null, false);
            return null;
        }
*/        
    }

    internal partial class RNReturn : RNYield
    {
        internal RNReturn(RThread p, RNode n) :
            base(p, n)
        {
        }
        internal RNReturn(RThread p) :
            base(p)
        {
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            if (st != null)
            {
                result = ruby.Eval(self, st);
                ruby.SetReturnValue(result);
            }
            else
            {
                result = null;
                ruby.SetReturnValue(null);
            }
            ruby.ReturnCheck();
            throw new eTagJump(Tag.TAG.RETURN);
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            if (ruby.IsBlockGiven)
                return "yield";
            return null;
        }
*/        
    }

    internal partial class RNBreak : RNode
    {
        internal RNBreak(RThread p) :
            base(p)
        {
        }

/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            throw new eTagJump(Tag.TAG.BREAK);
        }
*/        
    }

    internal partial class RNNext : RNode
    {
        internal RNNext(RThread p) :
            base(p)
        {
        }

/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            throw new eTagJump(Tag.TAG.NEXT);
        }
*/        
    }

    internal partial class RNRedo : RNode
    {
        internal RNRedo(RThread p) :
            base(p)
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            throw new eTagJump(Tag.TAG.REDO);
        }
*/        
    }

    internal partial class RNRetry : RNode
    {
        internal RNRetry(RThread p) :
            base(p)
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            throw new eTagJump(Tag.TAG.RETRY);
        }
*/        
    }

    internal partial class RNBegin : RNode
    {
        internal RNBegin(RThread p, RNode b)
            : base(p)
        {
            bdy = b;
        }
        protected RNode bdy;

        internal override RNode body
        {
            get { return bdy; }
            set { bdy = value; }
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = null;
            return body;
        }
*/        
    }

    internal partial class RNEnsure : RNode
    {
        internal RNEnsure(RThread p, RNode b, RNode en)
            : base(p)
        {
            ens = en;
            bdy = b;
        }
        internal override RNode ensr
        {
            get { return ens; }
        }
        RNode ens;
        RNode bdy;
    }


    internal partial class RNRescue : RNEnsure
    {
        internal RNRescue(RThread p, RNode b, RNode r, RNode e)
            : base(p, b, e)
        {
            res = r;
        }
        internal override RNode resq
        {
            get { return res; }
        }
        RNode res;
    }

    internal partial class RNRestArgs : RNode
    {
        internal RNRestArgs(RThread p, RNode a) :
            base(p)
        {
            hd = a;
        }
        internal override RNode head
        {
            get { return hd; }
            set { hd = value; }
        }
        RNode hd;
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = ruby.Eval(self, hd);
            if (result is RArray == false)
            {
                RArray a = new RArray(ruby, true);
                a.Add(result);
                result = a;
            }
            return null;
        }
*/        
    }

    internal partial class RNBlockArg : RNode
    {
        internal RNBlockArg(RThread p, uint v, int c) :
            base(p)
        {
            vi = v;
            ct = c;
        }
        internal override int cnt
        {
            get { return ct; }
            set { ct = value; }
        }
        internal override uint vid
        {
            get { return vi; }
        }
        int ct;
        uint vi;
    }

    internal partial class RNBlockPass : RNode
    {
        internal RNBlockPass(RThread p, RNode b) :
            base(p)
        {
            bdy = b;
        }
        internal override RNode head
        {
            get { return hd; }
            // Bugfix
            set { hd = value; }
        }
        internal override RNode body
        {
            get { return bdy; }
        }
        // Bugfix
        internal override RNode iter
        {
            get { return itr; }
            set { itr = value; }
        }

        RNode bdy;
        RNode hd;
        RNode itr;
    }
    
    internal partial class RNIf : RNode
    {
        internal RNIf(RThread p, RNode c, RNode t, RNode e)
            : base(p)
        {
            FixPos(c);
            cd = c;
            bdy = t;
            el = e;
        }
        internal RNIf(RThread p, RNode c, RNode t)
            : base(p)
        {
            FixPos(c);
            cd = c;
            bdy = t;
            el = null;
        }
        protected RNode cd;
        protected RNode bdy;
        protected RNode el;
        internal override RNode cond
        {
            get { return cd; }
        }
        internal override RNode body
        {
            get { return bdy; }
        }
        internal override RNode nd_else
        {
            get { return el; }
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            ruby.SourceLine = Line;
            result = null;
            object o = ruby.Eval(self, cond);
            if (RTest(o))
            {
                return bdy;
            }
            else
            {
                return el;
            }
        }
*/        
    }

    internal partial class RNPreExe : RNScope
    {
        internal RNPreExe(RThread p, RNode b) :
            base(p, b)
        {
        }
    }
    
    internal partial class RNPostExe : RNode
    {
        internal RNPostExe(RThread p) :
            base(p)
        {
            onece = false;
        }
        bool onece;
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = null;
            if (onece) return null;
            onece = true;
            ruby.End();
            return null;
        }
*/        
    }
    
    internal partial class RNDot2 : RNode
    {
        internal RNDot2(RThread p, RNode b, RNode e) :
            base(p)
        {
            bg = b;
            ed = e;
        }
        protected RNode bg;
        protected RNode ed;
        
        internal override RNode beg
        {
            get { return bg; }
            set { bg = value; }
        }
        internal override RNode end
        {
            get { return ed; }
            set { ed = value; }
        }
    }
    
    internal partial class RNDot3 : RNDot2
    {
        internal RNDot3(RThread p, RNode b, RNode e) :
            base(p, b, e)
        {
        }
    }
    
    internal partial class RNUnless : RNIf
    {
        internal RNUnless(RThread p, RNode c, RNode t, RNode e)
            : base(p, c, e, t)
        {
        }
        internal RNUnless(RThread p, RNode c, RNode t)
            : base(p, c, null, t)
        {
        }

    }

    internal partial class RNAlias : RNode
    {
        internal RNAlias(RThread p, uint n, uint o)
            : base(p)
        {
            od = o;
            nw = n;
        }
        internal override uint old
        {
            get { return od; }
        }
        internal override uint nd_new
        {
            get { return nw; }
        }
        internal override uint mid
        {
            get { return nw; }
        }
        protected uint nw;
        protected uint od;

/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = null;
            RThread th = ruby.GetCurrentContext();
            RMetaObject rc = th.rClass;
            if (rc == null)
            {
                throw new eTypeError("no class to make alias");
            }
            rc.DefineAlias(nw, od);
            ruby.Funcall(rc, "method_added", nw);
            return null;
        }
*/        
    }

    internal partial class RNVAlias : RNAlias
    {
        internal RNVAlias(RThread p, uint n, uint o)
            : base(p, o, n)
        {
        }
    }

    internal partial class RNUndef : RNode
    {
        internal RNUndef(RThread p, uint id) :
            base(p)
        {
            mi = id;
        }
        uint mi;
        internal override uint mid
        {
            get { return mi; }
        }
    }

    internal partial class RNClassBase : RNode
    {
        protected RNClassBase(RThread p, uint n, RNode b, RNode s) :
            base(p)
        {
            cn = n;
            sprcls = s;
            bdy = new RNScope(p, b);
        }
        protected RNode sprcls;
        protected uint cn;
        protected RNode bdy;
        internal override uint cname
        {
            get { return cn; }
        }
        internal override RNode body
        {
            get { return bdy; }
        }
        internal override RNode super
        {
            get { return sprcls; }
        }
    }

    internal partial class RNClass : RNClassBase
    {
        internal RNClass(RThread p, uint n, RNode b, RNode s) :
            base(p, n, b, s)
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            RThread th = ruby.GetCurrentContext();
            RMetaObject rc = th.rClass;
            if (rc == null)
            {
                throw new eTypeError("no outer class/module");
            }
            RClass spr;
            if (sprcls != null)
            {
                spr = Superclass(ruby, self);
            }
            else
            {
                spr = null;
            }

            if ((rc == ruby.cObject) && ruby.IsAutoloadDefined(cn))
            {
                ruby.AutoLoad(cn);
            }
            RBasic ks = null;
            RClass klass = null;
            if (rc.IsConstDefinedAt(cn))
            {
                ks = (RBasic)rc.ConstGet(cn);
            }
            bool ovrd = true;
            if (ks != null)
            {
                ovrd = false;
                if (ks is RClass == false)
                {
                    throw new eTypeError(ruby.id2name(cn) + " is not a class");
                }
                klass = (RClass)ks;
                if (spr != null)
                {
                    RBasic tmp = klass.super.ClassReal;
                    if (tmp != spr)
                    {
                        ovrd = true;
                    }
                }
                if (ovrd == false && th.safeLevel >= 4)
                {
                    throw new SecurityException("extending class prohibited");
                }
                // delete ClearCache by dev:15723
            }
            if (ovrd)
            {
                string name = ruby.id2name(cn);
                klass = RClass.ClassNew(ruby, spr, null);
                klass.SetClassPath(rc, name);
                rc.ConstSet(cn, klass);
            }
            if (th.wrapper != null)
            {
                RMetaObject.ExtendObject(klass, th.wrapper);
                RMetaObject.IncludeModule(klass, th.wrapper);
            }
            result = ruby.ModuleSetup(klass, bdy);
            return null;
        }

        private RClass Superclass(NetRuby ruby, object self)
        {
            object o = null;
            try
            {
                o = ruby.Eval(self, sprcls);
            }
            catch (Exception)
            {
                o = null;
            }
            if (o == null || (o is RClass == false))
            {
                if (sprcls is RNColon2)
                    throw new eTypeError("undefined superclass `" + ruby.id2name(sprcls.mid) + "'");
                if (sprcls is RNConst)
                    throw new eTypeError("undefined superclass `" + ruby.id2name(sprcls.vid) + "'");
                throw new eTypeError("superclass undefined");
            }
            RClass spr = (RClass)o;
            if (spr.IsSingleton)
            {
                throw new eTypeError("can't make subclass of virtual class");
            }
            return spr;
        }
*/        
    }
    
    internal partial class RNSClass : RNClassBase
    {
        internal RNSClass(RThread p, RNode r, RNode b) :
            base(p, 0, b, null)
        {
            FixPos(r);
            rv = r;
        }
        RNode rv;
        internal override RNode recv
        {
            get { return rv; }
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            RMetaObject klass;
            result = ruby.Eval(self, rv);
            RThread th = ruby.GetCurrentContext();
            if (result is bool)
            {
                if ((bool)result)
                    klass = ruby.cTrueClass;
                else
                    klass = ruby.cFalseClass;
            }
            else if (result == null)
            {
                klass = ruby.cNilClass;
            }
            else
            {
                if (RBasic.IsSpecialConstType(result))
                    throw new eTypeError("no virtual class for " + ruby.ClassOf(result).Name);
                RBasic bas = ruby.InstanceOf(result);
                if (th.safeLevel >= 4 && bas.IsTainted == false)
                    throw new SecurityException("Insecure: can't extend object");
                if (ruby.ClassOf(bas) is RSingletonClass)
                {
                    ; // why clear cache ?
                }
                klass = RBasic.SingletonClass(bas, ruby);
            }
            if (th.wrapper != null)
            {
                RMetaObject.ExtendObject(klass, th.wrapper);
                RMetaObject.IncludeModule(klass, th.wrapper);
            }
            result = ruby.ModuleSetup(klass, bdy);
            return null;
        }
*/        
    }
    
    internal partial class RNModule : RNClassBase
    {
        internal RNModule(RThread p, uint n, RNode b) :
            base(p, n, b, null)
        {
        }

/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            RThread th = ruby.GetCurrentContext();
            RMetaObject rClass = th.rClass;
            if (rClass == null)
            {
                throw new eTypeError("no outer class/module");
            }
            if ((rClass == ruby.cObject) && ruby.IsAutoloadDefined(cn))
            {
                ruby.AutoLoad(cn);
            }
            RMetaObject module = null;
            if (rClass.IsConstDefinedAt(cn))
            {
                module = (RMetaObject)rClass.ConstGet(cn);
            }
            string name = ruby.id2name(cn);
            if (module != null)
            {
                if (module is RModule == false)
                {
                    throw new eTypeError(name + " is not a module");
                }
                if (th.safeLevel >= 4)
                {
                    throw new SecurityException("extending module prohibited");
                }
            }
            else
            {
                module = new RModule(ruby, name);
                module.SetClassPath(rClass, name);
            }
            if (th.wrapper != null)
            {
                RMetaObject.ExtendObject(module, th.wrapper);
                RMetaObject.IncludeModule(module, th.wrapper);
            }
            result = ruby.ModuleSetup(module, bdy);
            return null;
        }
*/        
    }

    internal partial class RNColon3 : RNode
    {
        internal RNColon3(RThread p, uint i) :
            base(p)
        {
            mi = i;
        }
        protected uint mi;
        internal override uint mid
        {
            get { return mi; }
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = ruby.cObject.ConstGetAt(mi);
            return null;
        }
*/        
    }

    internal partial class RNColon2 : RNColon3
    {
        internal RNColon2(RThread p, RNode c, uint i) :
            base(p, i)
        {
            hd = c;
        }
        RNode hd;
        internal override RNode head
        {
            get { return hd; }
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            object o = ruby.Eval(self, hd);
            if (o is RMetaObject == false)
                result = ruby.Funcall(o, mi);
            else
                result = ((RMetaObject)o).ConstGet(mi);
            return null;
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            ruby.bug("not defined yet");
            return null;
        }
*/        
    }
    
/*    
    internal partial class RNCRef : RNode
    {
        internal RNCRef(RMetaObject c1, RNode c2) :
            base((RThread)null)
        {
            cls = c1;
            nxt = c2;
        }
        internal RNCRef(RMetaObject c1) :
            base((RThread)null)
        {
            cls = c1;
            nxt = null;
        }
        RMetaObject cls;
        RNode nxt;
        internal override RMetaObject clss
        {
            get { return cls; }
        }
        internal override RNode next
        {
            get { return nxt; }
            set { nxt = value; }
        }

        internal bool ConstDefined(NetRuby ruby, uint id, object self)
        {
            RNode cbase = this;
            while (cbase != null && cbase.next != null)
            {
                RMetaObject klass = cbase.clss;
                if (klass == null) return ruby.ClassOf(self).IsConstDefined(id);
                string s;
                if (klass.iv_tbl != null && klass.iv_tbl.lookup(id, out s))
                    return true;
                cbase = cbase.next;
            }
            return cls.IsConstDefined(id);
        }
    }
*/
    
    internal partial class RNWhile : RNode
    {
        internal RNWhile(RThread p, RNode c, RNode b, bool n)
            : base(p)
        {
            cd = c;
            bd = b;
            st = n;
        }
        bool st;
        RNode cd;
        RNode bd;
        internal override bool state
        {
            get { return st; }
            set { st = value; }
        }
        internal override RNode cond
        {
            get { return cd; }
        }
        internal override RNode body
        {
            get { return bd; }
        }

/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = ruby.ConditionalLoop(this, self, true);
            return null;
        }
*/        
    }

    internal partial class RNUntil : RNWhile
    {
        internal RNUntil(RThread p, RNode c, RNode b, bool n)
            : base(p, c, b, n)
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = ruby.ConditionalLoop(this, self, false);
            return null;
        }
*/        
    }

    internal partial class RNCase : RNode
    {
        internal RNCase(RThread p, RNode h, RNode b)
            : base(p)
        {
            hd = h;
            bd = b;
        }
        RNode hd;
        RNode bd;
        internal override RNode head
        {
            get { return hd; }
        }
        internal override RNode body
        {
            get { return bd; }
        }
    }

    internal partial class RNList : RNode
    {
        protected RNList(RThread p, RNode n1, RNode n2, RNode n3) :
            base(p)
        {
            hd = n1;
            bd = n2;
            nxt = n3;
        }
        protected RNode hd;
        protected RNode bd;
        protected RNode nxt;
        internal override RNode head
        {
            get { return hd; }
            set { hd = value; }
        }
        internal override RNode body
        {
            get { return bd; }
            set { bd = value; }
        }
        internal override RNode next
        {
            get { return nxt; }
            set { nxt = value; }
        }
    }        
    internal partial class RNWhen : RNList
    {
        internal RNWhen(RThread p, RNode c, RNode t, RNode e) :
            base(p, c, t, e)
        {
        }

        internal RNWhen(RThread p, RNode c) :
            base(p, c, null, null)
        {
        }
    }

    internal partial class RNFor : RNList
    {
        internal RNFor(RThread p, RNode v, RNode i, RNode b)
            : base(p, v, b, i)
        {
            if (v != null) FixPos(v);
        }
        internal override RNode var
        {
            get { return hd; }
        }
        internal override RNode iter
        {
            get { return nxt; }
            set { nxt = value; }
        }
    }

    internal partial class RNCall : RNode
    {
        internal RNCall(RThread p, RNode r, uint m, RNode a) :
            base(p)
        {
            rv = r;
            ag = a;
            mi = m;
        }
        protected uint mi;
        protected RNode rv;
        protected RNode ag;
        internal override RNode recv
        {
            get { return rv; }
        }
        internal override uint mid
        {
            get { return mi; }
        }
        internal override RNode args
        {
            get { return ag; }
            set { ag = value; }
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            object receiver;
            object[] argv = ruby.CallArgs(this, self, out receiver);
#if INVOKE_DEBUG
            System.Console.WriteLine("mid=" + ruby.id2name(mi) + ",receiver=" + ((receiver==null)?"null":(receiver is RBasic)?((RBasic)receiver).Inspect().ToString():receiver.ToString()) + ", klass=" + (ruby.ClassOf(receiver).Name));
#endif
            result = ruby.Call(ruby.ClassOf(receiver), receiver, mi, argv, 0);
            return null;
        }

        internal object[] SetupArgs(NetRuby ruby, object self, out object receiver)
        {
            receiver = ruby.Eval(self, recv);
            return SetupArgs(ruby, self, args);
        }

        internal override object Assign(NetRuby ruby, object self, object val, bool check)
        {
            System.Console.WriteLine(String.Format("Assign for {0} into {1} by {2}",
                                                   (val == null) ? "null" : val.ToString(),
                                                   (self == null) ? "null" : self.ToString(),
                                                   ToString()));
System.Console.WriteLine("NOT YET IMPLEMENTED!!");
            return null;
        }

        protected string CheckBound(NetRuby ruby, RMetaObject o, object self)
        {
            if (GetType().IsSubclassOf(typeof(RNCall)))
            {
                if (ruby.IsMethodBound(o, mi, false) == false)
                    return null;
            }
            else
            {
                NOEX noex;
                uint id = mi;
                if (o.GetMethodBody(ref id, out o, out noex) == null)
                    return null;
                if ((noex & NOEX.PRIVATE) != 0)
                    return null;
                if ((noex & NOEX.PROTECTED) != 0)
                {
                    if (ruby.InstanceOf(self).IsKindOf(o.ClassReal) == false)
                        return null;
                }
            }
            return ArgDefined(ruby, self, ag, "method");
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            if (IsDefined(ruby, self, rv) == null) return null;
            RThread th = ruby.GetCurrentContext();
            object o = null;
            th.PushTag(Tag.TAG.PROT_NONE);
            try
            {
                o = ruby.Eval(self, rv);
                return CheckBound(ruby, ruby.ClassOf(o), self);
            }
            catch
            {
                th.errInfo = null;
                return null;
            }
            finally
            {
                th.PopTag(false);
            }
        }
*/        
    }

    internal partial class RNFCall : RNCall
    {
        internal RNFCall(RThread p, uint m, RNode a) :
            base(p, null, m, a)
        {
        }

        internal RNFCall(RThread p, uint m) :
            base(p, null, m, null)
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            object[] args = ruby.CallArgs(this, self);
            result = ruby.Call(ruby.ClassOf(self), self, mi, args, 1);
            return null;
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            RMetaObject o = ruby.ClassOf(self);
            return CheckBound(ruby, o, self);
        }
*/        
    }

    internal partial class RNVCall : RNCall
    {
        internal RNVCall(RThread p, uint m) :
            base(p, null, m, null)
        {
        }

/*UNUSED
        internal RNVCall(RThread p) :
            base(p, null, 0, null)
        {
        }
*/

/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            result = ruby.Call(ruby.ClassOf(self), self, mid, new object[0], 2);
            return null;
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            RMetaObject o = ruby.ClassOf(self);
            return CheckBound(ruby, o, self);
        }
*/        
    }

    internal partial class RNSuper : RNode
    {
        internal RNSuper(RThread p, RNode a) :
            base(p)
        {
            ag = a;
        }
        protected RNode ag;
        internal override RNode args
        {
            get { return ag; }
        }
/*        
        protected override string IsDefined(NetRuby ruby, object self)
        {
            RThread th = ruby.GetCurrentContext();
            Frame frm = th.frame;
            if (frm.lastFunc == 0) return null;
            else if (frm.lastClass == null) return null;
            else if (ruby.IsMethodBound(frm.lastClass.super, frm.lastFunc, false))
            {
                if (this is RNZSuper == false)
                {
                    return ArgDefined(ruby, self, ag, "super");
                }
                return "super";
            }
            return null;
        }
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            RThread th = ruby.GetCurrentContext();
            Frame frm = th.frame;
            RMetaObject lc = frm.lastClass;
            if (lc == null)
                throw new eNameError(String.Format("superclass method `{0}' disabled",
                                                   ruby.id2name(frm.lastFunc)));
            object[] args = setupargs(ruby, self);
            ITER itr = (th.Iter != ITER.NOT) ? ITER.PRE : ITER.NOT;
            th.PushIter(itr);
            result = ruby.Call(lc.super, frm.self, frm.lastFunc, args, 3);
            th.PopIter();
            return null;
        }
        protected virtual object[] setupargs(NetRuby ruby, object self)
        {
            return ruby.CallArgs(this, self);
        }
*/        
    }

    internal partial class RNZSuper : RNSuper
    {
        internal RNZSuper(RThread p, NetRuby rb) :
            base(p, null)
        {
            ruby = rb;
        }
        NetRuby ruby;
/*        
        protected override object[] setupargs(NetRuby ruby, object self)
        {
            return ruby.GetCurrentContext().frame.args;
        }
        internal override object Funcall(RMetaObject klass, object recver, uint id, object[] args)
        {
            return ruby.Eval(recver, this);
        }
*/
    
    }
    internal partial class RNResBody : RNList
    {
        internal RNResBody(RThread p, RNode a, RNode ex, RNode n) :
            base(p, a, ex, n)
        {
        }
    }

    internal partial class RNIter : RNFor
    {
        internal RNIter(RThread p, RNode v, RNode i, RNode b) :
            base(p, v, i, b)
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = ruby.Iteration(this, self);
            return null;
        }
*/        
    }

    internal partial class RNArgs : RNode
    {
        /*
        internal RNArgs(RThread p, int f, RNode o, RNode r) :
            base(p)
        {
            op = o;
            rst = r;
            ct = f;
        }
        */
        
        internal RNArgs(RThread p, int f, RNode o, int r) :
            base(p)
        {
            op = o;
            rst = r;
            ct = f;
        }
        internal RNArgs(RThread p, int f, RNode o, uint r) :
            base(p)
        {
            op = o;
            rst = (int)r;
            ct = f;
        }
        int rst;
        int ct;
        RNode op;
        internal override int rest
        {
            get { return rst; }
        }
        internal override int cnt
        {
            get { return ct; }
        }
        internal override RNode opt
        {
            get { return op; }
        }
    }

    internal partial class RNArgsCat : RNList
    {
        internal RNArgsCat(RThread p, RNode a, RNode b) :
            base(p, a, b, null)
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            RArray ary = (RArray)ruby.Eval(self, hd);
            result = ary.Concat(ruby.Eval(self, bd));
            return null;
        }
*/        
    }
    
    internal partial class RNArgsPush : RNList
    {
        internal RNArgsPush(RThread p, RNode a, RNode b) :
            base(p, a, b, null)
        {
        }
    }

    internal partial class RNDefBase : RNode
    {
        protected RNDefBase(RThread ps, uint i, RNode a, RNode d) :
            base(ps)
        {
            def = new RNScope(ps, a, d);
            mi = i;
        }
        protected uint mi;
        protected RNScope def;
        internal override uint mid
        {
            get { return mi; }
        }
        internal override RNode defn
        {
            get { return def; }
        }
        protected RNode CopyNodeScope(NetRuby rb, RNode node, RNode rval)
        {
            RNode copy = new RNScope(rval, node.next);
            if (node.tbl != null)
            {
                copy.tbl = new uint[node.tbl.Length];
                Array.Copy(node.tbl, copy.tbl, node.tbl.Length);
            }
            return copy;
        }
    }
    
    internal partial class RNDefn : RNDefBase
    {
        internal RNDefn(RThread ps, uint i, RNode a, RNode d, NOEX p)
            : base(ps, i, a, d)
        {
            ex = p;
        }
        NOEX ex;
        internal override NOEX noex
        {
            get { return ex; }
            set { ex = value; }
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            RThread th = ruby.GetCurrentContext();
            RMetaObject rClass = th.rClass;
            if (def != null)
            {
                if (rClass == null)
                {
                    throw new eTypeError("no class to add method");
                }
                if (rClass == ruby.cObject && mi == ruby.intern("initialize"))
                {
                    ruby.warn("redefining Object#initialize may cause infinite loop");
                }
                if (mi == ruby.intern("__id__") || mi == ruby.intern("__send__"))
                {
                    ruby.warn("redefining `{0}' may cause serious problem", ruby.id2name(mi));
                }

                rClass.FrozenClassCheck();

                RMetaObject origin;
                RNode bdy = rClass.SearchMethod(mi, out origin);
                if (bdy != null)
                {
                    if (ruby.verbose && rClass == origin && bdy.cnt == 0)
                    {
                        ruby.warning("discarding old " + ruby.id2name(mi));
                    }
                    if (ex != NOEX.PUBLIC)
                    {
                        ruby.warning("overriding global function `" + ruby.id2name(mid) + "'");
                    }
                }
                NOEX nx = NOEX.PUBLIC;
                if (th.ScopeTest(Scope.ScopeMode.Private) || mi == ruby.intern("init")) {
                    nx = NOEX.PRIVATE;
                }
                else if (th.ScopeTest(Scope.ScopeMode.Protected))
                {
                    nx = NOEX.PROTECTED;
                }
                else if (rClass == ruby.cObject)
                {
                    nx =  ex;
                }

                if (bdy != null && origin == rClass && (bdy.noex & NOEX.UNDEF) != 0)
                {
                    nx |= NOEX.UNDEF;
                }

                RNode dfn = CopyNodeScope(ruby, def, th.cRef);
                ruby.ClearCache(mi);
                rClass.addMethod(mi, dfn, nx);

                if (th.ScopeMode == Scope.ScopeMode.ModFunc)
                {
                    RMetaObject.SingletonClass(rClass, ruby).addMethod(mi, dfn, NOEX.PUBLIC);
                    ruby.Funcall(rClass, "singleton_method_added",
                                 new object[1] { Symbol.ID2SYM(mi) });
                }
                if (rClass.IsSingleton)
                {
                    ruby.Funcall(rClass.IVarGet("__attached__"), "singleton_method_added",
                                 new object[1] { Symbol.ID2SYM(mi) });
                }
                else
                {
                    ruby.Funcall(rClass, "method_added",
                                 new object[1] { Symbol.ID2SYM(mi) });
                }
            }
            result = null;
            return null;
        }
*/        
    }

    internal partial class RNDefs : RNDefBase
    {
        internal RNDefs(RThread p, RNode r, uint i, RNode a, RNode d)
            : base(p, i, a, d)
        {
            rv = r;
        }
        RNode rv;
        internal override RNode recv
        {
            get { return rv; }
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
            result = null;
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            if (def != null)
            {
                RThread th = ruby.GetCurrentContext();
                object receiver = ruby.Eval(self, rv);
                RBasic bas = ruby.InstanceOf(receiver);
                if (th.safeLevel >= 4 && bas.IsTainted == false)
                {
                    throw new SecurityException("Insecure: can't define singleton method");
                }
                if (bas is Symbol || bas is RFixnum)
                {
                    throw new eTypeError(String.Format("can't define singleton method \"{0}\" for {1}",
                                                       ruby.id2name(mi),
                                                       ruby.ClassOf(bas).Name));
                }
                if (bas.IsFrozen) ruby.ErrorFrozen("object");
                RClass klass = RBasic.SingletonClass(bas, ruby);
                RNode body = null;
                object o;
                if (klass.m_tbl.lookup(mi, out o))
                {
                    if (th.safeLevel >= 4)
                    {
                        throw new SecurityException("redefining method prohibited");
                    }
                    ruby.warning("redefine {0}", ruby.id2name(mi));
                    body = (RNode)o;
                }
                RNode dfn = CopyNodeScope(ruby, def, th.cRef);
                dfn.rval = th.cRef;
                klass.addMethod(mi, dfn,
                         (body != null) ? (body.noex & NOEX.UNDEF) : NOEX.PUBLIC);
                ruby.Funcall(bas, "singleton_method_added", Symbol.ID2SYM(mi));
            }
            return null;
        }
*/        
    }

    internal partial class RNScope : RNode
    {
        internal RNScope(RThread p, RNode b1, RNode b2)
            : base(p)
        {
            tb = p.Locals;
            bdy = null;
            nxt = block_append(p, b1, b2);
        }
        
        internal RNScope(RThread p, RNode b)
            : base(p)
        {
            tb = p.Locals;
            bdy = null;
            nxt = b;
        }
        internal RNScope(RNode b1, RNode b2)
            : base((RThread)null)
        {
            tb = null;
            bdy = b1;
            nxt = b2;
        }
        internal override RNode next
        {
            get { return nxt; }
        }
        internal override uint[] tbl
        {
            get { return tb; }
            set { tb = value; }
        }
        internal override RNode body
        {
            get { return bdy; }
        }
        internal override RNode rval
        {
            get { return bdy; }
            set { bdy = value; }
        }
        RNode bdy;
        uint[] tb;
        RNode nxt;
/*        
        internal override object Funcall(RMetaObject klass, object recver, uint id, object[] args)
        {
            NetRuby ruby = klass.ruby;
            return ruby.ScopedFuncall(klass, recver, id, args, this);
        }
*/        
    }
        
    internal partial class RNSelf : RNode
    {
        internal RNSelf(RThread p)
            : base(p)
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = self;
            return null;
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            return "self";
        }
*/        
    }

    internal partial class RNNil : RNode
    {
        internal RNNil(RThread p)
            : base(p)
        {
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            result = null;
            return null;
        }
        
        protected override string IsDefined(NetRuby ruby, object self)
        {
            return "nil";
        }
*/
    }

    internal partial class RNTrue : RNode
    {
        internal RNTrue(RThread p)
            : base(p)
        {
        }

/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            result = true;
            return null;
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            return "true";
        }
*/            
    }
    
    internal partial class RNFalse : RNode
    {
        internal RNFalse(RThread p)
            : base(p)
        {
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            result = false;
            return null;
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            return "false";
        }
*/        
    }

    internal partial class RNDefined : RNode
    {
        internal RNDefined(RThread p, RNode e) :
            base(p)
        {
            hd = e;
        }
        internal override RNode head
        {
            get { return hd; }
        }
        RNode hd;
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            string desc = IsDefined(ruby, self, hd);
            result = (desc != null) ? desc : null;
            return null;
        }
*/        
    }

    internal partial class RNHash : RNode
    {
        internal RNHash(RThread p, RNode a) :
            base(p)
        {
            hd = a;
        }
        RNode hd;
        internal override RNode head
        {
            get { return hd; }
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            RHash h = new RHash(ruby);
            RNode list = hd;
            while (list != null)
            {
                object key = ruby.Eval(self, list.head);
                list = list.next;
                if (list == null)
                    ruby.bug("odd number list for Hash");
                object val = ruby.Eval(self, list.head);
                list = list.next;
                h[key] = val;
            }
            result = h;
            return null;
        }
*/        
    }

    internal partial class RNVarBase : RNode
    {
        protected RNVarBase(RThread p, uint v) :
            base(p)
        {
            vi = v;
        }
        protected uint vi;
        internal override uint vid
        {
            get { return vi; }
        }
    }
    internal partial class RNAsgnBase : RNVarBase
    {
        protected RNAsgnBase(RThread p, uint v, RNode vlu) :
            base(p, v)
        {
            vl = vlu;
        }
        protected RNode vl;
        internal override RNode val
        {
            get { return vl; }
            set { vl = value; }
        }
/*        
        protected override string IsDefined(NetRuby ruby, object self)
        {
            return "assignment";
        }
*/        
    }
    
    internal partial class RNGAsgn : RNAsgnBase
    {
        internal RNGAsgn(RThread p, NetRuby rb, uint v, RNode vl)
            : base(p, v, vl)
        {
            ent = rb.Global(v);
        }
        protected RNGAsgn(uint v, GlobalEntry e)
            : base(null, v, null)
        {
            ent = e;
        }
        internal override GlobalEntry entry
        {
            get { return ent; }
        }
        protected GlobalEntry ent;
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            result = ruby.Eval(self, val);
            ruby.GVarSet(ent, result);
            return null;
        }
*/        
    }

    internal partial class RNLAsgn : RNAsgnBase
    {
        internal RNLAsgn(RThread p, uint v, RNode vl)
            : base(p, v, vl)
        {
            ct = p.LocalCnt(v);
        }
        protected int ct;
        internal override int cnt
        {
            get { return ct; }
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            RThread th = ruby.GetCurrentContext();
            if (th.scope.local_vars == null)
            {
                ruby.bug("unexpected local variable assignement");
            }
            result = ruby.Eval(self, vl);
#if EVAL_TRACE
            System.Console.WriteLine("LAsgn:cnt=" + cnt.ToString() + ", value=" +
                                     ((val == null)?"null":val.ToString() + ", result=" +
                                     ((result==null)?"null":result.ToString())));
#endif
            th.scope.local_vars[ct] = result;
            return null;
        }

        internal override object Assign(NetRuby ruby, object self, object val, bool check)
        {
#if ASSIGN_DEBUG
            System.Console.WriteLine(String.Format("Assign for {0} into {1} by {2}",
                                                   (val == null) ? "null" : val.ToString(),
                                                   (self == null) ? "null" : self.ToString(),
                                                   ToString()));
#endif
            RThread th = ruby.GetCurrentContext();
            if (th.scope.local_vars == null)
            {
                ruby.bug("unexpected local variable assignment");
            }
            return th.scope.local_vars[ct] = val;
        }
*/        
    }

    internal partial class RNDAsgn : RNAsgnBase
    {
        internal RNDAsgn(RThread p, uint v, RNode vl)
            : base(p, v, vl)
        {
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            result = ruby.Eval(self, vl);
            ruby.GetCurrentContext().DVarAsgn(vi, result);
            return null;
        }

        internal override object Assign(NetRuby ruby, object self, object val, bool check)
        {
#if ASSIGN_DEBUG
            System.Console.WriteLine(String.Format("Assign for {0} into {1} by {2}",
                                                   (val == null) ? "null" : val.ToString(),
                                                   (self == null) ? "null" : self.ToString(),
                                                   ToString()));
#endif
            ruby.GetCurrentContext().DVarAsgn(vi, val);
            return val;
        }
*/        
    }

    internal partial class RNDAsgnCurr : RNAsgnBase
    {
        internal RNDAsgnCurr(RThread p, uint v, RNode vl)
            : base(p, v, vl)
        {
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = ruby.Eval(self, vl);
            ruby.GetCurrentContext().DVarAsgnCurr(vi, result);
            return null;
        }
        
        internal override object Assign(NetRuby ruby, object self, object val, bool check)
        {
#if ASSIGN_DEBUG
            System.Console.WriteLine(String.Format("Assign for {0} into {1} by {2}",
                                                   (val == null) ? "null" : val.ToString(),
                                                   (self == null) ? "null" : self.ToString(),
                                                   ToString()));
#endif
            ruby.GetCurrentContext().DVarAsgnCurr(vi, val);
            return val;
        }
*/        
    }

    internal partial class RNIAsgn : RNAsgnBase
    {
        internal RNIAsgn(RThread p, uint v, RNode vl)
            : base(p, v, vl)
        {
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            result = ruby.Eval(self, vl);
            RBasic o = (RBasic)self;
            o.IVarSet(vi, result);
            return null;
        }

        internal override object Assign(NetRuby ruby, object self, object val, bool check)
        {
#if ASSIGN_DEBUG
            System.Console.WriteLine(String.Format("Assign for {0} into {1} by {2}",
                                                   (val == null) ? "null" : val.ToString(),
                                                   (self == null) ? "null" : self.ToString(),
                                                   ToString()));
#endif
            RBasic o = (RBasic)self;
            o.IVarSet(vid, val);
            return val;
        }
*/        
    }

    internal partial class RNCDecl : RNAsgnBase
    {
        internal RNCDecl(RThread p, uint v, RNode vl)
            : base(p, v, vl)
        {
        }
/*
        internal override object Assign(NetRuby ruby, object self, object val, bool check)
        {
#if ASSIGN_DEBUG
            System.Console.WriteLine(String.Format("Assign for {0} into {1} by {2}",
                                                   (val == null) ? "null" : val.ToString(),
                                                   (self == null) ? "null" : self.ToString(),
                                                   ToString()));
#endif
            ruby.GetCurrentContext().rClass.ConstSet(vi, val);
            return val;
        }
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            RThread th = ruby.GetCurrentContext();
            if (th.rClass == null)
                throw new eTypeError("no class/module to define constant");
            result = ruby.Eval(self, vl);
            th.rClass.ConstSet(vi, result);
            return null;
        }
*/        
    }

    internal partial class RNCVAsgn : RNAsgnBase // class variable
    {
        internal RNCVAsgn(RThread p, uint v, RNode vl)
            : base(p, v, vl)
        {
        }
/*
        internal override object Assign(NetRuby ruby, object self, object val, bool check)
        {
#if ASSIGN_DEBUG
            System.Console.WriteLine(String.Format("Assign for {0} into {1} by {2}",
                                                   (val == null) ? "null" : val.ToString(),
                                                   (self == null) ? "null" : self.ToString(),
                                                   ToString()));
#endif
            ruby.CVarSingleton(self).CVarSet(vi, val);
            return val;
        }
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = ruby.Eval(self, vl);
            ruby.GetCurrentContext().cBase.CVarSet(vi, result);
            return null;
        }
*/        
    }

    internal partial class RNCVDecl : RNAsgnBase
    {
        internal RNCVDecl(RThread p, uint v, RNode vl)
            : base(p, v, vl)
        {
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            RThread th = ruby.GetCurrentContext();
            RMetaObject cbase = th.cBase;
            if (cbase == null)
            {
                throw new eTypeError("no class/module to define class variable");
            }
            result = ruby.Eval(self, vl);
            if (cbase.IsSingleton)
            {
                ruby.CVarSingleton(cbase.IVarGet("__attached__")).CVarDeclare(vi, result);
            }
            else
            {
                cbase.CVarDeclare(vi, result);
            }
            return null;
        }

        internal override object Assign(NetRuby ruby, object self, object val, bool check)
        {
#if ASSIGN_DEBUG
            System.Console.WriteLine(String.Format("Assign for {0} into {1} by {2}",
                                                   (val == null) ? "null" : val.ToString(),
                                                   (self == null) ? "null" : self.ToString(),
                                                   ToString()));
#endif
            RMetaObject cbase = ruby.GetCurrentContext().cBase;
            if (cbase.IsSingleton == false)
            {
                cbase.CVarDeclare(vi, val);
            }
            else
            {
                self = cbase.IVarGet("__attached__");
                ruby.CVarSingleton(self).CVarSet(vi, val);
            }
            return val;
        }
*/        
    }

    internal partial class RNLit : RNode
    {
        internal RNLit(RThread p, object l) :
            base(p)
        {
            lt = l;
        }
        object lt;
        internal override object lit
        {
            get { return lt; }
            set { lt = value; }
        }

/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            result = lt;
            return null;
        }
*/        
    }

    internal partial class RNStr : RNode
    {
        internal RNStr(RThread p, NetRuby rb, string s) :
            base(p)
        {
            st = new RString(rb, s);
        }
        protected RString st;
        
        internal RNStr(RThread p, NetRuby rb, object o) :
            base(p)
        {
            st = new RString(rb, o.ToString());
        }

        internal RNStr(RNode n) :
            base(n)
        {
            if (n is RNStr)
                st = ((RNStr)n).st;
        }
        internal override object lit
        {
            get { return st; }
            // Bugfix
            set { st = (RString)value; }
        }
        internal override RNode next
        {
            get { return null; }
            // node.cs(3054) error CS0546: 'NETRuby.RNDXStr.next':
            // cannot override because 'NETRuby.RNStr.next'
            // does not have an overridable set accessor
            set { throw new NotSupportedException("bug(next set):" + GetType().ToString()); }
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            result = st;
            return null;
        }
        
        protected RNode ExpandStr(NetRuby ruby, object self, out object result)
        {
            RThread th = ruby.GetCurrentContext();
            RNode list = next;
            StringBuilder sb = new StringBuilder(st.ToString());
            bool tainted = false;
            while (list != null)
            {
                if (list.head != null)
                {
                    RNode n = list.head;
                    if (n is RNStr)
                    {
                        sb.Append(n.lit.ToString());
                        if (n.lit is RBasic && ((RBasic)n.lit).IsTainted)
                            tainted = true;
                    }
                    else
                    {
                        if (n is RNEvStr)
                        {
                            RException rex = th.errInfo;
                            th.errInfo = null;
                            th.line = n.Line;
                            th.EnterEval();

                            list.head = ruby.CompileFile(th.file,
                                      new StringReader(lit.ToString()), th.line, th);
        
                            th.LeaveEval();
                            if (th.CompileFailed)
                            {
                                th.CompileError("string expansion");
                            }
                            if (rex != null) th.errInfo = rex;
                        }
                        object o = ruby.Eval(self, list.head);
                        if (o is RBasic && ((RBasic)o).IsTainted) tainted = true;
                        sb.Append(RString.AsString(ruby, o));
                    }
                }
                list = list.next;
            }
            if (tainted)
                result = new RString(ruby, sb.ToString(), true);
            else
                result = sb.ToString();
            return null;
        }
*/        
    }
    
    internal partial class RNXStr : RNStr
    {
        internal RNXStr(RThread p, NetRuby rb, string s) :
            base(p, rb, s)
        {
        }
        internal RNXStr(RThread p, NetRuby rb, object o) :
            base(p, rb, o.ToString())
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = ruby.Funcall(self, '`', st);
            return null;
        }
*/        
    }

    internal partial class RNDXStr : RNStr
    {
        internal RNDXStr(RThread p, NetRuby rb, string s) :
            base(p, rb, s)
        {
            nxt = null;
            argcnt = 0;
        }
        internal RNDXStr(RNode n) :
            base(n)
        {
            nxt = null;
            argcnt = 0;
        }
        RNode nxt;
        internal override RNode next
        {
            get { return nxt; }
            set { nxt = value; }
        }
        int argcnt;
        internal override int alen
        {
            get { return argcnt; }
            set { argcnt = value; }
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            ExpandStr(ruby, self, out result);
            result = ruby.Funcall(self, '`', result);
            return null;
        }
*/        
    }

    internal partial class RNEvStr : RNStr
    {
        internal RNEvStr(RThread p, NetRuby rb, string s, int len) :
            base(p, rb, s)
        {
        }
    }

    internal partial class RNDRegx : RNStr
    {
        internal RNDRegx(RNode n) :
            base(n)
        {
            nxt = null;
            cflg = 0;
        }
        internal RNode nxt;
        internal override RNode next
        {
            get { return nxt; }
            set { nxt = value; }
        }
        uint cflg;
        internal override uint cflag
        {
            get { return cflg; }
            set { cflg = value; }
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            ExpandStr(ruby, self, out result);
            result = RRegexpClass.s_new(ruby.cRegexp, result.ToString(), (int)cflag);
            return null;
        }
*/        
    }
    
    internal partial class RNDRegxOnce : RNDRegx
    {
        internal RNDRegxOnce(RNode n) :
            base(n)
        {
            regexp = null;
        }
        internal override object lit
        {
            get { return regexp; }
        }
        RRegexp regexp;
/*                
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            if (regexp == null)
            {
                ExpandStr(ruby, self, out result);
                regexp = (RRegexp)RRegexpClass.s_new(ruby.cRegexp, result.ToString(), (int)cflag);
            }
            result = regexp;
            return null;
        }
*/        
    }

    internal partial class RNBlockNoArg : RNode
    {
        internal RNBlockNoArg(RThread th)
            : base(th)
        {
        }
    }
    
    internal partial class RNMAsgn : RNode
    {
        internal RNMAsgn(RThread p, RNode l, int r)
            : base(p)
        {
            lv = l;
            irv = r;
            rv = null;
        }
        internal RNMAsgn(RThread p, RNode l, RNode r)
            : base(p)
        {
            lv = l;
            irv = 1;
            rv = r;
        }
        internal RNMAsgn(RThread p, RNode l)
            : base(p)
        {
            lv = l;
        }
        RNode lv;
        RNode rv;
        int irv;
        RNode vl;
        internal override RNode args
        {
            get { return rv; }
            set { rv = value; }
        }
        internal override RNode head
        {
            get { return lv; }
        }
        // Bugfix
        internal override RNode val
        {
            get { return vl; }
            set { vl = value; }
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = Assign(ruby, self, ruby.Eval(self, null), false);
            return null;
        }

        internal override object Assign(NetRuby ruby, object self, object val, bool check)
        {
#if ASSIGN_DEBUG
            System.Console.WriteLine(String.Format("Assign for {0} into {1} by {2}",
                                                   (val == null) ? "null" : val.ToString(),
                                                   (self == null) ? "null" : self.ToString(),
                                                   ToString()));
#endif
            ArrayList ary = null;
            if (val is QUndef)
                ary = new ArrayList();
            else if (val is ArrayList)
                ary = (ArrayList)val;
            else if (val is RArray)
                ary = ((RArray)val).ArrayList;
            else if (val is object[])
            {
                ary = new ArrayList((object[])val);
            }
            else
            {
                uint toary = ruby.intern("to_ary");
                if (ruby.RespondTo(self, toary))
                {
                    object o = ruby.Funcall(val, toary, null);
                    if (o is RArray == false)
                    {
                        throw new eTypeError(ruby.ClassOf(o).ClassName
                                             + "#to_ary should retrun Array");
                    }
                    val = ((RArray)o).ArrayList;
                }
                else
                {
                    ary = new ArrayList();
                    ary.Add(val);
                }
            }
            RNode list = head;
            int i = 0;
            foreach (object x in ary) // maybe faster than indexer
            {
                ruby.Assign(self, list.head, x, check);
                list = list.next;
                i++;
                if (list == null) break;
            }
            if (check && list != null) goto arg_error;
            if (rv != null)
            {
                if (irv == -1)
                {
                    // no check for more `*'
                }
                else if (list == null && i < ary.Count)
                {
                    RArray a = new RArray(ruby, ary, true);
                    a.ArrayList.RemoveRange(0, i);
                    ruby.Assign(self, rv, a, check);
                }
                else
                {
                    ruby.Assign(self, rv, new RArray(ruby, true), check);
                }
            }
            else if (check && i < ary.Count)
            {
                goto arg_error;
            }

            while (list != null)
            {
                i++;
                ruby.Assign(self, list.head, null, check);
                list = list.next;
            }
            return new RArray(ruby, ary);

        arg_error:
            while (list != null)
            {
                i++;
                list = list.next;
            }
            throw new ArgumentException(String.Format("wrong # of arguments ({0} for {1})",
                                                      ary.Count, i));
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            return "assignment";
        }
*/        
    }

    internal partial class RNOpAsgn1 : RNode
    {
        internal RNOpAsgn1(RThread p, RNode pn, uint id, RNode a) :
            base(p)
        {
            rv = pn;
            mi = id;
            ags = a;
        }
        protected RNode rv;
        protected uint mi;
        protected RNode ags;
        internal override RNode recv
        {
            get { return rv; }
        }
        internal override uint mid
        {
            get { return mi; }
        }
        internal override RNode args
        {
            get { return ags; }
            set { ags = value; }
        }
/*        
        protected override string IsDefined(NetRuby ruby, object self)
        {
            return "assignment";
        }
*/        
    }

    internal partial class RNOpAsgn2 : RNode
    {
        internal RNOpAsgn2(RThread p, RNode r, uint id, uint o, RNode val) :
            base(p)
        {
            rv = r;
            vl = val;
            nxt = new RNOpAsgn22(p, id, o);
        }

        protected RNode rv;
        protected RNode vl;
        protected RNode nxt;
        internal override RNode recv
        {
            get { return rv; }
        }
        internal override RNode val
        {
            get { return vl; }
            set { vl = value; }
        }
        internal override RNode next
        {
            get { return nxt; }
        }
/*
        protected override string IsDefined(NetRuby ruby, object self)
        {
            return "assignment";
        }
*/        
    }

    internal partial class RNOpAsgn22 : RNode
    {
        internal RNOpAsgn22(RThread p, uint i, uint o) :
            base(p)
        {
            vi = i;
            mi = o;
            ai = Parser.id_attrset(i);
        }
        uint vi;
        uint mi;
        uint ai;
        internal override uint vid
        {
            get { return vi; }
        }
        internal override uint mid
        {
            get { return mi; }
        }
        internal override uint aid
        {
            get { return ai; }
        }
    }
    
    internal partial class RNOpAsgnOr : RNode
    {
        internal RNOpAsgnOr(RThread p, RNode i, RNode val) :
            base(p)
        {
            nd1 = i;
            nd2 = val;
            id = 0;
        }
        RNode nd1;
        RNode nd2;
        uint id;
        internal override uint aid
        {
            get { return id; }
        }
        internal override RNode head
        {
            // BUG?
            //get { return nd2; }
            // fixed?
            get { return nd1; }
        }
        internal override RNode val
        {
            get { return nd2; }
        }
    }
    
    internal partial class RNOpAsgnAnd : RNode
    {
        internal RNOpAsgnAnd(RThread p, RNode i, RNode v) :
            base(p)
        {
            nd1 = i;
            nd2 = v;
            id = 0;
        }
        RNode nd1;
        RNode nd2;
        uint id;
        internal override uint aid
        {
            get { return id; }
        }
        internal override RNode head
        {
             // BUG?
            //get { return nd2; }
            // fixed?
            get { return nd1; }
        }
        internal override RNode val
        {
            get { return nd2; }
        }
    }

    internal partial class RNGVar : RNGAsgn
    {
        internal RNGVar(RThread p, NetRuby rb, uint v) :
            base(p, rb, v, null)
        {
        }
        internal RNGVar(uint v, GlobalEntry ent) :
            base(v, ent)
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = ruby.GVarGet(ent);
            return null;
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            return "global-variable";
        }
*/        
    }
    
    internal partial class RNLVar : RNLAsgn
    {
        internal RNLVar(RThread p, uint v) :
            base(p, v, null)
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            RThread th = ruby.GetCurrentContext();
            if (th.scope.local_vars == null)
            {
                ruby.bug("unexpected local variable");
            }
#if LOCAL_TRACE_2
            System.Console.WriteLine("LVar: cnt=" + ct.ToString() + "/" + th.scope.local_vars.Length);
            for (int i = 0; i < th.scope.local_vars.Length; i++)
            {
                System.Console.WriteLine("local[" + i.ToString() + "]=" + ((th.scope.local_vars[i] == null) ? "null" : th.scope.local_vars[i].ToString()));
            }
#endif
            result = th.scope.local_vars[ct];
#if LOCAL_TRACE_2
            System.Console.WriteLine("LVar:" + ((result==null)?"null":result.GetType().ToString()));
            if (result is RBasic)
            {
                System.Console.WriteLine("LVar:" + ((RBasic)result).id.ToString());
            }
#endif
            return null;
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            return "local-variable";
        }
*/        
    }
    
    internal partial class RNDVar : RNVarBase
    {
        internal RNDVar(RThread p, uint v) :
            base(p, v)
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = ruby.GetCurrentContext().DVarRef(vi);
            return null;
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            return "local-variable(in-block)";
        }
*/        
    }

/*UNUSED
    internal partial class RNAttrSet : RNVarBase
    {
        internal RNAttrSet(RThread p, uint v) :
            base(p, v)
        {
        }
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            RThread th = ruby.GetCurrentContext();
            int argc = (th.frame.args == null) ? 0 : th.frame.args.Length;
            if (argc != 1)
                throw new eArgError(String.Format("wrong # of arguments({0} for 1)", argc));
            result = ruby.InstanceOf(self).IVarSet(vi, th.frame.args[0]);
            return null;
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            return "assignment";
        }
    }
*/        
    
    internal partial class RNIVar : RNVarBase
    {
        internal RNIVar(RThread p, uint v) :
            base(p, v)
        {
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = ((RBasic)self).IVarGet(vi);
            return null;
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            return "instance-variable";
        }
*/        
    }
    
    internal partial class RNConst : RNVarBase
    {
        internal RNConst(RThread p, uint v) :
            base(p, v)
        {
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif        
            RThread th = ruby.GetCurrentContext();
            RNode constBase = th.frame.cBase;
            while (constBase != null && constBase.next != null)
            {
                RMetaObject klass = constBase.clss;
                if (klass == null)
                {
                    result = (ruby.ClassOf(self)).ConstGet(vi);
                    return null;
                }
                if (klass.valGet(vi, out result)) return null;
                constBase = constBase.next;
            }
            result = th.frame.cBase.clss.ConstGet(vi);
            return null;
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            RThread th = ruby.GetCurrentContext();
            if (th.frame.cBase.ConstDefined(ruby, vi, self))
            {
                return "constant";
            }
            return null;
        }
*/        
    }

    internal partial class RNCVar : RNVarBase // class variable
    {
        internal RNCVar(RThread p, uint v) :
            base(p, v)
        {
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            RMetaObject cbase = ruby.GetCurrentContext().cBase;
            if (cbase == null)
            {
                result = ruby.ClassOf(self).CVarGet(vi);
            }
            else if (cbase.IsSingleton == false)
            {
                result = cbase.CVarGet(vi);
            }
            else
            {
                self = cbase.IVarGet("__attached__");
                result = ruby.CVarSingleton(self).CVarGet(vi);
            }
            return null;
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            RThread th = ruby.GetCurrentContext();
            ruby.bug("RNCvar::IsDefined not implemented");
            return null;
        }
*/        
    }
    
    internal partial class RNCVar2 : RNVarBase
    {
        internal RNCVar2(RThread p, uint v) :
            base(p, v)
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = ruby.CVarSingleton(self).CVarGet(vi);
            return null;
        }
*/        
    }

    internal partial class RNArray : RNode
    {
        internal RNArray(RThread p, RNode n)
            : base(p)
        {
            hd = n;
            nxt = null;
            len = 1;
        }
        internal override RNode head
        {
            get { return hd; }
        }
        internal override RNode next
        {
            get { return nxt; }
            set { nxt = value; }
        }
        internal override int alen
        {
            get { return len; }
            set { len = value; }
        }
        RNode hd;
        RNode nxt;
        int len;

        internal RNode append(RThread p, RNode tail)
        {
            RNode last = this;
            while (last.next != null) {
                last = last.next;
            }

            last.next = new RNArray(p, tail);
            len += 1;
            return this;
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            RArray ary = new RArray(ruby, len);
            for (RNode node = this; node != null; node = node.next)
            {
                ary.ArrayList.Add(ruby.Eval(self, node.head));
            }
            result = ary;
            return null;
        }
*/        
    }
    
    internal partial class RNZArray : RNArray
    {
        internal RNZArray(RThread p) :
            base(p, null)
        {
        }
        internal override RNode head
        {
            get { return null; }
        }
        internal override RNode next
        {
            get { return null; }
        }
        internal override int alen
        {
            get { return 0; }
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = new RArray(ruby, true);
            return null;
        }
*/        
    }

    internal partial class RNDStr : RNStr
    {
        internal RNDStr(RThread p, NetRuby rb, string s) :
            base(p, rb, s)
        {
            nxt = null;
            argcnt = 0;
        }
        internal RNDStr(RThread p, NetRuby rb, object o) :
            base(p, rb, o)
        {
            nxt = null;
            argcnt = 0;
        }
        RNode nxt;
        internal override RNode next
        {
            get { return nxt; }
            set { nxt = value; }
        }
        int argcnt;
        internal override int alen
        {
            get { return argcnt; }
            set { argcnt = value; }
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            return ExpandStr(ruby, self, out result);
        }
*/        
    }

    internal partial class RNEVStr : RNStr
    {
        internal RNEVStr(RThread p, NetRuby rb, string s) :
            base(p, rb, s)
        {
        }
    }
    
    internal partial class RNMatch2 : RNode
    {
        internal RNMatch2(RThread p, RNode n, RNode gv) :
            base(p)
        {
            rv = n;
            vl = gv;
        }
        protected RNode rv;
        protected RNode vl;
        internal override RNode recv
        {
            get { return recv; }
        }
        internal override RNode val
        {
            get { return vl; }
        }
/*        
        protected override string IsDefined(NetRuby ruby, object self)
        {
            return "method";
        }
*/        
    }

    internal partial class RNMatch3 : RNMatch2
    {
        internal RNMatch3(RThread p, RNode r, RNode n2) :
            base(p, r, n2)
        {
        }
    }

    internal partial class RNFlip2 : RNode
    {
        internal RNFlip2(RNode r) :
            base(r)
        {
        }
    }

    internal partial class RNFlip3 : RNode
    {
        internal RNFlip3(RNode r) :
            base(r)
        {
        }
    }

    internal partial class RNMatch : RNode
    {
        internal RNMatch(RThread p, object n) :
            base(p)
        {
            hd = new RNLit(p, n);
        }
        internal RNMatch(RThread p, RNode n) :
            base(p)
        {
            hd = n;
        }
        protected RNode hd;
        internal override RNode head
        {
            get { return hd; }
        }
    }

    internal partial class RNLogic : RNode
    {
        internal RNLogic(RThread p, RNode l, RNode r) :
            base(p)
        {
            nd1st = l;
            nd2nd = r;
        }
        internal override RNode nd_1st
        {
            get { return nd1st; }
        }
        internal override RNode nd_2nd
        {
            get { return nd2nd; }
        }
        protected RNode nd1st;
        protected RNode nd2nd;
    }
    internal partial class RNAnd : RNLogic
    {
        internal RNAnd(RThread p, RNode l, RNode r) :
            base(p, l, r)
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = ruby.Eval(self, nd_1st);
            if (RTest(result) == false) return null;
            return nd_2nd;
        }
*/        
    }

    internal partial class RNOr : RNLogic
    {
        internal RNOr(RThread p, RNode l, RNode r) :
            base(p, l, r)
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            result = ruby.Eval(self, nd_1st);
            if (RTest(result)) return null;
            return nd_2nd;
        }
*/        
    }

    internal partial class RNNot : RNBegin
    {
        internal RNNot(RThread p, RNode a) :
            base(p, a)
        {
        }
/*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            object o = ruby.Eval(self, bdy);
            result = (RTest(o)) ? false : true;
            return null;
        }
*/        
    }

    internal partial class RNRef : RNode
    {
        protected RNRef(RThread p, int n) :
            base(p)
        {
            argcnt = n;
            ct = p.LocalCnt('~');
        }
        protected int ct;
        protected int argcnt;
        internal override int argc
        {
            get { return argcnt; }
        }
        internal override int nth
        {
            get { return argcnt; }
        }
        internal override int cnt
        {
            get { return ct; }
        }
    }
        
    internal partial class RNNthRef : RNRef
    {
        internal RNNthRef(RThread p, int n) :
            base(p, n)
        {
        }
/*        
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            RThread th = ruby.GetCurrentContext();
            RMatchData m = (RMatchData)th.BackRef;
            if (m == null)
                result = null;
            else
            {
                string s = m[argcnt];
                if (s == null)
                    result = null;
                else
                {
                    if (m.IsTainted)
                        result = new RString(ruby, s, true);
                    else
                        result = s;
                }
            }
            return null;
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            RThread th = ruby.GetCurrentContext();
            RMatchData m = (RMatchData)th.BackRef;
            if (m != null && m[argcnt] != null)
                return String.Format("${0}", argcnt);
            return null;
        }
*/        
    }

    internal partial class RNBackRef : RNRef
    {
        internal RNBackRef(RThread p, int n) :
            base(p, n)
        {
        }
        
        /*
        internal override RNode Eval(NetRuby ruby, object self, out object result)
        {
#if EVAL_TRACE
            System.Console.WriteLine("Eval:" + ToString());
#endif
            RThread th = ruby.GetCurrentContext();
            RMatchData m = (RMatchData)th.scope.local_vars[ct];
            if (m == null)
                result = null;
            else
            {
                string s = null;
                switch ((char)argcnt)
                {
                case '&':
                    s = m[0];
                    break;
                case '`':
                    s = m.Pre;
                    break;
                case '\'':
                    s = m.Post;
                    break;
                case '+':
                    s = m.Last;
                    break;
                default:
                    ruby.bug("unexpected back-ref");
                    break;
                }
                if (m.IsTainted)
                    result = new RString(ruby, s, true);
                else
                    result = s;
            }
            return null;
        }
        protected override string IsDefined(NetRuby ruby, object self)
        {
            RThread th = ruby.GetCurrentContext();
            RMatchData m = (RMatchData)th.BackRef;
            if (m != null && m[0] != null)
                return String.Format("${0}", Convert.ToChar(argcnt));
            return null;
        }
        */
    }
}
// vim:et:sts=4:sw=4
