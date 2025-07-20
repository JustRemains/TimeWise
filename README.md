# TimeWise

TimeWise – Appointment Scheduler
A desktop-based appointment scheduler developed in C# for Windows, designed to help users manage their time efficiently with a simple, intuitive interface.

## 📋 Overview
TimeWise is a local appointment scheduling application that provides reliable time management without requiring internet access or subscription services. Perfect for individuals, freelancers, and small businesses who need a straightforward scheduling solution.

## ✨ Features
### Core Functionality
- ✅ Create Appointments - Add appointments with title, description, date, start/end time
- ✅ View Schedules - Switch between daily and weekly calendar views
- ✅ Edit & Delete - Modify or remove existing appointments
- ✅ Local Storage - All data stored securely on your local machine
- ✅ Data Persistence - Appointments saved automatically between sessions
  
### Advanced Features
- 🔍 Search Functionality - Find appointments by keyword, date, or time
- 🎨 Color-Coded Categories - Organize appointments with visual labels (Work, Personal, Medical, etc.)
- 📊 CSV Export - Export appointment data for backup or sharing
- 🔔 Reminder Notifications - Configurable alerts before appointments
- ⚙️ Customizable Settings - Personalize default views and notification preferences
  
## 🏗️ Technical Architecture
- Framework: Microsoft .NET with WPF (Windows Presentation Foundation)
- Language: C#
- Database: SQLite (embedded, lightweight)
- ORM: Entity Framework Core
- Pattern: Model-View-ViewModel (MVVM)
- Platform: Windows Desktop

## 🚀 Installation
### Prerequisites
- Windows 10 or later
- .NET 6.0 Runtime or later

## Setup
1. Download the latest release from the releases page
2. Extract the application files to your desired location
3. Run TimeWise.exe to start the application
4. No additional setup or configuration required!

## 📖 Usage
### Getting Started
1. Launch TimeWise - Double-click the application icon
2. Create Your First Appointment - Click "New Appointment" and fill in the details
3. Choose Your View - Switch between Daily and Weekly views using the view toggle
4. Navigate Dates - Use navigation buttons or date picker to browse your schedule

### Managing Appointments
- Add: Click "New Appointment" button or use Ctrl+N
- Edit: Double-click an existing appointment or right-click and select "Edit"
- Delete: Right-click an appointment and select "Delete" (with confirmation)
- Search: Use the search bar to quickly find specific appointments

### Organization Features
- Categories: Assign color-coded categories when creating appointments
- Export: Use File → Export to CSV to backup your data
- Settings: Access Settings to customize notifications and default views
  
## 🗂️ Data Management
- Local Storage: All appointments stored in local SQLite database
- Privacy: No cloud storage - your data stays on your machine
- Backup: Use CSV export feature to create backups
- Data Location: Database file stored in application directory
  
## 🔧 Development
### Project Structure
```
TimeWise/
├── Models/          # Data models and business logic
├── ViewModels/      # MVVM view models
├── Views/           # WPF user interface
├── Services/        # Data access and business services
├── Database/        # Entity Framework context and migrations
└── Resources/       # UI resources and assets
```
## Development Timeline
This project follows Agile/Scrum methodology with a 7-week development cycle:

- [ ] Sprint 1 (Weeks 1-2): Foundation and Core Functionality
- [ ] Sprint 2 (Weeks 3-4): Enhanced Views and Basic Operations
- [ ] Sprint 3 (Weeks 5-6): Search, Categories, and Data Export
- [ ] Sprint 4 (Week 7): Polish, Notifications, and Settings
      
### Build Instructions
```
# Clone the repository
git clone [repository-url]

# Navigate to project directory
cd TimeWise

# Restore dependencies
dotnet restore

# Build the application
dotnet build

# Run the application
dotnet run
```

# 🎯 Product Roadmap
### Current Release (v1.0)

- Core appointment CRUD operations
- Daily and weekly calendar views
- Local data persistence
- Search and categorization
- CSV export capabilities
- Reminder notifications
### Future Enhancements
- Recurring appointments
- Multi-user support
- Calendar synchronization
- Advanced reporting
- Mobile companion app
## 🐛 Troubleshooting
### Common Issues

- Database errors: Ensure the application has write permissions to its directory
- Notifications not working: Check Windows notification settings
- Performance issues: Consider archiving old appointments using CSV export
### Support
For technical support or feature requests, please refer to the project documentation or contact the development team.

# 📄 License
This project is developed as part of IT488 coursework. Please refer to course guidelines for usage and distribution terms.

# 🙏 Acknowledgments
- Built using Microsoft .NET and WPF frameworks
- SQLite for reliable local data storage
- Entity Framework Core for data access
- Windows Presentation Foundation for modern UI
  
Version: 1.0  
Last Updated: July 14, 2025  
Development Approach: Agile/Scrum  
Platform: Windows Desktop  
Framework: .NET with WPF  
