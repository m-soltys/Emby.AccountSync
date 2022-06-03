using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace AccountSync.Configuration
{
    public class AccountSync
    {
        public string SyncToAccount { get; set; }
        public string SyncFromAccount { get; set; }
    }

    public class PluginConfiguration : BasePluginConfiguration
    {
        public List<AccountSync> SyncList { get; set; }
    }
}
