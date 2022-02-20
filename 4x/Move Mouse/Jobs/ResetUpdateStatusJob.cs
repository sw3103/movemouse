using Quartz;
using System.Threading.Tasks;

namespace ellabi.Jobs
{
    public class ResetUpdateStatusJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            StaticCode.OnUpdateAvailablityChanged(false);
            await Task.CompletedTask;
        }
    }
}