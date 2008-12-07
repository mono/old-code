EXTRA_DIST = 			\
	mdassembler.in		\
	mdcs2ecma.in		\
	mdnormalizer.in		\
	mdvalidater.in		\
	mdoc.in           \
	mdoc-subcommand.in \
	mod.in			\
	monodocer.in		\
	monodocs2html.in	\
	monodocs2slashdoc.in

MDOC_SUBCOMMANDS = \
	mdoc-assemble \
	mdoc-export-html \
	mdoc-export-msxdoc \
	mdoc-update \
	mdoc-validate

bin_SCRIPTS = 			\
	mdassembler		\
	mdcs2ecma		\
	mdoc \
	mdnormalizer		\
	mdvalidater		\
	mod			\
	monodocer		\
	monodocs2html		\
	monodocs2slashdoc	\
	$(MDOC_SUBCOMMANDS)

CLEANFILES = $(bin_SCRIPTS)

$(MDOC_SUBCOMMANDS) : mdoc-subcommand.in
	for script in $(MDOC_SUBCOMMANDS) ; do \
		cmd=`echo $$script | sed 's/mdoc-//'` ; \
		echo "cmd=$$cmd" ; \
		cat $< | sed "s/@SUBCOMMAND@/$$cmd/g" > $$script ; \
	done

