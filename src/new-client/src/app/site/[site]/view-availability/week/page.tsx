import NhsPage from '@components/nhs-page';
import dayjs from 'dayjs';

type PageProps = {
  searchParams: {
    from: string;
    until: string;
  };
};

const Page = async ({ searchParams }: PageProps) => {
  return (
    <NhsPage
      title={`${dayjs(searchParams.from).format('D MMMM')} to ${dayjs(searchParams.until).format('D MMMM')}`}
      breadcrumbs={[{ name: 'Home', href: '/' }]}
    >
      <p>View weekly availability page.</p>
    </NhsPage>
  );
};

export default Page;
