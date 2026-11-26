using STM.Domain.Entities.Clients;
using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace STM.Domain.Entities.ClientEngagement
{
    public sealed class Proposal
    {
        private const int PackageNameMaxLength = 200;

        private const int PackageDescriptionMaxLength = 5000;

        private const decimal PackageMaxPrice = 99999999.99m;

        private const int NotesMaxLength = 5000;

        private Proposal() { /* EF Core constructor */ }

        public Proposal(Guid clientId, Guid packageId, string packageName, string? packageDescription, 
            decimal packagePrice, int packageSessionsIncluded, string? notes)
        {
            List<DomainError> errors = [];

            errors.AddRange(ValidateClientId(clientId));
            errors.AddRange(ValidatePackageId(packageId));
            errors.AddRange(ValidatePackageName(packageName));
            errors.AddRange(ValidatePackageDescription(packageDescription));
            errors.AddRange(ValidatePackagePrice(packagePrice));
            errors.AddRange(ValidatePackageSessionsIncluded(packageSessionsIncluded));
            errors.AddRange(ValidateNotes(notes));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            Id = Guid.NewGuid();
            ClientId = clientId;
            PackageId = packageId;
            SetPackageName(packageName);
            SetPackageDescription(packageDescription);
            PackagePrice = packagePrice;
            PackageSessionsIncluded = packageSessionsIncluded;
            Status = ProposalStatus.Draft;
            IsActive = false;
            SentAt = null;
            RespondedAt = null;
            SetNotes(notes);
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = null;
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
        }

        public void UpdateDetails(string packageName, string? packageDescription, decimal packagePrice, int packageSessionsIncluded, string? notes)
        {
            if (IsDeleted)
                throw new BusinessRuleException([new DomainError(DomainErrors.Proposal.CannotUpdateDeletedProposal)]);
            
            if(Status != ProposalStatus.Draft)
                throw new BusinessRuleException([new DomainError(DomainErrors.Proposal.OnlyDraftProposalsCanBeUpdated)]);

            List<DomainError> errors = [];

            errors.AddRange(ValidatePackageName(packageName));
            errors.AddRange(ValidatePackageDescription(packageDescription));
            errors.AddRange(ValidatePackagePrice(packagePrice));
            errors.AddRange(ValidatePackageSessionsIncluded(packageSessionsIncluded));
            errors.AddRange(ValidateNotes(notes));

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);

            SetPackageName(packageName);
            SetPackageDescription(packageDescription);
            PackagePrice = packagePrice;
            PackageSessionsIncluded = packageSessionsIncluded;
            SetNotes(notes);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void MarkAsSent()
        {
            if (IsDeleted)
                throw new BusinessRuleException([new DomainError(DomainErrors.Proposal.CannotSendDeletedProposal)]);

            if (Status != ProposalStatus.Draft)
                throw new BusinessRuleException([new DomainError(DomainErrors.Proposal.OnlyDraftProposalsCanBeSent)]);

            Status = ProposalStatus.Sent;
            SentAt = DateTimeOffset.UtcNow;
        }

        public void MarkAsAccepted()
        {
            if (IsDeleted)
                throw new BusinessRuleException([new DomainError(DomainErrors.Proposal.CannotAcceptDeletedProposal)]);

            if (Status != ProposalStatus.Sent)
                throw new BusinessRuleException([new DomainError(DomainErrors.Proposal.OnlySentProposalsCanBeAccepted)]);

            Status = ProposalStatus.Accepted;
            IsActive = true;
            RespondedAt = DateTimeOffset.UtcNow;
        }

        public void MarkAsRejected()
        {
            if (IsDeleted)
                throw new BusinessRuleException([new DomainError(DomainErrors.Proposal.CannotRejectDeletedProposal)]);

            if (Status != ProposalStatus.Sent)
                throw new BusinessRuleException([new DomainError(DomainErrors.Proposal.OnlySentProposalsCanBeRejected)]);

            Status = ProposalStatus.Rejected;
            RespondedAt = DateTimeOffset.UtcNow;
        }

        public void Delete(Guid deletedBy)
        {
            if (IsDeleted)
                throw new BusinessRuleException([new DomainError(DomainErrors.Proposal.IsAlreadyDeleted)]);

            if(Status != ProposalStatus.Draft)
                throw new BusinessRuleException([new DomainError(DomainErrors.Proposal.OnlyDraftedProposalsCanBeDeleted)]);

            IsDeleted = true;
            DeletedAt = DateTimeOffset.UtcNow;
            DeletedBy = deletedBy;
        }

        private void SetPackageName(string packageName)
        {
            PackageName = packageName.Trim();
        }

        private void SetPackageDescription(string? packageDescription)
        {
            PackageDescription = string.IsNullOrWhiteSpace(packageDescription) ? null : packageDescription.Trim();
        }

        private void SetNotes(string? notes)
        {
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        }

        private static List<DomainError> ValidateClientId(Guid clientId)
        {
            List<DomainError> errors = [];

            if (clientId == Guid.Empty)
                errors.Add(new DomainError(DomainErrors.Proposal.ClientIdIsRequired));

            return errors;
        }

        private static List<DomainError> ValidatePackageId(Guid packageId)
        {
            List<DomainError> errors = [];

            if (packageId == Guid.Empty)
                errors.Add(new DomainError(DomainErrors.Proposal.PackageIdIsRequired));

            return errors;
        }

        private static List<DomainError> ValidatePackageName(string packageName)
        {
            List<DomainError> errors = [];

            if(string.IsNullOrWhiteSpace(packageName))
                errors.Add(new DomainError(DomainErrors.Proposal.PackageNameIsRequired));
            else if (packageName.Trim().Length > PackageNameMaxLength)
                errors.Add(new DomainError(DomainErrors.Proposal.PackageNameIsTooLong));

            return errors;
        }

        private static List<DomainError> ValidatePackageDescription(string? packageDescription)
        {
            List<DomainError> errors = [];

            if(!string.IsNullOrWhiteSpace(packageDescription) && packageDescription.Trim().Length > PackageDescriptionMaxLength)
                errors.Add(new DomainError(DomainErrors.Proposal.PackageDescriptionIsTooLong));

            return errors;
        }

        private static List<DomainError> ValidatePackagePrice(decimal packagePrice)
        {
            List<DomainError> errors = [];

            if(packagePrice <= 0 || packagePrice > PackageMaxPrice)
                errors.Add(new DomainError(DomainErrors.Proposal.PackagePriceIsInvalid));

            return errors;
        }

        private static List<DomainError> ValidatePackageSessionsIncluded(int packageSessionsIncluded)
        {
            List<DomainError> errors = [];

            if (packageSessionsIncluded <= 0)
                errors.Add(new DomainError(DomainErrors.Proposal.PackageSessionsIncludedIsInvalid));

            return errors;
        }

        private static List<DomainError> ValidateNotes(string? notes)
        {
            List<DomainError> errors = [];

            if (!string.IsNullOrWhiteSpace(notes) && notes.Trim().Length > NotesMaxLength)
                errors.Add(new DomainError(DomainErrors.Proposal.NotesIsTooLong));

            return errors;
        }

        public Guid Id { get; private set; }

        public Guid ClientId { get; private set; }

        public Guid PackageId { get; private set; }

        public string PackageName { get; private set; } = default!;

        public string? PackageDescription { get; private set; }

        public decimal PackagePrice { get; private set; }

        public int PackageSessionsIncluded { get; private set; }

        public ProposalStatus Status { get; private set; }

        public bool IsActive { get; private set; }

        public DateTimeOffset? SentAt { get; private set; }

        public DateTimeOffset? RespondedAt { get; private set; }

        public string? Notes { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public bool IsDeleted { get; private set; }

        public DateTimeOffset? DeletedAt { get; private set; }

        public Guid? DeletedBy { get; private set; }

        public byte[] RowVersion { get; private set; } = default!;


        /* Navigtion properties */

        public Package Package { get; private set; } = default!;

        public Client Client { get; private set; } = default!;
    }
}
