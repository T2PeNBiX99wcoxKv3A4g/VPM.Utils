using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using io.github.ykysnk.utils.Editor.Extensions;
using io.github.ykysnk.utils.Extensions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.PackageManager;

namespace io.github.ykysnk.utils.Editor;

public static class UpmInstaller
{
    private static int _isWorking;

    [PublicAPI]
    public static void Install(string[] packages) => _ = InstallAsync(packages);

    [PublicAPI]
    public static async Task InstallAsync(string[] packages) => await InstallAsync(packages, CancellationToken.None);

    [PublicAPI]
    public static async Task InstallAsync(string[] packages, CancellationToken token)
    {
        if (Interlocked.Exchange(ref _isWorking, 1) == 1) return;

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

        try
        {
            Progress.Report(progressId, 0f, "Checking installed packages...");

            var listRequest = await Client.List().AsTask(token);
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

                if (installed.TryGetValue(pkgName, out var currentVersion) && currentVersion == pkgVersion)
                {
                    Utils.Log(nameof(UpmInstaller), $"Skip (already installed): {pkg}");
                    Progress.Report(progressId, progress, $"Skip (already installed): {pkg}");
                    continue;
                }

                Utils.Log(nameof(UpmInstaller), $"Installing: {pkg}");
                Progress.Report(progressId, progress, $"Installing: {pkg}");

                var addRequest = await Client.Add(pkg).AsTask(token);
                if (addRequest.Status == StatusCode.Success)
                    Utils.Log(nameof(UpmInstaller), $"Installed: {addRequest.Result.packageId}");
                else
                    Utils.LogError(nameof(UpmInstaller), $"Failed: {addRequest.Error.message}");
            }

            Progress.Finish(progressId);
            Utils.Log(nameof(UpmInstaller), "All packages processed.");
        }
        catch (OperationCanceledException)
        {
            Progress.Finish(progressId, Progress.Status.Canceled);
            Progress.UnregisterCancelCallback(progressId);
            Utils.LogWarning(nameof(UpmInstaller), "Installation cancelled.");
        }
        catch (Exception ex)
        {
            Progress.Finish(progressId, Progress.Status.Failed);
            Progress.UnregisterCancelCallback(progressId);
            Utils.LogError(nameof(UpmInstaller), $"Installation Error: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            Interlocked.Exchange(ref _isWorking, 0);
        }
    }

    [PublicAPI]
    public static void Remove(string[] packages) => _ = RemoveAsync(packages);

    [PublicAPI]
    public static async Task RemoveAsync(string[] packages) => await RemoveAsync(packages, CancellationToken.None);

    [PublicAPI]
    public static async Task RemoveAsync(string[] packages, CancellationToken token)
    {
        if (Interlocked.Exchange(ref _isWorking, 1) == 1) return;

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

        try
        {
            Progress.Report(progressId, 0f, "Checking installed packages...");

            var listRequest = await Client.List().AsTask(token);
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

                if (installed.TryGetValue(pkgName, out var currentVersion) && currentVersion != pkgVersion)
                {
                    Utils.Log(nameof(UpmInstaller), $"Skip (not installed): {pkg}");
                    Progress.Report(progressId, progress, $"Skip (not installed): {pkg}");
                    continue;
                }

                Utils.Log(nameof(UpmInstaller), $"Removing: {pkg}");
                Progress.Report(progressId, progress, $"Removing: {pkg}");

                var removeRequest = await Client.Remove(pkg).AsTask(token);
                if (removeRequest.Status == StatusCode.Success)
                    Utils.Log(nameof(UpmInstaller), $"Removed: {removeRequest.PackageIdOrName}");
                else
                    Utils.LogError(nameof(UpmInstaller), $"Failed: {removeRequest.Error.message}");
            }

            Progress.Finish(progressId);
            Utils.Log(nameof(UpmInstaller), "All packages processed.");
        }
        catch (OperationCanceledException)
        {
            Progress.Finish(progressId, Progress.Status.Canceled);
            Progress.UnregisterCancelCallback(progressId);
            Utils.LogWarning(nameof(UpmInstaller), "Installation cancelled.");
        }
        catch (Exception ex)
        {
            Progress.Finish(progressId, Progress.Status.Failed);
            Progress.UnregisterCancelCallback(progressId);
            Utils.LogError(nameof(UpmInstaller), $"Installation Error: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            Interlocked.Exchange(ref _isWorking, 0);
        }
    }

    [PublicAPI]
    public static async Task<string[]> UpdateAsync() => await UpdateAsync(CancellationToken.None);

    [PublicAPI]
    public static async Task<string[]> UpdateAsync(CancellationToken token)
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

        try
        {
            Progress.Report(progressId, 0f, "Checking installed packages...");

            var listRequest = await Client.List().AsTask(token);
            var total = listRequest.Result.Count();
            var index = 0;

            foreach (var packageInfo in listRequest.Result)
            {
                index++;
                var progress = (float)index / total;

                Utils.Log(nameof(UpmInstaller), $"Checking update for: {packageInfo.name}");
                Progress.Report(progressId, progress, $"Checking update for: {packageInfo.name}");

                var searchResult = await Client.Search(packageInfo.name).AsTask(token);
                if (searchResult.Status == StatusCode.Failure || !searchResult.Result.Any())
                {
                    Utils.LogWarning(nameof(UpmInstaller), $"Failed to check update for: {packageInfo.name}");
                    continue;
                }

                if (searchResult.Result[0].version == packageInfo.version) continue;

                Utils.Log(nameof(UpmInstaller),
                    $"Update found for: {packageInfo.name} - {packageInfo.version} -> {searchResult.Result[0].version}");
                packages.Add($"{packageInfo.name}@{searchResult.Result[0].version}");
            }

            Progress.Finish(progressId);
            Utils.Log(nameof(UpmInstaller), "All packages processed.");
        }
        catch (OperationCanceledException)
        {
            Progress.Finish(progressId, Progress.Status.Canceled);
            Progress.UnregisterCancelCallback(progressId);
            Utils.LogWarning(nameof(UpmInstaller), "Installation cancelled.");
        }
        catch (Exception ex)
        {
            Progress.Finish(progressId, Progress.Status.Failed);
            Progress.UnregisterCancelCallback(progressId);
            Utils.LogError(nameof(UpmInstaller), $"Installation Error: {ex.Message}\n{ex.StackTrace}");
        }

        return packages.ToArray();
    }

    [PublicAPI]
    public static void Upgrade() => _ = UpgradeAsync();

    [PublicAPI]
    public static async Task UpgradeAsync() => await UpgradeAsync(CancellationToken.None);

    [PublicAPI]
    public static async Task UpgradeAsync(CancellationToken token)
    {
        var packages = await UpdateAsync(token);

        if (packages.Length < 1)
        {
            Utils.Log(nameof(UpmInstaller), "No packages to upgrade.");
            return;
        }

        await InstallAsync(packages, token);
    }
}