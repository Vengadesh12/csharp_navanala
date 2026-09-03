namespace MyBackend.Application.Common.Models
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string SenderName { get; set; } = "Workspace Administration";
        public string SenderEmail { get; set; } = "venkikc333@gmail.com";
        public string AppPassword { get; set; } = "dznudcfzffnyeqjl";
        public bool EnableSsl { get; set; } = true;
    }
}
