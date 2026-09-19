using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Challenge.Utils.Extensions.Assemblies;

/// <summary>
/// <see cref="Assembly"/> extensions
/// </summary>
[PublicAPI]
public static class AssemblyExtensions
{
    /// <param name="assembly">Assembly instance</param>
    extension(Assembly assembly)
    {
        /// <summary>
        /// The file <see cref="Version"/> for the given assembly
        /// </summary>
        /// <returns>The file <see cref="Version"/> for the given assembly</returns>
        public Version GetFileVersion => new(FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion!);
    }
}
