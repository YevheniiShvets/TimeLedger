# Project Plan – Advanced Planner Web Application

## 1. Project Overview

### 1.1 Project Title
Advanced Planner Web Application

### 1.2 Project Vision
The goal of this project is to develop a web-based planner application that allows users to efficiently manage scheduled events and, in later iterations, support advanced study planning and productivity tracking features.

### 1.3 Problem Statement
Many students struggle with organizing their time, managing overlapping tasks, and planning study sessions efficiently. This application aims to provide a structured and scalable solution for event scheduling and long-term planning.

---

## 2. Objectives

### 2.1 Primary Objective
To design and implement a scalable web-based planner using an iterative (Agile) development methodology.

### 2.2 Iteration 1 Objective
Deliver a working Event Management system that includes:
- Event CRUD operations
- Time validation
- Overlap detection
- Database persistence
- Basic frontend integration

---

## 3. Scope

### 3.1 In Scope (Iteration 1)
- Event entity
- Create, Edit, Delete events
- View events
- Overlap prevention logic
- Database integration
- Basic UI form

### 3.2 Out of Scope (Iteration 1)
- User authentication
- Multi-user support
- Recurring events
- Notifications
- Analytics
- Study automation

---

## 4. High-Level Feature List (Full Product Vision)

- Event management
- Multiple schedules
- User accounts & authentication
- Recurring events
- Event categories & tags
- Study planning automation
- Notifications & reminders
- Analytics dashboard
- Calendar integrations

---

## 5. Iteration Plan

### Iteration 1 – Core Event Engine
- Event CRUD
- Validation logic
- Overlap detection
- Database integration

### Iteration 2 – User Management
- Registration & login
- JWT authentication
- Event ownership

### Iteration 3 – Advanced Scheduling
- Recurring events
- Categories & filters
- Improved calendar views

### Iteration 4 – Smart Features & Optimization
- Study planning logic
- Notifications
- Analytics dashboard
- UI improvements

---

## 6. Technology Stack

### Backend
- ASP.NET Core (C#)
- Entity Framework Core

### Frontend
- React (Vite + TypeScript)

### Database
- PostgreSQL / SQL Server

### Version Control
- Git + GitHub

---

## 7. Architecture Overview

The system will follow a layered architecture:

Controller → Service → Repository → Database

This ensures:
- Separation of concerns
- Maintainability
- Scalability
- Testability

---

## 8. Risk Assessment

| Risk | Mitigation |
|------|------------|
| Scope creep | Strict iteration boundaries |
| Over-engineering | Focus only on core entity in Iteration 1 |
| Time constraints | Prioritize MVP features |

---

## 9. Definition of Done (Iteration 1)

Iteration 1 is considered complete when:

- Event CRUD works
- Overlapping events are prevented
- Data is stored in the database
- Basic frontend interaction is functional
- Code is committed with proper version control
