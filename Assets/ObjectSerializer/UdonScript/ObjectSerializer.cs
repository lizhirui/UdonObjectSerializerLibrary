using System.Text;
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;

public class ObjectSerializer
{
    private const int TokenType_None = -1;
    private const int TokenType_Null = 0;
    private const int TokenType_Boolean = 1;
    private const int TokenType_SByte = 2;
    private const int TokenType_Byte = 3;
    private const int TokenType_Short = 4;
    private const int TokenType_UShort = 5;
    private const int TokenType_Int = 6;
    private const int TokenType_UInt = 7;
    private const int TokenType_Long = 8;
    private const int TokenType_ULong = 9;
    private const int TokenType_Float = 10;
    private const int TokenType_Double = 11;
    private const int TokenType_String = 12;
    private const int TokenType_DataList = 13;
    private const int TokenType_DataDictionary = 14;
    private const int TokenType_Reference = 15;
    private const int TokenType_Error = 16;
    private const int TokenType_Array = 17;
    private const int TokenType_DataToken = 18;

    private static void ThrowException(string text)
    {
        Debug.LogError("ObjectSerializer: " + text);
        ((object)null).ToString();
    }

    private static void ThrowExceptionWithContext(string context, string text)
    {
        Debug.LogError("ObjectSerializer: " + text);
        Debug.LogError("Context: " + context);
        ((object)null).ToString();
    }

    private static string Pack(string str)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
    }

    private static string Unpack(string str)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(str));
    }

    private static string PackFloat(float f)
    {
        return Convert.ToBase64String(BitConverter.GetBytes(f));
    }

    private static float UnpackFloat(string str)
    {
        return BitConverter.ToSingle(Convert.FromBase64String(str), 0);
    }

    private static string PackDouble(double d)
    {
        return Convert.ToBase64String(BitConverter.GetBytes(d));
    }

    private static double UnpackDouble(string str)
    {
        return BitConverter.ToDouble(Convert.FromBase64String(str), 0);
    }

    private static object PackDataToken(DataToken dataToken)
    {
        var dataList = new DataList();
        dataList.Add(dataToken);
        var arr = new object[2]{"pack", dataList};
        return arr;
    }

    private static bool IsPackedDataToken(object obj)
    {
        if(!(obj.GetType() == typeof(object[])))
        {
            return false;
        }

        var arr = (object[])obj;

        if(arr.Length != 2)
        {
            return false;
        }

        if(arr[0].GetType() != typeof(string))
        {
            return false;
        }

        if(((string)arr[0]) == "pack")
        {
            return true;
        }

        return false;
    }

    private static DataToken UnpackDataToken(object obj)
    {
        return ((DataList)(((object[])obj)[1]))[0];
    }

    private static string SerializeDataToken(DataToken dataToken)
    {
        switch(dataToken.TokenType)
        {
            case TokenType.Null:
                ThrowException("TokenType Null can't be used in DataToken!");
                return "";

            case TokenType.Boolean:
                return TokenType_Boolean + ":" + (dataToken.Boolean ? 1 : 0);

            case TokenType.SByte:
                return TokenType_SByte + ":" + dataToken.SByte;

            case TokenType.Byte:
                return TokenType_Byte + ":" + dataToken.Byte;

            case TokenType.Short:
                return TokenType_Short + ":" + dataToken.Short;

            case TokenType.UShort:
                return TokenType_UShort + ":" + dataToken.UShort;

            case TokenType.Int:
                return TokenType_Int + ":" + dataToken.Int;

            case TokenType.UInt:
                return TokenType_UInt + ":" + dataToken.UInt;

            case TokenType.Long:
                return TokenType_Long + ":" + dataToken.Long;

            case TokenType.ULong:
                return TokenType_ULong + ":" + dataToken.ULong;

            case TokenType.Float:
                return TokenType_Float + ":" + PackFloat(dataToken.Float);

            case TokenType.Double:
                return TokenType_Double + ":" + PackDouble(dataToken.Double);

            case TokenType.String:
                return TokenType_String + ":" + Pack(dataToken.String);

            case TokenType.DataList:
                return Serialize_Internal(dataToken.DataList);

            case TokenType.DataDictionary:
                return Serialize_Internal(dataToken.DataDictionary);

            case TokenType.Reference:
                if(dataToken.Reference.GetType() == typeof(DataToken))
                {
                    ThrowException("DataToken can't nested directly!");
                    return "";
                }

                return Serialize_Internal(dataToken.Reference);

            case TokenType.Error:
                ThrowException("TokenType Error can't be used in DataToken!");
                return "";

            default:
                ThrowException("TokenType Exception!");
                return "";
        }
    }

    [RecursiveMethod]
    private static string Serialize_Internal(object obj)
    {
        var type = obj.GetType();

        if(type == typeof(bool))
        {
            return TokenType_Boolean + ":" + (((bool)obj) ? 1 : 0);
        }
        else if(type == typeof(sbyte))
        {
            return TokenType_SByte + ":" + ((sbyte)obj);
        }
        else if(type == typeof(byte))
        {
            return TokenType_Byte + ":" + ((byte)obj);
        }
        else if(type == typeof(short))
        {
            return TokenType_Short + ":" + ((short)obj);
        }
        else if(type == typeof(ushort))
        {
            return TokenType_UShort + ":" + ((ushort)obj);
        }
        else if(type == typeof(int))
        {
            return TokenType_Int + ":" + ((int)obj);
        }
        else if(type == typeof(uint))
        {
            return TokenType_UInt + ":" + ((uint)obj);
        }
        else if(type == typeof(long))
        {
            return TokenType_Long + ":" + ((long)obj);
        }
        else if(type == typeof(ulong))
        {
            return TokenType_ULong + ":" + ((ulong)obj);
        }
        else if(type == typeof(float))
        {
            return TokenType_Float + ":" + PackFloat((float)obj);
        }
        else if(type == typeof(double))
        {
            return TokenType_Double + ":" + PackDouble((double)obj);
        }
        else if(type == typeof(string))
        {
            return TokenType_String + ":" + ((string)obj);
        }
        else if(type == typeof(DataList))
        {
            var arg_str = "";
            var dataList = (DataList)obj;

            for(var i = 0;i < dataList.Count;i++)
            {
                arg_str += SerializeDataToken(dataList[i]);

                if(i < (dataList.Count - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_DataList + ":" + Pack(arg_str);
        }
        else if(type == typeof(DataDictionary))
        {
            var arg_str = "";
            var dataDictionary = (DataDictionary)obj;
            var keys = dataDictionary.GetKeys();

            for(var i = 0;i < keys.Count;i++)
            {
                arg_str += SerializeDataToken(keys[i]) + "?" + SerializeDataToken(dataDictionary[keys[i]]);

                if(i < (keys.Count - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_DataDictionary + ":" + Pack(arg_str);
        }
        else if(type == typeof(DataToken))
        {
            return TokenType_DataToken + ":" + SerializeDataToken((DataToken)obj);
        }
        if(type == typeof(bool[]))
        {
            var arr = (bool[])obj;
            var arg_str = TokenType_Boolean + "?";

            for(var i = 0;i < arr.Length;i++)
            {
                arg_str += (arr[i] ? 1 : 0) + "";

                if(i < (arr.Length - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_Array + ":" + Pack(arg_str);
        }
        else if(type == typeof(sbyte[]))
        {
            var arr = (sbyte[])obj;
            var arg_str = TokenType_SByte + "?";

            for(var i = 0;i < arr.Length;i++)
            {
                arg_str += arr[i] + "";

                if(i < (arr.Length - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_Array + ":" + Pack(arg_str);
        }
        else if(type == typeof(byte[]))
        {
            var arr = (byte[])obj;
            var arg_str = TokenType_Byte + "?";

            for(var i = 0;i < arr.Length;i++)
            {
                arg_str += arr[i] + "";

                if(i < (arr.Length - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_Array + ":" + Pack(arg_str);
        }
        else if(type == typeof(short[]))
        {
            var arr = (short[])obj;
            var arg_str = TokenType_Short + "?";

            for(var i = 0;i < arr.Length;i++)
            {
                arg_str += arr[i] + "";

                if(i < (arr.Length - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_Array + ":" + Pack(arg_str);
        }
        else if(type == typeof(ushort[]))
        {
            var arr = (ushort[])obj;
            var arg_str = TokenType_UShort + "?";

            for(var i = 0;i < arr.Length;i++)
            {
                arg_str += arr[i] + "";

                if(i < (arr.Length - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_Array + ":" + Pack(arg_str);
        }
        else if(type == typeof(int[]))
        {
            var arr = (int[])obj;
            var arg_str = TokenType_Int + "?";

            for(var i = 0;i < arr.Length;i++)
            {
                arg_str += arr[i] + "";

                if(i < (arr.Length - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_Array + ":" + Pack(arg_str);
        }
        else if(type == typeof(uint[]))
        {
            var arr = (uint[])obj;
            var arg_str = TokenType_UInt + "?";

            for(var i = 0;i < arr.Length;i++)
            {
                arg_str += arr[i] + "";

                if(i < (arr.Length - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_Array + ":" + Pack(arg_str);
        }
        else if(type == typeof(long[]))
        {
            var arr = (long[])obj;
            var arg_str = TokenType_Long + "?";

            for(var i = 0;i < arr.Length;i++)
            {
                arg_str += arr[i] + "";

                if(i < (arr.Length - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_Array + ":" + Pack(arg_str);
        }
        else if(type == typeof(ulong[]))
        {
            var arr = (ulong[])obj;
            var arg_str = TokenType_ULong + "?";

            for(var i = 0;i < arr.Length;i++)
            {
                arg_str += arr[i] + "";

                if(i < (arr.Length - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_Array + ":" + Pack(arg_str);
        }
        else if(type == typeof(float[]))
        {
            var arr = (float[])obj;
            var arg_str = TokenType_Float + "?";

            for(var i = 0;i < arr.Length;i++)
            {
                arg_str += PackFloat(arr[i]);

                if(i < (arr.Length - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_Array + ":" + Pack(arg_str);
        }
        else if(type == typeof(double[]))
        {
            var arr = (double[])obj;
            var arg_str = TokenType_Double + "?";

            for(var i = 0;i < arr.Length;i++)
            {
                arg_str += PackDouble(arr[i]);

                if(i < (arr.Length - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_Array + ":" + Pack(arg_str);
        }
        else if(type == typeof(string[]))
        {
            var arr = (string[])obj;
            var arg_str = TokenType_String + "?";

            for(var i = 0;i < arr.Length;i++)
            {
                arg_str += Pack(arr[i]) + "";

                if(i < (arr.Length - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_Array + ":" + Pack(arg_str);
        }
        else if(type == typeof(DataList[]))
        {
            var arr = (DataList[])obj;
            var arg_str = TokenType_DataList + "?";

            for(var i = 0;i < arr.Length;i++)
            {
                var _arg_str = "";

                for(var j = 0;j < arr[i].Count;j++)
                {
                    _arg_str += SerializeDataToken(arr[i][j]);

                    if(j < (arr[i].Count - 1))
                    {
                        _arg_str += ",";
                    }
                }

                arg_str += Pack(_arg_str) + "";

                if(i < (arr.Length - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_Array + ":" + Pack(arg_str);
        }
        else if(type == typeof(DataDictionary[]))
        {
            var arr = (DataDictionary[])obj;
            var arg_str = TokenType_DataDictionary + "?";

            for(var i = 0;i < arr.Length;i++)
            {
                var _arg_str = "";
                var keys = arr[i].GetKeys();

                for(var j = 0;j < keys.Count;j++)
                {
                    _arg_str += Pack(keys[j].String) + "?" + SerializeDataToken(arr[i][keys[j]]);

                    if(j < (keys.Count - 1))
                    {
                        _arg_str += ",";
                    }
                }

                arg_str += Pack(_arg_str) + "";

                if(i < (arr.Length - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_Array + ":" + Pack(arg_str);
        }
        else if(type == typeof(DataToken[]))
        {
            var arr = (DataToken[])obj;
            var arg_str = TokenType_DataToken + "?";

            for(var i = 0;i < arr.Length;i++)
            {
                arg_str += SerializeDataToken(arr[i]);

                if(i < (arr.Length - 1))
                {
                    arg_str += ",";
                }
            }

            return TokenType_Array + ":" + Pack(arg_str);
        }

        ThrowException("Unknown Type can't be serialized!");
        return "";
    }

    public static string Serialize<T>(T obj) where T : class
    {
        return Serialize_Internal(obj);
    }

    private static object[] ParseArguments(string context, string str)
    {
        var tokenType = TokenType_None;
        var subTokenType = TokenType_None;
        var arg_str = "";

        var items = str.Split(":");
        
        if((items.Length < 2) || (items.Length > 3))
        {
            ThrowExceptionWithContext(context, "Argument Length isn't in [2,3]: " + str);
            return null;
        }

        tokenType = int.Parse(items[0]);
        subTokenType = TokenType_None;
        arg_str = "";

        if(tokenType == TokenType_DataToken)
        {
            if(items.Length != 3)
            {
                ThrowExceptionWithContext(context, "Argument Length isn't 3 while TokenType is DataToken: " + str);
                return null;
            }

            subTokenType = int.Parse(items[1]);
            arg_str = items[2];
        }
        else
        {
            if(items.Length != 2)
            {
                ThrowExceptionWithContext(context, "Argument Length isn't 2 while TokenType isn't DataToken: " + str);
                return null;
            }

            arg_str = items[1];
        }

        return new object[3]{tokenType, subTokenType, arg_str};
    }

    [RecursiveMethod]
    private static object Deserialize_Type(string context, int tokenType, int subTokenType, string arg_str)
    {
        if(tokenType == TokenType_None)
        {
            var arguments = ParseArguments(context, arg_str);
            tokenType = (int)arguments[0];
            subTokenType = (int)arguments[1];
            arg_str = (string)arguments[2];
        }

        switch(tokenType)
        {
            case TokenType_Null:
                ThrowExceptionWithContext(context, "TokenType can't be Null: " + tokenType + ":" + arg_str);
                return null;

            case TokenType_Boolean:
                return (int.Parse(arg_str) != 0);

            case TokenType_SByte:
                return sbyte.Parse(arg_str);

            case TokenType_Byte:
                return byte.Parse(arg_str);

            case TokenType_Short:
                return short.Parse(arg_str);

            case TokenType_UShort:
                return ushort.Parse(arg_str);

            case TokenType_Int:
                return int.Parse(arg_str);

            case TokenType_UInt:
                return uint.Parse(arg_str);

            case TokenType_Long:
                return long.Parse(arg_str);

            case TokenType_ULong:
                return ulong.Parse(arg_str);

            case TokenType_Float:
                return UnpackFloat(arg_str);

            case TokenType_Double:
                return UnpackDouble(arg_str);

            case TokenType_String:
                return Unpack(arg_str);

            case TokenType_DataList:
            {
                arg_str = Unpack(arg_str);
                var items = arg_str.Split(",");
                var dataList = new DataList();

                foreach(var item in items)
                {
                    var arguments = ParseArguments(context, item);
                    var _tokenType = (int)arguments[0];
                    var _subTokenType = (int)arguments[1];
                    var _arg_str = (string)arguments[2];

                    if(_subTokenType != TokenType_None)
                    {
                        ThrowExceptionWithContext(context, "SubTokenType isn't None: " + item);
                        return null;
                    }

                    dataList.Add(UnpackDataToken(Deserialize_Type(context, TokenType_DataToken, _tokenType, _arg_str)));
                }

                return dataList;
            }

            case TokenType_DataDictionary:
            {
                arg_str = Unpack(arg_str);
                var items = arg_str.Split(",");
                var dataDictionary = new DataDictionary();

                foreach(var item in items)
                {
                    var dic_items = item.Split("?");

                    if(dic_items.Length != 2)
                    {
                        ThrowExceptionWithContext(context, "DataDictionary Item Length isn't 2: " + item);
                        return null;
                    }

                    var key_arguments = ParseArguments(context, dic_items[0]);

                    var key = UnpackDataToken(Deserialize_Type(context, TokenType_DataToken, (int)key_arguments[0], (string)key_arguments[2]));

                    if(dataDictionary.ContainsKey(key))
                    {
                        ThrowExceptionWithContext(context, "Key " + key + " is already in DataDictionary: " + item);
                        return null;
                    }
            
                    var arguments = ParseArguments(context, dic_items[1]);
                    var _tokenType = (int)arguments[0];
                    var _subTokenType = (int)arguments[1];
                    var _arg_str = (string)arguments[2];

                    if(_subTokenType != TokenType_None)
                    {
                        ThrowExceptionWithContext(context, "SubTokenType isn't None: " + item);
                        return null;
                    }
                    
                    dataDictionary.Add(key, UnpackDataToken(Deserialize_Type(context, TokenType_DataToken, _tokenType, _arg_str)));
                }

                return dataDictionary;
            }

            case TokenType_Reference:
                ThrowExceptionWithContext(context, "TokenType can't be Reference: " + tokenType + ":" + arg_str);
                return null;

            case TokenType_Error:
                ThrowExceptionWithContext(context, "TokenType can't be Error: " + tokenType + ":" + arg_str);
                return null;

            case TokenType_Array:
            {
                arg_str = Unpack(arg_str);
                var array_items = arg_str.Split("?");
        
                if(array_items.Length != 2)
                {
                    ThrowExceptionWithContext(context, "ArrayItems Length isn't 2: " + arg_str);
                    return null;
                }

                var elements = array_items[1].Split(",");
                var _tokenType = int.Parse(array_items[0]);

                switch(_tokenType)
                {
                    case TokenType_Null:
                        ThrowExceptionWithContext(context, "TokenType can't be Null: " + tokenType + ":" + arg_str);
                        return null;

                    case TokenType_Boolean:
                    {
                        var arr = new bool[elements.Length];

                        for(var i = 0;i < elements.Length;i++)
                        {
                            arr[i] = int.Parse(elements[i]) != 0;
                        }

                        return arr;
                    }

                    case TokenType_SByte:
                    {
                        var arr = new sbyte[elements.Length];

                        for(var i = 0;i < elements.Length;i++)
                        {
                            arr[i] = sbyte.Parse(elements[i]);
                        }

                        return arr;
                    }

                    case TokenType_Byte:
                    {
                        var arr = new byte[elements.Length];

                        for(var i = 0;i < elements.Length;i++)
                        {
                            arr[i] = byte.Parse(elements[i]);
                        }

                        return arr;
                    }

                    case TokenType_Short:
                    {
                        var arr = new short[elements.Length];

                        for(var i = 0;i < elements.Length;i++)
                        {
                            arr[i] = short.Parse(elements[i]);
                        }

                        return arr;
                    }

                    case TokenType_UShort:
                    {
                        var arr = new ushort[elements.Length];

                        for(var i = 0;i < elements.Length;i++)
                        {
                            arr[i] = ushort.Parse(elements[i]);
                        }

                        return arr;
                    }

                    case TokenType_Int:
                    {
                        var arr = new int[elements.Length];

                        for(var i = 0;i < elements.Length;i++)
                        {
                            arr[i] = int.Parse(elements[i]);
                        }

                        return arr;
                    }

                    case TokenType_UInt:
                    {
                        var arr = new uint[elements.Length];

                        for(var i = 0;i < elements.Length;i++)
                        {
                            arr[i] = uint.Parse(elements[i]);
                        }

                        return arr;
                    }

                    case TokenType_Long:
                    {
                        var arr = new long[elements.Length];

                        for(var i = 0;i < elements.Length;i++)
                        {
                            arr[i] = long.Parse(elements[i]);
                        }

                        return arr;
                    }

                    case TokenType_ULong:
                    {
                        var arr = new ulong[elements.Length];

                        for(var i = 0;i < elements.Length;i++)
                        {
                            arr[i] = ulong.Parse(elements[i]);
                        }

                        return arr;
                    }

                    case TokenType_Float:
                    {
                        var arr = new float[elements.Length];

                        for(var i = 0;i < elements.Length;i++)
                        {
                            arr[i] = UnpackFloat(elements[i]);
                        }

                        return arr;
                    }

                    case TokenType_Double:
                    {
                        var arr = new double[elements.Length];

                        for(var i = 0;i < elements.Length;i++)
                        {
                            arr[i] = UnpackDouble(elements[i]);
                        }

                        return arr;
                    }

                    case TokenType_String:
                    {
                        var arr = new string[elements.Length];

                        for(var i = 0;i < elements.Length;i++)
                        {
                            arr[i] = Unpack(elements[i]);
                        }

                        return arr;
                    }

                    case TokenType_DataList:
                    {
                        var arr = new DataList[elements.Length];

                        for(var i = 0;i < elements.Length;i++)
                        {
                            arr[i] = (DataList)Deserialize_Type(context, _tokenType, TokenType_None, Unpack(elements[i]));
                        }

                        return arr;
                    }

                    case TokenType_DataDictionary:
                    {
                        var arr = new DataDictionary[elements.Length];

                        for(var i = 0;i < elements.Length;i++)
                        {
                            arr[i] = (DataDictionary)Deserialize_Type(context, _tokenType, TokenType_None, Unpack(elements[i]));
                        }

                        return arr;
                    }

                    case TokenType_Reference:
                        ThrowExceptionWithContext(context, "TokenType can't be Reference: " + _tokenType + ":" + arg_str);
                        return null;

                    case TokenType_Error:
                        ThrowExceptionWithContext(context, "TokenType can't be Error: " + _tokenType + ":" + arg_str);
                        return null;

                    case TokenType_Array:
                        ThrowExceptionWithContext(context, "Array can't nested directly: " + arg_str);
                        return null;

                    case TokenType_DataToken:
                    {
                        var arr = new DataToken[elements.Length];

                        for(var i = 0;i < elements.Length;i++)
                        {
                            var arguments = ParseArguments(context, _tokenType + ":" + Unpack(elements[i]));
                            _tokenType = (int)arguments[0];
                            var _subTokenType = (int)arguments[1];
                            var _arg_str = (string)arguments[2];
                            arr[i] = UnpackDataToken(Deserialize_Type(context, _tokenType, _subTokenType, _arg_str));
                        }

                        return arr;
                    }

                    default:
                        ThrowExceptionWithContext(context, "Unknown TokenType " + tokenType + ": " + arg_str);
                        return null;
                }
            }

            case TokenType_DataToken:
                switch(subTokenType)
                {
                    case TokenType_Null:
                        ThrowExceptionWithContext(context, "TokenType can't be Null: " + subTokenType + ":" + arg_str);
                        return null;

                    case TokenType_Boolean:
                        return PackDataToken((int.Parse(arg_str) != 0));

                    case TokenType_SByte:
                        return PackDataToken(sbyte.Parse(arg_str));

                    case TokenType_Byte:
                        return PackDataToken(byte.Parse(arg_str));

                    case TokenType_Short:
                        return PackDataToken(short.Parse(arg_str));

                    case TokenType_UShort:
                        return PackDataToken(ushort.Parse(arg_str));

                    case TokenType_Int:
                        return PackDataToken(int.Parse(arg_str));

                    case TokenType_UInt:
                        return PackDataToken(uint.Parse(arg_str));

                    case TokenType_Long:
                        return PackDataToken(long.Parse(arg_str));

                    case TokenType_ULong:
                        return PackDataToken(ulong.Parse(arg_str));

                    case TokenType_Float:
                        return PackDataToken(UnpackFloat(arg_str));

                    case TokenType_Double:
                        return PackDataToken(UnpackDouble(arg_str));

                    case TokenType_String:
                        return PackDataToken(Unpack(arg_str));

                    case TokenType_DataList:
                        return PackDataToken((DataList)Deserialize_Type(context, subTokenType, TokenType_None, arg_str));

                    case TokenType_DataDictionary:
                        return PackDataToken((DataDictionary)Deserialize_Type(context, subTokenType, TokenType_None, arg_str));

                    case TokenType_Reference:
                        ThrowExceptionWithContext(context, "TokenType can't be Reference: " + subTokenType + ":" + arg_str);
                        return null;

                    case TokenType_Error:
                        ThrowExceptionWithContext(context, "TokenType can't be Error: " + subTokenType + ":" + arg_str);
                        return null;

                    case TokenType_Array:
                        return PackDataToken(new DataToken(Deserialize_Type(context, subTokenType, TokenType_None, arg_str)));

                    case TokenType_DataToken:
                        ThrowExceptionWithContext(context, "DataToken can't nested directly: " + arg_str);
                        return null;

                    default:
                        ThrowExceptionWithContext(context, "Unknown TokenType " + subTokenType + ": " + arg_str);
                        return null;
                }

            default:
                ThrowExceptionWithContext(context, "Unknown TokenType " + tokenType + ": " + arg_str);
                return null;
        }
    }
    
    public static T Deserialize<T>(string str) where T : class
    {
        var r = Deserialize_Type(str, TokenType_None, TokenType_None, str);

        if(r == null)
        {
            return default;
        }
        else
        {
            return (T)r;
        }
    }
}
