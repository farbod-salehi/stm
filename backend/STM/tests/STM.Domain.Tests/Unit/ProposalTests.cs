using STM.Domain.Entities.ClientEngagement;
using STM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Tests.Unit
{
    public class ProposalTests
    {
        [InlineData("package description", "proposal notes")]
        [InlineData(null, null)]
        [Theory]
        public void Proposal_must_be_created_and_returned_with_valid_inputs(string? packageDescription, string? notes)
        {
            // Arrange
            Guid clientId = Guid.NewGuid(); 
            Guid packageId = Guid.NewGuid(); 
            string packageName = "package name";
            decimal packagePrice = 99.9m;
            int packageSessionsIncluded = 5;

            var beforeCreationTime = DateTimeOffset.UtcNow;

            // Act
            Proposal proposal = new(clientId, packageId, packageName, packageDescription, packagePrice, packageSessionsIncluded, notes);

            var afterCreationTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.NotEqual(Guid.Empty, proposal.Id);
            Assert.Equal(clientId, proposal.ClientId);
            Assert.Equal(packageId, proposal.PackageId);
            Assert.Equal(packageName, proposal.PackageName);
            Assert.Equal(packageDescription, proposal.PackageDescription);
            Assert.Equal(packagePrice, proposal.PackagePrice);
            Assert.Equal(packageSessionsIncluded, proposal.PackageSessionsIncluded);
            Assert.Equal(notes, proposal.Notes);
            Assert.InRange(proposal.CreatedAt, beforeCreationTime, afterCreationTime);
            Assert.Equal(ProposalStatus.Draft, proposal.Status);
            Assert.False(proposal.IsActive);
            Assert.Null(proposal.SentAt);
            Assert.Null(proposal.RespondedAt);
            Assert.Null(proposal.UpdatedAt);
            Assert.Null(proposal.DeletedAt);
            Assert.Null(proposal.DeletedBy);
            Assert.False(proposal.IsDeleted);
        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [Theory]
        public void Empty_package_description_must_be_saved_as_null_for_creation(string? packageDescription)
        {
            // Act
            Proposal proposal = CreateProposal(packageDescription: packageDescription);

            // Assert
            Assert.Null(proposal.PackageDescription);
        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [Theory]
        public void Empty_notes_must_be_saved_as_null_for_creation(string? notes)
        {
            // Act
            Proposal proposal = CreateProposal(notes: notes);

            // Assert
            Assert.Null(proposal.Notes);
        }

        [InlineData("new package description", "new proposal notes")]
        [InlineData(null, null)]
        [Theory]
        public void Proposal_must_be_updated_with_valid_inputs(string? newPackageDescription, string? newNotes)
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var newPackageName = "new package name";
            var newPackagePrice = 109.9m;
            var newPackageSessionsIncluded = 10;

            var id = proposal.Id;
            var clientId = proposal.ClientId;
            var packageId = proposal.PackageId;

            var beforeUpdateTime = DateTimeOffset.UtcNow;

            // Act
            proposal.UpdateDetails(newPackageName, newPackageDescription, newPackagePrice, newPackageSessionsIncluded, newNotes);

            var afterUpdateTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(id, proposal.Id);
            Assert.Equal(clientId, proposal.ClientId);
            Assert.Equal(packageId, proposal.PackageId);
            Assert.Equal(newPackageName, proposal.PackageName);
            Assert.Equal(newPackageDescription, proposal.PackageDescription);
            Assert.Equal(newPackagePrice, proposal.PackagePrice);
            Assert.Equal(newPackageSessionsIncluded, proposal.PackageSessionsIncluded);
            Assert.Equal(newNotes, proposal.Notes);
            Assert.NotNull(proposal.UpdatedAt);
            Assert.InRange(proposal.UpdatedAt ?? default, beforeUpdateTime, afterUpdateTime);
            Assert.Null(proposal.DeletedAt);
            Assert.Null(proposal.DeletedBy);
            Assert.False(proposal.IsDeleted);
            Assert.Equal(ProposalStatus.Draft, proposal.Status);
            Assert.False(proposal.IsActive);
            Assert.Null(proposal.SentAt);
            Assert.Null(proposal.RespondedAt);
        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [Theory]
        public void Empty_package_description_must_be_saved_as_null_for_update(string? newPackageDescription)
        {
            // Arrange
            Proposal proposal = CreateProposal();

            // Act
            proposal.UpdateDetails(proposal.PackageName, newPackageDescription, proposal.PackagePrice,
                proposal.PackageSessionsIncluded, proposal.Notes);

            // Assert
            Assert.Null(proposal.PackageDescription);
        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [Theory]
        public void Empty_notes_must_be_saved_as_null_for_update(string? newNotes)
        {
            // Arrange
            Proposal proposal = CreateProposal();

            // Act
            proposal.UpdateDetails(proposal.PackageName, proposal.PackageDescription, proposal.PackagePrice,
               proposal.PackageSessionsIncluded, newNotes);

            // Assert
            Assert.Null(proposal.Notes);
        }

        [Fact]
        public void Proposal_must_be_deleted_when_status_is_draft()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var updatedAt = proposal.UpdatedAt;

            var deletedBy = Guid.NewGuid();

            var beforeDeleteTime = DateTimeOffset.UtcNow;

            // Act
            proposal.Delete(deletedBy);

            var afterDeleteTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.True(proposal.IsDeleted);
            Assert.Equal(deletedBy, proposal.DeletedBy);
            Assert.NotNull(proposal.DeletedAt);
            Assert.InRange(proposal.DeletedAt ?? default, beforeDeleteTime, afterDeleteTime);
            Assert.Equal(updatedAt, proposal.UpdatedAt);
          
        }

        [Fact]
        public void Delete_deleted_proposal_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var deletedBy = Guid.NewGuid();

            proposal.Delete(deletedBy);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.Delete(Guid.NewGuid()));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.IsAlreadyDeleted, domainError.Code);
            Assert.Equal(deletedBy, proposal.DeletedBy);
            
        }

        [Fact]
        public void Delete_when_proposal_is_sent_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            proposal.MarkAsSent();

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.Delete(Guid.NewGuid()));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.OnlyDraftedProposalsCanBeDeleted, domainError.Code);
            Assert.False(proposal.IsDeleted);
            Assert.Null(proposal.DeletedAt);
            Assert.Null(proposal.DeletedBy);

        }

        [Fact]
        public void Delete_when_proposal_is_accepted_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            proposal.MarkAsSent();
            proposal.MarkAsAccepted();

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.Delete(Guid.NewGuid()));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.OnlyDraftedProposalsCanBeDeleted, domainError.Code);
            Assert.False(proposal.IsDeleted);
            Assert.Null(proposal.DeletedAt);
            Assert.Null(proposal.DeletedBy);

        }

        [Fact]
        public void Delete_when_proposal_is_rejected_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            proposal.MarkAsSent();
            proposal.MarkAsRejected();

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.Delete(Guid.NewGuid()));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.OnlyDraftedProposalsCanBeDeleted, domainError.Code);
            Assert.False(proposal.IsDeleted);
            Assert.Null(proposal.DeletedAt);
            Assert.Null(proposal.DeletedBy);

        }

        [Fact]
        public void Update_deleted_proposal_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var packageName = proposal.PackageName;
            var updatedAt = proposal.UpdatedAt;

            proposal.Delete(Guid.NewGuid());

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.UpdateDetails("new package name", proposal.PackageDescription, proposal.PackagePrice, proposal.PackageSessionsIncluded, proposal.Notes));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.CannotUpdateDeletedProposal, domainError.Code);
            Assert.Equal(packageName, proposal.PackageName);
            Assert.Equal(updatedAt, proposal.UpdatedAt);

        }

        [Fact]
        public void Update_when_proposal_is_sent_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var packageName = proposal.PackageName;
            var updatedAt = proposal.UpdatedAt;

            proposal.MarkAsSent();

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.UpdateDetails("new package name", proposal.PackageDescription, proposal.PackagePrice, proposal.PackageSessionsIncluded, proposal.Notes));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.OnlyDraftProposalsCanBeUpdated, domainError.Code);
            Assert.Equal(packageName, proposal.PackageName);
            Assert.Equal(updatedAt, proposal.UpdatedAt);
        }

        [Fact]
        public void Update_when_proposal_is_accepted_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var packageName = proposal.PackageName;
            var updatedAt = proposal.UpdatedAt;

            proposal.MarkAsSent();
            proposal.MarkAsAccepted();

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.UpdateDetails("new package name", proposal.PackageDescription, proposal.PackagePrice, proposal.PackageSessionsIncluded, proposal.Notes));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.OnlyDraftProposalsCanBeUpdated, domainError.Code);
            Assert.Equal(packageName, proposal.PackageName);
            Assert.Equal(updatedAt, proposal.UpdatedAt);
        }

        [Fact]
        public void Update_when_proposal_is_rejected_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var packageName = proposal.PackageName;
            var updatedAt = proposal.UpdatedAt;

            proposal.MarkAsSent();
            proposal.MarkAsRejected();

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.UpdateDetails("new package name", proposal.PackageDescription, proposal.PackagePrice, proposal.PackageSessionsIncluded, proposal.Notes));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.OnlyDraftProposalsCanBeUpdated, domainError.Code);
            Assert.Equal(packageName, proposal.PackageName);
            Assert.Equal(updatedAt, proposal.UpdatedAt);
        }


        [Fact]
        public void Proposal_can_be_sent_when_it_is_draft()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var beforeSentTime = DateTimeOffset.UtcNow;

            // Act
            proposal.MarkAsSent();

            var afterSentTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(ProposalStatus.Sent, proposal.Status);
            Assert.NotNull(proposal.SentAt);
            Assert.InRange(proposal.SentAt ?? default, beforeSentTime, afterSentTime);
            Assert.False(proposal.IsActive);
            Assert.Null(proposal.RespondedAt);
            Assert.False(proposal.IsDeleted);
        }

        [Fact]
        public void Sending_when_proposal_is_deleted_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            proposal.Delete(Guid.NewGuid());

            // Act && Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.MarkAsSent());

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.CannotSendDeletedProposal, domainError.Code);
            Assert.Null(proposal.SentAt);
            Assert.NotEqual(ProposalStatus.Sent, proposal.Status);

        }

        [Fact]
        public void Sending_when_proposal_is_sent_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            proposal.MarkAsSent();

            var respondedAt = proposal.RespondedAt;

            // Act && Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.MarkAsSent());

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.OnlyDraftProposalsCanBeSent, domainError.Code);
            Assert.Equal(respondedAt, proposal.RespondedAt);
        }

        [Fact]
        public void Sending_when_proposal_is_accepted_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            proposal.MarkAsSent();
            proposal.MarkAsAccepted();

            var respondedAt = proposal.RespondedAt;

            // Act && Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.MarkAsSent());

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.OnlyDraftProposalsCanBeSent, domainError.Code);
            Assert.Equal(respondedAt, proposal.RespondedAt);

        }

        [Fact]
        public void Sending_when_proposal_is_rejected_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            proposal.MarkAsSent();
            proposal.MarkAsRejected();

            var respondedAt = proposal.RespondedAt;

            // Act && Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.MarkAsSent());

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.OnlyDraftProposalsCanBeSent, domainError.Code);
            Assert.Equal(respondedAt, proposal.RespondedAt);

        }

        [Fact]
        public void Proposal_can_be_accepted_when_it_is_sent()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            proposal.MarkAsSent();

            var sentAt = proposal.SentAt;

            var beforeAcceptTime = DateTimeOffset.UtcNow;

            // Act
            proposal.MarkAsAccepted();

            var afterAcceptTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(ProposalStatus.Accepted, proposal.Status);
            Assert.True(proposal.IsActive);
            Assert.NotNull(proposal.RespondedAt);
            Assert.InRange(proposal.RespondedAt ?? default, beforeAcceptTime, afterAcceptTime);
            Assert.Equal(sentAt, proposal.SentAt);
        }

        [Fact]
        public void Accept_when_proposal_is_draft_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            // Act && Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.MarkAsAccepted());

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.OnlySentProposalsCanBeAccepted, domainError.Code);
            Assert.Null(proposal.RespondedAt);
            Assert.NotEqual(ProposalStatus.Accepted, proposal.Status);
            Assert.False(proposal.IsActive);

        }

        [Fact]
        public void Accept_when_proposal_is_accepted_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            proposal.MarkAsSent();
            proposal.MarkAsAccepted();

            var respondedAt = proposal.RespondedAt;

            // Act && Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.MarkAsAccepted());

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.OnlySentProposalsCanBeAccepted, domainError.Code);
            Assert.Equal(respondedAt, proposal.RespondedAt);
        }

        [Fact]
        public void Accept_when_proposal_is_rejected_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            proposal.MarkAsSent();
            proposal.MarkAsRejected();

            var respondedAt = proposal.RespondedAt;

            // Act && Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.MarkAsAccepted());

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.OnlySentProposalsCanBeAccepted, domainError.Code);
            Assert.Equal(respondedAt, proposal.RespondedAt);
           
        }

        [Fact]
        public void Proposal_can_be_rejected_when_it_is_sent()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            proposal.MarkAsSent();

            var sentAt = proposal.SentAt;

            var beforeRejectTime = DateTimeOffset.UtcNow;

            // Act
            proposal.MarkAsRejected();

            var afterRejectTime = DateTimeOffset.UtcNow;

            // Assert
            Assert.Equal(ProposalStatus.Rejected, proposal.Status);
            Assert.NotNull(proposal.RespondedAt);
            Assert.InRange(proposal.RespondedAt ?? default, beforeRejectTime, afterRejectTime);
            Assert.Equal(sentAt, proposal.SentAt);
            Assert.False(proposal.IsActive);
        }


        [Fact]
        public void Reject_when_proposal_is_draft_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            // Act && Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.MarkAsRejected());

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.OnlySentProposalsCanBeRejected, domainError.Code);
            Assert.Null(proposal.RespondedAt);
            Assert.NotEqual(ProposalStatus.Rejected, proposal.Status);

        }

        [Fact]
        public void Reject_when_proposal_is_rejected_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            proposal.MarkAsSent();
            proposal.MarkAsRejected();

            var respondedAt = proposal.RespondedAt;

            // Act && Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.MarkAsRejected());

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.OnlySentProposalsCanBeRejected, domainError.Code);
            Assert.Equal(respondedAt, proposal.RespondedAt);
            
        }

        [Fact]
        public void Reject_when_proposal_is_accepted_must_throw_an_exception()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            proposal.MarkAsSent();
            proposal.MarkAsAccepted();

            var respondedAt = proposal.RespondedAt;

            // Act && Assert
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.MarkAsRejected());

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.OnlySentProposalsCanBeRejected, domainError.Code);
            Assert.Equal(respondedAt, proposal.RespondedAt);

        }

        [Fact]
        public void Package_name_must_be_trimmed_for_creation()
        {
            // Arrange
            var packageName = " package name ";

            // Act
            Proposal proposal = CreateProposal(packageName: packageName);

            // Assert
            Assert.Equal("package name", proposal.PackageName);

        }

        [Fact]
        public void Package_name_must_be_trimmed_for_update()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var newPackageName = " new package name ";

            // Act
            proposal.UpdateDetails(newPackageName, proposal.PackageDescription, proposal.PackagePrice, proposal.PackageSessionsIncluded, proposal.Notes);

            // Assert
            Assert.Equal("new package name", proposal.PackageName);
        }

        [Fact]
        public void Package_description_must_be_trimmed_for_creation()
        {
            // Arrange
            var packageDescription = " package description ";

            // Act
            Proposal proposal = CreateProposal(packageDescription: packageDescription);

            // Assert
            Assert.Equal("package description", proposal.PackageDescription);
        }

        [Fact]
        public void Package_description_must_be_trimmed_for_update()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var newPackageDescription = " new package description ";

            // Act
            proposal.UpdateDetails(proposal.PackageName, newPackageDescription, proposal.PackagePrice, proposal.PackageSessionsIncluded, proposal.Notes);

            // Assert
            Assert.Equal("new package description", proposal.PackageDescription);
        }

        [Fact]
        public void Notes_must_be_trimmed_for_creation()
        {
            // Arrange
            var notes = " proposal notes ";

            // Act
            Proposal proposal = CreateProposal(notes: notes);

            // Assert
            Assert.Equal("proposal notes", proposal.Notes);
        }

        [Fact]
        public void Notes_must_be_trimmed_for_update()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var newNotes = " new proposal notes ";

            // Act
            proposal.UpdateDetails(proposal.PackageName, proposal.PackageDescription, proposal.PackagePrice, proposal.PackageSessionsIncluded, newNotes);

            // Assert
            Assert.Equal("new proposal notes", proposal.Notes);
        }

        [Fact]
        public void Client_id_is_required_for_creation()
        {
            // Arrange
            var clientId = Guid.Empty;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreateProposal(clientId: clientId));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.ClientIdIsRequired, domainError.Code);
        }

        [Fact]
        public void Package_id_is_required_for_creation()
        {
            // Arrange
            var packageId = Guid.Empty;

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreateProposal(packageId: packageId));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.PackageIdIsRequired, domainError.Code);
        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [Theory]
        public void Package_name_is_required_for_creation(string? packageName)
        {
            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreateProposal(packageName: packageName!));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.PackageNameIsRequired, domainError.Code);
        }

        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [Theory]
        public void Package_name_is_required_for_update(string? newPackageName)
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var packageName = proposal.PackageName;

            // Act
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.UpdateDetails(newPackageName!, proposal.PackageDescription, proposal.PackagePrice, proposal.PackageSessionsIncluded, proposal.Notes));
            
            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.PackageNameIsRequired, domainError.Code);
            Assert.Equal(packageName, proposal.PackageName);
        }

        [Fact]
        public void Package_name_length_should_not_exceed_200_characters_for_creation()
        {
            // Arrange
            var packageName = new string('A', 201);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreateProposal(packageName: packageName));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.PackageNameIsTooLong, domainError.Code);
        }
        [Fact]
        public void Package_name_length_should_not_exceed_200_characters_for_update()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var packageName = proposal.PackageName;

            // Act
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.UpdateDetails(new string('A', 201), proposal.PackageDescription, proposal.PackagePrice, proposal.PackageSessionsIncluded, proposal.Notes));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.PackageNameIsTooLong, domainError.Code);
            Assert.Equal(packageName, proposal.PackageName);
        }

        [Fact]
        public void Package_description_length_should_not_exceed_5000_characters_for_creation()
        {
            // Arrange
            var packageDescription = new string('A', 5001);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreateProposal(packageDescription: packageDescription));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.PackageDescriptionIsTooLong, domainError.Code);
        }

        [Fact]
        public void Package_description_length_should_not_exceed_5000_characters_for_update()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var packageDescription = proposal.PackageDescription;

            // Act
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.UpdateDetails(proposal.PackageName, new string('A', 5001), proposal.PackagePrice, proposal.PackageSessionsIncluded, proposal.Notes));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.PackageDescriptionIsTooLong, domainError.Code);
            Assert.Equal(packageDescription, proposal.PackageDescription);
        }

        [InlineData(-100)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(999999999)]
        [Theory]
        public void Package_price_must_be_valid_for_creation(decimal packagePrice)
        {
            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreateProposal(packagePrice: packagePrice));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.PackagePriceIsInvalid, domainError.Code);
        }

        [InlineData(-100)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(999999999)]
        [Theory]
        public void Package_price_must_be_valid_for_update(decimal newPackagePrice)
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var packagePrice = proposal.PackagePrice;

            // Act
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.UpdateDetails(proposal.PackageName, proposal.PackageDescription, newPackagePrice, proposal.PackageSessionsIncluded, proposal.Notes));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.PackagePriceIsInvalid, domainError.Code);
            Assert.Equal(packagePrice, proposal.PackagePrice);
        }

        [InlineData(-100)]
        [InlineData(-1)]
        [InlineData(0)]
        [Theory]
        public void Package_sessions_included_must_be_valid_for_creation(int packageSessionsIncluded)
        {
            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreateProposal(packageSessionsIncluded: packageSessionsIncluded));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.PackageSessionsIncludedIsInvalid, domainError.Code);
        }

        [InlineData(-100)]
        [InlineData(-1)]
        [InlineData(0)]
        [Theory]
        public void Package_sessions_included_must_be_valid_for_update(int newPackageSessionsIncluded)
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var packageSessionsIncluded = proposal.PackageSessionsIncluded;

            // Act
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.UpdateDetails(proposal.PackageName, proposal.PackageDescription, proposal.PackagePrice, newPackageSessionsIncluded, proposal.Notes));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.PackageSessionsIncludedIsInvalid, domainError.Code);
            Assert.Equal(packageSessionsIncluded, proposal.PackageSessionsIncluded);
        }

        [Fact]
        public void Notes_length_should_not_exceed_5000_characters_for_creation()
        {
            // Arrange
            var notes = new string('A', 5001);

            // Act & Assert
            var exception = Assert.Throws<BusinessRuleException>(() => CreateProposal(notes: notes));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.NotesIsTooLong, domainError.Code);
        }

        [Fact]
        public void Notes_length_should_not_exceed_5000_characters_for_update()
        {
            // Arrange
            Proposal proposal = CreateProposal();

            var notes = proposal.Notes;

            // Act
            var exception = Assert.Throws<BusinessRuleException>(() => proposal.UpdateDetails(proposal.PackageName, proposal.PackageDescription, proposal.PackagePrice, proposal.PackageSessionsIncluded, new string('A', 5001)));

            DomainError domainError = Assert.Single(exception.Errors);
            Assert.Equal(DomainErrors.Proposal.NotesIsTooLong, domainError.Code);
            Assert.Equal(notes, proposal.Notes);
        }

        public static Proposal CreateProposal(Guid? clientId = null, Guid? packageId = null, string packageName = "package name", 
            string? packageDescription = "package description", decimal packagePrice = 99.9m, int packageSessionsIncluded = 5, string? notes = "proposal notes")
        {
            return new(clientId ?? Guid.NewGuid(), packageId ?? Guid.NewGuid(), packageName, packageDescription, packagePrice, packageSessionsIncluded, notes);
        }
    }
}
