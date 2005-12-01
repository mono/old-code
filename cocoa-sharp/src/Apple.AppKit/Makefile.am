TARGET = $(ASSEMBLY)
ASSEMBLY_NAME=Apple.AppKit
ASSEMBLY=$(ASSEMBLY_NAME).dll
CLEANFILES=$(ASSEMBLY) *.cs cocoa-sharp.snk
DISTCLEANFILES=AssemblyInfo.cs
noinst_DATA=$(ASSEMBLY) $(ASSEMBLY).config
EXTRA_DIST= \
	$(ASSEMBLY).config \
	AssemblyInfo.cs.in \
	*.cs

references = /r:../Apple.Foundation/Apple.Foundation.dll

sources= \
	*.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) 

$(ASSEMBLY): $(build_sources) cocoa-sharp.snk
	$(CSC) /nowarn:0114,0108 /out:$(ASSEMBLY) /target:library $(references) $(build_sources)

cocoa-sharp.snk: $(top_srcdir)/cocoa-sharp.snk
	cp $(top_srcdir)/cocoa-sharp.snk .

install-data-local:
	@if test -n '$(TARGET)'; then                                           \
	  echo "$(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS)"; \
	  $(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS) || exit 1;      \
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then                                           \
	  echo "$(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS)";       \
	  $(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS) || exit 1;    \
	fi
