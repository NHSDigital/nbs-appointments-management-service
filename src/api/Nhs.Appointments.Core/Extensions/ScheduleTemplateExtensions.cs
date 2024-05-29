namespace Nhs.Appointments.Core.Extensions;

public static class ScheduleTemplateExtensions
{
    public static WeekTemplate RemoveServices(this WeekTemplate weekTemplate, IEnumerable<string> disabledServices)
    {
        foreach (var scheduleBlock in weekTemplate.Items.SelectMany(i => i.ScheduleBlocks))
        {
            scheduleBlock.RemoveServices(disabledServices);
        }
                
        weekTemplate.RemoveEmptyBlocks();
        weekTemplate.RemoveRedundantItems();
        return weekTemplate;        
    }    
}