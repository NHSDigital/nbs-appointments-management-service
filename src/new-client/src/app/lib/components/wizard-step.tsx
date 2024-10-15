'use client';
import React, { ReactNode, useRef } from 'react';
import { InjectedWizardProps } from './wizard';

export interface WizardStepProps {
  children: ((props: InjectedWizardProps) => ReactNode) | ReactNode;
  onBack?: () => void;
  id?: string;
}

const WizardStep = ({ children, id, ...rest }: WizardStepProps) => {
  const ref = useRef<HTMLLIElement>(null);
  const injectedWizardProps = rest as InjectedWizardProps;
  const { stepNumber, currentStep } = injectedWizardProps;

  return (
    <li
      id={id}
      ref={ref}
      tabIndex={-1}
      hidden={stepNumber > currentStep}
      style={{ listStyle: 'none', paddingLeft: 0 }}
    >
      <div>
        {typeof children === 'function'
          ? children(injectedWizardProps)
          : children}
      </div>
    </li>
  );
};

export default WizardStep;
