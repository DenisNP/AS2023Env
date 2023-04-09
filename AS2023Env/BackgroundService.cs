﻿using AS2023Env.Models;

namespace AS2023Env;

public class BackgroundService : IHostedService
{
    private readonly IStorage<StaffUnit> _staffUnitStorage;
    private readonly IStorage<Employee> _employeeStorage;
    private readonly ILogger<BackgroundService> _logger;

    private Timer _timer = null;
    
    public BackgroundService(IStorage<StaffUnit> staffUnitStorage, IStorage<Employee> employeeStorage, ILogger<BackgroundService> logger)
    {
        _staffUnitStorage = staffUnitStorage;
        _employeeStorage = employeeStorage;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(FireEmployee, null, Constants.FireEmployeeDelay, Constants.FireEmployeeDelay);
        _logger.LogInformation("Фоновый сервис стартовал");
        return Task.CompletedTask;
    }

    private async void FireEmployee(object _)
    {
        List<StaffUnit> activeStaffUnits = await _staffUnitStorage.GetList(u => u.Active);
        if (activeStaffUnits.Count >= Constants.MaximumActiveStaffUnits)
        {
            return;
        }

        List<Employee> employees = await _employeeStorage.GetList();
        var rng = new Random();
        int index = rng.Next(0, employees.Count);

        Employee pickToFire = employees[index];
        StaffUnit staffUnit = (await _staffUnitStorage.GetList(u => u.EmployeeId == pickToFire.Id)).FirstOrDefault();
        if (staffUnit != null)
        {
            staffUnit.EmployeeId = null;
            await _staffUnitStorage.Update(staffUnit);
        }
        await _employeeStorage.Delete(pickToFire.Id);

        _logger.LogInformation("{EmployeeId} уволен, позиция {StaffUnitId} освобождена", pickToFire.Id, staffUnit?.Id);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Change(Timeout.Infinite, 0);
        _logger.LogInformation("Фоновый сервис останавливается");
        return Task.CompletedTask;
    }
}