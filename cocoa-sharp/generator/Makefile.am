ASSEMBLY_NAME=generator
ASSEMBLY=$(ASSEMBLY_NAME).exe
noinst_DATA=generator-run.stamp
	    
EXTRA_DIST= \
	custom/* \
	header-gen/*.cs \
	macho-gen/*.cs \
	gen-out/*.cs \
	Generator.cs \
	mapping.xml \
	typeconversion.xml \
	generator-run.stamp \
	generator.xml

CLEANFILES=$(ASSEMBLY) 
DISTCLEANFILES=

sources = \
	header-gen/Main.cs \
	header-gen/Element.cs \
	header-gen/HeaderCategory.cs \
	header-gen/HeaderEnum.cs \
	header-gen/HeaderInterface.cs \
	header-gen/HeaderMethod.cs \
	header-gen/HeaderProtocol.cs \
	header-gen/HeaderStruct.cs \
	header-gen/Main.cs \
	macho-gen/DylibCommand.cs \
	macho-gen/ICommand.cs \
	macho-gen/LoadCommand.cs \
	macho-gen/MachOCategory.cs \
	macho-gen/MachOClass.cs \
	macho-gen/MachOFile.cs \
	macho-gen/MachOIvar.cs \
	macho-gen/MachOMethod.cs \
	macho-gen/MachOProtocol.cs \
	macho-gen/MachOType.cs \
	macho-gen/Module.cs \
	macho-gen/Section.cs \
	macho-gen/SegmentCommand.cs \
	macho-gen/SymbolTable.cs \
	gen-out/Array.cs \
	gen-out/BitField.cs \
	gen-out/Category.cs \
	gen-out/Class.cs \
	gen-out/Configuration.cs \
	gen-out/Const.cs \
	gen-out/Enum.cs \
	gen-out/ExternalFunction.cs \
	gen-out/ExternalGlobal.cs \
	gen-out/Ivar.cs \
	gen-out/Method.cs \
	gen-out/ObjCClassInspector.cs \
	gen-out/OutputElement.cs \
	gen-out/Pointer.cs \
	gen-out/Protocol.cs \
	gen-out/Struct.cs \
	gen-out/Type.cs \
	gen-out/TypeUsage.cs \
	gen-out/WriteCS.cs \
	Generator.cs 

build_sources = $(addprefix $(srcdir)/, $(sources)) 

distclean:
	@if test $(addprefix $(srcdir)/, Generator.cs) != "./Generator.cs"; then \
	  rm -Rf Generator.cs; \
	fi
	@if test $(addprefix $(srcdir)/, macho-gen) != "./macho-gen"; then \
	  rm -Rf macho-gen; \
	fi
	@if test $(addprefix $(srcdir)/, header-gen) != "./header-gen"; then \
	  rm -Rf header-gen; \
	fi
	@if test $(addprefix $(srcdir)/, gen-out) != "./gen-out"; then \
	  rm -Rf gen-out; \
	fi
	@if test $(addprefix $(srcdir)/, custom) != "./custom"; then \
	  rm -Rf custom; \
	fi
	@if test $(addprefix $(srcdir)/, generator.xml) != "./generator.xml"; then \
	  rm -Rf generator.xml; \
	fi
	@if test $(addprefix $(srcdir)/, mapping.xml) != "./mapping.xml"; then \
	  rm -Rf mapping.xml; \
	fi
	@if test $(addprefix $(srcdir)/, typeconversion.xml) != "./typeconversion.xml"; then \
	  rm -Rf typeconversion.xml; \
	fi
generator-run.stamp: $(ASSEMBLY)
	@if test $(addprefix $(srcdir)/, Generator.cs) != "./Generator.cs"; then \
	  cp -R $(addprefix $(srcdir)/, Generator.cs) Generator.cs; \
	fi
	@if test $(addprefix $(srcdir)/, macho-gen) != "./macho-gen"; then \
	  cp -R $(addprefix $(srcdir)/, macho-gen) macho-gen; \
	fi
	@if test $(addprefix $(srcdir)/, header-gen) != "./header-gen"; then \
	  cp -R $(addprefix $(srcdir)/, header-gen) header-gen; \
	fi
	@if test $(addprefix $(srcdir)/, gen-out) != "./gen-out"; then \
	  cp -R $(addprefix $(srcdir)/, gen-out) gen-out; \
	fi
	@if test $(addprefix $(srcdir)/, custom) != "./custom"; then \
	  cp -R $(addprefix $(srcdir)/, custom) custom; \
	fi
	@if test $(addprefix $(srcdir)/, generator.xml) != "./generator.xml"; then \
	  cp $(addprefix $(srcdir)/, generator.xml) generator.xml; \
	fi
	@if test $(addprefix $(srcdir)/, mapping.xml) != "./mapping.xml"; then \
	  cp -f $(addprefix $(srcdir)/, mapping.xml) mapping.xml; \
	fi
	@if test $(addprefix $(srcdir)/, typeconversion.xml) != "./typeconversion.xml"; then \
	  cp -f $(addprefix $(srcdir)/, typeconversion.xml) typeconversion.xml; \
	fi
	(cd .. ; mono generator/generator.exe -xml:generator/generator.xml) && touch $(srcdir)/generator-run.stamp

$(ASSEMBLY): $(build_sources) 
	$(CSC) /unsafe /out:$(ASSEMBLY) $(build_sources)
