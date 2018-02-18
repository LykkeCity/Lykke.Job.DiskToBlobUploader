using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.DiskToBlobUploader.Core.Services
{
    public interface IBlobSaver
    {
        Task SaveToBlobAsync(IEnumerable<string> blocks, string storagePath);
    }
}
