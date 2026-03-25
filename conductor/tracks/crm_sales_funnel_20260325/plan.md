# Implementation Plan: CRM & Sales Funnel (Captación y Presupuestos)

## Phase 1: Inbound Leads & Real-time Alerts
- [x] Task: Create database schema for Leads 6dc0470 (Name, Email, Phone, Message, Status, Source).
    - [ ] Create DB Migration for `Lead` entity.
    - [ ] Implement `LeadRepository`.
- [x] Task: Develop Public Lead API 0bd072d and Confirmation Workflow.
    - [ ] Create `PublicController` with `/leads` endpoint (Rate Limited).
    - [ ] Implement `LeadService` with automated email confirmation.
- [x] Task: Implement Real-time Lead Notifications 0bd072d.
    - [ ] Update SignalR Hub to notify Igor of new leads.
    - [ ] Create UI Toast notification for new leads.
- [x] Task: Conductor - User Manual Verification 'Phase 1: Inbound' 4d181a7 (Protocol in workflow.md)

## Phase 2: Sales Pipeline Management (CRM UI)
- [x] Task: Build CRM Dashboard and Pipeline View c49d0f4
    - [x] Create `LeadsPage.razor` with a visual pipeline (Kanban or List).
    - [x] Implement lead status update logic (Drag & Drop or Select).
- [x] Task: Implement Lead Detail and Commercial Notes. c3d7e49
    - [ ] Create detail view for each lead with history of interactions.
    - [ ] Add `LeadHistory` entity to track status changes automatically.
- [x] Task: Conductor - User Manual Verification 'Phase 2: CRM UI' c3d7e49 (Protocol in workflow.md)

## Phase 3: Quote Engine (Motor de Presupuestos)
- [ ] Task: Develop `QuoteService` with dual-costing logic.
    - [ ] Implement fixed price calculation.
    - [ ] Implement dynamic costing (Fabric + Labor + Margin) integration.
- [x] Task: Implement Quote PDF Generation. eb6c241
    - [ ] Use QuestPDF to generate professional quotes in PT-BR.
- [x] Task: Quote Management UI. eb6c241
    - [ ] Interface to create, view, and download quotes for a specific Lead.
- [x] Task: Conductor - User Manual Verification 'Phase 3: Quotes' eb6c241 (Protocol in workflow.md)

## Phase 4: Final Integration & Polish
- [ ] Task: End-to-end testing of the lead-to-quote flow.
    - [ ] Ensure proper validation and error handling in PT-BR.
- [ ] Task: Conductor - User Manual Verification 'Phase 4: Final Integration' (Protocol in workflow.md)