using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Masuit.Tools.Core.AspNetCore;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Masuit.Tools.AspNetCore.Extensions;

[ServiceInject(ServiceLifetime.Scoped)]
public class MultipartRequestService : IMultipartRequestService
{
    public async Task<(Dictionary<string, StringValues>, byte[])> GetDataFromMultiPart(MultipartReader reader, CancellationToken cancellationToken)
    {
        var formAccumulator = new KeyValueAccumulator();
        var file = Array.Empty<byte>();

        MultipartSection section;
        while ((section = await reader.ReadNextSectionAsync(cancellationToken)) != null)
        {
            if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
            {
                continue;
            }

            if (contentDisposition.IsFormDisposition())
            {
                formAccumulator = await AccumulateForm(formAccumulator, section, contentDisposition);
            }
            else if (contentDisposition.IsFileDisposition())
            {
                // you will want to replace all of this because it copies the file into a memory stream. We don't want that.
                await using var memoryStream = new MemoryStream();
                await section.Body.CopyToAsync(memoryStream, cancellationToken);

                file = memoryStream.ToArray();
            }
        }

        return (formAccumulator.GetResults(), file);
    }

    private Encoding GetEncoding(MultipartSection section)
    {
        var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

        // UTF-7 is insecure and shouldn't be honored. UTF-8 succeeds in
        // most cases.
        if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
        {
            return Encoding.UTF8;
        }

        return mediaType.Encoding;
    }

    private async Task<KeyValueAccumulator> AccumulateForm(KeyValueAccumulator formAccumulator, MultipartSection section, ContentDispositionHeaderValue contentDisposition)
    {
        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
        using var streamReader = new StreamReader(section.Body, GetEncoding(section), true, 1024, true);
        {
            var value = await streamReader.ReadToEndAsync();
            if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
            {
                value = string.Empty;
            }
            formAccumulator.Append(key, value);

            if (formAccumulator.ValueCount > FormReader.DefaultValueCountLimit)
            {
                throw new InvalidDataException($"Form key count limit {FormReader.DefaultValueCountLimit} exceeded.");
            }
        }

        return formAccumulator;
    }
}
