using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;
using OpenUtau.Core.Ustx;

namespace OpenUtau.Plugin.Builtin 
{
    
    [Phonemizer("VOICEMITH Phonemizer", "ZH CVVC", "Inochi-PM")]
    public class ChineseCVVCPhonemizer : Phonemizer 
    {
       
        static readonly string[] consonants = new string[] {
            "a=a,a+,aer",
            "ai=ai,ai+,aier",
            "an=an,an+,aner,ann",
            "ang=ang,ang+,anger",
            "ao=ao,ao+,aoer",
            "e=e,e+,eer",
            "ei=ei,ei+,eier",
            "en=en,en+,ener",
            "eng=eng,eng+,enger",
            "eh=eh,eh+,eher",
            "er=er,er+,erer",
            "o=o,o+,ong,oer",
            "ou=ou,ou+,ouer",
            "wu=wu,wu+",
            "yi=yi,yi+",
            "yu=yu,yu+",
            "b=b,ba,bang,bao,biao,bai,ban,bo,ben,beng,bei,bie,bu,bi,bin,bing,bian,ba+,bang+,bao+,biao+,bai+,ban+,bo+,ben+,beng+,bei+,bie+,bu+,bi+,bin+,bing+,bian+,ber,ball,bianng,",
            "c=c,ca,cang,cao,cai,can,cong,cou,ce,cen,ceng,ci,cuan,cuo,cun,cui,cu,ca+,cang+,cao+,cai+,can+,cong+,cou+,ce+,cen+,ceng+,ci+,cuan+,cuo+,cun+,cui+,cu+,cier,ceh,cinng,cua",
            "ch=ch,cha,chang,chao,chai,chan,chong,chou,che,chen,cheng,chi,chuang,chuai,chuan,chuo,chun,chui,chu,cha+,chang+,chao+,chai+,chan+,chong+,chou+,che+,chen+,cheng+,chi+,chuang+,chuai+,chuan+,chuo+,chun+,chui+,chu+,chier",
            "d=d,da,dia,dang,dao,diao,dai,dan,duan,duo,dong,dou,diu,de,dun,deng,dei,dui,die,du,di,ding,dian,da+,dia+,dang+,dao+,diao+,dai+,dan+,duan+,duo+,dong+,dou+,diu+,de+,dun+,deng+,dei+,dui+,die+,du+,di+,ding+,dian+,der,den,diam,diang,duainng",
            "f=f,fa,fang,fan,fo,fou,fen,feng,fei,fu,fa+,fang+,fan+,fo+,fou+,fen+,feng+,fei+,fu+,fer,fiao,fo",
            "g=g,ga,gua,gang,guang,gao,gai,guai,gan,guan,guo,gong,gou,ge,gen,gun,geng,gei,gui,gu,ga+,gua+,gang+,guang+,gao+,gai+,guai+,gan+,guan+,guo+,gong+,gou+,ge+,gen+,gun+,geng+,gei+,gui+,gu+,ger,gianng,ging,ginng,guanng",
            "h=h,ha,hang,hao,hai,han,hong,hou,he,hen,heng,hei,hua,huang,huai,huan,huo,hun,hui,hu,ha+,hang+,hao+,hai+,han+,hong+,hou+,he+,hen+,heng+,hei+,hua+,huang+,huai+,huan+,huo+,hun+,hui+,hu+,her,hann,hiung,hn",
            "j=j,jue,ju,jun,juan,jia,jiang,jiao,jiong,jiu,jie,ji,jin,jing,jian,jue+,ju+,jun+,juan+,jia+,jiang+,jiao+,jiong+,jiu+,jie+,ji+,jin+,jing+,jian+,jier,jier+,jio,jiyu,rii",
            "k=k,ka,kua,kang,kuang,kao,kai,kuai,kan,kuan,kuo,kong,kou,ke,ken,kun,keng,kei,kui,ku,ka+,kua+,kang+,kuang+,kao+,kai+,kuai+,kan+,kuan+,kuo+,kong+,kou+,ke+,ken+,kun+,keng+,kei+,kui+,ku+,ker,ker+,ki,kiam,king,kiyu,kuanng",
            "l=l,la,lang,lao,lai,lan,luan,luo,long,lou,le,lun,leng,lei,lue,lve,lu,lv,lyu,lia,liang,liao,liu,lie,li,lin,ling,lian,la+,lang+,lao+,lai+,lan+,luan+,luo+,long+,lou+,le+,lun+,leng+,lei+,lue+,lve+,lu+,lv+,lyu+,lia+,liang+,liao+,liu+,lie+,li+,lin+,ling+,lian+,ler,ler+,lia,linng,lo,lyuan",
            "m=m,ma,mang,mao,mai,man,mo,mou,me,men,meng,mei,mu,miao,miu,mie,mi,min,ming,mian,ma+,mang+,mao+,mai+,man+,mo+,mou+,me+,men+,meng+,mei+,mu+,miao+,miu+,mie+,mi+,min+,ming+,mian+,mer,mall,mn,vanng,ve,veh,vinng,vonng",
            "n=n,na,nang,nao,nai,nan,nuan,nuo,nong,nou,ne,nen,neng,nei,nue,nve,nu,nv,nyu,niang,niao,niu,nie,ni,nin,ning,nian,na+,nang+,nao+,nai+,nan+,nuan+,nuo+,nong+,nou+,ne+,nen+,neng+,nei+,nue+,nve+,nu+,nv+,nyu+,niang+,niao+,niu+,nie+,ni+,nin+,ning+,nian+,ner,new,n,nann,no,nuang,ng",
            "p=p,pa,pang,pao,piao,pai,pan,po,pou,pen,peng,pei,pie,pu,pi,pin,ping,pian,pa+,pang+,pao+,piao+,pai+,pan+,po+,pou+,pen+,peng+,pei+,pie+,pu+,pi+,pin+,ping+,pian+,per,per+,pua,puen,pull",
            "q=q,que,qu,qun,quan,qia,qiang,qiao,qiong,qiu,qie,qi,qin,qing,qian,que+,qu+,qun+,quan+,qia+,qiang+,qiao+,qiong+,qiu+,qie+,qi+,qin+,qing+,qian+,qier",
            "r=r,rang,rao,ran,ruan,ruo,rong,rou,re,ren,run,reng,rui,ru,ri,rang+,rao+,ran+,ruan+,ruo+,rong+,rou+,re+,ren+,run+,reng+,rui+,ru+,ri+,rier",
            "s=s,sa,sang,sao,sai,san,song,sou,se,sen,seng,si,suan,suo,sun,sui,su,sa+,sang+,sao+,sai+,san+,song+,sou+,se+,sen+,seng+,si+,suan+,suo+,sun+,sui+,su+,sier,suainng,suei,swee",
            "sh=sh,sha,shang,shao,shai,shan,shou,she,shen,sheng,shei,shi,shua,shuang,shuai,shuan,shuo,shun,shui,shu,sha+,shang+,shao+,shai+,shan+,shou+,she+,shen+,sheng+,shei+,shi+,shua+,shuang+,shuai+,shuan+,shuo+,shun+,shui+,shu+,shier",
            "t=t,ta,tang,tao,tiao,tai,tan,tuan,tuo,tong,tou,te,tun,teng,tei,tui,tie,tu,ti,ting,tian,ta+,tang+,tao+,tiao+,tai+,tan+,tuan+,tuo+,tong+,tou+,te+,tun+,teng+,tei+,tui+,tie+,tu+,ti+,ting+,tian+,ter,tiam",
            "w=w,wa,wang,wai,wan,wo,wen,weng,wei,wa+,wang+,wai+,wan+,wo+,wen+,weng+,wei+,wuer",
            "x=x,xue,xu,xun,xuan,xia,xiang,xiao,xiong,xiu,xie,xi,xin,xing,xian,xue+,xu+,xun+,xuan+,xia+,xiang+,xiao+,xiong+,xiu+,xie+,xi+,xin+,xing+,xian+,xier,xiyu",
            "y=y,ya,yang,yao,yong,you,yo,ye,yin,ying,yan,ya+,yang+,yao+,yong+,you+,ye+,yin+,ying+,yan+,yier,yianng",
            "yu=yue,yun,yuan,yue+,yun+,yuan+,yuer",
            "zh=zh,zha,zhang,zhao,zhai,zhan,zhong,zhou,zhe,zhen,zheng,zhei,zhi,zhua,zhuang,zhuai,zhuan,zhuo,zhun,zhui,zhu,zha+,zhang+,zhao+,zhai+,zhan+,zhong+,zhou+,zhe+,zhen+,zheng+,zhei+,zhi+,zhua+,zhuang+,zhuai+,zhuan+,zhuo+,zhun+,zhui+,zhu+,zhier",
            "z=z,za,zang,zao,zai,zan,zong,zou,ze,zen,zeng,zei,zi,zuan,zuo,zun,zui,zu,za+,zang+,zao+,zai+,zan+,zong+,zou+,ze+,zen+,zeng+,zei+,zi+,zuan+,zuo+,zun+,zui+,zu+,zier,zuann,zyu",
            "R=a-,ai-,an-,ang-,ao-,e-,eh-,ei-,en-,eng-,er-,o-,ou-,wa-,wai-,wan-,wang-,wei-,wen-,weng-,wo-,wu-,ya-,yai-,yan-,yang-,yao-,ye-,yen-,yin-,yeng-,ying-,yi-,yo-,yong-,you-,yu-,yue-,yuan-,yun-,yuen-,bre,bre1,bre2,bre3,bre4,bre5,bre6,br",


        };
        
        static readonly string[] vowels = new string[] {
            "a=a,a-,ba,pa,ma,fa,da,ta,na,la,ga,ka,ha,zha,cha,sha,za,ca,sa,wa,wa-,gua,kua,hua,zhua,shua,ya,ya-,lia,jia,qia,xia,dia",
            "ai=ai,ai-,bai,pai,mai,dai,tai,nai,lai,gai,kai,hai,zhai,chai,shai,zai,cai,sai,wai,wai-,guai,kuai,huai,zhuai,chuai,shuai,yai,yai-",
            "an=n,an-,ban,pan,man,fan,dan,tan,nan,lan,gan,kan,han,zhan,chan,shan,ran,zan,can,san,wan,wan-,duan,tuan,nuan,luan,guan,kuan,huan,zhuan,chuan,shuan,ruan,zuan,cuan,suan",
            "ang=ang,ang-,bang,pang,mang,fang,dang,tang,nang,lang,gang,kang,hang,zhang,chang,shang,rang,zang,cang,sang,yang,yang-,liang,jiang,qiang,xiang,niang,wang,wang-,guang,kuang,huang,zhuang,chuang,shuang",
            "ao=ao,ao-,bao,pao,mao,dao,tao,nao,lao,gao,kao,hao,zhao,chao,shao,rao,zao,cao,sao,yao,yao-,biao,piao,miao,diao,tiao,niao,liao,jiao,qiao,xiao",
            "e=e,e-,me,de,te,ne,le,ge,ke,he,zhe,che,she,re,ze,ce,se",
            "ye=ye,ye-,bie,pie,mie,die,tie,nie,lie,jie,qie,xie",
            "yue=yue,yue-,nue,nve,lue,lve,jue,que,xue,eh,eh-",
            "er=er,er-",
            "ei=ei,ei-,bei,pei,mei,fei,dei,tei,nei,lei,gei,kei,hei,zhei,shei,zei,wei,wei-,dui,tui,gui,kui,hui,zhui,chui,shui,rui,zui,cui,sui",
            "en=en,en-,ben,pen,men,fen,nen,gen,ken,hen,zhen,chen,shen,ren,zen,cen,sen,wen,wen-,dun,tun,lun,gun,kun,hun,zhun,chun,shun,run,zun,cun,sun,den",
            "eng=eng,eng-,deng,teng,neng,leng,geng,keng,heng,zheng,cheng,sheng,reng,zeng,ceng,seng,peng,beng",
            "yong=yong=ong,dong,tong,nong,long,gong,kong,hong,zhong,chong,rong,zong,cong,song,yong,yong-,jiong,qiong,xiong,meng,feng,weng,weng-",
            "h-i=h-i=zhi,chi,shi,ri",
            "-i=zi,ci,si",
            "wo=wo=o,o-,bo,po,mo,fo,wo,wo-,duo,tuo,nuo,luo,guo,kuo,huo,zhuo,chuo,shuo,ruo,zuo,cuo,suo,yo,yo-",
            "ou=ou,ou-,pou,mou,nou,fou,dou,tou,lou,gou,kou,hou,zhou,chou,shou,rou,zou,cou,sou,you,you-,miu,diu,niu,liu,jiu,qiu,xiu",
            "yuen=yuen=yun,yun-,yuen,yuen-,jun,qun,xun",
            "wu=wu,wu-,bu,pu,mu,fu,du,tu,nu,lu,gu,ku,hu,zhu,chu,shu,ru,zu,cu,su",
            "yi=yi,yi-,bi,pi,mi,di,ti,ni,li,ji,qi,xi",
            "yin=yin,yin-,yen,yen-,bin,pin,min,nin,lin,jin,qin,xin",
            "ying=ying,ying-,yeng,yeng-,bing,ping,ming,ding,ning,ling,jing,qing,ting,xing",
            "yan=yan,yan-,bian,pian,mian,dian,tian,nian,lian,jian,qian,xian",
            "yuan=yuan,yuan-,juan,quan,xuan",
            "yu=yu,yu-,nv,nyu,lv,lyu,ju,qu,xu",
            "null=null=Ng,bre,bre1,bre2,bre3,bre4,bre5,bre6,br",

        };

        static HashSet<string> cSet;
        static Dictionary<string, string> vDict;

        static ChineseCVVPhonemizer() {
            cSet = new HashSet<string>(consonants.Split(','));
            vDict = vowels.Split(',')
                .Select(s => s.Split('='))
                .ToDictionary(a => a[0], a => a[1]);
        }

        private USinger singer;

        // Simply stores the singer in a field.
        public override void SetSinger(USinger singer) => this.singer = singer;

        public override Phoneme[] Process(Note[] notes, Note? prevNeighbour, Note? nextNeighbour) {
            // The overall logic is:
            // 1. Remove consonant: "duang" -> "uang".
            // 2. Lookup the trailing sound in vowel table: "uang" -> "_ang".
            // 3. Split the total duration and returns "duang" and "_ang".
            var note = notes[0];
            string vowel = string.Empty;
            if (note.lyric.Length > 2 && cSet.Contains(note.lyric.Substring(0, 2))) {
                // First try to find consonant "zh", "ch" or "sh", and extract vowel.
                vowel = note.lyric.Substring(2);
            } else if (note.lyric.Length > 1 && cSet.Contains(note.lyric.Substring(0, 1))) {
                // Then try to find single character consonants, and extract vowel.
                vowel = note.lyric.Substring(1);
            } // Otherwise we don't need the vowel.
            string phoneme0 = note.lyric;
            // We will need to split the total duration for phonemes, so we compute it here.
            int totalDuration = notes.Sum(n => n.duration);
            // Lookup the vowel split table. For example, "uang" will match "_ang".
            if (vDict.TryGetValue(vowel, out var phoneme1)) {
                // Now phoneme0="duang" and phoneme1="_ang",
                // try to give "_ang" 120 ticks, but no more than half of the total duration.
                int length1 = 100;
                if (length1 > totalDuration / 2) {
                    length1 = totalDuration / 2;
                }
                return new Phoneme[] {
                    new Phoneme() {
                        phoneme = phoneme0,
                    },
                    new Phoneme() {
                        phoneme = phoneme1,
                        position = totalDuration - length1,
                    }
                };
            }
            // Not spliting is needed. Return as is.
            return new Phoneme[] {
                new Phoneme() {
                    phoneme = phoneme0,
                }
            };
        }
    }
}
