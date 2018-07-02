﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Acklann.GlobN
{
    /// <summary>
    /// Contains the pattern matching extension methods.
    /// </summary>
    public static class GlobExtensions
    {
        /* Glob */

        /// <summary>
        /// Expands the name of each environment variable (%variable%) and 'up directory' expression ('..\') embedded in the <see cref="Glob" />.
        /// </summary>
        /// <param name="pattern">The glob pattern.</param>
        /// <param name="directory">The current directory (default: <see cref="Environment.CurrentDirectory"/>).</param>
        /// <param name="expandVariables">if set to <c>true</c> expands the environment variables within the <paramref name="pattern" /> and <paramref name="directory" />.</param>
        /// <returns>A string with each variable replaced by its value.</returns>
        public static string ExpandPath(this Glob pattern, string directory = null, bool expandVariables = false)
        {
            return ExpandPath(pattern.ToString(), directory, expandVariables);
        }

        /// <summary>
        /// Returns the files from the specified directory that matches the <paramref name="pattern"/>.
        /// </summary>
        /// <param name="pattern">The glob pattern.</param>
        /// <param name="directory">The current directory (default: <see cref="Environment.CurrentDirectory"/>).</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories. (default <see cref="SearchOption.AllDirectories"/>)</param>
        /// <param name="expandVariables">if set to <c>true</c> expands the environment variables within the <paramref name="pattern"/> and <paramref name="directory"/>.</param>
        /// <returns>The files that match the glob pattern from the directory.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="directory" /> do not exist.</exception>
        public static IEnumerable<string> ResolvePath(this Glob pattern, string directory = null, SearchOption searchOption = SearchOption.AllDirectories, bool expandVariables = false)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            // Checking to see if the pattern denotes a existing path. If so, return it.
            string absolutePath = (expandVariables ? Environment.ExpandEnvironmentVariables(pattern.ToString()) : pattern.ToString());
            if (File.Exists(absolutePath))
            {
                yield return absolutePath;
                yield break;
            }

            // Lookup all files in the directory that match the pattern.
            if (string.IsNullOrEmpty(directory)) directory = Environment.CurrentDirectory;
            if (expandVariables) directory = Environment.ExpandEnvironmentVariables(directory);
            if (!Directory.Exists(directory)) throw new DirectoryNotFoundException($"Could not find folder at '{directory}'.");

            directory = PathExtensions.MoveUpDirectory(directory, GetUpOperators(pattern, out string trimmedPattern));

            foreach (string path in Directory.EnumerateFiles(directory, "*", searchOption))
                if (pattern.IsMatch(path, expandVariables))
                {
                    yield return path;
                }
        }

        /* String */

        /// <summary>
        /// Expands the name of each environment variable (%variable%) and 'up directory' expression ('..\') embedded in the <see cref="Glob" />.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="directory">The current directory (default: <see cref="Environment.CurrentDirectory"/>).</param>
        /// <param name="expandVariables">if set to <c>true</c> expands the environment variables within the <paramref name="relativePath" /> and <paramref name="directory" />.</param>
        /// <returns>A string with each variable replaced by its value.</returns>
        public static string ExpandPath(this string relativePath, string directory = null, bool expandVariables = false)
        {
            if (string.IsNullOrEmpty(directory)) directory = Environment.CurrentDirectory;

            int totalUpOperators = GetUpOperators(relativePath, out relativePath);
            directory = PathExtensions.MoveUpDirectory(directory, totalUpOperators);

            if (expandVariables) return Environment.ExpandEnvironmentVariables(Path.Combine(directory, relativePath ?? string.Empty) ?? string.Empty);
            else return Path.Combine(directory, relativePath ?? string.Empty);
        }

        /// <summary>
        /// Returns the files from the specified directory that matches the <paramref name="pattern"/>.
        /// </summary>
        /// <param name="pattern">The glob pattern.</param>
        /// <param name="directory">The current directory (default: <see cref="Environment.CurrentDirectory"/>).</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories. (default <see cref="SearchOption.AllDirectories"/>)</param>
        /// <param name="expandVariables">if set to <c>true</c> expands the environment variables within the <paramref name="pattern"/> and <paramref name="directory"/>.</param>
        /// <returns>The files that match the glob pattern from the directory.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="directory" /> do not exist.</exception>
        public static IEnumerable<string> ResolvePath(this string pattern, string directory = null, SearchOption searchOption = SearchOption.AllDirectories, bool expandVariables = false)
        {
            return ResolvePath(new Glob(pattern), directory, searchOption, expandVariables);
        }

        /// <summary>
        /// Returns the files from the specified directory that matches the <paramref name="pattern" />.
        /// </summary>
        /// <param name="directory">The current directory (default: <see cref="Environment.CurrentDirectory" />).</param>
        /// <param name="pattern">The glob pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories. (default <see cref="SearchOption.AllDirectories"/>)</param>
        /// <param name="expandVariables">if set to <c>true</c> expands the environment variables within the <paramref name="pattern" /> and <paramref name="directory" />.</param>
        /// <returns>The files that match the glob pattern from the directory.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern" /> is null.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="directory" /> do not exist.</exception>
        public static IEnumerable<string> GetFiles(this string directory, Glob pattern, SearchOption searchOption = SearchOption.AllDirectories, bool expandVariables = false)
        {
            return ResolvePath(pattern, directory, searchOption, expandVariables);
        }

        /* DirectoryInfo */

        /// <summary>
        /// Returns the files from the specified directory that matches the <paramref name="pattern"/>.
        /// </summary>
        /// <param name="directory">The current directory.</param>
        /// <param name="pattern">The glob pattern.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories. (default <see cref="SearchOption.AllDirectories"/>)</param>
        /// <param name="expandVariables">if set to <c>true</c> expands the environment variables within the <paramref name="pattern"/> and <paramref name="directory"/>.</param>
        /// <returns>The files that match the glob pattern from the directory.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="directory" /> do not exist.</exception>
        public static IEnumerable<FileInfo> GetFiles(this DirectoryInfo directory, Glob pattern, SearchOption searchOption = SearchOption.AllDirectories, bool expandVariables = false)
        {
            foreach (var filePath in ResolvePath(pattern, directory.FullName, searchOption, expandVariables))
            {
                yield return new FileInfo(filePath);
            };
        }

        /* Collections */

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

        /* ===== */

        internal static bool IsaWildcard(this char character)
        {
            return character == '*' || character == '?';
        }

        internal static bool IsaDirectorySeparator(this char character)
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
                int ctr = 0, n = 0;

                do
                {
                    i = (n >= 2) ? relativePath[n - 2] : '\0';
                    ii = (n >= 1) ? relativePath[n - 1] : '\0';
                    iii = relativePath[n];

                    if (i == '.' && ii == '.' && (iii == '\\' || iii == '/')) ctr++;
                } while (((iii == '.' || iii == '\\' || iii == '/')) && ++n < relativePath.Length);

                trimmedPath = relativePath.Substring(n, (relativePath.Length - n));
                return ctr;
            }
        }
    }
}