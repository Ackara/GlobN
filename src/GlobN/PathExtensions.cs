using System;
using System.IO;
using System.Text;

namespace Acklann.GlobN
{
    /// <summary>
    /// Contains the path helper methods.
    /// </summary>
    public static class PathExtensions
    {
        /* String */

        /// <summary>
        /// Moves the path up a directory the specified amount of times.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="times">The amount of directories to move up.</param>
        /// <returns>Returns a parent directory of the path.</returns>
        /// <exception cref="ArgumentException">path</exception>
        public static string MoveUpDirectory(this string path, int times)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException($"{nameof(path)} cannot be null or empty.", nameof(path));
            else
            {
                for (int i = 0; i < times; i++)
                    if (!string.IsNullOrEmpty(path))
                    {
                        path = Path.GetDirectoryName(path);
                    }
            }

            return path;
        }

        /// <summary>
        /// Returns the specified segments of a path in a new string; starting from right to left.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="segments">The number of segments to return.</param>
        /// <returns>The specified segments of the path.</returns>
        /// <exception cref="ArgumentNullException">path</exception>
        public static string SplitPath(this string path, int segments)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            else if (segments <= 0) return string.Empty;
            else
            {
                int i = 0, target = 0;
                for (i = path.Length - 1; i >= 0; i--)
                {
                    if (path[i].IsDirectorySeparator() && ++target == segments) break;
                }

                i = i < 0 ? 0 : i;
                return path.Substring(i, (path.Length - i));
            }
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="contents">The contents.</param>
        public static void WriteToFile(this string path, string contents)
        {
            WriteToFile(path, contents, Encoding.Default);
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="contents">The contents.</param>
        /// <param name="encoding">The encoding.</param>
        public static void WriteToFile(this string path, string contents, Encoding encoding)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            File.WriteAllText(path, contents, encoding);
        }
    }
}