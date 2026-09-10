# aitsu

aitsuは、ローカルで動作するOllamaと接続して会話するC#コンソールアプリケーションです。
将来的なVRChat連携を想定しています。

## 現在の機能

- コンソールからの日本語入力
- Ollama Chat APIへのリクエスト
- Ollamaのストリーミング応答の逐次表示
- 人格設定と会話履歴を含む応答生成
- コンソールへの応答表示
- 実行中の会話履歴の削除

OpenAI APIなどの外部有料APIは使用しません。

## 構成

```text
aitsu_project/
├─ Program.cs              # CLI入出力とアプリケーション起動
├─ AitsuOptions.cs         # 環境変数、接続先、人格設定の読み込み
├─ IChatClient.cs          # AIクライアントの共通インターフェース
├─ ConversationService.cs  # 入力検証と会話履歴の管理
├─ OllamaClient.cs         # Ollama APIとの通信と応答解析
├─ persona.txt             # Ollamaへ渡す人格設定
├─ aitsu.csproj            # .NETプロジェクト設定
├─ global.json             # .NET SDKの選択設定
├─ .editorconfig           # コード整形規則
├─ .gitignore              # Git管理から除外するファイル
└─ README.md               # この説明書
```

## 必要な環境

- Windows 10またはWindows 11
- .NET 8 SDK
- Ollama
- Ollamaで取得したチャットモデル

現在の推奨SDKは`8.0.424`です。`global.json`で.NET 8 SDKを選択します。
OpenAI APIキーは不要です。

## 環境の導入

### .NET 8 SDK

PowerShellで実行します。

```powershell
winget install --id Microsoft.DotNet.SDK.8 --exact
```

### Ollama

```powershell
winget install --id Ollama.Ollama --exact
```

インストール後、Ollamaアプリケーションを起動します。
アプリケーションを使用しない場合は、次のコマンドでサーバーを起動します。

```powershell
ollama serve
```

モデルを取得します。

```powershell
ollama pull llama3.2
```

導入状態は次で確認できます。

```powershell
dotnet --version
ollama --version
ollama list
```

## ビルドと実行

プロジェクトのルートで実行します。

```powershell
dotnet restore
dotnet build
dotnet run
```

起動後、`You>`の後にメッセージを入力します。

```text
aitsuを開始しました。
終了: /exit  履歴削除: /clear
You> こんにちは
aitsu> こんにちは。今日はどうしましたか？
```

## CLIコマンド

- `/exit`: プログラムを終了
- `/clear`: 会話履歴を削除
- `Ctrl+C`: 実行中の処理をキャンセルして終了

会話履歴は実行中のメモリに保持されます。
最大20メッセージ、合計16,000文字までです。
プログラムを終了すると履歴は削除されます。

## 人格設定

人格は`persona.txt`に記述します。`persona.example.txt`のファイル名を変更して使用してください。
内容はOllamaへの`system`メッセージとして毎回送信されます。

```text
あなたは「aitsu」という名前の対話AIです。
基本的に日本語で答えてください。
不明なことは推測で断定せず、分からないと伝えてください。
```

人格設定は4,000文字以内で指定します。

## 環境変数

```powershell
$env:OLLAMA_MODEL = "llama3.2"
$env:OLLAMA_ENDPOINT = "http://localhost:11434/api/chat"
$env:AITSU_PERSONA_FILE = "persona.txt"
dotnet run
```

- `OLLAMA_MODEL`: 使用するOllamaモデル
- `OLLAMA_ENDPOINT`: Ollama Chat APIのURL
- `AITSU_PERSONA_FILE`: 人格設定ファイルのパス

`AITSU_PERSONA_FILE`の相対パスは、まず現在の作業ディレクトリから解決します。
HTTP接続はlocalhostなどのループバックアドレスだけが許可されます。
外部サーバーへ接続する場合はHTTPSを使用してください。

## 既定値と制限

- 接続先: `http://localhost:11434/api/chat`
- モデル: `llama3.2`
- リクエストタイムアウト: 5分
- 入力上限: 4,000文字
- 人格設定上限: 4,000文字
- 応答上限: 8,000文字
- 応答本文の読み込み上限: 4,000,000文字
- Ollamaリクエスト: `stream=true`

## 処理の流れ

1. `AitsuOptions.cs`が接続先、モデル、人格設定を読み込む
2. `Program.cs`がCLIから入力を受け取る
3. `ConversationService.cs`が入力と履歴を検証する
4. `OllamaClient.cs`がOllamaへリクエストを送信する
5. OllamaのNDJSONストリーミング応答を解析してCLIへ表示する
6. 成功した会話を次のリクエスト用に保存する

## トラブルシューティング

### `ollama`コマンドが見つからない

Ollamaをインストールし、ターミナルを再起動してください。

```powershell
ollama --version
```

### Ollamaに接続できない

Ollamaが起動しているか確認してください。

```powershell
ollama list
```

既定値以外の接続先を使用する場合は、`OLLAMA_ENDPOINT`を確認してください。

### モデルが見つからない

使用するモデルを取得してください。

```powershell
ollama pull llama3.2
```

`OLLAMA_MODEL`を設定した場合は、取得済みのモデル名と一致させてください。

## 今後の予定

- Ollama以外のAIクライアント対応
- 音声認識
- 音声合成
- VRChat Chatboxへの表示
- OSCによるアバターパラメータ制御
