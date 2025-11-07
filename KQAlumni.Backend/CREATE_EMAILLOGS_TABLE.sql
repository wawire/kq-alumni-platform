-- Run this SQL script directly on your database to create the EmailLogs table
-- This fixes the "Invalid object name 'EmailLogs'" error

USE KQAlumniDB;
GO

-- Create EmailLogs table
CREATE TABLE [dbo].[EmailLogs] (
    [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [RegistrationId] uniqueidentifier NULL,
    [ToEmail] nvarchar(256) NOT NULL,
    [Subject] nvarchar(500) NOT NULL,
    [EmailType] nvarchar(50) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [ErrorMessage] nvarchar(2000) NULL,
    [SmtpServer] nvarchar(256) NULL,
    [SentAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [DurationMs] int NULL,
    [RetryCount] int NOT NULL DEFAULT 0,
    [Metadata] nvarchar(1000) NULL,
    CONSTRAINT [PK_EmailLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EmailLogs_AlumniRegistrations_RegistrationId]
        FOREIGN KEY ([RegistrationId])
        REFERENCES [AlumniRegistrations] ([Id])
        ON DELETE SET NULL
);
GO

-- Create indexes
CREATE NONCLUSTERED INDEX [IX_EmailLogs_RegistrationId]
    ON [EmailLogs] ([RegistrationId]);

CREATE NONCLUSTERED INDEX [IX_EmailLogs_ToEmail]
    ON [EmailLogs] ([ToEmail]);

CREATE NONCLUSTERED INDEX [IX_EmailLogs_Status]
    ON [EmailLogs] ([Status]);

CREATE NONCLUSTERED INDEX [IX_EmailLogs_EmailType]
    ON [EmailLogs] ([EmailType]);

CREATE NONCLUSTERED INDEX [IX_EmailLogs_SentAt]
    ON [EmailLogs] ([SentAt]);

CREATE NONCLUSTERED INDEX [IX_EmailLogs_Status_SentAt]
    ON [EmailLogs] ([Status], [SentAt] DESC);

CREATE NONCLUSTERED INDEX [IX_EmailLogs_EmailType_Status]
    ON [EmailLogs] ([EmailType], [Status]);

CREATE NONCLUSTERED INDEX [IX_EmailLogs_Registration_SentAt]
    ON [EmailLogs] ([RegistrationId], [SentAt] DESC);
GO

PRINT 'EmailLogs table created successfully!';
GO
