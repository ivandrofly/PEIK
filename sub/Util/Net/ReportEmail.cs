﻿#region Imports

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;

#endregion

namespace sub.Util.Net
{
    internal class ReportEmail
    {
        private string _description;
        private string _host;
        private string _password;
        private int _port;
        private string _username;

        public ReportEmail(string description, string username, string password, string host, int port)
        {
            _description = description;
            _username = username;
            _password = password;
            _host = host;
            _port = port;
        }

        public void Send(string data, List<Attachment> att = null)
        {
            if (string.IsNullOrEmpty(data)) return; //We don't want to send empty logs
            MessageBox.Show("Sending Report: " + data);

            //append computer info to signature at the end
            data += ReportSignature.GetSignature();
            SmtpClient smtp = new SmtpClient
                                  {
                                      Host = _host,
                                      Port = _port,
                                      EnableSsl = true,
                                      DeliveryMethod = SmtpDeliveryMethod.Network,
                                      UseDefaultCredentials = false,
                                      Credentials = new NetworkCredential(_username, _password)
                                  };
            MailMessage message = new MailMessage(_username, _username)
                                      {
                                          Subject = Environment.MachineName + " - " + _description,
                                          Body = data
                                      };
            if (att != null)
            {
                foreach (Attachment atta in att)
                {
                    message.Attachments.Add(atta);
                }
            }

            smtp.Send(message);
        }
    }
}