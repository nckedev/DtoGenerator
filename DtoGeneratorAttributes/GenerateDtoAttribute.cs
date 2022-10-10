namespace DtoGenerator;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class GenerateDtoAttribute : Attribute
{
    private string Name { get; set; }

    public GenerateDtoAttribute(string name)
    {
        Name = name;
    }

    public GenerateDtoAttribute()
    {
        Name = "";
    }
}