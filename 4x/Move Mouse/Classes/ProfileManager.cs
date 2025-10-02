using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ellabi.Classes
{
    public class ProfileManager
    {
        public ObservableCollection<ActionProfile> Profiles { get; set; } = new ObservableCollection<ActionProfile>();
        public ActionProfile ActiveProfile { get; private set; }

        public void AddProfile(ActionProfile profile)
        {
            Profiles.Add(profile);
        }

        public void RemoveProfile(ActionProfile profile)
        {
            Profiles.Remove(profile);
        }

        public void SetActiveProfile(ActionProfile profile)
        {
            ActiveProfile = profile;
        }

        public void RenameProfile(ActionProfile profile, string newName)
        {
            if (profile == null || string.IsNullOrWhiteSpace(newName)) return;
            profile.Name = newName;
        }

        public IEnumerable<string> GetProfileNames()
        {
            return Profiles.Select(p => p.Name);
        }

        // Save all profiles to a file
        public void SaveProfiles(string filePath)
        {
            var serializer = new XmlSerializer(typeof(List<ActionProfile>));
            using var stream = new FileStream(filePath, FileMode.Create);
            serializer.Serialize(stream, Profiles.ToList());
        }

        // Load profiles from a file
        public void LoadProfiles(string filePath)
        {
            if (!File.Exists(filePath)) return;
            
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length == 0) return; // Skip empty files
            
            var serializer = new XmlSerializer(typeof(List<ActionProfile>));
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                var loadedProfiles = (List<ActionProfile>)serializer.Deserialize(stream);
                Profiles.Clear();
                foreach (var profile in loadedProfiles)
                {
                    Profiles.Add(profile);
                }
            }
        }
    }
}