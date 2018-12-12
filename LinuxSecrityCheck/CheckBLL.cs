using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinuxSecrityCheck
{
    class CheckBLL
    {
        public List<OsAccountInfo> AppOsUsersList(string appPath)
        {
            Common.FileGet fileget = new Common.FileGet();
            List<string> ipDirs = fileget.GetIPsDirPath(appPath+"\\主机");
            List<OsAccountInfo> osUsersList = new List<OsAccountInfo>();
            foreach (string ipDir in ipDirs)
            {
                osUsersList.Add(OsUsers(ipDir));
            }
            return osUsersList;
        }
        public OsAccountInfo OsUsers(string ipDir)
        {
            OsAccountInfo osusers = new OsAccountInfo();
            //string Ipaddress;
            //string Ostype;
            //bool LoginFile;
            //bool PamFile;
            //bool AcMatching;
            //List<ACinShadow> Acs;

            
            List<ACinShadow> acList = new List<ACinShadow>();

            //第一个属性Ipaddress
            osusers.Ipaddress = ipDir.Split('\\').Last();
            //第二个属性从excel文件中读取类型与版本号进行判断,excel单元格位置5D(4,3)
            string ostype = Common.ReadExcelCell(ipDir, 4, 3);
            if (ostype.ToLower().Contains("win"))
                osusers.Ostype = "Windows";
            else if (ostype.ToLower().Contains("lin"))
                osusers.Ostype = "Linux";
            else if (ostype.ToLower().Contains("unix"))
                osusers.Ostype = "Unix";
            else if (ostype.ToLower().Contains("hp"))
                osusers.Ostype = "AIX";
            else
                osusers.Ostype = "Unkown";
            //第三\四个属性,判断是否提供"login.defs","system-auth"文件
            if (File.Exists(ipDir + "\\login.defs"))
                osusers.LoginFile = true;
            if (File.Exists(ipDir + "\\system-auth"))
                osusers.PamFile = true;

            //检查第五个属性,需要判断"passwd"和"shadow"两个文件中的行数是否一致

            if (File.Exists(ipDir + "passwd") && File.Exists(ipDir + "shadow"))
            {
                List<string> readPasswd = Common.ReadTxtContent(ipDir + "passwd");
                List<string> readShadow = Common.ReadTxtContent(ipDir + "shadow");
                if (readPasswd.Count == readShadow.Count)
                {
                    osusers.AcMatching = true;

                    //获取第六个属性,截取"shadow"文件中所有有效的用户名
                    List<string> txtSplit = new List<string>();
                    foreach (string txt in readShadow)
                    {
                        ACinShadow acinfo = new ACinShadow();
                        if (txt.Contains(":$"))
                        {
                            acList.Add(GetOneAC(txt));
                        }
                    }
                    osusers.Acs = acList;
                }
            }
            return osusers;
        }

        private ACinShadow GetOneAC(string shadowLine)
        {
            //string Ac_name
            //string Ac_pwd 
            //DateTime Ac_pwdLastChange
            //int Ac_atleastDays 
            //int Ac_mustChangeDays 
            //int Ac_warningDays 
            //int Ac_excuseDays
            //DateTime Ac_disableDate 
            //string Reserved

            ACinShadow ac = new ACinShadow();
            string[] ac_9 = shadowLine.Split(':');
            ac.Ac_name = ac_9[0];
            ac.Ac_pwd = ac_9[1];
            ac.Ac_pwdLastChange = Common.GetTime(ac_9[2]);
            if (ac_9[3] != null)
                ac.Ac_atleastDays = int.Parse(ac_9[3]);
            if (ac_9[4] != null)
                ac.Ac_mustChangeDays = int.Parse(ac_9[4]);
            if (ac_9[5] != null)
                ac.Ac_warningDays = int.Parse(ac_9[5]);
            if (ac_9[6] != null)
                ac.Ac_excuseDays = int.Parse(ac_9[6]);
            if (ac_9[7] != null)
                ac.Ac_disableDate = Common.GetTime(ac_9[7]);
            if (ac_9[8] != null)
                ac.Reserved = ac_9[8];

            return ac;
        }

        //检查login.defs文件：1.max day 2.
        private string LoginDefs(string filePath)
        {
            bool maxday = false;
            bool minLength = false;
            bool warningDays = false;
            List<string> strs = Common.ReadTxtContent(filePath);
            foreach (string s in strs)
            {
                if (s.Contains("MaxDay") && s.Contains("90"))
                    maxday = true;
                if (s.Contains("MinLength") && s.Contains("8"))
                    minLength = true;
                if (s.Contains("warning") && s.Contains("15"))
                    warningDays = true;
            }
            if (maxday & minLength & warningDays)
                return "设置正确";
            else
                return "设置有误";
        }

        private string SystemAuth(string filePath)
        {
            bool ucredit = false;
            bool lcredit = false;
            bool dcredit = false;
            bool ocredit = false;
            bool minlen = false;
            bool difok = false;
            List<string> strs = Common.ReadTxtContent(filePath);
            foreach (string s in strs)
            {
                string s1 = s.Replace(" ", "");
                if (s1.Contains("ucredit=-1"))
                    ucredit = true;
                if (s1.Contains("lcredit=-1"))
                    lcredit = true;
                if (s1.Contains("dcredit=-2"))
                    dcredit = true;
                if (s1.Contains("ocredit=-2"))
                    ocredit = true;
                if (s1.Contains("minlen=8"))
                    minlen = true;
                if (int.Parse(s1.Substring((s1.IndexOf("difok=") + 7), 1)) > 2)
                    difok = true;
            }
            if (ucredit & lcredit & ocredit & dcredit & difok & minlen)
                return "设置正确";
            else
                return "设置有误";
        }

        /// <summary>
        /// 从系统操作系统账号中提取每台主机操作系统的账号
        /// 提供给Result表中的Result_ACs使用
        /// </summary>
        /// <param name="acList"></param>
        /// <param name="ipadd"></param>
        /// <returns></returns>
        private string OneOSAC(List<OsAccountInfo> acList, string ipadd)
        {
            string oneOSac = "";
            foreach(OsAccountInfo oneOS in acList)
            {
                if(oneOS.Ipaddress == ipadd)
                {
                    foreach(ACinShadow ac in oneOS.Acs)
                    {
                        oneOSac += ac.Ac_name+",";
                    }
                }
            }
            return oneOSac;
        }

        /// <summary>
        /// 计算每台主机操作系统账号的数量
        /// </summary>
        /// <param name="acs"></param>
        /// <returns></returns>
        private string OneOSAC_Count(string acs)
        {
            string acs1 = acs.Replace(",", "");
            return (acs.Length - acs1.Length).ToString();
        }
    }
}
