using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinuxSecrityCheck
{
    class Model
    {
    }
    public class OsAccountInfo
    {

        public string Ipaddress { get; set; }
        public string Ostype { get; set; }
        public bool LoginFile { get; set; } = false;
        public bool PamFile { get; set; } = false;
        public bool AcMatching { get; set; } = false;
        public List<ACinShadow> Acs { get; set; }

    }

    //划分shadow中每行信息类
    //1、账户名称（密码需要与账户对应的嘛）
　　//2、加密后的密码（总不能学CSDN放明文密码，是吧），如果这一栏的第一个字符为!或者* 的话，说明这是一个不能登录的账户，从上面可以看出，ubuntu默认的就不启用root账户。
　　//3、最近改动密码的日期（不是日期吗，咋是一堆数字，别急，这个是从1970年1月1日算起的总的天数）。那怎么才能知道今天距1970年1月1日有多少天呢？很简单，你改下密码，然后看下这个栏目中的数字是多少就可以了！
　　//4、密码不可被变更的天数：设置了这个值，则表示从变更密码的日期算起，多少天内无法再次修改密码，如果是0的话，则没有限制
　　//5、密码需要重新变更的天数：密码经常更换才能保证安全，为了提醒某些经常不更换密码的用户，可以设置一个天数，强制让用户更换密码，也就是说该用户的密码会在多少天后过期，如果为99999则没有限制
　　//6、密码过期预警天数：如果在5中设置了密码需要重新变更的天数，则会在密码过期的前多少天进行提醒，提示用户其密码将在多少天后过期
　　//7、密码过期的宽恕时间：如果在5中设置的日期过后，用户仍然没有修改密码，则该用户还可以继续使用的天数
　　//8、账号失效日期，过了这个日期账号就不能用了
　　//9、保留的
    public class ACinShadow
    {
        public string Ac_name { get; set; }
        public string Ac_pwd { get; set; } = "";
        public DateTime Ac_pwdLastChange { get; set; } = Common.GetTime("0");
        public int Ac_atleastDays { get; set; } = 0;
        public int Ac_mustChangeDays { get; set; } = 0;
        public int Ac_warningDays { get; set; } = 0;
        public int Ac_excuseDays { get; set; } = 0;
        public DateTime Ac_disableDate { get; set; } = Common.GetTime("0");
        public string Reserved { get; set; } = "";
    }

    public class CheckResult
    {
        public string Result_App { get; set; }
        public string Result_Type { get; set; }
        public string Result_IP { get; set; }
        public string Result_Login { get; set; }
        public string Result_Pam { get; set; }
        public string Result_P_S { get; set; }
        public string Result_AC_Count { get; set; }
        public string Result_ACs { get; set; }
    }
}
