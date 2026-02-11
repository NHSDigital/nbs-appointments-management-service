import { ReactNode } from 'react';
import { Footer } from 'nhsuk-react-components';

type NhsFooterProps = {
  buildNumber?: ReactNode;
};

const NhsFooter = ({ buildNumber }: NhsFooterProps) => {
  return (
    <Footer>
      <Footer.Meta>
        <Footer.ListItem
          href="http://www.digital.nhs.uk/services/vaccinations-national-booking-service/manage-your-appointments-guidance"
          target="_blank"
        >
          User guidance
        </Footer.ListItem>
        <Footer.ListItem
          href="https://digital.nhs.uk/services/vaccinations-national-booking-service/terms-of-use"
          target="_blank"
        >
          Terms of use
        </Footer.ListItem>
        <Footer.ListItem
          href="https://www.nhs.uk/our-policies/manage-your-appointments-privacy-policy/"
          target="_blank"
        >
          Privacy policy
        </Footer.ListItem>
        <Footer.ListItem
          href={`${process.env.CLIENT_BASE_PATH}/cookies-policy`}
        >
          Cookies policy
        </Footer.ListItem>
        <Footer.ListItem
          href="https://www.nhs.uk/our-policies/manage-your-appointment-accessibility-statement"
          target="_blank"
        >
          Accessibility statement
        </Footer.ListItem>
        <span aria-hidden style={{ display: 'none' }}>
          {buildNumber}
        </span>
      </Footer.Meta>
    </Footer>
  );
};

export default NhsFooter;
