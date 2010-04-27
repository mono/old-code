ASSEMBLY_NAME=test-library
ASSEMBLY=$(ASSEMBLY_NAME).dll
DEBUG=$(ASSEMBLY).mdb
TARGET=$(ASSEMBLY)

sources= \
	ExceptionTests.cs \
	InstanceTests.cs \
	NativeMarshalTests.cs \
	StaticTests.cs

EXTRA_DIST=$(sources)

noinst_SCRIPTS = $(ASSEMBLY)

CLEANFILES=$(ASSEMBLY) $(DEBUG)

build_sources = $(addprefix $(srcdir)/, $(sources))

$(ASSEMBLY): $(build_sources)
	$(MCS) /out:$(ASSEMBLY) /target:library $(build_sources)
