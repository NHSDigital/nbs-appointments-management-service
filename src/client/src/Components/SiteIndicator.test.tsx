import { act, screen } from "@testing-library/react";
import { SiteIndicator } from "./SiteIndicator";
import { wrappedRender } from "../utils";

describe("<SiteIndicator>", () => {
    it("Renders passed title prop correctly", async () => {
        wrappedRender(<SiteIndicator title="test" />);
        expect(screen.queryByText("test")).toBeVisible();
    });

    it("displays the site's name when present", async () => {
        wrappedRender(<SiteIndicator title="test"/>, {site: {id: "1", name: "Test Site", address: "Test Lane"}, siteConfig: null, saveSiteConfiguration: jest.fn(), hasPermission: () => true}, jest.fn());
        expect(screen.queryByText(/Test Site/)).toBeVisible();
    });
})
