using System.Net.Mail;
using System.Threading.Tasks;

namespace BitMinistry
{
    public interface IMailSender
    {
        Task SendAsync(MailMessage message);
        string SmtpUser { get; }
    }
}
