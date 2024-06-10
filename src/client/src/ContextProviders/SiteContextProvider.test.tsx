import { render, screen } from "@testing-library/react"
import { SiteContextProvider } from "./SiteContextProvider"
import { useSiteConfigurationService } from "../Services/SiteConfigurationService";
import { useAuthContext } from "./AuthContextProvider";
import { Site, ServiceConfiguration, SiteConfiguration } from "../Types";

jest.mock("../Services/SiteConfigurationService")
const useSiteConfigurationServiceMock = useSiteConfigurationService as jest.Mock<any>

jest.mock("./AuthContextProvider")
const useAuthContextMock = useAuthContext as jest.Mock<any>

jest.mock("../Components/SelectSite", () => {
    return {
        SelectSite: () => { return <div>Select Site</div>}
    }
})

jest.mock("../Components/SiteSetup", () => {
    return {
        SiteSetup: () => { return <div>Site Setup</div>}
    }
})

describe("<SiteContextProvider>", () => {
    it("render loading message when auth has not completed", async () => {
        useAuthContextMock.mockReturnValue({idToken: null})
        useSiteConfigurationServiceMock.mockReturnValue({getSiteConfiguration: jest.fn(), getSitesForUser: jest.fn(), setSiteConfiguration: jest.fn()})
        render(<SiteContextProvider><div>TEST CONTENT</div></SiteContextProvider>)        
        expect(await screen.findByText("Loading Configuration...")).toBeVisible()
    })

    it("gets sites for user after signin complete", async () => {
        const sites : Site[] = [
            {id: "1000", name: "Test", address: "somewhere"}
        ]
        const getSitesForUserMock = jest.fn();
        getSitesForUserMock.mockResolvedValue(sites);
        useAuthContextMock.mockReturnValue({idToken: "a_token"})
        useSiteConfigurationServiceMock.mockReturnValue({getSiteConfiguration: jest.fn(), getSitesForUser: getSitesForUserMock, setSiteConfiguration: jest.fn()})
        render(<SiteContextProvider><div>TEST CONTENT</div></SiteContextProvider>)        
        expect(getSitesForUserMock).toHaveBeenCalled();
    })

    it("loads main content when a single site is available", async () => {
        const sites : Site[] = [
            {id: "1000", name: "Test", address: "somewhere"}
        ]
        const getSitesForUserMock = jest.fn();
        getSitesForUserMock.mockResolvedValue(sites);

        const siteConfig : SiteConfiguration = {
            siteId: "1000",
            informationForCitizen: "",
            serviceConfiguration: [] as ServiceConfiguration[]
        }
        const getSiteConfigurationMock = jest.fn();
        getSiteConfigurationMock.mockResolvedValue(siteConfig)
        useAuthContextMock.mockReturnValue({idToken: "a_token"})
        useSiteConfigurationServiceMock.mockReturnValue({getSiteConfiguration: getSiteConfigurationMock, getSitesForUser: getSitesForUserMock, setSiteConfiguration: jest.fn()})
        render(<SiteContextProvider><div>TEST CONTENT</div></SiteContextProvider>)
        expect(await screen.findByText("TEST CONTENT")).toBeVisible()
        expect(getSiteConfigurationMock).toHaveBeenCalled();
    })

    it("loads site selector when multiple sites are available", async () => {
        const sites : Site[] = [
            {id: "1000", name: "Site Alpha", address: "somewhere"},
            {id: "1001", name: "Site Beta", address: "elsewhere"}
        ]
        const getSitesForUserMock = jest.fn();
        getSitesForUserMock.mockResolvedValue(sites);

        useAuthContextMock.mockReturnValue({idToken: "a_token"})
        useSiteConfigurationServiceMock.mockReturnValue({getSiteConfiguration: jest.fn(), getSitesForUser: getSitesForUserMock, setSiteConfiguration: jest.fn()})
        render(<SiteContextProvider><div>TEST CONTENT</div></SiteContextProvider>)
        expect(await screen.findByText("Select Site")).toBeVisible()        
    })

    it("loads site setup when site has no site config", async () => {
        const sites : Site[] = [
            {id: "1000", name: "Test", address: "somewhere"}
        ]
        const getSitesForUserMock = jest.fn();
        getSitesForUserMock.mockResolvedValue(sites);

        const getSiteConfigurationMock = jest.fn();
        getSiteConfigurationMock.mockResolvedValue(null)
        useAuthContextMock.mockReturnValue({idToken: "a_token"})
        useSiteConfigurationServiceMock.mockReturnValue({getSiteConfiguration: getSiteConfigurationMock, getSitesForUser: getSitesForUserMock, setSiteConfiguration: jest.fn()})
        render(<SiteContextProvider><div>TEST CONTENT</div></SiteContextProvider>)
        expect(await screen.findByText("Site Setup")).toBeVisible()
        expect(getSiteConfigurationMock).toHaveBeenCalled();
    })

    it("shows warning message when no sites found for user", async () => {
        const sites : Site[] = [] as Site[];            
        const getSitesForUserMock = jest.fn();
        getSitesForUserMock.mockResolvedValue(sites);
        useAuthContextMock.mockReturnValue({idToken: "a_token"})
        useSiteConfigurationServiceMock.mockReturnValue({getSiteConfiguration: jest.fn(), getSitesForUser: getSitesForUserMock, setSiteConfiguration: jest.fn()})
        render(<SiteContextProvider><div>TEST CONTENT</div></SiteContextProvider>)
        expect(await screen.findByText("We were unable to find your site. It may not be registered with our systems. You will need to contact our onboarding team.")).toBeVisible()
    })
})