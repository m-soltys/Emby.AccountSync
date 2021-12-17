namespace AccountSync
{
    using System;
    using System.Threading;
    using MediaBrowser.Controller.Entities;
    using MediaBrowser.Controller.Library;
    using MediaBrowser.Controller.Plugins;
    using MediaBrowser.Model.Entities;
    using MediaBrowser.Model.Logging;

    public class Synchronize : IServerEntryPoint
    {
        private static IUserDataManager UserDataManager { get; set; }
        private static ILogger Log { get; set; }

        public Synchronize(IUserDataManager userDataManager, ILogManager logManager)
        {
            UserDataManager = userDataManager;
            Log = logManager.GetLogger(Plugin.Instance.Name);
        }

        public static void SynchronizePlayState(
            User syncToUser,
            User syncFromUser,
            BaseItem item)
        {
            var syncToItemData = UserDataManager.GetUserData(syncToUser, item); //Sync To
            var syncFromItemData = UserDataManager.GetUserData(syncFromUser, item); //Sync From

            if ((syncToItemData.PlaybackPositionTicks != syncFromItemData.PlaybackPositionTicks || syncToItemData.Played != syncFromUser.Played)
                && (syncFromItemData.PlaystateLastModified > syncToItemData.PlaystateLastModified || syncFromItemData.LastPlayedDate > syncToItemData.LastPlayedDate))
            {
                syncToItemData.PlaybackPositionTicks = syncFromItemData.Played ? 0 : syncFromItemData.PlaybackPositionTicks;
                syncToItemData.Played = syncFromItemData.Played;

                UserDataManager.SaveUserData(syncToUser, item, syncToItemData, UserDataSaveReason.PlaybackProgress, CancellationToken.None);
            }
        }

        public static void SynchronizePlayState(
            User syncToUser,
            BaseItem item,
            long? playbackPositionTicks,
            bool playedToCompletion)
        {
            var syncToUserItemData = UserDataManager.GetUserData(syncToUser, item); //Sync To

            syncToUserItemData.PlaybackPositionTicks = playedToCompletion ? 0 : playbackPositionTicks ?? 0;
            syncToUserItemData.Played = playedToCompletion;

            UserDataManager.SaveUserData(syncToUser, item, syncToUserItemData, UserDataSaveReason.PlaybackProgress, CancellationToken.None);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
        }
    }
}