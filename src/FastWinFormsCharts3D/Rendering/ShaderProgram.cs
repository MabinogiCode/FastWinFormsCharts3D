// Copyright (c) 2026 MabinogiCode. All rights reserved.

using System.Numerics;
using System.Reflection;
using FastWinFormsCharts3D.Exceptions;
using Silk.NET.OpenGL;

namespace FastWinFormsCharts3D.Rendering;

/// <summary>
/// Wraps an OpenGL shader program compiled from embedded GLSL resources.
/// Caches uniform locations to avoid repeated <c>glGetUniformLocation</c> calls.
/// </summary>
public sealed class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly Dictionary<string, int> _uniformLocationCache = [];
    private bool _disposed;

    private ShaderProgram(GL gl, uint handle)
    {
        _gl = gl;
        _handle = handle;
    }

    /// <summary>
    /// Compiles and links a shader program from two embedded assembly resources.
    /// </summary>
    /// <param name="gl">The active OpenGL API.</param>
    /// <param name="vertResourceName">
    /// Fully-qualified name of the vertex shader embedded resource
    /// (e.g. <c>FastWinFormsCharts3D.Rendering.Shaders.scatter.vert</c>).
    /// </param>
    /// <param name="fragResourceName">
    /// Fully-qualified name of the fragment shader embedded resource.
    /// </param>
    /// <returns>A compiled and linked <see cref="ShaderProgram"/>.</returns>
    /// <exception cref="ShaderCompilationException">Thrown when compilation or linking fails.</exception>
    public static ShaderProgram FromEmbeddedResource(GL gl, string vertResourceName, string fragResourceName)
    {
        string vertSource = ReadEmbeddedResource(vertResourceName);
        string fragSource = ReadEmbeddedResource(fragResourceName);

        uint vertShader = CompileShader(gl, ShaderType.VertexShader, vertSource);
        uint fragShader = CompileShader(gl, ShaderType.FragmentShader, fragSource);

        uint program = gl.CreateProgram();
        gl.AttachShader(program, vertShader);
        gl.AttachShader(program, fragShader);
        gl.LinkProgram(program);

        gl.GetProgram(program, GLEnum.LinkStatus, out int linkStatus);
        if (linkStatus == 0)
        {
            string log = gl.GetProgramInfoLog(program);
            gl.DeleteProgram(program);
            throw new ShaderCompilationException($"Shader program linking failed:\n{log}");
        }

        gl.DeleteShader(vertShader);
        gl.DeleteShader(fragShader);

        return new ShaderProgram(gl, program);
    }

    /// <summary>Activates this shader program for subsequent draw calls.</summary>
    public void Use() => _gl.UseProgram(_handle);

    /// <summary>Sets a <see cref="Matrix4x4"/> uniform by name.</summary>
    /// <param name="name">The GLSL uniform variable name.</param>
    /// <param name="value">The matrix value to upload.</param>
    public unsafe void SetUniform(string name, Matrix4x4 value)
    {
        int location = GetUniformLocation(name);
        if (location >= 0)
        {
            _gl.UniformMatrix4(location, 1, false, (float*)&value);
        }
    }

    /// <summary>Sets a <c>float</c> uniform by name.</summary>
    /// <param name="name">The GLSL uniform variable name.</param>
    /// <param name="value">The float value to upload.</param>
    public void SetUniform(string name, float value)
    {
        int location = GetUniformLocation(name);
        if (location >= 0)
        {
            _gl.Uniform1(location, value);
        }
    }

    /// <summary>Sets a <see cref="Vector4"/> uniform by name.</summary>
    /// <param name="name">The GLSL uniform variable name.</param>
    /// <param name="value">The vector value to upload.</param>
    public void SetUniform(string name, Vector4 value)
    {
        int location = GetUniformLocation(name);
        if (location >= 0)
        {
            _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _gl.DeleteProgram(_handle);
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    private int GetUniformLocation(string name)
    {
        if (_uniformLocationCache.TryGetValue(name, out int cached))
        {
            return cached;
        }

        int location = _gl.GetUniformLocation(_handle, name);
        _uniformLocationCache[name] = location;
        return location;
    }

    private static uint CompileShader(GL gl, ShaderType type, string source)
    {
        uint shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);

        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
        if (status == 0)
        {
            string log = gl.GetShaderInfoLog(shader);
            gl.DeleteShader(shader);
            throw new ShaderCompilationException($"{type} compilation failed:\n{log}");
        }

        return shader;
    }

    private static string ReadEmbeddedResource(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
