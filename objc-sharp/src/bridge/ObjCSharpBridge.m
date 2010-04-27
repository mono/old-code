#import <ObjCSharpBridge.h>

@implementation ObjCSharpBridge 

- init
{
	[super init];

	exceptionName = [[NSString stringWithUTF8String:"ObjCSharpBridgeException"] retain];

	domain = (MonoDomain *)mono_jit_init ("ObjCSharp");
	if (!domain)
		[[NSException exceptionWithName: exceptionName reason: [NSString stringWithUTF8String:"objc-sharp.dll not found for mono_jit_init"] userInfo: nil] raise];


	assembly = mono_domain_assembly_open (domain, "objc-sharp.dll");
	if (!assembly)
		[[NSException exceptionWithName: exceptionName reason: [NSString stringWithUTF8String:"assembly open error for: objc-sharp.dll"] userInfo: nil] raise];
	
	image = mono_assembly_get_image (assembly);
	if (!image)
		[[NSException exceptionWithName: exceptionName reason: [NSString stringWithUTF8String:"image open error for: objc-sharp.dll"] userInfo: nil] raise];

	klass = mono_class_from_name (image, "ObjCSharp", "Bridge");
	if (!klass)
		[[NSException exceptionWithName: exceptionName reason: [NSString stringWithUTF8String:"class open error for: ObjCSharp.Bridge"] userInfo: nil] raise];

	getclass = mono_class_get_method_from_name (klass, "GetClass", 1);
	if (!getclass)
		[[NSException exceptionWithName: exceptionName reason: [NSString stringWithUTF8String:"method not found in ObjCSharp.Bridge"] userInfo: nil] raise];
	
	loader = mono_class_get_method_from_name (klass, "LoadAssembly", 1);
	if (!loader)
		[[NSException exceptionWithName: exceptionName reason: [NSString stringWithUTF8String:"method not found in ObjCSharp.Bridge"] userInfo: nil] raise];

	object = mono_object_new (domain, klass);

	mono_runtime_object_init (object);
	
	mono_assembly_set_main (assembly);
	mono_runtime_exec_main (mono_class_get_method_from_name (klass, "SetupBridge", 0), (MonoArray *)object, NULL);

//	mono_set_defaults (5, 0);

	return self;
}

- (void) release
{
	[super release];
	mono_jit_cleanup (domain);
}

- (int) loadAssembly: (char *) name
{
	gpointer args [1];
	MonoString *namestr;
	MonoObject *exception;

	namestr = mono_string_new (domain, name);

	args [0] = namestr;
	MonoObject *ret = mono_runtime_invoke (loader, object, args, &exception);
	if (exception)
		[[NSException exceptionWithName: [NSString stringWithUTF8String:mono_class_get_name (mono_object_get_class (exception))] reason: [NSString stringWithUTF8String:[self exceptionToString: exception]] userInfo: nil] raise];
	return 1;
}

- (char *) exceptionToString: (MonoObject *) ex
{
	MonoClass *kls;
	MonoMethod *mth;
	MonoString *str;

	kls = mono_object_get_class (ex);

	mth = mono_class_get_method_from_name_flags (kls, "ToString", 0,  0x0040 | 0x0006);

	str = (MonoString *) mono_runtime_invoke (mth, ex, NULL, NULL);

	return mono_string_to_utf8 (str);
}

- (Class) getClass: (char *) type
{
	gpointer args [1];
	MonoString *typestr;
	MonoObject *exception;

	typestr = mono_string_new (domain, type);

	args [0] = typestr;
	MonoObject *ret = mono_runtime_invoke (getclass, object, args, &exception);
	if (exception)
		[[NSException exceptionWithName: [NSString stringWithUTF8String:mono_class_get_name (mono_object_get_class (exception))] reason: [NSString stringWithUTF8String:[self exceptionToString: exception]] userInfo: nil] raise];
	return (Class)*(int *)mono_object_unbox (ret);
}
@end

void setupDelegate (void *ptr) {}

void dumpMethodList (struct objc_method_list *method_list) {
	int i = 0;
	if (method_list != nil && method_list != (struct objc_method_list *)0xffffffff) {
		struct objc_method *method = method_list->method_list;
		printf ("methods: %i\n", method_list->method_count);
		for (i = 0; i < method_list->method_count; method++, i++) {
			printf ("\tname: %s\n", method->method_name);
			printf ("\ttypes: %s\n", method->method_types);
			printf ("\timp: %x\n", method->method_imp);
		}
	}
}

void dumpClass (struct objc_class *cls) {
	printf ("name: %s %s\n", cls->name, cls->isa->name);
	printf ("\tversion: %i\n", cls->version);
	printf ("\tinfo: %i\n", cls->info);
	printf ("\tinstance_size: %i %i\n", cls->instance_size, cls->isa->instance_size);
	printf ("\tsuper_class: %x %s\n", cls->super_class, cls->super_class->name);
	printf ("\tivars: %x\n", cls->ivars);
	printf ("\tmethodLists: %x\n", cls->methodLists);
	printf ("\tcache: %x\n", cls->cache);
	printf ("\tprotocols: %x\n", cls->protocols);
        if (cls->methodLists) {
		dumpMethodList (*cls->methodLists);
	}
}	

