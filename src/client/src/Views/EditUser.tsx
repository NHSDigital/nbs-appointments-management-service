import React from "react";
import {Role} from "../Types/Permissions";
import Link from "next/link";
import {useUserService} from "../Services/UserService";
import {useSiteContext} from "../ContextProviders/SiteContextProvider";

export const EditUserCtx = () => {
    return ( <EditUser />)
}
export const EditUser = () => {
    const { site } = useSiteContext();
    const { setUserRoles } = useUserService();
    const [user, setUser] = React.useState("");
    
    const [roles, setRoles] = React.useState<Role[]>([
        {displayName: "Site Admin", id: "canned:site-admin"},
        {displayName: "Availability Manager", id: "canned:availability-manager"},
    ]);
    
    const [selectedRoles, setSelectedRoles] = React.useState<string[]>([] as string[]);
    
    const handleOnChange = (roleId: string) => {
        selectedRoles?.includes(roleId) ?
            setSelectedRoles( selectedRoles?.filter(r => r != roleId) ) :
            setSelectedRoles( [...selectedRoles, roleId] );
    }
    
    const saveUser = () => {
        setUserRoles(site!.id, user, selectedRoles).then(r => r);
    }
    
    return (
        <div>
            <div>
                <h3>Create new user</h3>
                <div>Set the details and roles of a new user</div>
                <label className="nhsuk-label">
                    Email
                </label>
                <input 
                    type="text" 
                    className="nhsuk-input nhsuk-input--width-20" 
                    value={user} 
                    onChange={e => setUser(e.target.value)}/>
                <h3>
                    Roles
                </h3>
                {roles.map(r => (
                    <div>
                        <label>
                            <input 
                                type="checkbox"
                                id={r.id}
                                checked={selectedRoles?.includes(r.id)}
                                onChange={() => handleOnChange(r.id)}
                            />
                            <span>{r.displayName}</span>
                        </label>
                    </div>
                ))}
                <div className="nhsuk-navigation">
                    <button
                        id="submit-schedule"
                        type="button"
                        className="nhsuk-button nhsuk-u-margin-bottom-0"
                        onClick={saveUser}
                    >
                        Confirm and create user
                    </button>
                    <Link href="/" className="nhsuk-button nhsuk-button--secondary nhsuk-u-margin-left-3 nhsuk-u-margin-bottom-0">
                        Cancel
                    </Link>
                </div>
            </div>
        </div>
    )
}