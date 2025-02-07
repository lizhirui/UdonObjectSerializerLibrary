# UdonObjectSerializerLibrary（翻訳者：fog8360）

## 概要

このリポジトリは、VRChat UdonSharp向けのオブジェクトシリアライズライブラリを実装したものです。このシリアライズライブラリは、既存のUdonSharpのJSONシリアライズライブラリと比べて、以下の利点があります：

1. 豊富な複合データ型をサポート。
2. 浮動小数点数の精度損失がない。
3. ジェネリックインターフェースを使用して、より使いやすい。

このライブラリは以下のデータ型をサポートしています：

1. C#基本データ型：bool、sbyte、byte、short、ushort、int、uint、long、ulong、float、double、string。
2. UdonSharp拡張データ型：DataList、DataDictionary、DataToken。
3. C#基本データ型およびUdonSharp拡張データ型の1次元配列（**配列の直接入れ子には対応していません**）。

**floatおよびdouble型に関しては、基盤としてIEEE754のバイナリ表現をそのまま格納しているため、一般的なJSONテキストシリアライズで発生する精度の切り捨てを避けています。**

**最上位にはDataToken型をサポートしていません。DataTokenは値型であり、対応するラッピング参照型がないため、自動ボクシングが発生せず、実際にUdonSharpでは`[RecursiveMethod]`マークが付けられたジェネリックメソッドが値型と参照型で同時に使用されるとコンパイルエラーが発生します。また、メソッドのオーバーロードもサポートされていないため、このライブラリでは最上位の要素型がDataTokenである場合をサポートしていません。**

**そのため、最上位の要素がDataToken型でなければならない場合は、DataList、DataDictionary、またはDataToken配列などの参照型にラップすることを検討してください。**

## 構成

コードは`Assets/ObjectSerializer/UdonScript`内にあり、`ObjectSerializer.cs`のみで構成されています。

## 使用方法

### インポート

`Assets`ディレクトリ内の`ObjectSerializer`フォルダをUnityプロジェクトの`Assets`ディレクトリに配置するだけでインポートできます。

### シリアライズ

`ObjectSerializer`クラスの静的メソッド`Serialize`を呼び出すだけでシリアライズできます。関数のシグネチャは次の通りです：
`public static string Serialize<T>(T obj) where T : class`。

### デシリアライズ

`ObjectSerializer`クラスの静的メソッド`Deserialize`を呼び出すだけでデシリアライズできます。関数のシグネチャは次の通りです：
`public static T Deserialize<T>(string str) where T : class`。