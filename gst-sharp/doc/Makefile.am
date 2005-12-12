if ENABLE_MONODOC
TARGETS = gst-sharp-docs.zip gst-sharp-docs.tree gst-sharp-docs.source
DOCDIR = `$(MONODOC) --get-sourcesdir`

install-data-local:
	install -m 644 $(srcdir)/gst-sharp-docs.source `monodoc --get-sourcesdir`
	install -m 644 $(srcdir)/gst-sharp-docs.tree `monodoc --get-sourcesdir`
	install -m 644 $(srcdir)/gst-sharp-docs.zip `monodoc --get-sourcesdir`
else
TARGETS = 
DOCDIR = $(datadir)
endif

noinst_DATA = $(TARGETS)

ASSEMBLER = $(MONODOC) --assemble
UPDATER = $(MONODOC) --update

gst-sharp-docs.zip gst-sharp-docs.tree: $(srcdir)/en/*/*.xml $(srcdir)/en/*.xml
	$(ASSEMBLER) --ecma $(srcdir)/en -o gst-sharp-docs

update:
		$(UPDATER) ../gst-sharp/gst-sharp.dll -o ./en -f

CLEANFILES = gst-sharp-docs.zip gst-sharp-docs.tree

EXTRA_DIST = gst-sharp-docs.source gst-sharp-docs.tree gst-sharp-docs.zip

dist-hook:
	mkdir -p $(distdir)/en
	cp $(srcdir)/en/*.xml $(distdir)/en/
	mkdir -p $(distdir)/en/Gst
	cp $(srcdir)/en/Gst/*.xml $(distdir)/en/Gst/

