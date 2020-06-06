﻿/*
The MIT License (MIT)

Copyright (c) 2018-2020 Cdec

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using UnityEngine;

namespace PlanetaGameLabo.UnityGitVersion
{
    /// <summary>
    /// An utility class to get a version string which is generated from commit id and tags of git.
    /// </summary>
    public class GitVersion
    {
        /// <summary>
        /// Version information.
        /// </summary>
        [Serializable]
        public struct Version
        {
            /// <summary>
            /// A string which represents version.
            /// This string is based on version string format in setting.
            /// </summary>
            public string versionString;

            /// <summary>
            /// True if version is generated correctly.
            /// </summary>
            public bool isValid;

            /// <summary>
            /// A tag of last commit from git.
            /// If there is no tag in last commit, this field is null.
            /// </summary>
            public string tag;

            /// <summary>
            /// A last commit id from git.
            /// If commit id is not available, this field is null.
            /// </summary>
            public string commitId;

            /// <summary>
            /// A hash of difference between last commit and current state.
            /// If there are no changes from last commit, this field is null.
            /// </summary>
            public string diffHash;

            /// <summary>
            /// Consider version is matched when version is unknown if this is true.
            /// </summary>
            public bool allowUnknownVersionMatching;
        }

        /// <summary>
        /// A path of directory used as resource asset path for GitVersion.
        /// </summary>
        public const string resourceAssetDirectory = "PlanetaGameLabo/GitVersion/";

        /// <summary>
        /// Access to generated or loeaded version information.
        /// </summary>
        public static Version version
        {
            get
            {
                if (_isInitialized || _gitVersionHolder)
                {
                    return isValid
                        ? _gitVersionHolder.version
                        : new Version {versionString = "Unknown Version", isValid = false};
                }

                //バージョンアセットをロードする
                _gitVersionHolder = Resources.Load<GitVersionHolder>(GitVersionHolder.assetPath);
                if (!_gitVersionHolder)
                {
                    Debug.LogError(
                        "Failed to get a version of GitVersion. Version generation may not be completed in building.");
                }

                _isInitialized = true;

                return isValid
                    ? _gitVersionHolder.version
                    : new Version {versionString = "Unknown Version", isValid = false};
            }
        }

        /// <summary>
        /// Return true if version information is succesfully generated or loaded.
        /// </summary>
        public static bool isValid => _gitVersionHolder != null;

        /// <summary>
        /// Check if my version matches to a parameter.
        /// </summary>
        /// <param name="targetVersion"></param>
        /// <returns>True if version matches.</returns>
        public static bool CheckIfVersionMatch(Version targetVersion)
        {
            if (!isValid)
            {
                return false;
            }

            if (!version.isValid || !targetVersion.isValid)
            {
                return version.allowUnknownVersionMatching;
            }

            if (!string.IsNullOrEmpty(version.tag) && version.tag == targetVersion.tag)
            {
                return version.diffHash == targetVersion.diffHash;
            }

            if (!string.IsNullOrEmpty(version.commitId) && version.commitId == targetVersion.commitId)
            {
                return version.diffHash == targetVersion.diffHash;
            }

            return false;
        }

        private static bool _isInitialized;
        private static GitVersionHolder _gitVersionHolder;
    }
}