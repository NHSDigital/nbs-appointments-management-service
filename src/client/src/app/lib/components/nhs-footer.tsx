import { Footer } from '@nhsuk-frontend-components';
import { getConfig } from '../utils/config';

const NhsFooter = async () => {
  const config = await getConfig();
  const buildNumberText = `Build number: ${config.BUILD_NUMBER}`;

  return (
    <Footer
      supportLinks={[
        {
          text: 'User guidance',
          href: 'http://www.digital.nhs.uk/services/vaccinations-national-booking-service/manage-your-appointments-guidance',
          target: '_blank',
        },
        {
          text: 'Terms of Use',
          href: 'https://digital.nhs.uk/services/vaccinations-national-booking-service/terms-of-use',
          target: '_blank',
        },
        {
          text: 'Privacy Policy',
          href: 'https://www.nhs.uk/our-policies/manage-your-appointments-privacy-policy/',
          target: '_blank',
        },
        {
          text: 'Cookies Policy',
          href: '/cookies-policy',
        },
      ]}
    >
      <span aria-hidden style={{ display: 'none' }}>
        {buildNumberText}
      </span>
    </Footer>
  );
};

export default NhsFooter;
