#import <ObjCSharpBridge.h>
#import <Foundation/Foundation.h>

int main (void) {
	[[NSAutoreleasePool alloc] init];

	@try {
		id bridge = [ObjCSharpBridge alloc];
		[bridge init];
	
		[bridge loadAssembly: "test-library.dll"];
	
		Class TestClass = [bridge getClass: "TestLibrary.StaticTests"];
		Class Console = [bridge getClass: "System.Console"];
		
		// Call a static class method
		[TestClass StaticMethod];
	
		// Call a static bound method
		[Console WriteLineWithString:"Console.WriteLine (\"Hello World\")"];

		if (*(int *)[TestClass ReturnInt32] != -1) {
			return 2; 
		}

		if (*(float *)[TestClass ReturnFloat] != -1.0f) {
			return 3;
		}

		return 0;
	} @catch (NSException *e) {
		NSLog (@"Unhandled Exception: %s", [[e name] cString]);
		NSLog (@"%s", [[e reason] cString]);
		return 1;
	}
}
