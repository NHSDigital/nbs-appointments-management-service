using System.Text.Json.Serialization;

namespace Nhs.Appointments.Core;

public class TimePeriod
{
    [JsonConstructor]
    public TimePeriod()
    {
        
    }
    
    public TimePeriod(DateTime from, DateTime until)
    {
        if (until <= from)
            throw new ArgumentException("Start must be earlier than finish");
        From = from;
        Duration = until - from;
    }

    public TimePeriod(DateTime from, TimeSpan duration)
    {
        From = from;
        Duration = duration;
    }

    public DateTime From { get; private set; }

    [JsonIgnore]
    public TimeSpan Duration { get; private set; }

    public DateTime Until => From.Add(Duration);

    public TimePeriod[] Split(DateTime pointInTime)
    {
        if(pointInTime > From && pointInTime < Until)
        {
            return new TimePeriod[]
            {
                new TimePeriod(From, pointInTime),
                new TimePeriod(pointInTime, Until)
            };
        }

        return new TimePeriod[] { this };
    }

    public TimePeriod[] Split(TimePeriod block)
    {
        return Split(block.From, block.Until);
    }

    public TimePeriod[] Split(DateTime from, DateTime until)
    {
        // case 0 block covers entire span - in which case return an empty array
        if (from <= From)
        {
            if (until >= Until)
            {
                return new TimePeriod[0];
            }

            // case 1 from is before original start date, then create a new one that starts at the until
            return new TimePeriod[] { new TimePeriod(until, Until) };
        }

        if (until >= Until)
        {
            return new TimePeriod[] { new TimePeriod(From, from) };
        }

        return new TimePeriod[]
        {
            new TimePeriod(From, from),
            new TimePeriod(until, Until)
        };
    }

    public bool Overlaps(TimePeriod other) => Overlaps(other.From, other.Until);
    

    public bool Overlaps(DateTime from, DateTime until)
    {
        return until >= From && until <= Until || from >= From && from <= Until;
    }


    public bool Contains(TimePeriod block)
    {
        return Contains(block.From, block.Until);
    }

    public bool Contains(DateTime from, DateTime until) 
    {
        return From <= from && Until >= until;
    }

    public TimePeriod[] Divide(TimeSpan division)
    {
        if (division == TimeSpan.Zero)
            throw new DivideByZeroException();

        var blocks = new List<TimePeriod>();
        var block = new TimePeriod(From, Until);

        while(block != null && block.Duration >= division)
        {
            var chip = new TimePeriod(block.From, division);
            blocks.Add(chip);
            block = block.Split(chip).SingleOrDefault();
        }

        return blocks.ToArray();
    }
}       
