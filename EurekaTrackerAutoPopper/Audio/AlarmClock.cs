using System;
using System.Threading;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace EurekaTrackerAutoPopper.Audio;

public class AlarmClock
{
    private bool IsRunning;
    private CancellationTokenSource TokenSource = new();

    public void StartAlarm()
    {
        if (IsRunning)
            return;

        IsRunning = true;
        TokenSource = new CancellationTokenSource();

        Task.Run(async () =>
        {
            try
            {
                await PlaySoundLoop(TokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Triggered by `Task.Delay()`, so we can safely ignore it
            }
            catch (Exception ex)
            {
                Plugin.Log.Error(ex, "Unable to start the alarm.");
            }

            IsRunning = false;
        });
    }

    public void StopAlarm()
    {
        if (!IsRunning)
            return;

        TokenSource.Cancel();
    }

    private async Task PlaySoundLoop(CancellationToken token)
    {
        for (var counter = 0; counter < 150; counter++)
        {
            if (token.IsCancellationRequested)
                return;

            UIGlobals.PlaySoundEffect(42);
            await Task.Delay(2000, token);
            Console.Beep();
        }
    }
}