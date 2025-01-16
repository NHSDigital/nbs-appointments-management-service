/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React from 'react';
import {
  Button,
  FormGroup,
  ButtonGroup,
  SmallSpinnerWithText,
  TextInput,
} from '@nhsuk-frontend-components';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';
import { SetSiteDetailsRequest, SiteWithAttributes } from '@types';
import { saveSiteDetails } from '@services/appointmentsService';

type FormFields = {
  name: string;
  address: string;
  phoneNumber: string;
  latitude: string;
  longitude: string;
};

const EditDetailsForm = ({
  siteWithAttributes,
}: {
  siteWithAttributes: SiteWithAttributes;
}) => {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting, isSubmitSuccessful },
  } = useForm<FormFields>({
    defaultValues: {
      name: siteWithAttributes.name,
      address: siteWithAttributes.address,
      phoneNumber: siteWithAttributes.phoneNumber,
      //TODO deciml controls?
      latitude: siteWithAttributes.location.coordinates[0].toString(),
      longitude: siteWithAttributes.location.coordinates[1].toString(),
    },
  });

  const { replace } = useRouter();

  const cancel = () => {
    replace(`/site/${siteWithAttributes.id}/details`);
  };

  const submitForm: SubmitHandler<FormFields> = async (form: FormFields) => {
    const payload: SetSiteDetailsRequest = {
      name: form.name,
      address: form.address,
      phoneNumber: form.phoneNumber,
      latitude: form.latitude,
      longitude: form.longitude,
    };
    await saveSiteDetails(siteWithAttributes.id, payload);

    replace(`/site/${siteWithAttributes.id}/details`);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      {/* <h3>Site name</h3> */}
      <FormGroup>
        <TextInput id="name" label="Name" {...register('name')}></TextInput>
        <TextInput
          id="address"
          label="Address"
          {...register('address')}
        ></TextInput>
        <TextInput
          id="phoneNumber"
          label="Phone number"
          {...register('phoneNumber')}
        ></TextInput>
      </FormGroup>

      {/* <h3>Site address</h3> */}

      {/* <FormGroup>
        <TextInput id="line1" label="Address line 1"></TextInput>
        <TextInput id="line2" label="Address line 2"></TextInput>
        <TextInput id="townCity" label="Town or city"></TextInput>
        <TextInput id="county" label="County (optional)"></TextInput>
        <TextInput id="postcode" label="Postcode"></TextInput>
      </FormGroup> */}
      <FormGroup>
        <TextInput
          id="latitude"
          label="Latitude"
          {...register('latitude')}
        ></TextInput>
        <TextInput
          id="longitude"
          label="Longitude"
          {...register('longitude')}
        ></TextInput>
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
  );
};

export default EditDetailsForm;
