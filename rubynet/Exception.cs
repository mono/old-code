/*
Copyright (C) 2001-2002 arton
Copyright (C) 2005      Jaen Saul
*/

using System;

namespace NETRuby
{
    internal class Tag
    {
        internal enum TAG {
            EMPTY = 0,
            RETURN = 1,
            BREAK = 2,
            NEXT = 3,
            RETRY = 4,
            REDO = 5,
            RAISE = 6,
            THROW = 7,
            FATAL = 8,
            MASK = 15,
            //
                PROT_NONE = 129,
            PROT_FUNC = 130,
            PROT_THREAD = 131,
        }
        internal Tag(TAG t, RThread th)
        {
            retval = null;
            ////frame = th.frame;
            ////iter = th.iter.Peek();
            scope = th.scope;
            tag = t;
            dst = 0;
        }
/*        
        internal void Jump(TAG st, RThread th)
        {
            th.frame = frame;
            if (th.scopes.Contains(scope))
            {
                while (scope != (Scope)th.scopes.Peek())
                {
                    th.scopes.Pop();
                }
            }
            if (th.iter.Contains(iter))
            {
                while (iter != th.iter.Peek())
                {
                    th.iter.Pop();
                }
            }
            throw new eTagJump(st);
        }
        internal void Pop(RThread th)
        {
            if (th.protTag.Contains(this))
            {
                while (this != (Tag)th.protTag.Peek())
                {
                    th.protTag.Pop();
                }
            }
            th.CurrentTag.retval = retval;
        }
*/
        
        internal void SetResult(Tag prv)
        {
            if (prv != null)
            {
                prv.retval = retval;
            }
        }
        internal object Result
        {
            get { return retval; }
            set { retval = value; }
        }
        
        object retval;
        ////Frame frame;
        ////object iter;
        Scope scope;
        internal TAG tag;
        int dst;
    }
    public class eArgError : Exception
    {
        public eArgError(string message)
          : base(message)
        {}
    }

    public class eNameError : Exception
    {
        public eNameError(string message)
          : base(message)
        {}
    }

    public class eRangeError : Exception
    {
        public eRangeError(string message)
          : base(message)
        {}
    }

    public class eFloatDomainError : eRangeError
    {
        public eFloatDomainError(string message) : base(message) {
        }
    }

    public class eTypeError : Exception
    {
        public eTypeError(string message) : base(message) {
        }
        public eTypeError(string message, Exception e) : base(message, e) {
        }
    }

    public class eLocalJumpError : Exception
    {
        public eLocalJumpError(string message)
          : base(message)
        {}
    }

    public class eThreadError : Exception
    {
        public eThreadError(string message)
          : base(message)
        {}
    }

    public class eIndexError : Exception
    {
        public eIndexError(string message)
          : base(message)
        {}
    }

    public class eLoadError : Exception
    {
        public eLoadError(string message)
          : base(message)
        {}
    }

    public class NetRubyException : Exception
    {
        internal NetRubyException(RException ex)
        {
            errInfo = ex;
        }
        public RException ErrorInfo
        {
            get { return errInfo; }
        }
        public new Exception InnerException
        {
            get { return errInfo.InnerException; }
        }
        public string[] Backtrace
        {
            get {
                RArray bt = errInfo.Backtrace;
                if (bt == null) return null;
                string[] ret = new string[bt.Count];
                bt.ArrayList.CopyTo(ret);
                return ret;
            }
        }
        RException errInfo;
    }
    
    internal class eTagJump : Exception
    {
        internal eTagJump(Tag.TAG t)
        {
            state = t;
        }
        internal eTagJump(Tag.TAG t, Exception e, string s) :
            base(s, e)
        {
            state = t;
        }
        internal eTagJump(RException e)
        {
            state = Tag.TAG.RAISE;
            rex = e;
            NetRuby rb = e.ruby;
            RThread th = rb.GetCurrentContext();
            if (th.file != null)
            {
                RArray at = e.Backtrace;
                if (at == null)
                {
                    at = rb.Backtrace(-1);
                    e.Backtrace = at;
                }
            }
            th.errInfo = e;
            if (rb.debug && e.IsKindOf(rb.eSystemExit) == false)
            {
                Console.Error.WriteLine("Exception `{0}' at {1}:{2} - {3}",
                                        rb.ClassOf(e).Name, th.file, th.line,
                                        e.ToString());
            }
            /*
            // trace
            if (th.protTag.Count <= 1)
            {
                rb.errorPrint(th);
            }
            */
        }
        internal Tag.TAG state;
        internal RException rex;
    }

    public class RException : RObject
    {
        internal RException(NetRuby ruby, Exception ex) :
            base(ruby, ruby.eException)
        {
            innerException = ex;
            IVarSet("mesg", ex.Message);
            Backtrace = ruby.Backtrace(-1);
        }
        internal RException(NetRuby ruby, RMetaObject meta) :
            base(ruby, meta)
        {
            innerException = null;
        }
        internal RException(NetRuby ruby, string str, RMetaObject meta) :
            base(ruby, meta)
        {
            innerException = null;
            IVarSet("mesg", str);
        }
        public Exception InnerException
        {
            get { return innerException; }
        }
        Exception innerException;
        public override string ToString()
        {
            uint msg = ruby.intern("mesg");
            object o = null;
            if (IsIVarDefined(msg))
            {
                o = IVarGet("mesg");
            }
            if (o == null) return ruby.ClassOf(this).ClassPath;
            return o.ToString();
        }
        public override object Inspect()
        {
            RMetaObject klass = ruby.ClassOf(this);
            string s = ToString();
            if (s.Length == 0)
                return klass.ClassPath;
            return String.Format("#<{0}: {1}>", klass.ClassPath, s);
        }
        
        static internal object exc_new(RBasic r, params object[] args)
        {
            RException ex = new RException(r.ruby, (RMetaObject)r);
            r.ruby.CallInit(ex, args);
            return ex;
        }
        static internal object exc_exception(RBasic r, params object[] args)
        {
            if (args.Length == 0) return r;
            if (args.Length == 1 && args[0] == r) return r;
            NetRuby rb = r.ruby;
            RMetaObject etype = rb.ClassOf(r);
            while (etype is RSingletonClass)
            {
                etype = etype.super;
            }
            RException ex = new RException(rb, etype);
            r.ruby.CallInit(ex, args);
            return ex;
        }
        static internal object exc_initialize(RBasic r, params object[] argv)
        {
            string s = String.Empty;
            if (argv.Length == 1)
            {
                s = argv[0].ToString();
            }
            r.IVarSet("mesg", s);
            return r;
        }
        public RArray Backtrace
        {
            get {
                uint bt = ruby.intern("bt");
                return (IsIVarDefined(bt)) ? (RArray)IVarGet(bt) : null;
            }
            set { IVarSet("bt", CheckBacktrace(ruby, value));}
        }
        static internal object exc_to_s(RBasic r, params object[] args)
        {
            return r.ToRString();
        }
        static internal object exc_backtrace(RBasic r, params object[] args)
        {
            return ((RException)r).Backtrace;
        }
        static private RArray CheckBacktrace(NetRuby rb, object o)
        {
            RArray a = null;
            if (o != null)
            {
                if (o is string || o is RString)
                    return new RArray(rb, new object[1] { o });
                if (o is RArray == false)
                {
                    throw new eTypeError("backtrace must be Array of String");
                }
                a = (RArray)o;
                foreach (object x in a)
                {
                    if (x is string == false && x is RString == false)
                        throw new eTypeError("backtrace must be Array of String");
                }
            }
            return a;
        }
        static internal object exc_set_backtrace(RBasic r, params object[] args)
        {
            return ((RException)r).Backtrace = CheckBacktrace(r.ruby, args[0]);
        }
        
        internal static void Init(NetRuby rb)
        {
            RClass ex = rb.DefineClass("Exception", rb.cObject);
            rb.eException = ex;
            ex.DefineSingletonMethod("exception", new RMethod(exc_new), -1);
            ex.DefineMethod("exception", new RMethod(exc_exception), -1);
            ex.DefineMethod("initialize", new RMethod(exc_initialize), -1);
            ex.DefineMethod("message", new RMethod(exc_to_s), 0);
            ex.DefineMethod("backtrace", new RMethod(exc_backtrace), 0);
            ex.DefineMethod("set_backtrace", new RMethod(exc_set_backtrace), 0);
            rb.eSystemExit = rb.DefineClass("SystemExit", ex);
            rb.eFatal              = rb.DefineClass("Fatal", ex);
            rb.eInterrupt   = rb.DefineClass("Interrupt", ex);
            rb.eSignal      = rb.DefineClass("SignalException", ex);
            rb.eStandardError = rb.DefineClass("StandardError", ex);
            rb.eTypeError   = rb.DefineClass("TypeError", rb.eStandardError);
            rb.eArgError    = rb.DefineClass("ArgumentError", rb.eStandardError);
            rb.eIndexError  = rb.DefineClass("IndexError", rb.eStandardError);
            rb.eRangeError  = rb.DefineClass("RangeError", rb.eStandardError);
            rb.eScriptError = rb.DefineClass("ScriptError", ex);
            rb.eSyntaxError = rb.DefineClass("SyntaxError", rb.eScriptError);
            rb.eNameError   = rb.DefineClass("NameError", rb.eScriptError);
            rb.eLoadError   = rb.DefineClass("LoadError", rb.eScriptError);
            rb.eNotImpError = rb.DefineClass("NotImplementedError", rb.eScriptError);
            rb.eRuntimeError = rb.DefineClass("RuntimeError", rb.eStandardError);
            rb.eSecurityError = rb.DefineClass("SecurityError", rb.eStandardError);
            rb.eNoMemError = rb.DefineClass("NoMemoryError", ex);
        }
    }
}
