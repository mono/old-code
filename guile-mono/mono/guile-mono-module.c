#include <guile-mono-module.h>
#include <mono/jit/jit.h>
#include <mono/metadata/verify.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/debug-helpers.h>
#include <locale.h>
#include <string.h>

typedef MonoObject * (*MonoScriptingInvokeFunc) (MonoMethod *method, gpointer data, gpointer params[]);
extern MonoMethod *
mono_marshal_get_scripting_invoke (MonoMethod *method, MonoScriptingInvokeFunc func, gpointer data);

extern void mono_config_parse (const char *filename);
extern void mono_set_rootdir (void);

scm_t_bits scm_tc_mono_object = 0;

static int
scm_mono_object_smob_print (SCM smob, SCM port, scm_print_state *pstate)
{
	MonoObject *obj;

	SCM_ASSERT (SCM_SMOB_PREDICATE (scm_tc_mono_object, smob),
		    smob, SCM_ARG1, "print");

	obj = (MonoObject *) SCM_SMOB_DATA (smob);

	scm_puts ("#<MonoObject ", port);
	scm_puts (obj->vtable->klass->name_space, port);
	scm_puts (".", port);
	scm_puts (obj->vtable->klass->name, port);
	scm_puts (">", port);

	return 1;
}

static size_t
scm_mono_object_smob_free (SCM smob)
{
	MonoObject *obj;

	SCM_ASSERT (SCM_SMOB_PREDICATE (scm_tc_mono_object, smob),
		    smob, SCM_ARG1, "print");

	obj = (MonoObject *) SCM_SMOB_DATA (smob);
	g_message (G_STRLOC ": %p", obj);
	return 0;
}

void
scm_init_mono_module (void)
{
	static gboolean initialized = FALSE;

	if (initialized)
		return;

	initialized = TRUE;

	scm_tc_mono_object = scm_make_smob_type ("MonoObject", 0);
	scm_set_smob_print (scm_tc_mono_object, scm_mono_object_smob_print);
	scm_set_smob_free (scm_tc_mono_object, scm_mono_object_smob_free);
}

MonoImage *
scm_mono_assembly_get_image (MonoAssembly *assembly)
{
	return assembly->image;
}

MonoClass *
scm_mono_assembly_find_class (MonoAssembly *assembly, const char *name_space, const char *name)
{
	MonoClass *klass;
	MonoAssembly **ptr;

	klass = mono_class_from_name (assembly->image, name_space, name);
	if (klass)
		return klass;

	for (ptr = assembly->image->references; ptr && *ptr; ptr++) {
		klass = scm_mono_assembly_find_class (*ptr, name_space, name);
		if (klass)
			return klass;
	}

	return NULL;
}

int
scm_mono_class_num_methods (MonoClass *klass)
{
	mono_class_init (klass);
	return klass->method.count;
}

MonoMethod *
scm_mono_class_get_method (MonoClass *klass, int idx)
{
	mono_class_init (klass);
	return klass->methods [idx];
}

int
scm_mono_class_num_properties (MonoClass *klass)
{
	mono_class_init (klass);
	return klass->property.count;
}

MonoProperty *
scm_mono_class_get_property (MonoClass *klass, int idx)
{
	mono_class_init (klass);
	return &klass->properties [idx];
}

const char *
scm_mono_property_get_name (MonoProperty *prop)
{
	return prop->name;
}

MonoMethod *
scm_mono_property_get_getter (MonoProperty *prop)
{
	return prop->get;
}

MonoMethod *
scm_mono_property_get_setter (MonoProperty *prop)
{
	return prop->set;
}

MonoType *
scm_mono_class_get_type (MonoClass *klass)
{
	return &klass->byval_arg;
}

MonoClass *
scm_mono_class_get_parent (MonoClass *klass)
{
	return klass->parent;
}

MonoClass *
scm_mono_class_type_get_class (MonoType *type)
{
	g_assert (type->type == MONO_TYPE_CLASS);
	return type->data.klass;
}

const char *
scm_mono_method_name (MonoMethod *method)
{
	return method->name;
}

const char *
scm_mono_class_name (MonoClass *klass)
{
	if (!strcmp (klass->name_space, ""))
		return g_strdup_printf ("%s", klass->name);
	else
		return g_strdup_printf ("%s.%s", klass->name_space, klass->name);
}

int
scm_mono_method_is_static (MonoMethod *method)
{
	return !method->signature->hasthis;
}

static int
can_marshal (MonoType *type)
{
	switch (type->type) {
	case MONO_TYPE_BOOLEAN:
	case MONO_TYPE_CHAR:
	case MONO_TYPE_I1:
	case MONO_TYPE_U1:
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
	case MONO_TYPE_I4:
	case MONO_TYPE_U4:
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
	case MONO_TYPE_R4:
	case MONO_TYPE_R8:
	case MONO_TYPE_CLASS:
	case MONO_TYPE_OBJECT:
		return TRUE;

	default:
		return FALSE;
	}
}

int
scm_mono_method_can_invoke (MonoMethod *method, int allow_accessor)
{
	MonoMethodSignature *sig;
	int i;

	if ((method->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) != METHOD_ATTRIBUTE_PUBLIC)
		return FALSE;

	if (method->flags & (METHOD_ATTRIBUTE_ABSTRACT |
			     METHOD_ATTRIBUTE_PINVOKE_IMPL |
			     METHOD_ATTRIBUTE_UNMANAGED_EXPORT))
		return FALSE;

	if (!allow_accessor && (method->flags & METHOD_ATTRIBUTE_SPECIAL_NAME))
		return FALSE;

	if (method->iflags != METHOD_IMPL_ATTRIBUTE_MANAGED)
		return FALSE;

	sig = method->signature;
	if (sig->ret) {
		if ((sig->ret->type != MONO_TYPE_VOID) && !can_marshal (sig->ret)) {
#if 0
			g_message (G_STRLOC ": %s.%s - %d", method->klass->name, method->name, sig->ret->type);
#endif
			return FALSE;
		}
	}

	for (i = 0; i < sig->param_count; i++)
		if (!can_marshal (sig->params [i])) {
#if 0
			g_message (G_STRLOC ": %s.%s - %d - %d", method->klass->name, method->name,
				   sig->params [i]->type, i);
#endif
			return FALSE;
		}

	return TRUE;
}

int
scm_mono_property_can_invoke (MonoProperty *prop)
{
	if (prop->get && !scm_mono_method_can_invoke (prop->get, TRUE))
		return FALSE;
	if (prop->set && !scm_mono_method_can_invoke (prop->set, TRUE))
		return FALSE;
	return TRUE;
}

MonoMethodSignature *
scm_mono_method_get_signature (MonoMethod *method)
{
	return method->signature;
}

MonoType *
scm_mono_method_signature_get_return_type (MonoMethodSignature *sig)
{
	return sig->ret;
}

int
scm_mono_type_get_kind (MonoType *type)
{
	return type->type;
}

int
scm_mono_method_signature_has_this (MonoMethodSignature *sig)
{
	return sig->hasthis;
}

int
scm_mono_method_signature_param_count (MonoMethodSignature *sig)
{
	return sig->param_count;
}

MonoType *
scm_mono_method_signature_get_param (MonoMethodSignature *sig, int idx)
{
	return sig->params [idx];
}

MonoClass *
scm_mono_method_get_class (MonoMethod *method)
{
	return method->klass;
}

#define MY_VALIDATE_INUM_RANGE_COPY(pos, z, low, high, cvar) \
  do { \
    gint64 tmpval; \
    if (SCM_INUMP (z))				\
      tmpval = (gint64) SCM_INUM (z);		\
    else if (SCM_BIGP (z))			\
      tmpval = scm_i_big2dbl (z);		\
    else					\
      {						\
        SCM_WRONG_TYPE_ARG (pos, z);		\
      }						\
    SCM_ASSERT_RANGE (pos, z, (low <= tmpval) && ((guint64)tmpval < high)); \
    cvar = tmpval; \
  } while (0)

#define FUNC_NAME "%scm-mono-value-box"
SCM
scm_mono_get_boxed_boolean (MonoDomain *domain, int value)
{
	MonoObject *obj = mono_value_box (domain, mono_defaults.boolean_class, &value);
	SCM_RETURN_NEWSMOB (scm_tc_mono_object, obj);
}

SCM
scm_mono_get_boxed_char (MonoDomain *domain, int value)
{
	MonoObject *obj = mono_value_box (domain, mono_defaults.char_class, &value);
	SCM_RETURN_NEWSMOB (scm_tc_mono_object, obj);
}

SCM
scm_mono_get_boxed_int8 (MonoDomain *domain, SCM num)
{
	MonoObject *obj;
	gint8 value;

	MY_VALIDATE_INUM_RANGE_COPY (1, num, -128, 128, value);
	obj = mono_value_box (domain, mono_defaults.sbyte_class, &value);
	SCM_RETURN_NEWSMOB (scm_tc_mono_object, obj);
}

SCM
scm_mono_get_boxed_uint8 (MonoDomain *domain, SCM num)
{
	MonoObject *obj;
	guint8 value;

	MY_VALIDATE_INUM_RANGE_COPY (1, num, 0, 256, value);
	obj = mono_value_box (domain, mono_defaults.byte_class, &value);
	SCM_RETURN_NEWSMOB (scm_tc_mono_object, obj);
}

SCM
scm_mono_get_boxed_int16 (MonoDomain *domain, SCM num)
{
	MonoObject *obj;
	gint16 value;

	MY_VALIDATE_INUM_RANGE_COPY (1, num, -32768, 32768, value);
	obj = mono_value_box (domain, mono_defaults.int16_class, &value);
	SCM_RETURN_NEWSMOB (scm_tc_mono_object, obj);
}

SCM
scm_mono_get_boxed_uint16 (MonoDomain *domain, SCM num)
{
	MonoObject *obj;
	guint16 value;

	MY_VALIDATE_INUM_RANGE_COPY (1, num, 0, 65536, value);
	obj = mono_value_box (domain, mono_defaults.uint16_class, &value);
	SCM_RETURN_NEWSMOB (scm_tc_mono_object, obj);
}

SCM
scm_mono_get_boxed_int32 (MonoDomain *domain, SCM num)
{
	MonoObject *obj;
	gint32 value;

	MY_VALIDATE_INUM_RANGE_COPY (1, num, -2147483648LL, 0x80000000LL, value);
	obj = mono_value_box (domain, mono_defaults.int32_class, &value);
	SCM_RETURN_NEWSMOB (scm_tc_mono_object, obj);
}

SCM
scm_mono_get_boxed_uint32 (MonoDomain *domain, SCM num)
{
	MonoObject *obj;
	guint32 value;

	MY_VALIDATE_INUM_RANGE_COPY (1, num, 0, 0x100000000LL, value);
	obj = mono_value_box (domain, mono_defaults.uint32_class, &value);
	SCM_RETURN_NEWSMOB (scm_tc_mono_object, obj);
}

SCM
scm_mono_get_boxed_int64 (MonoDomain *domain, SCM num)
{
	MonoObject *obj;
	gint64 value;

	MY_VALIDATE_INUM_RANGE_COPY (1, num, -9223372036854775807LL, 0x800000000000000LL, value);
	obj = mono_value_box (domain, mono_defaults.int64_class, &value);
	SCM_RETURN_NEWSMOB (scm_tc_mono_object, obj);
}

SCM
scm_mono_get_boxed_uint64 (MonoDomain *domain, SCM num)
{
	MonoObject *obj;
	guint64 value;

	MY_VALIDATE_INUM_RANGE_COPY (1, num, 0, 0xffffffffffffffffLL, value);
	obj = mono_value_box (domain, mono_defaults.uint64_class, &value);
	SCM_RETURN_NEWSMOB (scm_tc_mono_object, obj);
}

SCM
scm_mono_get_boxed_float (MonoDomain *domain, float value)
{
	MonoObject *obj = mono_value_box (domain, mono_defaults.single_class, &value);
	SCM_RETURN_NEWSMOB (scm_tc_mono_object, obj);
}

SCM
scm_mono_get_boxed_double (MonoDomain *domain, double value)
{
	MonoObject *obj = mono_value_box (domain, mono_defaults.double_class, &value);
	SCM_RETURN_NEWSMOB (scm_tc_mono_object, obj);
}

SCM
scm_mono_object_unbox (SCM smob)
{
	MonoObject *obj;
	void *ptr;

	SCM_ASSERT (SCM_SMOB_PREDICATE (scm_tc_mono_object, smob),
		    smob, SCM_ARG1, "%mono-object-unbox");
	obj = (MonoObject *) SCM_SMOB_DATA (smob);
	ptr = (void *) (obj + 1);

	switch (obj->vtable->klass->byval_arg.type) {
	case MONO_TYPE_CHAR:
		return SCM_MAKE_CHAR (* (gchar *) ptr);
	case MONO_TYPE_BOOLEAN:
		return * (gboolean *) ptr ? SCM_BOOL_T : SCM_BOOL_F;
	case MONO_TYPE_I1:
		return SCM_MAKINUM (* (gint8 *) ptr);
	case MONO_TYPE_U1:
		return SCM_MAKINUM (* (guint8 *) ptr);
	case MONO_TYPE_I2:
		return SCM_MAKINUM (* (gint16 *) ptr);
	case MONO_TYPE_U2:
		return SCM_MAKINUM (* (guint16 *) ptr);
	case MONO_TYPE_I4:
		return SCM_MAKINUM (* (gint32 *) ptr);
	case MONO_TYPE_U4:
		return SCM_MAKINUM (* (guint32 *) ptr);
	case MONO_TYPE_I8:
		return scm_long_long2num (* (gint64 *) ptr);
	case MONO_TYPE_U8:
		return scm_ulong_long2num (* (guint64 *) ptr);
	case MONO_TYPE_R4:
		return scm_float2num (* (float *) ptr);
	case MONO_TYPE_R8:
		return scm_double2num (* (double *) ptr);
	default:
		return SCM_UNSPECIFIED;
	}
}

MonoClass *
scm_mono_object_get_class (SCM smob)
{
	MonoObject *obj;

	SCM_ASSERT (SCM_SMOB_PREDICATE (scm_tc_mono_object, smob),
		    smob, SCM_ARG1, "%mono-object-get-class");
	obj = (MonoObject *) SCM_SMOB_DATA (smob);
	return obj->vtable->klass;
}

MonoDomain *
scm_mono_object_get_domain (SCM smob)
{
	MonoObject *obj;

	SCM_ASSERT (SCM_SMOB_PREDICATE (scm_tc_mono_object, smob),
		    smob, SCM_ARG1, "%mono-object-get-domain");
	obj = (MonoObject *) SCM_SMOB_DATA (smob);
	return obj->vtable->domain;
}

MonoClass *
scm_mono_defaults_boolean_class (void)
{
	mono_class_init (mono_defaults.boolean_class);
	return mono_defaults.boolean_class;
}

MonoClass *
scm_mono_defaults_char_class (void)
{
	mono_class_init (mono_defaults.char_class);
	return mono_defaults.char_class;
}

MonoClass *
scm_mono_defaults_int8_class (void)
{
	mono_class_init (mono_defaults.sbyte_class);
	return mono_defaults.sbyte_class;
}

MonoClass *
scm_mono_defaults_uint8_class (void)
{
	mono_class_init (mono_defaults.byte_class);
	return mono_defaults.byte_class;
}

MonoClass *
scm_mono_defaults_int16_class (void)
{
	mono_class_init (mono_defaults.int16_class);
	return mono_defaults.int16_class;
}

MonoClass *
scm_mono_defaults_uint16_class (void)
{
	mono_class_init (mono_defaults.uint16_class);
	return mono_defaults.uint16_class;
}

MonoClass *
scm_mono_defaults_int32_class (void)
{
	mono_class_init (mono_defaults.int32_class);
	return mono_defaults.int32_class;
}

MonoClass *
scm_mono_defaults_uint32_class (void)
{
	mono_class_init (mono_defaults.uint32_class);
	return mono_defaults.uint32_class;
}

MonoClass *
scm_mono_defaults_int64_class (void)
{
	mono_class_init (mono_defaults.int64_class);
	return mono_defaults.int64_class;
}

MonoClass *
scm_mono_defaults_uint64_class (void)
{
	mono_class_init (mono_defaults.uint64_class);
	return mono_defaults.uint64_class;
}

MonoClass *
scm_mono_defaults_object_class (void)
{
	mono_class_init (mono_defaults.object_class);
	return mono_defaults.object_class;
}

MonoClass *
scm_mono_defaults_single_class (void)
{
	mono_class_init (mono_defaults.single_class);
	return mono_defaults.single_class;
}

MonoClass *
scm_mono_defaults_double_class (void)
{
	mono_class_init (mono_defaults.double_class);
	return mono_defaults.double_class;
}

MonoClass *
scm_mono_defaults_string_class (void)
{
	mono_class_init (mono_defaults.string_class);
	return mono_defaults.string_class;
}

MonoClass *
scm_mono_defaults_multicast_delegate_class (void)
{
	mono_class_init (mono_defaults.multicastdelegate_class);
	return mono_defaults.multicastdelegate_class;
}

static SCM
do_runtime_invoke (MonoMethod *method, MonoObject *instance, SCM vector)
{
	MonoObject *retval;
	int length, i;
	void **params;

	length = SCM_INUM (scm_vector_length (vector));

	params = g_new0 (void *, length);
	for (i = 0; i < length; i++) {
		SCM smob = scm_vector_ref (vector, SCM_MAKINUM (i));
		MonoObject *temp;

		SCM_ASSERT (SCM_SMOB_PREDICATE (scm_tc_mono_object, smob),
			    smob, SCM_ARGn, "%mono-runtime-invoke");
		temp = (MonoObject *) SCM_SMOB_DATA (smob);

		if (temp->vtable->klass->valuetype)
			params [i] = temp + 1;
		else
			params [i] = temp;
	}

	retval = mono_runtime_invoke (method, instance, params, NULL);

	if (retval)
		SCM_RETURN_NEWSMOB (scm_tc_mono_object, retval);
	else
		return SCM_UNSPECIFIED;
}

SCM
scm_mono_runtime_invoke (MonoMethod *method, SCM instance, SCM vector)
{
	MonoObject *obj;

	if (method->signature->hasthis) {
		SCM_ASSERT (SCM_SMOB_PREDICATE (scm_tc_mono_object, instance),
			    instance, SCM_ARG2, "%mono-runtime-invoke");
		obj = (MonoObject *) SCM_SMOB_DATA (instance);
	} else
		obj = NULL;

	return do_runtime_invoke (method, obj, vector);
}

const char *
scm_mono_string_get_chars (SCM smob)
{
	MonoString *string = (MonoString *) SCM_SMOB_DATA (smob);
	return mono_string_to_utf8 (string);
}

void
scm_mono_boot_guile (gpointer delegate)
{
	void (*main_func) () = delegate;
	scm_boot_guile (0, NULL, main_func, NULL);
}

void
scm_mono_guile_repl (void)
{
	scm_shell (0, NULL);
}

static MonoObject *
delegate_invoke_func (MonoMethod *method, gpointer data, gpointer *params)
{
	int value = 3;
	MonoObject *target, *retval;
	MonoMethodSignature *sig;
	MonoDomain *domain;
	SCM vector, smob;
	int i;

	sig = method->signature;
	vector = scm_c_make_vector (sig->param_count + 2, SCM_UNSPECIFIED);

	target = *((MonoObject **)params [0]);
	domain = target->vtable->domain;

	SCM_NEWSMOB (smob, scm_tc_mono_object, target);
	scm_vector_set_x (vector, SCM_MAKINUM (0), smob);
	scm_vector_set_x (vector, SCM_MAKINUM (1), (SCM) data);

	for (i = 0; i < sig->param_count; i++) {
		gpointer vpos;
		MonoClass *class;
		MonoObject *arg;

		if (sig->params [i]->byref)
			vpos = *((gpointer *)params [i+1]);
		else 
			vpos = params [i+1];

		class = mono_class_from_mono_type (sig->params [i]);

		if (class->valuetype)
			arg = mono_value_box (domain, class, vpos);
		else 
			arg = *((MonoObject **)vpos);

		SCM_NEWSMOB (smob, scm_tc_mono_object, arg);
		scm_vector_set_x (vector, SCM_MAKINUM (i+2), smob);
	}

	smob = scm_apply_0 (SCM_CAR ((SCM) data), SCM_LIST1 (vector));

	if (sig->ret && (sig->ret->type != MONO_TYPE_VOID)) {
		SCM_ASSERT (SCM_SMOB_PREDICATE (scm_tc_mono_object, smob),
			    smob, SCM_ARGn, "%internal-delegate-invoke");
		retval = (MonoObject *) SCM_SMOB_DATA (smob);
	} else
		retval = NULL;

	return retval;
}

SCM
scm_mono_create_delegate (MonoDomain *domain, MonoType *type, SCM tsmob, SCM func, SCM closure)
{
        MonoClass *delegate_class = mono_class_from_mono_type (type);
	MonoObject *delegate, *target;
	MonoMethod *invoke, *method;
	gpointer addr;
	SCM data;

	SCM_ASSERT (SCM_SMOB_PREDICATE (scm_tc_mono_object, tsmob),
		    tsmob, SCM_ARG3, "%mono-create-delegate");
	target = (MonoObject *) SCM_SMOB_DATA (tsmob);

	g_assert (delegate_class->parent == mono_defaults.multicastdelegate_class);
	mono_class_init (delegate_class);
	invoke = mono_get_delegate_invoke (delegate_class);

	data = scm_cons (func, closure);
	delegate = mono_object_new (domain, delegate_class);
	method = mono_marshal_get_scripting_invoke (invoke, delegate_invoke_func, data);
	addr = mono_compile_method (method);

	((MonoDelegate *) delegate)->method_info = mono_method_get_object (mono_domain_get (), method, NULL);
	((MonoDelegate *) delegate)->method_ptr = addr;
	((MonoDelegate *) delegate)->delegate_trampoline = addr;
	((MonoDelegate *) delegate)->target = target;

	SCM_RETURN_NEWSMOB (scm_tc_mono_object, delegate);
}

SCM
scm_mono_invoke_delegate (SCM smob, SCM vector)
{
	MonoObject *delegate;
	MonoMethod *im;

	SCM_ASSERT (SCM_SMOB_PREDICATE (scm_tc_mono_object, smob),
		    smob, SCM_ARG3, "%mono-invoke-delegate");
	delegate = (MonoObject *) SCM_SMOB_DATA (smob);

	im = mono_get_delegate_invoke (delegate->vtable->klass);
	g_assert (im);

	return do_runtime_invoke (im, delegate, vector);
}

MonoAssembly *
scm_boot_mono (const char *filename)
{
	MonoDomain *domain;
	MonoAssembly *assembly;
	char *error;

	setlocale(LC_ALL, "");
	g_set_prgname (filename);
	mono_config_parse (NULL);
	mono_set_rootdir ();

	domain = mono_jit_init (filename);

	error = mono_verify_corlib ();
	if (error) {
		fprintf (stderr, "Corlib not in sync with this runtime: %s\n", error);
		exit (1);
	}

	assembly = mono_domain_assembly_open (domain, filename);
	if (!assembly){
		fprintf (stderr, "Can not open image %s\n", filename);
		exit (1);
	}

	mono_jit_compile_image (assembly->image, TRUE);

	return assembly;
}
