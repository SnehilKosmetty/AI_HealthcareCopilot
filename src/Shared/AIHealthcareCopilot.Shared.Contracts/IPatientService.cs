using AIHealthcareCopilot.Shared.Models;

namespace AIHealthcareCopilot.Shared.Contracts;

public interface IPatientService
{
    Task<Patient> GetPatientAsync(int id);
    Task<Patient> CreatePatientAsync(Patient patient);
    Task<Patient> UpdatePatientAsync(Patient patient);
    Task<bool> DeletePatientAsync(int id);
    Task<List<Patient>> SearchPatientsAsync(string searchTerm);
}
