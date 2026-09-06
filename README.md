# aitsu

AIとVRChatの連携を目標にしたC#プロジェクトです。

現在は、コンソールから入力を受け取り、入力内容を表示する最小構成です。

## ディレクトリ構造

```text
aitsu_project/
├─ Program.cs    # メインプログラム
├─ aitsu.csproj  # .NETプロジェクト設定
├─ README.md     # この説明書
└─ .gitignore    # ビルド生成物の除外設定
```

`bin/`と`obj/`は、`dotnet run`や`dotnet build`で自動生成されるため、Git管理から除外しています。

## 必要な環境

- .NET 8 SDK

確認コマンド:

```powershell
dotnet --version
```

## 実行方法

プロジェクトのルートで実行します。

```powershell
cd C:\Users\giasupe\Documents\.github\aitsu_project
dotnet run
```

入力した文字が表示されます。

```text
> こんにちは
You: こんにちは
> /exit
```

`/exit`を入力すると終了します。

空入力は無視されます。`Ctrl+C`でも終了できます。

## Program.csの処理

1. `while (true)`で入力処理を繰り返す
2. `Console.ReadLine()`で入力を受け取る
3. `/exit`なら`break`で終了する
4. それ以外の入力をコンソールへ表示する

## 作成手順

空のフォルダから作成する場合は、次のコマンドを実行します。

```powershell
mkdir aitsu_project
cd aitsu_project
dotnet new console
dotnet run
```

生成された`Program.cs`を書き換えた後、再度`dotnet run`を実行します。

## 今後の予定

- OpenAI APIとの接続
- 会話履歴の管理
- 音声認識と音声合成
- VRChat Chatboxへの表示
- OSCによるアバターパラメータ制御
