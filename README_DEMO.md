# Part Number Sync System - Interactive Demo Guide

## 🎯 Overview

The Part Number Synchronization System is a robust enterprise solution designed to maintain consistency and accuracy across multiple locations and database servers. This demo guide will help you understand and demonstrate the system's capabilities.

## 🏗️ System Architecture

### Multi-Server Architecture
The system operates across **two SQL Server databases**:
- **Primary Server**: Handles Location 1 and Location 2
- **Secondary Server**: Handles Location 3

### Key Components
1. **PartNumberAPI** (.NET 8 Web API)
   - RESTful endpoints for CRUD operations
   - Cross-server synchronization logic
   - Swagger UI for API testing
   - CORS enabled for web client access

2. **Database Layer**
   - PartNumbers table (main data storage)
   - SyncLog table (tracks synchronization activities)
   - PartNumberHistory table (audit trail)
   - Stored procedures for business logic

3. **Web UI**
   - Interactive dashboard
   - Create, search, and import part numbers
   - Real-time validation feedback

## 🔄 How Synchronization Works

### The Sync Process (Step-by-Step)

1. **User Creates Part Number**
   - User submits a new part number through the UI or API
   - System determines target server based on location:
     - Location 1 or 2 → Primary Server
     - Location 3 → Secondary Server

2. **Duplicate Check (Both Servers)**
   - API calls `sp_CheckPartNumberExists` on BOTH servers
   - Prevents duplicate part numbers across the entire system
   - Returns error if part number already exists anywhere

3. **Insert on Target Server**
   - Validates format (must be 8 digits)
   - Inserts into PartNumbers table
   - Logs action to PartNumberHistory
   - Creates SyncLog entry with SUCCESS status

4. **Automatic Replication**
   - System automatically replicates to the other server
   - Ensures both servers have complete data
   - Marks replicated entries as "(Synced)" in CreatedBy field

5. **Result**
   - User receives confirmation
   - Part number is now available on both servers
   - Searchable from any location

### Example Sync Flow

```
User creates Part #12345678 at Location 1
    ↓
Primary Server: Check if exists → Not found
Secondary Server: Check if exists → Not found
    ↓
Primary Server: Insert part number → SUCCESS
    ↓
Secondary Server: Replicate part number → SUCCESS
    ↓
Part #12345678 now exists on BOTH servers
```

## 🎨 Demo Scenarios

### Scenario 1: Creating a New Part Number

**Objective**: Show how the system validates and creates a part number

1. Open the interactive demo (demo.html)
2. Navigate to "Create Part Number" section
3. Enter part number: `12345678`
4. Select location: `Location 1`
5. Add description: `Sample Widget Assembly`
6. Click "Create"
7. **Expected Result**: 
   - Success message displayed
   - Part number appears in both servers
   - Sync log entry created

**Key Points to Highlight**:
- 8-digit validation
- Duplicate prevention across all locations
- Automatic cross-server replication

### Scenario 2: Duplicate Prevention

**Objective**: Demonstrate the system prevents duplicates globally

1. Try to create part number `12345678` again
2. **Expected Result**: Error message stating part already exists
3. Show that it checks BOTH servers before allowing creation

**Key Points to Highlight**:
- Global uniqueness enforcement
- Cross-server validation before insert
- Clear error messaging

### Scenario 3: Search Across All Locations

**Objective**: Show unified search across both servers

1. Go to "Search" section
2. Enter search term: `Widget`
3. **Expected Result**: Results from both Primary and Secondary servers
4. Try filtering by location

**Key Points to Highlight**:
- Searches aggregate data from both servers
- Can filter by location
- Shows where each part is stored

### Scenario 4: Bulk Import from CSV

**Objective**: Demonstrate batch processing capabilities

1. Go to "Import" section
2. Use the sample CSV file (SampleData/sample_import.csv)
3. Click "Import"
4. **Expected Result**:
   - Progress indicator shows processing
   - Success/failure count displayed
   - Detailed results for each part number

**Key Points to Highlight**:
- Validates each entry before import
- Checks for duplicates per item
- Provides detailed feedback for each record
- All imports are synchronized to both servers

### Scenario 5: Real-time Validation

**Objective**: Show instant feedback on data entry

1. In create form, enter invalid part number: `123` (too short)
2. **Expected Result**: Immediate validation error
3. Enter valid format: `12345678`
4. **Expected Result**: Validation passes

**Key Points to Highlight**:
- Client-side validation for immediate feedback
- Server-side validation for security
- Clear format requirements (8 digits)

## 📊 Business Benefits Demonstration

### 1. Data Integrity
- **Demo Point**: Try to create duplicate → System prevents it
- **Benefit**: No conflicting part numbers across locations

### 2. Time Savings
- **Demo Point**: Bulk import 100 parts in seconds
- **Benefit**: Eliminates manual entry across multiple systems

### 3. Audit Trail
- **Demo Point**: Show PartNumberHistory and SyncLog tables
- **Benefit**: Complete visibility into who created/modified what and when

### 4. Error Reduction
- **Demo Point**: Invalid format rejection, duplicate prevention
- **Benefit**: Automated validation eliminates human errors

### 5. Scalability
- **Demo Point**: System handles multiple locations easily
- **Benefit**: Easy to add more locations/servers as business grows

## 🔧 Technical Highlights

### API Endpoints

```
GET  /api/partnumbers/check/{partNumber}
     - Check if part number exists (both servers)

POST /api/partnumbers
     - Create new part number with auto-sync

GET  /api/partnumbers/search?searchTerm=X&location=Y
     - Search across both servers

POST /api/partnumbers/import
     - Bulk import from CSV/Excel
```

### Database Design Features
- Primary keys ensure uniqueness per server
- Indexes on Location and CreatedDate for fast searches
- SyncLog tracks all replication activities
- PartNumberHistory provides complete audit trail

### Synchronization Strategy
- **Active-Active Replication**: Both servers actively accept writes
- **Immediate Consistency**: Changes replicated immediately
- **Conflict Prevention**: Pre-check on both servers before insert
- **Error Handling**: Failed syncs logged for manual review

## 🚀 Running the Demo

### Option 1: Interactive HTML Demo (No Database Required)
```bash
# Open demo.html in a web browser
# This simulates the system without requiring databases
open demo.html
```

### Option 2: Full System Demo (Requires SQL Server)
```bash
# Setup databases
1. Create two SQL Server databases
2. Run Database/01_CreateTables.sql on both
3. Run Database/02_SyncProcedures.sql on both
4. Update appsettings.json with connection strings

# Run the API
cd PartNumberAPI
dotnet run

# Open Web UI
open WebUI/index.html
```

### Option 3: Swagger API Testing
```bash
# Start the API
cd PartNumberAPI
dotnet run

# Navigate to
https://localhost:5001/swagger

# Test endpoints directly through Swagger UI
```

## 📝 Demo Script (5-Minute Presentation)

**Minute 1: Introduction**
- "This is a Part Number Sync System managing parts across 3 locations and 2 servers"
- Show architecture diagram

**Minute 2: Create Demonstration**
- Create a part number for Location 1
- Show it appears on both servers automatically
- Highlight the sync process

**Minute 3: Validation & Error Handling**
- Try to create duplicate → Show error
- Try invalid format → Show validation
- Emphasize data integrity

**Minute 4: Search & Bulk Import**
- Search across all locations
- Import CSV file with multiple parts
- Show batch processing results

**Minute 5: Business Value**
- Recap: Data integrity, time savings, audit trail
- Show SyncLog and PartNumberHistory
- Discuss scalability and future enhancements

## 🎬 Video Demo Outline

1. **Opening Shot**: System architecture overview
2. **Scene 1**: Create part number walkthrough
3. **Scene 2**: Show automatic replication
4. **Scene 3**: Demonstrate duplicate prevention
5. **Scene 4**: Search functionality across servers
6. **Scene 5**: Bulk import demonstration
7. **Scene 6**: Review sync logs and history
8. **Closing**: Benefits summary

## 🛠️ Troubleshooting Demo Issues

**Issue**: Cannot connect to databases
- **Solution**: Use demo.html which simulates behavior without databases

**Issue**: CORS errors in browser
- **Solution**: Ensure API is running and CORS policy is configured

**Issue**: Port conflicts
- **Solution**: Check appsettings.json and change API port if needed

## 📚 Additional Resources

- **API Documentation**: Available at /swagger when API is running
- **Sample Data**: See SampleData/sample_import.csv for import examples
- **Database Schema**: See Database/01_CreateTables.sql for table structure
- **Stored Procedures**: See Database/02_SyncProcedures.sql for business logic

## 🎓 Learning Outcomes

After this demo, stakeholders should understand:
1. How the dual-server architecture maintains data consistency
2. The automatic synchronization mechanism
3. Built-in validation and error prevention
4. Search and import capabilities
5. Audit trail and compliance features
6. Scalability and business benefits

---

**Ready to Demo?** Open `demo.html` and start with Scenario 1!
