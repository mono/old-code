SUBDIRS=ObjectBrowser

all:
	@for dir in $(SUBDIRS); do \
		$(MAKE) -C $$dir $(MFLAGS); \
	done

install:
	@for dir in $(SUBDIRS); do \
		$(MAKE) -C $$dir $(MFLAGS) install; \
	done
