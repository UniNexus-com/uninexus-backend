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
            <h2>Hello {fullName},</h2>
            <p>To verify your account, click the button below:</p>
            <a href="{confirmUrl}"
               style="display:inline-block;padding:12px 24px;background:#4F46E5;
                      color:white;border-radius:6px;text-decoration:none;font-weight:bold">
                Verify Account
            </a>
            <p style="color:#666;margin-top:16px">This link is available for 24 hours.</p>
        </div>
        """;

        public static string ResetPassword(string fullName, string resetUrl) => $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
                <h2>Merhaba {fullName},</h2>
                <p>To reset your password, click the button below:</p>
                <a href="{resetUrl}"
                   style="display:inline-block;padding:12px 24px;background:#4F46E5;
                          color:white;border-radius:6px;text-decoration:none;font-weight:bold">
                    Reset My Password
                </a>
                <p style="color:#666;margin-top:16px">
                    This link is available for 1 hour.<br>
                    If you didn't make this request, disregard it.
                </p>
            </div>
            """;
    }
}
