namespace Opal
{
    public static class ScannerStates
    {
        public static int[,] Decompress8(byte[] compressed, int maxClasses, int maxStates) => 
            Decompress(new Reader8(compressed), maxClasses, maxStates);

        public static int[,] Decompress16(byte[] compressed, int maxClasses, int maxStates) =>
            Decompress(new Reader16(compressed), maxClasses, maxStates);

        public static int[,] Decompress24(byte[] compressed, int maxClasses, int maxStates) =>
            Decompress(new Reader24(compressed), maxClasses, maxStates);

        public static int[,] Decompress32(byte[] compressed, int maxClasses, int maxStates) =>
            Decompress(new Reader32(compressed), maxClasses, maxStates);

        private static int[,] Decompress(Reader reader, int maxClasses, int maxStates)
        {
            maxClasses++;
            var states = new int[maxStates + 1, maxClasses];
            using (reader)
            {
                for (var i = 0; i < maxStates; i++)
                    for (var j = 0; j < maxClasses; j++)
                        states[i, j] = reader.Read();
            }
            AdjustEofClass(states, maxClasses, maxStates);
            return states;
        }

        private static void AdjustEofClass(int[,] states, int maxClasses, int maxStates)
        {
            for (int i = 1; i < maxClasses; i++)
            {
                if (states[0, i] == 0)
                {
                    states[0, i] = maxStates;
                    states[maxStates, i] = maxStates;
                }
            }
            states[maxStates, 0] = -1;
        }
    }
}
