import React, { useState, useEffect } from "react";
import { When } from "../Components/When";
import { useSiteContext } from "../ContextProviders/SiteContextProvider";
import { useUserService } from "src/Services/UserService";
import { User } from "../Types/User";
import { Table } from "src/Components/Table";

type UserManagementProps = {
  siteId: string;
  getUsersForSite: () => Promise<User[]>;
};

export const UserManagementCtx = () => {
  const { site } = useSiteContext();
  const userService = useUserService();

  return (
    <UserManagement
      siteId={site!.id}
      getUsersForSite={() => userService.getUsersForSite(site!.id)}
    />
  );
};

export const UserManagement = ({
  siteId,
  getUsersForSite,
}: UserManagementProps) => {
  const [users, setUsers] = useState<User[]>([]);
  const [status, setStatus] = useState<null | "loading" | "errored" | "ready">(
    null
  );

  useEffect(() => {
    if (siteId) {
      setStatus("loading");
      getUsersForSite().then((users) => {
        console.dir(users);
        setUsers(users);
      });
      setStatus("ready");
    }
  }, [siteId]);

  return (
    <div className="nhsuk-grid-row">
      <div className="nhsuk-grid-column-one-half">
        <When condition={status === "loading"}>
          <div>Loading...</div>
        </When>
        <When condition={status === "errored"}>
          <div
            className="nhsuk-error-summary"
            aria-labelledby="error-summary-title"
            role="alert"
            tabIndex={-1}
          >
            <h2 className="nhsuk-error-summary__title" id="error-summary-title">
              <span className="nhsuk-u-visually-hidden">Error:</span>
              There is a problem
            </h2>
            <div className="nhsuk-error-summary__body">
              {/* <p>There has been a server error, please try again</p> */}
              <ul className="nhsuk-list nhsuk-error-summary__list">
                <li>
                  <a href="#!">Something went wrong.</a>
                </li>
              </ul>
            </div>
          </div>
        </When>
        <When condition={status === "ready"}>
          <Table
            caption={`Manage your current site's staff roles`}
            headers={["Email", "Roles"]}
            rows={users.map((user) => {
              return [
                user.id,
                user.roleAssignments?.map((ra) => ra.role)?.join(" | "),
              ];
            })}
          />
        </When>
      </div>
    </div>
  );
};
