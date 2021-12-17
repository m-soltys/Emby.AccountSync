namespace AccountSync
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MediaBrowser.Controller.Entities;
    using MediaBrowser.Controller.Library;
    using MediaBrowser.Model.Logging;
    using MediaBrowser.Model.Tasks;

    public class AccountSyncScheduledTask : IScheduledTask, IConfigurableScheduledTask
    {
        private IUserManager UserManager { get; }
        private IUserDataManager UserDataManager { get; }
        private ILibraryManager LibraryManager { get; }
        private ILogger Log { get; }

        public AccountSyncScheduledTask(
            IUserManager userManager,
            ILibraryManager libraryManager,
            ILogManager logManager,
            IUserDataManager userDataManager)
        {
            UserManager = userManager;
            LibraryManager = libraryManager;
            UserDataManager = userDataManager;
            Log = logManager.GetLogger(Plugin.Instance.Name);
        }

        public bool IsHidden => false;
        public bool IsEnabled => true;
        public bool IsLogged => true;
        public string Name => "Account Sync Notification";
        public string Key => "Account Sync";
        public string Description => "Sync watched states for media items between two accounts.";
        public string Category => "Accounts";

        public Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            try
            {
                foreach (var syncProfile in Plugin.Instance.Configuration.SyncList)
                {
                    var syncToUser = UserManager.GetUserById(syncProfile.SyncToAccount); //Sync To
                    var syncFromUser = UserManager.GetUserById(syncProfile.SyncFromAccount); //Sync From

                    var queryResultIds = LibraryManager.GetInternalItemIds(new InternalItemsQuery { IncludeItemTypes = new[] { "Movie", "Episode" } });

                    for (var i = 0; i <= queryResultIds.Length - 1; i++)
                    {
                        var item = LibraryManager.GetItemById(queryResultIds[i]);

                        Synchronize.SynchronizePlayState(syncToUser, syncFromUser, item);

                        progress.Report(queryResultIds.Length - (queryResultIds.Length - i) / 100);
                    }
                }
            }
            catch
            {
                // ignored
            }

            progress.Report(100.0);
            
            return Task.CompletedTask;
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return new[]
            {
                new TaskTriggerInfo
                {
                    Type = TaskTriggerInfo.TriggerInterval,
                    IntervalTicks = TimeSpan.FromHours(1).Ticks
                },
                new TaskTriggerInfo
                {
                    Type = TaskTriggerInfo.TriggerSystemEvent,
                    SystemEvent = SystemEvent.WakeFromSleep
                }
            };
        }
    }
}