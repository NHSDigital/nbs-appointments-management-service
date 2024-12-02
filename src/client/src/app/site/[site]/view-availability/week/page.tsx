import NhsPage from '@components/nhs-page';
import dayjs from 'dayjs';

// TODO: Include site in props
type PageProps = {
  searchParams: {
    // TODO: Make this a single date and use dayjs' isoWeek to get the start & end of the week and all the days in that week
    from: string;
    until: string;
  };
};

const Page = async ({ searchParams }: PageProps) => {
  // TODO: Add permission check on site for 'availability:query' - Make it a required param for <NhsPage ... />?

  return (
    <NhsPage
      title={`${dayjs(searchParams.from).format('D MMMM')} to ${dayjs(searchParams.until).format('D MMMM')}`}
      // TODO: Update breadcrumbs to include site & view availabitly
      breadcrumbs={[{ name: 'Home', href: '/' }]}
    >
      <p>View weekly availability page.</p>
    </NhsPage>
  );
};

export default Page;
