SUBDIRS = man resources src
bin_SCRIPTS = monoskel
EXTRA_DIST = monoskel.in

REWRITE = \
	sed -e "s|\@prefix\@|$(prefix)|g"       \
	    -e "s|\@pkglibdir\@|$(pkglibdir)|g" \
	    -e "s|\@bindir\@|$(bindir)|g"		\
	    -e "s|\@MONO\@|$(MONO)|g" 

$(bin_SCRIPTS): $(srcdir)/monoskel.in Makefile
	$(REWRITE) $(srcdir)/monoskel.in > $@.tmp
	mv $@.tmp $@

CLEANFILES = \
	monoskel

DISTCLEANFILES = \
	configure \
	config.guess \
	config.sub \
	Makefile.in \
	install-sh \
	aclocal.m4 \
	INSTALL \
	COPYING \
	missing

