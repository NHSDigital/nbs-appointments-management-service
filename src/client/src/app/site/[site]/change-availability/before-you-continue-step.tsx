'use client';
import NhsHeading from '@components/nhs-heading';
import { BackLink } from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { useRouter } from 'next/navigation';

const BeforeYouContinueStep = ({}: InjectedWizardProps) => {
  const router = useRouter();

  return (
    <>
      <BackLink
        onClick={() => router.back()}
        renderingStrategy="client"
        text="Back"
      />

      <NhsHeading title="Before you continue" />
    </>
  );
};

export default BeforeYouContinueStep;
