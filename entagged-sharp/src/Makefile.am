MCS_FLAGS = -debug
ASSEMBLY_NAME = entagged-sharp
ASSEMBLY = $(ASSEMBLY_NAME).dll

entaggedsharpdir=$(pkglibdir)
entaggedsharp_SCRIPTS=$(ASSEMBLY)

APE_CSFILES = \
	$(srcdir)/Ape/MonkeyFileReader.cs \
	$(srcdir)/Ape/Util/ApeTagReader.cs \
	$(srcdir)/Ape/Util/ApeTagWriter.cs \
	$(srcdir)/Ape/Util/ApeTagCreator.cs \
	$(srcdir)/Ape/Util/MonkeyDescriptor.cs \
	$(srcdir)/Ape/MonkeyFileWriter.cs \
	$(srcdir)/Ape/Util/MonkeyHeader.cs \
	$(srcdir)/Ape/Util/MonkeyInfoReader.cs \
	$(srcdir)/Ape/Util/WavFormatHeader.cs \
	$(srcdir)/Ape/Util/WavRIFFHeader.cs

ASF_CSFILES = \
	$(srcdir)/Asf/AsfFileReader.cs \
	$(srcdir)/Asf/Util/AsfTagReader.cs \
	$(srcdir)/Asf/Util/AsfInfoReader.cs \
	$(srcdir)/Asf/Util/GUID.cs

FLAC_CSFILES = \
	$(srcdir)/Flac/FlacFileReader.cs \
	$(srcdir)/Flac/Util/FlacInfoReader.cs \
	$(srcdir)/Flac/Util/FlacTagReader.cs \
	$(srcdir)/Flac/Util/FlacTagWriter.cs \
	$(srcdir)/Flac/Util/FlacTagCreator.cs \
	$(srcdir)/Flac/Util/MetadataBlockData.cs \
	$(srcdir)/Flac/Util/MetadataBlockDataStreamInfo.cs \
	$(srcdir)/Flac/Util/MetadataBlockHeader.cs

OGG_CSFILES = \
	$(srcdir)/Ogg/OggFileReader.cs \
	$(srcdir)/Ogg/OggTag.cs \
	$(srcdir)/Ogg/OggFileWriter.cs \
	$(srcdir)/Ogg/Util/OggCRCFactory.cs \
	$(srcdir)/Ogg/Util/VorbisTagCreator.cs \
	$(srcdir)/Ogg/Util/VorbisTagWriter.cs \
	$(srcdir)/Ogg/Util/OggTagCreator.cs \
	$(srcdir)/Ogg/Util/OggTagReader.cs \
	$(srcdir)/Ogg/Util/VorbisTagReader.cs \
	$(srcdir)/Ogg/Util/OggInfoReader.cs \
	$(srcdir)/Ogg/Util/OggPageHeader.cs \
	$(srcdir)/Ogg/Util/VorbisCodecHeader.cs

MPC_CSFILES = \
	$(srcdir)/Mpc/MpcFileReader.cs \
	$(srcdir)/Mpc/MpcFileWriter.cs \
	$(srcdir)/Mpc/Util/MpcHeader.cs \
	$(srcdir)/Mpc/Util/MpcInfoReader.cs

MP3_CSFILES = \
	$(srcdir)/Mp3/Id3Tag.cs \
	$(srcdir)/Mp3/Mp3FileReader.cs \
	$(srcdir)/Mp3/Util/Id3frames/CommId3Frame.cs \
	$(srcdir)/Mp3/Util/Id3frames/Id3Frame.cs \
	$(srcdir)/Mp3/Util/Id3frames/TextId3Frame.cs \
	$(srcdir)/Mp3/Util/Id3v24TagReader.cs \
	$(srcdir)/Mp3/Util/Id3v2TagReader.cs \
	$(srcdir)/Mp3/Util/Id3v2TagSynchronizer.cs \
	$(srcdir)/Mp3/Util/Mp3InfoReader.cs \
	$(srcdir)/Mp3/Util/Id3v1TagReader.cs \
	$(srcdir)/Mp3/Util/LameMPEGFrame.cs \
	$(srcdir)/Mp3/Util/MPEGFrame.cs \
	$(srcdir)/Mp3/Util/XingMPEGFrame.cs

MP4_CSFILES = \
	$(srcdir)/Mp4/Mp4FileReader.cs \
	$(srcdir)/Mp4/Util/Mp4InfoReader.cs \
	$(srcdir)/Mp4/Util/Mp4TagReader.cs

TRACKER_CSFILES = \
	$(srcdir)/Tracker/TrackerFileReader.cs \
	$(srcdir)/Tracker/Util/TrackerTagReader.cs

ASSEMBLY_SOURCES = \
	$(srcdir)/AssemblyInfo.cs \
	$(srcdir)/Exceptions/CannotReadException.cs \
	$(srcdir)/Exceptions/CannotWriteException.cs \
	$(srcdir)/Exceptions/UnsupportedFormatException.cs \
	$(srcdir)/Util/AbstractTagCreator.cs \
	$(srcdir)/Util/AudioFileReader.cs \
	$(srcdir)/Util/AudioFileWriter.cs \
	$(srcdir)/Util/ByteBuffer.cs \
	$(srcdir)/Util/CustomAttributes.cs \
	$(srcdir)/Util/TagField.cs \
	$(srcdir)/Util/Utils.cs \
	$(srcdir)/AudioFileContainer.cs \
	$(srcdir)/AudioFileIO.cs \
	$(srcdir)/AudioFile.cs \
	$(srcdir)/EncodingInfo.cs \
	$(srcdir)/Tag.cs \
	$(srcdir)/CreativeCommons.cs \
	$(APE_CSFILES) \
	$(ASF_CSFILES) \
	$(FLAC_CSFILES) \
	$(OGG_CSFILES) \
	$(MPC_CSFILES) \
	$(MP3_CSFILES) \
	$(MP4_CSFILES) \
	$(TRACKER_CSFILES)

all: $(ASSEMBLY)

$(ASSEMBLY): $(ASSEMBLY_SOURCES)
	$(MCS) $(MCS_FLAGS)  -target:library -out:$@ $(ASSEMBLY_SOURCES)

# NOT FOR GAC USE UNTIL MUCH ADOPTION
# http://www.mono-project.com/Guidelines:Application_Deployment
#
#install-data-hook:
#	$(GACUTIL) /i $(ASSEMBLY) /package $(PACKAGE) /gacdir $(libdir) /root $(DESTDIR)$(libdir)

#uninstall-hook:
#	$(GACUTIL) /u $(ASSEMBLY_NAME) /package $(PACKAGE) /gacdir $(libdir) /root $(DESTDIR)$(libdir)

EXTRA_DIST = \
	$(ASSEMBLY_SOURCES)

CLEANFILES = $(ASSEMBLY)
DISTCLEANFILES = *.mdb Makefile.in *.dll *.exe

test:
	$(MCS) -out:entagged-test.exe -r:entagged-sharp.dll EntaggedTest.cs

testrun:
	$(MONO) entagged-test.exe

