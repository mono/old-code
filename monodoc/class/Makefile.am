sourcesdir = $(libdir)/monodoc/sources
cserrors_sourcesdir = $(libdir)/monodoc/sources

sources_DATA = \
	ecma334.tree ecma334.zip ecma334.source 		\
	netdocs.tree netdocs.zip netdocs.source			\
	nunit-docs.tree nunit-docs.zip nunit-docs.source	\
	Mono.tree Mono.zip Mono.source 				\
	mono-tools.tree mono-tools.zip mono-tools.source \
	Novell.source Novell.zip Novell.tree

if BUILD_CSERRORS
cserrors_sources_DATA = \
	cs-errors.tree cs-errors.zip cs-errors.source
endif

ASSEMBLER = ../engine/assembler.exe

#
# Netdocs contains all the .NET Framework documentation
#
netdocs: netdocs.tree

netdocs.tree netdocs.zip: $(srcdir)/*/en/*/*.xml $(srcdir)/*/en/*.xml
	mono --debug $(ASSEMBLER) -o netdocs			\
		--ecma $(srcdir)/corlib/en/			\
		--ecma $(srcdir)/System/en/			\
		--ecma $(srcdir)/System.Core/en/ \
		--ecma $(srcdir)/System.XML/en/			\
		--ecma $(srcdir)/System.Web/en			\
		--ecma $(srcdir)/System.Web.Services/en		\
		--ecma $(srcdir)/System.Data/en			\
		--ecma $(srcdir)/System.DirectoryServices/en	\
		--ecma $(srcdir)/System.Drawing/en		\
		--ecma $(srcdir)/System.Security/en		\
		--ecma $(srcdir)/Microsoft.Build.Framework/en	\
		--ecma $(srcdir)/Microsoft.Build.BuildEngine/en	\
		--ecma $(srcdir)/Microsoft.Build.Utilities/en

#
# All the Mono.* namespaces go into a single package
#
Mono.tree Mono.zip: $(srcdir)/Mono.Cairo/en/*/*.xml  $(srcdir)/Mono.GetOptions/en/*/*.xml $(srcdir)/Mono.Posix/en/*/*.xml $(srcdir)/Mono.Security/en/*/*.xml $(srcdir)/Mono.Security.Win32/en/*/*.xml $(srcdir)/Commons.Xml.Relaxng/en/*/*.xml $(srcdir)/Mono.Addins/en/*/*.xml
	mono --debug $(ASSEMBLER) --ecma $(srcdir)/Mono.Cairo/en --ecma $(srcdir)/Mono.GetOptions/en --ecma $(srcdir)/Mono.Posix/en --ecma $(srcdir)/Mono.Security.Win32/en/ --ecma $(srcdir)/Mono.Security/en/ --ecma $(srcdir)/Commons.Xml.Relaxng/en/ --ecma $(srcdir)/Mono.Addins/en/ -o Mono

#
# All the Novell namespaces we handle go into a single file (one for now).
#
Novell.tree Novell.zip: $(srcdir)/Novell.Directory.Ldap/en/*/*.xml
	mono --debug $(ASSEMBLER) --ecma $(srcdir)/Novell.Directory.Ldap/en/ -o Novell

#
# These are the NUnit documents, not sure why we have these, if NUnit is well documented
#
nunit-docs.tree nunit-docs.zip: $(srcdir)/nunit/en/*/*.xml $(srcdir)/nunit/en/*.xml
	mono --debug $(ASSEMBLER) --ecma $(srcdir)/nunit/en/ -o nunit-docs

#
# Generation of C# errors, from the mcs source tree
#
cs-errors.tree cs-errors.zip: $(srcdir)/cs-errors.config
	mono --debug $(ASSEMBLER) --error $(srcdir)/cs-errors.config -o cs-errors

#
# The ECMA C# specification, from Jon Jagger.
#
ecma334.tree ecma334.zip: $(srcdir)/../ecma334/*
	mono --debug $(ASSEMBLER) --ecmaspec $(srcdir)/../ecma334 -o ecma334

#
# Mono tool documentation, from man pages
#
mono-tools.tree mono-tools.zip: $(srcdir)/mono-tools.config
	mono --debug $(ASSEMBLER) -o mono-tools --man $(srcdir)/mono-tools.config

CLEANFILES = \
	ecma334.tree ecma334.zip 	\
	netdocs.tree netdocs.zip 	\
	nunit-docs.tree nunit-docs.zip 	\
	cs-errors.tree cs-errors.zip 	\
	mono-tools.tree mono-tools.zip \
	Mono.tree Mono.zip 		\
	Novell.tree Novell.zip		\
	tmp 

EXTRA_DIST = \
	$(sources_DATA)			\
	$(cserrors_sources_DATA)	\
	mono-tools.config

# format: assembly-name,directory,versions
# if directory=-, uses assembly-name.
UPDATE_ASSEMBLIES=\
	mscorlib,corlib,1.0,2.0                                   \
	System,-,1.0,2.0                                          \
	System.Configuration.Install,-,1.0,2.0                    \
	System.Core,-,2.0                                         \
	System.Data,-,1.0,2.0                                     \
	System.Design,-,1.0,2.0                                   \
	System.Drawing,-,1.0,2.0                                  \
	System.Runtime.Remoting,1.0,2.0                           \
	System.Runtime.Serialization.Formatters.Soap,-,1.0,2.0    \
	System.Security,-,1.0,2.0                                 \
	System.Web,-,1.0,2.0                                      \
	System.Web.Services,-,1.0,2.0                             \
	System.Windows.Forms,-,1.0,2.0                            \
	System.Xml,System.XML,1.0,2.0                             \
	Microsoft.Build.BuildEngine,-,2.0                         \
	Microsoft.Build.Framework,-,2.0                           \
	Microsoft.Build.Utilities,-,2.0                           \
	Mono.Addins,-,1.0,2.0                                     \
	Mono.Cairo,-,1.0,2.0                                      \
	Mono.GetOptions,-,1.0,2.0                                 \
	Mono.Posix,-,1.0,2.0                                      \
	Mono.Security,-,1.0,2.0                                   \
	Mono.Security.Win32,-,1.0,2.0                             \
	Npgsql,-,1.0,2.0                                          \
	nunit.core,nunit,1.0                                      \
	nunit.framework,nunit,1.0                                 \
	nunit.mocks,nunit,1.0                                     \
	nunit.util,nunit,1.0

UPDATE_COMMAND = \
	if echo $(3) | grep 1.0 > /dev/null ; then                               \
		__md__=monodocer1.exe ;                                                \
	else                                                                     \
		__md__=monodocer.exe ;                                                 \
	fi ;                                                                     \
	echo mono --debug ../tools/$$__md__ -show_exceptions -pretty -overrides  \
		-assembly:@prefix@/lib/mono/$(3)/$(1).dll -path:./$(2)/en $(MD_EXTRA); \
	mono --debug ../tools/$$__md__ -show_exceptions -pretty -overrides       \
		-assembly:@prefix@/lib/mono/$(3)/$(1).dll -path:./$(2)/en $(MD_EXTRA)

IMPORT_COMMAND = \
	mono --debug ../tools/monodocer.exe -show_exceptions -pretty -overrides  \
		-assembly:@prefix@/lib/mono/$(3)/$(1).dll -path:./$(2)/en              \
		-importecmadoc:$(ECMA_XML_DOCS) $(MD_EXTRA)

update:
	for ai in $(UPDATE_ASSEMBLIES); do                     \
		aai=(`echo $$ai | sed 's/,/ /g'`);                   \
		asm=$${aai[0]};                                      \
		if [ -n "$(ASSEMBLY)" -a "$(ASSEMBLY)" != "$$asm" ] ; then \
			continue;                                          \
		fi;                                                  \
		dir=$${aai[1]};                                      \
		if [ "$$dir" = "-" ] ; then dir=$$asm ; fi;          \
		set -- $${aai[*]} ;                                  \
		shift 2 ;                                            \
		for v ; do                                           \
			$(call UPDATE_COMMAND,$$asm,$$dir,$$v) ;           \
		done;                                                \
	done

update-delete:
	for ai in $(UPDATE_ASSEMBLIES); do                     \
		aai=(`echo $$ai | sed 's/,/ /g'`);                   \
		asm=$${aai[0]};                                      \
		if [ -n "$(ASSEMBLY)" -a "$(ASSEMBLY)" != "$$asm" ] ; then \
			continue;                                          \
		fi;                                                  \
		dir=$${aai[1]};                                      \
		if [ "$$dir" = "-" ] ; then dir=$$asm ; fi;          \
		set -- $${aai[*]} ;                                  \
		shift 2 ;                                            \
		for v ; do                                           \
			$(call UPDATE_COMMAND,$$asm,$$dir,$$v) --delete ;  \
		done;                                                \
	done

import:
	for ai in $(UPDATE_ASSEMBLIES); do                     \
		aai=(`echo $$ai | sed 's/,/ /g'`);                   \
		asm=$${aai[0]};                                      \
		if [ -n "$(ASSEMBLY)" -a "$(ASSEMBLY)" != "$$asm" ] ; then \
			continue;                                          \
		fi;                                                  \
		dir=$${aai[1]};                                      \
		if [ "$$dir" = "-" ] ; then dir=$$asm ; fi;          \
		$(call IMPORT_COMMAND,$$asm,$$dir,2.0) ;             \
	done

update-dir:
	$(call UPDATE_COMMAND,$(ASSEMBLY),$(DIR),$(VER))

import-dir:
	$(call IMPORT_COMMAND,$(ASSEMBLY),$(DIR),$(VER))

svn-rm:
	for f in `svn status | grep '^\?.*\.remove$$' |   \
			sed -r 's/^\?\s+(.*)\.remove$$/\1/'` ; do     \
		svn rm --force $$f;                             \
	done

svn-add:
	for f in `svn status | grep '^\?.*/en/' | grep -v '.remove$$' |   \
			sed -r 's/^\?\s+(.*)$$/\1/'` ; do                             \
		svn add $$f ;                                                   \
	done

dist-hook:
	find . -name '*.xml' > files
	tar cTf files - | (cd $(distdir); tar xf -)
	cp $(srcdir)/cs-errors.* $(distdir)
	rm files
