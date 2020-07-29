using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BannedListBot
{
    class Program
    {
        static void Main(string[] args)
        {
            new Bot().MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }

    class Bot
    {
        private DiscordClient discord;
        private List<string> bannedWords;
        public List<string> BannedWords
        {
            get
            {
                if (this.bannedWords == null)
                {
                    this.bannedWords = this.IngestListFromFile("banned.txt");
                }
                return this.bannedWords;
            }
        }

        public async Task MainAsync(string[] args)
        {
            string token = File.ReadAllText("callisto_token.key");

            this.discord = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Info
            });

            this.discord.MessageCreated += async eventArgs =>
            {
                if (this.ContainsBanned(eventArgs.Message.Content))
                {
                    //await eventArgs.Message.CreateReactionAsync(DiscordEmoji.FromName(this.discord, ":pika:"));
                    await eventArgs.Channel.SendMessageAsync($"Bad {eventArgs.Author.Username}#{eventArgs.Author.Discriminator}!");
                }
            };

            this.discord.MessageCreated += async eventArgs =>
            {
                if (eventArgs.Author.Id == this.discord.CurrentApplication.Owner.Id && eventArgs.Message.Content.ToLower() == "arcas")
                {
                    await eventArgs.Channel.SendMessageAsync($"Goodbye");
                    Environment.Exit(0);
                }
            };

            await this.discord.ConnectAsync();
            Console.WriteLine("ready");
            DiscordDmChannel dm = await this.discord.CreateDmAsync(this.discord.CurrentApplication.Owner);
            await dm.SendMessageAsync(content: $"{this.discord.CurrentUser.Username} ready");
            await Task.Delay(-1);
        }

        private List<string> IngestListFromFile(string fileName)
        {
            return File.ReadAllText(fileName).Split("\n").Select(s => s.Trim()).Where(s => s != "").ToList();
        }

        private bool ContainsBanned(string msgContent)
        {
            return this.BannedWords.AsParallel().Any(bw => msgContent.Contains(bw));
        }
    }
}
