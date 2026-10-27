# STM - SessionMaterials Table

## Schema

| Column      | Type           | Description                                                 |
| ----------- | -------------- | ----------------------------------------------------------- |
| Id          | GUID           | Primary key;                                                |
| SessionId   | GUID           | Foreign key referencing Sessions table; (Required, Indexed) |
| Title       | NVARCHAR(200)  | Name of the material; (Required)                            |
| Description | NVARCHAR(MAX)  | Optional description; (Not Required)                        |
| FileUrl     | NVARCHAR(500)  | URL to the uploaded file (PDF, docx, etc.); (Required)      |
| FileType    | NVARCHAR(50)   | File type: pdf, docx, pptx, image, etc.; (Required)         |
| UploadedAt  | DATETIMEOFFSET | When the material was uploaded; (Required)                  |
| UploadedBy  | GUID           | Consultant who uploaded the material; (Required)            |
| UpdatedAt   | DATETIMEOFFSET | Updating timestamp; (Not Required)                          |

---

## Indexes

- `IX_SessionMaterials_SessionId`
