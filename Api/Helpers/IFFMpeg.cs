using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Xabe.FFmpeg;

namespace MovieProfanityDetector.Helpers
{
    public interface IFFmpeg
    {
        public Task ExtractAudio(string path, string fileName);
        public Task MuteVideo(Guid guid);
        public Task ConcatVideo(Guid guid, string fileName);
    }

    public class FFmpegHelper : IFFmpeg
    {
        public async Task ExtractAudio(string path, string fileName)
        {
            try
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var filePath = Path.Combine(path, fileName);
                var videoPath = Path.Combine(rootPath, filePath);
                var outputPath = Path.Combine(rootPath, path, Path.GetFileNameWithoutExtension(fileName) + ".wav");

                var conversion = await FFmpeg.Conversions.FromSnippet.ExtractAudio(videoPath, outputPath);
                await conversion.SetAudioBitrate(128000).SetOutputFormat(Format.wav).Start();
            }
            catch (Exception)
            {
                throw new BadHttpRequestException("Error Extracting Audio");
            }
        }

        public async Task MuteVideo(Guid guid)
        {
            var audioPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                Path.Combine("Movies", guid.ToString(), guid + ".wav"));

            var transPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                Path.Combine("Transcriptions", guid + ".json"));

            var trans = await System.IO.File.ReadAllTextAsync(transPath);
            var transList = JsonConvert.DeserializeObject<TranscriptionJobResult>(trans);

            var mutedPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MutedAudio");

            if (!Directory.Exists(mutedPath))
            {
                Directory.CreateDirectory(mutedPath);
            }

            var outputPath = Path.Combine(mutedPath, guid + ".wav");

            var badWordsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BadWords.txt");
            var badWords = await File.ReadAllLinesAsync(badWordsPath);

            var ffmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "Ffmpeg", "ffmpeg.exe");

            var text = $"\"{ffmpegPath}\" -i \"{audioPath}\" -af \"";

            foreach (var item in transList.results.items)
            {
                if (badWords.Contains(item.alternatives.FirstOrDefault()?.content))
                {
                    text += $"volume=enable='between(t,{item.start_time},{item.end_time})':volume=0 ,";
                }
            }

            text = text.TrimEnd(',');
            text += $"\" \"{outputPath}\"";


            var cmd = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };
            cmd.Start();

            await cmd.StandardInput.WriteLineAsync(text);
            await cmd.StandardInput.FlushAsync();
            cmd.StandardInput.Close();
            await cmd.WaitForExitAsync();
            //Thread.Sleep(1000);

        }

        public async Task ConcatVideo(Guid guid,string fileName)
        {
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
         
            var videoPath = Path.Combine(rootPath, "Movies",Path.Combine(guid.ToString(), fileName));

            var audioPath = Path.Combine(rootPath, "MutedAudio", guid + ".wav");

            var outputPath = Path.Combine(rootPath, "MutedVideo", fileName);

           //var conversions =  await FFmpeg.Conversions.FromSnippet.ExtractVideo(videoPath, outputPath);
           //await conversions.Start();

           if (!File.Exists(outputPath))
           {
               var conversions =  await FFmpeg.Conversions.FromSnippet.AddAudio(videoPath, audioPath, outputPath);
               await conversions.Start();
           }
          
        }
    }
}