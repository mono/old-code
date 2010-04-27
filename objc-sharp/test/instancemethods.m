#import <ObjCSharpBridge.h>
#import <Foundation/Foundation.h>

int main (void) {
	[[NSAutoreleasePool alloc] init];

	@try {
		id bridge = [ObjCSharpBridge alloc];
		[bridge init];
	
		[bridge loadAssembly: "test-library.dll"];

		Class InstanceTests = [bridge getClass: "TestLibrary.InstanceTests"];
		Class Console = [bridge getClass: "System.Console"];
	
		// Create an couple instances instance
		id a = [InstanceTests init];
		id b = [InstanceTests initWithInt32:-2 Single:-2.0f];

		if (*(int *)[a ReturnIntValue] != *(int *)[a get_IntValue])
			return 2;
		
		if (*(float *)[a ReturnFloatValue] != *(float *)[a get_FloatValue])
			return 3;

		if (*(bool *)[a CompareToWithInstanceTests:b] != FALSE)
			return 4;

		if (*(int *)[b ReturnIntValue] != -2)
			return 5;
	
		if (*(float *)[b ReturnFloatValue] != -2.0f)
			return 6;

		[a set_IntValueWithInt32: -2];
		[a set_FloatValueWithSingle: -2.0f];

		if (*(bool *)[a CompareToWithInstanceTests:b] != TRUE)
			return 7;

		return 0;
	} @catch (NSException *e) {
		NSLog (@"Unhandled Exception: %s", [[e name] cString]);
		NSLog (@"%s\n", [[e reason] cString]);
		return 1;
	}
}
