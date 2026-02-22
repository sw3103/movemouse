using Quartz;
using System.Threading.Tasks;

namespace ellabi.Jobs
{
    public class RefreshSchedulesJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            StaticCode.OnRefreshSchedules();
            await Task.CompletedTask;
        }
    }
}