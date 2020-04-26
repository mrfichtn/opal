using Generators;

namespace Opal.Containers
{
	public interface IVarProvider
	{
		bool AddVarValue(Generator generator, string varName);
	}
}
