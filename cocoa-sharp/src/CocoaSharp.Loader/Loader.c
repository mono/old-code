/*
 *  Loader.c
 *
 *  Created by Urs C Muff on Fri Feb 20 2004.
 *  Modified by Geoff Norton on Fri Jun 4 2004.
 *  Forked from MonoHelper.c in objc-sharp
 *  Changed to run all code at thread 0 to keep cocoa happy.
 *  Copyright (c) 2004 Quark Inc. All rights reserved.
 *
 */
#include <crt_externs.h>
#define environ (* _NSGetEnviron())

#include <mono/jit/jit.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/tokentype.h>
#include <mono/metadata/class.h>
#include <mono/metadata/object.h>
#include <mono/metadata/loader.h>

#include <string.h>

int main(int argc, const char* argv[]) {
	void *pool = BeginApp();
	char *assemblyName = GetAssembly();
	ChangeToResourceDir();

	printf("DEBUG:\n\tAssembly: %s\n", assemblyName);

	MonoDomain *domain = mono_jit_init (assemblyName);

	if(domain == NULL) {
		printf("ERROR: No domain for assembly: %s\n",assemblyName);
		exit(1);
	}

	MonoAssembly *assembly = mono_domain_assembly_open (domain,
		assemblyName);

	if(assembly == NULL) {
		printf("ERROR: Assembly load failed: %s\n",assemblyName);
		exit(1);
	}

	MonoImage *image = mono_assembly_get_image(assembly);

	if(image == NULL) {
		printf("ERROR: No assembly image: %s\n",assemblyName);
		exit(1);
	}

	mono_jit_exec (domain, assembly, argc, (char**)argv);

	int retval = mono_environment_exitcode_get ();
	// Clean up the pool before the jit
	
	// Clean up the JIT environment
	mono_jit_cleanup (domain);

	EndApp(pool);
	return retval;
}
