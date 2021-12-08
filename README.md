[![Release](https://img.shields.io/github/v/release/CdecPGL/unity-git-version?include_prereleases&sort=semver)](https://github.com/CdecPGL/unity-git-version/releases)
[![License](https://img.shields.io/github/license/CdecPGL/unity-git-version)](https://github.com/CdecPGL/unity-git-version/blob/master/LICENSE)
[![CircleCI Buld and Test Status](https://circleci.com/gh/CdecPGL/unity-git-version/tree/master.svg?style=shield)](https://circleci.com/gh/CdecPGL/unity-git-version/tree/master)

# UnityGitVersion

[日本語README](README_jp.md)

An asset to automatically generate a version string with Git like `v1.0.0-commit-abcdefg`.
This asset can be used for the following purposes.

- Display game version
- Check game version between a server and clients in online game
- etc.

## Requirement

- Unity: 2018.1 or higher (set `Api Compability Level` to `.Net 4.x` or `.Net Standard 2.0` in Player Setting)
- OS: Windows(, Linux or MacOSX)

## Install

### UPM Package via Open UPM

You can install UnityGitVersion via open upm.
[Prepare `openupm-cli`](https://openupm.com/docs/getting-started.html#installing-openupm-cli), then run below command in your project directory.

```bash
openupm add com.cdecpgl.git-version
```

It is also possible to install upm without open upm by adding scoped registory in package manager of Unity.

### UPM Package via Git URI

You can install UnityGitVersion via Git URI.

1. Open package manager in Unity
1. Click "+" button at top left of the package manager window
1. Click "Add package from git URI..."
1. Input `https://github.com/CdecPGL/unity-git-version.git?path=Assets/PlanetaGameLabo/UnityGitVersion` and click "Add" button

![UPM Package via Git URI](Documents/install_via_git_uri.jpg)

### Unity Package

You can install UnityGitVersion as an Unity Package by below steps.
Use this method if the version of your Unity doesn't supports UPM.

1. Download latest `UnityGitVersion.unitypackage` from [the release page](https://github.com/CdecPGL/unity-git-version/releases)
1. Import downloaded unity package to your project
1. :)

## Usage

### Display the Version on uGUI

1. Attach `GitVersionGUIText` component of UnityGitVersion to the object you want to display version
1. Arrange the uGUI object

### Check the Version on Editor

1. Selecting "Tools->GitVersion->Log VersionString" in the menu bar
1. Check a version string  logged to the console window

### Get the Version in Scripts

A version is held as `PlanetaGameLabo.UnityGitVersion.Version` structure and can be gotten by the below code.

```cs
var version = PlanetaGameLabo.UnityGitVersion.version;
Debug.Log(version.versionString)
```

`PlanetaGameLabo.UnityGitVersion.Version` structure has bellow public fields.

|Name|Type|Description|
|:---|:---|:---|
|versionString|string|A version string|
|isVersionValid|bool|whether the version is valid|
|tag|string|Tag|
|commitId|string|Commit ID|
|diffHash|string|SHA1 Hash of difference between last commit and current state|

If version is not available (the project directory is not git repository or a version is not saved due to some reasons), the version is treated as "Unknown Version" and `Version.isValid` is set to `false`.

### Check the Version Matching in Scripts

You can check if running game version matches other version by below code.

```cs
if(PlanetaGameLabo.GitVersion.CheckIfVersionMatch(other_version))
{
    // Match
}
else
{
    // Not match
}
```

## Setting

By clicking "Tools->GitVersion->Setting" in the menu bar, you can open a setting window of UnityGitVersion.

### Version String Format

Formats of version string generated by UnityGitVersion.
In version string format, spesific strings are replaced as below.

|Special String|Replace String|
|:---|:---|
|%c|A short commit ID of the last commit|
|%C|A commit ID of the last commit|
|%t|A tag of the last commit|
|%d|A short hash of difference between the last commit and current state|
|%D|A hash of difference between the last commit and current state|
|%x|A result of [git-describe](https://git-scm.com/docs/git-describe).|
|%y|A result of [git-describe](https://git-scm.com/docs/git-describe) with `--tags` option, which means lightweight matching is enabled.|
|%%|%|

There are 4 kinds of formats for each status of the repository.

#### Standard

This is used when there are no changes from last commit and no tag to the last commit.

`%c`, `%C`, `%x`, `%y` are available.

##### Example

Below are examples when commit ID is `1c8b748fe43d75bf76a9be505f96102ba2df19d7` and there not changes in the repogitory and no tag to the commit. Additionally, In below examples tag `v0.0.0` is added to a commit older by 14 commit.

|Format|Generated Version String|
|:---|:---|
|`commit-%c`|`commit-1c8b748`|
|`commit-%C`|`commit-1c8b748fe43d75bf76a9be505f96102ba2df19d7`|
|`%x`|`v0.0.0-14-g1c8b748`|

#### With Diff

This is used when there are some changes from last commit and no tag to the last commit.

`%c`, `%C`, `%d`, `%D`, `%x`, `%y` are available.

##### Example

Below are examples when commit ID is `1c8b748fe43d75bf76a9be505f96102ba2df19d7` and there are changes in the repogitory with SHA1 hash `ad5add9f6a2583696809fc103ce5303f6db1ff78` and no tag to the commit. Additionally, In below examples tag `v0.0.0` is added to a commit older by 14 commit.

|Format|Generated Version String|
|:---|:---|
|`commit-%c-%d`|`commit-1c8b748-ad5add9`|
|`commit-%C-%D`|`commit-1c8b748fe43d75bf76a9be505f96102ba2df19d7-ad5add9f6a2583696809fc103ce5303f6db1ff78`|
|`%x-%d`|`v0.0.0-14-g1c8b748-ad5add9`|

#### With Tag

This is used when there are no changes from last commit and a tag to the last commit.

`%c`, `%C`, `%t`, `%x`, `%y` are available.

##### Example

Below are examples when commit ID is `1c8b748fe43d75bf76a9be505f96102ba2df19d7` and there are no changes in the repogitory and tag `v1.0.0` to the commit.

|Format|Generated Version String|
|:---|:---|
|`%t`|`v1.0.0`|
|`%t-commit-%c`|`v1.0.0-commit-1c8b748`|
|`%x`|`v1.0.0`|

#### With Tag and Diff

This is used when there are some changes from last commit and a tag to the last commit.

`%c`, `%C`, `%t`, `%d`, `%D`, `%x`, `%y` are available.

##### Example

Below are examples when commit ID is `1c8b748fe43d75bf76a9be505f96102ba2df19d7` and there are changes in the repogitory with SHA1 hash `ad5add9f6a2583696809fc103ce5303f6db1ff78` and tag `v1.0.0` to the commit.

|Format|Generated Version String|
|:---|:---|
|`%t-%d`|`v1.0.0-ad5add9`|
|`%t-commit-%c-%d`|`v1.0.0-commit-1c8b748-ad5add9`|
|`%x-%d`|`v1.0.0-ad5add9`|

## Others

### Version Information Generation Timing

A version informatio is generated and saved when you play your game in editor or before build game.
In that time, an asset which has version information is saved on "Assets/PlanetaGameLabo/UnityGitVersion/Resources" directory, but it doesn't affect to Git because the asset file is ignored by .gitignore which is generated automatically in `PlanetaGameLabo/UnityGitVersion`.

### How to Generate Hash of Difference between the Last Commit and Current State

The hash of difference for `%d` and `%D` us the SHA1 hash of a string which is generated by merging three strings as below.

- The result of `git diff HEAD`
- Untracked file paths extracted from the result of `git status`
- Last update times of untracked file extracted from the result `git status`

The reason why we use not only `git diff HEAD` but also `git status` is that the result of `git diff HEAD` is not include untracked files.

### Git Operation in Scripts

You can do operation like getting commit ID by using `PlanetaGameLabo.UnityGitVersion.GitOperator` class.

## Contribution

Please submit [issues](https://github.com/CdecPGL/unity-git-version/issues) or create [Pull Requests](https://github.com/CdecPGL/unity-git-version/pulls) if you find bugs or want to propose new features.

You can create pull requests by below steps.

1. [Fork](https://github.com/CdecPGL/unity-git-version/fork)
1. Create a feature branch
1. Commit your changes
1. Rebase your local changes against the master branch
1. Push commits
1. Create new Pull Request

## License

The source codes in this repository are based on [the MIT Lisence](LICENSE).
