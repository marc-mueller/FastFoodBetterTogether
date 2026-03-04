using Dapper;
using FinanceService.Entities;
using FinanceService.Storage.Storages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Controllers;

public class SqlInjectionController : ControllerBase
{
    private FinanceStorage _context;

    public SqlInjectionController(FinanceStorage context)
    {
        _context = context;
    }
    

    [HttpGet("SqlInjection/SearchPersonUnsecure/{name}")]
    public async Task<IActionResult> SearchPersonUnsecure(string name)
    {
        var conn = _context.Database.GetDbConnection();
        var query = "SELECT Id, FirstName, LastName FROM Person WHERE FirstName Like '%" + name + "%'";
        IEnumerable<Customer> persons;

        try
        {
            await conn.OpenAsync();
            persons = await conn.QueryAsync<Customer>(query);
        }

        finally
        {
            conn.Close();
        }
        return Ok(persons);
    }
}