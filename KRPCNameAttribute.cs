namespace DHT;

[AttributeUsage(AttributeTargets.Property)]
public class KRPCNameAttribute : Attribute
{
    public string Name { get; }
    public KRPCNameAttribute(string name)
    {
        Name = name;
    }

}
