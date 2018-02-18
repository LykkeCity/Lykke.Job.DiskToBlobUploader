using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.DiskToBlobUploader.Settings
{
    public class AppSettings
    {
        public DiskToBlobUploaderSettings DiskToBlobUploaderJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }

    public class DiskToBlobUploaderSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }

        [AzureBlobCheck]
        public string BlobConnectionString { get; set; }

        public string DiskPath { get; set; }

        public string Directory { get; set; }

        public string FileNameTimeFormat { get; set; }

        public bool IsPublicContainer { get; set; }
    }

    public class SlackNotificationsSettings
    {
        public AzureQueuePublicationSettings AzureQueue { get; set; }
    }

    public class AzureQueuePublicationSettings
    {
        public string ConnectionString { get; set; }

        public string QueueName { get; set; }
    }
}
