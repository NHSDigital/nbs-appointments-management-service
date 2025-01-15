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

export const EditDetailsPage = async ({ site }: Props) => {
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
          <div className="nhsuk-hint">Edit site details</div>
        </div>

        <form onSubmit={handleSubmit(submitForm)}>
          <h3>Site name</h3>
          <FormGroup>
            <TextInput id="email"></TextInput>
          </FormGroup>
          <h3>Site address</h3>
          <FormGroup>
            <TextInput id="line1" label="Address line 1"></TextInput>
            <TextInput id="line2" label="Address line 2"></TextInput>
            <TextInput id="townCity" label="Town or city"></TextInput>
            <TextInput id="county" label="County (optional)"></TextInput>
            <TextInput id="postcode" label="Postcode"></TextInput>
          </FormGroup>

          <h3>Latitude and longitude</h3>
          <FormGroup>
            <TextInput id="latitude" label="Latitude"></TextInput>
            <TextInput id="longitude" label="Longitude"></TextInput>
          </FormGroup>

          {isSubmitting || isSubmitSuccessful ? (
            <SmallSpinnerWithText text="Updating details..." />
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
