﻿using System.Threading.Tasks;


namespace asp_net_po_schedule_management_server.Ssh.SmtpEmailService
{
    public interface ISmtpEmailService
    {
        Task SendResetPassword(UserEmailOptions userEmailOptions);
        Task SendCreatedUserAuthUser(UserEmailOptions userEmailOptions);
        Task SendNewContactMessage(UserEmailOptions userEmailOptions, string issueIdentified);
    }
}