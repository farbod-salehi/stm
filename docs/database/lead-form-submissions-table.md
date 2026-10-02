# STM - LeadFormSubmissions Table

## Schema

| Column      | Type          | Description                                                  |
| ----------- | ------------- | ------------------------------------------------------------ |
| Id          | GUID          | Primary key;                                                 |
| LeadFormId  | GUID          | Foreign key referencing LeadForms table; (Required, Indexed) |
| FullName    | NVARCHAR(200) | Lead's name; (Required)                                      |
| Email       | NVARCHAR(320) | Lead's email; (Not Required)                                 |
| Phone       | NVARCHAR(50)  | Lead's phone; (Not Required)                                 |
| AnswersJson | NVARCHAR(MAX) | String of JSON object mapping answers to fields; (Required)  |
| SubmittedAt | DATETIME2(0)  | Submission timestamp; (Required, Indexed)                    |

---

## Indexes

- `IX_LeadFormSubmissions_LeadFormId`
- `IX_LeadFormSubmissions_SubmittedAt`
