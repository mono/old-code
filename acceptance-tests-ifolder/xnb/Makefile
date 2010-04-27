CSC_DEBUGFLAGS=-debug -d:TRACE
CSC=gmcs $(CSC_DEBUGFLAGS)
MONO_DEBUGFLAGS=--debug
RUNTIME=mono $(MONO_DEBUGFLAGS)

#PKGCONFIG=pkg-config
#Xnb_PROTO_DIR=$(shell $(PKGCONFIG) --variable=xcbincludedir xcb-proto)

XMLTOOL=mono-xmltool
XCB_SCHEMA=xcb.xsd

#EXTENSIONS=bigreq randr shape xevie xvmc composite record shm xf86dri xv damage render sync xfixes dpms res xcb_types xprint glx screensaver xc_misc xproto
EXTENSIONS=bigreq randr shape xevie xvmc composite record shm xf86dri xv damage render sync xfixes dpms res xcb_types screensaver xc_misc xproto
EXTENSION_XML=$(addprefix proto/,$(addsuffix .xml, $(EXTENSIONS)))

ASSEMBLIES=xnb-generator.exe Xnb.dll nxauth.exe testsimple.exe
DEBUG=$(addsuffix .mdb, $(ASSEMBLIES))
GENSOURCES=Xnb.cs XnbTypes.cs XProto.cs XProtoTypes.cs XCMisc.cs XCMiscTypes.cs XFixes.cs XFixesTypes.cs
EXTS=Xnb XProto XCMisc
#GENSOURCES=$(addsuffix .cs,$(foreach f,$(f)Types %Proto,$(EXTS)))

all: $(ASSEMBLIES)

XNB_SOURCES=$(GENSOURCES) Connection.cs Xau.cs Authentication.cs IOVector.cs XReader.cs XWriter.cs XMarshaler.cs Extension.cs Structs.cs XResponseReader.cs XidManager.cs Auxiliary.cs Attributes.cs X11.cs

Xnb.dll: $(XNB_SOURCES)
	$(CSC) -unsafe -out:$@ -t:library -r:Mono.Posix $(XNB_SOURCES)

testsimple.exe: Xnb.dll TestSimple.cs
	$(CSC) -out:$@ -r:Xnb.dll TestSimple.cs

testserver.exe: Xnb.dll XServer.cs TestServer.cs
	$(CSC) -out:$@ -r:Mono.Posix -r:Mono.GetOptions -r:Xnb.dll XServer.cs TestServer.cs

XFixes.cs XFixesTypes.cs XFixesTypeMap: xnb-generator.exe xfixes.xml XCMiscTypeMap
	$(RUNTIME) xnb-generator.exe -o:XFixes -r:XCMisc xfixes.xml

XCMisc.cs XCMiscTypes.cs XCMiscTypeMap: xnb-generator.exe xc_misc.xml XProtoTypeMap
	$(RUNTIME) xnb-generator.exe -o:XCMisc -r:XProto xc_misc.xml

XProto.cs XProtoTypes.cs XProtoTypeMap: xnb-generator.exe xproto.xml XnbTypeMap
	$(RUNTIME) xnb-generator.exe -o:XProto -r:Xnb xproto.xml

Xnb.cs XnbTypes.cs XnbTypeMap: xnb-generator.exe xcb_types.xml TypeMap
	$(RUNTIME) xnb-generator.exe -o:Xnb xcb_types.xml

xnb-generator.exe: Generator.cs CodeWriter.cs XcbSchema.cs
	$(CSC) -out:$@ -r:Mono.GetOptions Generator.cs CodeWriter.cs XcbSchema.cs

XcbSchema.cs: XcbSchema.xsd
	xsd XcbSchema.xsd /c

#XcbSchema.xsd: xproto.xml xcb_types.xml
#	trang -I xml -O xsd xproto.xml xcb_types.xml $@

nxauth.exe: Xnb.dll XAuth.cs
	$(CSC) -out:$@ -r:Mono.GetOptions -r:Mono.Posix -r:Xnb.dll XAuth.cs



#temporary hack to make the demo work
XDPY_DEPTH=$(shell xdpyinfo | grep "depth of root window:" | sed -e 's/^.*://' | sed -e 's/ planes//')
XDPY_WID=$(shell xdpyinfo | grep "root window id:" | sed -e 's/^.*0x//')
XDPY_VIS=$(shell xdpyinfo | grep "default visual id:" | sed -e 's/^.*0x//')

.PHONY: run
run: testsimple.exe
	$(RUNTIME) testsimple.exe $(XDPY_DEPTH) $(XDPY_WID) $(XDPY_VIS)

.PHONY: validate
validate: $(EXTENSION_XML)
	$(XMLTOOL) --validate $(XCB_SCHEMA) $(EXTENSION_XML)

.PHONY: clean
clean:
	-rm $(ASSEMBLIES) $(DEBUG) $(GENSOURCES)

