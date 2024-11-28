

using Core;
using Core.Models;

class Program
{
    
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var coreRunner = new CoreRunner();
        coreRunner.Run(5, 4, 5, 40, 5, 5);

        //SFMLRenderer renderer = new SFMLRenderer(coreRunner.Data.LastGeneration);
        //renderer.DisplayTimetable();
    }
}   