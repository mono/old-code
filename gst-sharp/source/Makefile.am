EXTRA_DIST =				\
	README				\
	gstreamer-parse.patch

get-source-code:
	wget http://gstreamer.freedesktop.org/src/gstreamer/gstreamer-$(GSTREAMER_REQUIRED_VERSION).tar.gz --output-document=- | tar -xz
	patch -p0 < gstreamer-parse.patch
	wget http://gstreamer.freedesktop.org/src/gst-plugins/gst-plugins-$(GSTREAMER_PLUGINS_REQUIRED_VERSION).tar.gz --output-document=- | tar -xz
	patch -p0 < gst-plugins-parse.patch

api:
	$(GAPI_PARSER) gst-sharp-source.xml

