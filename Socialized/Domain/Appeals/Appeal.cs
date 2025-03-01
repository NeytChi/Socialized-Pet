using Domain.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Admins
{
    [Table("Appeals")]
    public partial class Appeal : BaseEntity
    {
        public Appeal()
        {
            Messages = new HashSet<AppealMessage>();
        }
        [ForeignKey("User")]
        public long UserId { get; set; }
        public string Subject { get; set; }
        public int State { get; set; }
        public DateTimeOffset LastActivity { get; set; }
        public virtual User User { get; set; }
        public ICollection<AppealMessage> Messages { get; set; }
    }
}
