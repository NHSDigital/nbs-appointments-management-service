import NhsTransactionalPage from '@components/nhs-transactional-page';
import ReportsTemplateWizard from './reports-template-wizard';
import { ReportType } from './reports-template-wizard';

type PageProps = {
  params: Promise<{
    reportType: ReportType;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { reportType } = { ...(await params) };

  return (
    <NhsTransactionalPage originPage="reports-wizard">
      <ReportsTemplateWizard reportType={reportType} />
    </NhsTransactionalPage>
  );
};

export default Page;
