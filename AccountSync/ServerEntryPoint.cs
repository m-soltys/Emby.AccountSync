namespace AccountSync
{
    using System;
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

        public ServerEntryPoint(ISessionManager sesMan, IUserManager userMan, ILogManager logManager)
        {
            Instance = this;
            SessionManager = sesMan;
            UserManager = userMan;
            Log = logManager.GetLogger(Plugin.Instance.Name);
            SessionManager.PlaybackStopped += SessionManager_PlaybackStopped;
        }

        private void SessionManager_PlaybackStopped(object sender, PlaybackStopEventArgs e)
        {
            var accountSyncs = Plugin.Instance.Configuration.SyncList.Where(user => user.SyncFromAccount == e.Session.UserId).ToList();

            foreach (var syncToUser in accountSyncs.Select(sync => UserManager.GetUserById(sync.SyncToAccount)))
            {
                Synchronize.SynchronizePlayState(syncToUser, e.Item, e.PlaybackPositionTicks, e.PlayedToCompletion);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            Plugin.Instance.UpdateConfiguration(Plugin.Instance.Configuration);
        }
    }
}