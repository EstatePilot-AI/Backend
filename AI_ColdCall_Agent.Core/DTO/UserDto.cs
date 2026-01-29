using System;
using System.Collections.Generic;
using System.Text;

namespace DTO;

public class UserDto
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public string Email { get; set; }
	public string PhoneNumber { get; set; }
	public string Role { get; set; }
}
