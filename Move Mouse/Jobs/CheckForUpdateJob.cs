using ellabi.Classes;
using Quartz;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace ellabi.Jobs
{
    public class CheckForUpdateJob : IJob
    {
        private static readonly HttpClient HttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var update = await GetUpdateAsync();

                if (update != null && update.LatestVersion > Assembly.GetExecutingAssembly().GetName().Version)
                {
                    StaticCode.UpdateUrl = update.DownloadUrl;
                    StaticCode.OnUpdateAvailablityChanged(true);
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private async Task<Update> GetUpdateAsync()
        {
            try
            {
                string xml = await HttpClient.GetStringAsync(StaticCode.UpdateXmlUrl);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);
                var versionNode = xmlDoc.SelectSingleNode("Update/LatestVersion");
                var urlNode = xmlDoc.SelectSingleNode("Update/DownloadUrl");

                if (versionNode != null && urlNode != null &&
                    !string.IsNullOrWhiteSpace(versionNode.InnerText) &&
                    !string.IsNullOrWhiteSpace(urlNode.InnerText) &&
                    Version.TryParse(versionNode.InnerText, out var version))
                {
                    return new Update
                    {
                        DownloadUrl = urlNode.InnerText,
                        LatestVersion = version
                    };
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            return null;
        }
    }
}
