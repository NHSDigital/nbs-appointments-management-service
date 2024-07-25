import { When } from "@/app/components/when";
import UserInput from "./user-input"
import AssignRoles from "./assign-roles";

export type UserPageProps = {
    params: {
        site: string
    }
    searchParams?: {
        user?: string;
      }
}

const Page = ({params, searchParams} : UserPageProps) => {

    const userIsSpecified = () => 'user' in searchParams!;

    return (
        <>
        <h2 className="text-4xl font-extrabold dark:text-white">User Management</h2>
        <When condition={userIsSpecified()}>
            <AssignRoles params={params} searchParams={searchParams} />
        </When>
        <When condition={!userIsSpecified()}>
            <UserInput />
        </When>
        </>
    )
}

export default Page