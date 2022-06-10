using System.Collections;
using System.Text;
namespace DHT;

public readonly struct BEncode
{
    private readonly StringBuilder _root;

    public const string String = "string";
    public const string Integer = "integer";
    public const string List = "list";
    public const string Dictionary = "dictionary";
    public BEncode() : this(string.Empty) { }
    public BEncode(string source)
    {
        _root = new StringBuilder(source);
    }
    /// <summary>
    /// 获取B编码类型对应C#类型
    /// </summary>
    /// <param name="type">B编码类型</param>
    /// <returns>C#类型</returns>
    public static string GetType(char type)
    {
        return type switch
        {
            'i' => Integer,
            'l' => List,
            'd' => Dictionary,
            _ => String
        };
    }

    /// <summary>
    /// 将B编码字符串转换为C#对象
    /// </summary>
    /// <returns></returns>
    public IList Read()
    {
        var list = new ArrayList();
        var data = _root.ToString().AsSpan();
        var index = 0;
        while (index < data.Length)
        {
            list.Add(ReadOne(data, ref index));
        }
        return list;
    }
    /// <summary>
    /// 添加一个C#对象
    /// </summary>
    /// <param name="value">C#对象</param>
    /// <typeparam name="T">C#对象类型</typeparam>
    /// <returns>B编码对象自身</returns>
    /// <exception cref="ArgumentException"></exception>
    public BEncode Add<T>(T value)
    {
        switch (value)
        {
            case int i:
                Add(i);
                break;
            case string s:
                Add(s);
                break;
            case IList l:
                Add(l);
                break;
            case Dictionary<string, object> d:
                Add(d);
                break;
            default:
                throw new ArgumentException("不支持的参数类型!");
        }
        return this;
    }
    private static object ReadOne(ReadOnlySpan<char> data, ref int index)
    {
        var type = GetType(data[index]);
        return type switch
        {
            Integer => ReadInt(data, ref index),
            String => ReadString(data, ref index),
            List => ReadList(data, ref index),
            Dictionary => ReadDictionary(data, ref index),
            _ => throw new Exception("不支持的类型!")
        };
    }

    private static int ReadInt(ReadOnlySpan<char> data, ref int index)
    {
        index++;
        var sb = new StringBuilder();
        while (data[index] != 'e')
        {
            sb.Append(data[index]);
            index++;
        }
        index++;
        return int.Parse(sb.ToString());
    }

    private static string ReadString(ReadOnlySpan<char> data, ref int index)
    {
        var sb = new StringBuilder();
        while (data[index] != ':')
        {
            sb.Append(data[index]);
            index++;
        }
        var length = int.Parse(sb.ToString());
        var res = data.Slice(++index, length).ToString();
        index += length;
        return res;
    }

    private static IList ReadList(ReadOnlySpan<char> data, ref int index)
    {
        var list = new ArrayList();
        index++;
        var type = GetType(data[index]);
        while (data[index] != 'e')
        {
            list.Add(ReadOne(data, ref index));
        }
        index++;
        return list;
    }

    private static IDictionary ReadDictionary(ReadOnlySpan<char> data, ref int index)
    {
        var dic = new Dictionary<string, object>();
        index++;
        while (data[index] != 'e')
        {
            var key = ReadString(data, ref index);
            var value = ReadOne(data, ref index);
            dic.Add(key, value);
        }
        index++;
        return dic;
    }


    private void Add(int value)
    {
        _root.Append($"i{value}e");
    }
    private void Add(string value)
    {
        _root.Append($"{value.Length}:{value}");

    }
    private void Add(IList value)
    {
        _root.Append('l');
        foreach (var l in value)
        {
            Add(l);
        }
        _root.Append('e');
    }
    private void Add(Dictionary<string, object> value)
    {
        _root.Append('d');
        foreach (var k in value.Keys.OrderBy(x => x))
        {
            Add(k);
            Add(value[k]);
        }
        _root.Append('e');
    }

    public override string ToString()
    {
        return _root.ToString();
    }
}
