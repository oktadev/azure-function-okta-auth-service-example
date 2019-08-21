using System;

namespace AuthService
{
	public class Session
	{
		public string Token { get; set; }
		public string ExpiresAt { get; set; }
	}
}
