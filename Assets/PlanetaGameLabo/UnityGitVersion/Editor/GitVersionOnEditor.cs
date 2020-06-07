/*
The MIT License (MIT)

Copyright (c) 2018-2020 Cdec

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.IO;
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
        public const string versionSettingPath = _gitVersionAssetRootDirectory + "Editor/UnityGitVersionSetting.asset";

        private const string _gitVersionAssetRootDirectory = "Assets/PlanetaGameLabo/UnityGitVersion/";
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
                commitId = GitOperator.GetLastCommitId(false),
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
            var shortCommitId = GitOperator.GetLastCommitId(true);
            version.tag = GitOperator.GetTagFromCommitId(version.commitId);
            var isModified = GitOperator.CheckIfRepositoryIsChangedFromLastCommit();
            if (isModified)
            {
                version.diffHash = GitOperator.GetHashOfChangesFromLastCommit(false);
                if (string.IsNullOrWhiteSpace(version.tag))
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
                if (string.IsNullOrWhiteSpace(version.tag))
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