using System;

namespace Generators
{
    public interface IGenerator: IDisposable
    {
        IGenerator Indent(int indent = 1);
        IGenerator UnIndent(int indent = 1);

        IGenerator StartBlock();
        IGenerator EndBlock();
        IGenerator EndBlock(string extraText);

        IGenerator Write(char value);
        IGenerator Write(string value);
        IGenerator Write(string format, params object[] args);
        IGenerator Write(IGeneratable generatable);

        IGenerator WriteLine();
        IGenerator WriteLine(string value);

        /// <summary>
        /// Writes character, not checking for indent position
        /// </summary>
        /// <param name="ch"></param>
        void WriteChar(char ch);
        
        void WriteIndent();
    }
}
