using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace CARS2019.Models
{
    public class MailSendHelper
    {
        public static void testSendingEmail(string senderName, string recipientEmail, string emailBody, string targetURL, string jobID)
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
            mail.CC.Add("jbrennan@tshore.com");
            mail.Subject = "New CARS Entry Added by " + senderName +" for JobID: " + jobID;
            mail.IsBodyHtml = true;

            string htmlBody = @"
            <b>New CARS issue has been entered:</b> <br/> <br/> <br/>";
            htmlBody += "      " + emailBody + " <br/><br/>"; 
            htmlBody += "      " + targetURL + " <br/>";
            

            mail.Body = htmlBody;

            smtp.Send(mail);
        }
    }
}