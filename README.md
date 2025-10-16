# AI Healthcare Copilot

## Description

**AI Healthcare Copilot** is a clinical assistant designed to help doctors and healthcare professionals efficiently manage patient records. The system can summarize patient notes, detect missing clinical details, and suggest next steps, streamlining the workflow in hospitals and clinics.

## Tech Stack

- **.NET 9** (Blazor Server for interactive dashboards)
- **ML.NET** and **Azure AI Services** (for text analytics and AI-driven insights)
- **Entity Framework Core** + **SQL Server** (for data storage and management)

## Features Implemented

- Blazor Server dashboard for doctors and clinicians
- Paste or upload patient notes to generate:
  - Summarized patient records
  - Diagnostic checklists
  - Suggestions for missing details or next steps
- Integration with Azure AI Services for advanced text analytics
- Secure data storage using EF Core and SQL Server

## Example Use Case

> **HealthConnect AI Assistant**:  
> A doctor logs into the dashboard, pastes a patient’s notes, and instantly receives a summary, a diagnostic checklist, and AI-powered suggestions for further action.

## Getting Started

1. Clone the repository: git clone https://github.com/SnehilKosmetty/AI_HealthcareCopilot.git
2. Open the solution in Visual Studio 2022.
3. Ensure you have .NET 9 SDK installed.
4. Configure your SQL Server connection string in `appsettings.json`.
5. Run the project.

## Future Plans

- Expand AI capabilities for more nuanced clinical suggestions
- Add user authentication and role-based access
- Integrate with EHR systems for real-time data sync
- Improve UI/UX for mobile and tablet devices

---

*This project is for research and prototyping purposes. Not for use in production clinical environments without further validation.*
