sourcesdir = `monodoc --get-sourcesdir`

sources_DATA = \
	mbuild.source \
	mbuild-Mono.Build.tree mbuild-Mono.Build.zip \
	mbuild-Monkeywrench.tree mbuild-Monkeywrench.zip \
	mbuild-handbook.tree mbuild-handbook.zip \
	mbuild-bundle-Core.tree mbuild-bundle-Core.zip

#	mbuild-extension.tree mbuild-extension.zip

all: $(sources_DATA)

mbuild-Mono.Build.tree mbuild-Mono.Build.zip: $(shell cat mbuild-Mono.Build.sources)
	monodoc --assemble --ecma Mono.Build -o mbuild-Mono.Build

mbuild-Monkeywrench.tree mbuild-Monkeywrench.zip: $(shell cat mbuild-Monkeywrench.sources)
	monodoc --assemble --ecma Monkeywrench -o mbuild-Monkeywrench

mbuild-handbook.tree mbuild-handbook.zip: handbook/handbook.xml $(shell cat mbuild-handbook.sources)
	monodoc --assemble --hb $< -o mbuild-handbook

#mbuild-extension.tree mbuild-extension.zip: extension/extension.xml
#	monodoc --assemble --hb $< -o mbuild-extension

handbook/handbook.xml: mkmstoc.xsl handbook/toc.xml
	xsltproc --stringparam htmlbase handbook/html/en --stringparam title "MBuild Handbook" $^ >$@

MBuildDynamic.Core-xhtml/toc.xml: $(shell cat MBuildDynamic.Core.sources)
	test -d MBuildDynamic.Core-xhtml || mkdir MBuildDynamic.Core-xhtml
	mb-bd2xhtml MBuildDynamic.Core-xhtml `cat MBuildDynamic.Core.sources`

MBuildDynamic.Core-xhtml/Core.xml: mkmstoc.xsl MBuildDynamic.Core-xhtml/toc.xml
	xsltproc --stringparam htmlbase MBuildDynamic.Core-xhtml --stringparam title "Core Bundle" $^ >$@

mbuild-bundle-Core.tree mbuild-bundle-Core.zip: MBuildDynamic.Core-xhtml/Core.xml
	monodoc --assemble --hb $< -o mbuild-bundle-Core

#extension/extension.xml: mkmstoc.xsl extension/toc.xml
#	xsltproc --stringparam htmlbase extension/html/en --stringparam title "MBuild Extender's Guide" $^ >$@

mbuild.source: mbuild.source.manual
	cp -f $< $@

update:
	monodoc --update ../Mono.Build.dll -o Mono.Build -f -p
	monodoc --update ../Monkeywrench.dll -o Monkeywrench -f -p
	mb-bundledoc ../MBuildDynamic.Core.dll MBuildDynamic.Core

merge:
	monodoc --merge-changes ~/.config/monodoc/changeset.xml `pwd`
	mv -f ~/.config/monodoc/changeset.xml ~/.config/monodoc/merged.xml

install: $(sources_DATA)
	for f in $(sources_DATA) ; do cp -f $$f $(sourcesdir) ; done
