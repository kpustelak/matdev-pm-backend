namespace matdev.Application.DTOs.Project;

public sealed record LookupOption(int Id, string Name);

public sealed record UserLookupOption(int Id, string FirstName, string LastName, string DisplayName);

public sealed record ProjectCreateLookupsDTO(
    IReadOnlyList<LookupOption> IssueTypes,
    IReadOnlyList<LookupOption> Topics,
    IReadOnlyList<LookupOption> Workpackages,
    IReadOnlyList<LookupOption> Statuses,
    IReadOnlyList<LookupOption> Priorities,
    IReadOnlyList<UserLookupOption> Users);
