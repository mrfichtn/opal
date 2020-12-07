namespace Opal.Dfa
{
    /// <summary>
    /// Generates states for a scanner state machine based on a DfaNode array
    /// </summary>
    public class ScannerStateTable
    {
        private readonly DfaNode[] states;

        public ScannerStateTable(DfaNode[] states) => this.states = states;

        protected virtual int Rows => states.Length;

        public virtual int[,] Create()
        {
            var rows = Rows;
            var classes = states[0].Count + 1;
            var data = new int[rows, classes];
            for (var i = 0; i < states.Length; i++)
            {
                var state = states[i];
                data[i, 0] = state.AcceptingState;
                for (var j = 0; j < state.Count; j++)
                    data[i, j + 1] = state[j];
            }
            return data;
        }
    }
    
    public class ScannerStateTableWithSyntaxErrors : ScannerStateTable 
    {
        public ScannerStateTableWithSyntaxErrors (DfaNode[] states) : base(states) { }
        
        protected override int Rows => base.Rows + 1;
        
        public override int[,] Create()
        {
            var data = base.Create();
            var columns = data.GetLength(1);
            var length = data.GetLength(0) - 1;

            for (var i = 1; i < columns; i++)
            {
                if (data[0, i] == 0)
                {
                    data[0, i] = length;
                    data[length, i] = length;
                }
            }
            data[length, 0] = -1;
            return data;
        }
    }
}
