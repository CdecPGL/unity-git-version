/*
The MIT License (MIT)

Copyright (c) 2018-2020 Cdec

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LibGit2Sharp;

namespace PlanetaGameLabo.UnityGitVersion.Editor
{
    /// <summary>
    /// A class to operate git and get information.
    /// </summary>
    public static class GitOperator
    {
        /// <summary>
        /// Check if git is installed and available.
        /// </summary>
        /// <returns>True if git is available.</returns>
        public static bool CheckIfGitIsAvailable()
        {
            return Repository.IsValid(_repositoryPath);
        }

        /// <summary>
        /// Check if there are any changes in the repository from the last commit.
        /// </summary>
        /// <returns>True if there are changes.</returns>
        public static bool CheckIfRepositoryIsChangedFromLastCommit()
        {
            var repo = GetGitRepository();
            return repo.RetrieveStatus().IsDirty;
        }

        /// <summary>
        /// Get current tag of the specified commit.
        /// </summary>
        /// <param name="commitId">An id of the target commit</param>
        /// <returns>Tag. If there are no tags for the commit ID or git is not available, this function returns empty string.</returns>
        public static string GetTagFromCommitId(string commitId)
        {
            var repo = GetGitRepository();
            return repo.Tags.FirstOrDefault(t => t.Target.Sha == commitId)?.FriendlyName ?? "";
        }

        /// <summary>
        /// Get current a last commit id of the current branch.
        /// </summary>
        /// <param name="shortVersion">Returns short commit id if this is true.</param>
        /// <returns>Commit ID. If there are no commits or git is not available, this function returns empty string.</returns>
        public static string GetLastCommitId(bool shortVersion)
        {
            var repo = GetGitRepository();
            var hash = repo.Head.Tip.Sha;
            return shortVersion ? hash.Substring(0, 7) : hash;
        }

        /// <summary>
        /// Get a hash of the difference between current worktree and the last commit.
        /// </summary>
        /// <param name="shortVersion">Returns short hash with 7 characters if this is true.</param>
        /// <returns>A SHA1 hash of diff between current repository and last commit</returns>
        public static string GetHashOfChangesFromLastCommit(bool shortVersion)
        {
	        var repo = GetGitRepository();
	        //　Available types of T is either Patch, TreeChanges or PatchStats.
	        // Currently, we use Patch type because we use a patch content string to get a hash of difference between worktree and the last commit.
	        // Untracked files are not included in the result of "git diff" command but the are included in the result of Diff.Compare method of LibGit2Sharp with DiffTargets.WorkingDirectory.
	        var diffResult = repo.Diff.Compare<Patch>(repo.Head.Tip.Tree, DiffTargets.WorkingDirectory);

	        // Generate SHA1 hash which is used in Git
	        var hash = GetHashString<SHA1CryptoServiceProvider>(diffResult);
	        // Make hash length 7 which is same as the length of short hash in Git if short flag is enabled
	        return shortVersion ? hash.Substring(0, 7) : hash;
        }

        /// <summary>
        /// Get a result of git describe
        /// </summary>
        /// <returns>A result of git describe</returns>
        public static string GetDescription(bool enableLightweightTagMatch)
        {
	        var repo = GetGitRepository();
            return repo.Describe(repo.Head.Tip, new DescribeOptions { Strategy = enableLightweightTagMatch ? DescribeStrategy.Tags : DescribeStrategy.Default });
        }

        private const string _repositoryPath = "./";
        private static IRepository _gitRepository;

        private static IRepository GetGitRepository()
        {
            return _gitRepository ?? (_gitRepository = new Repository(_repositoryPath));
        }

        private static string GetHashString<T>(string text) where T : HashAlgorithm, new()
        {
            var data = Encoding.UTF8.GetBytes(text);
            using (var algorithm = new T())
            {
                var hashBytes = algorithm.ComputeHash(data);
                // バイト型配列を16進数文字列に変換
                var result = new StringBuilder();
                foreach (var hashByte in hashBytes)
                {
                    result.Append(hashByte.ToString("x2"));
                }

                return result.ToString();
            }
        }
    }
}