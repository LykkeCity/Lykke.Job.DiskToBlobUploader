using System.Threading.Tasks;

namespace Lykke.Job.DiskToBlobUploader.Core.Services
{
    public interface IDirectoryProcessor
    {
        Task ProcessDirectoryAsync();
    }
}
