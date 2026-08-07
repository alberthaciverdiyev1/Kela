using Kela.Web.Infrastructure;

namespace Kela.Web.Models.Students;

public sealed record CreateStudentSuccessViewModel(
    StudentsIndexViewModel List,
    StudentCreatedResponse Credentials);
