# Our Rules of the Road

> *"Our objective is not merely to build software that works, but to build software that teaches. Every design decision, every test, every commit, and every document should leave the project easier to understand than we found it."*

---

# Purpose

This endeavor exists to accomplish more than producing a working Space Engineers script.

It is intended to:

- Solve a practical problem.
- Serve as an exercise in thoughtful software design and development.
- Provide a vehicle for learning Git and GitHub.
- Preserve engineering decisions for future reference.
- Become an example that can be shared with and learned from by others.

---

# Our Philosophy

We value clarity over cleverness.

A simple design that is easily understood is preferred over a sophisticated design that is difficult to maintain.

We recognize that software is read far more often than it is written.

---

# Project Maxims

- **Document the set things, not speculation.**
- **Plan verification before implementation.**
- **Every commit should represent one coherent idea.**
- **Leave the repository easier to understand than you found it.**
- **The local repository is the workshop; GitHub is the publication site.**
- **A repository should be able to explain itself to someone who has never met its original authors.**
- **Design for extension. Implement for today's requirements.**
- **Make implementation the least creative part of the project. Creativity belongs in analysis, architecture, and design.**

---

# Design Principles

- Separate concerns.
- Favor high cohesion and loose coupling.
- Keep business logic independent of presentation.
- Build incrementally.
- Refactor when understanding improves.
- Preserve proven behavior while improving internal structure.
- Automation should minimize intrusion into the player's world.
- Prefer explicit contracts over implicit behavior.
- Establish clear ownership boundaries between collaborating systems.

---

# Documentation Principles

- Document decisions, not speculation.
- Record architectural decisions when they become stable.
- Keep documentation aligned with the implementation.
- Update documentation incrementally throughout development.
- Each document should have a single primary purpose.

---

# Testing Philosophy

Testing exists to increase confidence, not merely to obtain a passing result.

Successful testing confirms both expected behavior and the architectural assumptions upon which the implementation depends.

Verification artifacts belong on the same branch as the implementation they validate.

---

# Coding Philosophy

Prefer readable, maintainable code.

Write code that clearly expresses the agreed design rather than code that merely produces the correct result.

---

# Creative Workflow

Every significant improvement begins with recognizing an Opportunity.

1. Identify an Opportunity.
2. Perform analysis.
3. Define one or more Work Items.
4. Plan verification by creating or updating the test plan.
5. Complete the design.
6. Implement the solution.
7. Verify the implementation.
8. Update the project documentation.

Creativity belongs primarily in identifying opportunities, performing analysis, and developing the design. Implementation should be the disciplined realization of those earlier decisions.

Testing is intentionally planned before implementation begins. The test plan defines how success will be measured and guides the implementation.

Documentation is a first-class development activity and is completed as part of the work, not afterward.

---

# Work Item Types

Every Work Item represents one coherent piece of work.

Each Work Item is classified according to its primary purpose.

Current Work Item Types are:

- **Feature** — Introduces new functionality.
- **Bug Fix** — Corrects unintended behavior.
- **Documentation** — Improves or expands project documentation.
- **Testing** — Plans, performs, or records verification activities.
- **Release** — Prepares or publishes a project release.
- **Experiment** — Explores ideas or prototypes whose outcomes are intentionally uncertain.

A Work Item should have exactly one primary type. The Work Item type determines how the work is organized and how purpose-specific Git branches are named.

---

# Definition of Done

A Work Item is complete only when it:

- Works correctly.
- Is verified.
- Is documented.
- Is merged into `main`.
- Leaves the repository releasable.
- Returns the working tree to a clean state.
- Has any associated tracking artifacts brought to an appropriate conclusion.

---

# Tracking Work

Our Creative Workflow is intentionally independent of any particular tool.

Work Items should be tracked using whatever tool best fits the project. For this repository, GitHub Issues are the preferred mechanism whenever persistent tracking, discussion, or collaboration is beneficial.

GitHub implements our process; it does not define it.

---

# Git Philosophy

- Keep `main` stable.
- Develop on purpose-specific branches.
- Every commit should represent one coherent idea.
- Commit messages should describe intent.
- Pull Requests should explain why the work should be merged.
- Finish every branch by returning to a clean, up-to-date `main` branch.

---
# GitHub Workflow

Each Work Item is carried through the repository using the following workflow.

1. Start on `main`.
2. Pull origin.
3. Create a purpose-specific branch.
4. Make the changes.
5. Commit locally.
6. Publish the branch.
7. Create a Pull Request.
8. Review the changes.
9. Merge into `main`.
10. Delete the branch.
11. Return to `main`.
12. Pull origin.
13. Verify a clean working tree before beginning the next Work Item.

The workflow is complete only when the repository has returned to a
known-good state on `main`, ready for the next Work Item.

---

# Branch Naming Convention

Our Git branch names reflect the type and purpose of the Work Item being implemented.

Each branch name begins with the appropriate Work Item type followed by a concise description.

## Branch Prefixes

- `feature/<name>` — New functionality.
- `bugfix/<name>` — Defect corrections.
- `docs/<name>` — Documentation-only work.
- `test/<name>` — Test planning, execution, or test documentation.
- `release/<version>` — Release preparation.
- `experiment/<name>` — Exploratory work or prototypes.

These prefixes correspond directly to the Work Item Types defined earlier in this document.

## Guidelines

- Use lowercase.
- Separate words with hyphens.
- Keep names concise.
- One coherent Work Item per branch.
- Work on only one Work Item per active branch.

---

# Final Thoughts

If future contributors understand not only *what* was built, but *why*, these rules have served their purpose.

---

**Adopted by the SpaceEngineers1-Scripts project.**