using System;
using System.Collections.Generic;
using System.IO;

namespace Acklann.GlobN
{
    /// <summary>
    /// Contains the string to glob extensions.
    /// </summary>
    public static class GlobExtensionMethods
    {
        // Glob

        /// <summary>
        /// Returns the specified segments of a path in a new string; starting from right to left.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="segments">The number of segments to return.</param>
        /// <returns>The specified segments of the path.</returns>
        /// <exception cref="ArgumentNullException">path</exception>
        public static string SplitPath(this Glob path, int segments)
        {
            return SplitPath(path.ToString(), segments);
        }

        /// <summary>
        /// Moves the path up a directory the specified amount of times.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="times">The amount of directories to move up.</param>
        /// <returns>Returns a parent directory of the path.</returns>
        /// <exception cref="ArgumentException">path</exception>
        public static string MoveUpDirectory(this Glob path, int times)
        {
            return MoveUpDirectory(path.ToString(), times);
        }

        /// <summary>
        /// Expands the name of each environment variable (%variable%) and 'up directory' expression ('..\') embedded in the <see cref="Glob" />.
        /// </summary>
        /// <param name="pattern">The glob pattern.</param>
        /// <param name="directory">The current directory (default: <see cref="Environment.CurrentDirectory"/>).</param>
        /// <param name="expandVariables">if set to <c>true</c> expands the environment variables within the <paramref name="pattern" /> and <paramref name="directory" />.</param>
        /// <returns>A string with each variable replaced by its value.</returns>
        public static string ExpandPath(this Glob pattern, string directory = null, bool expandVariables = true)
        {
            return ExpandPath(pattern.ToString(), directory, expandVariables);
        }

        /// <summary>
        /// Returns the files from the specified directory that matches the <paramref name="pattern"/>.
        /// </summary>
        /// <param name="pattern">The glob pattern.</param>
        /// <param name="directory">The current directory (default: <see cref="Environment.CurrentDirectory"/>).</param>
        /// <param name="expandVariables">if set to <c>true</c> expands the environment variables within the <paramref name="pattern"/> and <paramref name="directory"/>.</param>
        /// <returns>The files that match the glob pattern from the directory.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="directory" /> do not exist.</exception>
        public static IEnumerable<string> ResolvePath(this Glob pattern, string directory = null, bool expandVariables = true)
        {
            if (string.IsNullOrEmpty(directory)) directory = Environment.CurrentDirectory;
            if (expandVariables) directory = Environment.ExpandEnvironmentVariables(directory);

            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            else if (!Directory.Exists(directory)) throw new DirectoryNotFoundException($"Could not find '{directory}'.");
            else
            {
                pattern.ExpandVariables = expandVariables;
                int totalUpOperators = GetUpOperators(pattern.ToString(), out string notUsed);
                directory = MoveUpDirectory(directory, totalUpOperators);

                foreach (var path in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
                    if (pattern.IsMatch(path))
                    {
                        yield return path;
                    }
            }
        }

        // String

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
        /// Expands the name of each environment variable (%variable%) and 'up directory' expression ('..\') embedded in the <see cref="Glob" />.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="directory">The current directory (default: <see cref="Environment.CurrentDirectory"/>).</param>
        /// <param name="expandVariables">if set to <c>true</c> expands the environment variables within the <paramref name="relativePath" /> and <paramref name="directory" />.</param>
        /// <returns>A string with each variable replaced by its value.</returns>
        public static string ExpandPath(this string relativePath, string directory = null, bool expandVariables = true)
        {
            if (string.IsNullOrEmpty(directory)) directory = Environment.CurrentDirectory;

            int totalUpOperators = GetUpOperators(relativePath, out relativePath);
            directory = MoveUpDirectory(directory, totalUpOperators);

            if (expandVariables) return Environment.ExpandEnvironmentVariables(Path.Combine(directory, relativePath ?? string.Empty) ?? string.Empty);
            else return Path.Combine(directory, relativePath ?? string.Empty);
        }

        /// <summary>
        /// Returns the files from the specified directory that matches the <paramref name="pattern" />.
        /// </summary>
        /// <param name="directory">The current directory (default: <see cref="Environment.CurrentDirectory" />).</param>
        /// <param name="pattern">The glob pattern.</param>
        /// <param name="expandVariables">if set to <c>true</c> expands the environment variables within the <paramref name="pattern" /> and <paramref name="directory" />.</param>
        /// <returns>The files that match the glob pattern from the directory.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern" /> is null.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="directory" /> do not exist.</exception>
        public static IEnumerable<string> GetFiles(this string directory, Glob pattern, bool expandVariables = true)
        {
            return ResolvePath(pattern, directory, expandVariables);
        }

        /// <summary>
        /// Returns the files from the specified directory that matches the <paramref name="pattern"/>.
        /// </summary>
        /// <param name="pattern">The glob pattern.</param>
        /// <param name="directory">The current directory (default: <see cref="Environment.CurrentDirectory"/>).</param>
        /// <param name="expandVariables">if set to <c>true</c> expands the environment variables within the <paramref name="pattern"/> and <paramref name="directory"/>.</param>
        /// <returns>The files that match the glob pattern from the directory.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="directory" /> do not exist.</exception>
        public static IEnumerable<string> ResolvePath(this string pattern, string directory = null, bool expandVariables = true)
        {
            return ResolvePath(new Glob(pattern), directory, expandVariables);
        }

        // DirectoryInfo

        /// <summary>
        /// Returns the files from the specified directory that matches the <paramref name="pattern"/>.
        /// </summary>
        /// <param name="directory">The current directory.</param>
        /// <param name="pattern">The glob pattern.</param>
        /// <param name="expandVariables">if set to <c>true</c> expands the environment variables within the <paramref name="pattern"/> and <paramref name="directory"/>.</param>
        /// <returns>The files that match the glob pattern from the directory.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="directory" /> do not exist.</exception>
        public static IEnumerable<FileInfo> GetFiles(this DirectoryInfo directory, Glob pattern, bool expandVariables = true)
        {
            foreach (var filePath in ResolvePath(pattern, directory.FullName, expandVariables))
            {
                yield return new FileInfo(filePath);
            };
        }

        // Collections

        /// <summary>
        /// Returns the items from the specified <paramref name="collection"/> that matches the <paramref name="pattern"/>.
        /// </summary>
        /// <param name="collection">The collection of file paths.</param>
        /// <param name="pattern">The glob pattern.</param>
        /// <param name="negate">if set to <c>true</c> return the files that do not match instead.</param>
        /// <returns>The files that matches the pattern.</returns>
        public static IEnumerable<string> Filter(this IEnumerable<string> collection, Glob pattern, bool negate = false)
        {
            foreach (var file in collection)
            {
                if (pattern.IsMatch(file) == !negate) yield return file;
            }
        }

        #region Private Members

        internal static bool IsWildcard(this char character)
        {
            return character == '*' || character == '?';
        }

        internal static bool IsDirectorySeparator(this char character)
        {
            return character == '\\' || character == '/';
        }

        internal static int GetUpOperators(this string relativePath, out string trimmedPath)
        {
            trimmedPath = relativePath;
            if (string.IsNullOrEmpty(relativePath)) return 0;
            else
            {
                char i, ii, iii = '\0';
                int counter = 0, n = 0;

                do
                {
                    i = (n >= 2) ? relativePath[n - 2] : '\0';
                    ii = (n >= 1) ? relativePath[n - 1] : '\0';
                    iii = relativePath[n];

                    if (i == '.' && ii == '.' && (iii == '\\' || iii == '/')) counter++;
                } while (((iii == '.' || iii == '\\' || iii == '/')) && ++n < relativePath.Length);

                trimmedPath = relativePath.Substring(n, (relativePath.Length - n));
                return counter;
            }
        }

        #endregion Private Members
    }
}