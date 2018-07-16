using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatFilter
{
    public class ProfanityFilter
    {
	    private static readonly log4net.ILog Log =
		    log4net.LogManager.GetLogger(typeof(ProfanityFilter));

		private static readonly string[] BadWords;
	    private static readonly ProfanityEntry[] Regexes;
	    static ProfanityFilter()
	    {
		    string blacklistPath = Path.Combine(NoCapsPlugin.PluginDirectory, "blacklist.txt");
		    string whitelistPath = Path.Combine(NoCapsPlugin.PluginDirectory, "whitelist.txt");

		    if (File.Exists(blacklistPath))
		    {
			    BadWords = File.ReadAllText(blacklistPath).Split(new string[] {"\r\n", "\n"}, StringSplitOptions.None);
			}
		    else
		    {
				Log.Warn($"No profanity blacklist found!");
				BadWords = new string[0];
		    }
		    //BadWords = Encoding.ASCII.GetString(Resources.profanity).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

			Regexes = new ProfanityEntry[BadWords.Length];
		    for (var index = 0; index < BadWords.Length; index++)
		    {
			    var word = BadWords[index];
			    var expword = ExpandBadWordToIncludeIntentionalMisspellings(word);
			    var expression = @"^(?<Pre>(?:.*\s|\s))(?<Word>" + expword + @")(?<Post>.*)$";

			    var r = new Regex(expression);
			    Regexes[index] = new ProfanityEntry()
			    {
				    RegexExpression = r,
				    Length = word.Length
			    };
		    }
	    }

		public static string ReplaceBadWords(string data, out int badWordCount)
		{
			int count = 0;
			string op = data;
			foreach (var r in Regexes)
			{
				var matches = r.RegexExpression.Matches(data);
				foreach (Match match in matches)
				{
					string pre = match.Groups["Pre"].Value;
					string post = match.Groups["Post"].Value;
					string output = pre + new string('*', match.Value.Length) + post;
					op = op.Replace(match.Value, output);
					count++;
				}
			}
			badWordCount = count;
			return op;
		}

		public static string ExpandBadWordToIncludeIntentionalMisspellings(string word)
		{
			var chars = word.Replace(@"\", @"\\")
				.ToCharArray();

			var op = "[" + string.Join("][", chars) + "]";

			return op
				.Replace("[a]", "[a A @]")
				.Replace("[b]", "[b B I3 l3 i3]")
				.Replace("[c]", @"(?:[c C \(]|[k K])")
				.Replace("[d]", "[d D]")
				.Replace("[e]", "[e E 3]")
				.Replace("[f]", "(?:[f F]|[ph pH Ph PH])")
				.Replace("[g]", "[g G 6]")
				.Replace("[h]", "[h H]")
				.Replace("[i]", "[i I l ! 1]")
				.Replace("[j]", "[j J]")
				.Replace("[k]", @"(?:[c C \(]|[k K])")
				.Replace("[l]", "[l L 1 ! i]")
				.Replace("[m]", "[m M]")
				.Replace("[n]", "[n N]")
				.Replace("[o]", "[o O 0]")
				.Replace("[p]", "[p P]")
				.Replace("[q]", "[q Q 9]")
				.Replace("[r]", "[r R]")
				.Replace("[s]", "[s S $ 5]")
				.Replace("[t]", "[t T 7]")
				.Replace("[u]", "[u U v V]")
				.Replace("[v]", "[v V u U]")
				.Replace("[w]", "[w W vv VV]")
				.Replace("[x]", "[x X]")
				.Replace("[y]", "[y Y]")
				.Replace("[z]", "[z Z 2]")
				;
		}

	    private class ProfanityEntry
	    {
		    public Regex RegexExpression;
		    public int Length;
	    }
	}
}
