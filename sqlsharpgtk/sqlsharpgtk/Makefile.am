sqlsharpgtkdir=$(pkglibdir)
sqlsharpgtk_SCRIPTS = sqlsharpgtk.exe sqlsharpgtk.exe.config
EXTRA_DIST = $(sqlsharpgtk_sources) $(sqlsharpgtk_sources_in)
CLEANFILES = sqlsharpgtk.exe
DISTCLEANFILES = AssemblyInfo.cs Makefile.in

sqlsharpgtk_sources_in = AssemblyInfo.cs.in
sqlsharpgtk_generated_sources = $(sqlsharpgtk_sources_in:.in=)

sqlsharpgtk_sources = sqlsharpgtk.cs SqlEditorSharp.cs LoginDialog.cs DataSourceConnection.cs DataSourceConnectionCollection.cs \
DataGrid.cs DataTree.cs ConnectionString.cs AboutDialog.cs ComboHelper.cs \
OutputResults.cs ExecuteOutputType.cs BatchExecuteMode.cs QueryMode.cs EditorTab.cs \
FileSelectionDialog.cs SqlSharpDataAdapter.cs MultiResultsGrid.cs DataGridColumn.cs

SQLSHARP_GTK_LIBS = $(GTKSHARP_LIBS) /r:System.Drawing.dll \
/r:System.Data.dll /r:Mono.Data.dll \
/r:../browser/Mono.Data.SqlSharp.DatabaseBrowser.dll 

sqlsharpgtk_build_sources = $(addprefix $(srcdir)/, $(sqlsharpgtk_sources))
sqlsharpgtk_build_sources += $(sqlsharpgtk_generated_sources)

sqlsharpgtk.exe: $(sqlsharpgtk_build_sources) 
	$(MCS) -target:exe -out:$@ $(sqlsharpgtk_build_sources) $(SQLSHARP_GTK_LIBS)

# Install Unstable Mono Libraries (see configure.ac)

install-data-hook:
	for ASM in $(INSTALLED_ASSEMBLIES) sqlsharpgtk.exe.config; do \
		$(INSTALL) -c -m 0755 $$ASM $(DESTDIR)$(pkglibdir); \
	done;

uninstall-hook:
	for ASM in $(INSTALLED_ASSEMBLIES) sqlsharpgtk.exe.config; do \
		rm -f $(DESTDIR)$(pkglibdir)/`basename $$ASM`; \
	done;

