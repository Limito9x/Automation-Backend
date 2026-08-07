using System.Threading;
using System.Threading.Tasks;
using TickerQ.Utilities.Base;

namespace Automation.Files.Jobs;

public class TickerQJobs(FileJobs fileJobs)
{
    // Chạy mỗi ngày 1 lần vào lúc 2:00 AM sáng
    [TickerFunction("RunOrphanedCleanupAsync", "0 2 * * *")]
    public async Task RunOrphanedCleanupAsync(CancellationToken ct)
    {
        await fileJobs.CleanupOrphanedFiles(ct);
    }

    // Chạy 1 tuần 1 lần vào lúc 3:00 AM sáng Chủ Nhật
    [TickerFunction("RunUntrackedStorageCleanupAsync", "0 3 * * 0")]
    public async Task RunUntrackedStorageCleanupAsync(CancellationToken ct)
    {
        await fileJobs.CleanupUntrackedStorageFiles(ct);
    }
}

