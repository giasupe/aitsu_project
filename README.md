# aitsu

aitsuは、ローカルで動作するOllamaと接続して会話するC#コンソールアプリケーションです。
オプションで、AIの返答をVRChatのChatboxへOSC経由で送信できます。

## 現在の機能

- コンソールからの日本語入力
- Ollama Chat APIへのリクエスト
- Ollamaのストリーミング応答の逐次表示
- AI回答の最大100文字出力
- 人格設定と会話履歴を含む応答生成
- `/clear`による会話履歴の削除
- `dotnet test`による自動テスト
- VRChat ChatboxへのOSC送信
- AI応答中のVRChat Chatbox入力中表示

Chatbox連携は既定で無効です。
有効にした場合も、返答を1トークンずつ送信せず、AIの返答完了後に送信します。

## 構成

```text
aitsu_project/
├─ Program.cs                 # CLI入出力とアプリケーション起動
├─ AitsuOptions.cs            # Ollama・Chatbox・人格設定の読み込み
├─ IChatClient.cs             # AIクライアントの共通インターフェース
├─ ConversationService.cs     # 入力検証と会話履歴の管理
├─ OllamaClient.cs            # Ollama APIとの通信と応答解析
├─ IChatboxClient.cs          # Chatbox送信の共通インターフェース
├─ VrChatChatboxClient.cs     # VRChatへのOSC UDP送信
├─ OscPacketEncoder.cs        # OSCパケットのエンコード
├─ ChatboxTextFormatter.cs    # 144文字・9行への分割
├─ persona.txt                # Ollamaへ渡す人格設定
├─ persona.example.txt        # 人格設定の例
├─ aitsu.csproj               # .NETプロジェクト設定
├─ aitsu.sln                  # 本体とテストをまとめるソリューション
├─ Aitsu.Tests/               # 自動テストプロジェクト
├─ global.json                # .NET SDKの選択設定
├─ .editorconfig              # コード整形規則
├─ .gitignore                 # Git管理から除外するファイル
└─ README.md                  # この説明書
```

外部の有料AI APIは使用しません。
OSCパケットは.NET標準のUDP機能で作成するため、OSC用のNuGetパッケージも使用していません。

## 必要な環境

- Windows 10またはWindows 11
- .NET 8 SDK
- Ollama
- Ollamaで取得したチャットモデル
- VRChat（Chatbox連携を使う場合）

## Ollamaの準備

Ollamaを起動した状態で、使用するモデルを取得します。

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
dotnet test
dotnet run
```

`dotnet test`は`aitsu.sln`に含まれる自動テストを実行します。

起動後、`You>`の後にメッセージを入力します。

```text
aitsuを開始しました。
終了: /exit  履歴削除: /clear
VRChat Chatbox: 無効
You> こんにちは
aitsu> こんにちは。今日はどうしましたか？
```

## テスト

本体と`Aitsu.Tests`のテストプロジェクトを`aitsu.sln`で管理しています。

```powershell
dotnet test .\aitsu.sln
```

現在は、会話履歴、Ollama応答の100文字制限、Chatbox用テキスト分割をテストしています。

## CLIコマンド

- `/exit`: プログラムを終了
- `/clear`: 会話履歴を削除
- `/chatbox on`: Chatbox連携を有効化
- `/chatbox off`: Chatbox連携を無効化
- `/chatbox status`: Chatbox連携の状態を表示
- `/chatbox`: Chatbox連携のON/OFFを切り替え
- `Ctrl+C`: 実行中の処理をキャンセルして終了

会話履歴は実行中のメモリに保持されます。
最大20メッセージ、合計16,000文字までです。
プログラムを終了すると履歴は削除されます。

## 人格設定

人格は`persona.txt`に記述します。
内容はOllamaへの`system`メッセージとして毎回送信されます。

人格設定は4,000文字以内で指定します。
`persona.example.txt`を参考に編集してください。
`persona.txt`は個人用設定としてGit管理対象外です。
新しい環境では、次のコマンドで作成してください。

```powershell
Copy-Item .\persona.example.txt .\persona.txt
```

`persona.txt`が見つからない場合は、既定の人格設定で起動し、
作成方法をエラーメッセージに表示します。

```text
あなたは「aitsu」という名前の対話AIです。
基本的に日本語で答えてください。
不明なことは推測で断定せず、分からないと伝えてください。
```

## VRChat Chatbox連携

### VRChat側の準備

1. VRChatでOSCを有効にします
2. VRChatを起動します
3. VRChatのOSC受信ポートを`9000`にします
4. Ollamaとaitsuを起動します

VRChatのChatbox連携では、次のOSCアドレスを使用します。

```text
/chatbox/input
/chatbox/typing
```

`/chatbox/input`には、次の形式でユーザー入力とaitsuの返答を送信します。

```text
You> ユーザーの入力
aitsu> AIの返答
```

Chatboxの仕様に合わせ、メッセージは最大144文字・9行単位に分割して送信します。

### aitsu側の設定

PowerShellで次の環境変数を設定します。

```powershell
$env:AITSU_CHATBOX_ENABLED = "true"
$env:AITSU_CHATBOX_HOST = "127.0.0.1"
$env:AITSU_CHATBOX_PORT = "9000"
$env:AITSU_CHATBOX_NOTIFY = "false"
dotnet run
```

設定項目:

- `AITSU_CHATBOX_ENABLED`: Chatbox連携の有効・無効。既定値は`false`
- `AITSU_CHATBOX_HOST`: OSC送信先。`127.0.0.1`または`::1`のみ指定可能
- `AITSU_CHATBOX_PORT`: OSC受信ポート。既定値は`9000`
- `AITSU_CHATBOX_NOTIFY`: Chatbox通知音の有効・無効。既定値は`false`

起動後は、スラッシュコマンドでも状態を変更できます。
この変更はアプリケーション実行中だけ有効で、次回起動時は環境変数の設定に戻ります。

```text
You> /chatbox on
VRChat Chatbox: 有効
You> /chatbox off
VRChat Chatbox: 無効
You> /chatbox status
VRChat Chatbox: 無効
```

AIが返答を生成している間は、Chatboxの入力中表示を有効にします。
返答が完了すると入力中表示を解除し、ユーザー入力と返答を送信します。

Chatbox連携を無効に戻す場合:

```powershell
$env:AITSU_CHATBOX_ENABLED = "false"
dotnet run
```

## Ollamaとaitsuの設定

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
HTTP接続はループバックアドレスだけが許可されます。
外部サーバーへ接続する場合はHTTPSを使用してください。

## 処理の流れ

1. `AitsuOptions.cs`がOllama・Chatbox・人格設定を読み込む
2. `Program.cs`がCLIから入力を受け取る
3. `ConversationService.cs`が入力と履歴を検証する
4. `OllamaClient.cs`がOllamaへストリーミングリクエストを送信する
5. Ollamaの返答をCLIへ逐次表示する
6. 返答完了後、`VrChatChatboxClient.cs`がユーザー入力と返答をChatboxへOSC送信する
7. 成功した会話を次のリクエスト用に保存する

## 既定値と制限

- Ollama接続先: `http://localhost:11434/api/chat`
- Ollamaモデル: `llama3.2`
- Ollamaリクエスト: `stream=true`
- リクエストタイムアウト: 5分
- 入力上限: 4,000文字
- 人格設定上限: 4,000文字
- 応答上限: 100文字
- 会話履歴: 最大20メッセージ、合計16,000文字
- Chatbox送信先: `127.0.0.1:9000`
- Chatbox本文: 最大144文字、最大9行単位

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

### モデルが見つからない

使用するモデルを取得してください。

```powershell
ollama pull llama3.2
```

### VRChat Chatboxに表示されない

- `AITSU_CHATBOX_ENABLED`が`true`か確認する
- VRChatでOSCが有効か確認する
- VRChatの受信ポートが`9000`か確認する
- `AITSU_CHATBOX_HOST`と`AITSU_CHATBOX_PORT`を確認する
- VRChatを起動してからaitsuを実行する

## 今後の予定

- 音声認識
- 音声合成
- VRChatアバターパラメータへのOSC送信
- Ollama以外のAIクライアント対応
