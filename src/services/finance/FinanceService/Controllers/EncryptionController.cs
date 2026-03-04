using FinanceService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceService.Controllers;

public class EncryptionController : ControllerBase
{
    private IBadEncryptionService _badEncryptionService;

    public EncryptionController(IBadEncryptionService badEncryptionService)
    {
        _badEncryptionService = badEncryptionService;
    }

    [HttpGet("Encryption/EncryptString/{input}")]
    public IActionResult EncryptString(string input)
    {
        return Ok(_badEncryptionService.Encrypt(input));
    }
}