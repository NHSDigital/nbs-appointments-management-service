/* eslint-disable react/jsx-props-no-spreading */
'use client';
import { useRouter } from 'next/navigation';
import React from 'react';
import {
  fetchAttributeDefinitions,
  fetchSite,
} from '@services/appointmentsService';
import AddAttributesForm from '../edit-attributes/add-attributes-form';
import {
  Button,
  ButtonGroup,
  FormGroup,
  SmallSpinnerWithText,
  TextInput,
} from '@components/nhsuk-frontend';
import { SubmitHandler, useForm } from 'react-hook-form';

type Props = {
  site: string;
  permissions: string[];
};

export const EditCodesPage = async ({ site }: Props) => {
  // const siteDetails = await fetchSite(site);

  type FormFields = {
    email: string;
  };

  const {
    handleSubmit,
    formState: { errors, isSubmitting, isSubmitSuccessful },
  } = useForm<FormFields>({
    defaultValues: { email: '' },
  });

  const { replace } = useRouter();

  const cancel = () => {
    replace(`/site/${site}/details`);
  };

  const submitForm: SubmitHandler<FormFields> = () => {};

  return (
    <div className="nhsuk-grid-row">
      <div className="nhsuk-grid-column-one-half">
        <div className="nhsuk-form-group">
          <div className="nhsuk-hint">Edit code details</div>
        </div>

        <form onSubmit={handleSubmit(submitForm)}>
          <FormGroup>
            <TextInput id="ods" label="ODS code"></TextInput>
            <TextInput id="icb" label="ICB"></TextInput>
            <TextInput id="region" label="Region"></TextInput>
          </FormGroup>

          {isSubmitting || isSubmitSuccessful ? (
            <SmallSpinnerWithText text="Updating codes..." />
          ) : (
            <ButtonGroup>
              <Button type="submit">Save and continue</Button>
              <Button styleType="secondary" onClick={cancel}>
                Cancel
              </Button>
            </ButtonGroup>
          )}
        </form>
      </div>
    </div>
  );
};
