import { Footer } from '@nhsuk-frontend-components';

const NhsFooter = () => {
  return (
    <Footer
      supportLinks={[
        {
          text: 'User guidance',
          href: 'http://www.digital.nhs.uk/services/vaccinations-national-booking-service/manage-your-appointments-guidance',
        },
        {
          text: 'Terms of Use',
          href: 'https://digital.nhs.uk/services/vaccinations-national-booking-service/terms-of-use',
        },
        {
          text: 'Privacy Policy',
          href: 'https://www.nhs.uk/our-policies/manage-your-appointments-privacy-policy/',
        },
        {
          text: 'Cookies Policy',
          href: 'cookies-policy',
        },
      ]}
    />
  );
};

export default NhsFooter;
