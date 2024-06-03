import { screen } from "@testing-library/react";
import { ServiceSelector } from "./ServiceSelector";
import { wrappedRender } from "../utils";
import { ISiteContext, SiteContext } from "../ContextProviders/SiteContextProvider";


const context: ISiteContext = {    
    site: {id: "1", name: "Test Site", address: "Test Lane"},
    siteConfig: {
        siteId: "1", informationForCitizen: "", serviceConfiguration: [
            { code: "service1", displayName: "Service 1", duration: 10, enabled: true },
            { code: "service2", displayName: "Service 2", duration: 10, enabled: false }
        ]
    },
    saveSiteConfiguration: jest.fn()
}
describe("<SiteSelector>", () => {
    it("shows enabled services and filters disabled services from the list", async () => {
        wrappedRender(
            <ServiceSelector checkedServices={[]} uniqueId="" handleChange={jest.fn} hasError={false} handleSelectAllChange={jest.fn} />,
            context
        );
        const presentOption = await screen.findByLabelText(context.siteConfig?.serviceConfiguration[0].displayName!);
        const missingOption = await screen.queryByText(context.siteConfig?.serviceConfiguration[1].displayName!);
        expect(presentOption).toBeVisible();
        expect(missingOption).toBeInTheDocument();
    });
})