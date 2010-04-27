monoskeldir=$(pkglibdir)
monoskel_SCRIPTS = monoskel.exe
EXTRA_DIST = $(monoskel_sources) $(monoskel_sources_in)
CLEANFILES = monoskel.exe
DISTCLEANFILES = AssemblyInfo.cs Makefile.in

monoskel_sources_in = AssemblyInfo.cs.in
monoskel_generated_sources = $(monoskel_sources_in:.in=)
monoskel_sources = main.cs

monoskel_build_sources = $(addprefix $(srcdir)/, $(monoskel_sources))
monoskel_build_sources += $(monoskel_generated_sources)
resources = $(srcdir)/../resources/dummy.txt

embedded= $(foreach res,$(resources), $(addprefix -resource:,$(res)),$(notdir $(res)))

monoskel.exe: $(monoskel_build_sources) $(resources)
	$(MCS) $(GTKSHARP_LIBS) $(SUPERSTRINGSHARP_LIBS) $(embedded) -out:$@ $(monoskel_build_sources)

# Install Unstable Mono Libraries (see configure.ac)

install-data-hook:
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		$(INSTALL) -c -m 0755 $$ASM $(DESTDIR)$(pkglibdir); \
	done;

uninstall-hook:
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		rm -f $(DESTDIR)$(pkglibdir)/`basename $$ASM`; \
	done;

