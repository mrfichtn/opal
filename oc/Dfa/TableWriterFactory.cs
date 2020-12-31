namespace Opal.Dfa
{
    public interface ITableWriterFactory
    {
        IClassTableWriter CreateClassWriter();
        IStateTableWriter CreateStateWriter(Dfa dfa, ScannerStateTable tableFactory);
    }

    public class CompressedTableWriterFactory: ITableWriterFactory
    {
        public IClassTableWriter CreateClassWriter() => new CompressedClassTableWriter();
        public IStateTableWriter CreateStateWriter(Dfa dfa, ScannerStateTable tableFactory) => 
            new CompressStateWriter(dfa, tableFactory);
    }

    public class UncompressedTableWriterFactory: ITableWriterFactory
    {
        public IClassTableWriter CreateClassWriter() => new SparseClassTableWriter();
        public IStateTableWriter CreateStateWriter(Dfa dfa, ScannerStateTable tableFactory) =>
            new UncompressedStateWriter(tableFactory);
    }
}
