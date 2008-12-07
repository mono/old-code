monodoc_DATA = \
	mdoc.exe \
	monodocer.exe \
	monodocer1.exe \
	monodocs2html.exe \
	monodocs2slashdoc.exe \
	NDesk.Options.dll

CLEANFILES = $(monodoc_DATA)
CSC=gmcs

mdoc_sources = \
	$(srcdir)/mdoc.cs

mdoc_refs = \
	monodocer.exe \
	monodocs2slashdoc.exe \
	monodocs2html.exe \
	NDesk.Options.dll \
	../engine/assembler.exe \
	../engine/normalize.exe \
	../engine/validate.exe

monodocer_sources = 	\
	$(srcdir)/monodocer.cs

monodocs2slashdoc_sources = \
	$(srcdir)/../engine/XmlDocUtils.cs \
	$(srcdir)/monodocs2slashdoc.cs

monodocs2slashdoc_refs = \
	NDesk.Options.dll

monodocs2html_sources = \
	$(srcdir)/../engine/ManifestResourceResolver.cs \
	$(srcdir)/../engine/XmlDocUtils.cs \
	$(srcdir)/monodocs2html.cs \
	$(srcdir)/XhtmlWriter.cs

monodocs2html_resources = \
	$(srcdir)/../engine/mono-ecma-css.xsl \
	$(srcdir)/../engine/mdoc-html-utils.xsl \
	$(srcdir)/../engine/mdoc-sections-css.xsl \
	$(srcdir)/defaulttemplate.xsl \
	$(srcdir)/overview.xsl \
	$(srcdir)/stylesheet.xsl

NDesk_Options_sources = \
	$(srcdir)/Options.cs

MONODOC_TEST_FILES = \
	$(srcdir)/DocTest-v1.cs \
	$(srcdir)/DocTest-v2.patch \
	$(srcdir)/TestEcmaDocs.xml \
	$(shell find $(srcdir)/DocTest/en.expected* -name \*.xml)

MONODOCS2HTML_TEST_FILES = \
	$(shell find $(srcdir)/DocTest/html.expected* -name \*.html)

EXTRA_DIST = \
	$(mdoc_sources) \
	$(monodocer_sources) \
	$(monodocs2html_sources) \
	$(monodocs2html_resources) \
	$(monodocs2slashdoc_sources) \
	$(MONODOC_TEST_FILES) \
	$(MONODOCS2HTML_TEST_FILES) \
	$(srcdir)/bsd-man-to-ecma.pl \
	$(srcdir)/bsd-man-to-exception.pl \
	$(NDesk_Options_sources)

DISTCLEANFILES = \
	*.exe.mdb \
	DocTest.cs* \
	DocTest.dll* \
	DocTest.xml

distclean-local:
	-rm -Rf DocTest/en.actual DocTest/html.actual

NDesk.Options.dll: $(NDesk_Options_sources)
	$(CSC) -debug+ -t:library -out:$@ $(NDesk_Options_sources)

mdoc.exe: $(mdoc_sources) $(mdoc_refs)
	$(CSC) -debug+ $(mdoc_sources) -out:$@ -r:System.Core.dll $(addprefix -r:, $(mdoc_refs))

monodocer.exe: $(monodocer_sources)
	$(CSC) -debug $(monodocer_sources) -out:$@ -r:Mono.GetOptions

monodocer1.exe: $(monodocer_sources)
	mcs -debug -d:NET_1_0 $(srcdir)/monodocer.cs -out:$@ -r:Mono.GetOptions

monodocs2html.exe: $(monodocs2html_sources) $(monodocs2html_resources)
	$(CSC) -debug -out:$@ -r:Mono.GetOptions \
		$(monodocs2html_sources) \
		-resource:$(srcdir)/defaulttemplate.xsl,defaulttemplate.xsl \
		-resource:$(srcdir)/../engine/mdoc-html-utils.xsl,mdoc-html-utils.xsl \
		-resource:$(srcdir)/../engine/mdoc-sections-css.xsl,mdoc-sections-css.xsl \
		-resource:$(srcdir)/../engine/mono-ecma-css.xsl,mono-ecma-css.xsl \
		-resource:$(srcdir)/overview.xsl,overview.xsl \
		-resource:$(srcdir)/stylesheet.xsl,stylesheet.xsl

monodocs2slashdoc.exe: $(monodocs2slashdoc_sources) $(monodocs2slashdoc_refs)
	$(CSC) -debug $(monodocs2slashdoc_sources) -out:$@ \
		$(addprefix -r:, $(monodocs2slashdoc_refs))

xpath.exe: xpath.cs
	$(CSC) -debug xpath.cs

DocTest.dll: 
	$(CSC) $(TEST_CSCFLAGS) -debug -unsafe -target:library -out:$@ DocTest.cs

DocTest.dll-v1: 
	-rm -f DocTest.cs
	cp $(srcdir)/DocTest-v1.cs DocTest.cs
	-rm -f DocTest.dll
	$(MAKE) TEST_CSCFLAGS=$(TEST_CSCFLAGS) DocTest.dll

DocTest.dll-v2: 
	-rm -f DocTest.cs
	cp $(srcdir)/DocTest-v1.cs DocTest.cs
	patch -p0 < $(srcdir)/DocTest-v2.patch
	-rm -f DocTest.dll
	$(MAKE) TEST_CSCFLAGS=$(TEST_CSCFLAGS) DocTest.dll

check-monodocer-update: monodocer.exe
	find $(srcdir)/DocTest/en.expected -name \*.xml -exec rm "{}" \;
	$(MAKE) DocTest.dll-v1
	mono --debug monodocer.exe -show_exceptions \
		-assembly:DocTest.dll -path:$(srcdir)/DocTest/en.expected -pretty

check-monodocer: monodocer.exe
	rm -Rf DocTest/en.actual
	$(MAKE) DocTest.dll-v1
	mono --debug monodocer.exe -show_exceptions \
		-assembly:DocTest.dll -path:DocTest/en.actual -pretty
	diff --exclude=.svn -rup $(srcdir)/DocTest/en.expected DocTest/en.actual
	mono --debug monodocer.exe -show_exceptions \
		-assembly:DocTest.dll -path:DocTest/en.actual -pretty
	diff --exclude=.svn -rup $(srcdir)/DocTest/en.expected DocTest/en.actual
	mono --debug monodocer.exe -namespace:Mono.DocTest.Generic -pretty \
		-assembly:DocTest.dll -path:DocTest/en.actual -show_exceptions 
	diff --exclude=.svn -rup $(srcdir)/DocTest/en.expected DocTest/en.actual

check-monodocer-since-update: monodocer.exe
	find $(srcdir)/DocTest/en.expected.since -name \*.xml -exec rm "{}" \;
	$(MAKE) DocTest.dll-v1
	mono --debug monodocer.exe -show_exceptions \
		-assembly:DocTest.dll -path:$(srcdir)/DocTest/en.expected.since -pretty
	$(MAKE) DocTest.dll-v2
	mono --debug monodocer.exe -show_exceptions -since:"Version 2.0" \
		-assembly:DocTest.dll -path:$(srcdir)/DocTest/en.expected.since -pretty

check-monodocer-since: monodocer.exe
	rm -Rf DocTest/en.actual
	$(MAKE) DocTest.dll-v1
	mono --debug monodocer.exe -show_exceptions \
		-assembly:DocTest.dll -path:DocTest/en.actual -pretty
	$(MAKE) DocTest.dll-v2
	mono --debug monodocer.exe -show_exceptions -since:"Version 2.0" \
		-assembly:DocTest.dll -path:DocTest/en.actual -pretty
	diff --exclude=.svn -rup $(srcdir)/DocTest/en.expected.since DocTest/en.actual

check-monodocer-importslashdoc-update: monodocer.exe
	find $(srcdir)/DocTest/en.expected.importslashdoc -name \*.xml -exec rm "{}" \;
	$(MAKE) DocTest.dll-v1 TEST_CSCFLAGS=-doc:DocTest.xml
	mono --debug monodocer.exe -show_exceptions -importslashdoc:DocTest.xml \
		-assembly:DocTest.dll -path:$(srcdir)/DocTest/en.expected.importslashdoc -pretty

check-monodocer-importslashdoc: monodocer.exe
	rm -Rf DocTest/en.actual
	$(MAKE) DocTest.dll-v1 TEST_CSCFLAGS=-doc:DocTest.xml
	mono --debug monodocer.exe -show_exceptions -import:DocTest.xml \
		-assembly:DocTest.dll -path:DocTest/en.actual -pretty
	diff --exclude=.svn -rup $(srcdir)/DocTest/en.expected.importslashdoc DocTest/en.actual

check-monodocer-importecmadoc-update: monodocer.exe
	find $(srcdir)/DocTest/en.expected.importecmadoc -name \*.xml -exec rm "{}" \;
	$(MAKE) DocTest.dll-v1
	mono --debug monodocer.exe -show_exceptions -importecmadoc:$(srcdir)/TestEcmaDocs.xml \
		'-type:System.Action`1' -type:System.AsyncCallback -type:System.Environment -type:System.Array \
		-assembly:DocTest.dll -path:$(srcdir)/DocTest/en.expected.importecmadoc -pretty

check-monodocer-importecmadoc: monodocer.exe
	rm -Rf DocTest/en.actual
	$(MAKE) DocTest.dll-v1
	mono --debug monodocer.exe -show_exceptions -import:$(srcdir)/TestEcmaDocs.xml \
		'-type:System.Action`1' -type:System.AsyncCallback -type:System.Environment -type:System.Array \
		-assembly:DocTest.dll -path:DocTest/en.actual -pretty
	diff --exclude=.svn -rup $(srcdir)/DocTest/en.expected.importecmadoc DocTest/en.actual

check-monodocs2html-update: monodocs2html.exe
	find $(srcdir)/DocTest/html.expected -name \*.html -exec rm "{}" \;
	mono --debug monodocs2html.exe \
		-source:$(srcdir)/DocTest/en.expected.importslashdoc -dest:$(srcdir)/DocTest/html.expected

check-monodocs2html: check-monodocer monodocs2html.exe
	rm -Rf DocTest/html.actual
	mono --debug monodocs2html.exe \
		-source:$(srcdir)/DocTest/en.expected.importslashdoc -dest:DocTest/html.actual
	diff --exclude=.svn -rup $(srcdir)/DocTest/html.expected DocTest/html.actual

check-md-html-dir: monodocs2html.exe
	rm -Rf DocTest/html.actual
	mono --debug monodocs2html.exe \
		-source:$(DIR) -dest:DocTest/html.actual
	diff --exclude=.svn -rup $(srcdir)/DocTest/html.expected DocTest/html.actual

check-mdoc-export-msxdoc-update:
	mono --debug monodocs2slashdoc.exe -o - \
		$(srcdir)/DocTest/en.expected.importslashdoc \
		> $(srcdir)/DocTest/msxdoc-expected.importslashdoc.xml

check-mdoc-export-msxdoc:
	mono --debug monodocs2slashdoc.exe -o - \
		$(srcdir)/DocTest/en.expected.importslashdoc \
		| diff --brief - $(srcdir)/DocTest/msxdoc-expected.importslashdoc.xml

check : check-doc-tools

check-update : check-doc-tools-update

check-doc-tools: check-monodocer-since \
	check-monodocer-importecmadoc \
	check-monodocer-importslashdoc \
	check-monodocer \
	check-monodocs2html \
	check-mdoc-export-msxdoc

check-doc-tools-update: check-monodocer-since-update \
		check-monodocer-importecmadoc-update \
		check-monodocer-importslashdoc-update \
		check-monodocer-update \
		check-monodocs2html-update \
		check-mdoc-export-msxdoc-update

clean:
	-rm DocTest.dll DocTest.dll.mdb monodocer.exe monodocer.exe.mdb
