# BikeService — Developer & User Manual

## Table of Contents
1. [System Overview](#1-system-overview)
2. [Roles & Access](#2-roles--access)
3. [First-Time Setup](#3-first-time-setup)
4. [Customer Workflows](#4-customer-workflows)
5. [Mechanic Workflows](#5-mechanic-workflows)
6. [Admin Workflows](#6-admin-workflows)
7. [End-to-End Workflows](#7-end-to-end-workflows)
8. [Business Rules Reference](#8-business-rules-reference)
9. [Notification Triggers](#9-notification-triggers)
10. [Technical Reference](#10-technical-reference)

---

## 1. System Overview

BikeService is a web-based platform for managing a bike repair and service shop. It handles the full lifecycle of a service job:

```
Customer books appointment
    → Admin confirms appointment
        → Admin creates service ticket from appointment
            → Mechanic diagnoses & repairs
                → Admin generates & issues invoice
                    → Customer pays online
                        → Ticket marked Delivered
```

**Tech stack:** ASP.NET Core 8 MVC, Entity Framework Core 8, SQL Server, ASP.NET Identity, Redis (notifications cache), Bootstrap 5.

---

## 2. Roles & Access

### Admin
Full control over the shop's operations.

| Area | Capabilities |
|---|---|
| Service Tickets | View all, create, reassign mechanic, advance/cancel status, generate invoice |
| Invoices | Generate from ticket, issue to customer, void |
| Mechanics | Add, edit, toggle availability, link to user account |
| Service Types | Add, edit, toggle active, set base price |
| Parts | Add, edit, adjust stock, bulk CSV import |
| Payment Gateways | Add, edit, toggle active/sandbox, delete — credentials stored encrypted |
| Promo Codes | Create, edit, toggle, delete — track usage |
| Customers | List all, ban, unban |
| Analytics | Dashboard KPIs, revenue chart, top services/mechanics |
| Reports | Revenue, ticket, parts usage reports by date range + CSV export |
| Audit Logs | View all mutations with before/after values |
| Appointments | View and manage all |
| Leave Requests | View all mechanic leave requests; approve or reject with notes |

### Mechanic
Focused view of assigned work only.

| Area | Capabilities |
|---|---|
| Dashboard | KPI overview, recent tickets, quick actions, leave summary |
| Tickets | View only tickets assigned to them (`/Mechanic/Tickets`) |
| Status | Advance one step at a time (cannot skip) |
| Diagnosis | Add diagnosis notes and estimated completion date |
| Items | Add service types and parts to ticket (only while Diagnosed/InProgress) |
| Notes | Send messages to customer on any assigned ticket |
| Leave | Submit leave requests; view own request history; cancel pending requests |

### Customer
Self-service portal for their bikes and service history.

| Area | Capabilities |
|---|---|
| Bikes | Register multiple bikes (make, model, year, photo) |
| Appointments | Book, view upcoming appointments |
| Service Tickets | View their tickets and real-time status progress |
| Notes | Message the mechanic on active tickets |
| Invoices | View invoices, download PDF |
| Payments | Pay invoices online (multi-gateway) with promo code |
| Reviews | Leave one star rating per delivered ticket |
| Notifications | In-app bell + email alerts |

---

## 3. First-Time Setup

### Requirements
- .NET 8 SDK
- SQL Server (SQLEXPRESS)
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

### Configuration (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=BikeServiceDb;Trusted_Connection=True;"
  },
  "Redis": { "ConnectionString": "...", "InstanceName": "BikeService:" },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com", "Port": 587,
    "SenderEmail": "your@gmail.com", "Password": "app-specific-password"
  },
  "Imgbb": { "ApiKey": "..." }
}
```

### Run
```bash
dotnet run --project BikeService.Web
```
On first run, migrations are applied and seed data is inserted automatically.

### Post-Login Redirect
After signing in, users are automatically sent to their role's home:

| Role | Redirected to |
|---|---|
| Admin | `/Admin/Analytics` — analytics dashboard |
| Mechanic | `/Mechanic` — mechanic dashboard |
| Customer | `/` — public homepage |

If the user was trying to access a protected page before logging in, they are returned to that page instead.

### Seeded Accounts

| Role | Email | Password |
|---|---|---|
| Admin | `admin@bikeservice.com` | `Admin@123` |
| Mechanic | `james.wright@bikeservice.com` | `Mechanic@123` |
| Mechanic | `sarah.malik@bikeservice.com` | `Mechanic@123` |
| Mechanic | `tom.nguyen@bikeservice.com` | `Mechanic@123` |
| Customer | `alice.johnson@example.com` | `Customer@123` |
| Customer | `bob.smith@example.com` | `Customer@123` |
| Customer | `carol.white@example.com` | `Customer@123` |
| Customer | `david.brown@example.com` | `Customer@123` |
| Customer | `eva.green@example.com` | `Customer@123` |

Also seeded: 20 parts, 4 service types, 5 promo codes (`WELCOME10`, `SUMMER20`, `LOYAL15`, `FLASH30`, `VIP25`), and a Mock payment gateway.

---

## 4. Customer Workflows

### 4.1 Register a Bike
1. Log in → **My Bikes** → **Add Bike**
2. Enter make, model, year, registration number (optional photo upload)
3. Bike is now available for appointment booking

### 4.2 Book an Appointment
1. **Book Appointment** → select bike, date/time, service type, add notes
2. Appointment created with status `Scheduled`
3. Admin confirms the appointment (status → `Confirmed`) — customer notified

### 4.3 Track Service Progress
1. **My Tickets** → click a ticket
2. Visual timeline shows current status across 6 stages
3. Progress updates in real time as the mechanic advances the ticket
4. Customer receives in-app and email notifications at key milestones

### 4.4 Communicate with Mechanic
1. On the ticket detail page, scroll to **Notes & Communication**
2. Type a message and click **Send**
3. The mechanic sees the note immediately on their portal; they receive a bell notification
4. Notes are visible to both parties, chat-style (your messages on the right)
5. Notes are disabled once the ticket is Delivered or Cancelled

### 4.5 Pay an Invoice
1. When invoice is issued, customer receives a notification
2. Go to **My Invoices** → click **Pay Now** on an Issued invoice
3. Optional: enter a promo code and click **Apply** to preview the discount
4. Select a payment gateway → click **Pay**
5. Redirected to gateway (or instant for Mock gateway)
6. On success: invoice marked Paid, ticket marked Delivered, confirmation shown

### 4.6 Download Invoice PDF
- **My Invoices** → click the download icon (↓) on any invoice row
- Or from the invoice detail page → **Download PDF**
- PDF includes: invoice header, bike info, line items table, tax/discount breakdown, total

### 4.7 Leave a Review
1. After ticket is **Delivered**, a "Leave a Review" button appears on the ticket detail
2. Click → 1–5 star interactive rating + optional comment (500 chars max)
3. One review per ticket — the button disappears after submission
4. Reviews appear on the public homepage testimonials section

---

## 5. Mechanic Workflows

### 5.0 Mechanic Dashboard
- Log in → redirected to **Dashboard** (`/Mechanic`)
- Personalised greeting with time of day and today's date
- **KPI cards:** Active Tickets · Overdue (red highlight) · In Progress · Ready for Pickup (green highlight)
- **Active Tickets** panel lists up to 6 open tickets; click any row to open the ticket detail; "View All" goes to the full list
- **Quick Actions** panel: View All Tickets / My Leave (shows pending count) / Request Leave
- **Recent Leave** panel shows the last 5 leave requests with status badges

### 5.1 View Assigned Tickets
- Dashboard → **View All Tickets** or sidebar → **My Tickets** (`/Mechanic/Tickets`)
- Shows only active tickets assigned to the logged-in mechanic
- Delivered and Cancelled tickets are hidden from this list

### 5.2 Advance Ticket Status
1. Open a ticket → **Advance Status** card shows the next step
2. Click **Mark as [Next Status]** — each click moves exactly one step forward
3. Cannot skip steps or go backward
4. At `ReadyForPickup`: customer receives a notification ("Bike Ready for Pickup")

### 5.3 Add Diagnosis Notes
1. On ticket detail → scroll to **Diagnosis** section
2. Enter diagnosis notes and estimated completion date
3. Click **Save** — notes visible to customer on their ticket detail

### 5.4 Add Service Items or Parts
Available only while status is `Diagnosed` or `InProgress`.

**Add a Service:**
1. Select service type from dropdown (base price auto-fills)
2. Adjust price if needed → click **Add Service**

**Add a Part:**
1. Select part from dropdown (unit price auto-fills; out-of-stock parts are disabled)
2. Enter quantity → click **Add Part**
3. Parts with 0 stock cannot be added

**Remove an item:** Click **Remove** next to any item in the items table.

> When status advances to **InProgress**, all part quantities are deducted from inventory automatically.

### 5.5 Communicate with Customer
1. Scroll to **Notes & Communication** on the ticket detail
2. Type a message → click **Send**
3. Customer receives a bell notification and sees the message on their ticket detail

### 5.6 View Payroll Records
1. Sidebar → **Payroll** (`/Mechanic/Payroll`)
2. KPI cards at the top: **Total Paid** (sum of net pay for Paid records), **Pending** (Draft + Finalized), **Latest Period** (most recent month/year)
3. Table below lists all records with month, year, gross pay, bonus, deductions, net pay, and status badge
4. Read-only — mechanics cannot edit payroll records

### 5.7 Request Leave
1. Sidebar → **My Leave** (`/Mechanic/Leave`) — or use the **Request Leave** quick action on the dashboard
2. Click **+ New Request** → select leave type (Annual, Sick, Personal, Unpaid), start date, end date, optional reason
3. A live duration preview ("3 days") updates as you pick dates
4. Click **Submit Request** — status is immediately `Pending`
5. Pending requests can be cancelled from the My Leave card list
6. Admin approves or rejects; the decision (with any admin notes) appears on the card

> Dates cannot be in the past. Overlapping pending or approved requests are blocked.

---

---

## 6. Admin Workflows

### 6.1 Create a Service Ticket

**From a confirmed appointment (standard flow):**
1. **Admin → Appointments** → open any `Confirmed` appointment
2. Click **Create Service Ticket** in the Actions card
3. Ticket is created in `Pending` status, linked to the appointment's bike
4. Appointment is automatically marked `Completed`
5. Admin is redirected to the new ticket detail to assign a mechanic and proceed

**Walk-in (no appointment):**
1. **Admin → Tickets → New Walk-In** (button in the ticket list header)
2. Select a registered bike from the dropdown (shows Make / Model / Year / Reg No)
3. Optionally assign a mechanic, add diagnosis notes, set an estimated completion date
4. Click **Create Ticket** — ticket created at `Pending` status

### 6.2 Manage the Ticket Lifecycle
Admins can advance status directly or reassign mechanics from the ticket detail page.

Key admin actions on the ticket detail:
- **Reassign Mechanic** — available at any non-terminal status
- **Cancel Ticket** — available up to ReadyForPickup; parts are automatically restocked
- **Generate Invoice** — appears once ticket reaches ReadyForPickup or Delivered (no invoice yet)
- **View Invoice** — appears once an invoice has been generated; links directly to the invoice detail

### 6.3 Invoice Lifecycle (Manual Steps)
```
1. Ticket reaches ReadyForPickup (mechanic advances)
2. Admin: Ticket Detail → Generate Invoice        → status: Draft
3. Admin: Ticket Detail → View Invoice → Issue    → status: Issued (customer notified)
4. Customer pays online                           → status: Paid (ticket → Delivered)
```
- A Draft invoice can be voided
- A Paid invoice cannot be voided

### 6.4 Payment Gateway Management
1. **Admin → Payment Gateways → Add Gateway**
2. Select a **Gateway** (Stripe, SSLCommerz, bKash, SurjoPay, Mock)
3. If the gateway has multiple integration types, a **Service Type** dropdown appears — select the variant
4. Fill in the required credential fields — secrets are encrypted before saving
5. Set a **Display Name** (auto-filled from selection, editable)
6. Toggle **Active** to make it available at checkout
7. Toggle **Sandbox** for test mode

**Available gateways and service types:**

| Gateway | Service Types | Slug stored |
|---|---|---|
| Stripe | Checkout, Payment Intents | `stripe_checkout`, `stripe_payment_intents` |
| SSLCommerz | Hosted Payment, Easy Checkout | `sslcommerz_hosted`, `sslcommerz_easy` |
| bKash | Checkout, Tokenized, Webhook | `bkash_checkout`, `bkash_tokenized`, `bkash_webhook` |
| SurjoPay | Checkout, Seamless | `surjopay_checkout`, `surjopay_seamless` |
| Mock | — (single, no variant) | `mock` |

**Built-in Mock gateway**: No credentials required. Instantly marks payment as successful — use for testing.

> When editing a gateway, leaving a secret field blank keeps the existing encrypted value.

### 6.5 Promo Code Management
1. **Admin → Promo Codes → Create**
2. Set code, discount percent (e.g. 20 = 20% off), max usages, expiry date
3. Toggle active/inactive — inactive codes are rejected at checkout
4. Usage count is tracked automatically — cannot exceed MaxUsages

### 6.6 Parts & Inventory
- **Add Part**: SKU, name, unit price, initial stock, low-stock threshold
- **Edit**: update price, adjust stock quantity
- **Bulk Import**: upload CSV with columns `Name,SKU,UnitPrice,StockQuantity,LowStockThreshold`
- When stock drops below threshold after a service, a **PartStockAlert** is created and admin is notified

### 6.7 Analytics Dashboard
**Admin → Analytics** (`/Admin/Analytics`)

KPI cards (live):
- Total revenue (all time), revenue this month
- Tickets today, pending tickets, active tickets
- Total customers, total bikes

Charts and tables:
- **Monthly Revenue** — bar chart for the last 12 months
- **Top Services** — table showing usage count and revenue per service type
- **Top Mechanics** — ranked list by completed ticket count

### 6.8 Reports & Export
**Admin → Reports** → set date range → click **Generate Report**

Three report sections:
- **Revenue** — total, tax, discounts, paid invoice count; breakdown by service type and by mechanic
- **Tickets** — total, delivered, active, cancelled, overdue count, avg completion days; breakdown by status and mechanic
- **Parts Usage** — parts consumed in period with qty and total value

Each section has an **Export CSV** button that downloads that section as a `.csv` file.

### 6.8b Invoice PDF Download (Admin)
**Admin → Invoices → Detail → Download PDF**
- Opens a print-ready PDF with invoice header, bike info, line items, tax/discount breakdown, and total
- File name: `Invoice-{id}.pdf`
- Also available on the invoice detail page via the **Download PDF** button in the Actions card

### 6.8c Payroll Management
**Admin → Payroll** (`/Admin/Payroll`)

| Action | Steps |
|---|---|
| **Create** | Select mechanic + month/year; enter base salary, bonus, deductions; optional notes. Net pay previewed client-side. Status starts at `Draft`. |
| **Finalize** | Open detail page → **Finalize** — locks amounts; status → `Finalized` |
| **Mark Paid** | Detail page → **Mark Paid** — confirms salary was disbursed; status → `Paid` |
| **Edit** | Only `Draft` records can be edited |
| **Delete** | Only `Draft` records can be deleted |

All status transitions are recorded in the audit log.

### 6.9 Leave Request Management
**Admin → Leave Requests** (`/Admin/LeaveRequest`)

1. The list shows all requests across all mechanics, with mechanic name, dates, type, and status badge
2. Click **View** to open the detail page
3. For `Pending` requests a **Decision** card appears:
   - Enter optional admin notes (reason, instructions, etc.)
   - Click **Approve** or **Reject** — status updates immediately
4. Approved and rejected requests are read-only — no further changes possible

**Status meanings:**

| Status | Meaning |
|---|---|
| Pending | Submitted by mechanic, awaiting admin decision |
| Approved | Admin approved — mechanic's leave is confirmed |
| Rejected | Admin rejected — mechanic is expected to work as normal |
| Cancelled | Mechanic withdrew the request before a decision was made |

> Approving leave does **not** automatically toggle the mechanic's availability. If the mechanic should be excluded from ticket assignments during their leave, go to **Admin → Mechanics → Edit** and deactivate their availability manually.

### 6.10 Audit Logs
**Admin → Audit Logs** — complete history of every mutation in the system.

Each entry records:
- Entity name + action (Create / Update / Delete)
- User who made the change (email + ID)
- IP address and user agent
- Old values and new values as JSON (for Update actions)
- Timestamp

Services that write audit logs: ServiceTicket, Appointment, Part, Invoice, PaymentGateway, PromoCode, Mechanic, BulkImport, LeaveRequest.

---

## 7. End-to-End Workflows

### 7.1 Full Service Ticket Lifecycle

```
[Customer] Books appointment
    ↓
[Admin] Confirms appointment (status: Confirmed)
    ↓
[Admin] Opens appointment → clicks "Create Service Ticket"
        → Ticket created (Pending), Appointment auto-marked Completed
    ↓
[Mechanic] Receives assigned ticket
    ↓
[Mechanic] Inspects bike → advances to Diagnosed
    ↓
[Mechanic] Adds service types and parts → advances to InProgress
           (parts stock deducted automatically)
    ↓
[Mechanic] Completes work → advances to QualityCheck
    ↓
[Mechanic] Final check passes → advances to ReadyForPickup
           (customer notified: "Bike Ready for Pickup")
    ↓
[Admin] Generates invoice (Draft)
    ↓
[Admin] Issues invoice (Issued — customer notified: "Invoice Ready")
    ↓
[Customer] Pays online → payment gateway callback
    ↓
[System] Invoice → Paid, Ticket → Delivered
    ↓
[Customer] Leaves a review (optional)
```

### 7.2 Payment Flow

```
Customer: My Invoices → Pay Now (invoice must be Issued)
    ↓
Checkout page: shows order summary + optional promo code input
    ↓
Customer: applies promo code (GET redirect, discount previewed)
    ↓
Customer: selects gateway → clicks Pay
    ↓
POST /Payment/Initiate:
  - Validates invoice is Issued
  - Applies promo discount to invoice
  - Creates PaymentTransaction (Pending)
  - Calls IPaymentProcessor.InitiateAsync()
  - Returns redirect URL
    ↓
Browser redirected to payment gateway
    ↓
Gateway callback (GET or POST to /Payment/Success):
  - Calls IPaymentProcessor.VerifyAsync()
  - Marks transaction Success
  - Marks invoice Paid
  - Increments PromoCode.UsageCount
  - Marks ticket Delivered (if ReadyForPickup)
  - Sends in-app notification to customer
    ↓
Customer sees success page
```

### 7.3 Stock Management

**Deduction (when ticket moves to InProgress):**
- For each `ServiceTicketItem` with a `PartId`, subtract `Quantity` from `Part.StockQuantity`
- If resulting stock < `LowStockThreshold`, create a `PartStockAlert` and notify admin

**Restock (when ticket is Cancelled):**
- Only restocks if ticket was previously InProgress, QualityCheck, or ReadyForPickup
- Adds back all part quantities from ticket items

### 7.4 Notification Flow

Every significant action sends both an **in-app notification** (bell icon) and an **email**.

| Trigger | Recipient | Message |
|---|---|---|
| Appointment confirmed | Customer | "Your appointment is confirmed" |
| Ticket status changed | Customer | Status-specific message |
| Ticket ReadyForPickup | Customer | "Bike ready for pickup" |
| Invoice issued | Customer | "Invoice ready, please pay" |
| Payment successful | Customer | "Payment confirmed" |
| Low stock alert | Admin | "Part X is low on stock" |
| Mechanic note | Customer | "New message on ticket #N" |
| Customer note | Mechanic | "New message on ticket #N" |

---

## 8. Business Rules Reference

### Appointment
- Status flow: `Scheduled` → `Confirmed` → `Completed` (or `Cancelled`)
- Only one service ticket per appointment — the "Create Service Ticket" button is hidden once a ticket exists
- When a service ticket is created from an appointment, the appointment is automatically marked `Completed`
- `Cancelled` means the appointment never happened; `Completed` means it was fulfilled and work began

### Service Ticket
- Status must advance in sequence: Pending → Diagnosed → InProgress → QualityCheck → ReadyForPickup → Delivered
- `Cancelled` is allowed from InProgress, QualityCheck, or ReadyForPickup only
- Items (services/parts) can only be added or removed while status is `Diagnosed` or `InProgress`
- Parts are deducted from stock when status moves to `InProgress`
- Parts are restocked when ticket is `Cancelled` (if stock was previously deducted)

### Invoice
- Only one invoice per service ticket
- Tax is fixed at 15% of the subtotal, calculated at generation time
- Discount is applied at payment initiation (promo code) — not at generation
- `Draft` → `Issued` transition is manual (admin clicks Issue)
- A `Paid` invoice cannot be voided
- Customers can only view/download their own invoices

### Payment
- Only `Issued` invoices can be paid
- Payment success handler is idempotent — repeated callbacks are safe
- Promo code usage is counted only after a confirmed successful payment
- Failed payments do not prevent a retry — customer can attempt payment again

### Promo Codes
- Code must be active, not expired, and have remaining usages
- Discount is a percentage of the subtotal (before tax) applied as `DiscountAmount`
- One code per payment — applied at initiation, not at checkout view

### Reviews
- Customer can only review tickets assigned to their bikes
- Ticket must be in `Delivered` status
- Maximum one review per ticket per customer

### Ticket Notes
- Customer can only post notes on their own tickets (bike ownership check)
- Mechanic can only post notes on their assigned tickets
- Notes are disabled (form hidden) on Delivered and Cancelled tickets

### Parts
- Out-of-stock parts (StockQuantity = 0) cannot be added to a ticket
- A new low-stock alert is created each time stock drops below threshold after a deduction
- Alerts must be manually resolved by admin

### Leave Requests
- From date must be today or a future date — past dates are rejected
- To date must be on or after From date
- A mechanic cannot have overlapping `Pending` or `Approved` requests for the same date range
- Only `Pending` requests can be approved, rejected, or cancelled
- Mechanic can only cancel their own pending requests
- Approving leave does not automatically change mechanic availability — admin must toggle manually if needed
- All transitions (Create, Approve, Reject, Cancel) are recorded in the audit log

---

## 9. Notification Triggers

### In-App Notifications (bell icon)
Created by `INotificationService.CreateNotificationAsync`. Unread count shown in navbar.
All notifications auto-marked read when customer opens the notification list.

### Email Notifications
Sent by `IEmailService.SendEmailAsync` via Gmail SMTP.

Both channels fire for the same events — the service layer sends both in the same operation.

---

## 10. Technical Reference

### Architecture
```
Domain        — pure entity classes, enums, no logic
Application   — business logic, services, DTOs, interfaces
Infrastructure — EF Core, Identity, email, file storage, payments, PDF
Web           — controllers, Razor views, ViewModels
```
Dependencies flow inward only. Web never references Infrastructure directly.

### Admin Controller Routing
Admin controllers live in `Web/Controllers/Admin/` (no ASP.NET Core Areas). URLs follow the pattern `/Admin/{Controller}/{Action}`, enforced via attribute routing.

Each admin controller carries:
```csharp
[Authorize(Roles = AppRoles.Admin)]
[Route("Admin/[controller]/[action]/{id?}")]
public class AnalyticsController : Controller { ... }
```

Three admin controllers share a class name with customer/mechanic controllers. Those three keep the `Admin` prefix in their class name but use an explicit route to keep the URL clean:

| Class name | Route attribute | URL |
|---|---|---|
| `AdminAppointmentController` | `[Route("Admin/Appointment/[action]/{id?}")]` | `/Admin/Appointment/...` |
| `AdminInvoiceController` | `[Route("Admin/Invoice/[action]/{id?}")]` | `/Admin/Invoice/...` |
| `AdminMechanicController` | `[Route("Admin/Mechanic/[action]/{id?}")]` | `/Admin/Mechanic/...` |

Views for the non-conflicting controllers live in `Views/{ControllerName}/` (e.g. `Views/Analytics/`).
Views for the three conflicting ones live in `Views/AdminAppointment/`, `Views/AdminInvoice/`, `Views/AdminMechanic/`.

### Layouts
| Layout | Used by |
|---|---|
| `Views/Shared/_Layout.cshtml` | Public pages (Home, Offers) and Account pages — includes the main navbar |
| `Views/Shared/_CustomerLayout.cshtml` | Customer portal — white sidebar, indigo accent, notification bell, collapsible |
| `Views/Shared/Admin/_AdminLayout.cshtml` | All Admin controllers — dark sidebar, SweetAlert2 toasts |
| `Views/Shared/_MechanicLayout.cshtml` | Mechanic portal — dark-green sidebar, notification bell, SweetAlert2 toasts |

Each portal layout (Customer, Admin, Mechanic) is isolated from the public navbar. Navigation within each portal is handled entirely by its own sidebar. Customer portal views opt in via a `_ViewStart.cshtml` in each view folder pointing to `_CustomerLayout.cshtml`.

### Key Service Files
| Service | Responsibility |
|---|---|
| `ServiceTicketService` | Status workflow, stock deduction/restock, mechanic assignment |
| `InvoiceService` | Generate, issue, void, customer-scoped queries |
| `PaymentService` | Initiate, handle success/cancel, promo application |
| `ReportService` | Revenue, ticket, parts usage reports + CSV export |
| `TicketNoteService` | Add notes, ownership enforcement, cross-party notifications |
| `ReviewService` | Create review, validate ownership + status |
| `DashboardService` | KPI aggregation, monthly revenue chart |
| `AuditLogService` | Write audit entries, paginated query |
| `NotificationService` | Create, read, mark-read in-app notifications |
| `PartService` | CRUD, stock adjustment, low-stock alert creation |
| `LeaveRequestService` | Submit, approve, reject, cancel; overlap guard; audit logging |

### Adding a New Entity (Checklist)
1. `Domain/Entities/NewEntity.cs` — extends `BaseEntity`
2. `AppDbContext` — add `DbSet<NewEntity>`
3. `Infrastructure/Persistence/Configurations/NewEntityConfiguration.cs`
4. `dotnet ef migrations add <Name> --project BikeService.Infrastructure --startup-project BikeService.Web`
5. `Application/DTOs/NewEntity/NewEntityDto.cs`
6. `Application/Interfaces/Services/INewEntityService.cs`
7. `Application/Services/NewEntityService.cs`
8. Register in `ApplicationServiceRegistration.cs`
9. `Application/Mappers/NewEntityMapper.cs`
10. `Web/Controllers/Admin/NewEntityController.cs` — add `[Authorize(Roles = AppRoles.Admin)]` + `[Route("Admin/[controller]/[action]/{id?}")]`
11. `Web/ViewModels/NewEntity/NewEntityFormViewModel.cs`
12. `Web/ViewModels/Mappers/NewEntityViewModelMapper.cs` (only if ViewModel differs from DTO)
13. `Web/Views/NewEntity/` (Index, Create, Edit)
