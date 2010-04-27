#import <ObjCSharpBridge.h>
#import <Foundation/NSString.h>
#import <Foundation/NSException.h>
#import <objc/objc-class.h>
#import <mono/metadata/class.h>
#import <mono/metadata/object.h>
#import <mono/metadata/assembly.h>
#import <mono/metadata/image.h>
#import <mono/metadata/appdomain.h>

@interface ObjCSharpBridge : NSObject 
	
	NSString *exceptionName;
	MonoDomain *domain;
	MonoAssembly *assembly;
	MonoImage *image;
	MonoClass *klass;
	MonoMethod *getclass;
	MonoMethod *loader;
	MonoObject *object;

	- (id) init;
	- (void) release;
	- (int) loadAssembly: (char *) name;
	- (char *) exceptionToString: (MonoObject *) ex;
	- (Class) getClass: (char *) type;
@end
