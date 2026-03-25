# Implementation Plan: CRM & Sales Funnel (Captación y Presupuestos)

## Phase 1: Inbound Leads & Real-time Alerts
- [x] Task: Create database schema for Leads 6dc0470 (Name, Email, Phone, Message, Status, Source).
    - [ ] Create DB Migration for `Lead` entity.
    - [ ] Implement `LeadRepository`.
- [ ] Task: Develop Public Lead API and Confirmation Workflow.
    - [ ] Create `PublicController` with `/leads` endpoint (Rate Limited).
    - [ ] Implement `LeadService` with automated email confirmation.
- [ ] Task: Implement Real-time Lead Notifications.
    - [ ] Update SignalR Hub to notify Igor of new leads.
    - [ ] Create UI Toast notification for new leads.
- [ ] Task: Conductor - User Manual Verification 'Phase 1: Inbound' (Protocol in workflow.md)

## Phase 2: Sales Pipeline Management (CRM UI)
- [ ] Task: Build CRM Dashboard and Pipeline View.
    - [ ] Create `LeadsPage.razor` with a visual pipeline (Kanban or List).
    - [ ] Implement lead status update logic (Drag & Drop or Select).
- [ ] Task: Implement Lead Detail and Commercial Notes.
    - [ ] Create detail view for each lead with history of interactions.
- [ ] Task: Conductor - User Manual Verification 'Phase 2: CRM UI' (Protocol in workflow.md)

## Phase 3: Quote Engine (Motor de Presupuestos)
- [ ] Task: Develop `QuoteService` with dual-costing logic.
    - [ ] Implement fixed price calculation.
    - [ ] Implement dynamic costing (Fabric + Labor + Margin) integration.
- [ ] Task: Implement Quote PDF Generation.
    - [ ] Use QuestPDF to generate professional quotes in PT-BR.
- [ ] Task: Quote Management UI.
    - [ ] Interface to create, view, and download quotes for a specific Lead.
- [ ] Task: Conductor - User Manual Verification 'Phase 3: Quotes' (Protocol in workflow.md)

## Phase 4: Final Integration & Polish
- [ ] Task: End-to-end testing of the lead-to-quote flow.
    - [ ] Ensure proper validation and error handling in PT-BR.
- [ ] Task: Conductor - User Manual Verification 'Phase 4: Final Integration' (Protocol in workflow.md)