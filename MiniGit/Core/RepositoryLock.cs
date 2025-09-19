using MiniGit.Utils;

namespace MiniGit.Core
{
    public static class RepositoryLock
    {
        private static readonly object _lockObject = new object();
        private const string LockFileName = "repo.lock";
        private static readonly string LockFilePath = Path.Combine(".minigit", LockFileName);

        public static bool ExecuteWithLock(Action action, int timeoutMs = 30000)
        {
            Logger.INFO("Attempting to acquire repository lock...");

            bool lockAcquired = false;

            try
            {
                if (Monitor.TryEnter(_lockObject, timeoutMs))
                {
                    lockAcquired = true;

                    CreateLockFile();
                    Logger.INFO("Repository lock acquired successfully");

                    action();

                    Logger.INFO("Repository operation completed successfully");
                    return true;
                }
                else
                {
                    Logger.ERROR("Failed to acquire repository lock within timeout");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.ERROR($"Error during locked repository operation: {ex.Message}");
                throw;
            }
            finally
            {
                if (lockAcquired)
                {
                    RemoveLockFile();
                    Monitor.Exit(_lockObject);
                    Logger.INFO("Repository lock released");
                }
            }
        }

        private static void CreateLockFile()
        {
            try
            {
                var dir = Path.GetDirectoryName(LockFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var lockInfo = $"PID: {Environment.ProcessId}\nTime: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                File.WriteAllText(LockFilePath, lockInfo);
                Logger.DEBUG($"Created repository lock file: {LockFilePath}");
            }
            catch (Exception ex)
            {
                Logger.WARN($"Failed to create lock file: {ex.Message}");
            }
        }

        private static void RemoveLockFile()
        {
            try
            {
                if (File.Exists(LockFilePath))
                {
                    File.Delete(LockFilePath);
                    Logger.DEBUG($"Removed repository lock file: {LockFilePath}");
                }
            }
            catch (Exception ex)
            {
                Logger.WARN($"Failed to remove lock file: {ex.Message}");
            }
        }

        public static bool IsLocked()
        {
            return File.Exists(LockFilePath);
        }
    }
}