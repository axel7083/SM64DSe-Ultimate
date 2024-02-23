using System.Collections.Generic;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Serilog;
using SM64DSe.core.utils.Github;

namespace SM64DSe.core.managers
{
    public class AddonObject
    {
        [JsonProperty("name")]
        public string Name;
        
        [JsonProperty("repository")]
        public string Repository;
        
        [JsonProperty("description")]
        public string Description;
    }
    
    public class AddonsManager
    {
        private AddonsManager() {}

        private static AddonsManager _instance;
        
        public static AddonsManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new AddonsManager();
            }
            return _instance;
        }

        public List<AddonObject> GetAddons()
        {
            if (!File.Exists("assets/addons.json"))
            {
                Log.Warning("Cannot find assets/addons.json file.");
                return new List<AddonObject>();
            }

            using (StreamReader r = new StreamReader("assets/addons.json"))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<List<AddonObject>>(json);
            }
        }

        public List<GitHubRelease> GetGitHubRelease(AddonObject addon)
        {
            if (addon.Repository.StartsWith("https://github.com/"))
            {
                return GitHubHelper.GetReleases(addon.Repository.Substring("https://github.com/".Length));
            }
            return new List<GitHubRelease>();
        }

        public void DownloadAndExtract(GitHubRelease release)
        {
            
        }
    }
}