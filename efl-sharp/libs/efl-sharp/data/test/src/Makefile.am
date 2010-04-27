AINTAINERCLEANFILES = Makefile.in

NAME = flower

EDJE_CC = edje_cc
EDJE_FLAGS = \
-v \
-id $(top_srcdir)/data/test/images \
-fd $(top_srcdir)/data/fonts

filesdir = $(pkgdatadir)/data/test/
files_DATA = $(NAME).edj

$(NAME).edj: Makefile $(NAME).edc
	$(EDJE_CC) $(EDJE_FLAGS) \
	$(top_srcdir)/data/test/src/$(NAME).edc \
	$(top_builddir)/data/test/src/$(NAME).edj

clean-local:
	rm -f *.edj


