﻿using System.Data;
using System.Linq.Expressions;
using AS2023Env.Models;

namespace AS2023Env.Data;

public class StaffUnitStorage : IStorage<StaffUnit>
{
    private readonly IStorage<Employee> _employeeStorage;
    private readonly List<StaffUnit> _staffUnits = new();

    public StaffUnitStorage(IStorage<Employee> employeeStorage)
    {
        _employeeStorage = employeeStorage;
    }

    public async Task Init()
    {
        List<Employee> employees = await _employeeStorage.GetList();
        foreach (Employee employee in employees)
        {
            var newUnit = new StaffUnit
            {
                Id = Guid.NewGuid().ToString(),
                PositionId = employee.PositionId,
                EmployeeId = employee.Id
            };

            _staffUnits.Add(newUnit);
        }
    }

    public Task<List<StaffUnit>> GetList(Expression<Func<StaffUnit, bool>> filter = null)
    {
        return Task.FromResult(filter == null ? _staffUnits : _staffUnits.Where(filter.Compile()).ToList());
    }

    public Task Add(StaffUnit item)
    {
        throw new InvalidOperationException();
    }

    public Task Update(StaffUnit item)
    {
        StaffUnit found = _staffUnits.FirstOrDefault(e => e.Id == item.Id);
        if (found == null)
        {
            throw new DataException($"Сотрудник с id = {item.Id} не найден");
        }

        int index = _staffUnits.IndexOf(found);
        _staffUnits.RemoveAt(index);
        _staffUnits.Insert(index, item);
        
        return Task.CompletedTask;
    }

    public Task Delete(string itemId)
    {
        throw new InvalidOperationException();
    }
}