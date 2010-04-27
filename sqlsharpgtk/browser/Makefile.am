browserdir=$(pkglibdir)
browser_SCRIPTS = Mono.Data.SqlSharp.DatabaseBrowser.dll
EXTRA_DIST = $(browser_sources) $(browser_sources_in)
CLEANFILES = Mono.Data.SqlSharp.DatabaseBrowser.dll
DISTCLEANFILES = AssemblyInfo.cs Makefile.in

browser_sources_in = AssemblyInfo.cs.in
browser_generated_sources = $(browser_sources_in:.in=)

browser_sources = IMetaData.cs \
MetaTable.cs \
MetaTableCollection.cs \
MetaTableColumn.cs \
MetaTableColumnCollection.cs \
MetaView.cs \
MetaViewCollection.cs \
MetaProcedure.cs \
MetaProcedureCollection.cs \
MetaProcedureArgument.cs \
MetaProcedureArgumentCollection.cs \
SchemaBrowser.cs \
OracleMetaData.cs \
SqlMetaData.cs \
SqliteMetaData.cs \
SybaseMetaData.cs \
PostgreSqlMetaData.cs \
FirebirdMetaData.cs \
MySqlMetaData.cs

browser_build_sources = $(addprefix $(srcdir)/, $(browser_sources))
browser_build_sources += $(browser_generated_sources)

Mono.Data.SqlSharp.DatabaseBrowser.dll: $(browser_build_sources) 
	$(MCS) -target:library -out:$@ $(browser_build_sources) -r:System.Data.dll

# Install Unstable Mono Libraries (see configure.ac)

install-data-hook:
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		$(INSTALL) -c -m 0755 $$ASM $(DESTDIR)$(pkglibdir); \
	done;

uninstall-hook:
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		rm -f $(DESTDIR)$(pkglibdir)/`basename $$ASM`; \
	done;

