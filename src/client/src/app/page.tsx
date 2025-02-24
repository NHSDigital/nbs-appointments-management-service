import { redirect } from 'next/navigation';

// TODO: Remove this page. The redirects configured in config don't seem to redirect away from source: "/" unless there is a page there to redirect from.
const Page = async () => {
  redirect('/sites');
};

export default Page;
