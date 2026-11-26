using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using io.github.ykysnk.utils.Editor.Extensions;
using io.github.ykysnk.utils.Extensions;
using io.github.ykysnk.utils.NonUdon;
using io.github.ykysnk.utils.NonUdon.Extensions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.PackageManager;

namespace io.github.ykysnk.utils.Editor
{
    /// <summary>
    ///     Installs, removes and updates Unity Package Manager packages from editor code. Every operation shows a
    ///     sticky editor progress bar whose cancel button stops it, unless the editor is compiling or updating at that
    ///     moment. Install and remove share one lock, so only one of them runs at a time; a second call returns without
    ///     doing anything.
    /// </summary>
    public static class UpmInstaller
    {
        private static int _isWorking;

        /// <summary>Longest a single registry search may take before the check moves on to the next package.</summary>
        private static readonly TimeSpan UpdateCheckTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        ///     Starts installing the given packages and returns immediately.
        /// </summary>
        /// <param name="packages">Entries to install, either <c>name</c> or <c>name@version</c>.</param>
        [PublicAPI]
        public static void Install(string[] packages) => _ = InstallAsync(packages);

        /// <summary>
        ///     Installs the given packages and completes when all of them were processed.
        /// </summary>
        /// <param name="packages">Entries to install, either <c>name</c> or <c>name@version</c>.</param>
        /// <returns>Task that finishes once every entry was installed or skipped.</returns>
        [PublicAPI]
        public static async Task InstallAsync(string[] packages) => await InstallAsync(packages, CancellationToken.None);

        /// <summary>
        ///     Installs the given packages. Entries whose package is already installed at the requested version are
        ///     skipped; a failed entry is logged and the remaining ones are still processed.
        /// </summary>
        /// <param name="packages">Entries to install, either <c>name</c> or <c>name@version</c>.</param>
        /// <param name="token">
        ///     Stops the operation; it then finishes without throwing and the progress bar closes as cancelled.
        /// </param>
        /// <returns>Task that finishes once every entry was installed or skipped.</returns>
        [PublicAPI]
        public static async Task InstallAsync(string[] packages, CancellationToken token)
        {
            if (Interlocked.Exchange(ref _isWorking, 1) == 1) return;

            try
            {
                await InstallCore(packages, token);
            }
            finally
            {
                Interlocked.Exchange(ref _isWorking, 0);
            }
        }

        private static async Task InstallCore(string[] packages, CancellationToken token)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var progressId = Progress.Start(
                "Installing UPM Packages",
                "Preparing...",
                Progress.Options.Sticky
            );

            Progress.RegisterCancelCallback(progressId, () =>
            {
                if (cts.IsCancellationRequested || EditorApplication.isCompiling || EditorApplication.isUpdating)
                    return false;
                Utils.Log(nameof(UpmInstaller), "Cancel requested by user.");
                cts.Cancel();
                return true;
            });

            var result = await Try.Run(async () =>
            {
                Progress.Report(progressId, 0f, "Checking installed packages...");

                var listRequest = await Client.List().AsTask(cts.Token);
                var installed = listRequest.Result.ToDictionary(p => p.name, p => p.version);

                var total = packages.Length;
                var index = 0;

                foreach (var pkg in packages)
                {
                    index++;
                    var progress = (float)index / total;
                    var parts = pkg.Split('@');
                    var pkgName = parts[0];
                    var pkgVersion = parts.GetValueOrDefault(1);

                    if (cts.IsCancellationRequested)
                        throw new OperationCanceledException(cts.Token);

                    if (installed.TryGetValue(pkgName, out var currentVersion) &&
                        (pkgVersion == null || currentVersion == pkgVersion))
                    {
                        Utils.Log(nameof(UpmInstaller), $"Skip (already installed): {pkg}");
                        Progress.Report(progressId, progress, $"Skip (already installed): {pkg}");
                        continue;
                    }

                    Utils.Log(nameof(UpmInstaller), $"Installing: {pkg}");
                    Progress.Report(progressId, progress, $"Installing: {pkg}");

                    var addRequest = await Client.Add(pkg).AsTask(cts.Token);
                    if (addRequest.Status == StatusCode.Success)
                        Utils.Log(nameof(UpmInstaller), $"Installed: {addRequest.Result.packageId}");
                    else
                        Utils.LogError(nameof(UpmInstaller), $"Failed: {addRequest.Error.message}");
                }

                Progress.Finish(progressId);
                Utils.Log(nameof(UpmInstaller), "All packages are processed.");
            });

            result.OnFailure(ex =>
            {
                if (ex is OperationCanceledException)
                {
                    Progress.Finish(progressId, Progress.Status.Canceled);
                    Utils.LogWarning(nameof(UpmInstaller), "Installation cancelled.");
                }
                else
                {
                    Progress.Finish(progressId, Progress.Status.Failed);
                    Utils.LogError(nameof(UpmInstaller), $"Installation Error: {ex.Message}\n{ex.StackTrace}");
                }
            });

            Progress.UnregisterCancelCallback(progressId);
            cts.Dispose();
        }

        /// <summary>
        ///     Starts removing the given packages and returns immediately.
        /// </summary>
        /// <param name="packages">Package names to remove.</param>
        [PublicAPI]
        public static void Remove(string[] packages) => _ = RemoveAsync(packages);

        /// <summary>
        ///     Removes the given packages and completes when all of them were processed.
        /// </summary>
        /// <param name="packages">Package names to remove.</param>
        /// <returns>Task that finishes once every package was removed or failed.</returns>
        [PublicAPI]
        public static async Task RemoveAsync(string[] packages) => await RemoveAsync(packages, CancellationToken.None);

        /// <summary>
        ///     Removes the given packages. A failed package is logged and the remaining ones are still processed.
        /// </summary>
        /// <param name="packages">Package names to remove.</param>
        /// <param name="token">
        ///     Stops the operation; it then finishes without throwing and the progress bar closes as cancelled.
        /// </param>
        /// <returns>Task that finishes once every package was removed or failed.</returns>
        [PublicAPI]
        public static async Task RemoveAsync(string[] packages, CancellationToken token)
        {
            if (Interlocked.Exchange(ref _isWorking, 1) == 1) return;

            try
            {
                await RemoveCore(packages, token);
            }
            finally
            {
                Interlocked.Exchange(ref _isWorking, 0);
            }
        }

        private static async Task RemoveCore(string[] packages, CancellationToken token)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var progressId = Progress.Start(
                "Removing UPM Packages",
                "Preparing...",
                Progress.Options.Sticky
            );

            Progress.RegisterCancelCallback(progressId, () =>
            {
                if (cts.IsCancellationRequested || EditorApplication.isCompiling || EditorApplication.isUpdating)
                    return false;
                Utils.Log(nameof(UpmInstaller), "Cancel requested by user.");
                cts.Cancel();
                return true;
            });

            var result = await Try.Run(async () =>
            {
                Progress.Report(progressId, 0f, "Checking installed packages...");

                var listRequest = await Client.List().AsTask(cts.Token);
                var installed = listRequest.Result.ToDictionary(p => p.name, p => p.version);

                var total = packages.Length;
                var index = 0;

                foreach (var pkg in packages)
                {
                    index++;
                    var progress = (float)index / total;

                    if (cts.IsCancellationRequested)
                        throw new OperationCanceledException(cts.Token);

                    Utils.Log(nameof(UpmInstaller), $"Removing: {pkg}");
                    Progress.Report(progressId, progress, $"Removing: {pkg}");

                    var removeRequest = await Client.Remove(pkg).AsTask(cts.Token);
                    if (removeRequest.Status == StatusCode.Success)
                        Utils.Log(nameof(UpmInstaller), $"Removed: {removeRequest.PackageIdOrName}");
                    else
                        Utils.LogError(nameof(UpmInstaller), $"Failed: {removeRequest.Error.message}");
                }

                Progress.Finish(progressId);
                Utils.Log(nameof(UpmInstaller), "All packages processed.");
            });

            result.OnFailure(ex =>
            {
                if (ex is OperationCanceledException)
                {
                    Progress.Finish(progressId, Progress.Status.Canceled);
                    Utils.LogWarning(nameof(UpmInstaller), "Removal cancelled.");
                }
                else
                {
                    Progress.Finish(progressId, Progress.Status.Failed);
                    Utils.LogError(nameof(UpmInstaller), $"Removal Error: {ex.Message}\n{ex.StackTrace}");
                }
            });

            Progress.UnregisterCancelCallback(progressId);
            cts.Dispose();
        }

        /// <summary>
        ///     Checks every installed package against the registry and lists the ones with a different version.
        /// </summary>
        /// <returns>Entries of the form <c>name@version</c>; empty when nothing is outdated.</returns>
        [PublicAPI]
        public static async Task<string[]> UpdateAsync() => await UpdateAsync(CancellationToken.None);

        /// <summary>
        ///     Checks every installed registry package against the registry and lists the ones that have a newer
        ///     version. Built-in, embedded and local packages are not searched: they have no registry update.
        /// </summary>
        /// <param name="token">Stops the check; searches already in flight are not aborted.</param>
        /// <returns>
        ///     Entries of the form <c>name@version</c> for the outdated packages; empty when nothing is outdated. The
        ///     entries found before a failure are still returned.
        /// </returns>
        [PublicAPI]
        public static async Task<string[]> UpdateAsync(CancellationToken token) => await UpdateCore(token);

        private static async Task<string[]> UpdateCore(CancellationToken token)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var progressId = Progress.Start(
                "Get UPM Packages Update",
                "Preparing...",
                Progress.Options.Sticky
            );

            Progress.RegisterCancelCallback(progressId, () =>
            {
                if (cts.IsCancellationRequested || EditorApplication.isCompiling || EditorApplication.isUpdating)
                    return false;
                Utils.Log(nameof(UpmInstaller), "Cancel requested by user.");
                cts.Cancel();
                return true;
            });

            var packages = new List<string>();

            var result = await Try.Run(async () =>
            {
                Progress.Report(progressId, 0f, "Checking installed packages...");

                var listRequest = await Client.List().AsTask(cts.Token);

                // Built-in, embedded and local packages cannot be updated from the registry. Searching them was the
                // bulk of the old check: one round trip per installed package, most of which could never match.
                var candidates = listRequest.Result
                    .Where(packageInfo => packageInfo.source == PackageSource.Registry)
                    .ToArray();

                var total = candidates.Length;
                var completed = 0;
                var outdated = new ConcurrentBag<string>();
                var queue = new ConcurrentQueue<UnityEditor.PackageManager.PackageInfo>(candidates);

                // Searches are latency bound, so several run at once. One at a time made the check as slow as the
                // registry latency multiplied by the package count.
                var workerCount = Math.Min(16, Math.Max(1, total));
                var workers = Enumerable.Range(0, workerCount).Select(async _ =>
                {
                    while (queue.TryDequeue(out var packageInfo))
                    {
                        cts.Token.ThrowIfCancellationRequested();

                        Utils.Log(nameof(UpmInstaller), $"Checking update for: {packageInfo.name}");

                        var searchTask = Client.Search(packageInfo.name).AsTask(cts.Token);
                        var finished = await Task.WhenAny(searchTask, Task.Delay(UpdateCheckTimeout, cts.Token));

                        var done = Interlocked.Increment(ref completed);
                        Progress.Report(progressId, (float)done / total,
                            $"Checked {done}/{total}: {packageInfo.name}");

                        if (finished != searchTask)
                        {
                            cts.Token.ThrowIfCancellationRequested();
                            Utils.LogWarning(nameof(UpmInstaller),
                                $"Timed out checking update for: {packageInfo.name}");
                            continue;
                        }

                        var searchResult = await searchTask;
                        if (searchResult.Status == StatusCode.Failure || !searchResult.Result.Any())
                        {
                            Utils.LogWarning(nameof(UpmInstaller), $"Failed to check update for: {packageInfo.name}");
                            continue;
                        }

                        var newest = FindNewestVersion(packageInfo.name, packageInfo.version, searchResult.Result);
                        if (newest == null) continue;

                        Utils.Log(nameof(UpmInstaller),
                            $"Update found for: {packageInfo.name} - {packageInfo.version} -> {newest}");
                        outdated.Add($"{packageInfo.name}@{newest}");
                    }
                }).ToArray();

                await Task.WhenAll(workers);
                packages.AddRange(outdated.OrderBy(entry => entry, StringComparer.Ordinal));

                Progress.Finish(progressId);
                Utils.Log(nameof(UpmInstaller), "All packages processed.");
            });

            result.OnFailure(ex =>
            {
                if (ex is OperationCanceledException)
                {
                    Progress.Finish(progressId, Progress.Status.Canceled);
                    Utils.LogWarning(nameof(UpmInstaller), "Update check cancelled.");
                }
                else
                {
                    Progress.Finish(progressId, Progress.Status.Failed);
                    Utils.LogError(nameof(UpmInstaller), $"Update check Error: {ex.Message}\n{ex.StackTrace}");
                }
            });

            Progress.UnregisterCancelCallback(progressId);
            cts.Dispose();

            return packages.ToArray();
        }

        /// <summary>
        ///     Checks for updates and starts installing them, then returns immediately.
        /// </summary>
        [PublicAPI]
        public static void Upgrade() => _ = UpgradeAsync();

        /// <summary>
        ///     Checks for updates and installs them. Does nothing when every package is up to date.
        /// </summary>
        /// <returns>Task that finishes after the update check and the install it triggered.</returns>
        [PublicAPI]
        public static async Task UpgradeAsync() => await UpgradeAsync(CancellationToken.None);

        /// <summary>
        ///     Checks for updates and installs them. Does nothing when every package is up to date or when an install or
        ///     removal is already running.
        /// </summary>
        /// <param name="token">Stops the update check or the following install; nothing is thrown when it does.</param>
        /// <returns>Task that finishes after the update check and the install it triggered.</returns>
        [PublicAPI]
        public static async Task UpgradeAsync(CancellationToken token)
        {
            if (Interlocked.Exchange(ref _isWorking, 1) == 1) return;

            try
            {
                var packages = await UpdateCore(token);

                if (packages.Length < 1)
                {
                    Utils.Log(nameof(UpmInstaller), "No packages to upgrade.");
                    return;
                }

                await InstallCore(packages, token);
            }
            finally
            {
                Interlocked.Exchange(ref _isWorking, 0);
            }
        }

        /// <summary>
        ///     Returns the newest registry version of <paramref name="packageName" /> when it is strictly newer than
        ///     <paramref name="installedVersion" />, otherwise null.
        /// </summary>
        private static string? FindNewestVersion(string packageName, string installedVersion,
            IEnumerable<UnityEditor.PackageManager.PackageInfo> results)
        {
            string? bestVersion = null;

            foreach (var result in results)
            {
                if (result.name != packageName) continue;
                if (bestVersion == null || CompareVersions(result.version, bestVersion) > 0)
                    bestVersion = result.version;
            }

            return bestVersion != null && CompareVersions(bestVersion, installedVersion) > 0
                ? bestVersion
                : null;
        }

        /// <summary>
        ///     Compares two semantic versions: the numeric part component by component, then the pre-release suffix,
        ///     where a release sorts above any pre-release of the same numbers.
        /// </summary>
        private static int CompareVersions(string left, string right)
        {
            var leftDash = left.IndexOf('-');
            var rightDash = right.IndexOf('-');
            var leftCore = leftDash < 0 ? left : left.Substring(0, leftDash);
            var rightCore = rightDash < 0 ? right : right.Substring(0, rightDash);

            var core = CompareNumericParts(leftCore, rightCore);
            if (core != 0) return core;

            var hasLeftPre = leftDash >= 0;
            var hasRightPre = rightDash >= 0;
            if (!hasLeftPre && !hasRightPre) return 0;
            if (!hasLeftPre) return 1;
            if (!hasRightPre) return -1;

            return string.CompareOrdinal(left.Substring(leftDash + 1), right.Substring(rightDash + 1));
        }

        private static int CompareNumericParts(string left, string right)
        {
            var leftParts = left.Split('.');
            var rightParts = right.Split('.');
            var count = Math.Max(leftParts.Length, rightParts.Length);

            for (var i = 0; i < count; i++)
            {
                var leftValue = i < leftParts.Length && int.TryParse(leftParts[i], out var a) ? a : 0;
                var rightValue = i < rightParts.Length && int.TryParse(rightParts[i], out var b) ? b : 0;
                if (leftValue != rightValue) return leftValue.CompareTo(rightValue);
            }

            return 0;
        }
    }
}