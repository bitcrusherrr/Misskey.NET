using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MisskeyDotNet;

namespace MisskeyDotNet.Example
{
    class Program
    {
        static async Task Main()
        {
            MisskeyDotNet.Misskey io;
            if (File.Exists("credential"))
            {
                io = Misskey.Import(await File.ReadAllTextAsync("credential"));
            }
            else
            {
                var miAuth = new MiAuth("INSTANCE.TLD", "Misskey.NET", null, null, Permission.All);
                if (!miAuth.TryOpenBrowser())
                {
                    Console.WriteLine("Open the following URL in your web browser to complete the authentication.");
                    Console.WriteLine(miAuth.Url);
                }
                Console.WriteLine("Press ENTER when authorization is complete.");
                Console.ReadLine();

                io = await miAuth.CheckAsync();
                await File.WriteAllTextAsync("credential", io.Export());
            }

            await FetchReactions(io);
            await SummonError(io);
            await GetMeta(io);
            await FetchInstances();
        }

        private static async ValueTask FetchReactions(Misskey mi)
        {
            var note = await mi.ApiAsync<Note>("notes/show", new
            {
                noteId = "98c9jek61t",
            });

            Console.WriteLine("Note ID: " + note.Id);
            Console.WriteLine("Note Created At: " + note.CreatedAt);
            Console.WriteLine("CW: " + note.Cw ?? "null");
            Console.WriteLine("Body: " + note.Text ?? "null");
            Console.WriteLine("Reactions: ");
            int c = 0;
            foreach (var kv in note.Reactions)
            {
                Console.Write(" {0}: {1}", kv.Key, kv.Value);
                c++;
                if (c == 5)
                {
                    c = 0;
                    Console.WriteLine();
                }
            }
        }
        private static async ValueTask SummonError(Misskey mi)
        {
            try
            {
                var note = await mi.ApiAsync<Note>("notes/show", new
                {
                    noteId = "m",
                });
            }
            catch (MisskeyApiException e)
            {
                Console.WriteLine(e.Error.Message);
            }
        }
        private static async ValueTask GetMeta(Misskey mi)
        {
            try
            {
                var meta = await mi.ApiAsync<Meta>("meta");
                Console.WriteLine($"Instance name: {meta.Name}");
                Console.WriteLine($"Version: {meta.Version}");
                Console.WriteLine($"Description: {meta.Description}");
                Console.WriteLine($"Admin: {meta.MaintainerName}");
                Console.WriteLine($"Admin email: {meta.MaintainerEmail}");
                Console.WriteLine($"LTL: {(meta.DisableLocalTimeline ? "No" : "Yes")}");
                Console.WriteLine($"GTL: {(meta.DisableGlobalTimeline ? "No" : "Yes")}");
                Console.WriteLine($"Registration open: {(meta.DisableRegistration ? "No" : "Yes")}");
                Console.WriteLine($"Email enabled: {(meta.EnableEmail ? "Yes" : "No")}");
                Console.WriteLine($"Twitter Integration: {(meta.EnableTwitterIntegration ? "Yes" : "No")}");
                Console.WriteLine($"Discord Integration: {(meta.EnableDiscordIntegration ? "Yes" : "No")}");
                Console.WriteLine($"GitHub Integration: {(meta.EnableGithubIntegration ? "Yes" : "No")}");
            }
            catch (MisskeyApiException e)
            {
                Console.WriteLine(e.Error.Message);
            }
        }
        private static async ValueTask FetchInstances()
        {
            var res = await Misskey.JoinMisskeyInstancesApiAsync();
            Console.WriteLine($"Last update: {res.Date}");
            Console.WriteLine($"Number of instances: {res.Stats.InstancesCount}");
            Console.WriteLine($"Instance list: ");
            res.InstancesInfos.Select(meta => " " + meta.Url).ToList().ForEach(Console.WriteLine);
        }
    }
}
