using ellabi.Schedules;
using Quartz;
using System;
using System.Threading.Tasks;

namespace ellabi.Jobs
{
    public class ScheduleArrivedJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var dataMap = context.JobDetail.JobDataMap;
                var action = (ScheduleBase.ScheduleAction)Enum.Parse(typeof(ScheduleBase.ScheduleAction), dataMap["action"].ToString());
                StaticCode.OnScheduleArrived(action);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            await Task.CompletedTask;
        }
    }
}