using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Autodesk.Forge.WpfCsharp {

	public class DebugUtils {
		const int DBG_CONTINUE =0x00010002 ;
		const int DBG_EXCEPTION_NOT_HANDLED =unchecked((int)0x80010001) ;

		enum DebugEventType : int {
			CREATE_PROCESS_DEBUG_EVENT =3, //Reports a create-process debugging event. The value of u.CreateProcessInfo specifies a CREATE_PROCESS_DEBUG_INFO structure.
			CREATE_THREAD_DEBUG_EVENT =2, //Reports a create-thread debugging event. The value of u.CreateThread specifies a CREATE_THREAD_DEBUG_INFO structure.
			EXCEPTION_DEBUG_EVENT =1, //Reports an exception debugging event. The value of u.Exception specifies an EXCEPTION_DEBUG_INFO structure.
			EXIT_PROCESS_DEBUG_EVENT =5, //Reports an exit-process debugging event. The value of u.ExitProcess specifies an EXIT_PROCESS_DEBUG_INFO structure.
			EXIT_THREAD_DEBUG_EVENT =4, //Reports an exit-thread debugging event. The value of u.ExitThread specifies an EXIT_THREAD_DEBUG_INFO structure.
			LOAD_DLL_DEBUG_EVENT =6, //Reports a load-dynamic-link-library (DLL) debugging event. The value of u.LoadDll specifies a LOAD_DLL_DEBUG_INFO structure.
			OUTPUT_DEBUG_STRING_EVENT =8, //Reports an output-debugging-string debugging event. The value of u.DebugString specifies an OUTPUT_DEBUG_STRING_INFO structure.
			RIP_EVENT =9, //Reports a RIP-debugging event (system debugging error). The value of u.RipInfo specifies a RIP_INFO structure.
			UNLOAD_DLL_DEBUG_EVENT =7, //Reports an unload-DLL debugging event. The value of u.UnloadDll specifies an UNLOAD_DLL_DEBUG_INFO structure.
		}

		[StructLayout (LayoutKind.Sequential)]
		struct DEBUG_EVENT {
			[MarshalAs (UnmanagedType.I4)]
			public DebugEventType dwDebugEventCode ;
			public int dwProcessId ;
			public int dwThreadId ;
			[MarshalAs (UnmanagedType.ByValArray, SizeConst = 1024)]
			public byte [] bytes ;
		}

		[DllImport ("Kernel32.dll", SetLastError = true)]
		static extern bool DebugActiveProcess (int dwProcessId) ;
		[DllImport ("Kernel32.dll", SetLastError = true)]
		static extern bool WaitForDebugEvent ([Out] out DEBUG_EVENT lpDebugEvent, int dwMilliseconds) ;
		[DllImport ("Kernel32.dll", SetLastError = true)]
		static extern bool ContinueDebugEvent (int dwProcessId, int dwThreadId, int dwContinueStatus) ;
		[DllImport ("Kernel32.dll", SetLastError = true)]
		public static extern bool IsDebuggerPresent () ;
		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		static extern bool CheckRemoteDebuggerPresent (IntPtr hProcess, ref bool isDebuggerPresent) ;

		public static void StartDebuggerThread (int processId =0) {
			if ( processId == 0 )
				processId =Process.GetCurrentProcess ().Id ;
			new Thread (DebuggerThread) { IsBackground = true, Name = "DebuggerThread" }
				.Start (processId) ;
		}

		private static void DebuggerThread (object arg) {
			while ( true ) {
				bool isDebuggerPresent =false ;
				CheckRemoteDebuggerPresent (Process.GetCurrentProcess ().Handle, ref isDebuggerPresent) ;
				if ( isDebuggerPresent )
					Application.Current.Dispatcher.BeginInvokeShutdown (DispatcherPriority.ApplicationIdle) ;
			}
		}

	}

}
