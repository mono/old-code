if ENABLE_TESTS
TESTS = run-test
endif

MCS_FLAGS = -debug
NUNIT_FLAGS = @MONO_NUNIT_LIBS@

ASSEMBLY_NAME = entagged-tests
ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_CSFILES = $(srcdir)/EntaggedTest.cs

NUNIT_TESTER_NAME = ConsoleUi
NUNIT_TESTER = $(NUNIT_TESTER_NAME).exe
NUNIT_TESTER_CSFILES = $(srcdir)/$(NUNIT_TESTER_NAME).cs

$(ASSEMBLY): $(ASSEMBLY_CSFILES)
	$(MCS) $(MCS_FLAGS) $(NUNIT_FLAGS) -out:$@ -target:library -r:$(top_builddir)/src/entagged-sharp.dll $(ASSEMBLY_CSFILES)

$(NUNIT_TESTER): $(NUNIT_TESTER_CSFILES)
	$(MCS) $(MCS_FLAGS) -out:$@ $(NUNIT_FLAGS) $(NUNIT_TESTER_CSFILES)

run-test: $(NUNIT_TESTER) $(ASSEMBLY)
	MONO_PATH="../src" mono --debug $(NUNIT_TESTER) $(ASSEMBLY)

CLEANFILES = $(ASSEMBLY) $(NUNIT_TESTER) TestResult.xml
DISTCLEANFILES = *.mdb Makefile.in *.dll *.exe

