superstringdir=$(pkglibdir)
superstring_SCRIPTS = superstring-sharp.dll
EXTRA_DIST = $(superstring_sources) $(superstring_sources_in)
CLEANFILES = superstring-sharp.dll
DISTCLEANFILES = AssemblyInfo.cs Makefile.in

superstring_sources_in = AssemblyInfo.cs.in
superstring_generated_sources = $(superstring_sources_in:.in=)
superstring_sources = SuperString.cs

superstring_build_sources = $(addprefix $(srcdir)/, $(superstring_sources))
superstring_build_sources += $(superstring_generated_sources)

superstring-sharp.dll: $(superstring_build_sources)
	$(MCS) -target:library -out:$@ $(superstring_build_sources)
