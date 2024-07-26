import { render, screen, waitFor } from "@testing-library/react";
import { UserManagement } from "./UserManagement";
import { User } from "src/Types/User";

describe("UserManagement", () => {
  it("renders", () => {
    render(
      <UserManagement
        siteId={mockSiteId}
        getUsersForSite={mockGetUsersForSite}
      />
    );

    expect(
      screen.getByRole("table", {
        name: "Manage your current site's staff roles",
      })
    ).toBeInTheDocument();
  });

  it("displays each user in the table", async () => {
    render(
      <UserManagement
        siteId={mockSiteId}
        getUsersForSite={mockGetUsersForSite}
      />
    );

    screen.debug();
    await waitFor(() => {
      expect(screen.getAllByRole("row").length).toBe(3);
    });

    expect(
      screen.getByRole("cell", { name: "test.one@nhs.net" })
    ).toBeInTheDocument();
    expect(
      screen.getByRole("cell", {
        name: "canned:api-user | canned:site-configuration-manager",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("cell", { name: "test.two@nhs.net" })
    ).toBeInTheDocument();
    expect(
      screen.getByRole("cell", { name: "canned:api-user" })
    ).toBeInTheDocument();
  });

  // TODO: Add more robust tests for the unhappy path, loading states and error handling AFTER we agree a pattern moving forward
});

const mockSiteId = "1000";
const mockGetUsersForSite: (siteId: string) => Promise<User[]> = (
  siteId: string
): Promise<User[]> => {
  return Promise.resolve([
    {
      id: "test.one@nhs.net",
      roleAssignments: [
        { role: "canned:api-user", scope: `site:${siteId}` },
        { role: "canned:site-configuration-manager", scope: `site:${siteId}` },
      ],
    },
    {
      id: "test.two@nhs.net",
      roleAssignments: [{ role: "canned:api-user", scope: `site:${siteId}` }],
    },
  ]);
};
