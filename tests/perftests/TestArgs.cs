namespace perftests
{
    public class TestArgs
    {
        public Sources Sources { get; }
        public int Iterations { get; set; }

        public TestArgs(int iterations)
        {
            Iterations = iterations;
            Sources = new Sources();
        }

        public TestArgs Add(string filePath)
        {
            Sources.Add(filePath);
            return this;
        }
    }
}
