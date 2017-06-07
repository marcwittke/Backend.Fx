namespace Backend.Fx.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Net.Http.Headers;

    public class CsvInputFormatter : InputFormatter
    {
        private static readonly MediaTypeHeaderValue TextCsv = MediaTypeHeaderValue.Parse("text/csv");

        public CsvInputFormatter()
        {
            SupportedMediaTypes.Add(TextCsv);
        }

        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var type = context.ModelType;
            var request = context.HttpContext.Request;
            MediaTypeHeaderValue requestContentType;
            MediaTypeHeaderValue.TryParse(request.ContentType, out requestContentType);

            var result = ReadStream(type, request.Body);
            return InputFormatterResult.SuccessAsync(result);
        }

        public override bool CanRead(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            MediaTypeHeaderValue requestContentType;
            MediaTypeHeaderValue.TryParse(request.ContentType, out requestContentType);

            if (requestContentType == null)
            {
                return false;
            }

            return requestContentType.IsSubsetOf(TextCsv);
        }

        private static object ReadStream(Type type, Stream stream)
        {
            if (type == typeof(string[][]))
            {
                List<string[]> result = new List<string[]>();
                var reader = new StreamReader(stream);

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');
                    result.Add(values);
                }

                return result.ToArray();
            }

            throw new ArgumentException($"Media type 'text/csv' must be formatted into 'string[][]'. Type {type.Name} is not supported.");
        }
    }
}
