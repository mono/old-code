/*
Copyright (C) 2005      Jaen Saul
*/

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace NETRuby
{
    class Variable
    {
        public uint vid;

        public bool IsArgument;
        public bool IsDynamic; // Is used in a closure

        public LocalBuilder local;
        public int closure_slot; // Position in closure array
    }
    
    // Scope for emitting code into (method scope, block scope, class scope etc.)
    class EmitScope
    {
        public EmitScope parent;

        public EmitContext context;
        
        Hashtable vars = new Hashtable();
        int closure_slot_id = 0; // Index for next free slot in closures array
        
        public LocalBuilder closure_array_local; // Holder for closure array
        
        public TypeBuilder type_builder;
        public ILGenerator ig;
        // Constructor
        public ILGenerator cig;
        
        public bool IsBlock;

        public EmitScope(EmitContext c)
        {
            context = c;
        }
        
        internal int CreateClosureSlot()
        {
            return closure_slot_id++;
        }

        internal int ClosureLength()
        {
            return closure_slot_id;
        }
        
        internal Variable MarkVariable(uint vid)
        {
            return GetAndCreateVariable(vid, true);
        }
        
        private Variable GetAndCreateVariable(uint vid, bool create)
        {
            Variable v = vars[vid] as Variable;
            if(v == null) {
                if(IsBlock) {
                    Variable dv = GetMethodScope().vars[vid] as Variable;
                    if(dv != null) {
                        if(create) {
                            dv.IsDynamic = true;
                            dv.closure_slot = GetMethodScope().CreateClosureSlot();
                        } else if(!dv.IsDynamic) {
                            throw new Exception("bug: missing dynamic data for dynamic var: " + context.id2name(vid));
                        }
                        return dv;
                    }
                }
                if(create) {
                    v = new Variable();
                    vars[vid] = v;
                    return v;
                } else {
                    throw new Exception("Local not found: " + context.id2name(vid));
                }
            }
            return v;
        }

        internal Variable GetVariable(uint vid)
        {
            return GetAndCreateVariable(vid, false);
        }

        // Find the parent method scope for a (possibly nested) block
        internal EmitScope GetMethodScope()
        {
            EmitScope es = this;
            while(es != null) {
                if(!es.IsBlock)
                    break;
                es = es.parent;
            }
            return es;
        }
    }
    
    enum EmitState
    {
        // Finding block (closure) variables, creating slots/locals, types etc.
        RESOLVING,
        // Emitting IL code
        EMITTING
    }
    
    class EmitContext
    {
        NetRuby ruby;

        AssemblyBuilder assembly_builder;
        bool save;
        string filename;
        ModuleBuilder module_builder;

        EmitScope current_scope;
        EmitState state;
        
        int unique_id = 1; // Unique ID for method/type name generation

        public bool Emitting { get { return state == EmitState.EMITTING; } }
        public bool Resolving { get { return state == EmitState.RESOLVING; } }
        
        public EmitContext(NetRuby r, string name, string fn)
        {
            filename = fn;
            save = filename != null;
            
            ruby = r;
            
            AppDomain domain = AppDomain.CurrentDomain;
            
            AssemblyName assembly_name = new AssemblyName();
            assembly_name.Name = name;
            
            assembly_builder = domain.DefineDynamicAssembly(assembly_name, 
                        save ? AssemblyBuilderAccess.RunAndSave : AssemblyBuilderAccess.Run);
            
            string module_name = name + "Module";
            
            if(save) {
                module_builder = assembly_builder.DefineDynamicModule(module_name, filename);
            } else {
                module_builder = assembly_builder.DefineDynamicModule(module_name);
            }
        }

        ILGenerator ig { get { return scope.ig; } }

        internal EmitScope scope
        {
            get { return current_scope; }
        }

        internal EmitScope CreateBlockScope(string name)
        {
            EmitScope es = CreateScope(name, typeof(RCBlock),
                    new Type[] { typeof(RThread), typeof(RBasic[]), typeof(RCBlock) },
                    new Type[] { typeof(RBasic), typeof(RBasic[]) }
            );
            
            es.IsBlock = true;

            return es;
        }
 
        internal EmitScope CreateMethodScope(string name)
        {
            EmitScope es = CreateScope(name, typeof(RCMethod),
                    new Type[] { typeof(RThread), typeof(RBasic), typeof(RBasic[]), typeof(RCBlock) },
                    new Type[] { }
            );

            return es;
        }
        
        EmitScope CreateScope(string name, Type parent, Type[] args, Type[] cargs)
        {
            EmitScope es = new EmitScope(this);
            TypeBuilder tb = module_builder.DefineType(name + "_" + GetID(),
                    TypeAttributes.Public, parent);
            
            MethodInfo mi = parent.GetMethod("Call");
            
            MethodBuilder mb = tb.DefineMethod("Call", 
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    typeof(RBasic), args 
            );
            
            tb.DefineMethodOverride(mb, mi);
            
            ConstructorBuilder cb = tb.DefineConstructor(MethodAttributes.Public, 
                    CallingConventions.Standard, cargs);
            
            es.type_builder = tb;
            es.ig = mb.GetILGenerator();
            es.cig = cb.GetILGenerator();

            // Parent constructor call
            for(int i = 0; i <= cargs.Length; i++) {
                es.cig.Emit(OpCodes.Ldarg_S, (byte)i);
            }
            es.cig.Emit(OpCodes.Call, parent.GetConstructor(cargs));
            
            return es;
        }

        internal RCMethod Compile(RNode main)
        {
            EmitScope es = CreateMethodScope("main");
            
            PushScope(es);
            
            state = EmitState.RESOLVING;
            main.Walk(this);

            state = EmitState.EMITTING;
            EmitScopeInitializer();
            main.Walk(this);

            Type main_type = CloseScope(es);
            
            if(save) {
                assembly_builder.Save(filename);
            }
            
            // Call constructor
            object method = main_type.InvokeMember(null, BindingFlags.Instance | BindingFlags.Public | 
                    BindingFlags.FlattenHierarchy | BindingFlags.CreateInstance, null, null,
                    new object[] {}
            );

            return (RCMethod)method;
        }

        internal Type CloseScope(EmitScope es)
        {
            es.cig.Emit(OpCodes.Ret);
            es.ig.Emit(OpCodes.Ret);
            
            PopScope(es);
            
            return es.type_builder.CreateType();
        }
        
        int GetID()
        {
            return unique_id++;
        }

        // Hack, until we have real symbols
        internal string id2name(uint id)
        {
            return ruby.id2name(id);
        }
        
        internal void PushScope(EmitScope es)
        {
            es.parent = current_scope;
            current_scope = es;
        }

        internal void PopScope(EmitScope es)
        {
            if(current_scope != es) {
                throw new Exception("Unexpected scope encountered while popping scope stack");
            }
            current_scope = current_scope.parent;
        }

        // ( -- )
        internal void EmitScopeInitializer()
        {
            int len = current_scope.ClosureLength();
            if(len > 0) {
                LocalBuilder l = ig.DeclareLocal(typeof(RBasic[]));
                current_scope.closure_array_local = l;
                EmitInt(len);
                ig.Emit(OpCodes.Newarr, typeof(RBasic));
                ig.Emit(OpCodes.Stloc, l);
            }
        }

        // ( -- closure_array )
        internal void EmitLoadClosureArray()
        {
            if(current_scope.IsBlock) {
                ig.Emit(OpCodes.Ldarg_0);
                ig.Emit(OpCodes.Ldfld, typeof(RCBlock).GetField("locals"));
            } else {
                if(current_scope.closure_array_local != null) {
                    ig.Emit(OpCodes.Ldloc, current_scope.closure_array_local);
                } else {
                    ig.Emit(OpCodes.Ldnull);
                }
            }
        }
        
        // ( -- thread )
        internal void EmitLoadThread()
        {
            // Thread is always first argument
            ig.Emit(OpCodes.Ldarg_1);
        }

        // ( -- netruby )
        internal void EmitLoadRuby()
        {
            EmitLoadThread();
            ig.Emit(OpCodes.Ldfld, typeof(RThread).GetField("ruby", BindingFlags.Public | BindingFlags.Instance));
        }
        
        // ( -- string )
        internal void EmitRString(string s)
        {
            EmitLoadRuby();
            ig.Emit(OpCodes.Ldstr, s);
            ig.Emit(OpCodes.Newobj, typeof(RString).GetConstructor(
                        new Type[] { typeof(NetRuby), typeof(string) }
            ));
        }

        // ( -- num )
        internal void EmitRNum(int n)
        {
            EmitLoadRuby();
            EmitInt(n);
            ig.Emit(OpCodes.Newobj, typeof(RFixnum).GetConstructor(
                        new Type[] { typeof(NetRuby), typeof(int) }
            ));
        }

        // ( -- array )
        internal void EmitRArray(int capa)
        {
            EmitLoadRuby();
            EmitInt(capa);
            ig.Emit(OpCodes.Newobj, typeof(RArray).GetConstructor(
                        new Type[] { typeof(NetRuby), typeof(int) }
            ));
        }

        // ( idx val -- )
        internal void EmitRArraySet()
        {
            ig.Emit(OpCodes.Call, typeof(RArray).GetMethod("set_Item"));
        }

        // ( -- self )
        internal void EmitSelf()
        {
            if(scope.IsBlock) {
                ig.Emit(OpCodes.Ldarg_0);
                ig.Emit(OpCodes.Ldfld, typeof(RCBlock).GetField("self"));
            } else {
                ig.Emit(OpCodes.Ldarg_2);
            }
        }

        // ( -- block )
        internal void EmitBlockArg()
        {
            if(scope.IsBlock)
                ig.Emit(OpCodes.Ldarg_3);
            else
                ig.Emit(OpCodes.Ldarg_S, (byte)4);
        }
        
        // ( -- nil )
        internal void EmitNil()
        {
            EmitLoadRuby();
            ig.Emit(OpCodes.Ldfld, typeof(NetRuby).GetField("oNil"));
        }

        // ( -- false )
        internal void EmitFalse()
        {
            EmitLoadRuby();
            ig.Emit(OpCodes.Ldfld, typeof(NetRuby).GetField("oFalse"));
        }

        // ( -- true )
        internal void EmitTrue()
        {
            EmitLoadRuby();
            ig.Emit(OpCodes.Ldfld, typeof(NetRuby).GetField("oTrue"));
        }
        
        // ( -- null )
        internal void EmitNull()
        {
            ig.Emit(OpCodes.Ldnull);
        }
        
        // ( -- string )
        internal void EmitString(uint mid)
        {
            ig.Emit(OpCodes.Ldstr, ruby.id2name(mid));
        }
       
        // ( recvr name args block -- retval )
        internal void EmitSend()
        {
            ig.Emit(OpCodes.Callvirt, typeof(RBasic).GetMethod("BasicSend"));
        }

        // ( recvr thread args block -- retval )
        internal void EmitYield()
        {
            ig.Emit(OpCodes.Callvirt, typeof(RCBlock).GetMethod("Call"));
        }

        // ( -- arr )
        internal void EmitNewArgArray(int len)
        {
            if(len <= RThread.CacheCount) {
                EmitLoadThread();
                ig.Emit(OpCodes.Ldfld, typeof(RThread).GetField("argscache" + len.ToString()));
            } else {
                EmitInt(len);
                ig.Emit(OpCodes.Newarr, typeof(RBasic));
            }
        }

        // ( x -- x x )
        internal void EmitDup()
        {
            ig.Emit(OpCodes.Dup);
        }

        // ( -- n )
        internal void EmitInt(int n)
        {
            switch(n) {
                case -1: ig.Emit(OpCodes.Ldc_I4_M1); break;
                case 0: ig.Emit(OpCodes.Ldc_I4_0); break;
                case 1: ig.Emit(OpCodes.Ldc_I4_1); break;
                case 2: ig.Emit(OpCodes.Ldc_I4_2); break;
                case 3: ig.Emit(OpCodes.Ldc_I4_3); break;
                case 4: ig.Emit(OpCodes.Ldc_I4_4); break;
                case 5: ig.Emit(OpCodes.Ldc_I4_5); break;
                case 6: ig.Emit(OpCodes.Ldc_I4_6); break;
                case 7: ig.Emit(OpCodes.Ldc_I4_7); break;
                case 8: ig.Emit(OpCodes.Ldc_I4_8); break;
                default:
                    if(n >= SByte.MinValue && n <= SByte.MaxValue) {
                        ig.Emit(OpCodes.Ldc_I4_S, (byte)n);
                    } else {
                        ig.Emit(OpCodes.Ldc_I4, n);
                    }
                    break;
            }
        }

        // ( arr idx value -- )
        internal void EmitArrayStore()
        {
            ig.Emit(OpCodes.Stelem_Ref);
        }

        // ( val -- )
        internal void EmitReturn()
        {
            ig.Emit(OpCodes.Ret);
        }
       
        internal Label DefineLabel()
        {
            return ig.DefineLabel();
        }

        internal void MarkLabel(Label l)
        {
            ig.MarkLabel(l);
        }

        // ( -- )
        internal void EmitBranch(Label l)
        {
            ig.Emit(OpCodes.Br, l);
        }

        // ( obj -- )
        internal void EmitBranchIfFalse(Label l)
        {
            ig.Emit(OpCodes.Callvirt, typeof(RBasic).GetMethod("IsTrue"));
            ig.Emit(OpCodes.Brfalse, l);
        }

        // ( obj -- )
        internal void EmitBranchIfTrue(Label l)
        {
            ig.Emit(OpCodes.Callvirt, typeof(RBasic).GetMethod("IsTrue"));
            ig.Emit(OpCodes.Brtrue, l);
        }
       
        // ( val -- !val )
        internal void EmitNot()
        {
            ig.Emit(OpCodes.Callvirt, typeof(RBasic).GetMethod("Not"));
        }

        // ( x -- )
        internal void EmitDiscard()
        {
            ig.Emit(OpCodes.Pop);
        }

        void AllocateLocal(Variable v)
        {
            if(v.local == null) {
                v.local = ig.DeclareLocal(typeof(RBasic));
            }
        }
       
        internal LocalBuilder DeclareTemp()
        {
            return ig.DeclareLocal(typeof(RBasic));
        }
        
        // Scratch local, get rid of this somehow by rearranging the emit process?
        // ( tmpval -- )
        internal LocalBuilder EmitStoreTemp(LocalBuilder local)
        {
            ig.Emit(OpCodes.Stloc, local);
            return local;
        }

        internal LocalBuilder EmitStoreTemp()
        {
            return EmitStoreTemp(DeclareTemp());
        }

        // ( -- tmpval )
        internal void EmitLoadTemp(LocalBuilder local)
        {
            ig.Emit(OpCodes.Ldloc, local);
        }
        
        // ( -- val )
        internal void EmitLoadVar(Variable v)
        {
            if(v.IsDynamic) {
                EmitLoadClosureArray();
                EmitInt(v.closure_slot);
                ig.Emit(OpCodes.Ldelem_Ref);
            } else {
                AllocateLocal(v);
                ig.Emit(OpCodes.Ldloc, v.local);
            }
        }

        // ( -- )
        internal void EmitStoreVar(Variable v)
        {
            if(v.IsDynamic) {
                LocalBuilder tmp = EmitStoreTemp();
                EmitLoadClosureArray();
                EmitInt(v.closure_slot);
                EmitLoadTemp(tmp);
                ig.Emit(OpCodes.Stelem_Ref);
            } else {
                AllocateLocal(v);
                ig.Emit(OpCodes.Stloc, v.local);
            }
        }

        // ( -- cls )
        internal void EmitLoadClassScope()
        {
            EmitLoadThread();
            ig.Emit(OpCodes.Call, typeof(RThread).GetMethod("GetClassScope"));
        }

        // ( -- )
        internal void EmitDefine(string name, Type t)
        {
            EmitLoadClassScope();
            ig.Emit(OpCodes.Ldstr, name);
            ig.Emit(OpCodes.Newobj, t.GetConstructor(new Type[] {}));
            ig.Emit(OpCodes.Call, typeof(RMetaObject).GetMethod("DefineMethod", 
                        new Type[] { typeof(string), typeof(RCMethod) }
            ));
        }

        // ( -- arg )
        internal void EmitLoadArg(int n)
        {
            if(current_scope.IsBlock) {
                ig.Emit(OpCodes.Ldarg_2);
            } else {
                ig.Emit(OpCodes.Ldarg_3);
            }
            EmitInt(n);
            ig.Emit(OpCodes.Ldelem_Ref);
        }

        // ( -- block )
        internal void EmitCreateBlock(Type t)
        {
            EmitSelf();
            EmitLoadClosureArray();
            ig.Emit(OpCodes.Newobj, t.GetConstructor(new Type[] { typeof(RBasic), typeof(RBasic[]) }));
        }

        // ( val -- )
        internal void EmitSetGlobal(string name)
        {
            LocalBuilder tmp = EmitStoreTemp();
            EmitLoadThread();
            ig.Emit(OpCodes.Ldstr, name);
            EmitLoadTemp(tmp);
            ig.Emit(OpCodes.Call, typeof(RThread).GetMethod("GlobalSet"));
        }

        // ( -- val )
        internal void EmitGetGlobal(string name)
        {
            EmitLoadThread();
            ig.Emit(OpCodes.Ldstr, name);
            ig.Emit(OpCodes.Call, typeof(RThread).GetMethod("GlobalGet"));
        }
    }

    // Internal representations of compiled method and block objects
    
    public abstract class RClosure
    {
        // The definition (constant lookup) scope of the method/block
        internal RMetaObject module_scope;

        public int arity;
    }
    
    public abstract class RCMethod : RClosure
    {
        public abstract RBasic Call(RThread th, RBasic self, RBasic[] args, RCBlock block);

        public RCMethod() { }

        internal string name;
        
        public string Name { get { return name; } }
    }

    public abstract class RCBlock : RClosure
    {
        // Captured variables
        public RBasic self;
        public RCBlock parent_block;
        public RBasic[] locals;
        
        // A block argument to blocks is allowed in newer versions of Ruby
        public abstract RBasic Call(RThread thread, RBasic[] args, RCBlock block);

        public RCBlock(RBasic self, RBasic[] locals) 
        {
            this.self = self;
            this.locals = locals;
        }
    }
    
    // Node walkers
    
    internal partial class RNode 
    {
        internal virtual void Walk(EmitContext ec)
        {
            throw new NotSupportedException("bug: Walk not supported: " + this.GetType().Name);
        }
    }

    internal partial class RNBlock
    {
        internal override void Walk(EmitContext ec)
        {
            // Linear scan of linked list for O(1) stack space
            for(RNode n = this; n != null; ) {
                if(n is RNBlock) {
                    RNode current = n.head;
                    current.Walk(ec);
                    n = n.next;
                    if(ec.Emitting && n != null) {
                        // Discard previous result, block returns last value generated    
                        ec.EmitDiscard();
                    }
                } else {
                    throw new NotSupportedException("bug: not supported block tail: " + n.GetType().Name);
                }
                
            }
        }
    }

    internal partial class RNArray
    {
        internal override void Walk(EmitContext ec)
        {
            if(ec.Emitting) {
                ec.EmitRArray(alen);
            }
            
            int i = 0;
            for(RNode n = this; n != null; ) {
                if(n is RNArray) {
                    if(n.head != null) {
                        if(ec.Emitting) {
                            ec.EmitDup();
                            ec.EmitInt(i);
                        }
                        n.head.Walk(ec);
                        if(ec.Emitting) {
                            ec.EmitRArraySet();
                        }
                    }
                    n = n.next;
                    i++;
                } else {
                    // n.Walk(ec);
                    // break;
                    throw new NotSupportedException("bug: array has tail of type " + n.GetType().Name);
                }
            }
        }
    }

    internal partial class RNNewLine
    {
        internal override void Walk(EmitContext ec)
        {
            next.Walk(ec);
        }
    }

    internal partial class RNCall
    {
        internal Type block; // Generated type of the block attached to the call
        
        // ( argsarr -- argsarr )
        internal static void EmitStoreArg(EmitContext ec, int i, RNode val) {
            ec.EmitDup(); // arr
            ec.EmitInt(i); // idx
            val.Walk(ec); // val
            ec.EmitArrayStore();
        }

        // ( argsarr -- argsarr )
        internal static void EmitArgsArray(EmitContext ec, RNArray a)
        {
            int i = 0;
            while(a != null) {
                EmitStoreArg(ec, i, a.head);
                a = (RNArray)a.next;
                i++;
            }
        }
        
        internal override void Walk(EmitContext ec)
        {
            if(ec.Resolving) {
                if(recv != null)
                    recv.Walk(ec);
                if(args != null)
                    args.Walk(ec);
            }
            if(ec.Emitting) {
                RNArray a = (RNArray)args;
                // rcver
                if(recv != null) {
                    recv.Walk(ec);
                } else {
                    ec.EmitSelf();
                }
                // name
                ec.EmitString(mid);
                
                // args
                int len = a != null ? a.alen : 0;
                ec.EmitNewArgArray(len);
                
                EmitArgsArray(ec, a);
                
                // block
                if(block != null) {
                    ec.EmitCreateBlock(block);
                } else {
                    ec.EmitNull();
                }

                ec.EmitSend();
            }
        } 
    }
   
    internal partial class RNYield
    {
        internal override void Walk(EmitContext ec)
        {
            if(ec.Resolving)
                if(stts != null)
                    stts.Walk(ec);
            if(ec.Emitting) {
                // rcver
                ec.EmitBlockArg();

                // thread
                ec.EmitLoadThread();

                // args
                RNArray a = stts as RNArray;
                int len = a != null ? a.alen : (stts != null ? 1 : 0);
                ec.EmitNewArgArray(len);
                
                if(a != null) {
                    RNCall.EmitArgsArray(ec, a);
                } else if(stts != null) {
                    RNCall.EmitStoreArg(ec, 0, stts);
                }
                
                // block
                ec.EmitNull();
                
                ec.EmitYield();
            }
        }
    }

    internal partial class RNIter
    {
        EmitScope scope; // New scope for the block
        
        internal override void Walk(EmitContext ec)
        {
            if(ec.Resolving) {
                scope = ec.CreateBlockScope("block");
            }

            ec.PushScope(scope);

            if(ec.Emitting)
                ec.EmitScopeInitializer();
            
            // The parser should produce a more comfortable format for this
            if(var is RNDAsgn) {
                RNDefn.WalkArg(ec, 0, var.vid);
            } else {
                RNArray args = var != null ? (RNArray)((RNMAsgn)var).head : null;
            
                int i = 0;
                for(RNode n = args; n != null; ) {
                    RNDAsgn a = (RNDAsgn)n.head;
                    RNDefn.WalkArg(ec, i, a.vid);
                    n = n.next;
                    i++;
                }
            }
            
            body.Walk(ec);
            
            if(ec.Emitting) {
                Type t = ec.CloseScope(scope);

                RNCall call = (RNCall)iter;
                call.block = t;

                call.Walk(ec);
            } else {
                ec.PopScope(scope);

                iter.Walk(ec);
            }
        }
    }

    internal partial class RNDefn
    {
        EmitScope scope; // New scope for the method
        
        internal static void WalkArg(EmitContext ec, int i, uint vid)
        {
            if(ec.Resolving) {
                Variable v = ec.scope.MarkVariable(vid);
                v.IsArgument = true;
            }
            // Move argument from array to locals, required due to the cache used
            if(ec.Emitting) {
                Variable v = ec.scope.GetVariable(vid);
                ec.EmitLoadArg(i);
                ec.EmitStoreVar(v);
            }
        }
        
        internal override void Walk(EmitContext ec)
        {
            string name = ec.id2name(mid);
            if(ec.Resolving) {
                scope = ec.CreateMethodScope(name);
            }
            
            ec.PushScope(scope);
           
            if(ec.Emitting)
                ec.EmitScopeInitializer();
            
            RNScope sc = (RNScope)defn;
            RNode n = sc.next;
            RNArgs args = (RNArgs)n.head;
            RNode body = n.next;

            int argc = args.cnt;
            for(int i = 0; i < argc; i++) {
                // First two locals in table are $~ and $_
                uint vid = sc.tbl[i + 2];
                WalkArg(ec, i, vid);
            }
            
            // Methods can have no body
            if(body != null)
                body.Walk(ec);
            
            if(ec.Emitting) {
                Type t = ec.CloseScope(scope);
                ec.EmitDefine(name, t);
                ec.EmitNil(); // Return value
            } else {
                ec.PopScope(scope);
            }
        }
    }

    internal partial class RNReturn
    {
        internal override void Walk(EmitContext ec)
        {
            stts.Walk(ec);
            if(ec.Emitting)
                ec.EmitReturn();
        }
    }
    
    internal partial class RNVarBase
    {
        Variable v;

        internal override void Walk(EmitContext ec)
        {
            // Due to the slightly weird RNode inheritance hierarchy
            bool is_load = this is RNLVar || this is RNDVar;
            
            if(!(is_load || this is RNDAsgn || this is RNLAsgn)) {
                throw new NotSupportedException("bug: variable type not supported: " + this.GetType().Name);
            }

            if(ec.Resolving) {
                v = ec.scope.MarkVariable(vid);
            }

            if(!is_load)
                val.Walk(ec);
            
            if(ec.Emitting) {
                if(is_load) {
                    Variable v = ec.scope.GetVariable(vid);
                    ec.EmitLoadVar(v);
                } else {
                    ec.EmitDup();
                    ec.EmitStoreVar(v);
                }
            }
        }
    }

    internal partial class RNWhile
    {
        internal override void Walk(EmitContext ec)
        {
            if(ec.Resolving) {
                cond.Walk(ec);
                body.Walk(ec);
            }
            
            if(ec.Emitting) {
                Label end_label = ec.DefineLabel();
                Label start_label = ec.DefineLabel();
                
                ec.MarkLabel(start_label);
                cond.Walk(ec);

                if(this is RNUntil)
                    ec.EmitBranchIfTrue(end_label);
                else
                    ec.EmitBranchIfFalse(end_label);

                body.Walk(ec);
                // TODO return value in local
                ec.EmitDiscard();
                ec.EmitBranch(start_label);

                ec.MarkLabel(end_label);
                
                ec.EmitNull();
            }
        }
    }
    
    internal partial class RNStr
    {
        internal override void Walk(EmitContext ec)
        {
            if(ec.Emitting)
                ec.EmitRString(lit.ToString());
        }
    }
   
    internal partial class RNLit
    {
        internal override void Walk(EmitContext ec)
        {
            if(ec.Emitting) {
                if(lit is int) {
                    ec.EmitRNum((int)lit);
                } else {
                    throw new NotSupportedException("bug: literal type not supported: " + lit.GetType().Name);
                }
            }
        }
    }
     
    internal partial class RNTrue
    {
        internal override void Walk(EmitContext ec)
        {
            if(ec.Emitting)
                ec.EmitTrue();
        }
    }

    internal partial class RNFalse
    {
        internal override void Walk(EmitContext ec)
        {
            if(ec.Emitting)
                ec.EmitFalse();
        }
    }

    internal partial class RNNil
    {
        internal override void Walk(EmitContext ec)
        {
            if(ec.Emitting)
                ec.EmitNil();
        }
    }

    internal partial class RNIf
    {
        internal override void Walk(EmitContext ec)
        {
            if(ec.Resolving) {
                cond.Walk(ec);
                body.Walk(ec);
                if(nd_else != null)
                    nd_else.Walk(ec);
            }
            if(ec.Emitting) {
                Label else_label = ec.DefineLabel();
                Label end_label = ec.DefineLabel();
                
                cond.Walk(ec);
                ec.EmitBranchIfFalse(else_label);

                // TODO rearrange the body and else clause in this case
                if(body != null) {
                    body.Walk(ec);
                    ec.EmitDiscard();
                }
                
                if(nd_else != null) {
                    ec.EmitBranch(end_label);
                }
                    
                ec.MarkLabel(else_label);

                if(nd_else != null) {
                    nd_else.Walk(ec);
                    ec.EmitDiscard();
                }

                ec.MarkLabel(end_label);

                ec.EmitNull();
            }
        }
    }

    internal partial class RNNot
    {
        internal override void Walk(EmitContext ec)
        {
            body.Walk(ec);
            if(ec.Emitting)
                ec.EmitNot();
        }
    }

    internal partial class RNAnd
    {
        internal override void Walk(EmitContext ec)
        {
            if(ec.Emitting) {
                Label skip_label = ec.DefineLabel();

                nd1st.Walk(ec);
                ec.EmitDup();
                LocalBuilder first = ec.EmitStoreTemp();
                ec.EmitBranchIfFalse(skip_label);
                
                nd2nd.Walk(ec);
                ec.EmitDup();
                LocalBuilder second = ec.EmitStoreTemp();
                ec.EmitBranchIfFalse(skip_label);
                
                ec.EmitLoadTemp(second);
                ec.EmitStoreTemp(first);
                
                ec.MarkLabel(skip_label);
                ec.EmitLoadTemp(first);
            } else {
                nd1st.Walk(ec);
                nd2nd.Walk(ec);
            }
        }
    }

    internal partial class RNOr
    {
        internal override void Walk(EmitContext ec)
        {
            if(ec.Emitting) {
                Label skip_label = ec.DefineLabel();

                nd1st.Walk(ec);
                ec.EmitDup();
                LocalBuilder first = ec.EmitStoreTemp();
                ec.EmitBranchIfTrue(skip_label);
                
                nd2nd.Walk(ec);
                ec.EmitDup();
                LocalBuilder second = ec.EmitStoreTemp();
                ec.EmitBranchIfTrue(skip_label);
                
                ec.EmitLoadTemp(second);
                ec.EmitStoreTemp(first);
                
                ec.MarkLabel(skip_label);
                ec.EmitLoadTemp(first);
            } else {
                nd1st.Walk(ec);
                nd2nd.Walk(ec);
            }
        }
    }

    internal partial class RNGAsgn
    {
        internal override void Walk(EmitContext ec)
        {
            val.Walk(ec);
            if(ec.Emitting) {
                ec.EmitDup();
                ec.EmitSetGlobal(ec.id2name(vid));
            }
        }
    }

    internal partial class RNGVar
    {
        internal override void Walk(EmitContext ec)
        {
            if(ec.Emitting)
                ec.EmitGetGlobal(ec.id2name(vid));
        }
    }
}

// vim:et:sts=4:sw=4
