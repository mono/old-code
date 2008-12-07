SUBDIRS = engine class extras tools scripts man

EXTRA_DIST = monodoc.png monodoc.svg monodoc.pc mono.snk

pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = monodoc.pc

dist-hook:
	mkdir $(distdir)/ecma334
	cp $(srcdir)/ecma334/*xml $(distdir)/ecma334
