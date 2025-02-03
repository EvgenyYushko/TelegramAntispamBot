using System.Collections.Generic;
using System.Threading.Tasks;

namespace MailSenderService.ServiceLayer.Services
{
	public interface IMailService
	{
		Task Send(string toAddress, string body, string theme, bool isBodyHtml = false, List<string> copyAddress = null);
	}
}
