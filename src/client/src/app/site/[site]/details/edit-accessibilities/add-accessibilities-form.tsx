/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React, { useTransition } from 'react';
import {
  Button,
  FormGroup,
  CheckBoxes,
  CheckBox,
  ButtonGroup,
  SmallSpinnerWithText,
} from '@nhsuk-frontend-components';
import {
  AccessibilityDefinition,
  Accessibility,
  SetAccessibilitiesRequest,
} from '@types';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';
import { saveSiteAccessibilities } from '@services/appointmentsService';

type FormFields = {
  accessibilities: string[];
};

const AddAccessibilitiesForm = ({
  accessibilityDefinitions,
  site,
  accessibilities,
}: {
  accessibilityDefinitions: AccessibilityDefinition[];
  site: string;
  accessibilities: Accessibility[];
}) => {
  const [pendingSubmit, startTransition] = useTransition();
  const { replace } = useRouter();
  const { register, handleSubmit } = useForm<FormFields>({
    defaultValues: {
      accessibilities: accessibilities
        .filter(ac => ac.value === 'true')
        .map(ac => ac.id),
    },
  });

  const cancel = () => {
    replace(`/site/${site}/details`);
  };

  const submitForm: SubmitHandler<FormFields> = async (form: FormFields) => {
    startTransition(async () => {
      const payload: SetAccessibilitiesRequest = {
        accessibilities: accessibilityDefinitions.map(ad => ({
          id: ad.id,
          value:
            form.accessibilities.find((fv: string) => ad.id === fv) !==
            undefined
              ? 'true'
              : 'false',
        })),
      };
      await saveSiteAccessibilities(site, payload);

      replace(`/site/${site}/details`);
    });
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <FormGroup legend="Access needs">
        <div className="nhsuk-hint" id="example-hint">
          Select all options that the current site offers
        </div>
        <CheckBoxes>
          {accessibilityDefinitions.map(ad => (
            <CheckBox
              id={ad.id}
              label={ad.displayName}
              key={`checkbox-key-${ad.id}`}
              value={ad.id}
              {...register('accessibilities')}
            />
          ))}
        </CheckBoxes>
      </FormGroup>

      {pendingSubmit ? (
        <SmallSpinnerWithText text="Saving..." />
      ) : (
        <ButtonGroup>
          <Button type="submit">Confirm site details</Button>
          <Button styleType="secondary" onClick={cancel}>
            Cancel
          </Button>
        </ButtonGroup>
      )}
    </form>
  );
};

export default AddAccessibilitiesForm;
