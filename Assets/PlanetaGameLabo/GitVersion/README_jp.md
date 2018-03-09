# GitVersion

Gitの管理情報からゲームを識別するバージョンを生成するためのUnityアセットです。
ネットワーク通信型ゲームにおける、サーバークライアント間のバージョン確認などに用いることができます。

## 動作環境

- Unity5.6以上
- Windows, OSX用Unityエディタ

## 使用方法

### バージョンの生成

バージョンはエディター上での再生、ビルドの直前に自動的に生成され保存されます。
その際にバージョン保持アセットがAssets/PlanetaGameLabo/GitVersion/Resourcesフォルダ下に保存されますが、.gitignoreによりGitの管理対象外に指定されているため、Gitには影響を与えません。

### エディタ上でのバージョン確認

メニューバーのGitVersion->Log VersionStringを選択することで、コンソールウインドウに現在のバージョン文字列が出力されます。

### スクリプト内でのバージョン取得

バージョンはPlanetaGameLabo.GitVersion.Version構造体で保持され、以下のコードにより取得できます。

```cs
var version = PlanetaGameLabo.GitVersion.version;
```

PlanetaGameLabo.GitVersion.Version構造体は以下のフィールを持ちます。

- string versionString: バージョン文字列
- bool isValid: バージョンが有効かどうか
- string tag: タグ
- string commitId: 短縮コミットID
- string diffHash: 最終コミットからの変更のハッシュ
- bool allowUnknownVersionMatching: バージョンが不明な場合にバージョンが一致したとみなすかどうか

なお、Gitが使用できない、プロジェクトがGitリポジトリでない、何らかの理由でバージョンが保存されていない場合は不明なバージョンとなり、Version.isValidがfalseとなります。

### スクリプト内でのバージョンの一致確認

以下のコードにより、自分のバージョンと他のバージョンの一致を確認できます。

```cs
if(PlanetaGameLabo.GitVersion.CheckIfVersionMatch(other_version)){
    // バージョン一致
}
else{
    // バージョン不一致
}
```

## 設定

メニューバーのGitVersion->SettingからGitVersionの設定ウインドウを開くことができます。

### バージョン文字列フォーマット

生成されるバージョン文字列のフォーマットを設定します。
バージョン文字列フォーマット内では以下の文字列が対応する内容に置き換えられます。

- %c: 短縮コミットID
- %C: コミットID
- %t: 最新コミットのタグ
- %d: 最新コミットからの変更点のハッシュ
- %%: %

現在のリポジトリの状況により以下に示す4種類のフォーマットが使用されます。

#### Standard

最新のコミットから変更点がなく、タグが設定されていない場合に使用されます。

%c, %Cが使用可能です。

#### With Diff

最新のコミットから変更点があり、タグが設定されていない場合に使用されます。

%c, %C, %dが使用可能です。

#### With Tag

最新のコミットから変更点がなく、タグが設定されている場合に使用されます。

%c, %C, %tが使用可能です。

#### With Tag and Diff

最新のコミットから変更点があり、タグが設定されている場合に使用されます。

%c, %C, %t, %dが使用可能です。

### AllowUnknownVersionMatching

この設定にチェックを入れると、バージョンが不明な場合にはバージョンが一致したとみなします。

## ライセンス

MITライセンスの下公開しています。

***

The MIT License (MIT)

Copyright (c) 2018 Cdec

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

***
