namespace InterfaceInstance;

public interface IBaseInterface
{
    Guid Id { get; set; }
}

public interface IInterface : IBaseInterface
{
    string Text { get; set; }
    int Value { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        var instance = TestTools.CreateInterface<IInterface>(new
        {
            Id = new Guid("e59495ce-d938-48f0-8e14-35ffc43306ca"),
            Text = "It's a text",
            Value = 22
        });

        Console.WriteLine(instance.Id); // Output: e59495ce-d938-48f0-8e14-35ffc43306ca
        Console.WriteLine(instance.Text); // Output: It's a text
        Console.WriteLine(instance.Value); // Output: 22
    }
}