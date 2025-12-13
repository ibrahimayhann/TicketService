 Ticket Management API

Ticket Management API, destek ve iş takip süreçlerini yönetmek için geliştirilmiş, ASP.NET Core Web API tabanlı bir backend servisidir.
Proje; katmanlı mimari, temiz kod prensipleri ve kurumsal backend standartları gözetilerek geliştirilmiştir.



 Features

Ticket oluşturma, listeleme, güncelleme ve silme (CRUD)

Durum (Status) ve öncelik (Priority) yönetimi

Sayfalama (Pagination) ve filtreleme altyapısı

Global exception handling

Merkezi validation yapısı

RESTful API tasarımı

Swagger (OpenAPI) entegrasyonu




 Architecture

Proje Layered Architecture yaklaşımıyla geliştirilmiştir:

TicketApi
│
├── Controllers       → API endpoints
├── Services          → Business logic
├── Repositories      → Data access layer
├── Entities          → Database models
├── DTOs
│   ├── Request
│   └── Response
├── Exceptions        → Global error handling
├── Enums             → Status, Priority vb.
└── Configurations    → EF Core & app configurations



Technologies

ASP.NET Core 7

Entity Framework Core

MSSQL

FluentValidation

Swagger / OpenAPI

RESTful API principles
 Sample Endpoints
GET    /api/tickets
GET    /api/tickets/{id}
POST   /api/tickets
PUT    /api/tickets/{id}
DELETE /api/tickets/{id}


 Getting Started
1️⃣ Clone repository
git clone https://github.com/ibrahimayhann/TicketService.git
cd ticket-management-api

2️⃣ Database configuration

appsettings.json içerisine connection string ekleyin:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TicketDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}

3️⃣ Run the application
dotnet restore
dotnet run



Swagger arayüzü:

https://localhost:5001/swagger

 Design Decisions

DTO kullanımı ile Entity – API katmanı ayrıldı

FluentValidation ile merkezi doğrulama

Global Exception Middleware ile tutarlı error response yapısı

EF Core Fluent API ile entity ilişkileri yönetildi



 Purpose of the Project

Bu proje;

Spring Boot background’una sahip bir geliştiricinin

ASP.NET Core ekosistemine adaptasyonunu

kurumsal backend geliştirme pratiğini

göstermek amacıyla geliştirilmiştir.



 Possible Improvements

Authentication & Authorization (JWT)

Role-based access control

Logging (Serilog)

Dockerization



SQL Reports

Bu projede, SQL tabanlı raporlama sorguları DbScripts klasörü altında yer almaktadır.
Duruma Göre Ticket Sayısı Raporu:
Ticket kayıtlarının durumlarına göre (Open, InProgress, Resolved, Closed) dağılımını gösteren, GROUP BY kullanılarak oluşturulmuş toplu (aggregate) bir rapor sorgusudur.
Enum değerleri, rapor okunabilirliğini artırmak amacıyla CASE yapısı ile anlamlı durum isimlerine dönüştürülmüştür.


Author

İbrahim Ayhan
Software Engineer