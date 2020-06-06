/*
The MIT License (MIT)

Copyright (c) 2018-2020 Cdec

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace PlanetaGameLabo.UnityGitVersion.Editor
{
    /// <summary>
    /// A class including editor extension etc.
    /// </summary>
    public class GitVersionOnEditor : IPreprocessBuildWithReport
    {
        public const string resourceDirectory = _resourceRootDirectory + GitVersion.resourceAssetDirectory;
        public const string versionHolderPath = resourceDirectory + GitVersionHolder.assetName + ".asset";
        public const string versionSettingPath = _gitVersionAssetRootDirectory + "Editor/setting.asset";

        private const string _gitVersionAssetRootDirectory = "Assets/PlanetaGameLabo/GitVersion/";
        private const string _resourceRootDirectory = _gitVersionAssetRootDirectory + "Resources/";

        int IOrderedCallback.callbackOrder => 0;

        /// <summary>
        /// Generate a version from git.
        /// </summary>
        /// <returns>Generated version</returns>
        public static GitVersion.Version GenerateVersionFromGit()
        {
            //設定の読み込み
            var setting = AssetDatabase.LoadAssetAtPath<GitVersionSetting>(versionSettingPath);
            if (!setting)
            {
                Debug.LogWarning("Failed to load GitVersion setting. Default setting will be used.");
                setting = ScriptableObject.CreateInstance<GitVersionSetting>();
            }

            //バージョンの生成
            var version = new GitVersion.Version
            {
                commitId = GetLastCommitId(false),
                isValid = false,
                allowUnknownVersionMatching = setting.allowUnknownVersionMatching,
            };
            //Git操作に失敗
            if (version.commitId == null)
            {
                Debug.LogError("Failed to generate version from git.");
                return version;
            }

            //バージョン情報の設定
            var shortCommitId = GetLastCommitId(true);
            version.tag = GetTagFromCommitId(version.commitId);
            var isModified = CheckRepositoryChangesFromLastCommit();
            if (isModified)
            {
                version.diffHash = GetHashOfChangesFromLastCommit();
                if (version.tag == null)
                {
                    version.versionString = setting.versionStringFormatWithDiff;
                }
                else
                {
                    version.versionString = setting.versionStringFormatWithTagAndDiff;
                    version.versionString = Regex.Replace(version.versionString, "%{1}t", version.tag);
                }

                version.versionString = Regex.Replace(version.versionString, "%{1}d", version.diffHash);
            }
            else
            {
                if (version.tag == null)
                {
                    version.versionString = setting.versionStringFormat;
                }
                else
                {
                    version.versionString = setting.versionStringFormatWithTag;
                    version.versionString = Regex.Replace(version.versionString, "%{1}t", version.tag);
                }
            }

            version.versionString = Regex.Replace(version.versionString, "%{1}c", shortCommitId);
            version.versionString = Regex.Replace(version.versionString, "%{1}C", version.commitId);
            version.versionString = Regex.Replace(version.versionString, "%{2}", "%");
            version.isValid = true;
            return version;
        }

        /// <summary>
        /// Get current a last commit id of the current branch.
        /// </summary>
        /// <param name="shortVersion">Retruns short commit id if this is true.</param>
        /// <returns>Commit ID. If there are no commits or git is not available, this function returns null.</returns>
        public static string GetLastCommitId(bool shortVersion)
        {
            var commitId = ExecuteCommand("git log -n 1 --format=" + (shortVersion ? "%h" : "%H"))
                .Replace("\n", string.Empty);
            if (!string.IsNullOrEmpty(commitId))
            {
                return commitId;
            }

            Debug.LogError(
                "Failed to get commit id. Check if git is installed and the directory of this project is initialized as a git repository.");
            return null;
        }

        /// <summary>
        /// Get current a tag of the specified commit.
        /// </summary>
        /// <param name="commitId">An id of the target commit</param>
        /// <returns>Tag. If there are no tags or git is not available, this function returns null.</returns>
        public static string GetTagFromCommitId(string commitId)
        {
            var tag = ExecuteCommand("git tag -l --contains " + commitId).Replace("\n", string.Empty);
            return string.IsNullOrEmpty(tag) ? null : tag;
        }

        /// <summary>
        /// Check if there are any changes in the repository from last commit.
        /// </summary>
        /// <returns>Returns true if there are changes.</returns>
        public static bool CheckRepositoryChangesFromLastCommit()
        {
            return !string.IsNullOrEmpty(ExecuteCommand("git status --short").Replace("\n", string.Empty));
        }

        /// <summary>
        /// Get a hash of the difference between current repository and last commit.
        /// </summary>
        /// <returns>Hash of diff between current repository and last commit</returns>
        public static string GetHashOfChangesFromLastCommit()
        {
            // コミットしていない変更がある場合は最新コミットとの差分のハッシュを生成しバージョンに加える。
            // 異なる編集の存在するものは違うバージョン、同じ状態のものは同じバージョンであると保証するため
            var diff = ExecuteCommand("git add -N . ; git diff HEAD");
            var data = Encoding.UTF8.GetBytes(diff);
            // MD5ハッシュ生成。セキュリティは関係ないので、速度面からMD5を使う。
            var algorithm = new MD5CryptoServiceProvider();
            var bs = algorithm.ComputeHash(data);
            algorithm.Clear();
            // バイト型配列を16進数文字列に変換
            var diffHash = new StringBuilder();
            foreach (var b in bs)
            {
                diffHash.Append(b.ToString("x2"));
            }

            return diffHash.ToString();
        }

        /// <summary>
        /// Make directories if not exists.
        /// </summary>
        /// <param name="directoryPath">Directory path separated with "/".</param>
        public static void MakeAssetDirectoryRecursively(string directoryPath)
        {
            var directories = directoryPath.Split('/');
            var currentDirectory = "";
            foreach (var directory in directories)
            {
                var baseDirectory = currentDirectory;
                currentDirectory = Path.Combine(baseDirectory, directory);
                if (!AssetDatabase.IsValidFolder(currentDirectory))
                {
                    AssetDatabase.CreateFolder(baseDirectory, directory);
                }
            }
        }

        [MenuItem("Tools/GitVersion/Log VersionString")]
        private static void LogVersionString()
        {
            var versionString = GenerateVersionFromGit();
            Debug.Log(versionString.versionString);
        }

        private static string ExecuteCommand(string command)
        {
            //Processオブジェクトを作成
            var p = new System.Diagnostics.Process();
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    //ComSpec(cmd.exe)のパスを取得して、FileNameプロパティに指定
                    p.StartInfo.FileName = Environment.GetEnvironmentVariable("ComSpec");
                    p.StartInfo.Arguments = "/c " + command;
                    break;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                    p.StartInfo.FileName = Environment.GetEnvironmentVariable("/bin/bash");
                    p.StartInfo.Arguments = "-c \" " + command + "\"";
                    break;
                default:
                    Debug.LogError("This function is not supported in this platform.");
                    return null;
            }

            //出力を読み取れるようにする
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = false;
            //ウィンドウを表示しないようにする
            p.StartInfo.CreateNoWindow = true;

            //起動
            p.Start();

            //出力を読み取る
            var results = p.StandardOutput.ReadToEnd();

            //プロセス終了まで待機する
            //WaitForExitはReadToEndの後である必要がある
            //(親プロセス、子プロセスでブロック防止のため)
            p.WaitForExit();
            p.Close();
            return results;
        }

        /// <summary>
        /// Create gitignore for GitVersion if it doesn't exist.
        /// </summary>
        private static void CreateGitIgnore()
        {
            if (File.Exists(_gitVersionAssetRootDirectory + ".gitignore"))
            {
                return;
            }

            using (var fs = File.CreateText(_gitVersionAssetRootDirectory + ".gitignore"))
            {
                fs.WriteLine("Resources/*");
                fs.WriteLine("Resources.meta");
                fs.WriteLine(".gitignore");
            }
        }

        private static void CreateVersionHolderAsset()
        {
            //内部バージョン情報アセットを作成
            var versionHolder = ScriptableObject.CreateInstance<GitVersionHolder>();
            versionHolder.version = GenerateVersionFromGit();
            //バージョンが有効な場合にのみ作成
            if (!versionHolder.version.isValid)
            {
                return;
            }

            //GitVersion用リソースディレクトリがなかったら作成
            MakeAssetDirectoryRecursively(resourceDirectory);
            //古い内部バージョンアセットが残っていたら削除
            AssetDatabase.DeleteAsset(versionHolderPath);
            //新しいアセットを保存
            AssetDatabase.CreateAsset(versionHolder, versionHolderPath);
            AssetDatabase.SaveAssets();
            //gitignoreが存在しなかったら保存する
            CreateGitIgnore();
        }

        [InitializeOnLoadMethod]
        private static void CheckPlayModeState()
        {
            EditorApplication.playModeStateChanged += stateChange =>
            {
                if (stateChange == PlayModeStateChange.ExitingEditMode)
                {
                    CreateVersionHolderAsset();
                }
            };
        }

        void IPreprocessBuildWithReport.OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            CreateVersionHolderAsset();
        }
    }
}