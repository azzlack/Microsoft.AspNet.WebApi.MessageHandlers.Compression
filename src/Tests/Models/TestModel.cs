namespace Tests.Models
{
    public class TestModel
    {
        public TestModel()
        {
        }

        public TestModel(string data)
        {
            this.Data = data;
        }

        public string Data { get; set; }
    }
}
