# mndb.gdbinit
#
# Copyright (C) Chris Toshok, 2006
# Some rights reserved (guess which ones!)
#
# I got tired of waiting for a full featured debugger when all I
# really need is to set breakpoints, have the program stop at those
# breakpoints, and let me examine state.
# 
# Stepping, while useful, is definitely not as important as basic
# breakpoint/print functionality.
#
# This version of mndb.gdbinit supports breakpoints, but only at the
# start of a method.  Not based on file and line numbers, there's no
# overload resolution.  It's pretty simple, and quite beautiful if you
# can wrap your head around how nasty gdb's macro syntax is (and how
# broken its execution environment is).
#
# enjoy


# type/data inspection
# :   mptype
# :   mprint

define mptype
	# assumes args are: namespace class
	set $l = (GList*)$debug_handles
	set $mono_class = (MonoClass*)0

	while ($l != 0)
		set $mono_class = mono_class_from_name (((MonoDebugHandle*)$l->data)->image, $arg0, $arg1)
		if ($mono_class != 0)
			loop_break
		else
			set $l = $l->next
		end
	end

	if ($mono_class != 0)
		printf "%s\n", mono_type_get_name_full (mono_class_get_type ($mono_class), MONO_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED)
	else
		printf "no such type found in loaded assemblies: %s.%s\n", $arg0, $arg1
	end
end

define mndb_print_variable_index
# from mdb:
#                internal enum AddressMode : long
#                {
#                        Register        = 0,
#                        RegOffset       = 0x10000000,
#                        TwoRegisters    = 0x20000000
#                }
#
#                const long AddressModeFlags = 0xf0000000;
#
#                Mode = (AddressMode) (Index & AddressModeFlags);
#                Index = (int) ((long) Index & ~AddressModeFlags);

	set $_mode = $arg0 & 0xf0000000
	set $_index = $arg0 & ~0xf0000000

#                        if ((Mode == AddressMode.Register) || (Mode == AddressMode.RegOffset))
#                                Index = arch.RegisterMap [Index];
#
#
#                int[] register_map = { (int) I386Register.EAX, (int) I386Register.ECX,
#                                       (int) I386Register.EDX, (int) I386Register.EBX,
#                                       (int) I386Register.ESP, (int) I386Register.EBP,
#                                       (int) I386Register.ESI, (int) I386Register.EDI };

	if ($_mode == 0 || $_mode == 0x10000000)
#		set $_index = 
	end
end

define mprint
	set $jit_info = mono_jit_info_table_find (mono_domain_get (), $pc)
	set $mono_method = mono_jit_info_get_method ($jit_info)

	mndb_find_debug_handle_for_method $mono_method

	mndb_get_debug_method_info $mono_method $mono_handle

	set $jit_debug_info = mono_debug_find_method ($mono_method, mono_domain_get ())

	if (!strcmp ($arg0, "this"))
		printf "this = %s\n", mono_type_get_name ($jit_debug_info->this_var->type)
		mndb_print_variable_index $jit_debug_info->this_var->index
	end
	if (!strcmp ($arg0, "locals"))
		printf "%d locals:\n", $jit_debug_info->num_locals

		set $i = 0
		while ($i < $jit_debug_info->num_locals)
			printf " %d = %s\n", $i, mono_type_get_name ($jit_debug_info->locals[$i]->type)
			printf "    "
			mndb_print_variable_index $jit_debug_info->locals[$i]->index
			set $i = $i + 1
		end
	end
end

# thread support
# :   mthread

# stack manipulation
# :   mup
# :   mdown
# :   mframe
# :   mwhere/mbt

define mdown
	down-silently
	mndb_print_frame
end

define mup
	up-silently
	mndb_print_frame
end

define mwhere
	# save off our current $esp
	set $saved_esp = $esp
	set $saved_frame_index = 0

	# save off our endpoint
	up-silently 5000
	set $topmost_esp = $esp	

	select-frame 0

	set $i = 0
	set $last_esp = $esp
	while ($esp != $topmost_esp)
		if ($saved_esp == $esp)
			set $saved_frame_index = $i
		end
		set $foo = mono_pmip ($pc)
		if ($foo == 0x00)
			frame
		else
			printf "#%d  0x%X in%s\n", $i, $pc, $foo
		end
		up-silently
		if ($last_esp == $esp)
			loop_break
		end
		set $last_esp = $esp
		set $i = $i + 1
	end

	select-frame $saved_frame_index

# does this work for anyone?  not for me.. - toshok
# set $mono_thread = mono_thread_current ()
# if ($mono_thread == 0x00)
#   printf "No mono thread associated with this thread\n"
# else
#   set $ucp = malloc (sizeof (ucontext_t))
#   call (void) getcontext ($ucp)
#   call (void) mono_print_thread_dump ($ucp)
#   call (void) free ($ucp)
# end
end

# execution
# :   mrun
# :   mstart
# :   mcontinue
# :   mnext
# :   mstep

define mrun
	if ($started == 1)
		printf "mndb doesn't support restarting.  tough cookies.\n"
	else
		set $started = 1
		set $stop_at_main = 0
		continue
	end
end

define mstart
	if ($started == 1)
		printf "mndb doesn't support restarting.  tough cookies.\n"
	else
		set $started = 1
		set $stop_at_main = 1
		continue
	end
end

define mcontinue
	continue
end

define mnext
	set $jit_info = mono_jit_info_table_find (mono_domain_get (), $pc)
	set $mono_method = mono_jit_info_get_method ($jit_info)

	mndb_find_debug_handle_for_method $mono_method

	mndb_get_debug_method_info $mono_method $mono_handle

	set $jit_debug_info = mono_debug_find_method ($mono_method, mono_domain_get ())

	set $native_offset = $pc - $jit_debug_info->code_start

	set $source_location = mono_debug_lookup_source_location ($mono_method, $native_offset, mono_domain_get ())

	#printf "Current native offset = %d, il offset = %d\n", $native_offset, $source_location->il_offset

	# print out the current jit debug info line number table (native/il offsets)
	set $i = 0
	while ($i < $jit_debug_info->num_line_numbers)
		printf "jit_debug_info->line_numbers[%d] = %d, %d\n", $i, $jit_debug_info->line_numbers[$i].il_offset, $jit_debug_info->line_numbers[$i].native_offset
		set $i = $i + 1
	end

	set $i = 0
	while ($i < $jit_debug_info->num_line_numbers)
		if ($source_location->il_offset < $jit_debug_info->line_numbers[$i].il_offset)
			loop_break
		end
		set $i = $i + 1
	end
	if ($i == $jit_debug_info->num_line_numbers)
		printf "couldn't find line number information for current native offset\n"
	else
		if ($i == ($jit_debug_info->num_line_numbers - 1))
			printf "mndb currently doesn't let you return from a method by stepping\n"
			# one would think this would work, but it doesn't
			# tbreak *(void**)($ebp + 4)
			# continue
		else
			set $bp_addr = $jit_debug_info->code_start + $jit_debug_info->line_numbers[$i+1].native_offset 
			#printf "setting new breakpoint at 0x%0x, native offset %d\n", $bp_addr, $bp_addr - $jit_debug_info->code_start
			tbreak *$bp_addr
			continue
		end
	end
end

define mstep
	printf "not yet...\n"
end

# breakpoints
# :   mbreak
# :   mdelete (can we even implement this?   not likely...)

define mbreak
	set $method_name = (char*)$arg0

	set $method_desc = mono_method_desc_new ($method_name, 1)

	if ($method_desc == 0)
		printf "Couldn't find method description\n"
	else
		# this looks an awful lot like mndb_find_debug_handle_for_method, but
		# we're also trying to locate the method info, so...

		set $l = (GList*)$debug_handles
		set $handle = (MonoDebugHandle*)0
		set $mono_method = (MonoMethod*)0

		while ($l != 0)
			set $handle = (MonoDebugHandle*)$l->data
			set $mono_method = mono_method_desc_search_in_image ($method_desc, $handle->image)
			if ($mono_method != 0)
				#printf "Found method in %s\n", $handle->image_file
				loop_break
			else
				set $l = $l->next
			end
		end

		if ($mono_method == 0)
			printf "Couldn't find method\n"
		else
			mndb_insert_breakpoint $mono_method $handle
		end
	end
end

define mndb_get_debug_method_info
	set $mono_method = $arg0
	set $handle = $arg1

	# we have to disable breakpoints here, since
	# our gdb commands can't re-enter, and calling into
	# mono_debug_symfile_lookup_method causes the debugger
	# notification function to be called (with REFRESH_SYMTABS)
	disable
	set $debug_method_info = mono_debug_symfile_lookup_method ($handle, $mono_method)
	if ($debug_method_info == 0)
		printf "expect a crash, could not find debug method info for %s\n", mono_method_full_name ($mono_method, 1)
	end
	enable
end

define mndb_insert_breakpoint
	set $mono_method = $arg0
	set $handle = $arg1
	set $mono_domain = mono_domain_get ()

	mndb_get_debug_method_info $mono_method $handle

	set $debug_jit_info = mono_debug_find_method ($mono_method, $mono_domain)

	if ($debug_jit_info == 0)
		printf "adding pending breakpoint for method %s\n", mono_method_full_name ($mono_method, 1)

		set $jit_breakpoint_id = $jit_breakpoint_id + 1

		set $address_list = mono_debugger_insert_method_breakpoint ($mono_method, $jit_breakpoint_id)

		set $debug_method_address_lists = g_list_prepend ($debug_method_address_lists, $address_list)
	else
		set $bp_addr = $debug_jit_info->code_start + $debug_jit_info->prologue_end
		printf "Setting breakpoint on method %s at 0x%x\n", mono_method_full_name ($mono_method, 1), $bp_addr
		break *$bp_addr
	end
end

define mndb_print_frame
	set $mono_method = 0
	set $jit_info = mono_jit_info_table_find (mono_domain_get (), $pc)
	if ($jit_info != 0x00)
		set $mono_method = mono_jit_info_get_method ($jit_info)

		mndb_find_debug_handle_for_method $mono_method

		mndb_get_debug_method_info $mono_method $mono_handle

		set $jit_debug_info = mono_debug_find_method ($mono_method, mono_domain_get ())

		set $native_offset = $pc - $jit_debug_info->code_start

		set $source_location = mono_debug_lookup_source_location ($mono_method, $native_offset, mono_domain_get ())
	end

	set $line_index = -1
	if ($source_location != 0x00)
		set $line_index = 0
		while ($line_index < $jit_debug_info->num_line_numbers)
			if ($source_location->il_offset < $jit_debug_info->line_numbers[$line_index].il_offset)
				loop_break
			end
			set $line_index = $line_index + 1
		end
	end

	if ($line_index != -1 && $line_index != ($jit_debug_info->num_line_numbers - 1))
		
	else
		set $foo = mono_pmip ($pc)
		if ($foo == 0x00)
			frame
		else
			printf "0x%x in %s\n", $pc, $foo
		end
	end
end

############################################################
# the ugly bits of glue to hook up to mono

set $started = 0
set $reached_main = 0
set $stop_at_main = 0
set $debug_handles = 0
set $debug_method_address_lists = 0

set $jit_breakpoint_id = 0

# we generate a lot of spew
set pagination off

# and mono generates a lot of signals
handle SIGXCPU SIG33 SIGPWR nostop noprint
handle SIGTRAP noprint nostop nopass

watch mono_debugger_notification_function
run
delete 1

break *mono_debugger_notification_function
commands
	silent
	handle_mono_debugger_notification
end

printf "\n\n\n\n\n\n\n"
printf "Welcome to MNDB, the Mono Native Debugger\n"
printf "Version 0.002\n"
printf "To get started, type `mrun' or `mstart'\n"

# just for kicks (there's a trailing space)
set prompt (mndb) 


# these prints out the stack frame (if it's a mono frame, anyway)
# when we need to

# let the user use up/down as well as mup/mdown.  if we up/down to a
# mono frame, print out the location
define hookpost-up
	if ($reached_main)
		if (mono_pmip ($pc) != 0)
			mndb_print_frame
		end
	end
end

define hookpost-down
	if ($reached_main)
		if (mono_pmip ($pc) != 0)
			mndb_print_frame
		end
	end
end

define hook-stop
	if ($reached_main)
		if (mono_pmip ($pc) != 0)
			mndb_print_frame
		end
	end
end

define mndb_find_debug_handle_for_method
	set $l = (GList*)$debug_handles
	set $mono_handle = (MonoDebugHandle*)0

	while ($l != 0)
		set $iter_handle = (MonoDebugHandle*)$l->data
		if ($mono_method->klass->image == $iter_handle->image)
			set $mono_handle = $iter_handle
			loop_break
		else
			set $l = $l->next
		end
	end
end

define handle_mono_debugger_notification
	# this is gross, but since the function is generated a runtime
        # gdb doesn't have symbol info for it, and so we can't just
        # look at the event argument.  we have to grovel on the stack.
	set $event = *(guint32*)($esp + 4)
	set $data = *(guint32*)($esp + 12)
	set $arg = *(guint32*)($esp + 20)

	if ($event == MONO_DEBUGGER_EVENT_INITIALIZE_MANAGED_CODE)
		print ">>> INITIALIZE_MANAGED_CODE"
		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_INITIALIZE_CORLIB)
		set $debug_handle = (MonoDebugHandle*)$data
		printf "symbol file added to runtime: %s\n", $debug_handle->image_file
		set $debug_handles = g_list_prepend ($debug_handles, $debug_handle)
		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_LOAD_MODULE)
		set $debug_handle = (MonoDebugHandle*)$data
		printf "symbol file added to runtime: %s\n", $debug_handle->image_file
		set $debug_handles = g_list_prepend ($debug_handles, $debug_handle)
		return
		continue
        else
	if ($event == MONO_DEBUGGER_EVENT_UNLOAD_MODULE)
	print 1
		set $debug_handle = (MonoDebugHandle*)$data
		printf "symbol file removed from runtime: %s\n", $debug_handle->image_file
		set $debug_handles = g_list_remove ($debug_handles, $debug_handle)

		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_JIT_BREAKPOINT)
		set $method_address = (MonoDebugMethodAddress*)$data

		mndb_find_debug_handle_for_method $method_address->header.method

		mndb_insert_breakpoint $method_address->header.method $mono_handle

		call (void) mono_debugger_remove_method_breakpoint ((int)$arg)

		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_INITIALIZE_THREAD_MANAGER)
		print ">>> INITIALIZE_THREAD_MANAGER"
		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_ACQUIRE_GLOBAL_THREAD_LOCK)
		print ">>> ACQUIRE_GLOBAL_THREAD_LOCK"
		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_RELEASE_GLOBAL_THREAD_LOCK)
		print ">>> RELEASE_GLOBAL_THREAD_LOCK"
		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_WRAPPER_MAIN)
		print ">>> WRAPPER_MAIN"
		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_MAIN_EXITED)
		print ">>> MAIN_EXITED"
		set $reached_main = 0
		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_UNHANDLED_EXCEPTION)
		print ">>> UNHANDLED_EXCEPTION"
		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_THREAD_CREATED)
		print ">>> THREAD_CREATED"
		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_THREAD_CLEANUP)
		print ">>> THREAD_CLEANUP"
		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_GC_THREAD_CREATED)
		print ">>> GC_THREAD_CREATED"
		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_GC_THREAD_EXITED)
		print ">>> GC_THREAD_EXITED"
		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_THROW_EXCEPTION)
		print ">>> THROW_EXCEPTION"
		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_HANDLE_EXCEPTION)
		print ">>> HANDLE_EXCEPTION"
		return
		continue
	else
        if ($event == MONO_DEBUGGER_EVENT_REACHED_MAIN)
		set $reached_main = 1
		return
		if ($stop_at_main == 0)
			continue
		end
	else
	if ($event == MONO_DEBUGGER_EVENT_FINALIZE_MANAGED_CODE)
		print ">>> FINALIZE_MANAGED_CODE"
		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_DOMAIN_CREATE)
		print ">>> DOMAIN_CREATE"
		return
		continue
	else
	if ($event == MONO_DEBUGGER_EVENT_DOMAIN_UNLOAD)
		print ">>> DOMAIN_UNLOAD"
		return
		continue
	else
		printf "unrecognized event code %d\n", $event
	end
	end
	end
	end
	end
	end
	end
	end
	end
	end
	end
	end
	end
	end
	end
	end
	end
	end
	end
	end
	end
end
