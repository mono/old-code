thisdir := mbas2
SUBDIRS := 

all2:
	$(MAKE) PROFILE=net_2_0 all
	
include ../build/rules.make

#uncomment to enable some debug stuff
#DEBUG_FLAGS=/define:MCS_DEBUG

BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/net_2_0_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/net_2_0_bootstrap/mcs.exe $(DEBUG_FLAGS)

BUILT_SOURCES = cs-parser.cs

EXTRA_DISTFILES = cs-parser.jay

PROGRAM = mbas2.exe
PROGRAM_COMPILE = $(BOOT_COMPILE)

CLEAN_FILES = y.output *.exe *.mdb

cs-parser.cs: cs-parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ctv < $(topdir)/jay/skeleton.cs $< > jay-tmp.out && mv jay-tmp.out $@

include ../build/executable.make

clean-local: clean-net_2_0_bootstrap
clean-net_2_0_bootstrap:
	cd .. && $(MAKE) PROFILE=net_2_0_bootstrap clean

bootstrap_libs = mscorlib.dll System.dll System.Xml.dll Mono.CompilerServices.SymbolWriter.dll
bootstrap_libfiles = $(bootstrap_libs:%=$(topdir)/class/lib/net_2_0_bootstrap/%)

$(bootstrap_libfiles): bootstrap-libs
	@:
.PHONY: bootstrap-libs
bootstrap-libs:
	cd .. && $(MAKE) PROFILE=net_2_0_bootstrap all

$(PROGRAM): $(bootstrap_libfiles)

wc:
	wc -l $(all_sources)


