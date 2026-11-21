# ReName
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET: 10.0](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![YMM4](https://img.shields.io/badge/YMM4-v4.47%2B-green.svg)](https://manjubox.net/ymm4/)
[![Harmony](https://img.shields.io/badge/powered%20by-HarmonyLib-orange.svg)](https://github.com/pardeike/harmony)

**ReName** は、ゆっくりMovieMaker4 (YMM4) のタイムライン上で、アイテムの表示名を直接編集可能にするプラグインです。  
タイムラインアイテムを右クリックして **「名前を変更」** を選ぶだけで、アイテム上のラベル部分がそのまま編集可能になり、自由に分かりやすい名前をつけることができます。

---

## 主な特徴

### 1. 右クリックからワンクリックで名前変更
- タイムライン上のアイテムを右クリックした際のメニューに **「名前を変更」** が追加されます。
- テキスト、画像、動画、音声、図形、エフェクト、グループ制御など、タイムラインに配置できる**すべてのアイテムに対応**しています。

### 2. タイムライン上で直接インライン編集
- 別ウィンドウやダイアログを開くことなく、タイムライン上のアイテム表示位置にそのままテキストボックス（Adorner）が出現します。
- 呼び出し直後にテキストが全選択フォーカスされるため、キーボードからすぐに新しい名前を入力できます。

### 3. 安心の Undo / Redo (`Ctrl + Z` / `Ctrl + Y`) 完全対応
- 名前変更の操作は YMM4 内部の `UndoRedoManager` に記録されます。
- 誤って変更した場合や前の名前に戻したい場合も、標準の `Ctrl + Z`（元に戻す）や `Ctrl + Y`（やり直し）で安全に戻せます。

### 4. プロジェクト保存 (.ymmp) に対応＆安全設計
- 変更したカスタム名は、プロジェクト保存時 (`.ymmp`) に各アイテムの仮想データとして自動保存されます。
- プロジェクトを再度読み込んだ際にも、設定した名前がしっかりと復元されます。
- プラグインを導入していない環境で `.ymmp` を開いた場合でも、エラーで開けなくなることはなく、YMM4 本来のデフォルト名で安全に読み込まれます。

### 5. 空白でいつでもデフォルト名にリセット
- 名前を空欄にして確定すると、カスタム名が解除され、アイテム本来のデフォルト表示名（テキスト内容やファイル名など）にいつでも戻せます。

---

## 使い方

1. タイムライン上の任意のアイテムを**右クリック**します。
2. メニュー最下部の **「名前を変更」** をクリックします。
3. アイテムの上にインラインの入力ボックスが表示されるので、新しい名前を入力します。
4. **`Enter` キー** を押すか、タイムラインの別の場所をクリックすると確定されます。

### キーボード操作

| キー操作 | 挙動 |
| :--- | :--- |
| **`Enter`** | 変更した名前を確定して保存します（※日本語 IME 変換中の Enter では誤確定しません）。 |
| **`Esc`** | 変更をキャンセルし、編集前の名前に戻します。 |
| **ボックス外クリック / フォーカス移動** | 自動的に入力内容を確定してエディタを閉じます。 |

---

## インストール方法

### 配布パッケージ (.ymme) からのインストール
1. GitHub の [Releases](https://github.com/leftcontroller0518/reName/releases) より最新バージョンの `.ymme` ファイルをダウンロードします。
2. ダウンロードした `.ymme` ファイルをダブルクリックして実行するか、起動中の YMM4 ウィンドウへドラッグ＆ドロップします。
3. YMM4 を再起動します。

### 手動インストール
1. リリース zip 内の `ReName.dll` および `0Harmony.dll` をコピーします。
2. YMM4 インストールフォルダ内の `user\plugin\ReName\` フォルダに配置します。
3. YMM4 を起動（または再起動）します。

---

## ビルド方法

1. `Directory.Build.props` を開き、`<YMM4DirPath>` をお使いの YMM4 インストール先パス（末尾 `\` 必須）に設定します。
   ```xml
   <Project>
     <PropertyGroup>
       <YMM4DirPath>C:\YMM4\</YMM4DirPath>
     </PropertyGroup>
   </Project>
   ```
2. .NET 10 SDK がインストールされた環境で下記コマンドを実行します：
   ```powershell
   dotnet build -c Release
   ```
3. ビルドが成功すると、`$(YMM4DirPath)user\plugin\ReName\` に必要な DLL が自動コピーされます。

---

## プロジェクト構成

```
reName/
├── Directory.Build.props          # YMM4 インストールパス設定
├── ReName.csproj                  # プロジェクト設定 (.NET 10-windows / WPF)
├── ReNamePlugin.cs                # プラグインエントリポイント (IPlugin 実装・Harmony 初期化)
├── Core/
│   ├── CustomNameStore.cs         # アイテムごとのカスタム名管理・Undo/Redo コマンド
│   └── CustomNameJsonResolver.cs  # .ymmp 保存・読み込み用の仮想 JSON プロパティ制御
├── Patches/
│   ├── ContextMenuPatch.cs        # タイムラインアイテム右クリックメニュー拡張
│   └── TimelineLabelPatch.cs      # アイテム表示ラベル (Label) へのカスタム名反映フック
├── UI/
│   ├── InlineRenameAdorner.cs     # タイムライン上に重畳表示するインライン TextBox Adorner
│   └── InlineRenameController.cs  # インライン編集セッション・フォーカス・キーボード操作管理
└── README.md
```

---

## 技術仕様・使用ライブラリ

- **[HarmonyLib (Lib.Harmony)](https://github.com/pardeike/harmony)**: タイムラインの右クリックイベントハンドラおよびアイテムラベル読み出し処理のフックに使用しています。
- **Newtonsoft.Json**: YMM4 組み込みの JsonContractResolver を拡張し、既存のデータ構造を破壊せずに安全にカスタム名をシリアライズします。
- **WPF AdornerLayer**: タイムライン描画ツリーを直接破壊せず、UIElement の装飾層（Adorner）に入力コントロールを配置することで、高速かつ安全なインライン編集を実現しています。

---

## ライセンス

本プラグインは [MIT License](LICENSE) の下で公開されています。商用・非商用問わず、ライセンスの範囲内で自由にご利用いただけます。

## コードについて
- 制作には Claude、ChatGPT、Gemini などの AI アシスタントを活用しています。
- Windows 11 / YMM4 最新環境にてビルドおよび動作検証を行っています。
- Issues、Pull Request も大歓迎です。バグ報告や機能のご要望などがありましたらお気軽に GitHub Issues までお寄せください。
