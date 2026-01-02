using System.Reflection;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace MediaPlayer
{
    class MediaPlayer
    {
        static GlobalSystemMediaTransportControlsSessionManager sessionManager;
        static GlobalSystemMediaTransportControlsSession currentSession;
        
        static async Task Main()
        {
            sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    currentSession = sessionManager.GetCurrentSession();
                    if (currentSession != null)
                    {
                        var mediaProperties = await currentSession.TryGetMediaPropertiesAsync();
                        var status = currentSession.GetPlaybackInfo().PlaybackStatus;

                        string title = mediaProperties?.Title ?? "Unknown";
                        string artist = mediaProperties?.Artist ?? "Unknown";

                        string output = $"{title}|{artist}|{status}";
                        Console.WriteLine(output);
                        
                        byte[]? imageBytes = await GetThumbnailAsync(currentSession);

                        if (imageBytes != null)
                        {
                            File.WriteAllBytes("cover.png", imageBytes);
                            Console.WriteLine("COVER_READY");
                            Console.WriteLine("COVER_PATH|"+ title + "|" +Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+"\\cover.png");
                        }
                        else
                        {
                            Console.WriteLine("NO_COVER");
                        }
                    }
                    else
                    {
                        Console.WriteLine("NoMedia|NoMedia|Stopped");
                    }

                    await Task.Delay(1000);
                }
            });

            while (true)
            {
                string command = Console.ReadLine();
                if (currentSession == null)
                    continue;

                switch (command?.ToUpper())
                {
                    case "PLAY":
                        await currentSession.TryPlayAsync();
                        break;
                    case "PAUSE":
                        await currentSession.TryPauseAsync();
                        break;
                    case "NEXT":
                        await currentSession.TrySkipNextAsync();
                        break;
                    case "PREVIOUS":
                        await currentSession.TrySkipPreviousAsync();
                        break;
                }
            }
        }
        
        static async Task<byte[]?> GetThumbnailAsync(GlobalSystemMediaTransportControlsSession session)
        {
            var mediaProperties = await session.TryGetMediaPropertiesAsync();
            var thumbnail = mediaProperties.Thumbnail;

            if (thumbnail == null)
                return null;

            using IRandomAccessStream stream = await thumbnail.OpenReadAsync();
            using var reader = new DataReader(stream.GetInputStreamAt(0));

            uint size = (uint)stream.Size;
            await reader.LoadAsync(size);

            byte[] bytes = new byte[size];
            reader.ReadBytes(bytes);

            return bytes;
        }
    }
}