using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Entities.Clients
{
    public enum TimelineEventType
    {
        // Lead & Discovery
        LeadCaptured = 1,
        DiscoveryCallScheduled = 2,
        DiscoveryCallOutcome = 3,


        // Packages & Proposals
        PackageCreated = 10,
        PackageUpdated = 11,
        PackageDeleted = 12,
        ProposalSent = 13,
        ProposalAccepted = 14,
        ProposalRejected = 15,


        // Onboarding
        OnboardingStarted = 20,
        OnboardingTaskFinished = 21,


        // Sessions
        SessionScheduled = 30,
        SessionRescheduled = 31,
        SessionCancelled = 32,
        SessionCompleted = 33,


        // Reminders
        SessionReminderScheduled = 40,
        SessionReminderRescheduled = 41,
        SessionReminderCancelled = 42,
        SessionReminderSent = 43,


        // Notes
        ConsultantNoteAdded = 50,
        ConsultantNoteUpdated = 51,


        // Documents
        ConsultantDocumentUploaded = 60,
        ConsultantDocumentUpdated = 61,
        ConsultantDocumentDeleted = 62,


        // Client profile
        ClientProfileUpdated = 70,
        ClientStatusChanged = 71
    }
}
