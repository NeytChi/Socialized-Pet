﻿namespace UseCases.Response.Appeals
{
    public class AppealReplyResponse
    {
        public long Id { get; set; }
        public long AppealMessageId { get; set; }
        public required string Reply { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
