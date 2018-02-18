using System;
using System.Threading.Tasks;
using System.Net;
using Common;
using Common.Log;
using Lykke.Job.DiskToBlobUploader.Core.Services;

namespace Lykke.Job.DiskToBlobUploader.PeriodicalHandlers
{
    public class PeriodicalHandler : TimerPeriod
    {
        private readonly ILog _log;
        private readonly IDirectoryProcessor _directoryProcessor;

        private bool _apiIsReady = false;

        public PeriodicalHandler(
            ILog log,
            IDirectoryProcessor directoryProcessor) :
            base(nameof(PeriodicalHandler), (int)TimeSpan.FromMinutes(1).TotalMilliseconds, log)
        {
            _log = log;
            _directoryProcessor = directoryProcessor;
        }

        public override async Task Execute()
        {
            while (!_apiIsReady)
            {
                WebRequest request = WebRequest.Create($"http://localhost:{Program.Port}/api/isalive");
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    _apiIsReady = response != null && response.StatusCode == HttpStatusCode.OK;
                    if (!_apiIsReady)
                        await Task.Delay(TimeSpan.FromSeconds(1));
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            await _directoryProcessor.ProcessDirectoryAsync();

            await _log.WriteInfoAsync(nameof(PeriodicalHandler), nameof(Execute), "Directory is processed.");
        }
    }
}
