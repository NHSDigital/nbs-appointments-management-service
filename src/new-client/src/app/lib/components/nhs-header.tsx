import { Header } from '@nhsuk-frontend-components';
import { ReactNode } from 'react';

type NhsHeaderProps = {
  children?: ReactNode;
};

export const NhsHeader = ({ children }: NhsHeaderProps) => {
  return <Header>{children}</Header>;
};
