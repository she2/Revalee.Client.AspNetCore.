using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Revalee.NetCore
{
    public interface IRevaleeRegistrar
    {
        Task<Guid> RequestCallbackAsync(Uri callbackUri, DateTimeOffset callbackTime);
        Task<Guid> RequestCallbackAsync(Uri callbackUri, DateTimeOffset callbackTime, CancellationToken cancellationToken);
        Task<bool> CancelCallbackAsync(Guid callbackId, Uri callbackUri);
        Task<bool> CancelCallbackAsync(Guid callbackId, Uri callbackUri, CancellationToken cancellationToken);
        bool ValidateCallback(HttpContext suppliedContext = null);
    }
}
