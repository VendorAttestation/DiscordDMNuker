using Discord;
using DiscordDMNuker.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace DiscordDMNuker
{
    public partial class Main : Form
    {
        private ToolStripStatusLabel Status;
        private ListBox Logs;
        private readonly Random rnd = new Random();
        private readonly string currentPath = Directory.GetCurrentDirectory();

        // Allowed Extensions That Are Safe.
        private readonly string[] allowedExtensions = new string[]
        {
            ".mp4", ".avi", ".mov", ".mkv", ".flv", ".wmv", ".webm", // Video
            ".jpg", ".jpeg", ".png", ".gif", // Image
            ".mp3", ".wav", ".ogg", ".flac", ".m4a", // Audio
            ".txt", ".json", // Text
        };

        public Main()
        {
            InitializeComponent();
        }
        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        private void Main_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(Path.Combine(currentPath, "SavedMedia")))
            {
                Directory.CreateDirectory(Path.Combine(currentPath, "SavedMedia"));
            }

            if (!Directory.Exists(Path.Combine(currentPath, "SavedConvos")))
            {
                Directory.CreateDirectory(Path.Combine(currentPath, "SavedConvos"));
            }
        }
        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        private async void Start(string Token, ulong MainId, bool savepicsnvids, bool savemessages, bool delete, bool IsGroupChat, bool Edit, string MessageContent, bool IsChannel, ulong ChannelId)
        {
            await Task.Run(async () =>
            {
                try
                {
                    
                    Status.SafeChangeText("Starting");
                    DiscordClient client = new DiscordClient(Token);
                    Logs.SafeAddItem(string.Format("Logged In To: {0}", client.User.Username));
                    if (!IsGroupChat && !IsChannel)
                    {
                        DiscordUser User = await client.GetUserAsync(MainId);
                        PrivateChannel channelid = await client.CreateDMAsync(MainId);
                        Logs.SafeAddItem(string.Format("Created Dms With: {0}", User.Username));
                        System.Collections.Generic.IReadOnlyList<DiscordMessage> msg = await client.GetChannelMessagesAsync(channelid.Id);
                        Status.SafeChangeText("In Progress....");
                        string Convo = "SavedConvos/" + RemoveSpecialCharacters(User.Username) + rnd.Next(1, 999999999) + ".txt";
                        foreach (DiscordMessage message in msg)
                        {
                            if (savemessages)
                            {

                                using (StreamWriter writetext = new StreamWriter(Convo, true))
                                {
                                    writetext.WriteLine(message.Author.User.Username + " || " + message.Content);
                                }
                            }
                            if (savepicsnvids)
                            {
                                if (message.Attachments != null)
                                {
                                    foreach (var Attachment in message.Attachments)
                                    {
                                        if (allowedExtensions.Any(ext => Attachment.FileName.Contains(ext)))
                                        {
                                            var httpClient = new HttpClient();
                                            var fileName = RandomString(4) + Attachment.FileName;
                                            await httpClient.GetAsync(Attachment.Url)
                                                .ContinueWith(responseTask =>
                                                {
                                                    HttpResponseMessage response = responseTask.Result;
                                                    response.EnsureSuccessStatusCode();
                                                    using (var fileStream = File.Create("SavedMedia/" + fileName))
                                                    {
                                                        response.Content.CopyToAsync(fileStream).ContinueWith(copyTask =>
                                                        {
                                                            fileStream.Close();
                                                            if (copyTask.IsCompleted)
                                                            {
                                                                Logs.SafeAddItem(string.Format("Saved Image/Video: {0}", fileName));
                                                            }
                                                        });
                                                    }
                                                });
                                        }
                                    }
                                }
                            }
                            if (delete)
                            {
                                if (message.Author.User.Id == client.User.Id)
                                {
                                    if (message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.ChannelPinnedMessage && message.Type != MessageType.GuildBoostedTier1 && message.Type != MessageType.GuildBoostedTier2 && message.Type != MessageType.GuildBoostedTier3 && message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.GuildBoosted && message.Type != MessageType.ThreadCreated && message.Type != MessageType.ThreadStarterMessage && message.Type != MessageType.RecipientAdd && message.Type != MessageType.RecipientRemove && message.Type != MessageType.Call)
                                    {
                                        await message.DeleteAsync();
                                        Logs.SafeAddItem(string.Format("Deleted Message: {0}", message.Content));
                                        await Task.Delay(new Random().Next(2200, 2500));
                                    }
                                }
                            }
                            if (Edit)
                            {
                                if (message.Author.User.Id == client.User.Id)
                                {
                                    if (message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.ChannelPinnedMessage && message.Type != MessageType.GuildBoostedTier1 && message.Type != MessageType.GuildBoostedTier2 && message.Type != MessageType.GuildBoostedTier3 && message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.GuildBoosted && message.Type != MessageType.ThreadCreated && message.Type != MessageType.ThreadStarterMessage && message.Type != MessageType.RecipientAdd && message.Type != MessageType.RecipientRemove && message.Type != MessageType.Call)
                                    {
                                        await message.EditAsync(new MessageEditProperties { Content = MessageContent });
                                        Logs.SafeAddItem(string.Format("Edited Message: {0}", message.Content));
                                        await Task.Delay(new Random().Next(2200, 2500));
                                    }
                                }
                            }
                        }
                        Status.SafeChangeText("Completed");
                    }
                    else if (IsGroupChat)
                    {
                        DiscordChannel GroupChat = await client.GetChannelAsync(MainId);
                        if (GroupChat.Name != null)
                        {
                            Logs.SafeAddItem(string.Format("Hooked Group With Name: {0}", GroupChat.Name));
                            System.Collections.Generic.IReadOnlyList<DiscordMessage> msg = await client.GetChannelMessagesAsync(GroupChat.Id);
                            Status.SafeChangeText("In Progress....");
                            string Convo = "SavedConvos/" + RemoveSpecialCharacters(GroupChat.Name) + rnd.Next(1, 999999999) + ".txt";
                            foreach (DiscordMessage message in msg)
                            {
                                if (savemessages)
                                {

                                    using (StreamWriter writetext = new StreamWriter(Convo, true))
                                    {
                                        writetext.WriteLine(message.Author.User.Username + " || " + message.Content);
                                    }
                                }
                                if (savepicsnvids)
                                {
                                    if (message.Attachments != null)
                                    {
                                        foreach (var Attachment in message.Attachments)
                                        {
                                            if (allowedExtensions.Any(ext => Attachment.FileName.Contains(ext)))
                                            {
                                                var httpClient = new HttpClient();
                                                var fileName = RandomString(4) + Attachment.FileName;
                                                await httpClient.GetAsync(Attachment.Url)
                                                    .ContinueWith(responseTask =>
                                                    {
                                                        HttpResponseMessage response = responseTask.Result;
                                                        response.EnsureSuccessStatusCode();
                                                        using (var fileStream = File.Create("SavedMedia/" + fileName))
                                                        {
                                                            response.Content.CopyToAsync(fileStream).ContinueWith(copyTask =>
                                                            {
                                                                fileStream.Close();
                                                                if (copyTask.IsCompleted)
                                                                {
                                                                    Logs.SafeAddItem(string.Format("Saved Image/Video: {0}", fileName));
                                                                }
                                                            });
                                                        }
                                                    });
                                            }
                                        }
                                    }
                                }
                                if (delete)
                                {
                                    if (message.Author.User.Id == client.User.Id)
                                    {
                                        if (message.Type != MessageType.Call && message.Type != MessageType.ChannelIconChange && message.Type != MessageType.ChannelNameChange && message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.ChannelPinnedMessage && message.Type != MessageType.RecipientRemove && message.Type != MessageType.RecipientAdd)
                                        {
                                            await message.DeleteAsync();
                                            Logs.SafeAddItem(string.Format("Deleted Message: {0}", message.Content));
                                            await Task.Delay(new Random().Next(2200, 2500));
                                        }
                                    }
                                }
                                if (Edit)
                                {
                                    if (message.Author.User.Id == client.User.Id)
                                    {
                                        if (message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.ChannelPinnedMessage && message.Type != MessageType.GuildBoostedTier1 && message.Type != MessageType.GuildBoostedTier2 && message.Type != MessageType.GuildBoostedTier3 && message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.GuildBoosted && message.Type != MessageType.ThreadCreated && message.Type != MessageType.ThreadStarterMessage && message.Type != MessageType.RecipientAdd && message.Type != MessageType.RecipientRemove && message.Type != MessageType.Call)
                                        {
                                            await message.EditAsync(new MessageEditProperties { Content = MessageContent });
                                            Logs.SafeAddItem(string.Format("Edited Message: {0}", message.Content));
                                            await Task.Delay(new Random().Next(2200, 2500));
                                        }
                                    }
                                }
                            }
                            Status.SafeChangeText("Completed");
                        }
                        else
                        {
                            Logs.SafeAddItem(string.Format("Hooked Group With Name: {0}", GroupChat.Name));
                            System.Collections.Generic.IReadOnlyList<DiscordMessage> msg = await client.GetChannelMessagesAsync(GroupChat.Id);
                            Status.SafeChangeText("In Progress....");
                            string Convo = "SavedConvos/" + "NoNameGC" + rnd.Next(1, 999999999) + ".txt";
                            foreach (DiscordMessage message in msg)
                            {
                                if (savemessages)
                                {

                                    using (StreamWriter writetext = new StreamWriter(Convo, true))
                                    {
                                        writetext.WriteLine(message.Author.User.Username + " || " + message.Content);
                                    }
                                }
                                if (savepicsnvids)
                                {
                                    if (message.Attachments != null)
                                    {
                                        foreach (var Attachment in message.Attachments)
                                        {
                                            if (allowedExtensions.Any(ext => Attachment.FileName.Contains(ext)))
                                            {
                                                var httpClient = new HttpClient();
                                                var fileName = RandomString(4) + Attachment.FileName;
                                                await httpClient.GetAsync(Attachment.Url)
                                                    .ContinueWith(responseTask =>
                                                    {
                                                        HttpResponseMessage response = responseTask.Result;
                                                        response.EnsureSuccessStatusCode();
                                                        using (var fileStream = File.Create("SavedMedia/" + fileName))
                                                        {
                                                            response.Content.CopyToAsync(fileStream).ContinueWith(copyTask =>
                                                            {
                                                                fileStream.Close();
                                                                if (copyTask.IsCompleted)
                                                                {
                                                                    Logs.SafeAddItem(string.Format("Saved Image/Video: {0}", fileName));
                                                                }
                                                            });
                                                        }
                                                    });
                                            }
                                        }
                                    }
                                }
                                if (delete)
                                {
                                    if (message.Author.User.Id == client.User.Id)
                                    {
                                        if (message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.ChannelPinnedMessage && message.Type != MessageType.GuildBoostedTier1 && message.Type != MessageType.GuildBoostedTier2 && message.Type != MessageType.GuildBoostedTier3 && message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.GuildBoosted && message.Type != MessageType.ThreadCreated && message.Type != MessageType.ThreadStarterMessage && message.Type != MessageType.RecipientAdd && message.Type != MessageType.RecipientRemove && message.Type != MessageType.Call)
                                        {
                                            await message.DeleteAsync();
                                            Logs.SafeAddItem(string.Format("Deleted Message: {0}", message.Content));
                                            await Task.Delay(new Random().Next(2200, 2500));
                                        }
                                    }
                                }
                                if (Edit)
                                {
                                    if (message.Author.User.Id == client.User.Id)
                                    {
                                        if (message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.ChannelPinnedMessage && message.Type != MessageType.GuildBoostedTier1 && message.Type != MessageType.GuildBoostedTier2 && message.Type != MessageType.GuildBoostedTier3 && message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.GuildBoosted && message.Type != MessageType.ThreadCreated && message.Type != MessageType.ThreadStarterMessage && message.Type != MessageType.RecipientAdd && message.Type != MessageType.RecipientRemove && message.Type != MessageType.Call)
                                        {
                                            await message.EditAsync(new MessageEditProperties { Content = MessageContent });
                                            Logs.SafeAddItem(string.Format("Edited Message: {0}", message.Content));
                                            await Task.Delay(new Random().Next(2200, 2500));
                                        }
                                    }
                                }
                            }
                            Status.SafeChangeText("Completed");
                        }
                    }
                    else if (IsChannel)
                    {
                        DiscordGuild Guild = await client.GetGuildAsync(MainId);
                        Logs.SafeAddItem(string.Format("Hooked Group With Name: {0}", Guild.Name));
                        DiscordChannel Channel = await client.GetChannelAsync(ChannelId);
                        Logs.SafeAddItem(string.Format("Hooked Channel With Name: {0}", Channel.Name));
                        System.Collections.Generic.IReadOnlyList<DiscordMessage> msg = await client.GetChannelMessagesAsync(ChannelId);
                        Status.SafeChangeText("In Progress....");
                        string Convo = "SavedConvos/" + RemoveSpecialCharacters(Guild.Name) + rnd.Next(1, 999999999) + ".txt";
                        foreach (DiscordMessage message in msg)
                        {
                            if (savemessages)
                            {

                                using (StreamWriter writetext = new StreamWriter(Convo, true))
                                {
                                    writetext.WriteLine(message.Author.User.Username + " || " + message.Content);
                                }
                            }
                            if (savepicsnvids)
                            {
                                if (message.Attachments != null)
                                {
                                    foreach (var Attachment in message.Attachments)
                                    {
                                        if (allowedExtensions.Any(ext => Attachment.FileName.Contains(ext)))
                                        {
                                            var httpClient = new HttpClient();
                                            var fileName = RandomString(4) + Attachment.FileName;
                                            await httpClient.GetAsync(Attachment.Url)
                                                .ContinueWith(responseTask =>
                                                {
                                                    HttpResponseMessage response = responseTask.Result;
                                                    response.EnsureSuccessStatusCode();
                                                    using (var fileStream = File.Create("SavedMedia/" + fileName))
                                                    {
                                                        response.Content.CopyToAsync(fileStream).ContinueWith(copyTask =>
                                                        {
                                                            fileStream.Close();
                                                            if (copyTask.IsCompleted)
                                                            {
                                                                Logs.SafeAddItem(string.Format("Saved Image/Video: {0}", fileName));
                                                            }
                                                        });
                                                    }
                                                });
                                        }
                                    }
                                }
                            }
                            if (delete)
                            {
                                if (message.Author.User.Id == client.User.Id)
                                {
                                    if (message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.ChannelPinnedMessage && message.Type != MessageType.GuildBoostedTier1 && message.Type != MessageType.GuildBoostedTier2 && message.Type != MessageType.GuildBoostedTier3 && message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.GuildBoosted && message.Type != MessageType.ThreadCreated && message.Type != MessageType.ThreadStarterMessage && message.Type != MessageType.RecipientAdd && message.Type != MessageType.RecipientRemove && message.Type != MessageType.Call)
                                    {
                                        await message.DeleteAsync();
                                        Logs.SafeAddItem(string.Format("Deleted Message: {0}", message.Content));
                                        await Task.Delay(new Random().Next(2200, 2500));
                                    }
                                }
                            }
                            if (Edit)
                            {
                                if (message.Author.User.Id == client.User.Id)
                                {
                                    if (message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.ChannelPinnedMessage && message.Type != MessageType.GuildBoostedTier1 && message.Type != MessageType.GuildBoostedTier2 && message.Type != MessageType.GuildBoostedTier3 && message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.GuildBoosted && message.Type != MessageType.ThreadCreated && message.Type != MessageType.ThreadStarterMessage && message.Type != MessageType.RecipientAdd && message.Type != MessageType.RecipientRemove && message.Type != MessageType.Call)
                                    {
                                        await message.EditAsync(new MessageEditProperties { Content = MessageContent });
                                        Logs.SafeAddItem(string.Format("Edited Message: {0}", message.Content));
                                        await Task.Delay(new Random().Next(2200, 2500));
                                    }
                                }
                            }
                        }
                        Status.SafeChangeText("Completed");
                    }
                }
                catch (Exception ex)
                {
                    //debug stuff
                    Logs.SafeAddItem(string.Format("Error Message: {0}", ex.Message));
                    await Task.Delay(new Random().Next(2200, 2500));
                }
            });
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            using (FormStart FormStart = new FormStart())
            {
                FormStart.ShowDialog();
                if (FormStart.Start)
                {
                    Status = toolStripStatusLabel1;
                    Logs = listBox1;

                    Start(FormStart.Token, FormStart.MainId, FormStart.SavePicsNVids, FormStart.SaveMessages, FormStart.Delete, FormStart.IsGroupChat, FormStart.Edit, FormStart.Message, FormStart.IsChannel, FormStart.ChannelId);
                }
            }
        }
    }
}
