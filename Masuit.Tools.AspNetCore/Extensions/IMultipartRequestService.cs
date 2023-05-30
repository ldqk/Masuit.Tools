using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Masuit.Tools.AspNetCore.Extensions;

public interface IMultipartRequestService
{
    Task<(Dictionary<string, StringValues>, byte[])> GetDataFromMultiPart(MultipartReader reader, CancellationToken cancellationToken);
}
