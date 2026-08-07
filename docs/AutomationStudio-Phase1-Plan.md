# Automation Studio --- Phase 1 Implementation Plan

## Goal

Establish the application foundation with the first end-to-end vertical
slice.

Deliver: - Project management - Two-level application layout - Content
Type management - Content management - Reusable shared UI foundation

Out of scope: Resources, Pipelines, Inspection, Storage business, AI,
Permissions.

------------------------------------------------------------------------

# Backend Scope

## Projects

-   CRUD
-   Search
-   Offset pagination (existing template)
-   Validation

## Content Types

CRUD per Project.

Fields: - Name - DisplayName - Description - Icon - Color - SortOrder

## Contents

CRUD per Project.

Fields: - Name - ContentTypeId - Description - Thumbnail (placeholder
only)

------------------------------------------------------------------------

# Frontend Scope

## Level 1 Layout (Global)

Sidebar contains two sections.

### Projects

-   Search
-   Quick Create (+)
-   Project List

### Administration

Identity - Users - Roles

System - Settings - Audit Logs

Selecting a project enters the Project layout.

------------------------------------------------------------------------

## Level 2 Layout (Project)

Sidebar:

-   Overview
-   Contents
-   Content Types
-   Resources (placeholder)
-   Pipelines (placeholder)
-   Settings

### Overview (implemented)

Display: - Project information - Summary statistics - Recent contents -
Quick actions - Recent activity (placeholder)

Overview is a landing page only. No CRUD actions belong here.

------------------------------------------------------------------------

# Shared UI Convention

All features should reuse shared components.

## Layout

-   PageHeader
-   PageToolbar
-   PageSection

## Cards

-   BaseCard
-   CardGrid

Cards support: - Thumbnail - Title - Subtitle - Badge area - Context
menu

ProjectCard and ContentCard reuse BaseCard.

## Empty State

Reusable for every feature.

## Loading

Use Skeleton components.

## Dialog

-   Create/Edit -\> Modal Dialog
-   Delete -\> Confirmation Dialog

------------------------------------------------------------------------

# List Convention

Tables - Existing offset pagination.

Card Lists - Same offset API. - UI uses Load More. - Append items. - No
infinite scroll.

------------------------------------------------------------------------

# Feature Structure

Each feature follows the same structure:

-   Pages
-   Components
-   Api
-   Models
-   Hooks

Prefer shared components over feature-specific implementations.

------------------------------------------------------------------------

# Implementation Order

## Sprint 1

-   Project CRUD
-   Level 1 Layout
-   Project List
-   Quick Create

## Sprint 2

-   Level 2 Layout
-   Overview
-   Content Type CRUD

## Sprint 3

-   Shared UI Foundation
-   BaseCard
-   CardGrid
-   EmptyState
-   Skeleton
-   Dialog

## Sprint 4

-   Content CRUD
-   Content Detail
-   Search
-   Load More

------------------------------------------------------------------------

# Out of Scope

-   Resources
-   Pipelines
-   Inspection
-   Storage
-   File Upload
-   Versioning
-   Tags
-   Permissions
-   AI

Navigation placeholders are allowed.

------------------------------------------------------------------------

# Acceptance Criteria

## Projects

-   CRUD
-   Search
-   Switch project

## Overview

-   Project summary
-   Statistics
-   Recent contents
-   Quick actions

## Content Types

-   CRUD inside project

## Contents

-   CRUD
-   Content belongs to a Content Type
-   Card view
-   Search
-   Load More

## Frontend

-   Two-level navigation
-   Shared components reused across features
-   Consistent dialog, empty state and skeleton
