using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace lfmacher
{
    public partial class Form1 : Form
    {
        string[] countrys = new string[] { 
            "at",
            "de", 
            "nl",
            "ch",
            "no"
        };

        string[] countrid = new string[] { 
            "5",
            "2",
            "1",
            "6",
            "169"
        };

        string[] countrsite = new string[] {
            "https://www.lieferando.at/foodtracker?trackingid=",
            "https://www.lieferando.de/foodtracker?trackingid=",
            "https://www.thuisbezorgd.nl/foodtracker?trackingid=",
            "https://www.just-eat.ch/foodtracker?trackingid=",
            "https://www.just-eat.no/foodtracker?trackingid="
        };

        public Form1()
        {
            InitializeComponent();
        }

        public static string GenerateMD5(string input)
        {

            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public string req(int index,string[] arg_params)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://de.citymeal.com/android/android.php");
            var str = "language=";
            str += countrys[index] + "&";
            str += "version=5.7&";
            str += "systemVersion=24&";
            str += "appVersion=4.15.3.2&";

            string hash ="";
            foreach (string param in arg_params)
            {
                hash += param;
            }
            hash += "4ndro1d";
            hash = GenerateMD5(hash);

       
            for (int i = 0; i < arg_params.Length; i += 1)
            {
                str += "var" + (i + 1).ToString() + "=" + arg_params[i] + "&";
            }
            str += "var0=" + hash;

   
            var data = Encoding.ASCII.GetBytes(str);

            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            request.ContentLength = data.Length;
            request.ServicePoint.Expect100Continue = false;

           // request.Proxy = new WebProxy("", 5);

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            var response = (HttpWebResponse)request.GetResponse();
            using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (var x in countrys)
                comboBox1.Items.Add(x);
            comboBox1.SelectedIndex = 1;
        }

        static int g_count = 0;
        public int gen_random(int min, int max)
        {
            g_count++;
            return new Random(Environment.TickCount + g_count).Next(min, max);
        }

        Dictionary<int, int> email_map = new Dictionary<int, int>();

        private void CopySelectedRowsToClipboard(ListView listView, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                var builder = new StringBuilder();
                foreach (ListViewItem item in listView.SelectedItems)
                {
                    var subItems = item.SubItems.Cast<ListViewItem.ListViewSubItem>()
                        .Select(subItem => subItem.Text);
                    builder.AppendLine(string.Join(", ", subItems).Replace(Environment.NewLine, ""));
                }

                if (builder.Length > 0)
                    Clipboard.SetText(builder.ToString());
                else
                    Clipboard.Clear();
            }
        }

        struct buy_item
        {
            public string m_id;
            public string m_ordername;
            public float m_value;
        }

        string order_food(string time, int index, string name, string street,string postal, string city, string phone, 
            string mail, string restaurant_id, string order_format,
            string language,string countrycode,string longt, string latt,string remarks,string agent,string random_id)
        {
            var vax = req(index, new string[] {
                                    "placeorder",
                                    "",
                                    name, //name
                                    "", //companyName 
                                    street, //street
                                    "",//countryCode
                                    postal,//postalCode
                                    city,//city
                                    phone,//phone random
                                    mail,//email random
                                    time,//deliveryTime
                                    "",//paymentPart
                                    "",//remarks
                                    "0",//newsLetter
                                    restaurant_id,//restaurantId
                                    order_format,//formattedOrder
                                    "52",//siteCode unknwon
                                    language,//language
                                    countrycode,//countryCode correct
                                    "",//clientId
                                    "cash",//paymentMethod
                                    "",//bankId
                                    "",//foodTrackerId
                                    postal,//deliveryArea
                                    agent,//
                                    "{\"companyname\" : \"\",\"entrance\" : \"\",\"stock\" : \"\",\"door\" : \"\"}",//extraAddress 
                                    "",//username 
                                    "",//credentials 
                                    "",//addressId 
                                    "DELIVERY",//deliveryMethod
                                    "0",
                                    "",//voucherCode
                                    "",
                                    longt,//longitude 
                                    latt,//longitude
                                    remarks,//productRemarks
                                    "1",//isLocationAccurate
                                    random_id,//"aab1-a1a1_aa12aaaaaaaaa" + gen_random(100000000, 900000000).ToString(),
                                    "",
                                    "",
                                    "",
                                    ""
                                });

            
            return vax;
        }

        public static string random_id(int length)
        {
            g_count++;
            const string chars = "abcdef0123456789";
            var random = new Random(Environment.TickCount + g_count);
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        void bestell_bot_smart(int index)
        {
            email_map[textBox11.Text.GetHashCode()] = 0;


            int cur_salate = 0;
            int timesx = 0;
            int timesx2 = 0;

            var datx = req(index, new string[] { "getrestaurants", textBox3.Text,countrid[index], textBox7.Text, textBox8.Text, countrys[index] });
            var list_of_restaurants = JObject.Parse(datx);

            int skip = Convert.ToInt32(textBox13.Text);

            for (int i = skip; i < list_of_restaurants["rs"]["rt"].Count(); i += 1)
            {
                string is_open = (string)list_of_restaurants["rs"]["rt"][i]["op"];
                if (is_open == "1")
                {
                    string restaurant_id = (string)list_of_restaurants["rs"]["rt"][i]["id"];
                    var restaurant_info_raw = req(index, new string[] { "getrestaurantdata", restaurant_id, textBox3.Text, "1", textBox7.Text, textBox8.Text, "" });
                    var restaurant_info = JObject.Parse(restaurant_info_raw);
                    var min_buy = float.Parse((string)restaurant_info["rd"]["dc"]["ma"]);

                    var match = Regex.Matches(restaurant_info_raw, "id(.*?)tc\"");

                    List<buy_item> order_list = new List<buy_item>();

                    for (int x1 = 1; x1 < match.Count; x1 += 1)
                    {
                        var cur_item_value = Regex.Match(match[x1].Value, "pc\":\"(.*?)\"");
                        var cur_item_name = Regex.Match(match[x1].Value, "nm\":\"(.*?)\"");
                        var cur_item_id = Regex.Match(match[x1].Value, "id\":\"(.*?)\"");
                        if (cur_item_value.Success && cur_item_name.Success && cur_item_id.Success)
                        {
                            var buy_item = new buy_item();
                            buy_item.m_id = cur_item_id.Value.Remove(cur_item_id.Length - 1).Split('\"')[2];
                            buy_item.m_ordername = cur_item_name.Value.Remove(cur_item_name.Length - 1).Split('\"')[2];
                            buy_item.m_value = float.Parse(cur_item_value.Value.Remove(cur_item_value.Length - 1).Split('\"')[2]);

                            //skip under 3$
                            if (buy_item.m_value > 3.0f)
                                order_list.Add(buy_item);
                        }
                    }
                    if (order_list.Count() > 1)
                    {
                        //first get magarita

                        buy_item selected_item = new buy_item();
                        selected_item.m_value = 0.0f;

                        if(checkBox1.Checked && cur_salate < int.Parse(textBox4.Text))
                        {
                            cur_salate++;

                            for (int ii = 0; ii < order_list.Count(); ii += 1)
                            {
                                var var_order = order_list[ii];
                                if (var_order.m_ordername.Contains("alat"))
                                {
                                    if (var_order.m_value > selected_item.m_value)
                                    {
                                        selected_item = var_order;
                                    }
                                }
                            }

                            if (selected_item.m_value != 0.0f)
                            {
                                if ((selected_item.m_value * 3) < min_buy)
                                    selected_item.m_value = 0.0f;
                            }
                        }

                        if (selected_item.m_value == 0.0f)
                        {
                            for (int ii = 0; ii < order_list.Count(); ii += 1)
                            {
                                var var_order = order_list[ii];
                                if (var_order.m_ordername.Contains("argherita"))
                                {
                                    if (var_order.m_value > selected_item.m_value)
                                    {
                                        selected_item = var_order;

                                        if (selected_item.m_value > min_buy)
                                            break;
                                    }
                                }
                            }
                        }
  
                        if(selected_item.m_value == 0.0f)
                        {
                            List<buy_item> sort_by_value = order_list.OrderBy(o => o.m_value).ToList();                   
                            foreach (var ixm in sort_by_value)
                            {
                                if (ixm.m_value > min_buy)
                                {
                                    selected_item = ixm;
                                    break;
                                }
                            }

                            if (selected_item.m_value == 0.0f)
                                selected_item = sort_by_value[sort_by_value.Count - 1];
                        }

                        float cur_buy_value = 0.0f;
                        string order_format = "";

                        while (cur_buy_value < min_buy)
                        {
                            cur_buy_value += selected_item.m_value;
                            order_format += selected_item.m_id + ";";
                        }

                        string time__ = "";
                        if(checkBox2.Checked)
                        {
                            if(timesx2 != 0)
                            {
                                double min_add = (double)(double.Parse(textBox12.Text) * (double)timesx2) ;
                                DateTime duration = DateTime.Parse(textBox10.Text);
                                duration = duration.AddMinutes(min_add);
                                time__ = duration.ToString("HH:mm");
                            }
                            timesx++;
                            if (timesx == int.Parse(textBox5.Text))
                            {
                                timesx2++;
                                timesx = 0;
                            }
                        }

                        Back:

                        string fake_phone = "01" + gen_random(510, 999).ToString() + "-" + gen_random(111111, 999999).ToString();
                        string fake_mail = textBox1.Text.ToLower().Replace(" ", string.Empty) + gen_random(1, 1000).ToString() + "@gmail.com";
                        string fake_id = random_id(4) + "-" + random_id(4) + "_" + random_id(20);


                        if (textBox11.Text.Count() != 0 && email_map[textBox11.Text.GetHashCode()] > 9)
                        {
                            email_map[textBox11.Text.GetHashCode()] += 1;
                            fake_mail = textBox11.Text;
                        }

                        var ret = order_food(time__, index, textBox1.Text, textBox2.Text, textBox3.Text, textBox9.Text, fake_phone, fake_mail, restaurant_id, order_format,
                            countrys[index], countrid[index], textBox7.Text, textBox8.Text, textBox6.Text, textBox14.Text, fake_id);

                        if (ret == "{\"ok\":{\"on\":\"\",\"co\":\"\"}}")
                        {
                            MessageBox.Show("change IP/Data and continue");
                            goto Back;
                        }


                        var matched_trackingid = Regex.Match(ret, "trackingid=(.*?)\"");

                        if (matched_trackingid.Success)
                        {
                            try
                            {
                                string tracking_link = countrsite[index] + matched_trackingid.Value.Remove(matched_trackingid.Value.Length - 1).Split('=')[1];
                                listView1.Invoke(new MethodInvoker(delegate
                                {
                                    listView1.Items.Add(tracking_link);
                                }));
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }

             listView1.Invoke(new MethodInvoker(delegate
            {
                listView1.Items.Add("fertig mit allen");
            }));
        }
        private void button1_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();

            var idx = comboBox1.SelectedIndex;
            Task.Run(() => bestell_bot_smart(idx));
        }
        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            CopySelectedRowsToClipboard(listView1, e);
        }
    }
}
