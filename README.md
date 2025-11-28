# 🎟️ TicketFlow Enterprise

> A High-Concurrency Event Ticket Booking System built with .NET 8, Clean Architecture, and Cloud-Native technologies.

## 🚀 Introduction
TicketFlow is an enterprise-grade solution designed to handle high traffic loads (Flash Sales) for booking event tickets. The project demonstrates advanced software architecture patterns including **Microservices**, **CQRS**, **Event Sourcing**, and **Distributed Locking**.

## 🛠️ Tech Stack & Architecture

### Backend Core
- **Framework:** .NET 8 (C#)
- **Architecture:** Clean Architecture (Onion Architecture)
- **Patterns:** CQRS (MediatR), Repository, Unit of Work, Factory.
- **ORM:** Entity Framework Core (Code-First).

### Infrastructure & DevOps
- **Database:** SQL Server 2022 (Dockerized).
- **Caching:** Redis (Distributed Cache & Locking).
- **Containerization:** Docker & Docker Compose.
- **Proxy:** YARP (Yet Another Reverse Proxy) - *Coming Soon*.

### Security
- **Auth:** JWT (JSON Web Tokens) with Refresh Token mechanism.
- **Hashing:** BCrypt.

## 📂 Project Structure
- **Core (Domain):** Entities, Value Objects, Domain Exceptions.
- **Application:** Interfaces, Use Cases (CQRS Handlers), DTOs, Validators.
- **Infrastructure:** DbContext, External Services (Email, File Storage).
- **API:** Controllers, Middleware, Entry Point.

## ⚙️ How to Run
1. Ensure you have **Docker Desktop** installed and running.
2. Clone the repository.
3. Start Infrastructure (SQL Server, Redis):
   ```bash
   docker-compose up -d
