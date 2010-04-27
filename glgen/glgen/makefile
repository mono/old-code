MCS=mcs
MONO=mono

GLGEN_SRC_FILES = CodeGenerator.cs \
		  GenBase.cs \
		  Parameters.cs \
		  SymbolTable.cs \
		  ConstGen.cs \
		  Parser.cs \
		  FuncGen.cs \
		  IGeneratable.cs \
		  Statistics.cs \
		  Writer.cs


all: glgen.exe

glgen.exe: $(GLGEN_SRC_FILES)
	$(MCS) -g --unsafe -o glgen.exe -r System.Xml $(GLGEN_SRC_FILES)
	cp glgen.exe ../bin

clean:
	-rm -r glgen.exe
