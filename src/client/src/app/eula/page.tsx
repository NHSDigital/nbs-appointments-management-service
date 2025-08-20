import NhsAnonymousPage from '@components/nhs-anonymous-page';
import { fetchEula } from '@services/appointmentsService';
import Link from 'next/link';
import { AcceptEulaForm } from './accept-eula-form';

const Page = async () => {
  const latestVersion = await fetchEula();

  return (
    <NhsAnonymousPage title="Agree to the terms of use" originPage="eula">
      <p>You must:</p>
      <ul>
        <li>
          only use Manage Your Appointments to set-up and administer vaccination
          appointments
        </li>
        <li>always use your own details to login</li>
        <li>never allow anyone else to use your login details</li>
      </ul>

      <p>
        <Link
          href="https://digital.nhs.uk/services/vaccinations-national-booking-service/terms-of-use"
          target="_blank"
          prefetch={false}
        >
          Read the full terms of use for Manage Your Appointments
        </Link>
        . If you click accept and continue, you are agreeing to the full terms
        of use for this service.
      </p>

      <AcceptEulaForm eulaVersion={latestVersion} />
    </NhsAnonymousPage>
  );
};

export default Page;
