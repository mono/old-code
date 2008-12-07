sourcesdir = $(libdir)/monodoc/sources

dist-hook:
	-cp $(srcdir)/*zip $(srcdir)/*tree $(srcdir)/*source $(distdir)

install:
	-$(INSTALL_DATA) $(srcdir)/*zip $(srcdir)/*tree $(srcdir)/*source $(DESTDIR)$(sourcesdir)

