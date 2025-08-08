# Load Documents Feature 📄

## Overview

The Load Documents feature is a comprehensive document management system for logistics loads, allowing users to upload, organize, and manage various types of documents associated with shipping loads. This feature provides secure file storage, document lifecycle management, and role-based access control.

## 🚀 Key Features

- **Document Upload & Storage**: Secure upload of documents up to 50MB with blob storage integration
- **Document Types**: Support for 8 different document types including Bill of Lading, Proof of Delivery, Invoices, etc.
- **Lifecycle Management**: Document status tracking (Active, Archived, Deleted)
- **Role-Based Access**: Granular permissions for viewing, editing, and deleting documents
- **Audit Trail**: Complete tracking of who uploaded and modified documents
- **File Download**: Secure document retrieval with original filename preservation
- **Metadata Management**: Rich metadata including descriptions, file properties, and timestamps

## 📋 Supported Document Types

| Type | Description | Use Case |
|------|-------------|----------|
| `BillOfLading` | Bill of Lading | Primary shipping document |
| `ProofOfDelivery` | Proof of Delivery | Delivery confirmation |
| `Invoice` | Invoice | Billing documentation |
| `Receipt` | Receipt | Payment confirmation |
| `Contract` | Contract | Legal agreements |
| `InsuranceCertificate` | Insurance Certificate | Insurance coverage proof |
| `Photo` | Photo | Visual documentation |
| `Other` | Other | Miscellaneous documents |

## 🏗️ Architecture

### Domain Layer
- **LoadDocument Entity**: Core aggregate root with domain events
- **Document Enums**: Type and status definitions
- **Domain Events**: `LoadDocumentUploadedEvent`, `LoadDocumentDeletedEvent`
- **Blob Storage Service**: Abstraction for file storage operations

### Application Layer (CQRS Pattern)
- **Commands**: Upload, Update, Delete operations
- **Queries**: Get documents, download files
- **Handlers**: Business logic implementation
- **Validators**: Input validation using FluentValidation

### Infrastructure Layer
- **Azure Blob Storage**: Primary storage provider
- **File Blob Storage**: Alternative local storage provider
- **Entity Framework**: Data persistence
- **Database Migration**: Automated schema updates

## 🔌 API Endpoints

### Base URL
```
/loads/{loadId:guid}/documents
```

### 1. Get Load Documents
```http
GET /loads/{loadId}/documents
```

**Query Parameters:**
- `pageNumber` (optional): Page number for pagination
- `pageSize` (optional): Number of items per page
- `documentType` (optional): Filter by document type
- `status` (optional): Filter by document status

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fileName": "invoice_001.pdf",
      "originalFileName": "Invoice #001.pdf",
      "contentType": "application/pdf",
      "fileSizeBytes": 524288,
      "type": "Invoice",
      "status": "Active",
      "description": "Customer invoice for load #12345",
      "createdAt": "2025-01-15T10:30:00Z",
      "loadId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "loadName": "Chicago to New York",
      "loadNumber": 12345,
      "uploadedById": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "uploadedByName": "John Doe",
      "uploadedByEmail": "john.doe@company.com"
    }
  ]
}
```

### 2. Get Document by ID
```http
GET /loads/{loadId}/documents/{documentId}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fileName": "invoice_001.pdf",
    "originalFileName": "Invoice #001.pdf",
    "contentType": "application/pdf",
    "fileSizeBytes": 524288,
    "type": "Invoice",
    "status": "Active",
    "description": "Customer invoice for load #12345",
    "createdAt": "2025-01-15T10:30:00Z"
  }
}
```

### 3. Upload Document
```http
POST /loads/{loadId}/documents/upload
Content-Type: multipart/form-data
```

**Request Body:**
```json
{
  "file": "[binary file data]",
  "type": "Invoice",
  "description": "Customer invoice for shipment"
}
```

**Features:**
- ✅ 50MB file size limit
- ✅ Automatic file type validation
- ✅ Virus scanning (if configured)
- ✅ Duplicate filename handling
- ✅ Secure file storage

**Response:**
```json
{
  "success": true,
  "data": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "Document uploaded successfully"
}
```

### 4. Download Document
```http
GET /loads/{loadId}/documents/{documentId}/download
```

**Response:**
- Content-Type: Original file content type
- Content-Disposition: attachment; filename="original-filename.ext"
- Binary file content

### 5. Update Document
```http
PUT /loads/{loadId}/documents/{documentId}
Content-Type: application/json
```

**Request Body:**
```json
{
  "description": "Updated document description",
  "status": "Active"
}
```

### 6. Delete Document
```http
DELETE /loads/{loadId}/documents/{documentId}
```

**Response:**
```json
{
  "success": true,
  "message": "Document deleted successfully"
}
```

## 🔐 Security & Permissions

### Required Permissions
- **View Documents**: `Permissions.Loads.View`
- **Upload Documents**: `Permissions.Loads.Edit`
- **Update Documents**: `Permissions.Loads.Edit`
- **Delete Documents**: `Permissions.Loads.Delete`

### Security Features
- ✅ JWT token authentication required
- ✅ Role-based authorization
- ✅ File type validation
- ✅ File size limits (50MB)
- ✅ Secure blob storage with access controls
- ✅ Audit logging for all operations

## 💾 Database Schema

### LoadDocument Table
```sql
CREATE TABLE LoadDocuments (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    FileName NVARCHAR(255) NOT NULL,
    OriginalFileName NVARCHAR(255) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    FileSizeBytes BIGINT NOT NULL,
    BlobPath NVARCHAR(500) NOT NULL,
    BlobContainer NVARCHAR(100) NOT NULL,
    Type INT NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    Description NVARCHAR(1000) NULL,
    LoadId UNIQUEIDENTIFIER NOT NULL,
    UploadedById UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    FOREIGN KEY (LoadId) REFERENCES Loads(Id),
    FOREIGN KEY (UploadedById) REFERENCES Employees(Id)
);
```

## 🔄 Usage Examples

### Frontend Integration (JavaScript)

#### Upload Document
```javascript
const uploadDocument = async (loadId, file, type, description) => {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('type', type);
  formData.append('description', description);

  const response = await fetch(`/api/loads/${loadId}/documents/upload`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${authToken}`
    },
    body: formData
  });

  return await response.json();
};
```

#### Download Document
```javascript
const downloadDocument = async (loadId, documentId) => {
  const response = await fetch(`/api/loads/${loadId}/documents/${documentId}/download`, {
    headers: {
      'Authorization': `Bearer ${authToken}`
    }
  });

  if (response.ok) {
    const blob = await response.blob();
    const filename = response.headers.get('content-disposition')
      ?.split('filename=')[1]?.replace(/"/g, '') || 'document';
    
    // Create download link
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    window.URL.revokeObjectURL(url);
  }
};
```

#### Get Documents with Filtering
```javascript
const getLoadDocuments = async (loadId, filters = {}) => {
  const params = new URLSearchParams();
  
  if (filters.documentType) params.append('documentType', filters.documentType);
  if (filters.status) params.append('status', filters.status);
  if (filters.pageNumber) params.append('pageNumber', filters.pageNumber);
  if (filters.pageSize) params.append('pageSize', filters.pageSize);

  const response = await fetch(`/api/loads/${loadId}/documents?${params}`, {
    headers: {
      'Authorization': `Bearer ${authToken}`
    }
  });

  return await response.json();
};
```

## 🧪 Testing

### Unit Tests
```csharp
[Test]
public async Task UploadDocument_ValidInput_ReturnsSuccess()
{
    // Arrange
    var command = new UploadLoadDocumentCommand
    {
        LoadId = Guid.NewGuid(),
        FileName = "test.pdf",
        ContentType = "application/pdf",
        Type = DocumentType.Invoice
    };

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.IsTrue(result.Success);
    Assert.IsNotNull(result.Data);
}
```

### Integration Tests
```csharp
[Test]
public async Task LoadDocumentController_UploadDocument_ReturnsOk()
{
    // Arrange
    var content = new MultipartFormDataContent();
    var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
    content.Add(fileContent, "file", "test.pdf");
    content.Add(new StringContent("Invoice"), "type");

    // Act
    var response = await _client.PostAsync($"/api/loads/{_loadId}/documents/upload", content);

    // Assert
    response.EnsureSuccessStatusCode();
}
```

## 📊 Performance Considerations

### Optimizations Implemented
- ✅ **Streaming Upload**: Large files are streamed to prevent memory issues
- ✅ **Pagination**: Document lists support server-side pagination
- ✅ **Lazy Loading**: Navigation properties loaded on demand
- ✅ **Async Operations**: All I/O operations are asynchronous
- ✅ **Blob Storage CDN**: Optional CDN integration for faster downloads

### Monitoring & Metrics
- Document upload/download counts
- Average file sizes
- Storage usage by tenant
- Failed upload attempts
- Performance metrics (upload/download times)

## 🚀 Future Enhancements

### Planned Features
- [ ] **Document Versioning**: Track document versions and changes
- [ ] **OCR Integration**: Extract text from scanned documents
- [ ] **Document Templates**: Pre-defined templates for common document types
- [ ] **Bulk Operations**: Upload/download multiple documents
- [ ] **Digital Signatures**: Support for e-signatures
- [ ] **Document Approval Workflow**: Multi-step approval process
- [ ] **Advanced Search**: Full-text search within documents
- [ ] **Document Expiry**: Automatic archival of expired documents

## 📝 Configuration

### Blob Storage Configuration
```json
{
  "BlobStorage": {
    "Provider": "Azure", // or "File"
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...",
    "DefaultContainer": "load-documents",
    "MaxFileSizeBytes": 52428800, // 50MB
    "AllowedFileTypes": [".pdf", ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx"]
  }
}
```

### Feature Flags
```json
{
  "FeatureFlags": {
    "LoadDocuments": {
      "Enabled": true,
      "VirusScanningEnabled": false,
      "ThumbnailGenerationEnabled": true,
      "AutoArchiveAfterDays": 365
    }
  }
}
```

## 🐛 Troubleshooting

### Common Issues

#### Upload Failures
- **File too large**: Ensure file is under 50MB limit
- **Invalid file type**: Check allowed file extensions
- **Permission denied**: Verify user has `Loads.Edit` permission
- **Storage quota exceeded**: Check blob storage limits

#### Download Issues
- **File not found**: Document may have been deleted or moved
- **Access denied**: Verify user has `Loads.View` permission
- **Slow downloads**: Consider CDN configuration

#### Performance Issues
- **Slow uploads**: Check network bandwidth and file size
- **Memory issues**: Ensure streaming is enabled for large files
- **Database timeouts**: Review query performance and indexing

---

## 📞 Support

For questions or issues with the Load Documents feature, please:
1. Check this documentation first
2. Review the troubleshooting section
3. Contact the development team
4. Create an issue in the project repository

**Version**: 1.0.0  
**Last Updated**: January 2025  
**Author**: Development Team