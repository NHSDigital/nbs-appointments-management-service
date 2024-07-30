import Link from "next/link";
import { fetchSites } from "./lib/sitesService";

export default async function Home() {

  const sites = await fetchSites();

  return (
    <>
      <div>Welcome to Appointment Service</div>
      <ul className="text-sm font-medium text-gray-900 bg-white border border-gray-200 rounded-lg dark:bg-gray-700 dark:border-gray-600 dark:text-white">
        {sites.map(s => (
            <li key={s.id} className="w-full border-b border-gray-200 rounded-t-lg dark:border-gray-600">
                <Link href={`site/${s.id}/users`} className="text-sm font-medium text-gray-900 truncate dark:text-white">
                     {s.name}
                  </Link>
            </li>
        ))}
        </ul>
    </>
  );
}
