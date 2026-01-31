using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_ColdCall_Agent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateSuperAdminData : Migration
    {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			var superAdminId = Guid.NewGuid().ToString();
			var securityStamp = Guid.NewGuid().ToString();
			var concurrencyStamp = Guid.NewGuid().ToString();
			var passwordHash = "AQAAAAIAAYagAAAAEFh89IMLVTd9MiUF2RnvEzxGZCjBdYB30nR2cf1AbZKH5Xv73XVoL5AqLUVTwc1h8Q==";

			migrationBuilder.Sql($@"
        INSERT INTO ""AspNetUsers"" (
            ""Id"", ""UserName"", ""NormalizedUserName"", ""Email"", ""NormalizedEmail"", ""PhoneNumber"", 
            ""EmailConfirmed"", ""PasswordHash"", ""SecurityStamp"", ""ConcurrencyStamp"", 
            ""PhoneNumberConfirmed"", ""TwoFactorEnabled"", ""LockoutEnabled"", ""AccessFailedCount"", ""Name""
        )
        VALUES (
            '{superAdminId}', 'ym090066@gmail.com', 'YM090066@GMAIL.COM', 'ym090066@gmail.com', 'YM090066@GMAIL.COM', '01018225020', 
            TRUE, '{passwordHash}', '{securityStamp}', '{concurrencyStamp}', 
            FALSE, FALSE, TRUE, 0, 'Youssef Toba'
        );
        
    ");

			// --- STEP 2: Insert the Role (if it doesn't exist) ---
			var roleId = Guid.NewGuid().ToString();

			migrationBuilder.Sql($@"
        INSERT INTO ""AspNetRoles"" (""Id"", ""Name"", ""NormalizedName"", ""ConcurrencyStamp"")
        VALUES ('{roleId}', 'superadmin', 'SUPERADMIN', '{Guid.NewGuid()}')
        ON CONFLICT (""NormalizedName"") DO NOTHING;
    ");

			// --- STEP 3: Assign the User to the Role ---
			// This SQL selects the IDs dynamically to ensure we link the correct rows
			migrationBuilder.Sql(@"
        INSERT INTO ""AspNetUserRoles"" (""UserId"", ""RoleId"")
        SELECT u.""Id"", r.""Id""
        FROM ""AspNetUsers"" u, ""AspNetRoles"" r
        WHERE u.""Email"" = 'ym090066@gmail.com' 
          AND r.""Name"" = 'superadmin'
        ON CONFLICT (""UserId"", ""RoleId"") DO NOTHING;
    ");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			// 1. Remove the link
			migrationBuilder.Sql(@"
        DELETE FROM ""AspNetUserRoles"" 
        WHERE ""UserId"" IN (SELECT ""Id"" FROM ""AspNetUsers"" WHERE ""Email"" = 'admin@example.com');
    ");

			// 2. Remove the user
			migrationBuilder.Sql(@"DELETE FROM ""AspNetUsers"" WHERE ""Email"" = 'admin@example.com';");

			// 3. Optional: Remove the role (Only do this if you are sure no one else uses it)
			// migrationBuilder.Sql(@"DELETE FROM ""AspNetRoles"" WHERE ""Name"" = 'Superadmin';");
		}
	}
}

