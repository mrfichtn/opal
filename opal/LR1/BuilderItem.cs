namespace Opal.LR1
{
    public class BuilderItem
    {
        public BuilderItem(int action, LR1Item lr1Item)
        {
            Action = action;
            LR1Item = lr1Item;
        }

        #region Properties

        public int Action { get; }
        public LR1Item LR1Item { get; }

        #endregion
    }
}
