# Our Rules of the Road

> *"Our objective is not merely to build software that works, but to
> build software that teaches. Every design decision, every test, every
> commit, and every document should leave the project easier to
> understand than we found it."*

------------------------------------------------------------------------

# Purpose

This repository exists to accomplish more than producing a working Space
Engineers script.

It is intended to:

-   Solve a practical problem.
-   Serve as an exercise in thoughtful software engineering.
-   Provide a vehicle for learning Git and GitHub.
-   Preserve engineering decisions for future reference.
-   Become an example that can be shared with and learned from by
    others.

------------------------------------------------------------------------

# Our Philosophy

We value clarity over cleverness.

A simple design that is easily understood is preferred over a
sophisticated design that is difficult to maintain.

We recognize that software is read far more often than it is written.
Accordingly, we strive to make the repository understandable by someone
encountering it for the first time.

------------------------------------------------------------------------

# Project Maxims

These short statements capture the culture we wish to cultivate within
this repository.

-   **Document the set things, not speculation.**
-   **Every commit should represent one coherent idea.**
-   **Leave the repository easier to understand than you found it.**
-   **The local repository is the workshop; GitHub is the publication
    site.**
-   **A repository should be able to explain itself to someone who has
    never met its original authors.**

------------------------------------------------------------------------

# Design Principles

-   Separate concerns.
-   Favor high cohesion and loose coupling.
-   Keep business logic independent of presentation.
-   Prefer composition and clear interfaces over tightly coupled
    implementations.
-   Build incrementally.
-   Refactor when understanding improves.
-   Preserve proven behavior while improving internal structure.

------------------------------------------------------------------------

# Development Workflow

1.  Understand the problem before writing code.
2.  Discuss and document the design.
3.  Create a feature branch for each significant change.
4.  Implement in small, logical increments.
5.  Test the implementation.
6.  Update documentation alongside the code.
7.  Commit each coherent idea separately.
8.  Merge only when the feature is complete and understood.

------------------------------------------------------------------------

# Documentation Principles

We document **decisions**, not speculation.

Documentation should explain:

-   Why a design decision was made.
-   What responsibilities each component owns.
-   Assumptions and invariants.
-   Configuration and operational behavior.
-   Test results that were actually observed.

We avoid documenting imagined future capabilities or speculative designs
until they become active engineering decisions.

------------------------------------------------------------------------

# Testing Philosophy

Testing exists to increase confidence, not merely to obtain a passing
result.

We: - Record actual observations. - Preserve regression tests. - Expand
the test suite as features evolve. - Prefer repeatable tests over ad hoc
experimentation.

------------------------------------------------------------------------

# Git Philosophy

Git history is part of the project's documentation.

-   Keep `main` stable.
-   Develop features on feature branches.
-   Every commit should represent one coherent idea.
-   Commit messages should describe intent.
-   Use pull requests as an opportunity to review and explain changes.

GitHub is the publication site.

The local repository is the workshop.

------------------------------------------------------------------------

# Coding Philosophy

We strive to write code that communicates its intent.

We prefer: - Readability over brevity. - Explicitness over cleverness. -
Small focused classes. - Self-documenting code. - Comments that explain
*why*, not *what*. - Consistent naming. - Incremental improvement.

Whenever practical, leave the code easier to understand than it was
before.

------------------------------------------------------------------------

# Definition of Done

A feature is considered complete when:

-   The implementation works as intended.
-   Existing functionality continues to work.
-   Appropriate tests have been executed.
-   Documentation has been updated.
-   The repository remains in a releasable state.
-   Another developer could understand the change from the code,
    documentation, and commit history.

------------------------------------------------------------------------

# Final Thoughts

This repository is intended to demonstrate thoughtful software
engineering through practical work.

Its success will not be measured solely by the capabilities of the
finished script, but also by the clarity of its design, the quality of
its documentation, the discipline of its workflow, and the usefulness of
its history.

If future contributors understand not only *what* was built, but also
*why* it was built that way, then these rules have served their purpose.

------------------------------------------------------------------------

**Adopted by the SpaceEngineers1-Scripts project.**
