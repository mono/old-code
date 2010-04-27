#import <ObjCSharpBridge.h>
#import <Foundation/Foundation.h>

@implementation MyActor : NSObject
- (void) invokeHandler: (id) handler bridge: (id) bridge
{
	/*
	 * Delegate.Invoke expects and object[] with (object sender, EventArgs args) packed into it
	 * Lets get class representations of ArrayList and EventArgs
	 */
	Class ArrayListClass = [bridge getClass: "System.Collections.ArrayList"];
	Class EventArgsClass = [bridge getClass: "System.EventArgs"];

	// Lets get a new arraylist big enough for our needs
	id arraylist = [ArrayListClass initWithInt32:2];

	// Lets stuff in ourselves as the sender
	[arraylist AddWithObject:self];
	// And a instance of EventArgs
	[arraylist AddWithObject:[EventArgsClass init]];

	// Fly; lets call Delegate.DynamicInvoke
	[handler DynamicInvokeWithObjectArray:[arraylist ToArray]];
}
@end

@implementation MyTarget : NSObject
- (void) targetMethod: (id) sender args: (id) args
{
	/*
	 * Lets recap; at this point (you'll need to skip and read main below to follow)
	 * At this point we have an instance of MyActor that was created by calling into the
	 * Managed representation of it ([[MyActorClass alloc] init]).  We called a native 
	 * method on actor; but in fact this trampolined thru managed land because we alloced
	 * a managed instance representation of it.  The managed instance of MyActor then trampolined
	 * and translated the call down to the native instance of MyActor (see above).  The
	 * native instance of MyActor got passed a native representation of an EventHandler delegate
	 * that is pointing at a native instace of a managed representation of a native class.
	 * MyActor then built and array list and Invoked the delegate.  This call trampoilines
	 * back up to native land; and is acted on by the managed instance that handler represents.
	 * This instance (see below) was told to act on MyTarget (this).  The delegate will invoke
	 * a S.R.E generated instance of MyTarget.targetMethod which trampolines the call to this 
	 * selector.
	 *
	 * Confused yet? :)
	 */
	NSLog (@"I was invoked by a %s with %s", [[sender className] cString], [[args className] cString]);
}
@end

int main (void) {
	// Make Apple shut up
	[[NSAutoreleasePool alloc] init];

	@try { 
		// Get the bridge and initialize it
		id bridge = [[ObjCSharpBridge alloc] init];

		// Load objc representations of the managed representations of the real objc classes
		Class MyActorClass = [bridge getClass: "MyActor"];
		Class MyTargetClass = [bridge getClass: "MyTarget"];
		// Load objc representation of EventHandler
		Class EventHandlerClass = [bridge getClass: "System.EventHandler"];
	
		// Lets generate some instances (this will generate the objc and managed types and glue them together)
		id actor = [[MyActorClass alloc] init];
		id target = [[MyTargetClass alloc] init];
	
		/*
		 * Lets call a native method on actor; that takes an EventHandler as an argument
		 * actor will call Delegate.Invoke (object []) on the EventHandler
		 * the EventHandler is initialized with a target of "target" and method os the selector "targetMethod:args:"
		 * We have a special case for delegates which translates:
		 * new Delegate (object target, MethodInfo method) -> initWithTarget:Selector:
		 */
		[actor invokeHandler: [EventHandlerClass initWithTarget: target Selector: "targetMethod:args:"] bridge: bridge];

		return 0;
	} @catch (NSException *ex) {
		NSLog (@"Unhandled exception: %s", [[ex name] cString]);
		NSLog (@"%s", [[ex reason] cString]);
		return 1;
	}
}
