# UdonObjectSerializerLibrary

[日本語バージョン](README_ja.md)

## 简介

本仓库实现了一个用于VRChat UdonSharp的对象序列化库，该序列化库相比UdonSharp现有的JSON序列化库而言，具有如下优势：

1. 支持丰富的复合数据类型。
2. 浮点数不会损失任何精度。
3. 泛型接口，使用较为方便。

该库具体支持如下数据类型：

1. C#基本数据类型：bool、sbyte、byte、short、ushort、int、uint、long、ulong、float、double、string。
2. UdonSharp扩展数据类型：DataList、DataDictionary、DataToken。
3. C#基本数据类型和UdonSharp扩展数据类型的一维数组（**不支持数组直接嵌套**）。

**对于float和double浮点数类型而言，因为底层直接存储了IEEE754的二进制表示，所以避免了常规 JSON文本序列化时造成的精度截断。**

**最顶层不支持DataToken，因为DataToken本身为值类型，且没有对应的包装引用类型，因此不会产生自动装箱过程，而经实测，在UdonSharp中，一个带有`[RecursiveMethod]`标记的泛型方法，如果同时以值类型和引用类型的方式来使用，则会出现异常的编译错误，并且也不支持方法重载，因此该库不支持最顶层元素类型为DataToken的情形。**

**因此，若顶层元素的类型必须是DataToken，请考虑将其装入引用类型中，例如DataList、DataDictionary或DataToken数组。**

## 组成结构

代码实现位于Assets/ObjectSerializer/UdonScript下，仅有ObjectSerializer.cs组成。

## 使用方法

### 导入

只需要将Assets目录中的ObjectSerializer目录放置到Unity工程的Assets目录中即可。

### 序列化

只需要调用`ObjectSerializer`类的静态方法`Serialize`即可，其函数签名为：`public static string Serialize<T>(T obj) where T : class`。

### 反序列化

只需要调用`ObjectSerializer`类的静态方法`Deserialize`即可，其函数签名为：`public static T Deserialize<T>(string str) where T : class`。