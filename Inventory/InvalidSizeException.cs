using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLIB.Inventory
{
    public class InvalidSizeException : Exception
    {
        public InvalidSizeException(string message) : base(message) { }
    }
}
