using System;
using System.Net.Mail;
using cfgMgr = System.Configuration.ConfigurationManager;

namespace CoP.Enterprise
{
    public static class Email
    {
        private static readonly string sNL
            = Environment.NewLine;
        private static readonly string nL2 = sNL + sNL;
        private static readonly string nL3 = nL2 + sNL;

        public static void Send(string addressTo, 
            string addressFrom, string subject, 
            string body, string host, int port)
        {
            var testEmail = cfgMgr.AppSettings["testEmailAddress"];
            if (!string.IsNullOrWhiteSpace(testEmail))
            {
                body = $"THIS EMAIL REDIRECTED TO: [{testEmail}]{sNL}" +
                       $"      Original Addressee: [{addressTo}]{sNL}" +
                       $"{string.Empty.PadRight(23+addressTo.Length, '-')}{nL2}{body}";
                addressTo = testEmail;
            }
            // -------------------------------------------------------------------
            using (var smtp = new SmtpClient { Host = host, Port = port })
            {
                var msg = new MailMessage
                {
                    From = new MailAddress(addressFrom),
                    Subject = subject,
                    Body = body
                };
                msg.To.Add(addressTo);

                try { smtp.Send(msg); }
                catch (SmtpException sX)
                {
                    DALLog.Write(DALLog.Level.Error,
                        $"Unable to send the following email:{nL3}{subject}" +
                        $"Experienced SmtpException: {sX.Message} ");
                }
                catch (InvalidOperationException ioX)
                {
                    DALLog.Write(DALLog.Level.Error, 
                        $"Unable to send email: {ioX.Message} ");
                }
            }
        }
    }
}
