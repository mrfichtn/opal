using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opal.Exceptions
{
    [Serializable]
    public class ErrorException: Exception
    {
        public ErrorException(string msg, int exitCode = -1)
            : base(msg)
        {
            ExitCode = exitCode;
        }

        public int ExitCode { get; }
    }
}
