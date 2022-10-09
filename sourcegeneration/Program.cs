// See https://aka.ms/new-console-template for more information


using DtoGenerator;

namespace MyNamespace
{


    public static class Program
    {
        public static void Main(string[] args)
        {

            var a = new Test2Dto();

        }
    }

    [GenerateDto("etsd")]
    public class Test
    {
        public int Count { get; set; }

    }
}