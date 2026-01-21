# Part Number Synchronization System

## 🚀 Quick Demo

**Want to see how this works?** 

👉 **[Open demo.html](demo.html)** for an instant interactive demonstration (no setup required!)  
📖 **[Read the Quick Start Demo Guide](QUICK_START_DEMO.md)** for demo scenarios and tips  
📚 **[Read the Full Demo Documentation](README_DEMO.md)** for architecture and detailed explanations

## Overview
The Part Number Synchronization System is designed to streamline and automate the process of syncing part numbers across various platforms and databases, ensuring consistency and accuracy in parts management.

## Features
- **Real-time Synchronization**: Automatic updates across all platforms when a part number changes.
- **Error Handling**: Robust error handling to manage discrepancies in part number entries.
- **User-friendly Interface**: Easy-to-navigate dashboard for monitoring sync status and logs.
- **API Integration**: Seamless integration with existing systems via APIs.

## Installation Steps
1. Clone the repository: `git clone https://github.com/Bigschack77/part-number-sync-system.git`
2. Navigate to the project directory: `cd part-number-sync-system`
3. Install the required dependencies: `npm install`
4. Configure the environment variables required for proper database connections.
5. Run the application: `npm start`

## Usage Guide
- To initiate synchronization, use the command: `sync start`.
- For viewing logs, access the logs section on the dashboard.
- Configure sync settings via the settings menu in the application.

## Database Schema
The system utilizes a relational database with the following key tables:
- **Parts**: Holds details like part number, description, and specifications.
- **Sync Logs**: Tracks the sync operations, including timestamps and success/failure statuses.

## Sync Mechanism
The synchronization mechanism uses a job scheduler to monitor part number changes at regular intervals and updates the databases accordingly. Notifications are sent out if any syncing issues occur.

## Business Benefits
- Maintains data integrity across systems.
- Reduces manual entry errors, leading to fewer discrepancies.
- Saves time with automated updates, allowing businesses to focus on growth rather than data management.

---