all: Mono.Languages.Logo.dll test-target

clean:
	rm -f Mono.Languages.Logo.dll
	$(MAKE) -C test clean

MCS=mcs

RUNTIME_SOURCES = \
	Mono.Languages.Logo.Runtime/CTSMessageTarget.cs \
	Mono.Languages.Logo.Runtime/DefaultArgumentCountAttribute.cs \
	Mono.Languages.Logo.Runtime/LogoBinder.cs \
	Mono.Languages.Logo.Runtime/LogoContext.cs \
	Mono.Languages.Logo.Runtime/LogoMessageTarget.cs \
	Mono.Languages.Logo.Runtime/Funcs.cs \
	Mono.Languages.Logo.Runtime/IMessageStore.cs \
	Mono.Languages.Logo.Runtime/IMessageStoreCollection.cs \
	Mono.Languages.Logo.Runtime/IMessageTarget.cs \
	Mono.Languages.Logo.Runtime/ITypedMessageStore.cs \
	Mono.Languages.Logo.Runtime/MessageInfo.cs \
	Mono.Languages.Logo.Runtime/MessageNotSupportedException.cs \
	Mono.Languages.Logo.Runtime/PassContextAttribute.cs \
	Mono.Languages.Logo.Runtime/TypedMessageInfo.cs \
	Mono.Languages.Logo.Runtime/TypedMessageArgument.cs \
	#
	
COMPILER_SOURCES = \
	Mono.Languages.Logo.Compiler/ArgumentInfo.cs \
	Mono.Languages.Logo.Compiler/Compiler.cs \
	Mono.Languages.Logo.Compiler/CSharpCompiler.cs \
	Mono.Languages.Logo.Compiler/Element.cs \
	Mono.Languages.Logo.Compiler/ElementType.cs \
	Mono.Languages.Logo.Compiler/Function.cs \
	Mono.Languages.Logo.Compiler/IndentingWriter.cs \
	Mono.Languages.Logo.Compiler/InstructionList.cs \
	Mono.Languages.Logo.Compiler/Interpreter.cs \
	Mono.Languages.Logo.Compiler/Parser.cs \
	Mono.Languages.Logo.Compiler/Token.cs \
	Mono.Languages.Logo.Compiler/TokenList.cs \
	Mono.Languages.Logo.Compiler/TokenType.cs \
	Mono.Languages.Logo.Compiler/Tokenizer.cs \
	#

SOURCES = \
	$(RUNTIME_SOURCES) \
	$(COMPILER_SOURCES) \
	#

Mono.Languages.Logo.dll: $(SOURCES)
	$(MCS) /target:library /out:Mono.Languages.Logo.dll $(SOURCES)

test-target:
	$(MAKE) -C test

