// Copyright (c) 2026 MabinogiCode. All rights reserved.

namespace FastWinFormsCharts3D.Exceptions;

/// <summary>
/// Thrown when a GLSL shader fails to compile or a shader program fails to link.
/// The exception message includes the full OpenGL info log.
/// </summary>
public sealed class ShaderCompilationException : InvalidOperationException
{
    /// <summary>Initializes a new instance with the specified error message.</summary>
    /// <param name="message">The GLSL info log returned by the driver.</param>
    public ShaderCompilationException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with a message and an inner exception.</summary>
    /// <param name="message">The error description.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public ShaderCompilationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
