namespace UltrasoundAssistant.DoctorClient.ViewModels;

public sealed class BoolFilterOption
{
    public string Name { get; }
    public bool? Value { get; }

    public BoolFilterOption(string name, bool? value)
    {
        Name = name;
        Value = value;
    }

    public override string ToString()
    {
        return Name;
    }
}
