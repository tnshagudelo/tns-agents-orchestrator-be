using Domain.Entities;

namespace Domain.Interfaces
{
    /// <summary>
    /// Persiste estimaciones generadas por el ProjectManagerAgent.
    /// </summary>
    public interface IEstimationRepository
    {
        Task SaveAsync(ProjectEstimation estimation, CancellationToken ct = default);
        Task<ProjectEstimation?> GetBySessionAsync(Guid sessionId, CancellationToken ct = default);
        Task<IEnumerable<ProjectEstimation>> GetAllAsync(string createdBy, CancellationToken ct = default);
        Task UpdateStatusAsync(Guid estimationId, EstimationStatus status, CancellationToken ct = default);
    }
}
