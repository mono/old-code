#
# The following targets are available for updating docs:
#
# update-man: updates man pages.
# 

INSTALL=install
MONO=mono
SOURCESDIR=`monodoc --get-sourcesdir`
XSLTPROC=xsltproc
LOCALE=en

# rules for new / rewritten monkeyguide
new: monohb-new.tree monohb-new.zip
	mv monohb-new.tree monohb.tree
	mv monohb-new.zip monohb.zip

monohb-new.tree monohb-new.zip: Documentation-new.xml new/$(LOCALE)/index.html $(shell find new/$(LOCALE) -name "*.html" ) $(shell find new/en -name "*.html" )
	xmllint --noout $?
	cp Documentation-new.xml Documentation.xml # assembler.exe doesn't like other names
	$(MONO) $(SOURCESDIR)/../assembler.exe --hb Documentation-new.xml --out monohb-new

Documentation-new.xml : mkmstoc-new.xsl toc2.xml
	$(XSLTPROC) --stringparam locale $(LOCALE) mkmstoc-new.xsl toc2.xml > Documentation-new.xml

new/$(LOCALE)/index.html : mkindex.xsl toc2.xml
	$(XSLTPROC) mkindex.xsl toc2.xml > $@

clean-new:
	rm -f monohb-new.tree monohb-new.zip mgrand_*


#----------------------------------------

all: monohb.tree monohb.zip

monohb.tree monohb.zip: Documentation.xml html/en/index.html $(shell find html/en -name "*.html" )
	xmllint --noout $?
	$(MONO) $(SOURCESDIR)/../assembler.exe --hb Documentation.xml --out monohb
pt-br: Documentation.pt-br.xml $(shell find html/pt-br -name "*.html" )
	xmllint --noout $?
	$(MONO) $(PLAT_SOURCESDIR)/../assembler.exe --hb Documentation.pt-br.xml --out monohb

install-new: monohb-new.tree monohb-new.zip monohb.source
	$(INSTALL) -m 644 monohb-new.tree monohb-new.zip monohb.source $(SOURCESDIR)

install: monohb.tree monohb.zip monohb.source
	$(INSTALL) -m 644 monohb.tree monohb.zip monohb.source $(SOURCESDIR)

clean:
	rm -f monohb.tree monohb.zip mgrand_*

Documentation.xml : mkmstoc.xsl toc.xml
	$(XSLTPROC) mkmstoc.xsl toc.xml > $@

html/en/index.html : mkindex.xsl toc.xml
	$(XSLTPROC) mkindex.xsl toc.xml > $@

push: monohb.tree monohb.zip
	scp monohb.* root@www.go-mono.com:/mono/lib/monodoc/sources

update-man:
	for i in /cvs/mono/man/*.[15]; do \
		target=html/en/man/`basename $$i`.html; \
		man2html $$i | sed 's/&nbsp;//g' | tidy -asxml > $$target; \
		perl -pi -e 's/^Section: M.*//;' -e 's/^Content-type.*//g;' -e 's/.*Return to Main Contents.*//;' -e 's@<a href="#index">Index</a> <a href=@@g;' -e 's@http://localhost/cgi-bin/man/man2html\?(1)\+([a-z]*)"@\2.\1.html"@g' $$target; \
	done

update-doc: update-ado

#
# This expects a `built' tree of /cvs/mono/web/web
#
update-ado:
	for i in /cvs/mono/web/{ado-net,firebird,ibmdb2,sqlclient,mysql,odbc,oledb,oracle,postgresql,sqlite,sybase,tdsclient,tds-providers}; do\
		tidy -asxml /cvs/mono/web/web/src/`basename $$i`.src > html/en/adonet/`basename $$i`.html; \
	done

update-lang:
	for i in /cvs/mono/web/{mbas,languages}; do\
		tidy -asxml /cvs/mono/web/web/src/`basename $$i`.src > html/en/languages/`basename $$i`.html; \
	done
