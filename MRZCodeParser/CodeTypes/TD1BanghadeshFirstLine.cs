using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MRZCodeParser.CodeTypes
{
    internal class TD1BanghadeshFirstLine : MrzLine
    {
        internal TD1BanghadeshFirstLine(string value) : base(value)
        {
        }

        protected override string Pattern => "([A|C|I][A-Z0-9<]{1})(BGD)([0-9]{9}<[0-9]{1})([0-9]{1})([A-Z0-9<]{13})";

        internal override IEnumerable<FieldType> FieldTypes => new[]
        {
            FieldType.DocumentType,
            FieldType.CountryCode,
            FieldType.DocumentNumber,
            FieldType.DocumentNumberCheckDigit,
            FieldType.OptionalData1
        };

        public override FieldsCollection Fields
        {
            get
            {
                var regex = new Regex(Pattern);
                var match = regex.Match(Value);

                if (!match.Success)
                {
                    throw new MrzCodeException($"Line: {Value} does not match to pattern: {Pattern}");
                }

                var fields = new List<Field>();
                for (var i = 0; i < FieldTypes.Count(); i++)
                {
                    fields.Add(new Field(
                        FieldTypes.ElementAt(i),
                        new ValueCleaner(match.Groups[i + 1].Value).Clean().Replace(" ", string.Empty)));
                }

                return new FieldsCollection(fields);
            }
        }
    }
}
