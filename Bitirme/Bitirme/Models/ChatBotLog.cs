using Bitirme.Areas.Identity.Data;

namespace Bitirme.Models
{
    public class ChatBotLog
    {
        public int Id { get; set; } // Primary Key
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string Message { get; set; } // User's question or message
        public string Response { get; set; } // Chatbot's response
        public DateTime? Date { get; set; } = DateTime.Now;
    }


}
