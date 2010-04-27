eflsharpdir=$(bindir)
eflsharp_SCRIPTS = efl_sharp_test.exe
eflsharp_sources_in = AssemblyInfo.cs.in
eflsharp_generated_sources = $(eflsharp_sources_in:.in=)
eflsharp_sources = efl_sharp_test.cs
eflsharp_build_sources = $(addprefix $(srcdir)/, $(eflsharp_sources))
#build_sources = $(addprefix $(srcdir)/, $(FILES))
eflsharp_build_sources += $(eflsharp_generated_sources)



ewlsharpdir=$(bindir)
ewlsharp_SCRIPTS = ewl_sharp_test.exe                   
ewlsharp_sources_in = AssemblyInfo.cs.in
ewlsharp_generated_sources = $(ewlsharp_sources_in:.in=)
ewlsharp_sources = ewl_sharp_test.cs
ewlsharp_build_sources = $(addprefix $(srcdir)/, $(ewlsharp_sources))
#build_sources = $(addprefix $(srcdir)/, $(FILES))
ewlsharp_build_sources += $(ewlsharp_generated_sources)

DLL = $(top_builddir)/src/lib/efl-sharp.dll

efl_sharp_test.exe: $(eflsharp_build_sources)
	$(MCS) -warn:0 -r:$(DLL) $(embedded) -out:$@ $< data.cs

ewl_sharp_test.exe: $(ewlsharp_build_sources)
	$(MCS) -warn:0 -r:$(DLL) $(embedded) -out:$@ $< data.cs

all: efl_sharp_test.exe ewl_sharp_test.exe

EXTRA_DIST = $(eflsharp_sources) $(eflsharp_sources_in)
CLEANFILES = efl_sharp_test.exe ewl_sharp_test.exe
	
