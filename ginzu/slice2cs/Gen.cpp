// **********************************************************************
//
// Copyright (c) 2003
// ZeroC, Inc.
// Billerica, MA, USA
//
// Copyright (c) 2003
// Sparkle Studios, LLC
// San Francisco, CA, USA
//
// All Rights Reserved.
//
// Ice is free software; you can redistribute it and/or modify it under
// the terms of the GNU General Public License version 2 as published by
// the Free Software Foundation.
//
// **********************************************************************

#include <IceUtil/Functional.h>
#include <Gen.h>

using namespace std;
using namespace Slice;
using namespace IceUtil;

Slice::Gen::Gen(const std::string& argv0,
                const std::string& baseName,
                const std::string& outputDir)
    : _baseName (baseName)
{
    string file = baseName + ".cs";
    if (outputDir.length() != 0)
        file = outputDir + '/' + file;

    O.open (file.c_str());
    if (!O) {
        cerr << argv0 << ": can't open `" << file << "' for writing: " << strerror(errno) << endl;
        throw;
    }
}

Slice::Gen::~Gen()
{
}

bool
Slice::Gen::operator!() const
{
    return !O;
}

void
Slice::Gen::generate(const UnitPtr& p)
{
    ObjectVisitor objectVisitor (_baseName, O);
    p->visit (&objectVisitor);
}

static std::string
scoped_replace (const std::string& scoped, const std::string& joiner = ".")
{
    std::string result;

    string::size_type pos;
    string::size_type start = 0;
    do {
        string piece;
        pos = scoped.find(':', start);

        if (pos == string::npos) {
            piece = scoped.substr(start);
        } else {
            assert (scoped[pos + 1] == ':');
            piece = scoped.substr(start, pos - start);
            start = pos + 2;
        }

        if (!result.empty()) result += joiner;
        result += piece;

    } while (pos != string::npos);

    return result;
}


string
typeToCsString(const TypePtr& type, bool useProxyAttribute = true)
{
    static const char* builtinTable[] =
    {
	"byte",
	"bool",
	"short",
	"int",
	"long",
	"float",
	"double",
	"string",
	"Ice.Object",
	"[Ice.AsProxy] Ice.Object", //Prx",
	"Ice.LocalObject"
    };

    BuiltinPtr builtin = BuiltinPtr::dynamicCast(type);
    if(builtin) {
        if (builtin->kind() == 9 && !useProxyAttribute) // ObjectPrx
            return "Ice.Object";
	return builtinTable[builtin->kind()];
    }

    SequencePtr seq = SequencePtr::dynamicCast(type);
    if(seq)
        return typeToCsString(seq->type()) + "[]";

    ClassDeclPtr cl = ClassDeclPtr::dynamicCast(type);
    if(cl)
	return scoped_replace(cl->scoped());
	    
    ProxyPtr proxy = ProxyPtr::dynamicCast(type);
    if(proxy)
    {
        string s = "";
        if (useProxyAttribute)
            s += "[Ice.AsProxy] ";
	return s + scoped_replace(proxy->_class()->scoped()); // + "Prx";
    }
	    
    ContainedPtr contained = ContainedPtr::dynamicCast(type);
    if(contained)
    {
	return scoped_replace(contained->scoped());
    }

    EnumPtr en = EnumPtr::dynamicCast(type);
    if(en)
    {
	return scoped_replace(en->scoped());
    }
	    
    return "???";
}

string
returnTypeToCsString(const TypePtr& type)
{
    if(!type)
    {
	return "void";
    }

    BuiltinPtr builtin = BuiltinPtr::dynamicCast(type);
    if (builtin && builtin->kind() == 9) // ObjectPrx
        return "[return: Ice.AsProxy] Ice.Object";

    ProxyPtr proxy = ProxyPtr::dynamicCast(type);
    if (proxy)
        return "[return: Ice.AsProxy] " + typeToCsString(type, false);

    return typeToCsString(type);
}

string
inputTypeToCsString(const TypePtr& type)
{
    static const char* inputBuiltinTable[] =
    {
	"byte",
	"bool",
	"short",
	"int",
	"long",
	"float",
	"double",
	"string",
	"Ice.Object",
	"Ice.Object", //Prx",
	"Ice.LocalObject"
    };

    BuiltinPtr builtin = BuiltinPtr::dynamicCast(type);
    if(builtin)
    {
	return inputBuiltinTable[builtin->kind()];
    }

    SequencePtr seq = SequencePtr::dynamicCast(type);
    if(seq)
        return typeToCsString(seq->type()) + "[]";

    ClassDeclPtr cl = ClassDeclPtr::dynamicCast(type);
    if(cl)
    {
	return scoped_replace(cl->scoped());
    }
	    
    ProxyPtr proxy = ProxyPtr::dynamicCast(type);
    if(proxy)
    {
	return "[Ice.AsProxy] " + scoped_replace(proxy->_class()->scoped()); // + "Prx";
    }
	    
    EnumPtr en = EnumPtr::dynamicCast(type);
    if(en)
    {
	return scoped_replace(en->scoped());
    }
	    
    ContainedPtr contained = ContainedPtr::dynamicCast(type);
    if(contained)
    {
	return scoped_replace(contained->scoped());
    }

    return "???";
}

string
outputTypeToCsString(const TypePtr& type)
{
    static const char* outputBuiltinTable[] =
    {
	"out byte",
	"out bool",
	"out short",
	"out int",
	"out long",
	"out float",
	"out double",
	"out string",
	"out Ice.Object",
	"out Ice.Object", //Prx",
	"out Ice.LocalObject"
    };
    
    BuiltinPtr builtin = BuiltinPtr::dynamicCast(type);
    if(builtin)
    {
	return outputBuiltinTable[builtin->kind()];
    }

    SequencePtr seq = SequencePtr::dynamicCast(type);
    if(seq)
        return "out " + typeToCsString(seq->type()) + "[]";

    ClassDeclPtr cl = ClassDeclPtr::dynamicCast(type);
    if(cl)
    {
	return "out " + scoped_replace(cl->scoped());
    }
	    
    ProxyPtr proxy = ProxyPtr::dynamicCast(type);
    if(proxy)
    {
	return "[Ice.AsProxy] out " + scoped_replace(proxy->_class()->scoped()); // + "Prx";
    }
	    
    ContainedPtr contained = ContainedPtr::dynamicCast(type);
    if(contained)
    {
	return "out " + scoped_replace(contained->scoped());
    }

    return "???";
}


bool
Slice::Gen::ObjectVisitor::visitUnitStart(const UnitPtr& p)
{
    O << "//" << nl;
    O << "// " << _baseName << ".cs" << nl;
    O << "// Automatically generated by slice2cs; do not edit!" << nl;
    O << "//" << nl;
    O << nl;

    return true;
}

void
Slice::Gen::ObjectVisitor::visitUnitEnd(const UnitPtr& p)
{
}

bool
Slice::Gen::ObjectVisitor::visitModuleStart(const ModulePtr& p)
{
    O << "namespace " << p->name() << " {";
    O.inc();
    O << nl;

    return true;
}

void
Slice::Gen::ObjectVisitor::visitModuleEnd(const ModulePtr& p)
{
    O.dec();
    O << nl;
    O << "}" << nl;
}

void
Slice::Gen::ObjectVisitor::visitClassDecl(const ClassDeclPtr& p)
{
    // nothing;
}

bool
Slice::Gen::ObjectVisitor::visitClassDefStart(const ClassDefPtr& p)
{
    _classDicts.clear();
    _classSeqs.clear();

    ClassList bases = p->bases();

    O << nl;
    if (p->isInterface()) {
        O << "public interface ";
        O << p->name();

        if (bases.empty()) {
            O << nl << "{";
            O.inc();
            return true;
        }

        O << " : ";
    } else {
        writeLayoutAttribute();
        O << nl << "public abstract class ";
        O << p->name();

        if (p->isLocal())
            O << " : Ice.LocalObject";
        else
            O << " : Ice.Object";

        if (bases.empty()) {
            O << nl << "{";
            O.inc();
            return true;
        }

        O << ", ";
    }

    ClassList::const_iterator q = bases.begin();
    while (q != bases.end())
    {
        if (p->isInterface() && !(*q)->isInterface()) {
            std::cerr << "Interface " << p->scoped() << " inherits from non-interface " << (*q)->scoped() << "!" << std::endl;
            throw;
        }
        O << scoped_replace((*q)->scoped());
        if(++q != bases.end())
            O << ", ";
    }

    O << nl << "{";
    O.inc();
    return true;
}

void
Slice::Gen::ObjectVisitor::visitClassDefEnd(const ClassDefPtr& p)
{
    writeConstructor(p);

    O.dec();
    O << nl << "}" << nl;
}

void
Slice::Gen::ObjectVisitor::visitOperation(const OperationPtr& p)
{
    string name = p->name();

    TypePtr ret = p->returnType();
    string retS = returnTypeToCsString(ret);
    ContainerPtr container = p->container();
    ClassDefPtr cl = ClassDefPtr::dynamicCast(container);

    bool amd = !cl->isLocal() && (cl->hasMetaData("amd") || p->hasMetaData("amd"));

    string attr;
    string params;

    if (!cl->isLocal()) {
        if (p->mode() == Operation::Normal)
//            attr = "[Ice.OperationModeAttribute]";
            attr = "";          // don't output the attribute if it's Normal
        else if (p->mode() == Operation::Nonmutating)
            attr = "[Ice.OperationModeAttribute(Ice.OperationMode.Nonmutating)]";
        else if (p->mode() == Operation::Idempotent)
            attr = "[Ice.OperationModeAttribute(Ice.OperationMode.Idempotent)]";
    }

    ParamDeclList paramList = p->parameters();
    for(ParamDeclList::const_iterator q = paramList.begin(); q != paramList.end(); ++q)
    {
        string paramName = (*q)->name();
        TypePtr type = (*q)->type();
        bool isOutParam = (*q)->isOutParam();
        string typeString;
        if (isOutParam) {
            typeString = outputTypeToCsString(type);
        } else {
            typeString = inputTypeToCsString(type);
        }

        if (q != paramList.begin()) {
            params += ", ";
        }

        params += typeString;
        params += " ";
        params += paramName;
    }

    if (!cl->isLocal()) {
        if (!paramList.empty()) {
            params += ", ";
        }
        params += "Ice.Current __current";
    }

    O << nl;
    O << attr;
    O << nl;
    if (!cl->isInterface())
        O << "public abstract ";
    O << retS << " " << name << " (" << params << ");";
    O << nl;
}

bool
Slice::Gen::ObjectVisitor::visitStructStart(const StructPtr& p)
{
    _classDicts.clear();
    _classSeqs.clear();

    O << nl;
    writeLayoutAttribute();
    O << nl;
    O << "public struct " << p->name() << " {";
    O.inc();
}

void
Slice::Gen::ObjectVisitor::visitStructEnd(const StructPtr& p)
{
    writeConstructor(p);

    O.dec();
    O << nl;
    O << "}" << nl;
}

bool
Slice::Gen::ObjectVisitor::visitExceptionStart(const ExceptionPtr& p)
{
    _classDicts.clear();
    _classSeqs.clear();

    O << nl;
    writeLayoutAttribute();
    O << nl;
    O << "public class " << p->name() << " : System.Exception {";
    O.inc();
}

void
Slice::Gen::ObjectVisitor::visitExceptionEnd(const ExceptionPtr& p)
{
    writeConstructor(p);

    O.dec();
    O << nl;
    O << "}" << nl;
}

void
Slice::Gen::ObjectVisitor::visitDataMember(const DataMemberPtr& p)
{
    O << nl << "public " << typeToCsString(p->type()) << " " << p->name() << ";";
}

void
Slice::Gen::ObjectVisitor::visitDictionary(const DictionaryPtr& p)
{
    O << nl;
    O << "public class " << p->name() << " : Ice.Dictionary {";
    O.inc();
    O << nl;
    O << "public " << p->name() << " ()";
    O.inc();
    O << nl;
    O << " : base (typeof(" << typeToCsString(p->keyType())
      << "), typeof(" << typeToCsString(p->valueType()) << "))";
    O.dec();
    O << nl;
    O << "{ }";
    O.dec();
    O << nl;
    O << "}";
}

void
Slice::Gen::ObjectVisitor::visitSequence(const SequencePtr& p)
{
    O << nl;
    O << "// ";
    O << typeToCsString(p->type()) << "[] " << p->name() << ";";
}

void
Slice::Gen::ObjectVisitor::visitEnum(const EnumPtr& p)
{
    EnumeratorList enums = p->getEnumerators();
    string basetype;

    if (enums.size() < 128)
        basetype = "byte";
    else if (enums.size() < 32768)
        basetype = "short";
    else
        basetype = "int";

    O << nl;
    O << "public enum " << p->name() << " : " << basetype << " {";
    O.inc();

    for (EnumeratorList::const_iterator q = enums.begin();
         q != enums.end();
         q++)
    {
        if (q != enums.begin())
            O << ",";
        O << nl;
        O << (*q)->name();
    }

    O.dec();
    O << nl;
    O << "}";
    O << nl;
}

void
Slice::Gen::ObjectVisitor::visitConst(const ConstPtr& p)
{
    O << nl;
    O << "public static const " << typeToCsString(p->type()) << " " << p->name() << " = " << p->value() << ";";
}

void
Slice::Gen::ObjectVisitor::writeLayoutAttribute()
{
    O << "[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]";
}

void
Slice::Gen::ObjectVisitor::writeConstructor(const ContainedPtr& p)
{
#if 0
    if (_classDicts.size() == 0)
        return;

    O << "public " << p->name() << " ()" << nl;
    O << "{";
    O.inc();

    for (DictionaryList::const_iterator q = _classDicts.begin();
         q != _classDicts.end();
         q++)
    {
        O << nl;
        O << (*q)->name() << " = new Ice.Dictionary ();";
    }

    O.dec();
    O << nl;
    O << "}";
#endif
}
