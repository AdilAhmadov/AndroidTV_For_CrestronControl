using AndroidTvControl.Hellpers;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro.CrestronThread;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AndroidTvControl.Configuration_Manager
{
    public class ConfigurationManager
    {
        public event EventHandler<LoadConfigEventArgs> OnConfigCreated;

        private string ConfigFilePath = Directory.GetApplicationRootDirectory() + "/user/TvManifest.json";
        public ConfigurationData TvData;
        public string SaveSerielizedObject { get; set; } = string.Empty;
        public string ErrorMessage { get; set; }
        public bool ConfigReady { get; set; } = false;
        public ConfigurationManager()
        {
            TvData = new ConfigurationData();
        }
        public void GetConfig()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    Debug.PrintLine($"Android TV Configuration File is not exists creating");

                    SeedData();
                    GetAsJson();
                    FileHandler.WriteFile(ConfigFilePath, SaveSerielizedObject);
                }
                Thread.Sleep(2000);
                GetTvConfiguration();

                OnConfigCreated?.Invoke(this,new LoadConfigEventArgs("Success") { Apps = TvData.Apps, Configurations = TvData.Configurations });
            }
            catch (Exception ex)
            {
                ErrorLog.Error("Error In Loading Configuration File Exception: ", ex.Message);                
                OnConfigCreated?.Invoke(this,new LoadConfigEventArgs("Fail") { ErrorMessage = "Fail" });
            }
        }
        private void GetTvConfiguration()
        {
            try
            {
                var result = FileHandler.ReadFile(ConfigFilePath);               
                TvData = new ConfigurationData();
                TvData = JsonConvert.DeserializeObject<ConfigurationData>(result, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore});
            }
            catch (Exception ex)
            {
                ErrorLog.Exception("GetTvConfiguration: ", ex.InnerException);
            }
            
        }
        public void StoreTvData()
        {
            GetAsJson();
            FileHandler.StoreFile(ConfigFilePath, SaveSerielizedObject);
        }
        public void UpdateTvList(TVEntity entity)
        {
            var content = TvData.Configurations;
            if (content != null)
            {
                var result = content.Find(x => x.TvId == entity.TvId);
                if (result != null)
                {
                    var index = content.IndexOf(result);
                    content[index].TvId = entity.TvId;
                    content[index].RoomName = entity.RoomName;
                    content[index].IpAddress = entity.IpAddress;
                    content[index].Mac = entity.Mac;
                    content[index].Certificate = entity.Certificate;
                }
            }
        }
        public void UpdateTvCertificate(int id, string certificate, string Ip)
        {
            var content = TvData.Configurations;

            if (content != null)
            {
                var result = content.Find(x => x.TvId == id && x.IpAddress == Ip);
                if (result != null)
                {
                    var index = TvData.Configurations.IndexOf(result);
                    content[index].TvId = id;
                    content[index].RoomName = content[index].RoomName;
                    content[index].IpAddress = content[index].IpAddress;
                    content[index].Mac = content[index].Mac;
                    content[index].Certificate = certificate;
                }
            }
        }
        public string GetTvAppUrl(int id)
        {
            var result = TvData.Apps.Find(x => x.AppId == id);
            return result.Url;
        }
        public void UpdateTvCertificate(int id, string certificate, bool IsPaired)
        {
            //var content = TvData.Configurations;
            var content = TvData.Configurations.Find(x => x.TvId == id);

            if (content != null)
            {
                var index = TvData.Configurations.IndexOf(content);
                var update = TvData.Configurations[index];
                update.TvId = id;
                update.RoomName = update.RoomName;
                update.IpAddress = update.IpAddress;
                update.Mac = update.Mac;
                update.Certificate = certificate;
                update.IsPaired = IsPaired;
            }
            StoreTvData();
            //Debug.PrintLine($"TV Certificate for TV ID {id} has been updated {certificate}");
        }
        public string GetCertificate(int id)
        {
            var result = "";
            var entity = TvData.Configurations.Find(x => x.TvId == id);
            if (entity != null)
                result = entity.Certificate;

            return result;
        }
        public string GetTvIp(int id)
        {
            var result = "";
            var entity = TvData.Configurations.Find(x => x.TvId == id);
            if (entity != null)
                result = entity.IpAddress;

            return result;
        }
        public string GetCertificate(string Ip)
        {
            var result = "";
            var entity = TvData.Configurations.Find(x => x.IpAddress == Ip);
            if (entity != null)
                result = entity.Certificate;

            return result;
        }
        private void GetAsJson()
        {
            SaveSerielizedObject = String.Empty;
            SaveSerielizedObject = JsonConvert.SerializeObject(TvData, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
        private List<TVApp> SeedDataAppList()
        {
            var list = new List<TVApp>()
            {
                new TVApp(){AppId=0, AppName="NetFlix", Url= "https://www.netflix.com/title.*" },
                new TVApp(){AppId=1, AppName="Play Store", Url= "https://play.google.com/store/" },
                new TVApp(){AppId=2, AppName="Youtube", Url= "https://www.youtube.com/*" }, //vnd.youtube.launch://
                new TVApp(){AppId=3, AppName="Apple TV", Url= "https://tv.apple.com" },
                new TVApp(){AppId=4, AppName="BBC iPlayer", Url= "bbc://iplayer/browse" },
                new TVApp(){AppId=5, AppName="Plex", Url= "plex://" },
                new TVApp(){AppId=6, AppName="Disney", Url= "https://www.disneyplus.com" },
                new TVApp(){AppId=7, AppName="Prime Video", Url= "https://app.primevideo.com" },
                new TVApp(){AppId=8, AppName="Youtube Music", Url= "https://music.youtube.com/*" }, //vnd.youtube.music
            };
            return list;
        }
        private List<TVEntity> SeedDataTVConfiguration()
        {
            var config = new List<TVEntity>
            {
                new TVEntity(){TvId = 1, RoomName = "Master Bath", Certificate = "", IpAddress ="192.168.18.61", Mac = "78:F2:35:45:96:4E", IsPaired = false},
                new TVEntity(){TvId = 2, RoomName = "Dry Kitchen", Certificate = "", IpAddress ="192.168.18.62", Mac = "4C:31:2D:4E:79:1F", IsPaired = false},
                new TVEntity(){TvId = 3, RoomName = "Study Room",  Certificate = "", IpAddress ="192.168.18.63", Mac = "3C:2C:A6:10:6E:41", IsPaired = false},
                new TVEntity(){TvId = 4, RoomName = "Study Desk",  Certificate = "", IpAddress ="192.168.18.64", Mac = "04:7A:0B:41:58:C0", IsPaired = false},
                new TVEntity(){TvId = 5, RoomName = "Living Room", Certificate = "", IpAddress ="192.168.18.67", Mac = "3C:2C:A6:8C:29:46", IsPaired = false},
            };

            return config;
        }
        private void SeedData()
        {
            TvData = new ConfigurationData() { Configurations = SeedDataTVConfiguration(), Apps = SeedDataAppList() };
        }
    }
    public class LoadConfigEventArgs : EventArgs
    {
        public LoadConfigEventArgs(string error)
        {
            this.ErrorMessage = error;
        }
        public List<TVEntity> Configurations { get; set; }
        public List<TVApp> Apps { get; set; }
        public string ErrorMessage { get; set; }
    }
}
