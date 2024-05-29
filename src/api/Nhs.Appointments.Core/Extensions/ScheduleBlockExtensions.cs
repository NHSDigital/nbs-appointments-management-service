namespace Nhs.Appointments.Core.Extensions;

public static class ScheduleBlockExtensions
{
   public static void RemoveServices(this ScheduleBlock scheduleBlock, IEnumerable<string> servicesToRemove)
   {
      scheduleBlock.Services = scheduleBlock.Services.Except(servicesToRemove).ToArray();
   }
}