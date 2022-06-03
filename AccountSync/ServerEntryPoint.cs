namespace AccountSync
{
    using System.Linq;
    using MediaBrowser.Controller.Library;
    using MediaBrowser.Controller.Plugins;
    using MediaBrowser.Controller.Session;
    using MediaBrowser.Model.Logging;

    public class ServerEntryPoint : IServerEntryPoint
    {
        public static ServerEntryPoint Instance { get; set; }
        private ISessionManager SessionManager { get; }
        private IUserManager UserManager { get; }
        private ILogger Log { get; }

        public ServerEntryPoint(ISessionManager sessionManager, IUserManager userManager, ILogManager logManager)
        {
            Instance = this;
            SessionManager = sessionManager;
            UserManager = userManager;
            Log = logManager.GetLogger(Plugin.Instance.Name);
            SessionManager.PlaybackStopped += SessionManager_PlaybackStopped;
        }

        private void SessionManager_PlaybackStopped(object sender, PlaybackStopEventArgs e)
        {
            var accountSyncs = Plugin.Instance.Configuration.SyncList.Where(user => user.SyncFromAccount == e.Session.UserId).ToList();
            Log.Debug("Playback stopped. Syncing from {0}", e.Session.UserName);
                
            foreach (var syncToUser in accountSyncs.Select(sync => UserManager.GetUserById(sync.SyncToAccount)))
            {
                Log.Debug("Syncing from {0} to {1}", e.Session.UserName, syncToUser);
                Synchronize.SynchronizePlayState(syncToUser, e.Item, e.PlaybackPositionTicks, e.PlayedToCompletion);
            }
        }

        public void Dispose()
        {
        }

        public void Run()
        {
            Plugin.Instance.UpdateConfiguration(Plugin.Instance.Configuration);
        }
    }
}