﻿using CsvHelper.Configuration;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace CsvDataTool;

public class SiteMap : ClassMap<SiteDocument>
{
    public SiteMap()
    {
        Map(m => m.Id).Name("ID");
        Map(m => m.Name).Name("Name");
        Map(m => m.Address).Name("Address");
        Map(m => m.PhoneNumber).Name("PhoneNumber");
        Map(m => m.Location).Convert(x =>
            new Location(
                "Point",
                [x.Row.GetField<double>("Longitude"), x.Row.GetField<double>("Latitude")]
                ));
        Map(m => m.IntegratedCareBoard).Name("ICB");
        Map(m => m.Region).Name("Region");
        Map(m => m.DocumentType).Constant("site");
    }
}



