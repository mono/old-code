#import <ObjCSharpBridge.h>
#import <Foundation/Foundation.h>

int main (void) {
	[[NSAutoreleasePool alloc] init];

	@try {
		id bridge = [ObjCSharpBridge alloc];
		[bridge init];
	
		[bridge loadAssembly: "test-library.dll"];

		Class ExceptionTests = [bridge getClass: "TestLibrary.ExceptionTests"];

		[ExceptionTests ThrowException];

		return 1;
	} @catch (NSException *e) {
		return 0;
	}
}
