using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace ObjCSharp {
	internal class Mach {
		internal const int KERN_SUCCESS = 0;

		internal const int MACH_MSG_SUCCESS = 0x00000000;

		internal const int MACH_PORT_NULL = 0;
		internal const int MACH_PORT_RIGHT_RECEIVE = 1;
		internal const int MACH_MSG_TYPE_MAKE_SEND = 20;

		internal const int EXC_BAD_ACCESS = 1;
		internal const int EXC_BAD_INSTRUCTION = 2;
		internal const int EXC_ARITHMETIC = 3;
		internal const int EXC_EMULATION = 4;
		internal const int EXC_SOFTWARE = 5;
		internal const int EXC_BREAKPOINT = 6;
		internal const int EXC_SYSCALL = 7;
		internal const int EXC_MACH_SYSCALL = 8;
		internal const int EXC_RPC_ALERT = 9;

		internal const int EXC_MASK_MACHINE = 0;
		internal const int EXC_MASK_BAD_ACCESS = (1 << EXC_BAD_ACCESS);
		internal const int EXC_MASK_BAD_INSTRUCTION = (1 << EXC_BAD_INSTRUCTION);
		internal const int EXC_MASK_ARITHMETIC = (1 << EXC_ARITHMETIC);
		internal const int EXC_MASK_EMULATION = (1 << EXC_EMULATION);
		internal const int EXC_MASK_SOFTWARE = (1 << EXC_SOFTWARE);
		internal const int EXC_MASK_BREAKPOINT = (1 << EXC_BREAKPOINT);
		internal const int EXC_MASK_SYSCALL = (1 << EXC_SYSCALL);
		internal const int EXC_MASK_MACH_SYSCALL = (1 << EXC_MACH_SYSCALL);
		internal const int EXC_MASK_RPC_ALERT = (1 << EXC_RPC_ALERT);

		internal const int EXC_MASK_ALL = (EXC_MASK_BAD_ACCESS | EXC_MASK_BAD_INSTRUCTION | EXC_MASK_ARITHMETIC | EXC_MASK_EMULATION | EXC_MASK_SOFTWARE | EXC_MASK_BREAKPOINT | EXC_MASK_SYSCALL | EXC_MASK_MACH_SYSCALL | EXC_MASK_RPC_ALERT | EXC_MASK_MACHINE);

		internal const int EXCEPTION_DEFAULT = 1;

		internal const int THREAD_STATE_NONE = 7;

		internal const int MACH_SEND_MSG = 0x00000001;
		internal const int MACH_RCV_MSG = 0x00000002;

		internal const int MSG_SIZE = 512;

		internal static Thread exc_thread;
		internal static IntPtr exception_port = IntPtr.Zero;

		internal static void InstallExceptionHandler () {
			if (exception_port != IntPtr.Zero)
				return;

			Console.WriteLine ("[{0:yyyy-mm-dd hh:MM:ss}] Installing mach exception handler.", DateTime.Now);

			int rc = 0;
			exception_port = (IntPtr)MACH_PORT_NULL;
			IntPtr task = IntPtr.Zero;
			
			rc = mach_port_allocate (mach_task_self (), MACH_PORT_RIGHT_RECEIVE, ref exception_port);

			if (rc != KERN_SUCCESS)
				throw new Exception ("mach_port_allocate returned: " + rc);

			rc = mach_port_insert_right (mach_task_self (), exception_port, exception_port, MACH_MSG_TYPE_MAKE_SEND);
			
			if (rc != KERN_SUCCESS)
				throw new Exception ("mach_port_insert_right returned: " + rc);

			rc = task_for_pid (mach_task_self (), getpid (), ref task);
			
			if (rc != KERN_SUCCESS)
				throw new Exception ("task_for_pid returned: " + rc);
			
			rc = task_set_exception_ports (task, EXC_MASK_ALL & ~(EXC_MASK_MACH_SYSCALL | EXC_MASK_SYSCALL | EXC_MASK_RPC_ALERT), exception_port, EXCEPTION_DEFAULT, THREAD_STATE_NONE); 
			
			if (rc != KERN_SUCCESS)
				throw new Exception ("task_set_exception_ports returned: " + rc);
			
			exc_thread = new Thread (new ThreadStart (ExceptionHandler));
			exc_thread.Start ();
		}

		internal static void TestExceptionHandler () {
			Console.WriteLine ("[{0:yyyy-mm-dd hh:MM:ss}] Testing mach exception handler.", DateTime.Now);
			Console.WriteLine ("[{0:yyyy-mm-dd hh:MM:ss}] Marshal.ReadIntPtr (IntPtr.Zero);", DateTime.Now);
			try {
				Marshal.ReadIntPtr (IntPtr.Zero);
			} catch (NullReferenceException) {
				Console.WriteLine ("[{0:yyyy-mm-dd hh:MM:ss}] Generated and caught a NullReferenceException.", DateTime.Now);
			} catch (Exception e) {
				Console.WriteLine ("[{0:yyyy-mm-dd hh:MM:ss}] ERROR: An exception of type {1} was raised.", DateTime.Now, e.GetType ());
				throw e;
			}
		}

		internal static void RemoveExceptionHandler () {
			throw new NotImplementedException ();

		/*
		 * This isn't quite right; FIXME
			if (exception_port == IntPtr.Zero) 
				return;
			
			Console.WriteLine ("[{0:yyyy-mm-dd hh:MM:ss}] Removing mach exception handler.", DateTime.Now);

			int rc = 0;
			IntPtr type = IntPtr.Zero; 

			rc = mach_port_extract_right (mach_task_self (), exception_port, MACH_MSG_TYPE_MAKE_SEND, ref exception_port, ref type);
			
			if (rc != KERN_SUCCESS)
				throw new Exception ("mach_port_extract_right returned: " + rc);
			
			exception_port = IntPtr.Zero;
			exc_thread.Abort ();
		*/
		}

		internal static void ExceptionHandler () {
			int r;
			mach_msg msg;
			mach_msg reply;
	
			msg.buffer = IntPtr.Zero;
			msg.msgh_bits = 0;
			msg.msgh_size = 0;
			msg.msgh_remote_port = IntPtr.Zero;
			msg.msgh_local_port = IntPtr.Zero;
			msg.msgh_reserved = 0;
			msg.msgh_id = 0;
	
			reply.buffer = IntPtr.Zero;
			reply.msgh_bits = 0;
			reply.msgh_size = 0;
			reply.msgh_remote_port = IntPtr.Zero;
			reply.msgh_local_port = IntPtr.Zero;
			reply.msgh_reserved = 0;
			reply.msgh_id = 0;

			while (true) {
				//Console.WriteLine ("[{0:yyyy-mm-dd hh:MM:ss}] Waiting for a mach exception.", DateTime.Now);
				r = mach_msg (ref msg, MACH_RCV_MSG, MSG_SIZE, MSG_SIZE, exception_port, 0, MACH_PORT_NULL);

				if (r != MACH_MSG_SUCCESS)
					throw new Exception ("mach_msg");

				//Console.WriteLine ("Dumping exception msg:");
				//dump (msg, msg.msgh_size);

				Console.WriteLine ("[{0:yyyy-mm-dd hh:MM:ss}] Handling a mach exception.", DateTime.Now);

				reply.msgh_bits = 0x12;
				reply.msgh_size = 0x24;
				reply.msgh_remote_port = msg.msgh_remote_port;
				reply.msgh_local_port = IntPtr.Zero;
				reply.msgh_reserved = 0;
				reply.msgh_id = msg.msgh_id+0x64;
				unsafe {
					int *ptr = (int *)(((int) &reply.msgh_id)+(Marshal.SizeOf (typeof (int))*3));
					*(ptr) = 0x5;
				}
				
				//Console.WriteLine ("Dumping exception reply:");
				//dump (reply, reply.msgh_size);

				//Console.WriteLine ("[{0:yyyy-mm-dd hh:MM:ss}] Replying from a mach exception.", DateTime.Now);
				r = mach_msg (ref reply, MACH_SEND_MSG, reply.msgh_size, 0, msg.msgh_local_port, 0, MACH_PORT_NULL);

				if (r != MACH_MSG_SUCCESS)
					throw new Exception ("mach_msg reply");
			}
		}

		internal static void dump (mach_msg msg, uint size) {
			unsafe {
				int ctr = 0;
				int ln = 0;
				void *msgptr = &msg;
				for (int i = 0; i < size; i++) {
					if (ln == 0 && ctr == 0) 
						Console.Write ("\t0x{0:x} ", ((int)msgptr)+i);
					byte b = Marshal.ReadByte ((IntPtr)((int)msgptr+i));
					Console.Write ("{0:x2}", b);
					++ctr;
					if (ctr == 4) {
						Console.Write (" ");
						ctr = 0;
						ln++;
					}
					if (ln == 4) {
						Console.WriteLine ();
						ln = 0;
					}
				}
				for (int i = 0; i < (4-ln); i++)
					Console.Write ("00000000 ");
				Console.WriteLine ();
			}
		}

		[DllImport ("/usr/lib/libc.dylib")]
		internal extern static int mach_msg (ref mach_msg msg, uint message_type, uint snd_size, int rcv_size, IntPtr exception_port, int unknown, int mach_port);
		
		[DllImport ("/usr/lib/libc.dylib")]
		internal extern static int task_set_exception_ports (IntPtr mach_task_t, int exception_mask, IntPtr exception_port, int exception_state, int thread_state);

		[DllImport ("/usr/lib/libc.dylib")]
		internal extern static int mach_port_extract_right (IntPtr mach_task_t, IntPtr a_exception_port, uint mach_msg_type, ref IntPtr b_exception_port, ref IntPtr type);

		[DllImport ("/usr/lib/libc.dylib")]
		internal extern static int mach_port_insert_right (IntPtr mach_task_t, IntPtr a_exception_port, IntPtr b_exception_port, uint mach_msg_type);

		[DllImport ("/usr/lib/libc.dylib")]
		internal extern static int mach_port_allocate (IntPtr mach_task_t, uint mach_port_right_t, ref IntPtr exception_port);
		
		[DllImport ("/usr/lib/libc.dylib")]
		internal extern static int task_for_pid (IntPtr mach_task_t, uint pid, ref IntPtr task);
		
		[DllImport ("/usr/lib/libc.dylib")]
		internal extern static IntPtr mach_task_self ();
		
		[DllImport ("/usr/lib/libc.dylib")]
		internal extern static uint getpid ();
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct mach_msg {
		[FieldOffset (0)]
		internal uint msgh_bits;
		[FieldOffset (4)]
		internal uint msgh_size;
		[FieldOffset (8)]
		internal IntPtr msgh_remote_port;
		[FieldOffset (12)]
		internal IntPtr msgh_local_port;
		[FieldOffset (16)]
		internal uint msgh_reserved;
		[FieldOffset (20)]
		internal int msgh_id;
		[FieldOffset (508)]
		internal IntPtr buffer;
	}
}
