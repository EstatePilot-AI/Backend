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
}
