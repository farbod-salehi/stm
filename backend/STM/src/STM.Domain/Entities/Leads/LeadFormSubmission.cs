using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace STM.Domain.Entities.Leads
{
    public sealed class LeadFormSubmission
    {
        private const int FullNameMaxLength = 200;

        private const int EmailMaxLength = 320;

        private const int PhoneMaxLength = 50;

        private LeadFormSubmission() { /* EF Core construction */ }

        public LeadFormSubmission(Guid leadFormId, string fullName, string? email, string? phone, string answersJson)
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateLeadFormId(leadFormId));
            errors.AddRange(ValidateFullName(fullName));
            errors.AddRange(ValidateEmail(email));
            errors.AddRange(ValidatePhone(phone));
            errors.AddRange(ValidateAnswersJson(answersJson));
            
            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            Id = Guid.NewGuid();
            LeadFormId = leadFormId;
            SetFullName(fullName);
            SetEmail(email);
            SetPhone(phone);
            SetAnswersJson(answersJson);
            SubmittedAt = DateTimeOffset.UtcNow;
        }

        private static List<DomainError> ValidateLeadFormId(Guid leadFormId)
        {
            List<DomainError> errors = [];

            if (leadFormId == Guid.Empty)
                errors.Add(new DomainError(DomainErrors.LeadFormSubmission.LeadFormIdIsRequired));

            return errors;
        }

        private static List<DomainError> ValidateFullName(string fullName)
        {
            List<DomainError> errors = [];

            if(string.IsNullOrWhiteSpace(fullName))
                errors.Add(new DomainError(DomainErrors.LeadFormSubmission.FullNameIsRequired));
            else if (fullName.Trim().Length > FullNameMaxLength)
                errors.Add(new DomainError(DomainErrors.LeadFormSubmission.FullNameIsTooLong));

            return errors;
        }

        private static List<DomainError> ValidateEmail(string? email)
        {
            List<DomainError> errors = [];

            if (!string.IsNullOrWhiteSpace(email))
            {
                if(email.Trim().Length > EmailMaxLength)
                    errors.Add(new DomainError(DomainErrors.LeadFormSubmission.EmailIsTooLong));
                else
                {
                    try
                    {
                        _ = new MailAddress(email.Trim());
                    }
                    catch
                    {
                        errors.Add(new DomainError(DomainErrors.LeadFormSubmission.EmailInvalidFormat));
                    }
                }
            }
                

            return errors;
        }

        private static List<DomainError> ValidatePhone(string? phone)
        {
            List<DomainError> errors = [];

            if (!string.IsNullOrWhiteSpace(phone) && phone.Trim().Length > PhoneMaxLength)
                errors.Add(new DomainError(DomainErrors.LeadFormSubmission.PhoneIsTooLong));

            return errors;
        }

        private static List<DomainError> ValidateAnswersJson(string answersJson)
        {
            List<DomainError> errors = [];

            if (string.IsNullOrWhiteSpace(answersJson))
                errors.Add(new DomainError(DomainErrors.LeadFormSubmission.AnswersJsonIsRequired));

            return errors;
        }

        private void SetFullName(string fullName)
        {
            FullName = fullName.Trim();
        }

        private void SetEmail(string? email)
        {
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        }

        private void SetPhone(string? phone)
        {
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        }

        private void SetAnswersJson(string answersJson)
        {
            AnswersJson = answersJson.Trim();
        }


        public Guid Id { get; private set; }

        public Guid LeadFormId { get; private set; }

        public string FullName { get; private set; } = default!;

        public string? Email { get; private set; }

        public string? Phone { get; private set; }

        public string AnswersJson { get; private set; } = default!;

        public DateTimeOffset SubmittedAt { get; private set; }


        /* Navigation properties */

        public LeadForm LeadForm { get; private set; } = default!;
    }
}
