using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NoCaps.Properties;

namespace NoCaps
{
    public class ProfanityFilter
    {
	    private static readonly string[] BadWords;
	    static ProfanityFilter()
	    {
		    BadWords = Resources.profanity.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
	    }

		public static string ReplaceBadWords(string data, out int badWordCount)
		{
			int count = 0;
			Regex r;
			string op = data;
			foreach (var word in BadWords)
			{
				var expword = ExpandBadWordToIncludeIntentionalMisspellings(word);
				r = new Regex(@"(?<Pre>\s+)(?<Word>" + expword + @")(?<Post>\s+|\!\?|\.)");
				var matches = r.Matches(data);
				foreach (Match match in matches)
				{
					string pre = match.Groups["Pre"].Value;
					string post = match.Groups["Post"].Value;
					string output = pre + new string('*', word.Length) + post;
					op = op.Replace(match.Value, output);
					count++;
				}
			}
			badWordCount = count;
			return op;
		}

		public static string ExpandBadWordToIncludeIntentionalMisspellings(string word)
		{
			var chars = word
				.ToCharArray();

			var op = "[" + string.Join("][", chars) + "]";

			return op
				.Replace("[a]", "[a A @]")
				.Replace("[b]", "[b B I3 l3 i3]")
				.Replace("[c]", "(?:[c C \\(]|[k K])")
				.Replace("[d]", "[d D]")
				.Replace("[e]", "[e E 3]")
				.Replace("[f]", "(?:[f F]|[ph pH Ph PH])")
				.Replace("[g]", "[g G 6]")
				.Replace("[h]", "[h H]")
				.Replace("[i]", "[i I l ! 1]")
				.Replace("[j]", "[j J]")
				.Replace("[k]", "(?:[c C \\(]|[k K])")
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
	}
}
