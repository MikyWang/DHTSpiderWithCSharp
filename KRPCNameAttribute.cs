namespace DHT;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class KRPCNameAttribute : Attribute
{
    private readonly string _name;

    public string Name => _name;
    public KRPCNameAttribute(string name)
    {
        _name = name;
    }

}
