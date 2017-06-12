////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2012-2017 Flax Engine. All rights reserved.
////////////////////////////////////////////////////////////////////////////////////
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Runtime.CompilerServices;

namespace FlaxEngine
{
	/// <summary>
	/// Application management utilities.
	/// </summary>
	public static partial class Application
	{
		/// <summary>
		/// Returns true if is running 64 bit application (otherwise 32 bit).
		/// </summary>
		[UnmanagedCall]
		public static bool Is64bitApp
		{
#if UNIT_TEST_COMPILANT
			get; set;
#else
			get { return Internal_Is64bitApp(); }
#endif
		}

		/// <summary>
		/// Gets the name of the computer machine.
		/// </summary>
		[UnmanagedCall]
		public static string ComputerName
		{
#if UNIT_TEST_COMPILANT
			get; set;
#else
			get { return Internal_GetComputerName(); }
#endif
		}

		/// <summary>
		/// Gets the name of the current user.
		/// </summary>
		[UnmanagedCall]
		public static string UserName
		{
#if UNIT_TEST_COMPILANT
			get; set;
#else
			get { return Internal_GetUserName(); }
#endif
		}

		/// <summary>
		/// Gets the current user locale culture name eg. "pl-PL" or "en-US".
		/// </summary>
		[UnmanagedCall]
		public static string UserLocaleName
		{
#if UNIT_TEST_COMPILANT
			get; set;
#else
			get { return Internal_GetUserLocaleName(); }
#endif
		}

		/// <summary>
		/// Gets size of the primary desktop.
		/// </summary>
		[UnmanagedCall]
		public static Vector2 DesktopSize
		{
#if UNIT_TEST_COMPILANT
			get; set;
#else
			get { Vector2 resultAsRef; Internal_GetDesktopSize(out resultAsRef); return resultAsRef; }
#endif
		}

		/// <summary>
		/// Gets size of the virtual desktop made of all the monitors attached.
		/// </summary>
		[UnmanagedCall]
		public static Vector2 VirtualDesktopSize
		{
#if UNIT_TEST_COMPILANT
			get; set;
#else
			get { Vector2 resultAsRef; Internal_GetVirtualDesktopSize(out resultAsRef); return resultAsRef; }
#endif
		}

#region Internal Calls
#if !UNIT_TEST_COMPILANT
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool Internal_Is64bitApp();
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string Internal_GetComputerName();
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string Internal_GetUserName();
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string Internal_GetUserLocaleName();
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void Internal_GetDesktopSize(out Vector2 resultAsRef);
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void Internal_GetVirtualDesktopSize(out Vector2 resultAsRef);
#endif
#endregion
	}
}
