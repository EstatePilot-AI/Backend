using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services;

public class HTMLMessages
{
	public static string MeetingInfoEmail(DateTime meetingDate, string meetingLocation, string buyerName, string buyerPhone, string sellerName, string sellerPhone, string agentName, string agentPhone)
	{
		var message = $@"<!DOCTYPE html>
<html>
  <head>
    <title>Hello, World!</title>
    <link rel=""stylesheet"" href=""styles.css"" />
  </head>
  <body>
      <div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: auto; border: 1px solid #e0e0e0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);"">
    
    <div style=""background-color: #2c3e50; color: #ffffff; padding: 20px; text-align: center;"">
        <h2 style=""margin: 0; font-size: 22px;"">Meeting Information</h2>
    </div>

    <div style=""padding: 25px;"">
        <div style=""margin-bottom: 25px; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #3498db; border-radius: 4px;"">
            <p style=""margin: 0 0 10px 0;""><strong>📅 Date & Time:</strong> {meetingDate}</p>
            <p style=""margin: 0;""><strong>📍 Location:</strong><br>
               <span style=""color: #555; font-size: 0.95em;"">{meetingLocation}</span>
            </p>
        </div>

        <h3 style=""font-size: 18px; color: #2c3e50; border-bottom: 1px solid #eee; padding-bottom: 8px;"">Contact Directory</h3>
        <table style=""width: 100%; border-collapse: collapse;"">
            <thead>
                <tr style=""text-align: left; font-size: 12px; color: #888; text-transform: uppercase;"">
                    <th style=""padding-bottom: 10px;"">Role</th>
                    <th style=""padding-bottom: 10px;"">Name</th>
                    <th style=""padding-bottom: 10px;"">Phone</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td style=""padding: 12px 0; font-weight: bold;"">Buyer</td>
                    <td style=""padding: 12px 0;"">{buyerName}</td>
                    <td style=""padding: 12px 0;""><a href=""tel:{buyerPhone}"" style=""color: #3498db; text-decoration: none;"">{buyerPhone}</a></td>
                </tr>
                <tr style=""border-top: 1px solid #f1f1f1;"">
                    <td style=""padding: 12px 0; font-weight: bold;"">Seller</td>
                    <td style=""padding: 12px 0;"">{sellerName}</td>
                    <td style=""padding: 12px 0;""><a href=""tel:{sellerPhone}"" style=""color: #3498db; text-decoration: none;"">{sellerPhone}</a></td>
                </tr>
                <tr style=""border-top: 1px solid #f1f1f1;"">
                    <td style=""padding: 12px 0; font-weight: bold;"">Agent</td>
                    <td style=""padding: 12px 0;"">{agentName}</td>
                    <td style=""padding: 12px 0;""><a href=""tel:{agentPhone}"" style=""color: #3498db; text-decoration: none;"">{agentPhone}</a></td>
                </tr>
            </tbody>
        </table>
    </div>
</div>
  </body>
</html>";

        return message;
	}

    public static string ConfirmationEmailToSeller(int id)
    {
        var message = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Property Added</title>
    <style>
        /* Standard body styling */
        body {{
            font-family: Arial, Helvetica, sans-serif;
            background-color: #f5f5f5;
            color: #333333;
            padding: 40px 20px;
        }}

        /* Main container panel */
        .panel {{
            max-width: 500px;
            margin: 0 auto;
            background-color: #ffffff;
            border: 1px solid #cccccc;
            padding: 20px;
        }}

        /* Standard success alert box */
        .alert-success {{
            background-color: #dff0d8;
            border: 1px solid #d6e9c6;
            color: #3c763d;
            padding: 15px;
            margin-bottom: 20px;
        }}

        .alert-success h3 {{
            margin-top: 0;
            margin-bottom: 10px;
            font-size: 18px;
        }}

        .alert-success p {{
            margin: 0;
            font-size: 14px;
            line-height: 1.5;
        }}

        /* Action area layout */
        .action-area {{
            border-top: 1px solid #eeeeee;
            padding-top: 15px;
            text-align: right;
        }}

        /* Standard button styles for an anchor tag */
        .btn {{
            display: inline-block;
            padding: 8px 16px;
            font-size: 14px;
            text-decoration: none;
            text-align: center;
            cursor: pointer;
            border-radius: 2px;
        }}

        .btn-primary {{
            background-color: #337ab7;
            color: #ffffff;
            border: 1px solid #2e6da4;
        }}

        .btn-primary:hover {{
            background-color: #286090;
        }}
    </style>
</head>
<body>

    <div class=""panel"">
        <div class=""alert-success"">
            <h3>Success</h3>
            <p>Your property has been added successfully.</p>
        </div>
        
        <div class=""action-area"">
            <a href="""" class=""btn btn-primary"">Update Property Details</a>
        </div>
    </div>

</body>
</html>
";
        return message;
    }

    public static string ConfirmationEmailAboutCreatedAccount(string name, string email, string password)
    {
		var htmlMessage = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin: 0; padding: 0; background-color: #f0f2f5; font-family: 'Segoe UI', Helvetica, Arial, sans-serif;"">
    <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" style=""background-color: #f0f2f5; padding: 20px 0;"">
        <tr>
            <td align=""center"">
                <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" style=""max-width: 600px; background-color: #ffffff; border-radius: 12px; overflow: hidden; border-collapse: separate;"">
                    <tr>
                        <td style=""background-color: #2563eb; padding: 40px; text-align: center;"">
                            <h1 style=""margin: 0; font-size: 24px; color: #ffffff;"">Welcome Aboard!</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px 30px; color: #444444; line-height: 1.6;"">
                            <p style=""font-size: 18px; margin: 0 0 20px 0;"">Hi <strong>{name}</strong>,</p>
                            <p style=""margin: 0 0 20px 0;"">An administrator has created your account. You can now log in to the platform using the temporary credentials below.</p>
                            
                            <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" style=""background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px;"">
                                <tr>
                                    <td style=""padding: 20px;"">
                                        <div style=""font-size: 12px; text-transform: uppercase; color: #64748b; font-weight: bold; margin-bottom: 4px;"">Email Address</div>
                                        <div style=""font-family: 'Courier New', Courier, monospace; font-size: 16px; color: #1e293b; font-weight: bold;"">{email}</div>
                                        
                                        <div style=""height: 15px; font-size: 15px; line-height: 15px;"">&nbsp;</div>
                                        
                                        <div style=""font-size: 12px; text-transform: uppercase; color: #64748b; font-weight: bold; margin-bottom: 4px;"">Temporary Password</div>
                                        <div style=""font-family: 'Courier New', Courier, monospace; font-size: 16px; color: #dc2626; font-weight: bold;"">{password}</div>
                                    </td>
                                </tr>
                            </table>

                            <p style=""font-size: 13px; color: #64748b; margin: 20px 0;""><em>Note: For security reasons, please change your password immediately after your first login.</em></p>

                            <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                                <tr>
                                    <td align=""center"" style=""padding-top: 10px;"">
                                        <a href=""https://estatepilot-sable.vercel.app/login"" 
                                           style=""background-color: #2563eb; color: #ffffff; padding: 14px 30px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;"">
                                           Login to Your Account
                                        </a>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

        return htmlMessage;
	}
}
