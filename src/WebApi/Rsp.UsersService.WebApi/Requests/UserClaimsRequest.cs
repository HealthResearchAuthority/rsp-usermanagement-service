﻿namespace Rsp.UsersService.WebApi.Requests;

public class UserClaimsRequest
{
    public string Email { get; set; } = null!;

    public List<KeyValuePair<string, string>> Claims { get; set; } = [];
}