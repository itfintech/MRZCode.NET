using System;
using System.Collections.Generic;
using System.Linq;
using MRZCodeParser.CodeTypes;

namespace MRZCodeParser
{
    public abstract class MrzCode
    {
        protected IEnumerable<string> RawLines { get; }

        public abstract CodeType Type { get; }

        public abstract IEnumerable<MrzLine> Lines { get; }

        public IEnumerable<FieldType> FieldTypes => Lines.SelectMany(x => x.FieldTypes);

        public string this[FieldType type]
        {
            get
            {
                var fields = Fields;
                var targetType = ChangeBackwardFieldTypeToCurrent(type);

                if (fields.Fields.All(x => x.Type != targetType))
                {
                    throw new MrzCodeException($"Code {Type} does not contain field {type}");
                }

                return fields[targetType].Value;
            }
        }

        protected virtual FieldType ChangeBackwardFieldTypeToCurrent(FieldType type) => type;

        [Obsolete(message: "Will be changed to internal in next version")]
        public FieldsCollection Fields
        {
            get
            {
                var fields = new List<Field>();
                foreach (var line in Lines)
                {
                    fields.AddRange(line.Fields.Fields);
                }

                return new FieldsCollection(fields);
            }
        }

        protected MrzCode(IEnumerable<string> lines)
        {
            RawLines = lines;
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Lines.Select(x => x.Value));
        }

        public static MrzCode Parse(string code)
        {
            var lines = new LineSplitter(code)
                .Split()
                .ToList();
            var handlingType = new CodeTypeDetector(lines).DetectType();

            return handlingType switch
            {
                HandlingType.TD1 => new TD1MrzCode(lines),
                HandlingType.TD1Bangladesh => new TD1BangladeshMrzCode(lines),
                HandlingType.TD2 => new TD2MrzCode(lines),
                HandlingType.TD3 => new TD3MrzCode(lines),
                HandlingType.MRVA => new MRVAMrzCode(lines),
                HandlingType.MRVB => new MRVBMrzCode(lines),
                _ => new UnknownMrzCode(lines)
            };
        }
    }
}