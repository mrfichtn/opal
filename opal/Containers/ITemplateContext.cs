using Generators;

namespace Opal.Containers
{
    /// <summary>
    /// Template context
    /// </summary>
	public interface ITemplateContext
	{
        /// <summary>
        /// Used by if-macro to determine condition value
        /// </summary>
        bool Condition(string varName);

        /// <summary>
        /// Injects variable value into output stream
        /// </summary>
        bool WriteVariable(Generator generator, string varName);

        /// <summary>
        /// Returns template for varName
        /// </summary>
        string? Include(string varName);
    }
}
