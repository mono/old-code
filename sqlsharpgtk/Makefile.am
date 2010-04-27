#SUBDIRS = man resources src
SUBDIRS = browser sqlsharpgtk
bin_SCRIPTS = sqlsharpgtk.sh
EXTRA_DIST = sqlsharpgtk.sh.in 

REWRITE = \
	sed -e "s|\@prefix\@|$(prefix)|g"       \
	    -e "s|\@pkglibdir\@|$(pkglibdir)|g" \
	    -e "s|\@bindir\@|$(bindir)|g"		\
	    -e "s|\@MONO\@|$(MONO)|g" 

$(bin_SCRIPTS): $(srcdir)/sqlsharpgtk.sh.in Makefile
	$(REWRITE) $(srcdir)/sqlsharpgtk.sh.in > $@.tmp
	mv $@.tmp $@

CLEANFILES = \
	sqlsharpgtk.sh

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

