namespace MyORM;

[AttributeUsage(AttributeTargets.Property)]
public class ColumnAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
}

[AttributeUsage(AttributeTargets.Property)]
public class PrimaryKeyAttribute : Attribute { }
