/*
 Copyright(C) 2001-2002 arton

 Permission is granted for use, copying, modification, distribution,
 and distribution of modified versions of this work as long as the
 above copyright notice is included.
*/

using System;
using System.Collections;
using System.Reflection;

namespace NETRuby
{
    public class QUndef
    {
        private QUndef()
        {
        }
        static QUndef val = new QUndef();
        static QUndef Value
        {
            get { return val; }
        }
        public override string ToString()
        {
            return "Qundef";
        }
    }
    
    public class RNil : RBasic, ICloneable
    {
        internal RNil(NetRuby rb)
            : base(rb, rb.cObject)
        {
        }
        public override bool IsSpecialConst
        {
            get { return true; }
        }
        public override string ToString()
        {
            return "nil";
        }
        public override RArray ToArray()
        {
            return new RArray(ruby, true);
        }
        public override RFloat ToFloat()
        {
            return new RFloat(ruby, 0.0);
        }
        public override RInteger ToInteger()
        {
            return new RFixnum(ruby, 0);
        }
        public override int id
        {
            get { return Qnil; }
        }
        public override RMetaObject Class
        {
            get { return ruby.cNilClass; }
        }
        public override object Inspect()
        {
            return "nil";
        }
        public override bool IsNil
        {
            get { return true; }
        }
        public override bool IsTrue()
        {
            return false;
        }
        public override RBasic Not()
        {
            return ruby.oTrue;
        }
        public override object And(object o)
        {
            return false;
        }
        public override object Or(object o)
        {
            return RTest(o);
        }
        public override object Xor(object o)
        {
            return RTest(o);
        }
        public object Clone()
        {
            throw new eTypeError("can't clone nil");
        }
        static internal object nil_to_s(RBasic r, params object[] args)
        {
            return String.Empty;
        }
        internal void Init(NetRuby rb)
        {
            BindingFlags bf = BindingFlags.InvokeMethod
                | BindingFlags.Static | BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy
                | BindingFlags.Instance;
            rb.cNilClass = rb.DefineClass("NilClass", rb.cObject);
            Type obj = typeof(RNil);
            rb.cNilClass.DefineMethod("to_i", obj.GetMethod("ToInteger", bf));
            rb.cNilClass.DefineMethod("to_a", obj.GetMethod("ToArray", bf));
            rb.cNilClass.DefineMethod("to_s", new RMethod(nil_to_s), 0);
            rb.cNilClass.DefineMethod("&", new RMethod(ruby_and), 1);
            rb.cNilClass.DefineMethod("|", new RMethod(ruby_or), 1);
            rb.cNilClass.DefineMethod("^", new RMethod(ruby_xor), 1);
            rb.cNilClass.DefineMethod("nil?", new RMethod(ruby_true), 0);

            rb.ClassOf(rb.cNilClass).UndefMethod("new");
            rb.DefineGlobalConst("NIL", null);
        }
    }

    public class RTrue : RBool
    {
        internal RTrue(NetRuby rb) :
            base(rb, true)
        {
        }
        public override int id
        {
            get { return Qtrue; }
        }
        public override string ToString()
        {
            return "true";
        }
        public override RMetaObject Class
        {
            get { return ruby.cTrueClass; }
        }
        public override object Inspect()
        {
            return "true";
        }
        
        public override RBasic Not()
        {
            return ruby.oFalse;
        }
        
        public override object And(object o)
        {
            return RTest(o);
        }
        public override object Or(object o)
        {
            return true;
        }
        public override object Xor(object o)
        {
            return !RTest(o);
        }
        internal void Init(NetRuby rb)
        {
            rb.cTrueClass = rb.DefineClass("TrueClass", rb.cObject);
            rb.cTrueClass.DefineMethod("&", new RMethod(ruby_and), 1);
            rb.cTrueClass.DefineMethod("|", new RMethod(ruby_or), 1);
            rb.cTrueClass.DefineMethod("^", new RMethod(ruby_xor), 1);
            rb.ClassOf(rb.cTrueClass).UndefMethod("new");
            rb.DefineGlobalConst("TRUE", true);
        }
    }

    public class RFalse : RBool
    {
        internal RFalse(NetRuby rb) :
            base(rb, false)
        {
        }
        public override int id
        {
            get { return Qfalse; }
        }
        public override bool IsTrue()
        {
            return false;
        }
        public override string ToString()
        {
            return "false";
        }
        public override object Inspect()
        {
            return "false";
        }
        public override object And(object o)
        {
            return false;
        }
        public override object Or(object o)
        {
            return RTest(o);
        }
        public override object Xor(object o)
        {
            return RTest(o);
        }
        internal void Init(NetRuby rb)
        {
            rb.cFalseClass = rb.DefineClass("FalseClass", rb.cObject);
            rb.cFalseClass.DefineMethod("&", new RMethod(ruby_and), 1);
            rb.cFalseClass.DefineMethod("|", new RMethod(ruby_or), 1);
            rb.cFalseClass.DefineMethod("^", new RMethod(ruby_xor), 1);
            rb.ClassOf(rb.cFalseClass).UndefMethod("new");
            rb.DefineGlobalConst("FALSE", false);
        }
    }
    
    public class RBool : RBasic, ICloneable
    {
        internal RBool(NetRuby rb, bool f)
            : base(rb, rb.cObject)
        {
            val = f;
        }
        bool val;
        public bool ToBool()
        {
            return val;
        }
        public override bool IsSpecialConst
        {
            get { return true; }
        }
        public object Clone()
        {
            throw new eTypeError("can't clone Bool");
        }
    }

}

// vim:et:sts=4:sw=4
