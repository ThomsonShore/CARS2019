using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace CARS2019.Models
{
    public class MailSendHelper
    {
        public static string SendProductionEmails = ConfigurationManager.AppSettings["SendProductionEmails"];
        public static void SendingDepartmentEmail(string senderName, string recipientEmail, string emailBody, string targetURL, string jobID)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("donotreply@tshore.com");

            SmtpClient smtp = new SmtpClient();
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("donotreply@tshore.com", "Pr0gre$$");
            smtp.Host = "smtp.gmail.com";

            foreach (var address in recipientEmail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                mail.To.Add(address);
            }

            //mail.To.Add(recipientEmail);
            mail.CC.Add("jbrennan@tshore.com");  // change this to mail.TO.Add if we move the below condition check to the above loop *********************************
            mail.Subject = "New CARS Entry Added by " + senderName + " for JobID: " + jobID;
            mail.IsBodyHtml = true;

            string htmlBody = @"
            <b>New CARS issue has been entered:</b> <br/> <br/> <br/>";
            htmlBody += "      " + emailBody + " <br/><br/>";
            htmlBody += "      " + targetURL + " <br/>";


            mail.Body = htmlBody;
            // Added a key (SendProdcutionEmails) to the root web config to disable (off) and enable (on) sending alert emails in production (error emails will still be sent)
            // maybe move this to above code where we loop though and append emails, only add my email, that way we can still see an email sent??? ****************************************
            if (SendProductionEmails == "on")
            {
                smtp.Send(mail);
            }
            
        }


        public static void SendingErrorEmail(string senderName, string recipientEmail, string emailBody)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("donotreply@tshore.com");

            SmtpClient smtp = new SmtpClient();
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("donotreply@tshore.com", "Pr0gre$$");
            smtp.Host = "smtp.gmail.com";

            mail.To.Add(recipientEmail);
            mail.Subject = "CARS ERROR ";
            mail.IsBodyHtml = true;

            string htmlBody = @"
                <b>New CARS issue has been entered:</b> <br/> <br/> <br/>";
            htmlBody += "      " + emailBody + " <br/><br/>";

            mail.Body = htmlBody;

            smtp.Send(mail);
        }
    }
}
