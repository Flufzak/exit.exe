// using Microsoft.AspNetCore.Mvc;

// namespace Exit.exe.Web.Controllers;

// [ApiController]
// [Route("api/ai-test")]
// public sealed class AiTestController : ControllerBase
// {
//     [HttpGet]
//     public async Task<IActionResult> TestAi()
//     {
//         var client = new HttpClient();
//         client.BaseAddress = new Uri("http://127.0.0.1:1234");

//         var request = new
//         {
//             model = "qwen2.5-7b-instruct-1m",
//             messages = new[]
//             {
//                 new { role = "user", content = "Explain hangman in one sentence." }
//             },
//             temperature = 0.7
//         };

//         var response = await client.PostAsJsonAsync("/v1/chat/completions", request);
//         var content = await response.Content.ReadAsStringAsync();

//         return Ok(content);
//     }
// }