# aitsu

aitsuは、AIとVRChatを連携するシステムです。

## 現在の構成

初期開発では、aitsuシステムの最小構成を作成しています。

- AIとのテキスト対話
- コンソールへの応答表示

## 開発方針

将来の音声認識、音声合成、アバターギミックとの連携を考慮し、実装言語にはC#を使用します。

将来的な連携対象は以下です。

- 音声認識
- AIによる応答生成
- 音声合成
- VRChat Chatboxへのテキスト表示
- OSCによるアバターパラメータ制御

## 現在の実装

コンソールから入力した文章をOpenAI Responses APIへ送信し、応答をコンソールに表示します。
会話履歴はプロセス実行中に保持され、`/clear`で削除できます。

## 実行方法

1. [.env.example](.env.example)を参考に`OPENAI_API_KEY`を環境変数へ設定します。
2. .NET 8 SDKをインストールします。
3. プロジェクトのルートで次のコマンドを実行します。

```powershell
$env:OPENAI_API_KEY = "APIキー"
dotnet run
```

`persona.txt`にAPIへ渡す人格設定を記述します。`OPENAI_MODEL`で使用するモデルを変更できます。

## 構成
