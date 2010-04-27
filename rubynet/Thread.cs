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
using System.Reflection.Emit;
using System.Threading;
using System.Security;

namespace NETRuby
{

    internal class Scope : RBasic, ICloneable
    {
        public Scope(NetRuby r) :
            base(r, null)
        {
            flag = 0;
            local_tbl = null;
            local_vars = null;
        }
        internal void InitLocal(uint[] tbl)
        {
            lock (this)
            {
                local_tbl = tbl;
                local_vars = new object[tbl.Length];
            }
        }
        internal void InitLocal(uint[] tbl, object[] vars)
        {
            lock (this)
            {
                local_tbl = tbl;
                local_vars = vars;
            }
        }
        public object Clone()
        {
            lock (this)
            {
                Scope s = (Scope)MemberwiseClone();
                s.Dup();
                return s;
            }
        }
        internal object this[int idx]
        {
            get {
                if (local_vars == null) return null;
                return local_vars[idx];
            }
        }

        internal void Dup()
        {
            flag |= DONT_RECYCLE;
            if (local_tbl != null)
            {
                local_vars = (object[])local_vars.Clone();
            }
        }
        public override bool Equals(object o)
        {
            // Bypass RBasic's Equals
            return (this == o);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        internal enum ScopeMode {
            Public = 0,
            Private = 1,
            Protected = 2,
            ModFunc = 5,
            Mask = 7,
        }

        internal const int XHR = 1;
        internal const int DONT_RECYCLE = 4;
        internal bool DontRecycle
        {
            get { return (flag & DONT_RECYCLE) != 0; }
        }
        int flag;
        internal uint[] local_tbl;
        internal object[] local_vars;
    }

    internal class Block
    {
/*    
        internal Block(RThread th, RNode vr, RNode bd, object slf)
        {
            origThread = th;
            tag = new BTag(th.ruby);
            var = vr;
            body = bd;
            self = slf;
            frame = (Frame)th.frame.Clone();
            klass = th.rClass;
            frame.file = th.file;
            frame.line = th.line;
            scope = th.scope;
            prev = th.block;
            iter = (ITER)th.iter.Peek();
            vMode = th.ScopeMode;
            flags = Flag.DScope;
            dyna_vars = th.dyna_vars;
            wrapper = th.wrapper;
        }
        internal bool CheckTag(Tag.TAG tg)
        {
            return (tg == tag.dst);
        }
        internal Tag.TAG DStatus
        {
            get { return tag.dst; }
            set { tag.dst = value; }
        }
        internal int TagID
        {
            get { return tag.GetHashCode(); }
        }
        internal bool IsTag
        {
            get { return tag != null; }
        }
        internal bool IsDynamic
        {
            get { return (flags & Flag.Dynamic) != 0; }
            set { if (value) flags |= Flag.Dynamic; else flags &= ~Flag.Dynamic; }
        }
        internal bool Orphan
        {
            set { if (value) flags |= Flag.Orphan; else flags &= ~Flag.Orphan; }
        }
        internal bool DScope
        {
            get { return (flags & Flag.DScope) != 0; }
            set { if (value) flags |= Flag.DScope; else flags &= ~Flag.DScope; }
        }
        internal class BTag : RBasic
        {
            internal BTag(NetRuby rb) :
                base(rb, null)
            {
                tflags = 0;
                dst = Tag.TAG.EMPTY;
                GetHashCode();
            }
            internal Tag.TAG dst;
            internal int tflags;
        }
        internal RNode var;
        internal RNode body;
        internal object self;
        internal Frame frame;
        internal Scope scope;
        BTag tag;
        enum Flag
        {
            DScope = 1,
            Dynamic = 2,
            Orphan = 4,
        }
        Flag flags;
        internal RMetaObject klass;
        internal ITER iter;
        internal Scope.ScopeMode vMode;
        internal RVarmap dyna_vars;
        internal RThread origThread;
        internal RModule wrapper;
        internal Block prev;

        internal Block Clone()
        {
            Block c = (Block)MemberwiseClone();
            c.scope = (Scope)scope.Clone();
            return c;
        }
        internal void DupPrev()
        {
            lock (this)
            {
                Block blk = this;
                while (blk.prev != null)
                {
                    Block tmp = (Block)blk.prev.Clone();
                    if (tmp.frame.args != null)
                    {
                        tmp.frame.args = (object[])tmp.frame.args.Clone();
                    }
                    tmp.scope = (Scope)tmp.scope.Clone();
                    blk.prev = tmp;
                    blk = tmp;
                }
            }
        }

        internal bool IsOrphan(RThread th)
        {
            if (scope != null && th.scopes.Contains(scope) == false)
            {
                return true;
            }
            if (origThread != th)
            {
                return true;
            }
            return false;
        }
*/        
    }
/*
    enum ITER
    {
        NOT = 0,
        PRE = 1,
        CUR = 2,
    }

    internal class Frame : ICloneable
    {
        internal Frame()
        {
            prev = null;
            tmp = null;
            args = null;
            line = 0;
            iter = ITER.NOT;
        }

        internal Frame(RThread th)
        {
            prev = th.frame;
            file = th.file;
            line = th.line;
            iter = (ITER)th.iter.Peek();
            if (prev != null)
            {
                cBase = prev.cBase;
            }
            tmp = null;
            args = null;
        }
        internal Frame(Frame curr)
        {
            self = curr.self;
            args = curr.args;
            lastFunc = curr.lastFunc;
            cBase = curr.cBase;
            prev = curr.prev;
            tmp = curr;
            file = curr.file;
            line = curr.line;
            iter = curr.iter;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
        public Frame Dup()
        {
            Frame nf = (Frame)MemberwiseClone();
            Frame top = nf;
            for (;;)
            {
                if (nf.args != null)
                {
                    nf.args = (object[])nf.args.Clone();
                }
                nf.tmp = null;
                if (nf.prev == null) break;
                nf.prev = (Frame)nf.prev.Clone();
                nf = nf.prev;
            }
            return top;
        }
        internal void CopyFromPrev()
        {
            self = prev.self;
            lastFunc = prev.lastFunc;
            lastClass = prev.lastClass;
            args = prev.args;
        }
        internal object self;
        internal object[] args;
        internal uint lastFunc;
        internal RMetaObject lastClass;
        internal RNCRef cBase;        // constants base node
        internal Frame prev;
        internal Frame tmp;
        internal string file;
        internal int line;
        internal ITER iter;
    }
*/
    public class RThread
    : RData, ICloneable
    {
        internal RThread(NetRuby rb) :
            base(rb, rb.cThread)
        {
            result = null;
            ////safeLevel = 0;
            threadId = AppDomain.GetCurrentThreadId();
            thread = Thread.CurrentThread;
            ////vMode = Scope.ScopeMode.Private;
        
            ////frame = new Frame();
            ////rb.topFrame = frame;
            scopes = new Stack();
            scope = new Scope(rb);
            dyna_vars = null;
            ////iter = new Stack();
            ////iter.Push(ITER.NOT);
            ////protTag = new Stack();
            ////    protTag.Push(null);

            ////block = null;
            lvtbl = null;

            rClass = null;
            wrapper = null;
            ////cRef = null;

            file = "ruby";
            line = 0;
            inEval = 0;
            tracing = false;
            errInfo = null;
            lastCallStat = CALLSTAT.PUBLIC;

            locals = null;
            gid = 0;
        }
        
        internal RThread(NetRuby rb, RThread th) :
            base(rb, rb.cThread)
        {
            result = null;
            ////safeLevel = th.safeLevel;
            gid = th.gid;
            abortOnException = rb.cThread.abortOnException;
            threadId = AppDomain.GetCurrentThreadId();
            thread = Thread.CurrentThread;
            vMode = Scope.ScopeMode.Private;
        
            ////frame = (Frame)rb.topFrame.Dup();
            ////frame.self = rb.topSelf;
            ////frame.cBase = rb.topCRef;
            scopes = new Stack();
            scope = new Scope(rb);
            dyna_vars = null;
            ////iter = new Stack();
            ////iter.Push(ITER.NOT);
            ////protTag = new Stack();
            ////    protTag.Push(null);

            ////block = null;
            lvtbl = null;

            rClass = rb.cObject;
            wrapper = null;
            ////cRef = rb.topCRef;

            file = "ruby";
            line = 0;
            inEval = 0;
            tracing = false;
            errInfo = null;
            lastCallStat = CALLSTAT.PUBLIC;

            locals = null;
        }

        public object Clone()
        {
            RThread tx = null;
            lock (this)
            {
                tx = (RThread)MemberwiseClone();
                tx.errInfo = null;
                tx.lastCallStat = CALLSTAT.PUBLIC;
                tx.locals = null;
                ////tx.iter = (Stack)iter.Clone();

                tx.scopes = (Stack)scopes.Clone();
                ////tx.frame = frame.Dup();
                ////if (block != null)
                ////{
                ////    tx.block.DupPrev();
                ////}
            }
            ////tx.protTag = new Stack();
            ////tx.protTag.Push(null);
            tx.inEval = 0;
            tx.threadId = 0;
            return tx;
        }

        //
        // Compiler methods and fields
        // 
        
        public RBasic[] argscache0 = new RBasic[0];
        public RBasic[] argscache1 = new RBasic[1];
        public RBasic[] argscache2 = new RBasic[2];
        public RBasic[] argscache3 = new RBasic[3];
        public static int CacheCount = 3;

        public Stack class_scopes = new Stack();
        public Stack legacy_blocks = new Stack();
        
        public RMetaObject GetClassScope()
        {
            return (RMetaObject)class_scopes.Peek();
        }

        public void PushClassScope(RMetaObject cls)
        {
            class_scopes.Push(cls);
        }

        public void PopClassScope()
        {
            class_scopes.Pop();
        }

        
        public bool IsBlockGiven()
        {
            return legacy_blocks.Count > 0 && GetLegacyBlock() != null;
        }
        
        public RCBlock GetLegacyBlock()
        {
            return (RCBlock)legacy_blocks.Peek();
        }
        
        public void PushLegacyBlock(RCBlock b)
        {
            //Console.WriteLine("Push block " + b);
            legacy_blocks.Push(b);
        }

        public void PopLegacyBlock()
        {
            //Console.WriteLine("Pop block");
            legacy_blocks.Pop();
        }

        public void GlobalSet(string name, RBasic val)
        {
            GlobalEntry ent = ruby.global_entry(ruby.global_id(name));
            ruby.GVarSet(ent, val);
        }

        public RBasic GlobalGet(string name)
        {
            GlobalEntry ent = ruby.global_entry(ruby.global_id(name));
            return (RBasic)ruby.GVarGet(ent);
        }
        
        //
        // End compiler methods
        //
        
        public bool AbortOnException
        {
            get { return abortOnException; }
            set { abortOnException = value; }
        }

        public int Priority
        {
            get { return (int)thread.Priority; }
            set { thread.Priority = (ThreadPriority)value; }
        }
        
        public bool HasError
        {
            get {
                return (errInfo != null) ? true : false;
            }
        }
        static System.Threading.ThreadState[] stopTbl = new System.Threading.ThreadState[] {
            System.Threading.ThreadState.WaitSleepJoin,
            System.Threading.ThreadState.Stopped,
            System.Threading.ThreadState.Suspended,
            System.Threading.ThreadState.Unstarted,
        };
        public bool IsStop
        {
            get {
                if (thread.IsAlive == false) return true;
                return (Array.IndexOf(stopTbl, thread.ThreadState) < 0) ? false : true;
            }
        }
        public bool IsAlive
        {
            get { return thread.IsAlive; }
        }
        internal string ConvStat()
        {
            if (thread.IsAlive == false) return "dead";
            System.Threading.ThreadState st = thread.ThreadState;
            if ((st & System.Threading.ThreadState.AbortRequested) != 0)
                return "aborting";
            if ((st & (System.Threading.ThreadState.WaitSleepJoin
                       | System.Threading.ThreadState.Stopped
                       | System.Threading.ThreadState.Suspended)) != 0)
                return "sleep";
            if (st == System.Threading.ThreadState.Running ||
                (st & (System.Threading.ThreadState.Background
                       | System.Threading.ThreadState.SuspendRequested
                       | System.Threading.ThreadState.StopRequested
                       | System.Threading.ThreadState.Unstarted)) != 0)
                return "run";
        
            return String.Format("unknown({0})", st.ToString());
        }
        
        public override object Inspect()
        {
            string cname = ruby.ClassOf(this).Name;
            string str = String.Format("#<{0}:0x{1:x8} {2}>", cname,
                                       threadId, ConvStat());
        
            /*if (IsTainted)
            {
                return new RString(ruby, str, true);
            }*/
            return str;
        }
/*
        internal RMetaObject cBase
        {
            get { return frame.cBase.clss; }
        }
        internal Frame DupFrame()
        {
            return frame = new Frame(frame);
        }
        internal void CopyPrevFrame()
        {
            lock (this)
            {
                frame.CopyFromPrev();
            }
        }
        internal Frame PushFrame(bool makedummy)
        {
            lock (this)
            {
                frame = new Frame(this);
                if (makedummy)
                {
                    frame = (Frame)frame.prev.prev.Clone();
                }
            }
            return frame;
        }
        internal void PopFrame()
        {
            lock (this)
            {
                file = frame.file;
                line = frame.line;
                frame = frame.prev;
            }
        }
        internal void PushTag(Tag.TAG t)
        {
            protTag.Push(new Tag(t, this));
        }
        internal Tag PopTag(bool setResult)
        {
            Tag tag = (Tag)protTag.Pop();
            if (setResult)
            {
                tag.SetResult((Tag)protTag.Peek());
            }
            return tag;
        }
        internal Tag CurrentTag
        {
            get { return (Tag)protTag.Peek(); }
        }
        internal void TagJump(Tag.TAG state)
        {
            Tag tag = (Tag)protTag.Peek();
            if (tag != null)
            {
                tag.Jump(state, this);
            }
        }
        internal void PushIter(ITER i)
        {
            iter.Push(i);
        }
        internal void PopIter()
        {
            iter.Pop();
        }
        internal bool IsBlockGiven
        {
            get { return (frame.iter != 0); }
        }

        internal bool IsPriorBlockGiven
        {
            get {
                if (frame.prev != null && frame.prev.iter != 0 && block != null)
                    return true;
                return false;
            }
        }
*/        
        internal void CompileError(string s)
        {
            ErrorAppend(s);
            nerrs++;
        }
        internal void ErrorAppend(string s)
        {
            if (inEval > 0)
            {
                if (errInfo != null)
                {
                    StringBuilder sb = new StringBuilder(errInfo.ToString());
                    sb.AppendFormat("\n{0}", s);
                    s = sb.ToString();
                }
                errInfo = new RException(ruby, s, ruby.eSyntaxError);
            }
            else
            {
                System.Console.Error.WriteLine("Compile error after line " + line.ToString() + ": " + s);
            }
        }
        
        internal int errorHandle(Exception exc)
        {
            Tag.TAG state = (exc is eTagJump) ? ((eTagJump)exc).state : Tag.TAG.RAISE;
            int ex = 0;
            switch (state & Tag.TAG.MASK) {
            case Tag.TAG.EMPTY:
                ex = 0;
                break;
            case Tag.TAG.RETURN:
                errorPos();
                System.Console.Error.WriteLine(": unexpected return");
                ex = 1;
                break;
            case Tag.TAG.NEXT:
                errorPos();
                System.Console.Error.WriteLine(": unexpected next");
                ex = 1;
                break;
            case Tag.TAG.BREAK:
                errorPos();
                System.Console.Error.WriteLine(": unexpected break");
                ex = 1;
                break;
            case Tag.TAG.REDO:
                errorPos();
                System.Console.Error.WriteLine(": unexpected redo");
                ex = 1;
                break;
            case Tag.TAG.RETRY:
                errorPos();
                System.Console.Error.WriteLine(": retry outside of rescue clause");
                ex = 1;
                break;
            case Tag.TAG.RAISE:
            case Tag.TAG.FATAL:
                ruby.errorPrint(this, exc);
                ex = 1;
                break;
            default:
                ruby.bug("Unknown longjmp status " + ex.ToString());
                break;
            }
            return ex;
        }
        
        internal void errorPos()
        {
            string s;
            if (file != null)
            {
                /*
                if (frame.lastFunc != 0)
                {
                    s = String.Format("{0}:{1}:in `{2}'",
                                      file, line, ruby.id2name(frame.lastFunc));
                }
                else
                */
                if (line == 0)
                {
                    s = file;
                }
                else
                {
                    s = String.Format("{0}:{1}", file, line);
                }
                System.Console.Error.Write(s);
            }
        }

        Scope.ScopeMode vMode;
        internal RMetaObject PushClass(RMetaObject klass)
        {
            RMetaObject old = rClass;
            rClass = klass;
            return old;
        }
        internal void PopClass(RMetaObject org)
        {
            rClass = org;
        }
        internal Scope.ScopeMode PushScope()
        {
            Scope.ScopeMode vm = vMode;
            Scope sc = new Scope(ruby);
            scopes.Push(scope);
            scope = sc;
            vMode = Scope.ScopeMode.Public;
            return vm;
        }
        internal void PopScope(Scope.ScopeMode vm)
        {
            scope = (Scope)scopes.Pop();
            vMode = vm;
        }
        internal Scope.ScopeMode ScopeMode
        {
            get { return vMode; }
        }
        internal void ScopeSet(Scope.ScopeMode f)
        {
            vMode = f;
        }
        internal bool ScopeTest(Scope.ScopeMode f)
        {
            return (vMode & f) != 0;
        }
/*
        internal ITER Iter
        {
            get { return (ITER)iter.Peek();}
        }
*/        
        // Locals
        internal RVarmap DynaPush()
        {
            RVarmap vars = dyna_vars;

            dvar_push(0, null);
            lvtbl.dlev++;
            return vars;
        }

        internal void DynaPop(RVarmap vars)
        {
            lvtbl.dlev--;
            dyna_vars = vars;
        }


        internal RVarmap new_dvar(uint id, object value, RVarmap prev)
        {
            RVarmap vars = new RVarmap(ruby, id, value, prev);
            return vars;
        }

        internal bool dvar_defined(uint id)
        {
            RVarmap vars = dyna_vars;
            while (vars != null)
            {
                if (vars.id == id) return true;
                vars = vars.next;
            }
            return false;
        }

        internal bool dvar_curr(uint id)
        {
            RVarmap vars = dyna_vars;
            while (vars != null)
            {
                if (vars.id == 0) break;
                if (vars.id == id) return true;
                vars = vars.next;
            }
            return false;
        }

        internal void dvar_push(uint id, object value)
        {
            lock (this)
            {
                dyna_vars = new_dvar(id, value, dyna_vars);
            }
        }
        private void DVarAsgn(uint id, object val, bool curr)
        {
            RVarmap vars = dyna_vars;
            while (vars != null)
            {
                if (curr && vars.id == id)
                {
                    vars.val = val;
                    return;
                }
                vars = vars.next;
            }
            if (dyna_vars == null)
            {
                dyna_vars = new_dvar(id, val, null);
            }
            else
            {
                vars = new_dvar(id, val, dyna_vars.next);
                dyna_vars.next = vars;
            }
        }
        internal void DVarAsgn(uint id, object val)
        {
            DVarAsgn(id, val, false);
        }
        internal void DVarAsgnCurr(uint id, object val)
        {
            DVarAsgn(id, val, true);
        }
        internal object DVarRef(uint id)
        {
            RVarmap vars = dyna_vars;
            while (vars != null)
            {
                if (vars.id == id)
                {
                    return vars.val;
                }
                vars = vars.next;
            }
            return null;
        }

        internal bool IsDynaInBlock
        {
            get { return (lvtbl.dlev > 0); }
        }

        internal bool LocalID(uint id)
        {
            if (lvtbl == null || lvtbl.tbl == null) return false;
            return (Array.IndexOf(lvtbl.tbl, id, 2) < 0) ? false : true;
        }
        
        void SpecialLocalSet(char c, object val)
        {
            TopLocalInit();
            int cnt = LocalCnt((uint)c);
            TopLocalSetup();
            scope.local_vars[cnt] = val;
        }

        internal void TopLocalInit()
        {
            LocalPush();
            int cnt = (scope.local_tbl != null) ? scope.local_tbl.Length : 0;
            lvtbl.Init(cnt, scope.local_tbl, (dyna_vars != null));
        }

        internal void TopLocalSetup()
        {
            int len = lvtbl.Length;

            if (len > 0)
            {
                scope.InitLocal(lvtbl.tbl);
            }
            LocalPop();
        }

        internal void LocalPush()
        {
            LocalVars local = new LocalVars(lvtbl, 0, 0);
            lvtbl = local;
        }
        internal void LocalPop()
        {
            LocalVars local = lvtbl.prev;
            if (lvtbl.tbl != null)
            {
                lvtbl.tbl = null;
            }
            lvtbl = local;
        }
        internal int LocalAppend(uint id)
        {
            return lvtbl.Append(id);
        }

        internal int LocalCnt(uint id)
        {
            return lvtbl.Count(id);
        }
        internal uint[] Locals
        {
            get { return lvtbl.tbl; }
        }

        internal object BackRef
        {
            get { return scope[1]; }
            set {
                if (scope.local_vars == null)
                    SpecialLocalSet('~', value);
                else
                    scope.local_vars[1] = value;
            }
        }

        internal Thread Owner
        {
            get { return thread; }
            set { thread = value; }
        }
        
        internal void EnterEval()
        {
            inEval++;
        }
        internal void LeaveEval()
        {
            inEval--;
        }
        internal bool IsInEval
        {
            get { return (inEval > 0); }
        }
        internal void ClearCompileError()
        {
            nerrs = 0;
        }
        internal bool CompileFailed
        {
            get { return (nerrs > 0); }
        }

        internal object result;
        
        internal RNode evalTreeBegin = null;
        internal RNode evalTree = null;
        //
        ////internal Frame frame;
        internal Scope scope;
        internal RVarmap dyna_vars;
        ////internal Block block;
        internal Stack scopes;
        ////internal Stack iter;
        ////internal Stack protTag;
        internal RMetaObject rClass;
        internal RModule wrapper;
        ////internal RNCRef cRef;
        private LocalVars lvtbl;
        internal Thread thread;
        internal int gid;

        internal string file;
        internal int line;
        internal bool tracing;
        internal RException errInfo;
        private int inEval;
        private int nerrs;
        internal CALLSTAT lastCallStat;
        private object[] args;

        ////internal int safeLevel;
        int threadId;
        bool abortOnException;

        internal st_table locals;

/*
        internal void Secure(int level)
        {
            if (level <= safeLevel)
            {
                string s = String.Format("Insecure operation `{0}' at level {1}",
                                         ruby.id2name(frame.lastFunc), safeLevel);
                throw new SecurityException(s);
            }
        }
*/
        
        internal void entry()
        {
/*        
            threadId = AppDomain.GetCurrentThreadId();
            PushTag(Tag.TAG.PROT_THREAD);
            try
            {
                result = ruby.Yield(args, null, null, true);
            }
            catch (eTagJump e)
            {
                if (e.state == Tag.TAG.RAISE && errInfo != null)
                {
                    checkError();
                }
            }
            catch (Exception e)
            {
                errInfo = new RException(ruby, e);
                checkError();
            }
            PopTag(true);
            ruby.ClearContext(this);
*/            
        }
        void checkError()
        {
            if (/*safeLevel < 4 &&*/
                (abortOnException || ruby.cThread.AbortOnException || ruby.debug))
            {
                ruby.errorPrint(this);
                Environment.Exit(1);
            }
        }
        internal void run()
        {
            if (thread.IsAlive == false)
                throw new eThreadError("killed thread");
            if ((thread.ThreadState & System.Threading.ThreadState.WaitSleepJoin) != 0)
                thread.Interrupt();
            else if ((thread.ThreadState & System.Threading.ThreadState.Suspended) != 0)
                thread.Resume();
        }
        internal RThread exit()
        {
            if (Thread.CurrentThread != thread)
            {
                ////Secure(4);
                if (thread.IsAlive == false) return this;
            }
        
            if (this == ruby.threadMain/* && ruby.threads.Count == 0 */)
            {
                ruby.Exit(0);
            }
            ruby.ClearContext(this);
            thread.Abort();
            return this;
        }

        internal object join()
        {
            if (thread.IsAlive)
            {
                if (Thread.CurrentThread == thread)
                    throw new eThreadError("thread tried to join itself");
                thread.Join();
            }
            if (errInfo != null)
            {
                throw new eTagJump(errInfo);
            }
            return result;
        }

        internal object GetTLS(object key)
        {
            uint id = ruby.ToID(key);
            return LocalARef(id);
        }

        internal object SetTLS(object key, object val)
        {
            RThread th = ruby.GetCurrentContext();
/*            
            if (th.safeLevel >= 4 && th != this)
            {
                throw new SecurityException("Insecure: can't modify thread locals");
            }
*/            
            if (IsFrozen) ruby.ErrorFrozen("thread locals");
            uint id = ruby.ToID(key);
            return LocalASet(id, val);
        }
        internal bool IsKey(object key)
        {
            if (locals == null) return false;
            return locals.ContainsKey(ruby.ToID(key));
        }
        internal object LocalARef(uint key)
        {
            if (locals == null) return null;
            object o;
            if (locals.lookup(key, out o)) return o;
            return null;
        }
        internal object LocalASet(uint key, object val)
        {
            lock (this)
            {
                if (locals == null)
                {
                    locals = new st_table();
                }
                if (val == null)
                {
                    locals.Remove(id);
                }
                else
                {
                    locals[id] = val;
                }
            }
            return val;
        }
        public RThread Raise(object[] argv)
        {
            if (thread.IsAlive == false) return null;
            if (thread == Thread.CurrentThread)
            {
                ruby.ruby_raise(this, argv);
            }
            throw new NotSupportedException("NETRuby can't handle Thread#raise");
        }
        
        static internal RThread Start(NetRuby ruby, object[] args)
        {
            RThread parent = ruby.GetCurrentContext();
            RThread thrd = (RThread)parent.Clone();
            return thrd.Start(args);
        }

        internal RThread Start(object[] argv)
        {
            thread = new Thread(new ThreadStart(entry));
            args = argv;
            ruby.AddContext(this);
            thread.Start();
            return this;
        }
        
        internal void Init(NetRuby rb)
        {
            if (klass == null)
                klass = rb.cThread;
            rClass = rb.cObject;
            ////frame.self = rb.topSelf;
            ////cRef = rb.topCRef;
            abortOnException = rb.cThread.abortOnException;

            RThreadGroup tg = (RThreadGroup)rb.cThreadGroup.ConstGetAt(rb.intern("Default"));
            tg.Add(this);
        }
    }
    
    public class RThreadClass : RClass
    {
        private RThreadClass(NetRuby rb) :
            base(rb, "Thread", rb.cObject)
        {
            abortOnException = false;
        }
        internal bool abortOnException;
        public bool AbortOnException
        {
            get { return abortOnException; }
            set { abortOnException = value; }
        }

        static public RBasic NewThread(object[] argv, RMetaObject meta)
        {
            NetRuby ruby = meta.ruby;
            RThread parent = ruby.GetCurrentContext();
            RThread thrd = (RThread)parent.Clone();
            thrd.thread = null;
            thrd.klass = meta;
            ruby.CallInit(thrd, argv);
            return thrd;
        }

        static private RThread check(NetRuby rb, object o)
        {
            if (o is RThread == false)
                throw new eTypeError(String.Format("wrong argument type {0} (expected Thread)",
                                                   rb.ClassOf(o).Name));
            return (RThread)o;
        }
        static internal object thread_new(RBasic r, params object[] args)
        {
            RBasic o = NewThread(args, (RMetaObject)r);
            return o;
        }
        static internal object initialize(RBasic r, params object[] args)
        {
/*        
            if (r.ruby.IsBlockGiven == false)
            {
                throw new eThreadError("must be called with a block");
            }
            return check(r.ruby, r).Start(args);
*/
return null; //PH
        }
        static internal object thread_start(RBasic r, params object[] args)
        {
/*        
            if (r.ruby.IsBlockGiven == false)
            {
                throw new eThreadError("must be called with a block");
            }
            return RThread.Start(r.ruby, args);
*/
            return null; //PH            
        }
        static internal object thread_stop(RBasic r, params object[] args)
        {
            RThread th = r.ruby.GetCurrentContext();
            if (th.thread != null && th.thread.IsAlive)
            {
                th.thread.Suspend();
            }
            return null;
        }
        static internal object thread_s_kill(RBasic r, params object[] args)
        {
            return check(r.ruby, args[0]).exit();
        }
        static internal object thread_exit(RBasic r, params object[] args)
        {
            RThread th = r.ruby.GetCurrentContext();
            return th.exit();
        }
        static internal object thread_pass(RBasic r, params object[] args)
        {
            Thread.Sleep(0);
            return null;
        }
        static internal object thread_current(RBasic r, params object[] args)
        {
            return r.ruby.GetCurrentContext();
        }
        static internal object thread_main(RBasic r, params object[] args)
        {
            return r.ruby.threadMain;
        }
        static internal object thread_list(RBasic r, params object[] args)
        {
            return r.ruby.ThreadList;
        }
        
        static internal object thread_run(RBasic r, params object[] args)
        {
            ((RThread)r).run();
            return r;
        }
        static internal object thread_wakeup(RBasic r, params object[] args)
        {
            return thread_run(r, args);
        }
        static internal object thread_kill(RBasic r, params object[] args)
        {
            return ((RThread)r).exit();
        }
        static internal object thread_value(RBasic r, params object[] args)
        {
            return ((RThread)r).join();
        }
        static internal object thread_status(RBasic r, params object[] args)
        {
            RThread th = (RThread)r;
            Thread thrd = th.thread;
            string s = th.ConvStat();
            if (th.IsAlive == false)
            {
                if (th.HasError) return null;
                return false;
            }
            return s;
        }
        static internal object thread_join(RBasic r, params object[] args)
        {
            ((RThread)r).join();
            return r;
        }
        static internal object thread_alive(RBasic r, params object[] args)
        {
            RThread th = (RThread)r;
            return th.thread.IsAlive;
        }
        static internal object thread_isstop(RBasic r, params object[] args)
        {
            return ((RThread)r).IsStop;
        }
        static internal object thread_raise(RBasic r, params object[] args)
        {
            return ((RThread)r).Raise(args);
        }
        static internal object critical_getset(RBasic r, params object[] args)
        {
            throw new NotSupportedException("NETRuby can't handle Thread#critical");
        }

        static internal object thread_s_abort_exc(RBasic r, params object[] args)
        {
            NetRuby rb = r.ruby;
            return rb.cThread.AbortOnException;
        }
        static internal object thread_s_abort_set(RBasic r, params object[] args)
        {
            NetRuby rb = r.ruby;
            bool b = RBasic.RTest(args[0]);
            rb.cThread.AbortOnException = b;
            return b;
        }
        static internal object thread_abort_exc(RBasic r, params object[] args)
        {
            return ((RThread)r).AbortOnException;
        }
        static internal object thread_abort_set(RBasic r, params object[] args)
        {
            bool b = RBasic.RTest(args[0]);
            ((RThread)r).AbortOnException = b;
            return b;
        }

        static internal object thread_priority(RBasic r, params object[] args)
        {
            return ((RThread)r).Priority;
        }
        static int[] threadPtyVal = new int[] {
            (int)ThreadPriority.AboveNormal,
            (int)ThreadPriority.BelowNormal,
            (int)ThreadPriority.Highest,
            (int)ThreadPriority.Lowest,
            (int)ThreadPriority.Normal,
        };
        static internal object thread_priority_set(RBasic r, params object[] args)
        {
            NetRuby rb = r.ruby;
            long l = rb.InstanceOf(args[0]).ToInteger().ToLong();
            if (Array.IndexOf(threadPtyVal, (int)l) < 0)
            {
                throw new ArgumentException("invalid thread priority value");
            }
            ((RThread)r).Priority = (int)l;
            return null;
        }
/*        
        static internal object thread_safe_level(RBasic r, params object[] args)
        {
            return ((RThread)r).safeLevel;
        }
*/        
        static internal object thread_aref(RBasic r, params object[] args)
        {
            return ((RThread)r).GetTLS(args[0]);
        }
        static internal object thread_aset(RBasic r, params object[] args)
        {
            return ((RThread)r).SetTLS(args[0], args[1]);
        }
        static internal object thread_keyp(RBasic r, params object[] args)
        {
            return ((RThread)r).IsKey(args[0]);
        }

        internal static void Init(NetRuby rb)
        {
            rb.eThreadError = rb.DefineClass("ThreadError", rb.eException);
        
            RThreadClass t = new RThreadClass(rb);
            t.DefineClass("Thread", rb.cObject);
            rb.cThread = t;
            t.DefineSingletonMethod("new", new RMethod(thread_new), -1);
            t.DefineMethod("initialize", new RMethod(initialize), -2);
            t.DefineSingletonMethod("start", new RMethod(thread_start), -2);
            t.DefineSingletonMethod("fork", new RMethod(thread_start), -2);
            t.DefineSingletonMethod("stop", new RMethod(thread_stop), 0);
            t.DefineSingletonMethod("kill", new RMethod(thread_s_kill), 1);
            t.DefineSingletonMethod("exit", new RMethod(thread_exit), 0);
            t.DefineSingletonMethod("pass", new RMethod(thread_pass), 0);
            t.DefineSingletonMethod("current", new RMethod(thread_current), 0);
            t.DefineSingletonMethod("main", new RMethod(thread_main), 0);
            t.DefineSingletonMethod("list", new RMethod(thread_list), 0);

            t.DefineSingletonMethod("critical", new RMethod(critical_getset), 0);
            t.DefineSingletonMethod("critical", new RMethod(critical_getset), 1);
            t.DefineSingletonMethod("abort_on_exception", new RMethod(thread_s_abort_exc), 0);
            t.DefineSingletonMethod("abort_on_exception=", new RMethod(thread_s_abort_set), 1);
        
            t.DefineMethod("run", new RMethod(thread_run), 0);
            t.DefineMethod("wakeup", new RMethod(thread_wakeup), 0);
            t.DefineMethod("kill", new RMethod(thread_kill), 0);
            t.DefineMethod("exit", new RMethod(thread_kill), 0);
            t.DefineMethod("value", new RMethod(thread_value), 0);
            t.DefineMethod("status", new RMethod(thread_status), 0);
            t.DefineMethod("join", new RMethod(thread_join), 0);
            t.DefineMethod("alive?", new RMethod(thread_alive), 0);
            t.DefineMethod("stop?", new RMethod(thread_isstop), 0);
            t.DefineMethod("raise", new RMethod(thread_raise), 0);

            t.DefineMethod("abort_on_exception", new RMethod(thread_abort_exc), 0);
            t.DefineMethod("abort_on_exception=", new RMethod(thread_abort_set), 1);

            t.DefineMethod("priority", new RMethod(thread_priority), 0);
            t.DefineMethod("priority=", new RMethod(thread_priority_set), 1);
            ////t.DefineMethod("safe_level", new RMethod(thread_safe_level), 0);
        
            t.DefineMethod("[]", new RMethod(thread_aref), 1);
            t.DefineMethod("[]=", new RMethod(thread_aset), 2);
            t.DefineMethod("key?", new RMethod(thread_keyp), 1);
        }
    }

    public class RThreadGroup : RData
    {
        internal RThreadGroup(NetRuby rb, RMetaObject meta) :
            base(rb, meta)
        {
            gid = GetHashCode();
        }
        int gid;
        public RArray List
        {
            get {
                RArray array = new RArray(ruby, true);
                Hashtable threads = ruby.threads;
                lock (threads.SyncRoot)
                {
                    foreach (DictionaryEntry ent in threads)
                    {
                        RThread r = (RThread)ent.Value;
                        if (r.gid == gid) array.Add(r);
                    }
                }
                if (ruby.threadMain.gid == gid)
                    array.Add(ruby.threadMain);
                return array;
            }
        }
        public RThreadGroup Add(object o)
        {
            RThread th = ruby.GetCurrentContext();
            ////th.Secure(4);
            if (o is RThread == false)
            {
                throw new eTypeError(String.Format("wrong argument type {0} (expected Thread)",
                                                   ruby.ClassOf(o).Name));
            }
            lock (o)
            {
                ((RThread)o).gid = gid;
            }
            return this;
        }
    }
  
    public class RThreadGroupClass : RClass
    {
        private RThreadGroupClass(NetRuby rb) :
            base(rb, "ThreadGroup", rb.cObject)
        {
        }
        static public RBasic NewThreadGroup(object[] argv, RMetaObject meta)
        {
            NetRuby ruby = meta.ruby;
            RThreadGroup tg = new RThreadGroup(ruby, meta);
            ruby.CallInit(tg, argv);
            return tg;
        }
        static internal object tg_new(RBasic r, params object[] args)
        {
            return NewThreadGroup(args, (RMetaObject)r);
        }
        static internal object list(RBasic r, params object[] args)
        {
            return ((RThreadGroup)r).List;
        }
        static internal object add(RBasic r, params object[] args)
        {
            return ((RThreadGroup)r).Add(args[0]);
        }
        
        internal static void Init(NetRuby rb)
        {
            RThreadGroupClass t = new RThreadGroupClass(rb);
            t.DefineClass("ThreadGroup", rb.cObject);
            rb.cThreadGroup = t;
            t.DefineSingletonMethod("new", new RMethod(tg_new), -1);
            t.DefineMethod("list", new RMethod(list), 0);
            t.DefineMethod("add", new RMethod(add), 1);
            RThreadGroup rg = (RThreadGroup)tg_new(t);
            t.ConstSet(rb.intern("Default"), rg);
            rg.Add(rb.GetCurrentContext());
        }
    }   
}

// vim:et:sts=4:sw=4
