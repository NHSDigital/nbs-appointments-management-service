/* eslint-disable react/jsx-props-no-spreading */
'use client';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import React from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { EMAIL_REGEX } from '../../../../../constants';

type FormFields = {
  email: string;
};

const FindUserForm = ({ site }: { site: string }) => {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const { replace } = useRouter();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>({
    defaultValues: { email: '' },
  });

  const submitForm: SubmitHandler<FormFields> = form => {
    const params = new URLSearchParams(searchParams);
    if (form.email) {
      params.set('user', form.email);
    } else {
      params.delete('user');
    }
    replace(`${pathname}?${params.toString()}`);
  };

  const cancel = () => {
    replace(`/site/${site}/users`);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <div
        className={`nhsuk-form-group ${errors.email ? 'nhsuk-form-group--error' : ''}`}
      >
        <div className="nhsuk-form-group">
          {errors.email && (
            <span className="nhsuk-error-message">
              <span className="nhsuk-u-visually-hidden">Error:</span> You have
              not entered a valid nhs email address
            </span>
          )}
          <label htmlFor="email" className="nhsuk-label">
            Email
          </label>
          <input
            id="email"
            className="nhsuk-input nhsuk-input--width-20"
            type="text"
            {...register('email', {
              required: true,
              pattern: EMAIL_REGEX,
            })}
          />
        </div>

        <div className="nhsuk-navigation">
          <button
            type="submit"
            aria-label="save user"
            className="nhsuk-button nhsuk-u-margin-bottom-0"
          >
            Search user
          </button>
          <button
            type="button"
            aria-label="cancel"
            className="nhsuk-button nhsuk-button--secondary nhsuk-u-margin-left-3 nhsuk-u-margin-bottom-0"
            onClick={cancel}
          >
            Cancel
          </button>
        </div>
      </div>
    </form>
  );
};

export default FindUserForm;
