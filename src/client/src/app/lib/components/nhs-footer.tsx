import { Footer } from '@nhsuk-frontend-components';
import { ReactNode } from 'react';

type NhsFooterProps = {
  buildNumber: ReactNode;
};

const NhsFooter = ({ buildNumber }: NhsFooterProps) => {
  return (
    <Footer
      supportLinks={[
        {
          text: 'User guidance',
          href: 'http://www.digital.nhs.uk/services/vaccinations-national-booking-service/manage-your-appointments-guidance',
          target: '_blank',
          internal: false,
        },
        {
          text: 'Terms of Use',
          href: 'https://digital.nhs.uk/services/vaccinations-national-booking-service/terms-of-use',
          target: '_blank',
          internal: false,
        },
        {
          text: 'Privacy Policy',
          href: 'https://www.nhs.uk/our-policies/manage-your-appointments-privacy-policy/',
          target: '_blank',
          internal: false,
        },
        {
          text: 'Cookies Policy',
          href: '/cookies-policy',
          internal: true,
        },
        {
          text: 'Accessibility Statement',
          href: 'https://www.nhs.uk/our-policies/manage-your-appointment-accessibility-statement',
          target: '_blank',
          internal: false,
        },
      ]}
    >
      <span aria-hidden style={{ display: 'none' }}>
        {buildNumber}
      </span>
    </Footer>
  );
};

export default NhsFooter;
