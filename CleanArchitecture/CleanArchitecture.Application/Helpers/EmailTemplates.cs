using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Helpers
{
    public class EmailTemplates
    {
        public static string ConfirmEmail(string fullName, string confirmUrl) => $"""
        <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
            <h2>Merhaba {fullName},</h2>
            <p>Hesabını doğrulamak için aşağıdaki butona tıkla:</p>
            <a href="{confirmUrl}"
               style="display:inline-block;padding:12px 24px;background:#4F46E5;
                      color:white;border-radius:6px;text-decoration:none;font-weight:bold">
                Hesabı Doğrula
            </a>
            <p style="color:#666;margin-top:16px">Bu bağlantı 24 saat geçerlidir.</p>
        </div>
        """;

        public static string ResetPassword(string fullName, string resetUrl) => $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
                <h2>Merhaba {fullName},</h2>
                <p>Şifreni sıfırlamak için aşağıdaki butona tıkla:</p>
                <a href="{resetUrl}"
                   style="display:inline-block;padding:12px 24px;background:#4F46E5;
                          color:white;border-radius:6px;text-decoration:none;font-weight:bold">
                    Şifremi Sıfırla
                </a>
                <p style="color:#666;margin-top:16px">
                    Bu bağlantı 1 saat geçerlidir.<br>
                    Eğer bu isteği sen yapmadıysan dikkate alma.
                </p>
            </div>
            """;
    }
}
