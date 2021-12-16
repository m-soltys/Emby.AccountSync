define(["require", "loading", "dialogHelper", "formDialogStyle", "emby-checkbox", "emby-select", "emby-toggle"],
    function (require, loading, dialogHelper) {

        const pluginId = "AFEE16BE-0273-455B-89DA-8AECE378094E";

        function getSyncProfileUsersData(syncProfile) {
            return new Promise((resolve, reject) => {
                const profileCardData = {};
                ApiClient.getJSON(ApiClient.getUrl("Users")).then(
                    (users) => {
                        users.forEach(
                            (user) => {
                                if (user.Id === syncProfile.SyncToAccount) {
                                    profileCardData.toUser = user;
                                }
                                if (user.Id === syncProfile.SyncFromAccount) {
                                    profileCardData.fromUser = user;
                                }
                            });
                        resolve(profileCardData);
                    }); 
            });
        }

        function getProfileHtml(syncProfileUsersData) {

            let html = "";
            html += '<div data-syncTo="' + syncProfileUsersData.toUser.Id + '" data-syncFrom="' + syncProfileUsersData.fromUser.Id + '" class="syncButtonContainer cardBox visualCardBox syncProfile" style="max-width:322px; width:322px">';
            html += '<div class="cardScalable">';
            html += '<i class="md-icon btnDeleteProfile fab" data-index="0" style="position:absolute; right:2px; margin:1em">close</i>';
            
            html += '<h3 style="margin: 1em;"class=""><i class="md-icon">account_circle</i> From: ' + syncProfileUsersData.fromUser.Name + '</h3>';
          
            html += '<h3 style="margin: 1em;"class=""><i class="md-icon">account_circle</i> To: ' + syncProfileUsersData.toUser.Name + '</h3>'; 
           
            html += '</div>';
            html += '</div>';

            return html;  
        }

        return function(view) {
            view.addEventListener('viewshow',
                () => {

                    const userOneSelect = view.querySelector('#syncToAccount');
                    const userTwoSelect = view.querySelector('#syncFromAccount');

                    const savedProfileCards = view.querySelector('#syncProfiles');

                    ApiClient.getJSON(ApiClient.getUrl("Users")).then(
                        (users) => {
                            users.forEach(
                                (user) => {
                                   userOneSelect.innerHTML +=
                                        ('<option value="' + user.Id + '" data-name="' + user.Name + '" data-id="' + user.Id + '">' + user.Name + '</option>');
                                   userTwoSelect.innerHTML +=
                                        ('<option value="' + user.Id + '" data-name="' + user.Name + '" data-id="' + user.Id + '">' + user.Name + '</option>');
                                });
                        });

                    ApiClient.getPluginConfiguration(pluginId).then(function (config) {
                        if (config.SyncList) {
                            savedProfileCards.innerHTML = "";
                            config.SyncList.forEach((profile) => {
                                getSyncProfileUsersData(profile).then((result) => {
                                    savedProfileCards.innerHTML += getProfileHtml(result);
                                });
                            });
                        }
                    });


                    view.querySelector('#syncProfiles').addEventListener('click',
                        (e) => {

                            if (e.target.classList.contains('btnDeleteProfile')) {

                                const syncTo = e.target.closest('div.syncButtonContainer').dataset.syncto;
                                const syncFrom = e.target.closest('div.syncButtonContainer').dataset.syncfrom;

                                ApiClient.getPluginConfiguration(pluginId).then((config) => {
                                    config.SyncList = config.SyncList.filter((c) => c.SyncToAccount !== syncTo && c.SyncFromAccount !== syncFrom);
                                    
                                    ApiClient.updatePluginConfiguration(pluginId, config).then(
                                        (result) => {
                                            Dashboard.hideLoadingMsg();
                                            Dashboard.processPluginConfigurationUpdateResult(result);
                                        });
                                });

                                e.target.closest('div.syncButtonContainer').remove();
                                return false; 
                            }   

                            if (e.target.closest('div > .syncProfile')) {

                                const ele = e.target.closest('div > .syncProfile');
                                userOneSelect.value = ele.dataset.syncto;
                                userTwoSelect.value = ele.dataset.syncfrom;
                                   
                            }
                            return false;
                        });


                    view.querySelector('#syncButton').addEventListener('click',
                        (e) => {
                            e.preventDefault;

                            const user1 = userOneSelect.options[userOneSelect.selectedIndex >= 0 ? userOneSelect.selectedIndex : 0].dataset.id;
                            const user2 = userTwoSelect.options[userTwoSelect.selectedIndex >= 0 ? userTwoSelect.selectedIndex : 0].dataset.id;

                            ApiClient.getPluginConfiguration(pluginId).then((config) => {

                                let syncList = [];

                                const syncProfile = {
                                    SyncToAccount: user1,
                                    SyncFromAccount: user2
                                };

                                if(!config.SyncList.some(
                                    (sync) => sync.SyncToAccount === syncProfile.SyncToAccount && sync.SyncFromAccount === syncProfile.SyncFromAccount)) {
                                    syncList.push(syncProfile);
                                }
                                
                                if (config.SyncList) {
                                    syncList = syncList.concat(config.SyncList);
                                }
                                config.SyncList = syncList;

                                ApiClient.updatePluginConfiguration(pluginId, config).then(function (result) {
                                    Dashboard.processPluginConfigurationUpdateResult(result);
                                });

                                savedProfileCards.innerHTML = "";

                                config.SyncList.forEach((profile) => {
                                    getSyncProfileUsersData(profile).then((result) => {
                                        savedProfileCards.innerHTML += getProfileHtml(result);
                                    });
                                });

                            });
                        });  

                });

            view.addEventListener('viewhide', () => {});
            view.addEventListener('viewdestroy', () => {});
        }
    });