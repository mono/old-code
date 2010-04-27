ASSEMBLY_NAME=objc-sharp
ASSEMBLY=$(ASSEMBLY_NAME).dll
TARGET=$(ASSEMBLY)
noinst_DATA=$(ASSEMBLY) objc-sharp.snk

CLEANFILES=$(ASSEMBLY) AssemblyInfo.cs

sources = \
	Bridge.cs \
	NativeRepresentation.cs \
	VarargStack.cs \
	Mach.cs \
	NativeMember.cs \
	ObjCMessaging.cs

EXTRA_DIST=$(sources) \
	AssemblyInfo.cs \
	objc-sharp.snk

build_sources = $(addprefix $(srcdir)/, $(sources))

$(ASSEMBLY): $(build_sources)
	$(MCS) /unsafe /out:$(ASSEMBLY) /target:library $(build_sources)
