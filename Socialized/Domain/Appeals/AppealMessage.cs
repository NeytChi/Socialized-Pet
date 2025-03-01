using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Admins
{
    [Table("AppealMessages")]
    public partial class AppealMessage : BaseEntity
    {
        public AppealMessage()
        {
            Files = new HashSet<AppealFile>();
            AppealMessageReplies = new HashSet<AppealMessageReply>();
        }
        [ForeignKey("Appeal")]
        public long AppealId { get; set; }
        public string Message { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public virtual Appeal Appeal { get; set; }
        public virtual ICollection<AppealFile> Files { get; set; }
        public virtual ICollection<AppealMessageReply> AppealMessageReplies { get; set; }
    }
}