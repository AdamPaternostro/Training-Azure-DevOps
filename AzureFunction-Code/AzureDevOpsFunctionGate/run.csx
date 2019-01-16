#r "Newtonsoft.Json"
#r "Microsoft.AspNetCore.Mvc.Formatters.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");

    var returnValue = new { status = "true"};

    return new Microsoft.AspNetCore.Mvc.JsonResult(returnValue);        
}
