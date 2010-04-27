SUBDIRS = \
glue

ADDIN_BUILD = $(top_builddir)/src/lib
ASSEMBLY = $(ADDIN_BUILD)/efl-sharp.dll


FILES = \
AssemblyInfo.cs \
../bin/data.cs \
common/e_common.cs \
ecore/ecore_animator.cs \
ecore/ecore_app.cs \
ecore/ecore_evas.cs \
ecore/ecore_event.cs \
ecore/ecore_file.cs \
ecore/ecore_idler.cs \
ecore/ecore_main.cs \
ecore/ecore_timer.cs \
ecore/ecore_x.cs \
evas/evas_canvas.cs \
evas/evas_list.cs \
evas/evas_object.cs \
evas/evas_object_gradient.cs \
evas/evas_object_hash.cs \
evas/evas_object_image.cs \
evas/evas_object_line.cs \
evas/evas_object_polygon.cs \
evas/evas_object_rectangle.cs \
evas/evas_object_text.cs \
evas/evas_object_textblock.cs \
edje/edje.cs \
ewl/ewl_app.cs \
ewl/ewl_button.cs \
ewl/ewl_checkbutton.cs \
ewl/ewl_config.cs \
ewl/ewl_container.cs \
ewl/ewl_embed.cs \
ewl/ewl_entry.cs \
ewl/ewl_enums.cs \
ewl/ewl_event.cs \
ewl/ewl_image.cs \
ewl/ewl_object.cs \
ewl/ewl_overlay.cs \
ewl/ewl_password.cs \
ewl/ewl_radiobutton.cs \
ewl/ewl_spinner.cs \
ewl/ewl_text.cs \
ewl/ewl_widget.cs \
ewl/ewl_window.cs \
epsilon/epsilon.cs 

EXTRA_DIST = $(FILES)

all: $(ASSEMBLY)

build_sources = $(addprefix $(srcdir)/, $(FILES))

$(ASSEMBLY): $(build_sources)
	mkdir -p $(ADDIN_BUILD)
	$(COMPILER) -warn:0 -out:$@ -target:library $(build_sources)

assemblydir = $(libdir)
assembly_DATA = $(ASSEMBLY)

CLEANFILES = $(ASSEMBLY)


install:
	gacutil -i $(ASSEMBLY)
	mkdir @libdir@/mono/@PACKAGE_VERSION@ || true
	install -m 0644 $(ASSEMBLY) @libdir@/mono/@PACKAGE_VERSION@/
	cd glue && make install
