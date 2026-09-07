# aitsu

aitsuは、AIとVRChatの連携を目標にしたC#プロジェクトです。

初期型では、外部の有料APIを使わず、ローカルで動作するOllamaと接続してCLI上で会話します。

## ディレクトリ構造

```text
aitsu_project/
├─ Program.cs              # CLIの入出力とアプリケーション起動
├─ AitsuOptions.cs         # Ollamaと人格設定の読み込み
├─ ConversationService.cs  # 会話履歴の管理
├─ OllamaClient.cs         # Ollama APIとの通信
├─ persona.txt             # aitsuの人格設定
├─ aitsu.csproj            # .NETプロジェクト設定
├─ README.md               # この説明書
└─ .gitignore              # Git管理から除外するファイル
```

APIキーや外部の有料APIは使用しません。

## 必要な環境

- .NET 8 SDK
- Ollama
- Ollamaでダウンロードしたチャットモデル

## Ollamaの準備

Ollamaをインストールした後、Ollamaが起動している状態でモデルを取得します。

例:

```powershell
ollama pull llama3.2
```

モデルが使用できるか確認するには、次のコマンドを実行します。

```powershell
ollama run llama3.2
```

Ollamaが通常の設定で起動している場合、aitsuは次のURLへ接続します。

```text
http://localhost:11434/api/chat
```

## 実行方法

プロジェクトのルートで実行します。

```powershell
cd C:\Users\giasupe\Documents\.github\aitsu_project
dotnet run
```

起動後、`You>`の後にメッセージを入力してください。

```text
aitsuを開始しました。
終了: /exit  履歴削除: /clear
You> こんにちは
aitsu> こんにちは。今日はどうしましたか？
```

## CLIコマンド

- `/exit`: プログラムを終了
- `/clear`: 会話履歴を削除
- `Ctrl+C`: プログラムを終了

会話履歴は実行中のメモリに最大20メッセージ保持されます。
プログラムを終了すると履歴は削除されます。

## 人格設定

人格は`persona.txt`に記述します。
この内容はOllamaへの`system`メッセージとして毎回送信されます。

例えば、次のように変更できます。

```text
あなたは落ち着いた雰囲気のAIです。
必ず日本語で答えてください。
返答は短く、分かりやすくしてください。
```

`persona.txt`を変更した後に`dotnet run`を実行すると、変更内容が反映されます。

## 環境変数による設定

既定値を変更したい場合は、PowerShellで環境変数を設定できます。

```powershell
$env:OLLAMA_MODEL = "llama3.2"
$env:OLLAMA_ENDPOINT = "http://localhost:11434/api/chat"
$env:AITSU_PERSONA_FILE = "persona.txt"
dotnet run
```

`AITSU_PERSONA_FILE`には、別の人格ファイルのパスも指定できます。

## 処理の流れ

1. `AitsuOptions.cs`がOllamaのURL、モデル、人格設定を読み込む
2. `Program.cs`がCLIからユーザー入力を受け取る
3. `ConversationService.cs`が会話履歴を管理する
4. `OllamaClient.cs`が人格設定・履歴・入力をOllamaへ送信する
5. Ollamaの返答をCLIへ表示する
6. 成功した会話を次のリクエスト用に保存する

## トラブルシューティング

### Ollamaに接続できない

Ollamaが起動しているか確認してください。

```powershell
ollama list
```

### モデルが見つからない

使用するモデルをダウンロードしてください。

```powershell
ollama pull llama3.2
```

`OLLAMA_MODEL`を設定した場合は、ダウンロードしたモデル名と一致しているか確認してください。

## 今後の予定

- 音声認識
- 音声合成
- VRChat Chatboxへの表示
- OSCによるアバターパラメータ制御
