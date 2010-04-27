SUBDIRS=testlibrary

DEPS=$(top_builddir)/src/bridge/libObjCSharp.la
LDADDS=$(top_builddir)/src/bridge/libObjCSharp.la

noinst_PROGRAMS= \
	events \
	exceptionhandling \
	instancemethods \
	marshalling \
	staticmethods

EXTRA_DIST= \
	$(events_SOURCES) \
	$(exceptionhandling_SOURCES) \
	$(instancemethods_SOURCES) \
	$(marshalling_SOURCES) \
	$(staticmethods_SOURCES) 

OBJCFLAGS=$(MONO_LIBS) $(MONO_CFLAGS) -I$(top_srcdir)/src/bridge -Wl,-framework -Wl,Foundation -fobjc-exceptions

events_DEPENDENCIES=$(DEPS)
events_LDADD=$(LDADDS)
events_SOURCES=events.m

exceptionhandling_DEPENDENCIES=$(DEPS)
exceptionhandling_LDADD=$(LDADDS)
exceptionhandling_SOURCES=exceptionhandling.m

instancemethods_DEPENDENCIES=$(DEPS)
instancemethods_LDADD=$(LDADDS)
instancemethods_SOURCES=instancemethods.m

marshalling_DEPENDENCIES=$(DEPS)
marshalling_LDADD=$(LDADDS)
marshalling_SOURCES=marshalling.m

staticmethods_DEPENDENCIES=$(DEPS)
staticmethods_LDADD=$(LDADDS)
staticmethods_SOURCES=staticmethods.m

test:
	cp ../src/objc-sharp/objc-sharp.dll .
	cp ./testlibrary/test-library.dll .
	./events
	./exceptionhandling
	./marshalling
	./staticmethods
	./instancemethods
