# Create Interface instance
This is an example which shows how to create an interface in C#
## The problem
In my work we have to write tests which cover method with interfaces as arguments. I spend a lot of time to find the easiest way to do that ... but without any significant success.
## The solution
Example
```csharp
public interface IBaseInterface
{
    Guid Id { get; set; }
}

public interface IInterface : IBaseInterface
{
    string Text { get; set; }
    int Value { get; set; }
    INestedData Data { get; set; }
}

public interface INestedData
{
    string Name { get; set; }
    int Age { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        var instance = TestTools.CreateInterface<IInterface>(new
        {
            Id = new Guid("e59495ce-d938-48f0-8e14-35ffc43306ca"),
            Text = "It's a text",
            Value = 22,
            Data = new 
            { 
                Name = "My name",
                Age = 25
            }
        });

        Console.WriteLine(instance.Id); // Output: e59495ce-d938-48f0-8e14-35ffc43306ca
        Console.WriteLine(instance.Text); // Output: It's a text
        Console.WriteLine(instance.Value); // Output: 22
        Console.WriteLine(instance.Data.Name); // Output: My name
        Console.WriteLine(instance.Data.Age); // Output: 25
    }
}
```
This CreateInterface method creates an interface instance.

# My observations
The code is not ready for production use. The creation is not too fast. Now I use it only for test purposes.

## What's new
18 May 2023 Support of nested object was added
