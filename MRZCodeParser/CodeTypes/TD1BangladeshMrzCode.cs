﻿using System.Collections.Generic;
using System.Linq;

namespace MRZCodeParser.CodeTypes
{
    internal class TD1BangladeshMrzCode : MrzCode
    {
        internal TD1BangladeshMrzCode(IEnumerable<string> lines) : base(lines)
        {
        }

        public override CodeType Type => CodeType.TD1;

        public override IEnumerable<MrzLine> Lines => new MrzLine[]
        {
            new TD1BangladeshFirstLine(RawLines.First()),
            new TD1SecondLine(RawLines.ElementAt(1)),
            new TD1ThirdLine(RawLines.Last())
        };
    }
}