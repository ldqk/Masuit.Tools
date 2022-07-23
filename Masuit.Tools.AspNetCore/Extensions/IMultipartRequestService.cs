using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masuit.Tools.AspNetCore.Extensions;

public interface IMultipartRequestService
{
    Task<(Dictionary<string, StringValues>, byte[])> GetDataFromMultiPart(MultipartReader reader, CancellationToken cancellationToken);
}
