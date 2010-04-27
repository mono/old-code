export MONO_PATH := $(MONO_PATH):$(NUNIT_DIR)

GusTests_FILES = \
	$(srcdir)/AssemblyInfo.cs \
	$(srcdir)/HandlerTests.cs \
	$(srcdir)/ServiceTests.cs \
	$(srcdir)/TestUtilities.cs

GusTests_FLAGS = /target:library

all: GusTests.dll

GusTests.dll: $(GusTests_FILES)
	$(CSC) /out:$@ $(CSCFLAGS) $(GusTests_FLAGS) $(NUNIT_REF) $^ 
	
.PHONY: test
test:
if WINDOWS	
	gacutil -i $(NUNIT_DIR)/nunit.core.dll -f
	gacutil -i $(NUNIT_DIR)/nunit.framework.dll -f
endif	
	$(NUNIT) $(NUNIT_FLAGS) GusTests.dll
	
CLEAN_FILES = *.exe *.dll *.pdb *.mdb $(srcdir)/obj $(srcdir)/files

clean-local:
	rm -rf $(CLEAN_FILES)
    
