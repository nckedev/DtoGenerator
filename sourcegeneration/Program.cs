// See https://aka.ms/new-console-template for more information


using DtoGenerator;
using GeneratedDtos;

namespace sourcegeneration
{


    public static class Program
    {
        public static void Main(string[] args)
        {
            var c = new TestDto();
        }
    }
    
    

    [GenerateDto]
    public class Test
    {
        public int Count { get; set; }
        
        public int Count2 { get; set; }
        
        [ExcludeFromDto]
        public string Id { get; set; }
        
        private string PrivateProp { get; set; }
    }

}

namespace GeneratedDtos
{
    
    public partial class TestDto
    {
        public string? TestPartial { get; set; }
    }
}