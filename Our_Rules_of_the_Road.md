# Our Rules of the Road

> *"Our objective is not merely to build software that works, but to build software that teaches. Every design decision, every test, every commit, and every document should leave the project easier to understand than we found it."*

---

# Purpose

This repository exists to accomplish more than producing a working Space Engineers script.

It is intended to:

- Solve a practical problem.
- Serve as an exercise in thoughtful software engineering.
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

# Engineering Workflow

Every engineering activity follows this lifecycle.

1. Create or identify an Issue.
2. Perform analysis.
3. Plan verification by creating or updating the test plan.
4. Complete the design.
5. Implement the solution.
6. Verify the implementation.
7. Update the project documentation.
8. Create and review the Pull Request.
9. Merge into `main`.
10. Close the Issue.

Testing is intentionally planned before implementation begins. The test
plan defines how success will be measured and guides the implementation.

Documentation is a first-class engineering activity and is completed as
part of the feature, not afterward.

---

# GitHub Workflow

Each engineering activity is carried through the repository using the
following workflow.

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
13. Verify a clean working tree before beginning the next task.

The workflow is complete only when the repository has returned to a
known-good state on `main`, ready for the next engineering activity.

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

# Branch Naming Convention

## Branch Types

- `feature/<name>` — New functionality.
- `bugfix/<name>` — Defect corrections.
- `docs/<name>` — Documentation-only work.
- `test/<name>` — Test planning, execution, or test documentation.
- `release/<version>` — Release preparation.
- `experiment/<name>` — Exploratory work or prototypes.

## Guidelines

- Use lowercase.
- Separate words with hyphens.
- Keep names concise.
- One coherent engineering activity per branch.
- Keep only one engineering branch active at a time.

---

# Git Philosophy

- Keep `main` stable.
- Develop on purpose-specific branches.
- Every commit should represent one coherent idea.
- Commit messages should describe intent.
- Pull Requests should explain why the work should be merged.
- Finish every branch by returning to a clean, up-to-date `main` branch.

---

# GitHub Issues Philosophy

GitHub Issues represent engineering work, not Git operations.

Examples of good Issues include:

- Complete Version 1.1 Verification.
- Update Version 1.1 Documentation.
- Release Version 1.1.

Examples of workflow activities rather than Issues include:

- Commit.
- Publish.
- Merge.
- Delete branch.

---

# Coding Philosophy

Prefer readable, maintainable code.

Write code that clearly expresses the agreed design rather than code that merely produces the correct result.

---

# Definition of Done

An engineering activity is complete only when it:

- Works correctly.
- Is verified.
- Is documented.
- Is merged into `main`.
- Leaves the repository releasable.
- Returns the working tree to a clean state.
- Has its associated Issue closed.

---

# Final Thoughts

If future contributors understand not only *what* was built, but *why*, these rules have served their purpose.

---

**Adopted by the SpaceEngineers1-Scripts project.**