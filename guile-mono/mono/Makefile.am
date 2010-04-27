schemelibdir = $(pkgdatadir)/$(VERSION)

INCLUDES = \
	-I.					\
	-I$(srcdir)				\
	-I$(top_srcdir)				\
	$(WARN_CFLAGS)				\
	$(GUILE_MONO_CFLAGS)			\
	$(GUILE_CFLAGS)				\
	$(GWRAP_CFLAGS)				\
	-DGUILE_GOBJECT_DIR=\""$(schemelibdir)"\" \
	-Werror -Wno-unused

ETAGS_ARGS = --regex='/SCM_\(GLOBAL_\)?\(G?PROC\|G?PROC1\|SYMBOL\|VCELL\|CONST_LONG\).*\"\([^\"]\)*\"/\3/' \
   --regex='/[ \t]*SCM_[G]?DEFINE1?[ \t]*(\([^,]*\),[^,]*/\1/'

guilemoduledir = $(datadir)/guile/mono

guilemodule_DATA = guile-mono.scm mono.scm

lib_LTLIBRARIES = libguile-mono.la

libguile_mono_la_SOURCES = \
	guile-mono-module.c		\
	guile-mono-module.h		\
	guile-mono.c

libguile_mono_la_LIBADD = \
	$(GUILE_MONO_LIBS) -lgc		\
	$(GUILE_LIBS)			\
	$(GWRAP_LIBS)

libguile_mono_la_LDFLAGS = \
	-export-dynamic

DOT_X_FILES = \
	guile-mono-module.x

SUFFIXES = .x
.c.x:
	guile-snarf $(DEFS) $(INCLUDES) $(CPPFLAGS) $(CFLAGS) $< > $@ \
	|| { rm $@; false; }

guile-mono.h guile-mono.c guile-mono.html: guile-mono.gwp
	FLAVOR=gnome guile -c \
	"(set! %load-path (cons \"$(top_srcdir)\" %load-path)) \
	 (primitive-load \"$(srcdir)/guile-mono.gwp\")"

BUILT_SOURCES = \
	guile-mono.h guile-mono.c guile-mono.html		\
	$(DOT_X_FILES)

CLEANFILES = $(BUILT_SOURCES)


EXTRA_DIST = guile-mono.scm guile-mono.gwp

