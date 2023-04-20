﻿using AS2023Env.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AS2023Env.Controllers;

[Route("/employees")]
[SwaggerTag("Сотрудники")]
public class EmployeesController : BaseController<Employee>
{
    private readonly IStorage<StaffUnit> _staffUnitStorage;

    public EmployeesController(IStorage<Employee> storage, AuthService authService,
        IStorage<StaffUnit> staffUnitStorage) : base(storage, authService)
    {
        _staffUnitStorage = staffUnitStorage;
    }

    [HttpGet("List/{positionId}")]
    [SwaggerOperation("Получить список сотрудников по должности")]
    public async Task<List<Employee>> EmployeesByPosition(string positionId)
    {
        if (!CheckAuth()) return null;
        return await Storage.GetList(e => e.PositionId == positionId);
    }

    [HttpPost("Register")]
    [SwaggerOperation("Зарегистрировать нового сотрудника")]
    public async Task<RegisterEmployeeResult> RegisterEmployeeAsync(RegisterEmployeeRequest request)
    {
        if (!CheckAuth()) return null;

        StaffUnit unit = (await _staffUnitStorage.GetList(u => u.Id == request.StaffUnitId)).FirstOrDefault();
        if (unit == null)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new RegisterEmployeeResult($"Штатная единица с id = {request.StaffUnitId} не найдена");
        }
        if (unit.Status != StaffUnitStatus.Opened)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new RegisterEmployeeResult($"Штатная единица с id = {request.StaffUnitId} занята");
        }

        List<Employee> existing = await Storage.GetList(e => e.Email.ToLower() == request.Email.ToLower());
        if (existing.Count > 0)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new RegisterEmployeeResult($"Сотруник с email = {request.Email} уже зарегистрирован");
        }

        var newEmp = new Employee
        {
            Id = Guid.NewGuid().ToString(),
            PositionId = unit.PositionId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email
        };
        await Storage.Add(newEmp);
        unit.EmployeeId = newEmp.Id;
        await _staffUnitStorage.Update(unit);

        return new RegisterEmployeeResult(newEmp.Id);
    }
}