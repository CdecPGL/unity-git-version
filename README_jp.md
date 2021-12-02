[![Release](https://img.shields.io/github/v/release/CdecPGL/unity-git-version?include_prereleases&sort=semver)](https://github.com/CdecPGL/unity-git-version/releases)
[![License](https://img.shields.io/github/license/CdecPGL/unity-git-version)](https://github.com/CdecPGL/unity-git-version/blob/master/LICENSE)
[![CircleCI Buld and Test Status](https://circleci.com/gh/CdecPGL/unity-git-version/tree/master.svg?style=shield)](https://circleci.com/gh/CdecPGL/unity-git-version/tree/master)

# UnityGitVersion

Gitの管理情報から、`v1.0.0-commit-abcdefg`のようなゲームバージョンを識別する文字列を生成するためのUnityアセットです。
以下のような用途に使用できます。

- ゲームバージョンの表示
- ネットワーク通信型ゲームにおける、サーバークライアント間のバージョン確認

## 必要要件

- Unity: 2018.1又はそれ以上のバージョン (Player Settingで、`Api Compability Level`を`.Net 4.x`又は`.Net Standard 2.0`に設定してください)
- OS: Windows(, Linux or MacOSX)

## インストール

以下の手順でUnityGitVersionをインストールできます。

1. [リリースページ](https://github.com/CdecPGL/unity-git-version/releases)から最新の`UnityGitVersion.unitypackage`をダウンロードする
1. ダウンロードしたUnityパッケージを使用したいプロジェクトにインポートする
1. ^o^

## 使い方

### uGUIでバージョンを表示する

1. UnityGitVersionの`GitVersionGUIText`コンポーネントをバージョンを表示したいオブジェクトにアタッチする
1. コンポーネントをアタッチしたオブジェクトを任意の場所に配置する

### エディターでバージョンを確認する

1. メニューバーで「Tools->GitVersion->Log VersionString」を選択する
1. コンソールウインドウに表示されるバージョン文字列を確認する

### スクリプトでバージョンを取得する

バージョン情報は`PlanetaGameLabo.UnityGitVersion.Version`構造体で保持されており、以下のコードで参照することができます。

```cs
var version = PlanetaGameLabo.UnityGitVersion.version;
Debug.Log(version.versionString)
```

`PlanetaGameLabo.UnityGitVersion.Version`構造体は以下のフィールドを持ちます。

|名前|型|説明|
|:---|:---|:---|
|versionString|string|バージョン文字列|
|isVersionValid|bool|バージョンが使用可能かどうか|
|tag|string|タグ|
|commitId|string|コミットID|
|diffHash|string|最後のコミットとビルド時点でのリポジトリ状態の差分のSHA1ハッシュ|

バージョンが使用できない場合（プロジェクトがGitリポジトリでない、何らかの理由でバージョン情報がビルド時に生成されなかった場合など）、バージョンは「不明なバージョン」として扱われ、`Version.isValid`が`false`に設定されます。

### スクリプトでバージョンの一致確認を行う

現在実行しているゲームのバージョンと他のバージョンが一致しているかどうか、以下のコートで確認できます。

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

## 設定

メニューバーの「Tools->GitVersion->Setting」を選択することで、UnityGitVersionの設定ウインドウが開きます。

### Version String Format

UnityGitVersionにより生成されるバージョン文字列のフォーマットです。
バージョン文字列フォーマット内では、以下の特定の文字列が以下のように置き換えられます。

|文字列|置き換え後|
|:---|:---|
|%c|最新コミットの短縮コミットID|
|%C|最新コミットのコミットID|
|%t|最新コミットのタグ|
|%d|最新コミットとその時点でのリポジトリ状態の差分のSHA1ハッシュ短縮版|
|%D|最新コミットとその時点でのリポジトリ状態の差分のSHA1ハッシュ|
|%x|[git-describe](https://git-scm.com/docs/git-describe)の実行結果|
|%y|[git-describe](https://git-scm.com/docs/git-describe)に`--tags`オプションを付与した実行結果。短縮タグの検出が有効になる|
|%%|%|

リポジトリの状態ごとに、4種類のフォーマットがあります。

#### Standard

最新コミットからの変更がなく、タグがついていない場合のフォーマットです。

`%c`, `%C`, `%x`, `%y`が使用可能です。

##### 例

以下は、コミットIDが`1c8b748fe43d75bf76a9be505f96102ba2df19d7`で、最新コミットからの変更はなく、タグはついていない場合のフォーマットとそれに対する生成例です。なお、以下の例では、14個前のコミットにタグ`v0.0.0`がついているとします。

|フォーマット|生成バージョン文字列|
|:---|:---|
|`commit-%c`|`commit-1c8b748`|
|`commit-%C`|`commit-1c8b748fe43d75bf76a9be505f96102ba2df19d7`|
|`%x`|`v0.0.0-14-g1c8b748`|

#### With Diff

最新コミットからの変更があり、タグがついていない場合のフォーマットです。

`%c`, `%C`, `%d`, `%D`, `%x`, `%y`が使用可能です。

##### 例

以下は、コミットIDが`1c8b748fe43d75bf76a9be505f96102ba2df19d7`で、ハッシュ値が`ad5add9f6a2583696809fc103ce5303f6db1ff78`である最新コミットからの変更があり、タグはついていない場合のフォーマットとそれに対する生成例です。なお、以下の例では、14個前のコミットにタグ`v0.0.0`がついているとします。

|フォーマット|生成バージョン文字列|
|:---|:---|
|`commit-%c-%d`|`commit-1c8b748-ad5add9`|
|`commit-%C-%D`|`commit-1c8b748fe43d75bf76a9be505f96102ba2df19d7-ad5add9f6a2583696809fc103ce5303f6db1ff78`|
|`%x-%d`|`v0.0.0-14-g1c8b748-ad5add9`|

#### With Tag

最新コミットからの変更がなく、タグがついている場合のフォーマットです。

`%c`, `%C`, `%t`, `%x`, `%y`が使用可能です。

##### 例

以下は、コミットIDが`1c8b748fe43d75bf76a9be505f96102ba2df19d7`で、最新コミットからの変更はなく、タグ`v1.0.0`がついている場合のフォーマットとそれに対する生成例です。

|フォーマット|生成バージョン文字列|
|:---|:---|
|`%t`|`v1.0.0`|
|`%t-commit-%c`|`v1.0.0-commit-1c8b748`|
|`%x`|`v1.0.0`|

#### With Tag and Diff

最新コミットからの変更があり、タグがついている場合のフォーマットです。

`%c`, `%C`, `%t`, `%d`, `%D`, `%x`, `%y`が使用可能です。

##### 例

以下は、コミットIDが`1c8b748fe43d75bf76a9be505f96102ba2df19d7`で、ハッシュ値が`ad5add9f6a2583696809fc103ce5303f6db1ff78`である最新コミットからの変更があり、タグ`v1.0.0`がついている場合のフォーマットとそれに対する生成例です。

|フォーマット|生成バージョン文字列|
|:---|:---|
|`%t-%d`|`v1.0.0-ad5add9`|
|`%t-commit-%c-%d`|`v1.0.0-commit-1c8b748-ad5add9`|
|`%x-%d`|`v1.0.0-ad5add9`|

## その他

### バージョン情報生成のタイミング

エディタでゲームを再生するとき、又はゲームをビルドする前のタイミングで、バージョン情報が生成され、保存されます。
このとき、バージョン情報を持つアセットは`Assets/PlanetaGameLabo/UnityGitVersion/Resources`ディレクトリに保存されますが、アセットファイルは`PlanetaGameLabo/UnityGitVersion`に自動的に生成される`.gitignore`によって無視されるため、Gitには影響しません。

### スクリプトでのGit操作

`PlanetaGameLabo.UnityGitVersion.GitOperator`クラスを使用して、コミットIDを取得するなどの操作を実行できます。

## ライセンス

本リポジトリに含まれるコードは、[MITライセンス](LICENSE)に基づきます。
