/// --------------------------------------
/// .NET Bot for Internet Relay Chat (IRC)
/// Copyright (c) ThomasKerman 2015
/// QIRC is licensed under the MIT License
/// --------------------------------------

/// System
using System;
using System.IO;
using PathIO = System.IO.Path;

/// <summary>
/// The main namespace. Here's everything that executes actively.
/// </summary>
namespace QIRC
{
    /// <summary>
    /// Path wrapper to return the current Directory
    /// </summary>
    public class Path
    {
        /// <summary>
        /// The respective path, relative to the main executable.
        /// </summary>
        public String name { get; set; }

        /// <summary>
        /// Create the path object.
        /// </summary>
        /// <param name="name">See <see cref="name"/></param>
        public Path(String name)
        {
            this.name = name;
        }

        /// <summary>
        /// Returns if the given path exists
        /// </summary>
        public Boolean Exists()
        {
            return File.Exists(PathIO.Combine(Directory.GetCurrentDirectory(), name));
        }

        /// <summary>
        /// Returns if the given path exists.
        /// This one has an additional check for a file that is located at the path.
        /// </summary>
        public Boolean Exists(String file)
        {
            return File.Exists(PathIO.Combine(Directory.GetCurrentDirectory(), name, file));
        }

        /// <summary>
        /// Converts the path into a string representation of the (absolute) path.
        /// </summary>
        public static implicit operator String(Path path)
        {
            String p = PathIO.Combine(Directory.GetCurrentDirectory(), path.name);
            return p;
        }

        /// <summary>
        /// Converts a string into a path representation.
        /// </summary>
        public static implicit operator Path(String path)
        {
            if (path.StartsWith(Directory.GetCurrentDirectory()))
                path = path.Remove(0, Directory.GetCurrentDirectory().Length + 1);
            return new Path(path);
        }
    }
}