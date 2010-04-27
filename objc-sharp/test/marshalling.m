#import <ObjCSharpBridge.h>
#import <Foundation/Foundation.h>

@implementation NativeClass : NSObject
int myint;

- (void) setInteger: (int) toset
{
	myint = toset;
}

- (bool) compareInteger: (int) tocompare
{
	return (tocompare == myint);
}
	
@end

int main (void) {
	// Make Apple shut up
	[[NSAutoreleasePool alloc] init];

	@try { 
		// Get the bridge and initialize it
		id bridge = [[ObjCSharpBridge alloc] init];

		[bridge loadAssembly: "test-library.dll"];

		id native = [[NativeClass alloc] init];
		[native setInteger:-1];

		Class NativeMarshalTests = [bridge getClass: "TestLibrary.NativeMarshalTests"];
		
		id tester = [NativeMarshalTests init];

		if ([tester InvokeCompareWithInt32:-1 Object:native] == FALSE)
			return  2;

		return 0;
	} @catch (NSException *ex) {
		NSLog (@"Unhandled exception: %s", [[ex name] cString]);
		NSLog (@"%s", [[ex reason] cString]);
		return 1;
	}
}
