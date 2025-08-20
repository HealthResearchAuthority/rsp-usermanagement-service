﻿namespace Rsp.UsersService.WebApi.Requests;

public class SearchUserRequest
{
    public string? SearchQuery { get; set; }
    public List<string> Country { get; set; } = [];
    public List<string> ReviewBody { get; set; } = [];
    public List<string> Role { get; set; } = [];
    public bool? Status { get; set; }
    public DateTime? FromDate { get; set; } = null;
    public DateTime? ToDate { get; set; } = null;
}