using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EcosDeNebula
{
    // Presentation-only catalogue: gameplay values and saved source text remain unchanged.
    // Exact phrases take priority. A single pass also handles legacy labels assembled
    // from translated phrases and numbers; translated output is never translated again.
    public static class NebulaLocalization
    {
        static readonly Dictionary<string,string> english = new Dictionary<string,string>(StringComparer.Ordinal);
        static readonly Dictionary<string,string> cache = new Dictionary<string,string>(StringComparer.Ordinal);
        static Regex phrases;
        static string language = "es";
        public static string Language { get { return language; } }
        public static int EntryCount { get { return english.Count; } }

        public static void SetLanguage(string value)
        {
            language = value == "en" ? "en" : "es";
            cache.Clear();
        }

        public static void LoadEnglish(string catalogue)
        {
            english.Clear(); cache.Clear();
            foreach (string row in catalogue.Replace("\r", "").Split('\n'))
            {
                if (row.Length == 0 || row.StartsWith("#", StringComparison.Ordinal)) continue;
                int split = row.IndexOf('|');
                if (split <= 0 || split == row.Length-1) throw new FormatException("Invalid localisation row");
                string source = row.Substring(0,split).Replace("\\n", "\n").Replace("\\s", " ");
                string target = row.Substring(split+1).Replace("\\n", "\n").Replace("\\s", " ");
                if (english.ContainsKey(source)) throw new FormatException("Duplicate localisation key: " + source);
                english.Add(source,target);
            }
            // HUD headings use uppercase versions of names, rules and subtitles.
            var originals = new List<KeyValuePair<string,string>>(english);
            foreach (var entry in originals)
                if (!english.ContainsKey(entry.Key.ToUpperInvariant()))
                    english.Add(entry.Key.ToUpperInvariant(),entry.Value.ToUpperInvariant());
            var keys = new List<string>(english.Keys);
            keys.Sort(delegate(string a,string b) { int length=b.Length.CompareTo(a.Length); return length!=0?length:string.CompareOrdinal(a,b); });
            var patterns = new List<string>();
            foreach (string key in keys)
                patterns.Add((char.IsLetterOrDigit(key[0])?@"(?<![\p{L}\p{N}_])":"") + Regex.Escape(key) +
                    (char.IsLetterOrDigit(key[key.Length-1])?@"(?![\p{L}\p{N}_])":""));
            phrases = new Regex(string.Join("|",patterns.ToArray()),RegexOptions.CultureInvariant);
        }

        public static string Text(string source)
        {
            if (language != "en" || string.IsNullOrEmpty(source)) return source;
            string result;
            if (english.TryGetValue(source,out result) || cache.TryGetValue(source,out result)) return result;
            result = phrases == null ? source : phrases.Replace(source,delegate(Match match) { return english[match.Value]; });
            if (cache.Count >= 256) cache.Clear();
            cache[source] = result;
            return result;
        }
    }
}
