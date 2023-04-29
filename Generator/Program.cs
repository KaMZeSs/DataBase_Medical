namespace Generator
{
    class Program
    {
        public static void Main(string[] args) 
        {
            var db = DataBase.GenerateDataBase(10000);
        }
    }
}