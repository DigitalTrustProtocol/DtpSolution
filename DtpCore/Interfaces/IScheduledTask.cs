// Source:
// https://blog.maartenballiauw.be/post/2017/08/01/building-a-scheduled-cache-updater-in-aspnet-core-2.html

namespace DtpCore.Interfaces
{
    public interface IScheduledTask
    {
        string Schedule { get; }
    }
}
