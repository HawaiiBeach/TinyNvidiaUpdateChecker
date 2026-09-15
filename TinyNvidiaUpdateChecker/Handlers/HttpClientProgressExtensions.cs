using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientProgress
{
    public static class HttpClientProgressExtensions
    {
        private const int BufferSize = 1048576;
        private const long SegmentedDownloadThreshold = 20L * 1024 * 1024;
        private const int SegmentCount = 6;
        private const int ProgressIntervalMilliseconds = 200;
        private static readonly TimeSpan HeaderTimeout = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan IdleTimeout = TimeSpan.FromSeconds(30);

        public static async Task DownloadDataAsync(this HttpClient client, string requestUrl, Stream destination, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(destination);
            if (!destination.CanWrite) throw new ArgumentException("The destination must be writable.", nameof(destination));
            ProgressState state = new(progress);
            if (destination is FileStream file && file.CanSeek && file.Position == 0 && file.Length == 0)
            {
                if (await TrySegmentedDownloadAsync(client, requestUrl, file.Name, state, cancellationToken).ConfigureAwait(false)) return;
                file.SetLength(0);
                file.Position = 0;
            }

            using HttpRequestMessage request = new(HttpMethod.Get, requestUrl);
            using HttpResponseMessage response = await SendAsync(client, request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using CancellationTokenSource idle = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            byte[] buffer = new byte[BufferSize];
            long totalBytes = 0;
            long? length = response.Content.Headers.ContentLength;
            while (true)
            {
                idle.CancelAfter(IdleTimeout);
                int bytesRead = await source.ReadAsync(buffer, idle.Token).ConfigureAwait(false);
                if (bytesRead == 0) break;
                idle.CancelAfter(IdleTimeout);
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), idle.Token).ConfigureAwait(false);
                totalBytes += bytesRead;
                if (length is > 0) state.Report(totalBytes, length.Value);
            }
            if (length.HasValue && totalBytes != length.Value) throw new IOException("The response length did not match its content length.");
            cancellationToken.ThrowIfCancellationRequested();
            state.Complete();
        }

        private static async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(HeaderTimeout);
            return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
        }

        private static bool ValidRange(ContentRangeHeaderValue range, long start, long end, long length)
        {
            return range != null && string.Equals(range.Unit, "bytes", StringComparison.OrdinalIgnoreCase)
                && range.From == start && range.To == end && range.Length == length;
        }

        private static async Task<bool> TrySegmentedDownloadAsync(HttpClient client, string url, string path, ProgressState state, CancellationToken cancellationToken)
        {
            try
            {
                using HttpRequestMessage probe = new(HttpMethod.Get, url);
                probe.Headers.Range = new RangeHeaderValue(0, 0);
                using HttpResponseMessage response = await SendAsync(client, probe, cancellationToken).ConfigureAwait(false);
                ContentRangeHeaderValue range = response.Content.Headers.ContentRange;
                if (response.StatusCode != HttpStatusCode.PartialContent || range?.Length is not long length
                    || length <= SegmentedDownloadThreshold || !ValidRange(range, 0, 0, length)) return false;
                EntityTagHeaderValue etag = response.Headers.ETag;
                DateTimeOffset? lastModified = response.Content.Headers.LastModified;
                if ((etag == null || etag.IsWeak) && !lastModified.HasValue) return false;
                using (FileStream target = new(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite)) target.SetLength(length);
                using CancellationTokenSource segments = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                int count = (int)Math.Min(SegmentCount, 2 + (length - 1) / (8 * 1024 * 1024));
                Task[] tasks = new Task[count];
                for (int i = 0; i < count; i++)
                {
                    long start = length / count * i;
                    long end = i == count - 1 ? length - 1 : length / count * (i + 1) - 1;
                    tasks[i] = DownloadSegmentAsync(client, url, path, start, end, length, etag, lastModified, state, segments);
                }
                await Task.WhenAll(tasks).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                state.Complete();
                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex) when (ex is HttpRequestException or IOException or OperationCanceledException)
            {
                return false;
            }
        }

        private static async Task DownloadSegmentAsync(HttpClient client, string url, string path, long start, long end, long totalLength,
            EntityTagHeaderValue etag, DateTimeOffset? lastModified, ProgressState state, CancellationTokenSource segments)
        {
            try
            {
                CancellationToken token = segments.Token;
                using HttpRequestMessage request = new(HttpMethod.Get, url);
                request.Headers.Range = new RangeHeaderValue(start, end);
                request.Headers.IfRange = etag != null && !etag.IsWeak ? new RangeConditionHeaderValue(etag) : new RangeConditionHeaderValue(lastModified.Value);
                using HttpResponseMessage response = await SendAsync(client, request, token).ConfigureAwait(false);
                if (response.StatusCode != HttpStatusCode.PartialContent || !ValidRange(response.Content.Headers.ContentRange, start, end, totalLength))
                    throw new IOException("The server returned an invalid content range.");
                if ((etag != null && !etag.Equals(response.Headers.ETag))
                    || (lastModified.HasValue && lastModified != response.Content.Headers.LastModified))
                    throw new IOException("The file changed during the ranged download.");
                await using Stream source = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
                await using FileStream target = new(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite, BufferSize, useAsync: true);
                target.Position = start;
                using CancellationTokenSource idle = CancellationTokenSource.CreateLinkedTokenSource(token);
                byte[] buffer = new byte[BufferSize];
                long position = start;
                while (true)
                {
                    idle.CancelAfter(IdleTimeout);
                    int bytesRead = await source.ReadAsync(buffer, idle.Token).ConfigureAwait(false);
                    if (bytesRead == 0) break;
                    int bytesToWrite = (int)Math.Min(bytesRead, end - position + 1);
                    idle.CancelAfter(IdleTimeout);
                    await target.WriteAsync(buffer.AsMemory(0, bytesToWrite), idle.Token).ConfigureAwait(false);
                    position += bytesToWrite;
                    if (bytesRead != bytesToWrite) throw new IOException("The ranged response exceeded its allocated slice.");
                    state.Add(bytesToWrite, totalLength);
                }
                if (position != end + 1) throw new IOException("The ranged response was incomplete.");
                idle.CancelAfter(IdleTimeout);
                await target.FlushAsync(idle.Token).ConfigureAwait(false);
            }
            catch
            {
                segments.Cancel();
                throw;
            }
        }

        private sealed class ProgressState(IProgress<float> progress)
        {
            private readonly object sync = new();
            private long totalBytes;
            private long lastReport = Stopwatch.GetTimestamp();
            private float highestPercentage;

            public void Add(int bytes, long length)
            {
                lock (sync)
                {
                    totalBytes += bytes;
                    Report(totalBytes, length);
                }
            }

            public void Report(long bytes, long length)
            {
                lock (sync)
                {
                    long now = Stopwatch.GetTimestamp();
                    float percentage = (float)Math.Min(99.9, bytes * 100d / length);
                    if (percentage <= highestPercentage || (now - lastReport) * 1000d / Stopwatch.Frequency < ProgressIntervalMilliseconds) return;
                    highestPercentage = percentage;
                    lastReport = now;
                    progress?.Report(percentage);
                }
            }

            public void Complete()
            {
                lock (sync)
                {
                    highestPercentage = 100f;
                    progress?.Report(100f);
                }
            }
        }
    }
}