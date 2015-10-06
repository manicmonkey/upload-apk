using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class RefreshTokenResponse
{
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public int ExpiresIn { get; set; }
}