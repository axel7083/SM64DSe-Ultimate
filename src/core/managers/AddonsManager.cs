using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Serilog;
using SM64DSe.core.cli;
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

    public class DownloadedAddon: AddonObject
    {
        [JsonProperty("versions")]
        public string[] Versions;
    }
    
    public class AddonsManager
    {
        public static string CommandFile = "commands.sm64ds";
        
        private AddonsManager() {}

        private static AddonsManager _instance;

        private bool _isWorking = false;
        
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

        public List<DownloadedAddon> GetDownloadedAddons()
        {
            List<DownloadedAddon> output = new List<DownloadedAddon>();
            string addonsFolder = GetAddonsFolder();
            if (!Directory.Exists(addonsFolder))
                return output;

            List<AddonObject> addons = GetAddons();
            foreach (AddonObject addonObject in addons)
            {
                string target = Path.Combine(addonsFolder, Sanitize(addonObject.Name));
                if(!Directory.Exists(target))
                    continue;

                DownloadedAddon dwDownloadedAddon = new DownloadedAddon();
                dwDownloadedAddon.Name = addonObject.Name;
                dwDownloadedAddon.Repository = addonObject.Repository;
                dwDownloadedAddon.Description = addonObject.Description;

                List<string> paths = new List<string>();
                string[] dirs = Directory.GetDirectories(target);
                foreach (string dir in dirs)
                {
                    if(!File.Exists(Path.Combine(dir, CommandFile)))
                        continue;
                    paths.Add(dir);
                }

                dwDownloadedAddon.Versions = paths.ToArray();
                output.Add(dwDownloadedAddon);
            }

            return output;
        }

        public List<GitHubRelease> GetGitHubRelease(AddonObject addon)
        {
            if (addon.Repository.StartsWith("https://github.com/"))
            {
                return GitHubHelper.GetReleases(addon.Repository.Substring("https://github.com/".Length));
            }
            return new List<GitHubRelease>();
        }

        public void DownloadAndExtract(AddonObject addon, GitHubRelease release)
        {
            Log.Information($"Starting downloading release {release.Name}.");
            
            if (_isWorking)
            {
                throw new Exception("Cannot download and extract multiple releases at the same time.");
            }
            
            try
            {
                _isWorking = true;
                this.PerformDownloadAndExtract(addon, release);
            }
            catch (Exception ex)
            {
                Log.Error($"Something went wrong while downloading and extracting release {release.Name}: {ex.Message}");
            }
            finally
            {
                _isWorking = false;
            }
        }

        private string GetAddonsFolder()
        {
            if (Program.m_ROM == null || Program.m_ROM.m_Path == null)
                throw new Exception("m_ROM need to be not null.");
            
            string parentDirectory = Directory.GetParent(Program.m_ROM.m_Path).FullName;
            return Path.Combine(parentDirectory, "addons");
        }

        private void PerformDownloadAndExtract(AddonObject addon, GitHubRelease release)
        {
            string addons = GetAddonsFolder();
            if(!Directory.Exists(addons))
            {
                Log.Warning($"Creating directory {addons}.");
                Directory.CreateDirectory(addons);
            }
            
            Log.Information($"Downloading {release.ZipBallUrl}.");
            string target = Path.Combine(addons, "tmp.zip");
            if (File.Exists(target))
            {
                File.Delete(target);
            }
            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent: SM64DSe-Ultimate");
                client.DownloadFile(release.ZipBallUrl, target);
            }
            
            FastZip fastZip = new FastZip();

            string targetDir = Path.Combine(addons, Sanitize(addon.Name));
            
            Log.Debug($"Extracting {target} to {targetDir}.");
            fastZip.ExtractZip(target, targetDir, null);
            
            File.Delete(target);
            Log.Information("Download and extract finish.");
        }

        public void PerformInstall(DownloadedAddon addon, int versionSelected)
        {
            if (addon.Versions.Length < versionSelected || versionSelected < 0)
                throw new Exception("Invalid version selected.");

            string path = addon.Versions[versionSelected];
            string command = Path.Combine(path, CommandFile);
            if (!File.Exists(command))
                throw new Exception($"Missing {CommandFile} in version selected.");
            
            CLIService.Run(new[] {"batches", command, "--force", "--with-progress-bar"});
            
            Log.Debug("batches finished.");

            string objects = Path.Combine(path, "objects.json");
            if (File.Exists(objects))
            {
                ObjectDatabase.LoadFromFile(objects);
                File.Copy(
                    objects, 
                    Path.Combine(Directory.GetParent(Program.m_ROM.m_Path).FullName, "objects.json")
                    );
            }
        }

        private static string Sanitize(string name)
        {
            string nName = name.Replace(' ', '-');
            nName = nName.Replace('.', '_');
            nName = nName.Replace('/', '_');
            nName = nName.Replace('\\', '_');
            nName = nName.Replace('"', '_');
            nName = nName.Replace('\'', '_');
            return nName;
        }
    }
}