# 概要

スプレッドシートのダウンロードができる自分用ライブラリ

※アップロード機能も今後対応予定

```csharp

async UniTask HogeAsync()
{
    var sheetLoader = new MornSpreadSheetLoader("スプレッドシートのID")
    var sheet = await core.LoadSheetAsync("シート名");
    
    // 1列目を配列で取得
    var column = sheet.GetColumn(1);
    
    // 1行目を配列で取得
    var row = sheet.GetRow(1);
    
    // 1行1列目のセルを取得
    var cell = sheet.GetCell(1, 1);
    
    cell.AsString(); // 文字列として取得
    cell.AsInt(); // 整数として取得
    cell.AsFloat(); // 浮動小数点数として取得
    cell.AsBool(); // 真偽値として取得
}
```

### 動作確認環境

- Unity 2022.3.14f1

### 依存ライブラリ

- UniTask
    - https://github.com/Cysharp/UniTask
    - 通信の待機に用いている

# 使い方

- 適当なスプレッドシートを用意する
- スプレッドシートのIDをメモ
    - https://docs.google.com/spreadsheets/d/スプレッドシートのID
- シート名をメモ
    - Sheet1
- `MornSpreadSheetLoader` インスタンスを作成し、シートのIDを与える
- `LoadSheetAsync` 関数をシート名を引数に与えて実行する
- `GetColumn`、`GetRow`、`GetCell` 関数で列/行/セルを取得する
- `AsInt`、`AsFloat`、`AsBool`、`AsString` 関数でセルの値を取得する

### その他

- `DefineSymbol` に `DISABLE_MORN_SPREAD_SHEET_LOG` を設定すると、ログ出力を無効化できる
