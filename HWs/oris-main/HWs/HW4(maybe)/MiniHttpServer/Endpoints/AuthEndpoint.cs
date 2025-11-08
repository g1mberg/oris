using MiniHttpServer.Core.Attributes;
using MiniHttpServer.Services;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class AuthEndpoint
    {
        // Get /auth/
        [HttpGet]
        public string LoginPage()
        {

            return "index.html";
        }

        // Post /auth/
        [HttpPost]
        public void Login(/*string email, string password*/)
        {
            // Отправка на почту email указанного email и password
            // EmailService.SendEmail(email, title, message);
        }


        // Post /auth/sendEmail
        [HttpPost("sendEmail")]
        public void SendEmail(string to, string title, string message)
        {
            // Отправка на почту email указанного email и password

            
        }

    }
}
