import { render, screen } from "@testing-library/react"
import { SelectSite } from "./SelectSite"
import { Site } from "../Types/Site"
import userEvent from "@testing-library/user-event"
import { wrappedRender } from "../utils"


describe("<SiteSelector>", () => {
    it("shows available sites for user", async () => {
        var sites = [
            {id: "1000", name: "Site Alpha", address: "somewhere"},
            {id: "1001", name: "Site Beta", address: "elsewhere"}
        ] as Site[]
        wrappedRender(<SelectSite sites={sites} selectSite={() => {}} />);
        const siteAlphaLink = await screen.findByRole("link", {name: "Site Alpha"});
        const siteBetaLink = await screen.findByRole("link", {name: "Site Beta"});
        expect(siteAlphaLink).toBeVisible();
        expect(siteBetaLink).toBeVisible();
    })

    it("allows user to select a site", async () => {
        var selectSiteFunc = jest.fn();
        var sites = [
            {id: "1000", name: "Site Alpha", address: "somewhere"},
            {id: "1001", name: "Site Beta", address: "elsewhere"}
        ] as Site[]
        wrappedRender(<SelectSite sites={sites} selectSite={selectSiteFunc} />);
        const siteAlphaLink = await screen.findByRole("link", {name: "Site Alpha"});
        const siteBetaLink = await screen.findByRole("link", {name: "Site Beta"});
        userEvent.click(siteAlphaLink);
        expect(selectSiteFunc).toHaveBeenCalledWith({"address": "somewhere", "id": "1000", "name": "Site Alpha"});
    })
})