# FaithTrack

**A Web-Based Faith Resource Management System**

> CST-452 Senior Capstone Project II — Grand Canyon University
> Matthew Kollar | Instructor: Professor Michael Landreth | 2026

FaithTrack gives authenticated users a centralized platform to create, organize, search, and manage faith-based resources like Bible study guides, sermon notes, prayer journals, and reading plans — all in one secure, structured web application.

---

## Live Demonstrations

| Sprint | Link | Covers |
|---|---|---|
| Sprint 1 | https://somup.com/cOnuVPWJ0h | Login, Dashboard, Resource CRUD (US 1–7) |
| Sprint 2 | https://somup.com/cOfjo7VcHgB | Categories, Search, Filter, Registration (US 8–14) |

---

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 8) |
| ORM | Entity Framework Core |
| Database | SQL Server Express / LocalDB |
| Authentication | ASP.NET Core Identity |
| Frontend | Razor Views, Bootstrap 5 |
| IDE | Visual Studio 2022 |
| Web Server | Kestrel (local development) |

---

## Architecture Overview

FaithTrack enforces a strict N-Layer architecture with full separation of concerns. Controllers never touch the database directly. Services contain all business logic. Repositories abstract all data access. Every layer is wired through dependency injection.

```
┌─────────────────────────────────────────────────────────┐
│                   PRESENTATION LAYER                    │
│  ResourceController  CategoryController  AccountController │
│  Razor Views (.cshtml)   Bootstrap 5   ViewModels       │
└───────────────────────────┬─────────────────────────────┘
                            │ (calls service interfaces)
┌───────────────────────────▼─────────────────────────────┐
│                   APPLICATION LAYER                     │
│  IResourceService / ResourceService                     │
│  ICategoryService / CategoryService                     │
│  ASP.NET Core Identity (auth & authorization)           │
└───────────────────────────┬─────────────────────────────┘
                            │ (calls repository interfaces)
┌───────────────────────────▼─────────────────────────────┐
│                   DATA ACCESS LAYER                     │
│  IResourceRepository / ResourceRepository               │
│  ICategoryRepository / CategoryRepository               │
│  FaithTrackDbContext (EF Core)                          │
└───────────────────────────┬─────────────────────────────┘
                            │ (EF Core migrations)
┌───────────────────────────▼─────────────────────────────┐
│                      DATABASE                           │
│  SQL Server Express / LocalDB                           │
│  Tables: Users, Roles, UserRoles, Resources, Categories │
└─────────────────────────────────────────────────────────┘
```

---

## Implementation Approach

Development followed an **Agile Scrum methodology** across three sprints spanning CST-451 and CST-452. The full architecture was designed before any code was written (Milestone 3 Architecture Plan), so every implementation decision traces back to a documented design artifact.

Key decisions:

- **Repository pattern** abstracts EF Core from the service layer, making data access independently testable and swappable
- **ASP.NET Core Identity** used instead of custom auth to leverage proven PBKDF2 password hashing and reduce security risk
- **Interface-based dependency injection** throughout — every service and repository is registered against its interface, never its concrete type
- **Async/await** on every database operation to avoid blocking the Kestrel web server
- **[Authorize] on all CRUD routes** keeps security at the infrastructure level rather than scattered through method bodies

---

## Code Snippets

### Repository Pattern

The repository abstracts all database operations behind an interface. The service layer never references EF Core directly.

```csharp
public class ResourceRepository : IResourceRepository
{
    private readonly FaithTrackDbContext _context;

    public ResourceRepository(FaithTrackDbContext context)
    {
        _context = context;
    }

    /// <summary>Retrieves all resources with their category.</summary>
    public async Task<IEnumerable<Resource>> GetAllAsync()
    {
        return await _context.Resources
            .Include(r => r.Category)
            .ToListAsync();
    }

    /// <summary>Adds a new resource and persists changes.</summary>
    public async Task AddAsync(Resource entity)
    {
        await _context.Resources.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>Deletes a resource by ID if it exists.</summary>
    public async Task DeleteAsync(int id)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource != null)
        {
            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();
        }
    }
}
```

### Service Layer — Business Logic and Validation

The service layer maps ViewModels to domain entities and enforces business rules before delegating to the repository.

```csharp
public class ResourceService : IResourceService
{
    private readonly IResourceRepository _resourceRepo;
    private readonly ICategoryRepository _categoryRepo;

    public ResourceService(IResourceRepository resourceRepo,
                           ICategoryRepository categoryRepo)
    {
        _resourceRepo = resourceRepo;
        _categoryRepo = categoryRepo;
    }

    /// <summary>Creates a new resource after validating the view model.</summary>
    public async Task<bool> CreateResourceAsync(ResourceViewModel vm)
    {
        if (!ValidateResource(vm)) return false;

        var entity = MapToEntity(vm);
        await _resourceRepo.AddAsync(entity);
        return true;
    }

    /// <summary>Validates required fields on the resource view model.</summary>
    private bool ValidateResource(ResourceViewModel vm)
    {
        return !string.IsNullOrWhiteSpace(vm.Title) && vm.CategoryId > 0;
    }

    /// <summary>Maps a ResourceViewModel to a Resource domain entity.</summary>
    public Resource MapToEntity(ResourceViewModel vm)
    {
        return new Resource
        {
            ResourceId      = vm.ResourceId,
            Title           = vm.Title,
            Description     = vm.Description,
            Url             = vm.Url,
            CategoryId      = vm.CategoryId,
            CreatedByUserId = vm.CreatedByUserId,
            CreatedDate     = vm.CreatedDate == default ? DateTime.UtcNow : vm.CreatedDate,
            UpdatedDate     = DateTime.UtcNow
        };
    }
}
```

### Category Delete — Cascade Protection

The most interesting challenge: preventing deletion of a category that still has resources assigned, without letting the FK constraint surface as an unhandled SQL exception.

```csharp
/// <summary>
/// Deletes a category only if no resources are assigned to it.
/// Returns a descriptive error if resources still reference the category.
/// </summary>
public async Task<(bool Success, string ErrorMessage)> DeleteCategoryAsync(int id)
{
    var resourceCount = await _context.Resources
        .CountAsync(r => r.CategoryId == id);

    if (resourceCount > 0)
    {
        return (false, $"Cannot delete this category — {resourceCount} resource(s) " +
                       "are still assigned to it. Reassign or delete those resources first.");
    }

    var category = await _context.Categories.FindAsync(id);
    if (category == null) return (false, "Category not found.");

    _context.Categories.Remove(category);
    await _context.SaveChangesAsync();
    return (true, string.Empty);
}
```

### Controller — Authorization and Dependency Injection

```csharp
[Authorize]
public class ResourceController : Controller
{
    private readonly IResourceService _resourceService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ResourceController(IResourceService resourceService,
                              UserManager<ApplicationUser> userManager)
    {
        _resourceService = resourceService;
        _userManager     = userManager;
    }

    /// <summary>Displays the resource list for the current authenticated user.</summary>
    public async Task<IActionResult> Index()
    {
        var userId    = GetCurrentUserId();
        var resources = await _resourceService.GetAllResourcesAsync(userId);
        return View(resources);
    }

    /// <summary>Handles POST to create a new resource.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResourceViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        vm.CreatedByUserId = GetCurrentUserId();
        await _resourceService.CreateResourceAsync(vm);
        return RedirectToAction(nameof(Index));
    }

    private string GetCurrentUserId() => _userManager.GetUserId(User);
}
```

---

## Database Schema

```sql
CREATE TABLE Categories (
    CategoryId  INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500) NULL
);

CREATE TABLE Resources (
    ResourceId      INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(200) NOT NULL,
    Description     NVARCHAR(MAX) NULL,
    Url             NVARCHAR(500) NULL,
    CategoryId      INT NOT NULL,
    CreatedByUserId INT NOT NULL,
    CreatedDate     DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedDate     DATETIME2 NULL,
    CONSTRAINT FK_Resources_Categories FOREIGN KEY (CategoryId)
        REFERENCES Categories(CategoryId) ON DELETE NO ACTION,
    CONSTRAINT FK_Resources_Users FOREIGN KEY (CreatedByUserId)
        REFERENCES Users(UserId) ON DELETE NO ACTION
);
```

---

## Features

- **Secure Authentication** — Login, logout, registration with PBKDF2 password hashing and cookie-based sessions
- **Resource Management** — Full CRUD with title, category, description, and optional URL
- **Category System** — Create and manage categories with cascade-protection validation on delete
- **Search and Filter** — Keyword search by title/description and filter by category
- **User Profiles** — View and update account info, change password
- **Data Validation** — Server-side model validation, antiforgery tokens on all POST actions, inline error messages

---

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server Express or LocalDB
- Visual Studio 2022

### Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/mkollar91/FaithTrack.git
   ```

2. Open `FaithTrack.sln` in Visual Studio 2022.

3. Update the connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FaithTrackDb;Trusted_Connection=True;"
   }
   ```

4. Apply EF Core migrations in the Package Manager Console:
   ```
   Update-Database
   ```

5. Run the application:
   ```bash
   dotnet run
   ```

6. Navigate to `https://localhost:7192` and register a new account to get started.

---

## Project Structure

```
FaithTrack/
├── Controllers/          # Presentation Layer
│   ├── ResourceController.cs
│   ├── CategoryController.cs
│   ├── AccountController.cs
│   └── HomeController.cs
├── Views/                # Razor Views
│   ├── Resources/        # Index, Create, Edit, Details, Delete
│   ├── Categories/       # Index, Create, Edit, Delete
│   ├── Account/          # Login, Register, Profile
│   ├── Home/             # Dashboard
│   └── Shared/           # _Layout.cshtml
├── Services/             # Application Layer
│   ├── IResourceService.cs / ResourceService.cs
│   └── ICategoryService.cs / CategoryService.cs
├── Repositories/         # Data Access Layer
│   ├── IResourceRepository.cs / ResourceRepository.cs
│   └── ICategoryRepository.cs / CategoryRepository.cs
├── Models/               # Domain Entities
│   ├── Resource.cs
│   └── Category.cs
├── ViewModels/
│   ├── ResourceViewModel.cs
│   ├── CategoryViewModel.cs
│   ├── LoginViewModel.cs
│   └── RegisterViewModel.cs
├── Data/
│   ├── FaithTrackDbContext.cs
│   └── Migrations/
└── wwwroot/              # Static Assets (Bootstrap, CSS, JS)
```

---

## Sprint Summary

| Sprint | Focus | Stories | Points | Status |
|---|---|---|---|---|
| Sprint 1 | Auth, Dashboard, Resource CRUD | US 1–7 | 21 | ✅ Complete |
| Sprint 2 | Categories, Search, Filter, Registration | US 8–14 | 21 | ✅ Complete |
| Sprint 3 | Profile, Password, Nav, Validation, Tests | US 15–20 | 15 | ✅ Complete |
| **Total** | | **20 / 20** | **57 / 57** | **✅ 100%** |

---

## Documentation

| Document | Description |
|---|---|
| `CST-451_Milestone1_ProjectProposal_Kollar.pdf` | Original project proposal |
| `CST-451_Milestone3_ArchitecturePlan_FaithTrack_MatthewKollar.pdf` | Architecture plan with UML diagrams, wireframes, ER diagram |
| `FaithTrack_Milestone6_Final.docx` | Final sprint backlog, test cases, traceability matrix |
| `FaithTrack_RequirementsReview.docx` | Requirements met/not met and planned improvements |
| `FaithTrack_ShowcasePoster.html` | Capstone showcase poster |

---

## Author

**Matthew Kollar**
CST-452 Senior Capstone Project II — Grand Canyon University
Instructor: Professor Michael Landreth
mkollar@my.gcu.edu
