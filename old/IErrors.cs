using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opal
{
    public interface IErrors
    {
        void UnexpectedToken(Token t);
    }
}
