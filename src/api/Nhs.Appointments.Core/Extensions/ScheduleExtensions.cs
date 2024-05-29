namespace Nhs.Appointments.Core.Extensions;

public static class WeekTemplateExtensions
{
    public static void RemoveEmptyBlocks(this WeekTemplate template)
    {        
        foreach (var item in template.Items) 
        {
            item.ScheduleBlocks = item.ScheduleBlocks.Where(sb => sb.Services.Any()).ToArray();
        }
    }

    public static void RemoveRedundantItems(this WeekTemplate template) 
    { 
        template.Items = template.Items.Where(i => i.ScheduleBlocks.Any()).ToArray();
    }
}