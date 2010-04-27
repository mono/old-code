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

#ifndef GEN_H
#define GEN_H

#include <Slice/Parser.h>
#include <IceUtil/OutputUtil.h>
#include <string>
#include <vector>

namespace Slice
{

class Gen : public ::IceUtil::noncopyable, public ParserVisitor
{
public:

    Gen(const std::string& argv0, const std::string& baseName, const std::string& outputDir);
    virtual ~Gen();

    bool operator!() const;

    void generate (const UnitPtr& p);

private:
    
    std::string _baseName;
    ::IceUtil::Output O;

    class ObjectVisitor : public ::IceUtil::noncopyable, public ParserVisitor
    {
    private:
        std::string _baseName;
        ::IceUtil::Output& O;

    public:
        ObjectVisitor (const std::string& bname, ::IceUtil::Output& o)
            : _baseName (bname), O (o)
        {
        }

	virtual bool visitUnitStart(const UnitPtr&);
	virtual void visitUnitEnd(const UnitPtr&);

        virtual bool visitModuleStart(const ModulePtr&);
        virtual void visitModuleEnd(const ModulePtr&);

        virtual void visitClassDecl(const ClassDeclPtr&);
	virtual bool visitClassDefStart(const ClassDefPtr&);
	virtual void visitClassDefEnd(const ClassDefPtr&);

        virtual bool visitStructStart(const StructPtr&);
        virtual void visitStructEnd(const StructPtr&);

	virtual bool visitExceptionStart(const ExceptionPtr&);
	virtual void visitExceptionEnd(const ExceptionPtr&);

	virtual void visitDataMember(const DataMemberPtr&);
        virtual void visitOperation(const OperationPtr&);

        virtual void visitSequence(const SequencePtr&);
        virtual void visitDictionary(const DictionaryPtr&);
        virtual void visitEnum(const EnumPtr&);
        virtual void visitConst(const ConstPtr&);

    private:
        void writeLayoutAttribute();
        void writeConstructor(const ContainedPtr&);
        DictionaryList _classDicts;
        SequenceList _classSeqs;
    };
};

} // namespace Slice

#endif
