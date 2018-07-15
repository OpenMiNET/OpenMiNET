using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MiNET;
using MiNET.Utils;
using OpenAPI.Player;

namespace NoCaps
{
    public class ChatCompanion : IOpenPlayerAttribute
	{
		public CooldownTimer SpamCooldown = new CooldownTimer(TimeSpan.FromSeconds(3));
		public int SpamAttempts = 0;
		public int CapsReports = 0;
		public int UrlFilterTriggers = 0;
		public int ProfanityUsed = 0;
		public bool Muted = false;

		private OpenPlayer Player;
		public ChatCompanion(OpenPlayer player)
		{
			Player = player;
		}

		public bool CanChat()
		{
			if (!SpamCooldown.CanExecute())
			{
				SpamAttempts++;
				return false;
			}

			return true;
		}

		public void SentChatMesssage()
		{
			SpamCooldown.Execute();
		}

		public bool Filter(string filtered, out string s)
		{
			s = filtered;
			if (!FilterCaps(filtered, out filtered))
			{
				return false;
			}

			if (!FilterUrls(filtered, out filtered))
			{
				return false;
			}

			filtered = ProfanityFilter.ReplaceBadWords(filtered, out int badWords);
			ProfanityUsed += badWords;

			s = filtered;

			return true;
		}

		private static readonly Regex UrlRegex = new Regex(@"((https?|ftp|file)\://|www.)[A-Za-z0-9\.\-]+(/[A-Za-z0-9\?\&\=;\+!'\(\)\*\-\._~%]*)*", RegexOptions.Singleline);

		private bool FilterUrls(string input, out string message)
		{
			int m = 0;
			message = UrlRegex.Replace(input, match =>
			{
				m++;
				return new string('*', match.Length);
			});

			UrlFilterTriggers += m;

			return true;
		}

		private bool FilterCaps(string input, out string message)
		{
			message = input;
			string[] wordArr = input.Split(' ');
			int wordArrLength = wordArr.Length;

			//dont filter if caps count is acceptable
			if (!TooManyCaps(input))
				return true;

			for (int i = 0; i < wordArrLength; ++i)
			{
				if (IsPlayerName(Player.Level.Players.Values.ToArray(), wordArr[i]))
					continue;

				if (IsSmilie(wordArr[i]))
					continue;

				if (TooManyCaps(wordArr[i]))
				{
					wordArr[i] = wordArr[i].ToLower();
					CapsReports++;
				}
			}

			message = string.Join(" ", wordArr);
			return true;
		}

		protected bool IsPlayerName(Player[] playersOnline, string word)
		{
			foreach (Player p in playersOnline)
			{
				//noinspection deprecation
				if (p.Username.Equals(word, StringComparison.InvariantCultureIgnoreCase))
					return true;
			}

			return false;
		}

		private bool IsSmilie(string txt)
		{
			return Regex.IsMatch("(?i)X-?D{1,3}|D-?X{1,3}|:-?D{1,3}|D{1,3}-?:|D{1,3}-?:|:-?P{1,3}|:-?O{1,3}|O{1,3}-?:", txt);
		}

		private bool TooManyCaps(string word)
		{
			return (CountCaps(word) >= ((float)word.Length / 100) * PluginSettings.MaxCaps);
		}

		private int CountCaps(string txt)
		{
			int counter = 0;

			for (int i = 0; i < txt.Length; ++i)
			{
				if (char.IsUpper(txt[i]))
					counter++;
			}

			return counter;
		}
	}
}
