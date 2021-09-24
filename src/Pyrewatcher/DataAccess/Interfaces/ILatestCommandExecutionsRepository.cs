using System;
using System.Threading.Tasks;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface ILatestCommandExecutionsRepository
  {
    Task<DateTime?> GetLatestExecutionAsync(long broadcasterId, long commandId);
    Task<bool> InsertLatestExecution(long broadcasterId, long commandId, DateTime timestampUtc);
    Task<bool> UpdateLatestExecution(long broadcasterId, long commandId, DateTime timestampUtc);
  }
}