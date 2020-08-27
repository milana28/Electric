using System.Collections.Generic;
using System.Text;
using Electric.Models;

namespace Electric.Csv
{
    public static class GenerateCsv
    {
        public static string ReturnData(IEnumerable<KeyValue> data)
        {
            var columnNames = GetColumnNames();
            var builder = new StringBuilder();

            builder.AppendJoin(",", columnNames);
            builder.AppendLine();


            foreach (var element in data)
            {
                var values = GetValues(element);
                builder.AppendJoin(";", values);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        public static IEnumerable<KeyValue> MapEnclosureIntoKeyValueArray(List<Enclosure> enclosures)
        {
            var data = new List<KeyValue>();

            enclosures.ForEach(el =>
            {
                data.Add(new KeyValue()
                {
                    Key = el.GetType().GetProperty("Id")?.GetValue(el, null)?.ToString(),
                    Value = el.GetType().GetProperty("Name")?.GetValue(el, null)?.ToString()
                });
            });

            return data;
        }

        private static IEnumerable<string> GetColumnNames()
        {
            return new[]
            {
                "Key",
                "Value"
            };
        }

        private static IEnumerable<string> GetValues(KeyValue data)
        {
            return new[]
            {
                data.Key,
                data.Value
            };
        }
    }
}