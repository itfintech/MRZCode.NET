using System.Collections.Generic;
using System.Linq;

namespace MRZCodeParser
{
    internal class CodeTypeDetector
    {
        private readonly IEnumerable<string> lines;

        internal CodeTypeDetector(IEnumerable<string> lines)
        {
            this.lines = lines;
        }

        internal HandlingType DetectType()
        {
            HandlingType type = lines.Count() == 3 && lines.First().Length == 30
                ? lines.First().StartsWith("I<BGD")
                    ? HandlingType.TD1Bangladesh
                    : HandlingType.TD1
                : lines.First().Length == 44 && lines.Count() == 2
                    ? lines.First()[0] == 'P'
                        ? HandlingType.TD3
                        : HandlingType.MRVA
                    : lines.First().Length == 36 && lines.Count() == 2
                        ? lines.First()[0] == 'V'
                            ? HandlingType.MRVB
                            : HandlingType.TD2
                        : HandlingType.UNKNOWN;

            return type;
        }
    }
}