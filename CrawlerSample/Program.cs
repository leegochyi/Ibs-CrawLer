using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using CsQuery;

namespace CrawlerSample
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpWebRequest req;

            HttpWebResponse rsp;

            StreamReader rdr;

            string txt;

            CQ csq;

            IDomObject don;

            /* 取得清單頁內容 */

            String LinkUrl = "http://www.ibsjeans.com.tw/product.php?pid_for_show={0}";

            //String LinkUrl = "http://www.ibsjeans.com.tw/category.php?type=1&arem1=549&arem=111&category_page={0}";

            Int32 index = 4328;

            while (true) { 

                req = HttpWebRequest.Create(String.Format(LinkUrl,index+1)) as HttpWebRequest;

                req.Method = "GET";

                req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1985.125 Safari/537.36 OPR/23.0.1522.60";

                rsp = req.GetResponse() as HttpWebResponse;

                rdr = new StreamReader(rsp.GetResponseStream());

                txt = rdr.ReadToEnd();

                rdr.Close();

                rdr.Dispose();

                File.WriteAllText(@"Step-1-P"+index+".txt", txt, Encoding.UTF8);

            

            

            /* 讀取所有連結 */
            File.WriteAllText(@"Step-2-P"+index+".txt", string.Empty, Encoding.UTF8);

            List<string> links = new List<string>();

            csq = CQ.Create(txt, Encoding.UTF8);

            foreach (IDomObject _node in csq.Select("a.product_item_name"))
            {
                string name = _node.InnerText;

                name = Regex.Replace(name, @"&#(\d+);", new MatchEvaluator(
                    delegate (Match match)
                    {
                        byte[] barr = BitConverter.GetBytes(Int32.Parse(match.Groups[1].Value));

                        string data = Encoding.Unicode.GetString(new byte[] { barr[0], barr[1] });

                        return data;
                    }
                ));

                name = name.Replace(" ", string.Empty).Replace("\n\r", string.Empty).Replace("\n", string.Empty);

                string link = _node.Attributes["href"];

                links.Add(link);

                Console.WriteLine(name);

                Console.WriteLine(link);

                File.AppendAllText(@"Step-2-P" + index + ".txt", name + Environment.NewLine + link + Environment.NewLine + Environment.NewLine, Encoding.UTF8);
            }

            /* 載入產品網頁 */
            File.WriteAllText(@"Step-3-P" + index + ".txt", string.Empty, Encoding.UTF8);

                foreach (string link in links)
                {
                    req = HttpWebRequest.Create(link) as HttpWebRequest;

                    req.Method = "GET";

                    req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1985.125 Safari/537.36 OPR/23.0.1522.60";

                    rsp = req.GetResponse() as HttpWebResponse;

                    rdr = new StreamReader(rsp.GetResponseStream());

                    txt = rdr.ReadToEnd();

                    rdr.Close();

                    rdr.Dispose();

                    File.WriteAllText(string.Format("{0}.htm", link.Replace("http://www.ibsjeans.com.tw/product.php?pid_for_show=", string.Empty).Replace("&category_sn=549", string.Empty)), txt, Encoding.UTF8);

                    string pimg = string.Empty;


                    List<string> items = new List<string>();

                    csq = CQ.Create(txt, Encoding.UTF8);

                    foreach (IDomObject _node in csq.Select("div.ajax_box_product_div_context_center > p > img"))
                    {
                        pimg = _node.Attributes["src"];

                        pimg = "<p style =" + "text-align: center;" + "><" + "img src=" +'\u0022'+pimg + '\u0022' + " " + "/></p>";

                        List<string> list = new List<string>(pimg.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries));

                        foreach (string item in list)
                        {
                            string data = item.Replace("&nbsp;", " ").Replace("\n\r", string.Empty).Replace("\n", string.Empty);

                            items.Add(data);
                        }

                    }
                    foreach (IDomObject _node in csq.Select("td.product_context > p"))
                    {
                        string desc = _node.InnerHTML;

                        desc = Regex.Replace(desc, @"&#(\d+);", new MatchEvaluator(
                            delegate (Match match)
                            {
                                byte[] barr = BitConverter.GetBytes(Int32.Parse(match.Groups[1].Value));

                                string data = Encoding.Unicode.GetString(new byte[] { barr[0], barr[1] });

                                return data;
                            }
                        ));

                        List<string> list = new List<string>(desc.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries));

                        foreach (string item in list)
                        {
                            string data = item.Replace("&nbsp;", " ").Replace("\n\r", string.Empty).Replace("\n", string.Empty);

                            items.Add(data);
                        }

                    }



                    File.AppendAllText(@"Step-3-P" + index + ".txt", pimg + Environment.NewLine, Encoding.UTF8);


                    foreach (string item in items)
                    {
                        File.AppendAllText(@"Step-3-P" + index + ".txt", string.Format("{0}" + Environment.NewLine, item), Encoding.UTF8);
                    }

                    File.AppendAllText(@"Step-3-P" + index + ".txt", Environment.NewLine, Encoding.UTF8);

                }
                index++;
           
            }
        }
    }
}
