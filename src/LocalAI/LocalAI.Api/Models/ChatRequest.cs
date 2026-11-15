namespace LocalAiApi.Models
{
    public class ChatRequest 
    {
        public string Message { get; set; } 
        public string Prompt { get; set; } = "analyse";
    }
}
