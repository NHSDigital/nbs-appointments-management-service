import NhsAnonymousPage from '@components/nhs-anonymous-page';
import { BackLink, InsetText } from '@components/nhsuk-frontend';
import { fetchEula } from '@services/appointmentsService';

const Page = async () => {
  const eula = await fetchEula();

  return (
    <NhsAnonymousPage title="End User Licence Agreement">
      <BackLink href={'/eula'} renderingStrategy="server" />
      <InsetText>{eula.content}</InsetText>
    </NhsAnonymousPage>
  );
};

export default Page;
