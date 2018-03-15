using Autofac;
using Common.Log;
using Lykke.Common;
using Lykke.Job.DiskToBlobUploader.Core.Services;
using Lykke.Job.DiskToBlobUploader.Settings;
using Lykke.Job.DiskToBlobUploader.Services;
using Lykke.Job.DiskToBlobUploader.PeriodicalHandlers;

namespace Lykke.Job.DiskToBlobUploader.Modules
{
    public class JobModule : Module
    {
        private readonly DiskToBlobUploaderSettings _settings;
        private readonly ILog _log;

        public JobModule(DiskToBlobUploaderSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.RegisterResourcesMonitoring(_log);

            builder.RegisterType<BlobSaver>()
                .As<IBlobSaver>()
                .SingleInstance()
                .WithParameter("blobConnectionString", _settings.BlobConnectionString)
                .WithParameter("directory", _settings.Directory)
                .WithParameter(TypedParameter.From(_settings.IsPublicContainer));

            builder.RegisterType<DirectoryProcessor>()
                .As<IDirectoryProcessor>()
                .SingleInstance()
                .WithParameter("fileNameTimeFormat", _settings.FileNameTimeFormat)
                .WithParameter("diskPath", _settings.DiskPath)
                .WithParameter("directory", _settings.Directory);

            builder.RegisterType<PeriodicalHandler>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}
