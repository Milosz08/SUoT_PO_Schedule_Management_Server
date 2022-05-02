﻿using System;


namespace asp_net_po_schedule_management_server.Utils
{
    public class GlobalConfigurer
    {
        // obsługa JWT
        public static string JwtKey { get; set; }
        public static TimeSpan JwtExpiredTimestamp { get; set; }
        
        //--------------------------------------------------------------------------------------------------------------
        
        // obsługa SSH
        public static string SshServer { get; set; }
        public static string SshUsername { get; set; }
        public static string SshPassword { get; set; }
        public static string SshPasswordFieldName { get; set; }
        
        // inne
        public static string UserEmailDomain { get; set; }
        public static string DbDriverVersion { get; set; }
        public static byte UserEmailMaxSizeMb { get; set; }
    }
}