namespace CloudShopping.Application.Features.Departments.ViewModels
{
    public sealed record DepartmentViewModel(
            int Id,
            string Name,
            string Slug,
            bool IsSystemDefault
        );
}
