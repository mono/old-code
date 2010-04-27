##
## mphoto makefile
##

##
## please use the csc-compatible options in this file
## as it is intded to be used as is on win32 as well.

CSC=mcs -g

# CSFLAGS = /d:NO_MULTITHREAD /d:USE_EXIF_THUMBS
#CSFLAGS = /d:HAVE_LIBEOG /d:NO_MULTITHREAD /d:USE_EXIF_THUMBS
#CSFLAGS = /d:HAVE_LIBEOG /d:USE_EXIF_THUMBS
CSFLAGS = /d:USE_EXIF_THUMBS
CSUNSAFE = /unsafe

MPHOTO_SOURCES = \
	main.cs \
	MphotoToplevel.cs \

LIB_SOURCES = \
	IconList.cs \
	Util.cs \
	IImageCollection.cs \
	IImageRepository.cs \
	IImageImporter.cs \
	ISearchableRepository.cs \
	DbImageRepository.cs \
	DbImageCollection.cs \
	DbProvider.cs \
	ImageItem.cs \
	DirectoryImageImporter.cs \
	Thumbnailer.cs \
	IIconListAdapter.cs \
	CollectionIconListAdapter.cs \
	DirImageRepository.cs \
	DirImageCollection.cs \
	EogImageView.cs \
	EogUiImage.cs \
	SimpleSearchIconListAdapter.cs\
	ImageInfoTreeView.cs \
	ImageInfoTreeStore.cs \
	CollectionsTreeView.cs \
	CollectionsTreeStore.cs \
	KeywordsWidget.cs \
	KeywordsTreeStore.cs \
	IconListPopup.cs

EXIF_SOURCES = \
	libexif-wrapper.cs

GTK_ASSEMBLIES= \
	-pkg:gtk-sharp

GNOME_ASSEMBLIES = \
	-pkg:glade-sharp -pkg:gnome-sharp

SYSTEM_ASSEMBLIES = \
	/r:System.Drawing.dll \
	/r:System.Data.dll \
	/r:Mono.Data.SqliteClient.dll

ASSEMBLIES = $(GTK_ASSEMBLIES) $(GNOME_ASSEMBLIES) $(SYSTEM_ASSEMBLIES)

RESOURCES_IMAGES = \
	/resource:loading.png \
	/resource:invalid.png \

RESOURCES_OTHER = \
	/resource:mphoto.glade \
	/resource:mphoto-sqlite.sql

RESOURCES = $(RESOURCES_IMAGES) $(RESOURCES_OTHER)

all: mphoto.exe

# mphoto-eog.exe test-exif.exe # mphoto.exe 

mphoto: mphoto.exe

mphoto-eog: mphoto-eog.exe

mphoto.exe: $(MPHOTO_SOURCES) $(LIB_SOURCES) $(EXIF_SOURCES) mphoto.glade mphoto-sqlite.sql
	$(CSC) $(CSFLAGS) $(CSUNSAFE) /out:$@ $(ASSEMBLIES) $(MPHOTO_SOURCES) $(LIB_SOURCES) $(EXIF_SOURCES) $(RESOURCES)

mphoto-eog.exe: $(MPHOTO_SOURCES) $(LIB_SOURCES) $(EXIF_SOURCES) mphoto.glade mphoto-sqlite.sql
	$(CSC) $(CSFLAGS) /d:HAVE_LIBEOG $(CSUNSAFE) /out:$@ $(ASSEMBLIES) $(MPHOTO_SOURCES) $(LIB_SOURCES) $(EXIF_SOURCES) $(RESOURCES)

test-exif.exe: test-exif.cs libexif-wrapper.cs
	$(CSC) /unsafe /out:$@ $^

gtkmphoto.exe: GtkMphoto.cs $(LIB_SOURCES) $(EXIF_SOURCES)
	$(CSC) $(CSUNSAFE) $(CSFLAGS) /d:NO_EXIF /out:$@ GtkMphoto.cs $(LIB_SOURCES) $(EXIF_SOURCES) $(GTK_ASSEMBLIES) $(SYSTEM_ASSEMBLIES) $(RESOURCES_IMAGES)

dist:
	(cd ..; tar czvf mphoto.tar.gz --exclude='*~' --exclude='*.exe' mphoto)

clean:
	rm -f *.exe *.il *.exe.o

t: mphoto.exe
	mono --debug mphoto.exe i