using ellabi.Classes;
using Quartz;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace ellabi.Jobs
{
    //public class UpdateThemeJob : IJob
    //{
    //    public void Execute(IJobExecutionContext context)
    //    {
    //        try
    //        {
    //            //StaticCode.OnThemeUpdated(GetThemes()?.FirstOrDefault(theme => theme.StartTime <= DateTime.Now && theme.EndTime >= DateTime.Now && (theme.Regions == null || theme.Regions.Any(region => RegionInfo.CurrentRegion.EnglishName.Equals(region)))));
    //        }
    //        catch (Exception ex)
    //        {
    //            StaticCode.Logger?.Here().Error(ex.Message);
    //        }
    //    }

    //    //private Theme[] GetThemes()
    //    //{
    //    //    XmlReader xr = null;

    //    //    try
    //    //    {
    //    //        var xmlDoc = new XmlDocument();
    //    //        xmlDoc.Load(StaticCode.ThemesXmlUrl);
    //    //        var xs = new XmlSerializer(typeof(Theme[]));
    //    //        xr = XmlReader.Create(new StringReader(xmlDoc.OuterXml));
    //    //        return (Theme[])xs.Deserialize(xr);
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        StaticCode.Logger?.Here().Error(ex.Message);
    //    //    }
    //    //    finally
    //    //    {
    //    //        xr?.Close();
    //    //    }

    //    //    return null;
    //    //}
    //}
}