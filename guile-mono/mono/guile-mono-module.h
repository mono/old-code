#ifndef __GUILE_MONO_MODULE_H__
#define __GUILE_MONO_MODULE_H__

#include <libguile.h>
#include <mono/jit/jit.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/object.h>

G_BEGIN_DECLS

void gw_init_wrapset_guile_mono (void);

void scm_init_mono_module (void);
MonoImage *scm_mono_assembly_get_image (MonoAssembly *assembly);
MonoClass *scm_mono_assembly_find_class (MonoAssembly *assembly,
					 const char *name_space, const char *name);
int scm_mono_class_num_methods (MonoClass *klass);
MonoMethod *scm_mono_class_get_method (MonoClass *klass, int idx);
int scm_mono_class_num_properties (MonoClass *klass);
MonoProperty *scm_mono_class_get_property (MonoClass *klass, int idx);
const char *scm_mono_property_get_name (MonoProperty *prop);
MonoMethod *scm_mono_property_get_getter (MonoProperty *prop);
MonoMethod *scm_mono_property_get_setter (MonoProperty *prop);
const char *scm_mono_class_name (MonoClass *klass);
MonoType *scm_mono_class_get_type (MonoClass *klass);
MonoClass *scm_mono_class_get_parent (MonoClass *klass);
MonoClass *scm_mono_class_type_get_class (MonoType *type);
const char *scm_mono_method_name (MonoMethod *method);
int scm_mono_method_is_static (MonoMethod *method);
MonoClass *scm_mono_method_get_class (MonoMethod *method);

SCM scm_mono_get_boxed_boolean (MonoDomain *domain, int value);
SCM scm_mono_get_boxed_char (MonoDomain *domain, int value);
SCM scm_mono_get_boxed_int8 (MonoDomain *domain, SCM value);
SCM scm_mono_get_boxed_uint8 (MonoDomain *domain, SCM value);
SCM scm_mono_get_boxed_int16 (MonoDomain *domain, SCM value);
SCM scm_mono_get_boxed_uint16 (MonoDomain *domain, SCM value);
SCM scm_mono_get_boxed_int32 (MonoDomain *domain, SCM value);
SCM scm_mono_get_boxed_uint32 (MonoDomain *domain, SCM value);
SCM scm_mono_get_boxed_int64 (MonoDomain *domain, SCM value);
SCM scm_mono_get_boxed_uint64 (MonoDomain *domain, SCM value);
SCM scm_mono_get_boxed_float (MonoDomain *domain, float value);
SCM scm_mono_get_boxed_double (MonoDomain *domain, double value);
SCM scm_mono_object_unbox (SCM smob);
MonoClass *scm_mono_object_get_class (SCM);
MonoDomain *scm_mono_object_get_domain (SCM smob);
SCM scm_mono_runtime_invoke (MonoMethod *method, SCM instance, SCM vector);
MonoClass *scm_mono_defaults_boolean_class (void);
MonoClass *scm_mono_defaults_char_class (void);
MonoClass *scm_mono_defaults_int8_class (void);
MonoClass *scm_mono_defaults_uint8_class (void);
MonoClass *scm_mono_defaults_int16_class (void);
MonoClass *scm_mono_defaults_uint16_class (void);
MonoClass *scm_mono_defaults_int32_class (void);
MonoClass *scm_mono_defaults_uint32_class (void);
MonoClass *scm_mono_defaults_int64_class (void);
MonoClass *scm_mono_defaults_uint64_class (void);
MonoClass *scm_mono_defaults_object_class (void);
MonoClass *scm_mono_defaults_single_class (void);
MonoClass *scm_mono_defaults_double_class (void);
MonoClass *scm_mono_defaults_string_class (void);
MonoClass *scm_mono_defaults_multicast_delegate_class (void);
const char *scm_mono_string_get_chars (SCM);
int scm_mono_method_can_invoke (MonoMethod *method, int allow_accessor);
int scm_mono_property_can_invoke (MonoProperty *property);
MonoMethodSignature *scm_mono_method_get_signature (MonoMethod *method);
MonoType *scm_mono_method_signature_get_return_type (MonoMethodSignature *sig);
int scm_mono_method_signature_has_this (MonoMethodSignature *sig);
int scm_mono_method_signature_param_count (MonoMethodSignature *sig);
MonoType *scm_mono_method_signature_get_param (MonoMethodSignature *sig, int idx);
int scm_mono_type_get_kind (MonoType *type);
SCM scm_mono_create_delegate (MonoDomain *domain, MonoType *type, SCM target, SCM func, SCM closure);
SCM scm_mono_invoke_delegate (SCM delegate, SCM vector);

MonoAssembly *scm_boot_mono (const char *filename);
void scm_mono_boot_guile (gpointer delegate);
void scm_mono_guile_repl (void);

G_END_DECLS

#endif
