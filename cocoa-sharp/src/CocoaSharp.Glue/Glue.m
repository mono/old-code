#import <objc/objc-runtime.h>
#import <objc/objc-class.h>
#import <Foundation/NSObject.h>
#import <Foundation/NSString.h>
#import <Foundation/NSInvocation.h>
#import <Foundation/NSMethodSignature.h>
#import <AppKit/NSTextField.h>

// This is needed until bug #61033 is fixed
#define JIT_HACK 1

typedef id (*constructorDelegate)(id THIS, const char *className);
typedef id (*managedDelegate)(int what,id anInvocation);
typedef int (*classHandlerDelegate)(const char *className);
#if JIT_HACK
typedef managedDelegate (*getManagedDelegate)(id THIS);
#else
typedef void (*getManagedDelegate)(id THIS);
#endif
#define GLUE_methodSignatureForSelector 0
#define GLUE_forwardInvocation 1


constructorDelegate sConstructorDelegate = nil;
getManagedDelegate sGetManagedDelegate = nil;
BOOL sIsGlueVerbose = NO;

BOOL IsGlueVerbose() { return sIsGlueVerbose; }
void SetGlueVerbose(BOOL verbose) { sIsGlueVerbose = verbose; }

void SetConstructorDelegate(constructorDelegate _constructorDelegate,getManagedDelegate _getManagedDelegate) {
    if (IsGlueVerbose())
        NSLog(@"GLUE: Setting delegates (%p,%p)", _constructorDelegate,_getManagedDelegate);
    sConstructorDelegate = _constructorDelegate;
    sGetManagedDelegate = _getManagedDelegate;
}

void InitGlue(classHandlerDelegate classHandler) {
    objc_setClassHandler(classHandler);
}

const char * GetObjectSuperClassName(id THIS, int depth) {
    id sclass = THIS;
    int i = 0;
    for (i = 0; i < depth; i++)
        sclass = [sclass superclass];
//	printf ("DEBUG: %s - %s\n", [[THIS className] cString], [[sclass className] cString]);
    return [[sclass className] cString];
}
const char * GetObjectClassName(id THIS) {
    return [[THIS className] cString];
}

#if JIT_HACK
managedDelegate sJIT_HACK_Delegate;
void SetJIT_HACK_Delegate(managedDelegate delegate) {
    NSLog(@"GLUE: SetJIT_HACK_Delegate: %p", delegate);
    sJIT_HACK_Delegate = delegate;
}
#endif

managedDelegate GetDelegateForBase(id base) {
    if (IsGlueVerbose())
        NSLog(@"GLUE: GetDelegateForBase base=%@",base); 
    managedDelegate delegate = nil;
    object_getInstanceVariable(base,"mDelegate",(void**)&delegate);
    if (delegate == nil)
#if JIT_HACK
    {
        NSLog(@"   inst var == nil --> fetch delegate"); 
        sGetManagedDelegate(base); delegate = sJIT_HACK_Delegate; 
	}
#else
        delegate = sGetManagedDelegate(base);
#endif
    return delegate;
}

void AddMethods(Class cls,int numOfMethods,const char **methods,const char **signatures,IMP method,int count,...) {
    struct objc_method_list *methodsToAdd = (struct objc_method_list *)
        malloc((count+numOfMethods)*sizeof(struct objc_method) + sizeof(struct objc_method_list));
    methodsToAdd->method_count = count+numOfMethods;

    int i;
    struct objc_method *meth = methodsToAdd->method_list;

    for (i = 0; i < numOfMethods; ++i) {
        meth->method_name = sel_getUid(methods[i]);
	if (strcmp (meth->method_name, "initWithFrame:") == 0) {
            meth->method_name = "INTERNAL_initWithFrame:";
        }
        meth->method_types = (char*)strdup(signatures[i]);
        meth->method_imp = method;
        if (IsGlueVerbose())
            NSLog(@"  registering method: %s (%s) %p",sel_getName(meth->method_name),meth->method_types,meth->method_imp);
        ++meth;
    }

    va_list vl;
    va_start(vl,count);
    for (i = 0; i < count; ++i) {
        meth->method_name = va_arg(vl,SEL);
        meth->method_types = (char*)strdup(va_arg(vl,const char *));
        meth->method_imp = va_arg(vl,IMP);
        if (IsGlueVerbose())
            NSLog(@"  registering method: %s (%s) %p",sel_getName(meth->method_name),meth->method_types,meth->method_imp);
        ++meth;
    }

    class_addMethods(cls, methodsToAdd);
}

void AddInstanceVariables(Class cls,
    int numOfVars,const char **varNames,const char **varTypes,int *varSizes,
    int count,...) {

    int i = 0;
    int ivar_size = 0;
    int ivar_count = numOfVars + count;
    struct objc_ivar_list *ivar_list = nil;
    struct objc_ivar *ivar = nil;

    if(ivar_count > 0) {
        ivar_size = cls->super_class->instance_size;
        ivar_list = malloc(sizeof(struct objc_ivar_list) + (ivar_count)*sizeof(struct objc_ivar));
	ivar_list->ivar_count = 0; 
        for(i = 0; i < numOfVars ; i++) {
            ivar = ivar_list->ivar_list + ivar_list->ivar_count;
            ivar_list->ivar_count++;
            ivar->ivar_name = (char*)strdup(varNames[i]);
            ivar->ivar_type = (char*)strdup(varTypes[i]);
            ivar->ivar_offset = ivar_size;
            
            ivar_size += varSizes[i];
            if (IsGlueVerbose())
                NSLog(@"  registering var: %s (%s) %i",ivar->ivar_name,ivar->ivar_type,ivar->ivar_offset);
        }
        va_list vl;
        va_start(vl, count);
        for(i = 0; i < count; i++) {
            ivar = ivar_list->ivar_list + ivar_list->ivar_count;
            ivar_list->ivar_count++;
            ivar->ivar_name = va_arg(vl, char*);
            ivar->ivar_type = va_arg(vl, char*);
            ivar->ivar_offset = ivar_size;
            
            ivar_size += va_arg(vl, int);
            if (IsGlueVerbose())
                NSLog(@"  registering var: %s (%s) %i",ivar->ivar_name,ivar->ivar_type,ivar->ivar_offset);
        }
        if (IsGlueVerbose())
            NSLog(@"GLUE: ivar_size=%i ivar_list=%i", ivar_size, (sizeof(struct objc_ivar_list) + (ivar_count)*sizeof(struct objc_ivar)));
        cls->instance_size = ivar_size;
        cls->ivars = ivar_list;
    }
}

NSMethodSignature * MakeMethodSignature(const char *types) {
    NSMethodSignature *ret = [NSMethodSignature signatureWithObjCTypes: types];
    if (IsGlueVerbose())
        NSLog(@"GLUE: MakeMethodSignature %s --> %@",types,ret);
    return ret;
}

//- (id) initWithManagedDelegate: (managedDelegate) delegate
id glue_initWithManagedDelegate(id base, SEL sel, ...) {
    if (IsGlueVerbose())
        NSLog(@"GLUE: glue_initWithManagedDelegate %@ %s", base, sel_getName(sel));

    va_list vl;
    va_start(vl,sel);
    managedDelegate delegate = va_arg(vl,managedDelegate);
    object_setInstanceVariable(base,"mDelegate",delegate);
    return base;
}

//- (NSMethodSignature *) methodSignatureForSelector: (SEL) aSelector
id glue_methodSignatureForSelector(id base, SEL sel, ...) {
    va_list vl;
    va_start(vl,sel);
    SEL aSelector = va_arg(vl,SEL);
    NSString *strSel = [NSString stringWithCString: sel_getName(aSelector)];
    
    if (IsGlueVerbose())
        NSLog(@"GLUE: glue_methodSignatureForSelector %p %s", base, sel_getName(aSelector));

    NSMethodSignature* signature = [[base superclass] instanceMethodSignatureForSelector: aSelector];
    
    if (!signature && [strSel hasPrefix: @"_dotNet_INTERNAL_"]) {
#if true
        managedDelegate delegate = GetDelegateForBase(base);

        aSelector = sel_getUid([[strSel substringFromIndex: 17] cString]);
        signature = (NSMethodSignature*)delegate(GLUE_methodSignatureForSelector,(id)aSelector);
#else
        signature = MakeMethodSignature([[strSel substringFromIndex: 17] cString]);
#endif
    }
    else if (!signature && [strSel hasPrefix: @"_dotNet_"]) {
#if true
        managedDelegate delegate = GetDelegateForBase(base);

        aSelector = sel_getUid([[strSel substringFromIndex: 8] cString]);
        signature = (NSMethodSignature*)delegate(GLUE_methodSignatureForSelector,(id)aSelector);
#else
        signature = MakeMethodSignature([[strSel substringFromIndex: 8] cString]);
#endif
    }
    
    return signature;
}

id glue_initToManaged(id base, SEL sel, ...) {
    if (IsGlueVerbose())
        NSLog(@"GLUE: glue_initToManaged (base=%@)",base);
    sConstructorDelegate(base,GetObjectClassName(base));
    return base;
}

id glue_initWithFrameToManaged(id base, SEL sel, ...) {
    struct objc_super superContext;
    NSRect arg;
    if (IsGlueVerbose())
        NSLog(@"GLUE: glue_initWithFrameToManaged (base=%@)",base);

    va_list vl;
    va_start(vl,sel);
    superContext.receiver = base;
    superContext.class = [base superclass];
    
    arg = va_arg(vl, NSRect);
    base = objc_msgSendSuper (&superContext, sel, arg);
    if (!base)
	NSLog (@"initWithFrame failed");
    sConstructorDelegate(base,GetObjectClassName(base));

    [base _dotNet_INTERNAL_initWithFrame:arg];
    return base;
}
    
//- (void) forwardInvocation: (NSInvocation *) anInvocation;
id glue_forwardInvocation(id base, SEL sel, ...) {
    va_list vl;
    va_start(vl,sel);
    NSInvocation * anInvocation = va_arg(vl,NSInvocation *);

    NSString *selName = [[NSString stringWithCString: sel_getName([anInvocation selector])] substringFromIndex: 8];
    NSString *internal = [NSString stringWithCString: "INTERNAL_"];
    if ([selName hasPrefix: internal]) 
        selName = [selName substringFromIndex: 9];
    SEL aSelector = sel_getUid([selName cString]);
    [anInvocation setSelector: aSelector];

    if (IsGlueVerbose())
        NSLog(@"GLUE: glue_forwardInvocation: calling delegate %@ %s", base, sel_getName([anInvocation selector]));

    managedDelegate delegate = GetDelegateForBase(base);
    if (IsGlueVerbose())
        NSLog(@"GLUE: glue_forwardInvocation: base=%@ delegate=%p",base,delegate);

    if (delegate == nil || delegate(GLUE_forwardInvocation,anInvocation) != nil)
        [base doesNotRecognizeSelector: [anInvocation selector]];
    return base;
}

id glue_implementMethod(id base, SEL sel, ...) {
    va_list vl;
    va_start(vl,sel);

    Method method = class_getInstanceMethod([base class], sel);
    int numArgs = method_getNumberOfArguments(method);
    int size = method_getSizeOfArguments(method);
    NSString *selName = [NSString stringWithFormat: @"_dotNet_%s", sel_getName (sel)];
    NSString *internal = [NSString stringWithCString: "INTERNAL_"];
    if ([selName hasPrefix: internal]) 
        selName = [selName substringFromIndex: 9];
    SEL forwardSel = sel_getUid([selName cString]);
    marg_list margs;
    marg_malloc(margs, method);

    const char* type;
    void *arg;
    int i, offset;
    // Add the id to the margs
    method_getArgumentInfo(method, 0, &type, &offset);
    marg_setValue(margs, offset, id, base);
    // Add the sel to the margs
    method_getArgumentInfo(method, 1, &type, &offset);
    marg_setValue(margs, offset, SEL, forwardSel);
    // Process the rest of the margs on the stack
    size = (numArgs)*4;
    if (IsGlueVerbose())
        NSLog(@"GLUE: glue_implementMethod base=%@",base);
    for(i = 2; i < numArgs; i++) {
        // TODO: handle structures and non-4 byte argument types
        arg = va_arg(vl, void *);
        method_getArgumentInfo(method, i, &type, &offset);
        if (IsGlueVerbose())
            NSLog(@"    Getting arg: %i (type=%s inserting at: %i = %p", i, type, offset, arg);
        marg_setValue(margs, offset, void *, arg); 
    }
    if (IsGlueVerbose())
        NSLog(@"glue_implementMethod %@ %s (%s) method=%p margs=%p size=%i", base, sel_getName(forwardSel), sel_getName(method->method_name), method,margs,size);

    id ret;
    if(numArgs == 2)
        ret = (id)objc_msgSend(base, forwardSel);
    else
        ret = (id)objc_msgSendv(base, forwardSel, size, margs);
    marg_free(margs);
    return ret;
}

id DotNetForwarding_initWithManagedDelegate(id THIS, managedDelegate delegate) {
    if (IsGlueVerbose())
        NSLog(@"GLUE: DotNetForwarding_initWithManagedDelegate: %@",THIS);
    return glue_initWithManagedDelegate(THIS, @selector(initWithManagedDelegate:), delegate);
}

Class CreateClassDefinition(const char * name, const char * superclassName,
    int numOfMethods,const char **methods,const char **signatures,
    int numOfVars,const char **varNames,const char **varTypes,int *varSizes) {
    //
    // Ensure that the superclass exists and that someone
    // hasn't already implemented a class with the same name
    //
    Class super_class = (Class)objc_lookUpClass (superclassName);
    if (super_class == nil)
        return nil;

    Class new_class = (Class)objc_lookUpClass (name);
    if (new_class == nil) {
        if (IsGlueVerbose())
            NSLog(@"GLUE: creating a subclass of %s named %s", superclassName, name);

        // Find the root class
        Class root_class = (Class)super_class;
        while(root_class->super_class != nil)
            root_class = root_class->super_class;
    
        // Allocate space for the class and its metaclass
        new_class = (Class)calloc( 2, sizeof(struct objc_class) );
        Class meta_class = &new_class[1];
    
        // setup class
        new_class->isa      = meta_class;
        new_class->info     = CLS_CLASS;
        meta_class->info    = CLS_META;
    
        //
        // Create a copy of the class name.
        // For efficiency, we have the metaclass and the class itself 
        // to share this copy of the name, but this is not a requirement
        // imposed by the runtime.
        //
        new_class->name = (const char *)strdup(name);
        meta_class->name = new_class->name;

            //
        // Connect the class definition to the class hierarchy:
        // Connect the class to the superclass.
        // Connect the metaclass to the metaclass of the superclass.
        // Connect the metaclass of the metaclass to
        //      the metaclass of the root class.
        new_class->super_class  = super_class;
        meta_class->super_class = super_class->isa;
        meta_class->isa         = (void *)root_class->isa;
    
        //
        // Allocate empty method lists.
        // We can add methods later.
        //
        new_class->methodLists = (struct objc_method_list**)calloc( 1, sizeof(struct objc_method_list *) );
        *new_class->methodLists = (struct objc_method_list*)-1;
        meta_class->methodLists = (struct objc_method_list**)calloc( 1, sizeof(struct objc_method_list *) );
        *meta_class->methodLists = (struct objc_method_list*)-1;

        AddInstanceVariables(
            new_class, 
            numOfVars,varNames,varTypes,varSizes,
            1,
            "mDelegate", @encode(managedDelegate), sizeof(managedDelegate)
        );
    
        // Finally, register the class with the runtime.
        objc_addClass( new_class );

        AddMethods(new_class, 
                numOfMethods, methods, signatures, glue_implementMethod,
                5, 
                @selector(init), "@8@0:4", glue_initToManaged,
                @selector(initWithFrame:), "@12@0:4@8", glue_initWithFrameToManaged,
                @selector(initWithManagedDelegate:), "@12@0:4^?8", glue_initWithManagedDelegate,
                @selector(methodSignatureForSelector:), "@12@0:4:8", glue_methodSignatureForSelector,
                @selector(forwardInvocation:), "v12@0:4@8", glue_forwardInvocation);
    }
    else
        AddMethods(new_class, 
                numOfMethods, methods, signatures, glue_implementMethod,
                4, 
                @selector(init), "@8@0:4", glue_initToManaged,
                @selector(initWithFrame:), "@12@0:4@8", glue_initWithFrameToManaged,
                @selector(methodSignatureForSelector:), "@12@0:4:8", glue_methodSignatureForSelector,
                @selector(forwardInvocation:), "v12@0:4@8", glue_forwardInvocation);

    return new_class;
}
