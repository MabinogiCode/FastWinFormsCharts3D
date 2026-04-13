// Copyright (c) 2026 MabinogiCode. All rights reserved.

namespace FastWinFormsCharts3D.Exceptions;

/// <summary>
/// Thrown when creation or activation of the OpenGL/WGL context fails.
/// The <see cref="Win32ErrorCode"/> property carries the HRESULT from
/// <see cref="System.Runtime.InteropServices.Marshal.GetLastWin32Error"/>.
/// </summary>
public sealed class OpenGLContextException : InvalidOperationException
{
    /// <summary>Initializes a new instance with the specified message and Win32 error code.</summary>
    /// <param name="message">A description of the failure.</param>
    /// <param name="win32ErrorCode">The Win32 error code at the time of failure.</param>
    public OpenGLContextException(string message, int win32ErrorCode)
        : base(message)
    {
        Win32ErrorCode = win32ErrorCode;
    }

    /// <summary>Initializes a new instance with a message and an inner exception.</summary>
    /// <param name="message">A description of the failure.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public OpenGLContextException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Gets the Win32 error code returned by the OS at the time of failure.</summary>
    public int Win32ErrorCode { get; }
}
