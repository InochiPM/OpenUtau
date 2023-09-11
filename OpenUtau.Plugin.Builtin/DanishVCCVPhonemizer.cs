using System;
using System.Collections.Generic;
using System.Text;
using OpenUtau.Api;
using System.Linq;


namespace OpenUtau.Plugin.Builtin {
    [Phonemizer("Danish VCCVPhonemizer", "DA VCCV", "Mim & InochiPM", language: "DA")]
    // This is Phonemizer 

    public class DanishVCCVPhonemizer : SyllableBasedPhonemizer {

        private readonly string[] vowels = "i,E,e,},@,x,{,3r,2,U,9,O,o,0,6,Eu,Q,A,I,8,Ow".Split(",");
        private readonly string[] consonants = "ch,p,ph,t,tt,dh,dj,k,g,h,hh,l,m,n,ng,r,rh,th,s,sh,v,w,y,z,zh,f".Split(",");
        private readonly Dictionary<string, string> dictionaryReplacements = (
            "i=i;").Split(';')
                .Select(entry => entry.Split('='))
                .Where(parts => parts.Length == 2)
                .Where(parts => parts[0] != parts[1])
                .ToDictionary(parts => parts[0], parts => parts[1]);


        protected override string[] GetVowels() => vowels;
        protected override string[] GetConsonants() => consonants;
        protected override string GetDictionaryName() => "dict_da.txt";
        protected override Dictionary<string, string> GetDictionaryPhonemesReplacement() => dictionaryReplacements;

        protected override List<string> ProcessSyllable(Syllable syllable) {
            string prevV = syllable.prevV;
            string[] cc = syllable.cc;
            string v = syllable.v;
            var lastC = cc.Length - 1;
            var firstC = 0;
            var lastCPrevWord = syllable.prevWordConsonantsCount;

            string basePhoneme;
            var phonemes = new List<string>();


            // --------------------------- STARTING V ------------------------------- //
            if (syllable.IsStartingV) {
                var sV = $"- {v}";
                if (HasOto(sV, syllable.tone)) {
                    basePhoneme = sV;
                } else
                    basePhoneme = $"{v}";


            } else if (syllable.IsVV) {  // if VV
                if (!CanMakeAliasExtension(syllable)) {
                    var VV = $"{prevV} {v}";
                    if (HasOto(VV, syllable.tone)) {
                        basePhoneme = VV;
                    } else basePhoneme = $"{prevV}{v}";

                    if(HasOto(basePhoneme,syllable.tone))
                        if($"{v}" == "Eu") {
                            basePhoneme = $"{prevV} E";
                        }
                    if($"{v}" == "Ow") {
                            basePhoneme = $"{prevV} O";
                        }

                } else {
                    // the previous alias will be extended
                    basePhoneme = null;
                }
                // --------------------------- STARTING CV ------------------------------- //
            } else if (syllable.IsStartingCVWithOneConsonant) {

                basePhoneme = $"- {cc[0]}{v}";
                if (!HasOto(basePhoneme, syllable.tone)) {
                    basePhoneme = $"{cc[0]}{v}";
                }

                // --------------------------- STARTING CCV ------------------------------- //
            } else if (syllable.IsStartingCVWithMoreThanOneConsonant) {


                var Scc = $"- {cc[0]}{cc[1]}";
                phonemes.Add(Scc);

                if (cc.Length > 2) {
                    for (int i = 1; i < cc.Length - 1; i++) {
                        var currentcc = $"{cc[i]}{cc[i + 1]}";

                        phonemes.Add(currentcc);
                    }
                }

                basePhoneme = $"_{cc.Last()}{v}";
                if (!HasOto(basePhoneme, syllable.tone)) {
                    basePhoneme = $"{cc.Last()}{v}";
                }

            }
                // --------------------------- IS VCV ------------------------------- //
                else if (syllable.IsVCVWithOneConsonant) {

                var vc = $"{prevV} {cc[0]}";
                phonemes.Add(vc);

                basePhoneme = $"{cc[0]}{v}";

            } else {
                // ------------- IS VCV WITH MORE THAN ONE CONSONANT --------------- //
                var vc = $"{prevV} {cc[0]}";
                phonemes.Add(vc);

                basePhoneme = $"_{cc.Last()}{v}";
                if(!HasOto(basePhoneme, syllable.tone)) {
                    basePhoneme = $"{cc.Last()}{v}";
                }

                for (int i = 0; i < cc.Length - 1; i++) {
                    var currentcc = $"{cc[i]}{cc[i + 1]}";
                    if(i == lastCPrevWord - 1 || !HasOto(currentcc,syllable.tone)) {
                        currentcc = $"{cc[i]} {cc[i + 1]}";
                    }

                    if(HasOto(currentcc, syllable.tone)) {
                        phonemes.Add(currentcc);
                    }
                }

            }

            phonemes.Add(basePhoneme);
            return phonemes;
        }
        protected override List<string> ProcessEnding(Ending ending) {
            string[] cc = ending.cc;
            string v = ending.prevV;

            var phonemes = new List<string>();

            // --------------------------- ENDING V ------------------------------- //
            if (ending.IsEndingV) {
                var vE = $"{v} -";
                if(!HasOto(vE,ending.tone)) {
                    vE= $"{v}d -";
                }

                    phonemes.Add(vE);


            } else {
                // --------------------------- ENDING VC ------------------------------- //
                if (ending.IsEndingVCWithOneConsonant) {

                    // try 'VC -' else 'V C' + 'C -'
                    var vc = $"{v}{cc[0]} -";
                    if (HasOto(vc, ending.tone)) {
                        phonemes.Add(vc);
                    } else {
                        vc = $"{v} {cc[0]}";
                        phonemes.Add(vc);

                        var cE = $"{cc[0]} -";
                        phonemes.Add(cE);
                    }



                } else {

                    // --------------------------- ENDING VCC ------------------------------- //
                    var vc = $"{v} {cc[0]}";
                    phonemes.Add(vc);

                    for (int i = 0; i < cc.Length - 1; i++) {
                        var cci = $"{cc[i]} {cc[i + 1]}";

                        if (!HasOto(cci, ending.tone))
                        {
                            cci = $"{cc[i]}{cc[i + 1]}-";
                        }
                        if (!HasOto(cci, ending.tone)) {
                            cci = $"{cc[i]}{cc[i + 1]}_";

                        }
                        if (!HasOto(cci, ending.tone)) {
                            cci = $"{cc[i]}{cc[i + 1]}";
                        }

                        TryAddPhoneme(phonemes, ending.tone, cci);
                    }

                    var cE = $"{cc.Last()} -";
                    TryAddPhoneme(phonemes, ending.tone, cE);
                }


            }

            // ---------------------------------------------------------------------------------- //

            return phonemes;
        }


        protected override double GetTransitionBasicLengthMs(string alias = "") {
            return base.GetTransitionBasicLengthMs() * 1.25;
        }



    }
}
