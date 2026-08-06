using Kela.Domain.Common;

namespace Kela.Domain.Entities;

public class Section : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    /// <summary>FK → AspNetUsers (Teacher rolündeki kullanıcı).</summary>
    public int? TeacherId { get; set; }
    public User? Teacher { get; set; }

    /// <summary>Şubeye kayıtlı öğrenciler (Student rolündeki kullanıcılar).</summary>
    public ICollection<User> Students { get; set; } = new List<User>();
}
