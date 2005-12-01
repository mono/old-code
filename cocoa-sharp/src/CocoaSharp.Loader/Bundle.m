/*
 *  Bundle.m
 *
 *  Created by Urs C Muff on Fri Feb 20 2004.
 *  Modified by Geoff Norton on Fri Jun 4 2004.
 *  Forked from MonoHelper.c in objc-sharp
 *  Changed to run all code at thread 0 to keep cocoa happy.
 *  Copyright (c) 2004 Quark Inc. All rights reserved.
 *
 */

#import <Foundation/NSBundle.h>
#import <Foundation/NSString.h>
#import <Foundation/NSAutoreleasePool.h>
#import <AppKit/NSApplication.h>

void * BeginApp() {
	// Create a pool to clean up after us
	void * pool = [[NSAutoreleasePool alloc] init];

	return pool;
}

void ChangeToResourceDir() {
	chdir([[[NSBundle mainBundle] resourcePath] cString]);
}

char *GetAssembly() {
	NSLog(@"%s\n", [[[[NSBundle mainBundle] pathsForResourcesOfType:@"exe" inDirectory:nil] objectAtIndex:0] cString]);
	return [[[[NSBundle mainBundle] pathsForResourcesOfType:@"exe" inDirectory:nil] objectAtIndex:0] cString];
}

void EndApp(void * pool) {
	// Release the pool
	[(id)pool release];
}
