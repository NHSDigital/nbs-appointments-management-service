using Nhs.Appointments.Core.Sites;

namespace Nhs.Appointments.Core.UnitTests;

public static class SiteServiceCacheTestsMockData
{
    public static readonly Site[] MockAllSites =
    [
        new(
            "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
            "Site 1",
            "1 Park Row",
            "0113 1111111",
            "ABC01",
            "R1",
            "ICB1",
            location: new Location("Point", [.505, 65]),
            InformationForCitizens: "",
            Accessibilities: new List<Accessibility> { new("accessibility/access_need_1", "true") },
            status: SiteStatus.Online, isDeleted: false,
            Type: string.Empty),
        new(
            "3ad2deb1-791b-452d-95dc-7090edd97f9a",
            "Site 2",
            "2 Park Row",
            "0113 1111111",
            "ABC02",
            "R1",
            "ICB1",
            location: new Location("Point", [.506, 65]),
            InformationForCitizens: "",
            Accessibilities: new List<Accessibility> { new("accessibility/access_need_1", "true") },
            status: SiteStatus.Offline, isDeleted: true,
            Type: string.Empty),
        new(
            "0c06c137-2f8a-4334-a594-0632d0407966",
            "Site 3",
            "3 Park Row",
            "0113 1111111",
            "ABC03",
            "R1",
            "ICB1",
            location: new Location("Point", [.507, 65]),
            InformationForCitizens: "",
            Accessibilities: new List<Accessibility> { new("accessibility/access_need_1", "true") },
            status: SiteStatus.Offline, isDeleted: null,
            Type: string.Empty),
        new(
            "b40d5219-e1c5-4f28-a2c4-d40f68bcde36",
            "Site 4",
            "4 Park Row",
            "0113 1111111",
            "ABC04",
            "R1",
            "ICB1",
            location: new Location("Point", [.507, 65]),
            InformationForCitizens: "",
            Accessibilities: new List<Accessibility> { new("accessibility/access_need_1", "true") },
            status: SiteStatus.Online, isDeleted: true,
            Type: string.Empty)
    ];

    public static readonly Site[] MockOnlyNonDeletedSites =
    [
        new(
            "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
            "Site 1",
            "1 Park Row",
            "0113 1111111",
            "ABC01",
            "R1",
            "ICB1",
            location: new Location("Point", [.505, 65]),
            InformationForCitizens: "",
            Accessibilities: new List<Accessibility> { new("accessibility/access_need_1", "true") },
            status: SiteStatus.Online, isDeleted: false,
            Type: string.Empty),
        new(
            "0c06c137-2f8a-4334-a594-0632d0407966",
            "Site 3",
            "3 Park Row",
            "0113 1111111",
            "ABC03",
            "R1",
            "ICB1",
            location: new Location("Point", [.507, 65]),
            InformationForCitizens: "",
            Accessibilities: new List<Accessibility> { new("accessibility/access_need_1", "true") },
            status: SiteStatus.Offline, isDeleted: null,
            Type: string.Empty)
    ];

    public static Site CreateMockSite(string id, string name)
    {
        return new Site(
            id,
            name,
            "3 Park Row",
            "0113 1111111",
            "ABC03",
            "R1",
            "ICB1",
            location: new Location("Point", [.507, 65]),
            InformationForCitizens: "",
            Accessibilities: new List<Accessibility> { new("accessibility/access_need_1", "true") },
            status: SiteStatus.Offline, isDeleted: null,
            Type: string.Empty);
    }
}
