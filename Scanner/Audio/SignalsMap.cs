using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner.Audio
{
    public class SignalsMap
    {
        public Dictionary<string, List<string>> Map { get; } = new();

        public SignalsMap(string filePath)
        {

        }
    }
}
