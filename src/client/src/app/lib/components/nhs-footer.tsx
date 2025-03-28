import { Footer } from '@nhsuk-frontend-components';
import { fetchFeatureFlag } from '@services/appointmentsService';

type SupportLink = {
  text: string;
  href: string;
  target?: string;
};

const NhsFooter = async () => {
  const buildNumberText = `Build number: ${process.env.BUILD_NUMBER}`;
  const accessibilityFlag = await fetchFeatureFlag('AccessibilityStatement');

  const supportLinks: SupportLink[] = [
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
  ];

  if (accessibilityFlag.enabled) {
    supportLinks.push({
      text: 'Accessibility Statement',
      href: 'https://www.nhs.uk/our-policies/manage-your-appointment-accessibility-statement',
      target: '_blank',
    });
  }

  return (
    <Footer supportLinks={supportLinks}>
      <span aria-hidden style={{ display: 'none' }}>
        {buildNumberText}
      </span>
    </Footer>
  );
};

export default NhsFooter;
