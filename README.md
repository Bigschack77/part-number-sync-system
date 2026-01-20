# Part Number Synchronization System

## 🎯 Overview

A real-time part number synchronization system for managing 8-digit part numbers across 3 company locations with 2 SQL Server databases.

**Architecture:**
- **Locations 1 & 2**: Share one SQL Server (Primary)
- **Location 3**: Has its own SQL Server (Secondary)

## ✨ Key Features

- ✅ Real-time duplicate prevention across both SQL Servers
- ✅ Automatic synchronization between servers
- ✅ Manual part number entry with instant validation
- ✅ Excel/CSV import for bulk data migration
- ✅ Search functionality across all locations
- ✅ Full audit trail and history tracking
- ✅ Web-based interface - no installation required
- ✅ 8-digit numeric format validation

## 🚀 Quick Start

### Prerequisites

- SQL Server 2016+ (both locations)
- .NET 8.0 SDK
- Web browser (Chrome, Edge, Firefox)

### Installation Steps

1. **Clone the repository:**
```bash
git clone https://github.com/Bigschack77/part-number-sync-system.git
cd part-number-sync-system
```

2. **Set up databases on BOTH SQL Servers:**
```sql
CREATE DATABASE PartNumberDB;
GO
USE PartNumberDB;
GO
```

3. **Run the SQL scripts in order on both servers:**
   - Execute Database/01_CreateTables.sql
   - Execute Database/02_SyncProcedures.sql
   - Execute Database/03_SyncConfiguration.sql

4. **Update connection strings in PartNumberAPI/appsettings.json**

5. **Run the API:**
```bash
cd PartNumberAPI
dotnet restore
dotnet build
dotnet run
```

## 📖 Usage Guide

### Creating Part Numbers

1. Navigate to Create Part Number tab
2. Enter 8-digit part number
3. System checks both servers for duplicates in real-time
4. Fill in description and select location
5. Click Create Part Number
6. Part is saved and synced to both servers automatically

### Importing from Excel

1. Prepare Excel with columns: PartNumber, Description, Location
2. Save as CSV
3. Use the Import tab to upload
4. Review import results

### Searching

Search across all locations and both SQL Servers simultaneously.

## 🛡️ Data Validation

- Must be exactly 8 digits
- Numeric only
- Unique across all 3 locations
- Real-time duplicate checking

## 📊 Database Schema

### Tables
- **PartNumbers**: Main part number storage
- **SyncLog**: Synchronization tracking
- **PartNumberHistory**: Complete audit trail
- **SyncConfiguration**: Multi-server configuration

## 🔧 How Sync Works

1. User enters part number at any location
2. API checks BOTH SQL Servers for duplicates
3. If unique: Saves to appropriate server and replicates to the other
4. If duplicate: Shows error with location details

## 🎁 Business Benefits

- ✅ Eliminates duplicate part number errors
- ✅ Reduces manual coordination between locations
- ✅ Speeds up part number creation process
- ✅ Provides instant visibility across all locations
- ✅ Maintains data integrity automatically

## 📞 Support

For questions or issues, create an issue on GitHub.

## 📄 License

Internal use only - Company Confidential

---

**Version**: 1.0  
**Last Updated**: 2026-01-20 13:42:47  
**Created By**: Bigschack77