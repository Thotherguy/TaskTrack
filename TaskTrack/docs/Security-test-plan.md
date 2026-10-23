# TaskTrack: repeatable role and security test plan

Run only against an authorized local or staging instance containing disposable test data. Do not use real personal information. Record build, tester, date, account role, URL, actual result, and pass/fail for every case.

## Setup

1. Build the solution and apply all EF migrations, including AddTaskCommentsAndActivity. The current local instance has already been migrated.
2. Use a trusted Admin and register separate Manager, Member A, and Member B test accounts with different strong passwords. New registrations must receive Member. The Admin promotes only the Manager account through Administration.
3. In a Manager session create Project A, starting October 1 and due November 30 of the testing year. Add Member A by their exact registered email. Keep Member B outside the project.
4. Create Task A assigned to Member A and Task B unassigned. Give each a distinctive title. Give Task A a past due date within the project dates. Use isolated browser profiles or cookie jars for each account.

## Cases

| Case | Action | Expected result |
| --- | --- | --- |
| Anonymous data | Open Dashboard, Projects, a task detail, and project activity while signed out. | Redirect to login; no project data returned. |
| Default role | Register a fresh account and try Create Project. | Account receives Member; project creation is forbidden. |
| Role administration | Admin promotes Member B to Manager, then demotes them. | Form succeeds; role change invalidates existing sessions. Fresh login has only the new permissions. |
| Protected Admin | Submit a Manager/Member role change for an existing Admin. | Bad request; Admin remains unchanged. |
| Public role tampering | Include Role=Admin in a registration request. | Account remains Member. |
| Admin visibility | Admin views Project A and both tasks. | Both tasks are visible. |
| Ownership policy | Another Manager, or an Admin who does not own A, opens its Edit/Delete/member management actions. | Not found; write operations remain owner-only. |
| Project membership | Add Member A twice using their exact email. | One membership exists; duplicate addition is harmless. |
| Membership privacy | Open Project A's members page. | Only the project owner and existing team are listed. Other registered users are not enumerated. |
| Email validation | Add a malformed or unregistered email. | Validation feedback; membership unchanged. |
| Owner protection | Forge a Remove request for the owner. | Bad request; owner remains. |
| Member project read | Member A opens Project A. | Project loads. |
| Unrelated project read | Member B opens Project A and its activity by ID. | Not found. |
| Task visibility | Member A opens task list, dashboard, Task A, and Task B by ID. | Task A is visible; Task B is not in lists/counts and its detail returns not found. |
| Dashboard totals | Compare Member A and Manager views. | A sees only Task A; Manager sees both. Open overdue tasks increase Overdue; completing A removes it from Overdue. |
| Date bounds | Submit task due dates before project start and after project due. | Validation feedback; no task saved. |
| Assignee tampering | Submit Member B's user ID as Task A's assignee. | Validation feedback; unrelated assignment is rejected. |
| Member status | Member A changes Task A to Completed while also submitting Title, ProjectId, and AssignedUserId fields. | Only status changes. Task history records the status transition. |
| Invalid status | Submit a status outside To Do / In Progress / Completed. | Bad request; existing status unchanged. |
| Member writes | Member tries project/task Create, Edit, Delete URLs and POSTs. | Forbidden; no state change. |
| Comment authorization | A posts to Task A; B posts to Task A; A posts to Task B. | Only the first request succeeds. |
| Comment author tampering | Post a comment with forged AuthorId and CreatedAtUtc values. | Current signed-in author and server UTC time are used. |
| Comment validation | Submit blank, whitespace-only, and 2,001-character comments. | Validation feedback; nothing stored. |
| Stored HTML | Post `<img src=x onerror=alert(1)>` as comment text. | Literal text is shown safely; markup does not execute. |
| Anti-forgery | Omit/alter the anti-forgery token on comment, membership, role, project, and status forms. | Bad request; no state change. |
| Pagination | Add at least 23 comments and enough events to exceed 25. Visit older pages. | 20 comments and 25 project events per page; no duplicates/omissions under an unchanged dataset. |
| Invalid pagination | Request page zero, negative pages, or pages over 10,000. | Bad request. |
| Activity authors | Create/update tasks, comment, and add/remove membership. | History shows an event, its author, and UTC timestamp. |
| Activity visibility | Member opens Project A activity and dashboard while Task B has a distinctive private title. | Task B's title/history is absent. Owner/Admin can see it. |
| Membership revocation | Manager removes A, then A uses an already-open task page/token to read or comment/update status. | Access is immediately denied; assignment is cleared; dashboard no longer includes A. |
| Task deletion history | Manager deletes a disposable task. | Comments cascade away; project activity retains the task-deletion event. |
| Project deletion | Manager deletes the disposable test project. | Its tasks, comments, memberships, and project activity are removed without affecting other projects. |

## Cleanup and evidence

Delete only fixtures created for this test. Remove generated accounts through their own Identity account-management pages. Keep screenshots and a results table without passwords, cookies, tokens, or real personal data. Test passwords must not be committed.

## History behavior

History starts with Step 5; previous edits are not backfilled. It records successful project/task/member/comment changes made through the application's asynchronous database save path, in the same transaction. It is project-scoped activity, not a permanent compliance audit: deleting a project deletes its activity, and direct SQL changes are not captured. Task detail shows the latest 30 events; project activity provides paginated history.
