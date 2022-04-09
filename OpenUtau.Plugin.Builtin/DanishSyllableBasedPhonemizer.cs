using System;
using System.Collections.Generic;
using System.Text;
using OpenUtau.Api;
using System.Linq;
using Serilog;

namespace OpenUtau.Plugin.Builtin {
  [Phonemizer("Danish Syllable-Based Phonemizer", "Da SYL", "Inochi-PM")]
    public class DanishSyllableBasedPhonemizer : SyllableBasedPhonemizer {

        /// <summary>
        /// Danish syllable-based phonemizer.
        /// Created based on my own VCCV Danish Reclist.
        ///</summary>

        private readonly string[] vowels = "@,{,A,O,e,I,i,x+,E,9,u,0,2,2r,o,},3r,M,N,L,NG".Split(',');
        private readonly string[] consonants = "p,ph,tt,t,kh,k,f,s,v,dh,y,h,m,n,l,rh,sh,ch,j".Split(',');
        private readonly Dictionary<string, string> dictionaryReplacements = ("A:={;{:=@;a:=A;6=3r;Q=9;E:=e;@-=x+;3=3r;i:=E;I:=i;i^=I;9:=2r;U=o;}:=O+;V=u;Y=};" +
                "p=ph;b=p;t=tt;d=t;k=kh;g=k;D=dh;j=y;m=M;n=N;N=NG;R=rh;s j=sh;t j=ch;d j=j").Split(';')
                .Select(entry => entry.Split('='))
                .Where(parts => parts.Length == 2)
                .Where(parts => parts[0] != parts[1])
                .ToDictionary(parts => parts[0], parts => parts[1]);

        private readonly string[] shortConsonants = "r".Split(',');
        private readonly string[] longConsonants = "sh,ch,j".Split(',');

        protected override string[] GetVowels() => vowels;
        protected override string[] GetConsonants() => consonants;
        protected override string GetDictionaryName() => "cmudict_da.txt";
        protected override Dictionary<string, string> GetDictionaryPhonemesReplacement() => dictionaryReplacements;

        protected override List<string> ProcessSyllable(Syllable syllable)
        {
            string prevV = syllable.prevV;
            string[] cc = syllable.cc;
            string v = syllable.v;

            string basePhoneme;
            var phonemes = new List<string>();
            var lastC = cc.Length - 1;
            var firstC = 0;
            if (syllable.IsStartingV)
            {
                basePhoneme = $"- {v}";
            }
            else if (syllable.IsVV)
            {
                basePhoneme = $"{prevV} {v}";
                if (!HasOto(basePhoneme, syllable.vowelTone))
                {
                    basePhoneme = $"{v}";
                }
            }
            else if (syllable.IsStartingCVWithOneConsonant)
            {
                // TODO: move to config -CV or -C CV
                var rc = $"- {cc[0]}{v}";
                if (HasOto(rc, syllable.vowelTone))
                {
                    basePhoneme = rc;
                }
                else
                {
                    basePhoneme = $"{cc[0]}{v}";
                }
            }
            else if (syllable.IsStartingCVWithMoreThanOneConsonant)
            {
                // try RCCV
                var rvvc = $"- {string.Join("", cc)}{v}";
                if (HasOto(rvvc, syllable.vowelTone))
                {
                    basePhoneme = rvvc;
                }
                else
                {
                    basePhoneme = $"{cc.Last()}{v}";
                    // try RCC
                    for (var i = cc.Length; i > 1; i--)
                    {
                        if (TryAddPhoneme(phonemes, syllable.tone, $"- {string.Join("", cc.Take(i))}"))
                        {
                            firstC = i;
                            break;
                        }
                    }
                    if (phonemes.Count == 0)
                    {
                        TryAddPhoneme(phonemes, syllable.tone, $"- {cc[0]}");
                    }
                    // try CCV
                    for (var i = firstC; i < cc.Length - 1; i++)
                    {
                        var ccv = string.Join("", cc.Skip(i)) + v;
                        if (HasOto(ccv, syllable.tone))
                        {
                            basePhoneme = ccv;
                            lastC = i;
                            break;
                        }
                    }
                }
            }
            else
            { // VCV
                var vcv = $"{prevV} {cc[0]}{v}";
                if (HasOto(vcv, syllable.vowelTone) && (syllable.IsVCVWithOneConsonant))
                {
                    basePhoneme = vcv;
                }
                else
                {
                    // try vcc
                    for (var i = lastC + 1; i >= 0; i--)
                    {
                        var vcc = $"{prevV} {string.Join("", cc.Take(i))}";
                        if (HasOto(vcc, syllable.tone))
                        {
                            phonemes.Add(vcc);
                            firstC = i - 1;
                            break;
                        }
                    }
                    basePhoneme = cc.Last() + v;
                    // try CCV
                    if (cc.Length - firstC > 1)
                    {
                        for (var i = firstC; i < cc.Length; i++)
                        {
                            var ccv = $"{string.Join("", cc.Skip(i))}{v}";
                            if (HasOto(ccv, syllable.vowelTone))
                            {
                                lastC = i;
                                basePhoneme = ccv;
                                break;
                            }
                        }
                    }
                }
            }
            for (var i = firstC; i < lastC; i++)
            {
                // we could use some CCV, so lastC is used
                // we could use -CC so firstC is used
                var cc1 = $"{cc[i]} {cc[i + 1]}";
                if (!HasOto(cc1, syllable.tone))
                {
                    cc1 = $"{cc[i]}{cc[i + 1]}";
                }
                if (i + 1 < lastC)
                {
                    var cc2 = $"{cc[i + 1]} {cc[i + 2]}";
                    if (!HasOto(cc2, syllable.tone))
                    {
                        cc2 = $"{cc[i + 1]}{cc[i + 2]}";
                    }
                    if (HasOto(cc1, syllable.tone) && HasOto(cc2, syllable.tone))
                    {
                        // like [V C1] [C1 C2] [C2 C3] [C3 ..]
                        phonemes.Add(cc1);
                    }
                    else if (TryAddPhoneme(phonemes, syllable.tone, $"{cc[i]} {cc[i + 1]}-"))
                    {
                        // like [V C1] [C1 C2-] [C3 ..]
                        i++;
                    }
                    else if (TryAddPhoneme(phonemes, syllable.tone, cc1))
                    {
                        // like [V C1] [C1 C2] [C2 ..]
                    }
                    else
                    {
                        // like [V C1] [C1] [C2 ..]
                        TryAddPhoneme(phonemes, syllable.tone, cc[i], $"{cc[i]} -");
                    }
                }
                else if (!syllable.IsStartingCVWithMoreThanOneConsonant)
                {
                    // like [V C1] [C1 C2]  [C2 ..] or like [V C1] [C1 -] [C3 ..]
                    TryAddPhoneme(phonemes, syllable.tone, cc1, cc[i], $"{cc[i]} -");
                }
            }

            phonemes.Add(basePhoneme);
            return phonemes;
        }

        protected override List<string> ProcessEnding(Ending ending)
        {
            string[] cc = ending.cc;
            string v = ending.prevV;

            var phonemes = new List<string>();
            if (ending.IsEndingV)
            {   // ending V
                phonemes.Add($"{v} -");
            } else if (ending.IsEndingVCWithOneConsonant)
            {   // ending VC
                var vcr = $"{v} {cc[0]}-";
                if (HasOto(vcr, ending.tone))
                {   // applies ending VC
                    phonemes.Add(vcr);
                } else
                {   // if no ending VC, then regular VC
                    phonemes.Add($"{v} {cc[0]}");
                }
            } else if (ending.IsEndingVCWithMoreThanOneConsonant)
            {   // ending VCC (very rare, usually only occurs in words ending with "x")
                var vccr = $"{v} {string.Join("", cc)}";
                if (HasOto(vccr, ending.tone))
                {   // applies ending VCC
                    phonemes.Add(vccr);
                } else if (!HasOto(vccr, ending.tone))
                {   // if no ending VCC, then CC transitions
                    phonemes.Add($"{v} {cc[0]}");
                    // all CCs except the first one are /C1C2/, the last one is /C1 C2-/
                    // but if there is no /C1C2/, we try /C1 C2-/, vise versa for the last one
                    for (var i = 0; i < cc.Length - 1; i++)
                    {
                        var cc1 = $"{cc[i]} {cc[i + 1]}";
                        if (i < cc.Length - 2)
                        {
                            var cc2 = $"{cc[i + 1]} {cc[i + 2]}";
                            if (HasOto(cc1, ending.tone) && HasOto(cc2, ending.tone))
                            {
                                // like [C1 C2][C2 ...]
                                phonemes.Add(cc1);
                            }
                            else if (TryAddPhoneme(phonemes, ending.tone, $"{cc[i + 1]} {cc[i + 2]}"))
                            {
                                // like [C1 C2][C2 ...]
                            }
                            else if (TryAddPhoneme(phonemes, ending.tone, $"{cc[i + 1]} {cc[i + 2]}-"))
                            {
                                // like [C1 C2-][C3 ...]
                                i++;
                            }
                            else
                            {
                                // like [C1][C2 ...]
                                TryAddPhoneme(phonemes, ending.tone, cc[i], $"{cc[i]} -");
                            }
                        }
                        else
                        {
                            if (TryAddPhoneme(phonemes, ending.tone, $"{cc[i]} {cc[i + 1]}-"))
                            {
                                // like [C1 C2-]
                                i++;
                            }
                            else if (TryAddPhoneme(phonemes, ending.tone, $"{cc[i]} {cc[i + 1]}"))
                            {
                                // like [C1 C2][C2 -]
                                TryAddPhoneme(phonemes, ending.tone, $"{cc[i + 1]} -", cc[i + 1]);
                                i++;
                            }
                            else if (TryAddPhoneme(phonemes, ending.tone, cc1))
                            {
                                // like [C1 C2][C2 -]
                                TryAddPhoneme(phonemes, ending.tone, $"{cc[i + 1]} -", cc[i + 1]);
                                i++;
                            }
                            else
                            {
                                // like [C1][C2 -]
                                TryAddPhoneme(phonemes, ending.tone, cc[i], $"{cc[i]} -");
                                TryAddPhoneme(phonemes, ending.tone, $"{cc[i + 1]} -", cc[i + 1]);
                                i++;
                            }
                        }
                    }
                }
            }
            return phonemes;
        }

        protected override double GetTransitionBasicLengthMs(string alias = "")
        {
            foreach (var c in shortConsonants)
            {
                if (alias.EndsWith(c))
                {
                    return base.GetTransitionBasicLengthMs() * 0.75;
                }
            }
            foreach (var c in longConsonants)
            {
                if (alias.EndsWith(c))
                {
                    return base.GetTransitionBasicLengthMs() * 1.5;
                }
            }
            return base.GetTransitionBasicLengthMs();
        }

    }
}
