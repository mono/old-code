ifeq ("$(WINDIR)","")
	CSC := mcs /debug+
	S:=/
else
	CSC := csc /debug+
	S:=\\
endif

ICECSFILES=IceCore.cs \
	IceProtocolReader.cs \
	IceProtocolWriter.cs \
	IceObject.cs \
	IceProxy.cs \
	IceProtocol.cs \
	IceCollectionWrappers.cs \
	IceReceiverDispatcher.cs \
	IceManager.cs \
	IceEndpoint.cs \
	IceUtil.cs \
	IceChannel.cs \
	IceChannelUtils.cs \
	IceClientChannel.cs \
	IceClientFormatterSink.cs \
	IceClientFormatterSinkProvider.cs \
	IceClientTransportSink.cs \
	IceClientTransportSinkProvider.cs \
	IceServerChannel.cs \
	IceServerFormatterSink.cs \
	IceServerFormatterSinkProvider.cs \
	IceServerTransportSink.cs \
	IceExceptions.cs

#SLICEFILES=slice2cs$(S)BuiltinSequences.cs \
#	slice2cs$(S)LocalException.cs \
#	slice2cs$(S)Current.cs \
#	slice2cs$(S)Facet.cs \
#	slice2cs$(S)Locator.cs \
#	slice2cs$(S)Logger.cs \
#	slice2cs$(S)ObjectFactory.cs \
#	slice2cs$(S)Plugin.cs \
#	slice2cs$(S)Properties.cs \
#	slice2cs$(S)Router.cs

.SUFFIXES: .cs .exe

%.exe: %.cs ice.dll
	$(CSC) /out:$@ /r:ice.dll $<

all: ice.dll
	cd samples; $(MAKE) CSC="$(CSC)"; cd ..

ice.dll: $(ICECSFILES) $(SLICEFILES)
	$(CSC) /out:$@ /target:library $^

clean:
	rm -f ice.dll ice.pdb *.exe

