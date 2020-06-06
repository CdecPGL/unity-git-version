/*
The MIT License (MIT)

Copyright (c) 2018-2020 Cdec

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;

namespace PlanetaGameLabo {
	/// <summary>
	/// This is a scriptable to hold a version string in executables, and this is created in prebuild process and removed in postbuild process.
	/// This object is included in executables and refered from scripts in executables.
	/// In editor, this object is not created and refered.
	/// </summary>
	public class GitVersionHolder : ScriptableObject {
		/// <summary>
		/// A name of the asset file to hold a version string.
		/// </summary>
		public const string ASSET_NAME = "version";
		/// <summary>
		/// A path of the asset file to hold a version string.
		/// This is refered when load version holder.
		/// </summary>
		public const string ASSET_PATH = GitVersion.RESOURCE_ASSET_DIRECTORY + ASSET_NAME;

		[SerializeField]
		public GitVersion.Version version = new GitVersion.Version { versionString = "Unknown Version", isValid = false };
	}
}
