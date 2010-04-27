SUBDIRS=Mono.Languages.Logo mlogo

all:
	for i in $(SUBDIRS); do \
		$(MAKE) -C $$i || exit 0; \
	done

clean:
	for i in $(SUBDIRS); do \
		$(MAKE) -C $$i clean || exit 0; \
	done

