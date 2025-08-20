import Link from 'next/link';
import { HTMLAttributeAnchorTarget, ReactNode } from 'react';

type supportLink = {
  text: string;
  href: string;
  target?: HTMLAttributeAnchorTarget;
  internal: boolean;
};

type FooterProps = {
  supportLinks?: supportLink[];
  children?: ReactNode;
};

/**
 * A footer component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/footer
 */
const Footer = ({ supportLinks = [], children }: FooterProps) => {
  return (
    <footer role="contentinfo">
      <div className="nhsuk-footer-container">
        <div className="nhsuk-width-container">
          <h2 className="nhsuk-u-visually-hidden">Support links</h2>
          <div className="nhsuk-footer">
            <ul className="nhsuk-footer__list">
              {supportLinks.map((link, index) => (
                <li
                  key={`support-link-${index}`}
                  className="nhsuk-footer__list-item nhsuk-footer-default__list-item"
                >
                  <Link
                    className="nhsuk-footer__list-item-link"
                    href={link.href}
                    target={link.target ?? '_self'}
                    rel="noopener noreferrer"
                    prefetch={link.internal}
                  >
                    {link.text}
                  </Link>
                </li>
              ))}
            </ul>
            <div>
              <p className="nhsuk-footer__copyright">© NHS England</p>
            </div>
            <div>{children}</div>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
