using Kela.Domain.Common;

namespace Kela.Domain.Entities;

public class StudentProfile : BaseEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }

    public int? CityId { get; set; }
    public City? City { get; set; }

    public DateTime? BirthDate { get; set; }
}
